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

namespace Crawler.Instance
{
    /// <summary>
    /// 深圳市区会议信息
    /// </summary>
    public class MeetSz : WebSiteCrawller
    {
        public MeetSz()
            : base()
        {
            this.Group = "会议信息";
            this.Title = "广东省深圳市";
            this.Description = "自动抓取广东省深圳市区会议信息";
            this.ExistCompareFields = "Prov,Area,Road,MeetName,MeetPlace,ProjectName,BeginDate";
            this.MaxCount = 1000;
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,11:30,14:05,14:25,14:50,15:45,16:50,19:00";
            this.SiteUrl = "http://www.szjsjy.com.cn/BusinessInfo/HyxxList.aspx";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
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
            string viewState = this.ToolWebSite.GetAspNetViewState(html);
            string eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
            NameValueCollection nvcBeg = this.ToolWebSite.GetNameValueCollection(
                    new string[] { "__EVENTTARGET", 
                        "__EVENTARGUMENT", 
                        "__LASTFOCUS", 
                        "__VIEWSTATE", 
                        "__VIEWSTATEENCRYPTED",
                    "__EVENTVALIDATION",
                    "ctl00$Content$ddlGCLB",
                    "ctl00$Content$txtStartHysj",
                    "ctl00$Content$txtEndHysj",
                    "ctl00$hdnPageCount"},
                    new string[] { "", "", "", viewState, "", eventValidation, "-1", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"), "1" });
            html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvcBeg, Encoding.UTF8);
            //抓取30天之内的会议信息

            parser= new Parser(new Lexer(html));
            NodeList aNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_Content_GridView1")));
            try
            {
                TableTag table = aNodes[0] as TableTag;
                string temp = table.Rows[table.RowCount - 1].ToPlainTextString();
                temp = temp.GetRegexBegEnd("，共", "页");
                pageInt = int.Parse(temp);
            }
            catch { }
            for (int p = 1; p <= pageInt; p++)
            {
                if (p > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                        new string[] { "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__LASTFOCUS",
                        "__VIEWSTATE",
                        "__VIEWSTATEENCRYPTED",
                        "__EVENTVALIDATION",
                        "ctl00$Content$ddlGCLB",
                        "ctl00$Content$txtStartHysj",
                        "ctl00$Content$txtEndHysj",
                        "ctl00$hdnPageCount"},
                        new string[] { "ctl00$Content$GridView1",
                        "Page$"+p.ToString(),
                        "",
                        viewState,
                        "",
                        eventValidation,
                        "-1",
                        DateTime.Now.ToString("yyyy-MM-dd"),
                       DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"),
                        p.ToString()}
                        );
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl,nvc,Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_Content_GridView1")));
                if (tableNode != null && tableNode.Count > 0)
                {
                    TableTag table = (TableTag)tableNode[0];
                    int rowCount = pageInt > 1 ? table.RowCount - 1 : table.RowCount;
                    for (int i = 1; i < rowCount; i++)
                    {
                        string meetTime = string.Empty, prjName = string.Empty, meetName = string.Empty, place = string.Empty;
                        meetTime = table.Rows[i].Columns[1].ToPlainTextString().Trim();
                        prjName = table.Rows[i].Columns[2].ToPlainTextString().Trim();
                        place = table.Rows[i].Columns[3].ToPlainTextString().Trim();
                        meetName = table.Rows[i].Columns[4].ToPlainTextString().Trim();
                        MeetInfo info = ToolDb.GenMeetInfo("广东省", "深圳市工程", string.Empty, string.Empty, prjName, place, meetName, meetTime,
                            string.Empty, "深圳市建设工程交易中心", SiteUrl, string.Empty, string.Empty, string.Empty, string.Empty);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount)
                        {
                            // 删除 
                            string bDate = DateTime.Today.ToString("yyyy-MM-dd"), eDate = DateTime.Today.AddDays(31).ToString("yyyy-MM-dd");
                            string sqlwhere = " where City='深圳市工程' and BeginDate>='" + bDate + "' and BeginDate<='" + eDate + "' and InfoUrl='" + this.SiteUrl + "'";
                            string delMeetSql = "delete from MeetInfo " + sqlwhere;
                            int countMeet = ToolDb.ExecuteSql(delMeetSql);
                            return list;
                        }
                    }
                }
            }
            if (list != null && list.Count > 0)
            {
                string bDate = DateTime.Today.ToString("yyyy-MM-dd"), eDate = DateTime.Today.AddDays(31).ToString("yyyy-MM-dd");
                string sqlwhere = " where City='深圳市工程' and BeginDate>='" + bDate + "' and BeginDate<='" + eDate + "' and InfoUrl='" + this.SiteUrl + "'";
                string delMeetSql = "delete from MeetInfo " + sqlwhere;
                int countMeet = ToolDb.ExecuteSql(delMeetSql);
            }
            return list;
        }
    }
}