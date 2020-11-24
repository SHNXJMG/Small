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
    public class BidSituationSzLGFZX : WebSiteCrawller
    {
        public BidSituationSzLGFZX()
            : base()
        {
            this.Group = "开标定标";
            this.Title = "深圳市建设交易服中心龙岗分中心开标情况公示";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市建设交易服中心龙岗分中心开标情况公示";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/queryKBJiLuList.do?page=1&rows=";
            this.MaxCount = 500; 

        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidSituation>();
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
                    string code = string.Empty, prjName = string.Empty,
                        PublicityEndDate = string.Empty, InfoUrl = string.Empty,
                        msgType = string.Empty, ctx = string.Empty, HtmlTxt = string.Empty,
                        beginDate = string.Empty;
                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    code = Convert.ToString(dic["bdBH"]);
                    prjName = Convert.ToString(dic["bdName"]);
                    beginDate = Convert.ToString(dic["faBuTime2"]);
                    string idt = Convert.ToString(dic["kbJiLuGuid"]);


                    //if (!prjName.Contains("龙岗ceshi项目"))
                    //    continue; 
 

                    InfoUrl = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/queryOldOTDataDetail.do?type=5&id=" + code;

                    string attachJson = string.Empty;
                    try
                    {
                        HtmlTxt = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString().GetReplace("\\t,\\r,\\n,\"");

                        if (string.IsNullOrWhiteSpace(HtmlTxt))
                        {
                            string kdGuid = Convert.ToString(dic["kbJiLuGuid"]);
                            InfoUrl = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/kbJiLu_View.do?kbJiLuGuid=" + kdGuid;
                            HtmlTxt = this.ToolWebSite.GetHtmlByUrl(InfoUrl);
                            string url = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/querykbJiLuDetail.do?ggGuid=&bdGuid=&kbJiLuGuid=" + kdGuid;
                            attachJson = this.ToolWebSite.GetHtmlByUrl(url);
                        }
                    }
                    catch { continue; }
                    string gcBh = string.Empty, gcName = string.Empty, gcLeixing = string.Empty,
                        jywTime = string.Empty, kbjiGuid = string.Empty, surl = string.Empty,
                        attachId = string.Empty;
                    
                    if (!string.IsNullOrWhiteSpace(attachJson))
                    {
                        JavaScriptSerializer newSerializer = new JavaScriptSerializer();
                        Dictionary<string, object> newTypeJson = (Dictionary<string, object>)newSerializer.DeserializeObject(attachJson);
                        Dictionary<string, object> kdInfo = (Dictionary<string, object>)newTypeJson["kbJiLu"];

                        try
                        {
                            attachId = Convert.ToString(kdInfo["attachFileGroupGuid"]);
                        }
                        catch { }
                        gcLeixing = Convert.ToString(kdInfo["gcLeiXing"]);
                        jywTime = Convert.ToString(kdInfo["jywFaBuEndTime"]);
                       
                        surl = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/kbJiLu_View.do?kbJiLuGuid=" + attachId;
                        attachJson = this.ToolWebSite.GetHtmlByUrl(surl);

                        HtmlTxt = attachJson;
                        Parser parserNew = new Parser(new Lexer(HtmlTxt));
                        NodeList tableNode = parserNew.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "de_tab1")));
                        if (tableNode != null && tableNode.Count > 0)
                        {
                            HtmlTxt = tableNode.AsHtml();
                            HtmlTxt = HtmlTxt.GetReplace("<td id=\"bdBH\">&nbsp;</td>", "<td id=\"bdBH\">&nbsp;"+ code + "</td>");
                            HtmlTxt = HtmlTxt.GetReplace("<td id=\"bdName\">&nbsp;</td>", "<td  id=\"bdName\">&nbsp;" + prjName + "</td>");
                            HtmlTxt = HtmlTxt.GetReplace("<td id=\"gcLeiXing\">&nbsp;</td>", "<td id=\"gcLeiXing\">&nbsp;" + gcLeixing + "</td>");
                            HtmlTxt = HtmlTxt.GetReplace("<td id=\"jieZhiTime\">&nbsp;</td>", "<td id=\"jieZhiTime\">&nbsp;" + jywTime + "</td>");
                            ctx = HtmlTxt.Replace("</tr>", "\r\n").ToCtxString();
                            

                        }
                    } 
                    ctx = HtmlTxt.ToCtxString();
                    msgType = "深圳市建设工程交易中心龙岗分中心";
                    string saveUrl = Convert.ToString(dic["detailUrl"]);

                    BidSituation info = ToolDb.GetBidSituation("广东省", "深圳龙岗区工程", "龙岗区", code, prjName, PublicityEndDate, msgType, saveUrl, ctx, HtmlTxt, beginDate);

                    sqlCount++;
                    if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                    {
                        if (!string.IsNullOrWhiteSpace(attachId))
                        {
                            string moJson = string.Empty;
                            string sUrl = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/filegroup/queryByGroupGuidZS.do?groupGuid=" + attachId;
                            try
                            {
                                moJson = this.ToolWebSite.GetHtmlByUrl(sUrl);
                            }
                            catch { }
                            if (!string.IsNullOrWhiteSpace(moJson))
                            {
                                JavaScriptSerializer newSerializers = new JavaScriptSerializer();
                                Dictionary<string, object> newTypeJsons = (Dictionary<string, object>)newSerializers.DeserializeObject(moJson);
                                Dictionary<string, object> mofo = (Dictionary<string, object>)newTypeJsons;
                                object[] objs = (object[])mofo["rows"];
                                foreach (object objAttach in objs)
                                {
                                    Dictionary<string, object> attachs = (Dictionary<string, object>)objAttach;
                                    string attachguid = Convert.ToString(attachs["attachGuid"]);
                                    string attachName = Convert.ToString(attachs["attachName"]);
                                    string link = "https://www.szjsjy.com.cn:8001/file/downloadFile?fileId=" + attachguid;
                                    BaseAttach attach = ToolHtml.GetBaseAttach(link, attachName, info.Id, "SiteManage\\Files\\Attach\\");
                                    if (attach != null)
                                        ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                                }
                            }
                        }
                        else
                        {
                            Parser parser = new Parser(new Lexer(HtmlTxt));
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
                                            BaseAttach attach = null;
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
                                            attach = ToolHtml.GetBaseAttach(link, tag.LinkText, info.Id, "SiteManage\\Files\\Attach\\");
                                          
                                            if (attach != null)
                                                ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");

                                        }
                                        catch { continue; }
                                    }
                                }
                            }
                        }
                    }
                    if (!crawlAll && sqlCount >= this.MaxCount) return null;
                }
            }
            return list;
        }
    }
}
