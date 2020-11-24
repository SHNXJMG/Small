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
using System.Web.Script.Serialization;
using System.Threading;
namespace Crawler.Instance
{
    /// <summary>
    /// 深圳宝安区会议信息
    /// </summary>
    public class MeetSzBaoan : WebSiteCrawller
    {
        public MeetSzBaoan()
            : base()
        {
            this.Group = "会议信息";
            this.Title = "广东省深圳市宝安区";
            this.Description = "自动抓取广东省深圳宝安区会议信息";
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,11:30,14:05,14:25,14:50,15:49,16:50,19:00";
            this.ExistCompareFields = "Prov,City,MeetName,MeetPlace,ProjectName,BeginDate,PrjCode,InfoSource";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryHYList.do?page=1&rows=";
            this.MaxCount = 200;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<MeetInfo>();
            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddDays(30);
            long startTime = ToolHtml.GetDateTimeLong(startDate);
            long endTime = ToolHtml.GetDateTimeLong(endDate);
            string Url = this.SiteUrl + this.MaxCount + "&startTime=" + startTime + "&endTime=" + endTime;
            string infUrl = "http://www.bajsjy.com/jyxx/hyxx/";
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(Url);
            }
            catch { return null; }
            int startIndex = html.IndexOf("{");
            int endIndex = html.LastIndexOf("}");
            html = html.Substring(startIndex, (endIndex + 1) - startIndex);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
            object[] objvalues = smsTypeJson["rows"] as object[];
            foreach (object objValue in objvalues)
            {
                Dictionary<string, object> dic = (Dictionary<string, object>)objValue;
                string meetTime = string.Empty, prjName = string.Empty, meetName = string.Empty, place = string.Empty,prjCode=string.Empty;

                prjCode = Convert.ToString(dic["bdBH"]);
                meetTime = Convert.ToString(dic["huiYiStartTime2"]);
                prjName = Convert.ToString(dic["bdName"]);
                place = Convert.ToString(dic["huiYiDiDianName"]);
                meetName = Convert.ToString(dic["huiYiLeiXingName"]);
                MeetInfo info = ToolDb.GenMeetInfo("广东省", "深圳宝安区工程", string.Empty, string.Empty, prjName, place, meetName, meetTime,  string.Empty, "深圳市建设工程交易中心宝安分中心", infUrl, prjCode, string.Empty, string.Empty, string.Empty);
                list.Add(info);
                if (!crawlAll && list.Count >= this.MaxCount)
                {
                    // 删除 
                    string bDate = startDate.ToString("yyyy-MM-dd"), eDate = endDate.ToString("yyyy-MM-dd");
                    string sqlwhere = " where City='深圳宝安区工程' and InfoUrl='" + infUrl + "' and BeginDate>='" + bDate + "' and BeginDate<='" + eDate + "'";
                    string delMeetSql = "delete from MeetInfo " + sqlwhere;
                    int countMeet = ToolDb.ExecuteSql(delMeetSql);
                    return list;
                }
            }
            if (list != null && list.Count > 0)
            {
                // 删除 
                string bDate = startDate.ToString("yyyy-MM-dd"), eDate = endDate.ToString("yyyy-MM-dd");
                string sqlwhere = " where City='深圳宝安区工程' and InfoUrl='" + infUrl + "' and BeginDate>='" + bDate + "' and BeginDate<='" + eDate + "'";
                string delMeetSql = "delete from MeetInfo " + sqlwhere;
                int countMeet = ToolDb.ExecuteSql(delMeetSql);
            }
            return list;
        }
    }
}