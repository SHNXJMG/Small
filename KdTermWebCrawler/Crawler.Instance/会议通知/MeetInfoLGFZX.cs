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
    public class MeetInfoLGFZX : WebSiteCrawller
    {
        public MeetInfoLGFZX()
        {
            this.Group = "会议信息";
            this.Title = "深圳市建设交易服中心龙岗分中心会议信息";
            this.Description = "自动抓取深圳市建设交易服中心龙岗分中心会议信息";
            this.PlanTime = "9:10,11:10,14:10,16:10,18:10";
            this.ExistCompareFields = "Prov,Area,Road,MeetName,MeetPlace,ProjectName,BeginDate,PrjCode";
            this.MaxCount = 300;
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/queryHYList.do?page=1&rows=";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<MeetInfo>();
            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddDays(30);
            long startTime = ToolHtml.GetDateTimeLong(startDate);
            long endTime = ToolHtml.GetDateTimeLong(endDate);
            string infoUrl = this.SiteUrl + this.MaxCount + "&startTime=" + startTime + "&endTime" + endTime;


            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.UTF8);
            }
            catch
            {
                 
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
                    string meetTime = string.Empty, prjName = string.Empty, meetName = string.Empty,
                        place = string.Empty, builUnit = string.Empty, prjCode = string.Empty;
                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    prjName = Convert.ToString(dic["bdName"]).GetReplace("<br/>");
                    meetTime = Convert.ToString(dic["huiYiStartTime2"]);
                    meetName = Convert.ToString(dic["huiYiLeiXingName"]);
                    place = Convert.ToString(dic["huiYiDiDianName"]);
                    prjCode = Convert.ToString(dic["bdBH"]);
                    MeetInfo info = ToolDb.GenMeetInfo("广东省", "深圳龙岗区工程", string.Empty, string.Empty, prjName, place, meetName, meetTime, string.Empty, "深圳市建设工程交易中心龙岗分中心", SiteUrl, prjCode, builUnit, string.Empty, string.Empty);
                    list.Add(info);
                    if (!crawlAll && list.Count >= this.MaxCount)
                    {
                        // 删除 
                        string bDate = startDate.ToString("yyyy-MM-dd"), eDate = endDate.ToString("yyyy-MM-dd"); 
                        string sqlwhere = " where City='深圳龙岗区工程' and InfoUrl='"+SiteUrl+"' and BeginDate>='" + bDate + "' and BeginDate<='" + eDate + "'";
                        string delMeetSql = "delete from MeetInfo " + sqlwhere;
                        int countMeet = ToolDb.ExecuteSql(delMeetSql);
                        return list;
                    }
                }
            }
            if (list != null && list.Count > 0)
            {
                // 删除 
                string bDate = startDate.ToString("yyyy-MM-dd"), eDate = endDate.ToString("yyyy-MM-dd");
                string sqlwhere = " where City='深圳龙岗区工程' and InfoUrl='" + SiteUrl + "' and BeginDate>='" + bDate + "' and BeginDate<='" + eDate + "'";
                string delMeetSql = "delete from MeetInfo " + sqlwhere;
                int countMeet = ToolDb.ExecuteSql(delMeetSql);
            }
            return list;

        }

    }
}
