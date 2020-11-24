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
    public class NoticeLongGangZSJYJGS : WebSiteCrawller
    {
        public NoticeLongGangZSJYJGS()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "深圳市建设交易服中心龙岗分中心资审及业绩公告";
            this.Description = "自动抓取深圳市建设交易服中心龙岗分中心资审及业绩公告";
            this.PlanTime = "9:10,11:10,14:10,16:10,18:10";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/queryYeJiList.do?page=1&rows=";
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
            foreach (KeyValuePair<string, object> obj in smsTypeJson)
            {
                if (obj.Key == "total") continue;
                object[] array = (object[])obj.Value;
                foreach (object arrValue in array)
                {
                    string InfoTitle = string.Empty, InfoType = string.Empty, bgType = string.Empty, prjType = string.Empty,
                        PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty,
                        prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty,
                        infoSource = string.Empty;
                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    prjCode = Convert.ToString(dic["bdBH"]);
                    InfoTitle = Convert.ToString(dic["bdName"]);
                    InfoType = "资审及业绩公示";
                    PublistTime = Convert.ToString(dic["faBuTime"]);
                    InfoUrl = Convert.ToString(dic["detailUrl"]);

                    if (InfoTitle.Contains("测试"))//龙岗区小型建设工程承包商预选库批量招标
                        continue;
                    try
                    {
                        Uri uri = new Uri(InfoUrl);
                        string url = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/queryOldOTDataDetail.do" + uri.Query;
                        htmlTxt = this.ToolWebSite.GetHtmlByUrl(url);
                        htmlTxt = htmlTxt.GetReplace("\"");
                    }
                    catch { continue; }
                    InfoCtx = htmlTxt.GetReplace("<br />", "\r\n").GetReplace("</tr>", "\r\n").ToCtxString();
                    buildUnit = InfoCtx.GetBuildRegex();

                    infoSource = "深圳市建设工程交易服务中心龙岗分中心";
                    NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳龙岗区工程", "龙岗区", string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, infoSource, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, prjType, bgType, htmlTxt);
                    list.Add(info);

                    ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx);

                    if (!crawlAll && list.Count >= this.MaxCount) return list;

                    Parser parser = new Parser(new Lexer(htmlTxt));
                    NodeList fileNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                    if (fileNode != null && fileNode.Count > 0)
                    {
                        for (int f = 0; f < fileNode.Count; f++)
                        {
                            ATag tag = fileNode[f] as ATag;
                            if (tag.IsAtagAttach() || tag.Link.ToLower().Contains("downloadfile"))
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
                                        link = link.GetReplace("\"", "");
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
                }
            }
                    return list;
        }
    }
}
