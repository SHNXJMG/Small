using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;
using System.Data;
namespace Crawler.Instance
{
    /// <summary>
    /// 深圳宝安区会议信息
    /// </summary>
    public class MeetSzBaoanOld : WebSiteCrawller
    {
        public MeetSzBaoanOld()
            : base(true)
        {
            this.Group = "会议信息";
            this.Title = "广东省深圳市宝安区（旧版）";
            this.Description = "自动抓取广东省深圳宝安区会议信息";
            this.ExistCompareFields = "Prov,Area,Road,MeetName,MeetPlace,ProjectName,BeginDate";
            this.SiteUrl = "http://www.bajsjy.com/SiteManage/GreetingArrange.aspx?MenuName=PublicInformation&ModeId=5&ItemId=hyxx&ItemName=%e4%bc%9a%e8%ae%ae%e4%bf%a1%e6%81%af&clearpaging=true";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();

            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            string viewState = this.ToolWebSite.GetAspNetViewState(parser);
            string eventValidation = this.ToolWebSite.GetAspNetEventValidation(parser);
            string beginDate = DateTime.Today.ToString("yyyy-MM-dd"), endDate = DateTime.Today.AddDays(30).ToString("yyyy-MM-dd");
            NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "__VIEWSTATE","__EVENTTARGET","__EVENTARGUMENT",
                "ctl00$cph_context$GreetingArrangeList1$ImageButton1.x","ctl00$cph_context$GreetingArrangeList1$ImageButton1.y",
                "ctl00$cph_context$GreetingArrangeList1$ddlSearch", "ctl00$cph_context$GreetingArrangeList1$txtStartTime", 
                "ctl00$cph_context$GreetingArrangeList1$txtEndTime","__EVENTVALIDATION","ctl00$ScriptManager1",
                "ctl00$cph_context$GreetingArrangeList1$GridViewPaging1$txtGridViewPagingForwardTo" },
                new string[] { viewState, string.Empty,string.Empty,"23","6",
                    "fbrq", beginDate, endDate,eventValidation,
                    "ctl00$cph_context$GreetingArrangeList1$UpdatePanel1|ctl00$cph_context$GreetingArrangeList1$ImageButton1","1" });
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                // 删除
                string sqlwhere = " where City='深圳市宝安区' and BeginDate>'" + beginDate + "' and BeginDate<'" + endDate + "'";
                string delAttachSql = "delete from BaseAttach where  SourceID in(select Id from MeetInfo " + sqlwhere + ")";
                string delMeetSql = "delete from MeetInfo " + sqlwhere;
                int countlAttach = ToolDb.ExecuteSql(delAttachSql);
                int countMeet = ToolDb.ExecuteSql(delMeetSql);

            }
            catch (Exception ex)
            {


            }

            //处理第一页
            DealHtml(list, html, crawlAll);

            //取得页码
            int pageInt = 1;
            parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_cph_context_GreetingArrangeList1_GridViewPaging1_LblPageCount")));
            if (tdNodes != null)
            {
                string pageTemp = tdNodes[0].ToPlainTextString().Trim();
                try
                {
                    // pageTemp = pageTemp.Substring(pageTemp.IndexOf("页，共")).Replace("页，共", string.Empty).Replace("页", string.Empty);
                    pageInt = int.Parse(pageTemp);
                }
                catch (Exception) { }
            }
            parser.Reset();

            //处理后续页
            if (pageInt > 1)
            {
                string cookiestr = string.Empty;
                for (int i = 2; i <= pageInt; i++)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "__VIEWSTATE","__EVENTTARGET","__EVENTARGUMENT",
                        "ctl00$cph_context$GreetingArrangeList1$GridViewPaging1$btnNext.x","ctl00$cph_context$GreetingArrangeList1$GridViewPaging1$btnNext.y",
                        "ctl00$cph_context$GreetingArrangeList1$ddlSearch", "ctl00$cph_context$GreetingArrangeList1$txtStartTime", 
                        "ctl00$cph_context$GreetingArrangeList1$txtEndTime","__EVENTVALIDATION","ctl00$ScriptManager1",
                        "ctl00$cph_context$GreetingArrangeList1$GridViewPaging1$txtGridViewPagingForwardTo" },
                        new string[] { viewState, string.Empty,string.Empty,"6","4",
                        "fbrq", beginDate, endDate,eventValidation,
                        "ctl00$cph_context$GreetingArrangeList1$update1|ctl00$cph_context$GreetingArrangeList1$GridViewPaging1$btnNext",(i-1).ToString() });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                        //处理后续页
                        DealHtml(list, html, crawlAll);
                    }
                    catch (Exception ex)
                    {
                        continue;

                    }

                }
            }

            return list;
        }

        public void DealHtml(IList list, string html, bool crawlAll)
        {
            Parser parserDtl = new Parser(new Lexer(html));
            NodeList aNodes = parserDtl.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_GreetingArrangeList1_GridView1")));
            if (aNodes != null && aNodes.Count == 1 && aNodes[0] is TableTag)
            {
                TableTag table = (TableTag)aNodes[0];
                for (int i = 1; i < table.Rows.Length; i++)
                {
                    if (table.Rows[i].Columns.Length == 5)
                    {
                        string meetTime = string.Empty, prjName = string.Empty, meetName = string.Empty, place = string.Empty;
                        meetTime = table.Rows[i].Columns[1].ToPlainTextString().Trim();
                        prjName = table.Rows[i].Columns[2].ToPlainTextString().Trim();
                        place = table.Rows[i].Columns[3].ToPlainTextString().Trim();
                        meetName = table.Rows[i].Columns[4].ToPlainTextString().Trim();

                        MeetInfo info = ToolDb.GenMeetInfo("广东省", "深圳宝安区工程", string.Empty, string.Empty, prjName, place, meetName, meetTime,
                            string.Empty, "深圳市建设工程交易中心宝安分中心", SiteUrl, string.Empty, string.Empty, string.Empty, string.Empty);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return;
                    }
                }
            }
            parserDtl.Reset();
        }
    }
}