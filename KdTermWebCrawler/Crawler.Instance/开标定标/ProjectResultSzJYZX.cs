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
    public class ProjectResultSzJYZX : WebSiteCrawller
    {
        public ProjectResultSzJYZX()
            : base()
        {
            this.Group = "开标定标";
            this.Title = "深圳市交易中心定标结果公示";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市交易中心定标结果公示";
            this.SiteUrl = "http://www.szjsjy.com.cn/BusinessInfo/DBJGGSList.aspx";
            this.MaxCount = 500;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ProjectResult>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1, sqlCount = 0;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "ListTable")));
            if (pageNode != null && pageNode.Count > 0)
            {
                TableTag table = pageNode[0] as TableTag;
                try
                {
                    string temp = table.Rows[table.RowCount - 1].ToNodePlainString().GetRegexBegEnd("，共", "页");
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
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "__VIEWSTATEENCRYPTED",
                    "__EVENTVALIDATION",
                    "ctl00$Header$drpSearchType",
                    "ctl00$Header$txtQymc",
                    "ctl00$Content$hdnOperate",
                    "ctl00$hdnPageCount"
                    }, new string[]{
                    "ctl00$Content$GridView_定标结果工程",
                    "Page$"+i,
                    viewState,
                    "",
                    eventValidation,
                    "ProjCode",
                    "","",
                    pageInt.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "ListTable")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string Code = string.Empty, prjName = string.Empty, BuildUnit = string.Empty, FinalistsWay = string.Empty, RevStaMethod = string.Empty, SetStaMethod = string.Empty, VoteMethod = string.Empty, RevStaDate = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty, Ctx = string.Empty, Html = string.Empty,beginDate=string.Empty;

                        TableRow tr = table.Rows[j];

                        Code = tr.Columns[1].ToNodePlainString();
                        prjName = tr.Columns[2].ToNodePlainString();
                        beginDate = DateTime.Now.ToString();
                        InfoUrl = "http://www.szjsjy.com.cn/BusinessInfo/" + tr.Columns[3].GetATagHref();
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ContentContainer")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            Html = dtlNode.AsHtml();
                            Ctx = Html.ToCtxString();
                            parser = new Parser(new Lexer(Html));
                            NodeList ctxNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                            if (ctxNode != null && ctxNode.Count > 0)
                            {
                                string dtlCtx = string.Empty;
                                TableTag ctxTable = ctxNode[0] as TableTag;
                                for (int d = 0; d < ctxTable.RowCount; d++)
                                {
                                    for (int k = 0; k < ctxTable.Rows[d].ColumnCount; k++)
                                    {
                                        if ((k + 1) % 2 == 0)
                                            dtlCtx += ctxTable.Rows[d].Columns[k].ToNodePlainString() + "\r\n";
                                        else
                                            dtlCtx += ctxTable.Rows[d].Columns[k].ToNodePlainString() + "：";
                                    }
                                }
                                BuildUnit = dtlCtx.GetRegex("建设单位");
                                FinalistsWay = dtlCtx.GetRegex("入围方式");
                                RevStaMethod = dtlCtx.GetRegex("评标方法");
                                SetStaMethod = dtlCtx.GetRegex("定标方法");
                                VoteMethod = dtlCtx.GetRegex("票决方法");
                                RevStaDate = dtlCtx.GetRegex("定标时间");
                            }
                        }
                        MsgType = "深圳市建设工程交易中心";

                        sqlCount++;
                        if (!crawlAll && sqlCount >= this.MaxCount) return list;

                        ProjectResult info = ToolDb.GetProjectResult("广东省", "深圳市工程", "", Code, prjName, BuildUnit, FinalistsWay, RevStaMethod, SetStaMethod, VoteMethod, RevStaDate, InfoUrl, MsgType, Ctx, Html, beginDate);
                        
                        if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                        {
                            parser = new Parser(new Lexer(htmldtl.Replace("th","td")));
                            NodeList prjDtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "ListTable")));
                            if (prjDtlNode != null && prjDtlNode.Count > 0)
                            {
                                TableTag prjTable = prjDtlNode[0] as TableTag;
                                string colName1 = prjTable.Rows[1].Columns[2].ToNodePlainString();
                                string colName2 = prjTable.Rows[1].Columns[3].ToNodePlainString();
                                for (int c = 2; c < prjTable.RowCount; c++)
                                {
                                    TableRow dr = prjTable.Rows[c]; 
                                     
                                    string UnitName = string.Empty, BidDate = string.Empty, IsBid = string.Empty, Ranking = string.Empty, WinNumber = string.Empty, TicketNumber = string.Empty;
                                     
                                    UnitName = dr.Columns[1].ToNodePlainString();
                                    if (colName1.Contains("投标时间") || colName1.Contains("投标日期"))
                                        BidDate = dr.Columns[2].ToPlainTextString();
                                    else if (colName1.Contains("得票数"))
                                        TicketNumber = dr.Columns[2].ToNodePlainString();
                                    else if (colName1.Contains("取胜次数"))
                                        WinNumber = dr.Columns[2].ToNodePlainString();
                                    if (colName2.Contains("排名"))
                                        Ranking = dr.Columns[3].ToNodePlainString();
                                    else if (colName2.Contains("中标候选人"))
                                        IsBid = dr.Columns[3].ToNodePlainString() == "" ? "0" : "1";

                                    ProjectResultDtl infoDtl = ToolDb.GetProjectResultDtl(info.Id, UnitName, BidDate, IsBid, Ranking, WinNumber, TicketNumber);
                                    ToolDb.SaveEntity(infoDtl, "SourceId,UnitName", this.ExistsUpdate);
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
