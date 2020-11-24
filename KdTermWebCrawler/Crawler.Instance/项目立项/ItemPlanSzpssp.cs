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
    public class ItemPlanSzpssp : WebSiteCrawller
    {
        public ItemPlanSzpssp()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "深圳市坪山区工程领域项目审批信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市坪山区工程领域项目审批信息";
            this.SiteUrl = "http://other.sznews.com/pinshanproject/ProjectList.aspx?nodeid=1";
            this.MaxCount = 1200;
            this.ExistCompareFields = "InfoUrl";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "gvlist")), true), new TagNameFilter("table")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    TableTag table = pageNode[0] as TableTag;
                    string temp = table.Rows[0].Columns[table.Rows[0].ColumnCount - 1].ToNodePlainString();
                    pageInt = int.Parse(temp);
                }
                catch
                { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "__VIEWSTATEGENERATOR",
                    "__EVENTVALIDATION",
                    "ddlDepartment",
                    "txtstartDate",
                    "txtendDate",
                    "keyword",
                    },
                        new string[]{
                        "gvlist",
                        "Page$"+i,
                        viewState,
                        "D5D8AE3D",
                        eventValidation,
                        "发展财政局",
                        string.Empty,
                        string.Empty,
                        string.Empty
                        });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "gvlist")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;
                        TableRow tr = table.Rows[j];
                        ItemCode = tr.Columns[0].ToNodePlainString();
                        ItemName = tr.Columns[1].ToNodePlainString();
                        PlanDate = tr.Columns[2].ToNodePlainString();
                        InfoUrl = "http://other.sznews.com/pinshanproject/" + tr.Columns[1].GetATagHref();
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "stylecontent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            ItemCtx = CtxHtml.ToCtxString();
                            TotalInvest = ItemCtx.GetRegexBegEnd("总投资", "万元").GetChina();
                            IssuedPlan = ItemCtx.GetRegexBegEnd("本次下达", "万元").GetChina();
                            PlanType = "项目审批信息";
                            MsgType = "深圳市坪山区发改局";
                            ItemPlan info = ToolDb.GenItemPlan("广东省", "深圳市区", "坪山区", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
