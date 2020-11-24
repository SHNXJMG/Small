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
    public class NoticeSzJyzxBggs : WebSiteCrawller
    {
        public NoticeSzJyzxBggs()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市交易中心变更公示(2015版)";
            this.PlanTime = "9:27,11:27,14:17,17:27";
            this.Description = "自动抓取广东省深圳市交易中心变更公示(2015版)";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw/queryBGList.do?page=1&rows=";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            string html = string.Empty, cookiestr=string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + (MaxCount + 20),Encoding.UTF8,ref cookiestr);
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
                    string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty,prjType=string.Empty,bgType=string.Empty;
                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    prjCode = Convert.ToString(dic["bdBH"]);
                    InfoTitle = Convert.ToString(dic["bgBiaoTi"]); 
                    bgType = Convert.ToString(dic["bgGongShiLeiXing2"]);
                    InfoType = "变更公示";
                    PublistTime = Convert.ToString(dic["faBuTime2"]);
                    InfoUrl = "https://www.szjsjy.com.cn:8001/jyw/queryOldDataDetail.do?type=8&guid=" + Convert.ToString(dic["guid"]);
                    //string newUrl = "https://www.szjsjy.com.cn:8001/jyw/jyw/bggs_View.do?guid=" + Convert.ToString(dic["guid"]);
                    //string htmldtl = string.Empty;
                    //try
                    //{
                    //    htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);//.GetReplace("\\t,\\r,\\n,\"");
                    //     htmlTxt = this.ToolWebSite.GetHtmlByUrl(newUrl).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                    //}
                    //catch {  }
                    //var bgList={"total":13815,"rows":[{"guid":"201092","bgGongShiLeiXing":109,"bgBiaoTi":"后海南河闸门改造工程（土建）(截标时间变更)","faBuTime":1446447709790,"bdBH":"4403052015502201","bdName":"后海南河闸门改造工程（土建）","faBuTime2":"2015-11-02","bgGongShiLeiXing2":"截标时间","isOldData":true,"detailUrl":"https://www.szjsjy.com.cn:8001/jyw/jyw/oldData_View.do?type=8&id=201092"},

                    htmlTxt =  "标段编号：" + prjCode + "<br/>标段名称：" + dic["bdName"] + "<br/>变更类型：" + InfoTitle + "<br/>变更内容：" + dic["bgGongShiLeiXing2"] + "<br/>发布时间：" + PublistTime;

                    InfoCtx = htmlTxt.GetReplace("<br />,<br/>,</ br>,</br>", "\r\n").ToCtxString() + "\r\n";
                    NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳市工程", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.ShenZhenMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty,prjType,bgType, htmlTxt);
                    list.Add(info);
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }
            return list;
        }
    }
}
