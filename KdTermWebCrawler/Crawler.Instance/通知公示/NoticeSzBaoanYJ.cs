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
    public class NoticeSzBaoanYJ : WebSiteCrawller
    {
        public NoticeSzBaoanYJ()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市宝安区资审及业绩公示";
            this.Description = "自动抓取广东省深圳宝安区资审及业绩公示";
            this.PlanTime = "9:10,11:10,14:10,16:10,18:10";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryYeJiList.do?page=1&rows=";
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
                string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty,
                    InfoCtx = string.Empty, InfoUrl = string.Empty, htmlTxt = string.Empty, prjCode = string.Empty,
                    buildUnit = string.Empty, prjType = string.Empty;
                Dictionary<string, object> dic = (Dictionary<string, object>)objValue;
                prjCode = Convert.ToString(dic["bdBH"]);
                InfoTitle = Convert.ToString(dic["bdName"]);
                InfoType = "资审及业绩公示";
                PublistTime = Convert.ToString(dic["faBuTime"]);
                InfoUrl = Convert.ToString(dic["detailUrl"]);
                string idt = Convert.ToString(dic["pbYeJiGongShiGuid"]);
                try
                {
                    string urll = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryOldOTDataDetail.do?type=2&id=" + idt;
                    htmlTxt = this.ToolWebSite.GetHtmlByUrl(urll).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                }
                catch (Exception ex) { continue; }
                InfoCtx = htmlTxt.Replace("</tr>", "\r\n").Replace("<br />", "\r\n").Replace("<BR>", "\r\n").ToCtxString();
                NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳宝安区工程", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "深圳市建设工程交易中心宝安分中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, prjType, string.Empty, htmlTxt);
                            list.Add(info);
                if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                {
                    Parser parser = new Parser(new Lexer(htmlTxt));
                    NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                    if (aNode != null && aNode.Count > 0)
                    {
                        for (int k = 0; k < aNode.Count; k++)
                        {
                            ATag a = aNode[k] as ATag;


                            string link = string.Empty;
                            if (a.Link.ToLower().Contains("http"))
                            {
                                link = a.Link.Replace("\\", "");

                                BaseAttach attach = null;
                                try
                                {
                                    attach = ToolHtml.GetBaseAttach(link, a.LinkText, info.Id, "SiteManage\\Files\\Attach\\");
                                }
                                catch { }
                                if (attach != null)
                                    ToolDb.SaveEntity(attach, "");
                            }

                        }
                    }
                }
                if (!crawlAll && list.Count >= this.MaxCount) return list;
                                                         
            }
            return list;
        }  
    }
}
