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
using System.Linq;


namespace Crawler.Instance
{
    public class NoticeInfoSZBAJB : WebSiteCrawller
    {
        public NoticeInfoSZBAJB()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市宝安区截标信息";
            this.PlanTime = "9:25,11:25,14:15,17:25";
            this.Description = "自动抓取广东省深圳市交易中心截标信息";
            this.ExistCompareFields = "PrjCode,InfoTitle,PrjType,PublishTime";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryJBList.do?page=1&rows=";
            this.MaxCount = 200;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            DateTime endDate = DateTime.Today.AddDays(1);
            DateTime startDate = endDate.AddDays(-60);
            long startTime = ToolHtml.GetDateTimeLong(startDate);
            long endTime = ToolHtml.GetDateTimeLong(endDate);
            string infoUrl = this.SiteUrl + this.MaxCount + "&jbTimeMin=" + startTime + "&jbTimeMax=" + endTime;
            string html = string.Empty;

            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.UTF8);
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
                string InfoTitle = string.Empty, InfoType = string.Empty,
                        PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty,
                        prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty, prjType = string.Empty;
                    Dictionary<string, object> dic = (Dictionary<string, object>)objValue;

                    InfoType = "截标信息";
                    prjCode = Convert.ToString(dic["bdBH"]);
                    InfoTitle = Convert.ToString(dic["bdName"]);
                    prjType = Convert.ToString(dic["gcLeiXing2"]);
                    PublistTime = Convert.ToString(dic["jbTime2"]);
                    InfoUrl = "http://www.bajsjy.com/jyxx/jbxx/";

                    htmlTxt = string.Format("标段编号：{0}</br>标段名称：{1}</br>工程类型：{2}</br>截标时间：{3}", prjCode, InfoTitle, prjType, PublistTime);
                    InfoCtx = string.Format("标段编号：{0}\r\n标段名称：{1}\r\n工程类型：{2}\r\n截标时间：{3}", prjCode, InfoTitle, prjType, PublistTime);
                    NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳宝安区工程", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "深圳市建设工程交易中心宝安分中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, prjType, string.Empty, htmlTxt);
                    list.Add(info);                 
                    if (!crawlAll && list.Count >= this.MaxCount) return list;              
            }
            return list;
        }
    }
}
