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
using System.IO;
using System.Web.UI.MobileControls;
using System.Collections.Generic;
using System.Web.Script.Serialization;


namespace Crawler.Instance
{
    public class NoticeSzLonggangBGGS : WebSiteCrawller
    {
        public NoticeSzLonggangBGGS()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市龙岗区变更公示";
            this.Description = "自动抓取广东省深圳市龙岗区变更公示"; 
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/queryBGList.do?page=1&rows=";
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
            foreach (KeyValuePair<string, object> obj in smsTypeJson)
            {
                if (obj.Key == "total") continue;

                object[] array = (object[])obj.Value;
                foreach (object arrValue in array)
                {
                    string InfoTitle = string.Empty, InfoType = string.Empty, bgType = string.Empty, prjType = string.Empty,
                        PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty,
                        prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty;
                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    prjCode = Convert.ToString(dic["bdBH"]);
                    InfoTitle = Convert.ToString(dic["bdName"]);
                    InfoType = "变更公示";
                    bgType = Convert.ToString(dic["bgGongShiLeiXing2"]);
                    PublistTime = Convert.ToString(dic["faBuTime2"]);
                    InfoUrl = Convert.ToString(dic["detailUrl"]);

                    if (InfoUrl.Contains("guid"))
                    {
                        htmlTxt = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        Uri uri = new Uri(InfoUrl);
                        string url = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/queryBGInfoByGuid.do" + uri.Query;

                        string jsonHtml = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8).GetJsString();

                        JavaScriptSerializer newSerializer = new JavaScriptSerializer();
                        Dictionary<string, object> newTypeJson = (Dictionary<string, object>)newSerializer.DeserializeObject(jsonHtml);
                        Dictionary<string, object> kdInfo = (Dictionary<string, object>)newTypeJson["bgInfo"];
                        string bdName = string.Empty,
                            bglie = string.Empty, bgneYo = string.Empty;

                        bdName = Convert.ToString(kdInfo["bgBiaoTi"]);
                        bglie = Convert.ToString(kdInfo["bgGongShiLeiXing2"]);
                        bgneYo = Convert.ToString(kdInfo["bgNeiRong"]);

                        Parser parserNew = new Parser(new Lexer(htmlTxt));
                        NodeList tableNode = parserNew.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "de_tab1")));
                        if (tableNode != null && tableNode.Count > 0)
                        {
                            htmlTxt = tableNode.AsHtml();
                            htmlTxt = htmlTxt.GetReplace("<td class=\"in_bg_td\">标段编号</td>", "<td class=\"in_bg_td\">标段编号</td><td class=\"in_bg\">" + prjCode + "</td>");
                            htmlTxt = htmlTxt.GetReplace("<td class=\"in_bg_td\">标段名称</td>", "<td class=\"in_bg_td\">标段名称</td><td class=\"in_bg\">" + bdName + "</td>");
                            htmlTxt = htmlTxt.GetReplace("<td id=\"bgType\">&nbsp;</td>", "<td id=\"bgType\">&nbsp;" + bglie + "</td>");
                            htmlTxt = htmlTxt.GetReplace("<td id=\"bgNeiRong\">&nbsp;</td>", "<td id=\"bgNeiRong\">&nbsp;" + bgneYo + "</td>");
                        }
                    }
                    else
                    {
                        Uri uri = new Uri(InfoUrl);
                        string url = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/queryOldOTDataDetail.do" + uri.Query;
                        try
                        {
                            htmlTxt = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                    }

                    InfoCtx = htmlTxt.Replace("<br />", "\r\n").ToCtxString();
                    buildUnit = InfoCtx.GetBuildRegex();

                    NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳龙岗区工程", "龙岗区", string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "深圳市建设工程交易中心龙岗分中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, prjType, bgType, htmlTxt);
                    list.Add(info);

                    Parser parser = new Parser(new Lexer(htmlTxt));
                    NodeList fileNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                    if (fileNode != null && fileNode.Count > 0)
                    {
                        for (int f = 0; f < fileNode.Count; f++)
                        {
                            ATag tag = fileNode[f] as ATag;
                            if (tag.IsAtagAttach())
                            {
                                try
                                {
                                    string link = string.Empty;
                                    if (tag.Link.ToLower().Contains("http"))
                                    {
                                        link = tag.Link;
                                        if (link.StartsWith("\\"))
                                            link = link.Substring(link.IndexOf("\\"), link.Length - link.IndexOf("\\"));
                                        if (link.EndsWith("//"))
                                            link = link.Remove(link.LastIndexOf("//"));
                                        link = link.GetReplace("\\", "");
                                    }
                                    else
                                        link = "https://www.szjsjy.com.cn:8001/" + tag.Link;

                                    BaseAttach attach = ToolDb.GenBaseAttach(tag.LinkText, info.Id, link);
                                    base.AttachList.Add(attach);
                                }
                                catch { continue; }
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
