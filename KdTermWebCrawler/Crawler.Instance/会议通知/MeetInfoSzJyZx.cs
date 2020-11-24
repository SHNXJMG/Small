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

namespace Crawler.Instance
{
    public class MeetInfoSzJyZx : WebSiteCrawller
    {
        public MeetInfoSzJyZx()
            : base()
        {
            this.Group = "会议信息";
            this.Title = "广东省深圳市会议信息(2015版)";
            this.Description = "自动抓取广东省深圳市会议信息(2015版)";
            this.PlanTime = "9:05,11:15,14:15,16:15,18:15";
            this.ExistCompareFields = "Prov,Area,Road,MeetName,MeetPlace,ProjectName,BeginDate,PrjCode";
            this.MaxCount = 300;
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw/queryHYList.do?rows=100";
        } 
        protected override IList ExecuteCrawl(bool crawlAll)
        {

            IList list = new List<MeetInfo>();
            DateTime startDate = DateTime.Now.Date;
            DateTime endDate = DateTime.Now.Date.AddDays(30);
            long startTime = ToolHtml.GetDateTimeLong(startDate);
            long endTime = ToolHtml.GetDateTimeLong(endDate);
            string infoUrl = this.SiteUrl + "&startTime=" + startTime + "&endTime" + endTime + "&page=";// "&page=";

            int page = 10;
            for (int i = 1; i <= page; i++)
            {
                string html = string.Empty;
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(infoUrl + i.ToString(),Encoding.UTF8);
                }
                catch
                {
                    continue;
                }
                int startIndex = html.IndexOf("{");
                int endIndex = html.LastIndexOf("}");
                html = html.Substring(startIndex, (endIndex + 1) - startIndex);
                int len = html.Length;
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                Dictionary<string, object> smsTypeJson = null;
                smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
                foreach (KeyValuePair<string, object> obj in smsTypeJson)
                {
                    if (obj.Key == "total") continue;
                    object[] array = (object[])obj.Value;
                    foreach (object arrValue in array)
                    {
                        string meetTime = string.Empty, prjName = string.Empty, meetName = string.Empty, place = string.Empty, builUnit = string.Empty, prjCode = string.Empty;
                        Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                        prjName = Convert.ToString(dic["bdName"]).GetReplace("<br/>");
                        meetTime = Convert.ToString(dic["huiYiStartTime2"]);
                        meetName = Convert.ToString(dic["huiYiLeiXingName"]);
                        place = Convert.ToString(dic["huiYiDiDianName"]);
                        prjCode = Convert.ToString(dic["bdBH"]);
                        MeetInfo info = ToolDb.GenMeetInfo("广东省", "深圳市工程", string.Empty, string.Empty, prjName, place, meetName, meetTime, string.Empty, "深圳市建设工程交易中心", SiteUrl, prjCode, builUnit, string.Empty, string.Empty);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount)
                        {
                            // 删除 
                            string bDate = startDate.ToString("yyyy-MM-dd"), eDate = endDate.ToString("yyyy-MM-dd");
                            string sqlwhere = " where City='深圳市工程' and BeginDate>='" + bDate + "' and BeginDate<='" + eDate + "' and InfoUrl='" + SiteUrl + "'";
                            string delMeetSql = "delete from MeetInfo " + sqlwhere;
                            int countMeet = ToolDb.ExecuteSql(delMeetSql);
                            return list;
                        }
                    }
                }
            }
            if (list != null && list.Count > 0)
            {
                // 删除 
                string bDate = startDate.ToString("yyyy-MM-dd"), eDate = endDate.ToString("yyyy-MM-dd");
                string sqlwhere = " where City='深圳市工程' and BeginDate>='" + bDate + "' and BeginDate<='" + eDate + "' and InfoUrl='" + this.SiteUrl + "'";
                string delMeetSql = "delete from MeetInfo " + sqlwhere;
                int countMeet = ToolDb.ExecuteSql(delMeetSql);
            }
            return list;
        }
    }
}
