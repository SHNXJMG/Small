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
    public class InviteJmzfcg : WebSiteCrawller
    {
        public InviteJmzfcg()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "江门市政府采购招标信息";
            this.PlanTime = "9:11,11:11,13:11,15:11,17:11";
            this.Description = "自动抓取江门市政府采购招标信息";
            this.SiteUrl = "http://jiangmen.gdgpo.com/queryMoreInfoList/channelCode/0005.html";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("form"), new HasAttributeFilter("name", "qPageForm")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    NodeList aNode = new Parser(new Lexer(pageNode.ToHtml())).ExtractAllNodesThatMatch(new TagNameFilter("a"));
                    if (aNode != null && aNode.Count > 0)
                    {
                        string temp = aNode[aNode.Count - 2].GetATagHref().Replace("turnOverPage", "").Replace("(", "").Replace(")", "").Replace(";", "");
                        pageInt = int.Parse(temp);
                    }
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{ "channelCode","pageIndex","pageSize","pointPageIndexId"
                    }, new string[]{
                    "0005",i.ToString(),"15","1"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://jiangmen.gdgpo.com/queryMoreInfoList.do", nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "m_m_c_list")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = listNode[j].GetATag(1);
                        prjName = aTag.GetAttribute("title");
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://jiangmen.gdgpo.com" + aTag.Link;

                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "zw_c_c_cont")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();//.Replace("<br", "\r\n<br");
                            inviteCtx = HtmlTxt.ToCtxString();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            code = code.GetChina();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            inviteType = prjName.GetInviteBidType();
                            msgType = "江门市政府采购";
                            specType = "政府采购";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "江门市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList fileNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (fileNode != null && fileNode.Count > 0)
                            {
                                for (int k = 0; k < fileNode.Count; k++)
                                {
                                    ATag fileAtag = fileNode[k].GetATag();
                                    if (fileAtag.IsAtagAttach())
                                    {
                                        string fileName = fileAtag.LinkText.ToNodeString().Replace(" ", "");
                                        string fileLink = fileAtag.Link;
                                        if (!fileLink.ToLower().Contains("http"))
                                            fileLink = "http://jiangmen.gdgpo.gov.cn" + fileAtag.Link;
                                        base.AttachList.Add(ToolDb.GenBaseAttach(fileName, info.Id, fileLink));
                                    }
                                }
                            }
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;

                        }
                    }
                }
            }
            return list;
        }
    }
}
