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
    public class NoticeSzJyzxJbxx : WebSiteCrawller
    {
        public NoticeSzJyzxJbxx()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市交易中心截标信息(2015版)";
            this.PlanTime = "9:27,11:27,14:17,17:27";
            this.Description = "自动抓取广东省深圳市交易中心截标信息(2015版)";
            this.ExistCompareFields = "PrjCode,InfoTitle,PrjType,PublishTime";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw/queryJBList.do?page=1&rows=";
            this.MaxCount = 100;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            string html = string.Empty;

            DateTime beginDate = DateTime.Today;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + (MaxCount + 20) + "&jbTimeMin=" + ToolHtml.GetDateTimeLong(beginDate) + "&jbTimeMax=" + ToolHtml.GetDateTimeLong(beginDate.AddDays(7).Date));
            }
            catch { return null; }
            int startIndex = html.IndexOf("{");
            int endIndex = html.LastIndexOf("}");
            html = html.Substring(startIndex, (endIndex + 1) - startIndex);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
            foreach (KeyValuePair<string, object> obj in smsTypeJson)
            {
                if (obj.Key == "total") continue;
                object[] array = (object[])obj.Value;
                foreach (object arrValue in array)
                {
                    string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty, prjType = string.Empty, bgType = string.Empty;
                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    prjCode = Convert.ToString(dic["bdBH"]);
                    InfoTitle = Convert.ToString(dic["bdName"]);
                    prjType = Convert.ToString(dic["gcLeiXing2"]);
                    PublistTime = Convert.ToString(dic["jbTime2"]);
                    InfoType = "截标信息";
                    //InfoUrl = "http://61.144.226.5:8001/jyw/queryOldDataDetail.do?type=3&id=" + Convert.ToString(dic["ggBDGuid"]);

                    htmlTxt = InfoCtx = "工程编号：" + prjCode + "\r\n工程名称：" + InfoTitle + "\r\n工程类型：" + prjType + "\r\n截标日期：" + PublistTime;
                    NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳市工程", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.ShenZhenMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, prjType, bgType, htmlTxt);
                    list.Add(info);
                    if (!crawlAll && list.Count >= this.MaxCount)
                    {
                        string delSql = string.Format("delete from NoticeInfo where InfoType='{0}' and PublistTime>='{1}' and PublistTime<='{2}' and InfoSource='{3}'", info.InfoType, beginDate, beginDate.AddDays(7), info.InfoSource);
                        ToolDb.ExecuteSql(delSql);
                        return list;
                    }
                }
            }
            if (list != null && list.Count > 0)
            {
                string delSql = string.Format("delete from NoticeInfo where InfoType='截标信息' and PublistTime>='{0}' and PublistTime<='{1}' and InfoSource='{2}'",  beginDate, beginDate.AddDays(7), MsgTypeCosnt.ShenZhenMsgType);
                ToolDb.ExecuteSql(delSql);
            }
            return list;
        }
    }
}
