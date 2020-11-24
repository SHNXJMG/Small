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
    public class NoticeSzJyzxKzjgs : WebSiteCrawller
    {
        public NoticeSzJyzxKzjgs()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市交易中心控制价公示(2015版)";
            this.PlanTime = "9:27,11:27,14:17,17:27";
            this.Description = "自动抓取广东省深圳市交易中心控制价公示(2015版)";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw/queryKongZhiJiaList.do?page=1&rows=";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + (MaxCount + 20));
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
                    PublistTime = Convert.ToString(dic["fbStartTime2"]);
                    InfoType = "标底公示";
                    InfoUrl = "https://www.szjsjy.com.cn:8001/jyw/showKongZhiJia.do?kzjGuid=" + Convert.ToString(dic["kzJGuid"]) + "&ggGuid=&bdGuid=";
                    try
                    {
                        htmlTxt = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString().GetReplace("\\t,\\r,\\n,\",{html:,}");
                    }
                    catch { }
                    if (string.IsNullOrEmpty(htmlTxt))
                    {
                        InfoUrl = "https://www.szjsjy.com.cn:8001/jyw/queryOldDataDetail.do?type=3&id=" + Convert.ToString(dic["kzJGuid"]);
                        try
                        {
                            htmlTxt = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString().GetReplace("\\t,\\r,\\n,\",{html:,}");
                        }
                        catch { continue; }
                    }
                    InfoCtx = htmlTxt.GetReplace("<br />,<br/>,</ br>,</br>", "\r\n").ToCtxString() + "\r\n";
                    NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳市工程", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.ShenZhenMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, prjType, bgType, htmlTxt);
                    list.Add(info);
                    Parser parser = new Parser(new Lexer(htmlTxt));
                    NodeList aList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                    if (aList != null && aList.Count > 0)
                    {
                        for (int k = 0; k < aList.Count; k++)
                        {
                            ATag aTag = aList[k] as ATag;
                            if (aTag.IsAtagAttach() || aTag.Link.Contains("download"))
                            {
                                string alink = string.Empty;
                                if (!aTag.Link.ToLower().Contains("http"))
                                    alink = "https://www.szjsjy.com.cn:8001/" + aTag.Link.GetReplace("\\");
                                else
                                    alink = aTag.Link.GetReplace("\\");
                                BaseAttach attach = ToolDb.GenBaseAttach(aTag.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, alink);
                                base.AttachList.Add(attach);
                            }
                        }
                    }
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }

            return list;
        }
    }
}
