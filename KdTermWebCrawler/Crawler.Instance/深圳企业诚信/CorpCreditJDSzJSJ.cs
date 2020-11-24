using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace Crawler.Instance
{
    public class CorpCreditJDSzJSJ : WebSiteCrawller
    {
        public CorpCreditJDSzJSJ()
            : base(true)
        {
            this.IsCrawlAll = true;
            this.Group = "企业评价信息";
            this.PlanTime = "3-02 5:00,6-02 5:00,9-02 5:00,12-02 5:00";
            this.Title = "深圳市建设局企业评价信息阶段得分列表";
            this.Description = "自动抓取深圳市建设局企业评价信息阶段得分列表";
            this.ExistCompareFields = "CalcuBeginDate,CalcuEndDate";
            this.MaxCount = 5000;
            this.SiteUrl = "http://61.144.226.2:8008/JDScore.aspx?clearPaging=true&guid=438945";
        }


        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string html = string.Empty;
            string html1 = string.Empty;
            IList list = new ArrayList();
            try
            {
                html = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            string cookiestr = string.Empty;
            string viewState = ToolWeb.GetAspNetViewState(parser);
            string eventValidation = ToolWeb.GetAspNetEventValidation(parser);
            string beginDate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
            for (int i = 1; i <= 12; i++)
            {
                string ddlIndex = string.Empty;
                ddlIndex = i.ToString();
                if (i == 12)
                {
                    ddlIndex = "999999999";
                }

                NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] { "ScriptManager1", "__EVENTTARGET", "__EVENTARGUMENT", "__LASTFOCUS", "__VIEWSTATE", "drpRpt", "DropDownList2", "txtCorpName", "DropDownList1", "GridViewPaging1$txtGridViewPagingForwardTo", "__EVENTVALIDATION" }, new string[] { "UpdatePanel1|DropDownList2", "DropDownList2", string.Empty, string.Empty, viewState, "419425", ddlIndex, string.Empty, "-1", "1", eventValidation });
                try
                {
                    html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    html1 = html;
                    //处理第一页
                    DealHtml(list, html, crawlAll, ddlIndex);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                if (!crawlAll && list.Count >= this.MaxCount) return list;
                //取得页码
                int pageInt = 1;
                parser = new Parser(new Lexer(html));
                NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "GridViewPaging1_lblGridViewPagingDesc")));
                if (tdNodes != null)
                {
                    string pageTemp = tdNodes[0].ToPlainTextString().Trim();
                    try
                    {
                        pageTemp = pageTemp.Substring(pageTemp.IndexOf("页，共")).Replace("页，共", string.Empty).Replace("页", string.Empty);
                        pageInt = int.Parse(pageTemp);
                    }
                    catch (Exception ex) { Logger.Error(ex); }
                }
                parser.Reset();

                //处理后续页
                if (pageInt > 1)
                {
                    for (int j = 2; j <= pageInt; j++)
                    {
                        string viewStatePage = ToolWeb.GetAspNetViewState(html1);
                        string eventValidationPage = ToolWeb.GetAspNetEventValidation(html1);
                        string cookPage = string.Empty;
                        NameValueCollection nvcPage = null;
                        if (j == 14 && ddlIndex.Equals("2"))
                        {
                            j++;
                            nvcPage = ToolWeb.GetNameValueCollection(new string[] { "ScriptManager1", "__EVENTTARGET", "__EVENTARGUMENT", "__LASTFOCUS", "__VIEWSTATE", "drpRpt", "DropDownList2", "txtCorpName", "DropDownList1", "GridViewPaging1$txtGridViewPagingForwardTo", "__EVENTVALIDATION", "GridViewPaging1$btnForwardToPage" }, new string[] { "UpdatePanel1|GridViewPaging1$btnForwardToPage", string.Empty, string.Empty, string.Empty, viewStatePage, "419425", ddlIndex, string.Empty, "-1", j.ToString(), eventValidationPage, "Go" });
                        }
                        else
                        {
                            nvcPage = ToolWeb.GetNameValueCollection(new string[] { "ScriptManager1", "__EVENTTARGET", "__EVENTARGUMENT", "__LASTFOCUS", "__VIEWSTATE", "drpRpt", "DropDownList2", "txtCorpName", "DropDownList1", "GridViewPaging1$txtGridViewPagingForwardTo", "__EVENTVALIDATION", "GridViewPaging1$btnNext.x", "GridViewPaging1$btnNext.y" }, new string[] { "UpdatePanel1|GridViewPaging1$btnNext", string.Empty, string.Empty, string.Empty, viewStatePage, "419425", ddlIndex, string.Empty, "-1", (j - 1).ToString(), eventValidationPage, "6", "10" });
                        }

                        try
                        {
                            html = ToolWeb.GetHtmlByUrl(SiteUrl, nvcPage, Encoding.UTF8, ref cookiestr);
                            //处理后续页
                            DealHtml(list, html, crawlAll, ddlIndex);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            continue;
                        }
                    }
                }
                if (!crawlAll && list.Count >= this.MaxCount) return list;
            }
            return list;
        }

        public void DealHtml(IList list, string html, bool crawlAll, string ddlIndex)
        {

            Parser parserCtxTime = new Parser(new Lexer(html));
            NodeList ctxNodeTime = parserCtxTime.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("id", "drpRpt")));
            string dateTime = ctxNodeTime.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("option"), new HasAttributeFilter("value", "419425")), true).AsString().Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Trim();
            parserCtxTime.Reset();

            Parser parserCtx = new Parser(new Lexer(html));
            NodeList ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("id", "DropDownList2")));
            string classification = ctxNode.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("option"), new HasAttributeFilter("value", ddlIndex)), true).AsString().Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Trim();
            parserCtx.Reset();

            Parser parserDtl = new Parser(new Lexer(html));
            NodeList aNodes = parserDtl.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
            if (aNodes != null && aNodes.Count == 1 && aNodes[0] is TableTag)
            {
                TableTag table = (TableTag)aNodes[0];
                for (int i = 1; i < table.Rows.Length; i++)
                {
                    if (table.Rows[i].Columns.Length == 6)
                    {
                        string corpName = string.Empty, corpType = string.Empty, corpClassification = string.Empty, corpRank = string.Empty;
                        int corpAllRanking = 0, classificationRanking = 0;
                        decimal realScore = 0;

                        corpName = table.Rows[i].Columns[1].ToPlainTextString().Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Trim();
                        corpRank = table.Rows[i].Columns[2].ToPlainTextString().Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Trim();
                        try
                        {
                            corpAllRanking = int.Parse(table.Rows[i].Columns[3].ToPlainTextString().Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Trim());
                            classificationRanking = int.Parse(table.Rows[i].Columns[4].ToPlainTextString().Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Trim());
                            realScore = decimal.Parse(table.Rows[i].Columns[5].ToPlainTextString().Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Trim());
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex + "企业名称：" + corpName);
                            continue;
                        }
                        corpClassification = classification;
                        DateTime satrtTime = new DateTime();
                        DateTime endTime = new DateTime();
                        try
                        {
                            satrtTime = DateTime.Parse(dateTime.Substring(0, 10));
                            endTime = DateTime.Parse(dateTime.Substring(dateTime.IndexOf("----") + 4, 10));
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex + "企业名称：" + corpName);
                            continue;
                        }
                        CorpCreditjd info = ToolDb.GenCorpCreditJD(corpName, corpType, corpRank, corpClassification.ToString(), corpAllRanking.ToString(), classificationRanking.ToString(), satrtTime.ToString().ToString(), endTime.ToString(), realScore.ToString(), "广东省", "深圳市", "深圳市住房和建设局", SiteUrl, "","","","");
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return;
                    }
                }
            }
            parserDtl.Reset();
        }
    }
}
