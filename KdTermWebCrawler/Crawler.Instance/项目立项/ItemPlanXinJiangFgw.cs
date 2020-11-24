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
    public class ItemPlanXinJiangFgw : WebSiteCrawller
    {
        public ItemPlanXinJiangFgw()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "新疆维吾尔自治区发展和改革委员会项目信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取新疆维吾尔自治区发展和改革委员会项目信息";
            this.SiteUrl = "http://www.xjdrc.gov.cn:808/CivilProject/ConstructionProjectList.aspx";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return null; }

            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "lblPageCount")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString();
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
                    "__VIEWSTATEENCRYPTED",
                    "__EVENTVALIDATION",
                    "ddlArea",
                    "ddlType",
                    "ddlYear",
                    "txtNumber",
                    "txtProjectName",
                    "ddlPager"
                    }, new string[] {
                     "ddlPager",
                    "",
                    "",
                    viewState,
                    "",
                    eventValidation,
                    " ",
                    " ",
                    " ",
                    "",
                    "",
                    i.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "gvMajorProjects")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        ItemCode = ApprovalCode = tr.Columns[0].ToNodePlainString();
                        ItemName = aTag.GetAttribute("title");
                        PlanDate = tr.Columns[5].ToPlainTextString().GetDateRegex();
                        ApprovalUnit = tr.Columns[2].ToNodePlainString();
                        MsgUnit = tr.Columns[3].ToNodePlainString();
                        TotalInvest = tr.Columns[4].ToNodePlainString();
                        InfoUrl = "http://www.xjdrc.gov.cn:808" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "tdContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            parser = new Parser(new Lexer(CtxHtml));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag tag = tableNode[0] as TableTag;
                                for (int r = 0; r < tag.RowCount; r++)
                                {
                                    for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = tag.Rows[r].Columns[c].ToNodePlainString();
                                        if ((c + 1) % 2 == 0)
                                            ItemCtx += temp.GetReplace(":,：") + "\r\n";
                                        else
                                            ItemCtx += temp.GetReplace(":,：") + "：";
                                    }
                                }
                            }
                            else
                                ItemCtx = CtxHtml.ToCtxString();
                            ItemContent = ItemCtx.GetRegex("建设内容", true, 500);
                            ApprovalDate = ItemCtx.GetRegex("审批时间,审批日期,备案时间,备案日期,核准时间,核准日期");
                            ItemAddress = ItemCtx.GetRegex("建设地点");
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList title = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "tdParentTitle")));
                            if (title != null && title.Count > 0)
                                PlanType = title[0].ToNodePlainString();
                            MsgType = "新疆维吾尔自治区发展和改革委员会";

                            ItemPlan info = ToolDb.GenItemPlan("新疆维吾尔自治区", "新疆维吾尔自治区及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
