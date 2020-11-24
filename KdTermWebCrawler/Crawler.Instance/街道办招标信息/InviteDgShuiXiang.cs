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
    public class InviteDgShuiXiang : WebSiteCrawller
    {
        public InviteDgShuiXiang()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省东莞水乡特色发展经济区管理委员会信息招标公告";
            this.Description = "自动抓取东莞水乡特色发展经济区管理委员会信息招标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://dgsx.dg.gov.cn/publicfiles//business/htmlfiles/dgsx/s41001/index.htm";
            this.MaxCount = 150;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch { return list; }
            int startIndex = html.IndexOf("<xml");
            int endIndex = html.IndexOf("</xml>");
            string xmlstr = html.Substring(startIndex, endIndex - startIndex).ToLower().GetReplace("infourl", "span").GetReplace("info", "div").GetReplace("publishedtime", "p");
            Parser parser = new Parser(new Lexer(xmlstr));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("div"));
            if (pageNode != null && pageNode.Count > 0)
            {
                for (int i = 0; i < pageNode.Count; i++)
                {
                    string code = string.Empty, buildUnit = string.Empty,
                   prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                   specType = string.Empty, endDate = string.Empty, beginDate = string.Empty,
                   remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                   CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                    parser = new Parser(new Lexer(pageNode[i].ToHtml()));
                    NodeList dateNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("p"));
                    beginDate = dateNode[0].ToPlainTextString().GetDateRegex();
                    parser.Reset();
                    NodeList urlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("span"));
                    InfoUrl = "http://dgsx.dg.gov.cn/publicfiles//business/htmlfiles/" + urlNode[0].ToPlainTextString();
                    string htmldtl = string.Empty;
                    try
                    {
                        htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                    }
                    catch { continue; }
                    parser = new Parser(new Lexer(htmldtl));
                    NodeList titleNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("title"));
                    string prjName = titleNode[0].ToNodePlainString();
                    parser.Reset();
                    NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("valign", "top")));
                    if (dtlNode != null && dtlNode.Count > 0)
                    {
                        HtmlTxt = dtlNode[0].ToHtml();
                        inviteCtx = HtmlTxt.GetReplace("</p>", "\r\n").ToCtxString();

                        buildUnit = inviteCtx.GetBuildRegex();
                        prjAddress = inviteCtx.GetAddressRegex();
                        code = inviteCtx.GetCodeRegex().GetCodeDel();
                        if (buildUnit.Contains("地址"))
                            buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));

                        specType = "政府采购";
                        inviteType = prjName.GetInviteBidType();
                        msgType = "东莞水乡特色发展经济区管理委员会";

                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "东莞市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                        list.Add(info);
                        parser = new Parser(new Lexer(HtmlTxt));
                        NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        if (aNode != null && aNode.Count > 0)
                        {
                            for (int k = 0; k < aNode.Count; k++)
                            {
                                ATag a = aNode[k].GetATag();
                                if (a.IsAtagAttach())
                                {
                                    string link = string.Empty;
                                    if (a.Link.ToLower().Contains("http"))
                                        link = a.Link;
                                    else
                                        link = "http://dgsx.dg.gov.cn/" + a.Link;
                                    BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                    base.AttachList.Add(attach);
                                }
                            }
                        }
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }

            return list;
        }
    }
}
