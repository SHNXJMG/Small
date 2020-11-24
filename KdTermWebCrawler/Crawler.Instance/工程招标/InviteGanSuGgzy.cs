using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteGanSuGgzy : WebSiteCrawller
    {
        public InviteGanSuGgzy()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "甘肃省公共资源交易中心招标信息(房建市政工程)";
            this.Description = "自动抓取甘肃省公共资源交易中心招标信息(房建市政工程)";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 200;
            this.SiteUrl = "http://www.gsggzyjy.cn/InfoPage/AnnoGoodsList.aspx?SiteItem=71&InfoType=3";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "navigation")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode[0].ToNodePlainString().GetRegexBegEnd("总共", "页").GetReplace("【,】,[,]");
                try
                {
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "slidingList")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        INode node = listNode[j];

                        ATag aTag = node.GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = node.GetSpan().StringText;
                        if (!string.IsNullOrEmpty(beginDate))
                            beginDate = beginDate.Substring(0, 4) + "-" + beginDate.Substring(4, 2) + "-" + beginDate.Substring(6, 2);
                        InfoUrl = "http://www.gsggzyjy.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ContentPlaceHolder1_AnnoGoodsHtml")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            msgType = "甘肃省公共资源交易中心";
                            specType = "政府采购";
                            inviteType = "房建市政工程";
                            InviteInfo info = ToolDb.GenInviteInfo("甘肃省", "甘肃省及地市", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("Iframe"), new HasAttributeFilter("id", "Iframe")));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    IFrameTag itag = aNode[k] as IFrameTag;
                                    string link = itag.GetAttribute("src");
                                    if (!string.IsNullOrEmpty(link))
                                    {
                                        BaseAttach attach = ToolDb.GenBaseAttach(prjName+".pdf", info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList atagNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (atagNode != null && atagNode.Count > 0)
                            {
                                for (int a = 0; a < atagNode.Count; a++)
                                {
                                    ATag fileTag = atagNode[a] as ATag;
                                    if (fileTag.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (fileTag.Link.Contains("http"))
                                            link = fileTag.Link;
                                        else
                                            link = "http://www.gsggzyjy.cn/" + fileTag.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, link);
                                        if (!base.AttachList.Exists(x => x.AttachServerPath == link))
                                            base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
