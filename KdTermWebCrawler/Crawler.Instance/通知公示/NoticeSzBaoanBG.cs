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
    public class NoticeSzBaoanBG : WebSiteCrawller
    {
        public NoticeSzBaoanBG()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市宝安区变更公示";
            this.Description = "自动抓取广东省深圳宝安区变更公示";
            this.ExistCompareFields = "Prov,Area,Road,InfoTitle,InfoType,PublishTime,InfoUrl";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryBGList.do?page=1&rows=";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            int sqlCount = 0;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + this.MaxCount);
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
                string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, htmlTxt = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, prjType = string.Empty;

                InfoTitle = Convert.ToString(dic["bdName"]);
                prjCode = Convert.ToString(dic["bdBH"]);
                InfoUrl = Convert.ToString(dic["detailUrl"]);
                InfoType = "变更公示";
                PublistTime = Convert.ToString(dic["faBuTime2"]);
                string bgbt = Convert.ToString(dic["bgBiaoTi"]);
                string urll = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryBGInfoByGuid.do?guid=" + dic["guid"];
                string htmldtl = this.ToolWebSite.GetHtmlByUrl(urll);

                JavaScriptSerializer Newserializer = new JavaScriptSerializer();
                Dictionary<string, object> newTypeJson = (Dictionary<string, object>)Newserializer.DeserializeObject(htmldtl);
                Dictionary<string, object> kdInfo = (Dictionary<string, object>)newTypeJson["bgInfo"];
                string ner = string.Empty, bgls = string.Empty;
               
              
                     ner = Convert.ToString(kdInfo["bgNeiRong"]);
                     bgls = Convert.ToString(kdInfo["bgGongShiLeiXing2"]);

                htmlTxt += "工程编号：" + prjCode + "\r\n工程名称：" + InfoTitle + "\r\n变更类型：" + bgls + "\r\n变更标题：" + bgbt +  "\r\n变更内容：" + ner + "\r\n发布时间：" + PublistTime;
                InfoCtx = htmlTxt;

                NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳宝安区工程", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "深圳市建设工程交易中心宝安分中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, prjType, string.Empty, htmlTxt);
                list.Add(info);
                if (!crawlAll && list.Count >= this.MaxCount) return list;

            }
            return list;
        }

    }
}
