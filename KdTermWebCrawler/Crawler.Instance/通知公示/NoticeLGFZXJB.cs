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
   public class NoticeLGFZXJB : WebSiteCrawller
    {
        public NoticeLGFZXJB()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "深圳市建设交易服中心龙岗分中心截标公示";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市建设交易服中心龙岗分中心截标公示";
            this.ExistCompareFields = "PrjCode,InfoTitle,PrjType";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/queryJBList.do?page=1&rows=";
            this.MaxCount = 400;
      
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
            Dictionary<string, object> smsTypeJson = null;
            smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
            foreach (KeyValuePair<string, object> obj in smsTypeJson)
            {
                if (obj.Key == "total") continue;
                object[] array = (object[])obj.Value;
                foreach (object arrValue in array)
                {
                    string InfoTitle = string.Empty, InfoType = string.Empty,
                       PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty,
                       prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty,
                       prjType = string.Empty;

                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;

                    InfoUrl = "http://jyzx.cb.gov.cn/jyxx/jbxx/";
                    prjCode = Convert.ToString(dic["bdBH"]);
                    InfoTitle = Convert.ToString(dic["bdName"]);
                    InfoType = "截标信息";
                    PublistTime = Convert.ToString(dic["jbTime2"]);
                    prjType = Convert.ToString(dic["gcLeiXing2"]);
                    htmlTxt = string.Format("标段编号：{0}</br>标段名称：{1}</br>工程类型：{2}</br>截标时间：{3}", prjCode, InfoTitle, prjType, PublistTime);
                    InfoCtx = string.Format("标段编号：{0}\r\n标段名称：{1}\r\n工程类型：{2}\r\n截标时间：{3}", prjCode, InfoTitle, prjType, PublistTime);
                    NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳龙岗区工程", "龙岗区", string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "深圳市建设工程交易中心龙岗分中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, prjType, string.Empty, htmlTxt);
                    list.Add(info); 
                    if (!crawlAll && list.Count >= this.MaxCount) return list; 
                }
            }
            return list;

        }
    }
}
