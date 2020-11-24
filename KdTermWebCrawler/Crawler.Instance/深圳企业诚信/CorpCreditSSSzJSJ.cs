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
using System.Web.UI.WebControls;

namespace Crawler.Instance
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class CorpCreditSSSzJSJ : WebSiteCrawller
    {
        public CorpCreditSSSzJSJ()
            : base()
        {
            this.IsCrawlAll = true;
            this.Group = "企业评价信息";
            this.Title = "深圳市建设局企业评价信息实时得分列表";
            this.MaxCount = 5000;
            this.Description = "自动抓取深圳市建设局企业评价信息实时得分列表";
            this.ExistCompareFields = "CorpName,CalcuDate,CorpCategory,Ranking,CategoryRank,RealScore";
            this.PlanTime = "7:00";
            this.SiteUrl = "http://61.144.226.2:8008/CorpRealList.aspx";
        }
        private int count = 1;
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string html = string.Empty;
            string html1 = string.Empty;
            string cookiestr = string.Empty;
            IList list = new ArrayList();
            try
            {
                html = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return list;
            }
            Parser parser = new Parser(new Lexer(html));

            string viewState = ToolWeb.GetAspNetViewState(parser);
            string eventValidation = ToolWeb.GetAspNetEventValidation(parser);
            string beginDate = string.Empty;
            for (DateTime day = DateTime.Today; day >= DateTime.Today.AddDays(-2); day = day.AddDays(-1))
            {
                beginDate = day.ToString("yyyy-MM-dd");
                if (beginDate != DateTime.Today.ToString("yyyy-MM-dd"))
                {
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(
                        new string[] { "__EVENTTARGET", 
                            "__EVENTARGUMENT", 
                            "__LASTFOCUS",
                            "__VIEWSTATE",
                            "__VIEWSTATEENCRYPTED", 
                            "__EVENTVALIDATION", 
                            "txtCorpName", 
                            "DropDownList2", 
                    "txtBeginDate",
                    "GridViewPaging1$txtGridViewPagingForwardTo"},
                    new string[] { "DropDownList2",
                        string.Empty,
                        string.Empty,  
                        viewState, 
                        string.Empty, 
                        eventValidation, 
                          string.Empty, 
                        "-1", 
                        beginDate, 
                        "1"
                         });
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch (Exception ex) { Logger.Error(ex); }
                    parser = new Parser(new Lexer(html));
                    NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "GridViewPaging1_lblGridViewPagingDesc")));
                    if (tdNodes != null)
                    {
                        string pageTemp = tdNodes[0].ToPlainTextString().Trim();
                        try
                        {
                            pageTemp = pageTemp.GetRegexBegEnd("共","条");
                            int pInt = int.Parse(pageTemp);
                            int max = pInt - this.MaxCount;
                            //if (max > 10 || max < -10)
                            //{
                            //    break;
                            //}
                        }
                        catch (Exception ex) { Logger.Error(ex); }
                    }
                }
                for (int i = 1; i <= 14; i++)
                {
                    if (i == 7) continue; 
                    string ddlIndex = i.ToString();// i.ToString();
                    if (i == 14)
                    {
                        ddlIndex = "999999999";
                    }

                    viewState = ToolWeb.GetAspNetViewState(html);
                    eventValidation = ToolWeb.GetAspNetEventValidation(html);
                     
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(
                        new string[] { "__EVENTTARGET", 
                            "__EVENTARGUMENT", 
                            "__LASTFOCUS",
                            "__VIEWSTATE",
                            "__VIEWSTATEENCRYPTED", 
                            "__EVENTVALIDATION", 
                            "txtCorpName", 
                            "DropDownList2", 
                    "txtBeginDate",
                    "GridViewPaging1$txtGridViewPagingForwardTo"},
                    new string[] { "DropDownList2",
                        string.Empty,
                        string.Empty,  
                        viewState, 
                        string.Empty, 
                        eventValidation, 
                          string.Empty, 
                        ddlIndex, 
                        beginDate, 
                        "1"
                         });


                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch (Exception ex) { Logger.Error(ex); }
                    //处理第一页
                    DealHtml(list, html, crawlAll, ddlIndex, DateTime.Parse(beginDate));
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

                            string viewStatePage = ToolWeb.GetAspNetViewState(html);
                            string eventValidationPage = ToolWeb.GetAspNetEventValidation(html);

                            NameValueCollection nvcPage = ToolWeb.GetNameValueCollection(
                        new string[] { "__EVENTTARGET", 
                            "__EVENTARGUMENT", 
                            "__LASTFOCUS",
                            "__VIEWSTATE",
                            "__VIEWSTATEENCRYPTED", 
                            "__EVENTVALIDATION", 
                            "txtCorpName", 
                            "DropDownList2", 
                    "txtBeginDate",
                    "GridViewPaging1$txtGridViewPagingForwardTo",
                    "GridViewPaging1$btnForwardToPage"},
                    new string[] {
                        string.Empty,
                        string.Empty,
                        string.Empty,  
                        viewStatePage, 
                        string.Empty, 
                        eventValidationPage, 
                          string.Empty, 
                        ddlIndex, 
                        beginDate, 
                        j.ToString(),
                        "Go"});
                            try
                            {
                                html = ToolWeb.GetHtmlByUrl(this.SiteUrl, nvcPage, Encoding.UTF8);//ToolWeb.GetHtmlByUrlAndIsError(SiteUrl, nvcPage, Encoding.UTF8,ref isError); 
                                //处理后续页
                                DealHtml(list, html, crawlAll, ddlIndex, DateTime.Parse(beginDate));
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);
                                continue;
                            }
                        }
                    }

                }
                if (!crawlAll && list.Count >= this.MaxCount) return list;
            }
            return list;
        }


        public void DealHtml(IList list, string html, bool crawlAll, string ddlIndex, DateTime dateTime)
        {
            Parser parserCtx = new Parser(new Lexer(html));
            NodeList ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("id", "DropDownList2")));
            string classification = ctxNode.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("option"), new HasAttributeFilter("value", ddlIndex)), true).AsString().Replace("&nbsp;", "");

            parserCtx.Reset();

            Parser parserDtl = new Parser(new Lexer(html));
            NodeList aNodes = parserDtl.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
            if (aNodes != null && aNodes.Count == 1 && aNodes[0] is TableTag)
            {
                TableTag table = (TableTag)aNodes[0];
                for (int i = 1; i < table.Rows.Length; i++)
                {

                    string corpName = string.Empty, corpType = string.Empty, corpClassification = string.Empty;
                    int corpAllRanking = 0, classificationRanking = 0;
                    decimal realScore = 0;
                    corpName = table.Rows[i].Columns[1].ToPlainTextString().Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Trim();
                    corpType = table.Rows[i].Columns[2].ToPlainTextString().Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Trim();
                    try
                    {
                        corpAllRanking = int.Parse(table.Rows[i].Columns[3].ToPlainTextString().Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Trim());
                        classificationRanking = Convert.ToInt32(table.Rows[i].Columns[3].ToPlainTextString().Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Trim());
                        realScore = decimal.Parse(table.Rows[i].Columns[4].ToPlainTextString().Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "").Trim());
                    }
                    catch (Exception ex) { Logger.Error(ex + "企业名称：" + corpName); continue; }

                    corpClassification = classification;
                    //DateTime dateTime=DateTime.Parse(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"));
                    CorpCreditss info = ToolDb.GenCorpCreditSS(corpName, corpType, corpClassification, corpAllRanking, classificationRanking, 0, 0, 0, 0, 0, 0, dateTime, realScore, "广东省", "深圳市", DateTime.Now, "深圳市住房和建设局", SiteUrl);
                    if (info != null && !string.IsNullOrEmpty(info.CorpName))
                        ToolDb.SaveEntity(info, this.ExistCompareFields);
                    count++;
                    if (count >= 200)
                    { 
                        Thread.Sleep(1000 * 500); 
                        count = 1; 
                    } 
                    
                }
            }
            
        }
    }
}
