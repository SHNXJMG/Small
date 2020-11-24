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
using System.Web.UI.HtmlControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class NanFangDW : WebSiteCrawller
    {
        public NanFangDW()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "中国南方电网";
            this.Description = "自动抓取中国南方电网";
            this.PlanTime = "8:50,9:40,10:30,11:30,13:40,15:00,16:30";
            this.SiteUrl = "http://www.bidding.csg.cn/zbgg/index.jhtml";
            this.MaxCount = 40;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList noList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "Top10 TxtCenter")));
            if (noList != null && noList.Count > 0)
            {
                string temp = noList.AsString().GetRegexBegEnd("/", "页");
                try
                {
                    pageInt = Convert.ToInt32(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.bidding.csg.cn/zbgg/index_" + i.ToString() + ".jhtml", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "W750 Right")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 1; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                   prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                   specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                   remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                   CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.LinkText;
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.bidding.csg.cn" + aTag.Link;
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = ToolHtml.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "Center W1000")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList nameNode = parser.ExtractAllNodesThatMatch(new AndFilter(new
                                 TagNameFilter("h1"),new HasAttributeFilter("class","TxtCenter Padding10")));
                            if (nameNode != null && nameNode.Count > 0)
                            {
                                prjName = nameNode[0].ToNodePlainString();
                            }
                            inviteCtx = HtmlTxt.ToCtxString();

                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            prjAddress = ToolHtml.GetRegexString(inviteCtx, ToolHtml.AddressRegex);
                            buildUnit = ToolHtml.GetRegexString(inviteCtx, ToolHtml.BuildRegex);
                            code = ToolHtml.GetRegexString(inviteCtx, ToolHtml.CodeRegex);
                            prjAddress = ToolHtml.GetSubString(prjAddress, 150);
                            buildUnit = ToolHtml.GetSubString(buildUnit, 150);
                            code = ToolHtml.GetSubString(code, 50);
                            if (string.IsNullOrEmpty(code))
                            {
                                code = "见招标信息";
                            }
                            if (string.IsNullOrEmpty(prjAddress))
                            {
                                prjAddress = "见招标信息";
                            }
                            specType = "其他";
                            msgType = "中国南方电网有限责任公司招标服务中心";
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "中国南方电网有限责任公司招标服务中心";
                            }
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "电网专项工程", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList nodeAtag = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (nodeAtag != null && nodeAtag.Count > 0)
                            {
                                for (int c = 0; c < nodeAtag.Count; c++)
                                {
                                    ATag a = nodeAtag[c] as ATag;
                                    if (a.Link.IsAtagAttach())
                                    {
                                        string alink = "http://www.bidding.csg.cn/" + a.Link;
                                        try
                                        {
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, alink);
                                            base.AttachList.Add(attach);
                                        }
                                        catch
                                        {
                                        }
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
