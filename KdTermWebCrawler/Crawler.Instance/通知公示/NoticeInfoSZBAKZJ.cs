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
    public class NoticeInfoSZBAKZJ : WebSiteCrawller
    {
        public NoticeInfoSZBAKZJ()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市宝安区控制价公示";
            this.PlanTime = "9:25,11:25,14:15,17:25";
            this.Description = "自动抓取广东省深圳市宝安区控制价公示";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://112.112.12.76:8721/jyw-ba/jyxx/queryKongZhiJiaList.do?page=1&rows=";
            this.MaxCount = 100;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>(); 
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
                string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty, prjType = string.Empty;
                InfoType = "控制价公示";
                InfoTitle = Convert.ToString(dic["ggName"]);
                prjCode = Convert.ToString(dic["bdBH"]);
                prjType = Convert.ToString(dic["gcLeiXing2"]);
                PublistTime = Convert.ToString(dic["fbStartTime2"]);
                InfoUrl = Convert.ToString(dic["detailUrl"]);
                string url = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryOldOTDataDetail.do?type=3&id=" + prjCode;
                try
                { 
                    htmlTxt = this.ToolWebSite.GetHtmlByUrl(url).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                }
                catch { continue; }
                InfoCtx = htmlTxt.Replace("<br />", "\r\n").Replace("<BR>", "\r\n").ToCtxString();
                buildUnit = InfoCtx.GetBuildRegex();
                NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳宝安区工程", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "深圳市建设工程交易中心宝安分中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, prjType, string.Empty, htmlTxt);
                list.Add(info);
                Parser parser = new Parser(new Lexer(htmlTxt));
                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                if (aNode != null && aNode.Count > 0)
                {
                    for (int k = 0; k < aNode.Count; k++)
                    {
                        ATag aTag = aNode[k] as ATag;
                        if (aTag.IsAtagAttach())
                        {
                            string alink = aTag.Link.GetReplace("\\", "");
                            BaseAttach attach = ToolDb.GenBaseAttach(aTag.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, alink);
                            base.AttachList.Add(attach);
                        }
                    }
                }
                if (!crawlAll && list.Count >= this.MaxCount) return list;
            }
            return list;
        }
    }
}
