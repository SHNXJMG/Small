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
    public class InviteAnHuiJsgc : WebSiteCrawller
    {
        public InviteAnHuiJsgc()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "安徽省建设工程招标投投标招标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取安徽省建设工程招标投投标招标信息";
            this.SiteUrl = "http://www.act.org.cn/news.asp?pid=169";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "Page")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("页数：", "页").ToNodeString().GetReplace("|");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "sublist_list_ul")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        prjName = aTag.LinkText;
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.act.org.cn/Article.Asp?Pid=252&id=37845";//"http://www.act.org.cn/" + aTag.Link.GetReplace("amp;");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl.ToLower().GetReplace("<div", "<div ").GetReplace("<span", "<span ").GetReplace("<p", "<p ").GetReplace("<table", "<table ").GetReplace("<tr", "<tr ").GetReplace("<td", "<td ").GetReplace("<th", "<th ")));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "subcont_cont")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            
                            inviteCtx = HtmlTxt.ToLower().GetReplace("<br/>,<br>,</p>", "\r\n").GetReplace("?").ToCtxString();
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList htmlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "subcont_cont")));
                            if (htmlNode != null && htmlNode.Count > 0)
                            {
                                string strHtml = htmlNode.AsHtml();
                                if (strHtml.Length < 600)
                                    HtmlTxt = inviteCtx;
                            }
                            buildUnit = inviteCtx.GetReplace(" ").GetBuildRegex();
                            code = inviteCtx.GetReplace(" ").GetCodeRegex().GetCodeDel();
                            prjAddress = inviteCtx.GetReplace(" ").GetAddressRegex();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                string ctx = string.Empty;
                                TableTag tag = tableNode[0] as TableTag;
                                for (int r = 0; r < tag.RowCount; r++)
                                {
                                    for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = tag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                        if ((c + 1) % 2 == 0)
                                            ctx += temp + "\r\n";
                                        else
                                            ctx += temp + "：";
                                    }
                                }
                                if (string.IsNullOrEmpty(code))
                                    code = ctx.GetCodeRegex();
                                if (string.IsNullOrEmpty(buildUnit))
                                    buildUnit = ctx.GetBuildRegex();
                                if (string.IsNullOrEmpty(prjAddress))
                                    prjAddress = ctx.GetAddressRegex();
                            }
                            msgType = "安徽省建设工程招标投标办公室";
                            specType = inviteType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("安徽省", "安徽省及地市", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
