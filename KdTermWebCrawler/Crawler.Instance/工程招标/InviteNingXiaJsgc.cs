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
    public class InviteNingXiaJsgc : WebSiteCrawller
    {
        public InviteNingXiaJsgc()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "宁夏建设工程招标投标信息网招标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取宁夏建设工程招标投标信息网招标信息";
            this.SiteUrl = "http://www.nxzb.com.cn/SiteAcl.srv?id=1003891287";
            this.MaxCount = 400;
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            try
            {
                string temp = html.ToCtxString().GetRegexBegEnd("第1/", "页");
                pageInt = int.Parse(temp);
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "pageno",
                    "mode",
                    "linkname"
                    }, new string[]{
                   i.ToString(),
                   "query",
                   "currinfo"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "list")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[listNode.Count-1] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();
                        if (aTag == null) continue;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        prjName = aTag.GetAttribute("title");
                        InfoUrl = "http://www.nxzb.com.cn/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl.ToLower().GetReplace("th", "td")));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "tabcon")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            TableTag dtlTable = dtlNode[0] as TableTag;
                            HtmlTxt = dtlTable.ToHtml();
                            for (int r = 0; r < dtlTable.RowCount; r++)
                            {
                                for (int c = 0; c < dtlTable.Rows[r].ColumnCount; c++)
                                {
                                    if ((c + 1) % 2 == 0)
                                        inviteCtx += dtlTable.Rows[r].Columns[c].ToNodePlainString() + "\r\n";
                                    else
                                        inviteCtx += dtlTable.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：") + "：";
                                }
                            }
                            beginDate = inviteCtx.GetRegex("发布日期,发布时间").GetDateRegex(); 
                            buildUnit = inviteCtx.GetBuildRegex();
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList iframeNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("iframe"), new HasAttributeFilter("id", "icontent")));
                            if (iframeNode != null && iframeNode.Count > 0)
                            {
                                InfoUrl = (iframeNode[0] as IFrameTag).FrameLocation;
                                try
                                {
                                    htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                                }
                                catch { }
                                parser = new Parser(new Lexer(htmldtl));
                                NodeList htmlDtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                                if (htmlDtlNode != null && htmlDtlNode.Count > 0)
                                {
                                    HtmlTxt = htmlDtlNode.AsHtml();
                                    inviteCtx = HtmlTxt.ToLower().GetReplace("<br/>,<br>", "\r\n").ToCtxString();
                                    prjAddress = inviteCtx.GetAddressRegex();
                                    code = inviteCtx.GetCodeRegex().GetCodeDel();
                                }
                            } 
                            msgType = "宁夏建设工程招标投标管理中心";
                            specType = inviteType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("宁夏回族自治区", "宁夏回族自治区及地市", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
