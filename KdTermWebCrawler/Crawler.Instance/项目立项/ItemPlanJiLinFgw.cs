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
    public class ItemPlanJiLinFgw : WebSiteCrawller
    {
        public ItemPlanJiLinFgw()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "吉林省省发展和改革委员会项目立项";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取吉林省省发展和改革委员会项目立项";
            this.SiteUrl = "http://222.168.7.143:8888/er/AttachManage/ProjectPublic/web_projectlistS.aspx?type=0";
            this.MaxCount = 400;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ItemPlan>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "GridView1_ctl21_labCountInfo")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("/", "每");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { 
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__LASTFOCUS",
                    "__VIEWSTATE",
                    "__VIEWSTATEGENERATOR",
                    "__EVENTVALIDATION"
                    }, new string[] { 
                    "GridView1$ctl21$lbNext",
                    "","",
                    viewState,
                    "44ED84FE",
                    eventValidation
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty, Area = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();
                        Parser divPerser = new Parser(new Lexer(tr.Columns[0].ToHtml()));
                        NodeList divNode = divPerser.ExtractAllNodesThatMatch(new TagNameFilter("div"));
                        if (divNode != null && divNode.Count > 0)
                        {
                            ItemName = (divNode[0] as Div).GetAttribute("title");
                        }
                        else
                            ItemName = aTag.LinkText;
                        Area = tr.Columns[1].ToNodePlainString();
                        PlanDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://222.168.7.143:8888/er/AttachManage/ProjectPublic/" + aTag.Link.Replace("../", "");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "682")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode[0].ToHtml();
                            ItemCtx = CtxHtml.ToCtxString();
                            string ctx = string.Empty;
                            for (int q = 1; q < dtlNode.Count; q++)
                            {
                                TableTag tag = dtlNode[q] as TableTag;
                                for (int r = 0; r < tag.RowCount; r++)
                                {
                                    for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = tag.Rows[r].Columns[c].ToNodePlainString();
                                        if (r == 0 && c == 0)
                                            continue;
                                        if (r == 0)
                                        {
                                            if ((c + 1) % 2 == 0)
                                                ctx += temp.GetReplace(":,：") + "：";
                                            else
                                                ctx += temp.GetReplace(":,：") + "\r\n";
                                        }
                                        else
                                        {
                                            if ((c + 1) % 2 == 0)
                                                ctx += temp.GetReplace(":,：") + "\r\n";
                                            else
                                                ctx += temp.GetReplace(":,：") + "：";
                                        }
                                    }
                                }
                            }
                            ItemCode = ctx.GetCodeRegex();
                            ItemContent = ctx.GetRegex("建设内容", true, 500);
                            ApprovalCode = ctx.GetRegex("文号");
                            ApprovalDate = ctx.GetRegex("批复时间");
                            ItemAddress = ctx.GetAddressRegex();
                            PlanType = "项目公开";
                            MsgType = "吉林省发展和改革委员会";

                            ItemPlan info = ToolDb.GenItemPlan("吉林省", "吉林省及地市", Area, ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
