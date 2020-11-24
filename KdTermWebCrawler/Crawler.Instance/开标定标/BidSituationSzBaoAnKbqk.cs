using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;
using Crawler.Base.KdService;
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
    public class BidSituationSzBaoAnKbqk : WebSiteCrawller
    {
        public BidSituationSzBaoAnKbqk()
            : base()
        {
            this.Group = "开标定标";
            this.Title = "深圳市宝安区交易中心开标情况";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市宝安区交易中心开标情况";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryKBJiLuList.do?page=1&rows=";
            this.MaxCount = 100;
            this.ExistsUpdate = true;
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
            object[] objvalues = smsTypeJson["rows"] as object[];
            foreach (object objValue in objvalues)
            {
                Dictionary<string, object> dic = (Dictionary<string, object>)objValue;
                string code = string.Empty, prjName = string.Empty, PublicityEndDate = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, ctx = string.Empty, HtmlTxt = string.Empty, beginDate = string.Empty;
                code = Convert.ToString(dic["bdBH"]);
                prjName = Convert.ToString(dic["bdName"]);
                beginDate = Convert.ToString(dic["faBuTime2"]);
                string idt = Convert.ToString(dic["bdGuid"]);
                InfoUrl = Convert.ToString(dic["detailUrl"]);
                string attachJson = string.Empty;
                try
                {
                    string urll = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryOldOTDataDetail.do?type=5&id=" + idt;
                    HtmlTxt = this.ToolWebSite.GetHtmlByUrl(urll).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                    if (string.IsNullOrWhiteSpace(HtmlTxt))
                    {
                        string kdGuid = Convert.ToString(dic["kbJiLuGuid"]);
                        InfoUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/kbJiLu_View.do?kbJiLuGuid=" + kdGuid;
                        HtmlTxt = this.ToolWebSite.GetHtmlByUrl(InfoUrl);
                        string url = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/querykbJiLuDetail.do?ggGuid=&bdGuid=&kbJiLuGuid=" + kdGuid;
                        attachJson = this.ToolWebSite.GetHtmlByUrl(url);
                    }
                }
                catch (Exception ex) { continue; }

                string gcBh = string.Empty, gcName = string.Empty, gcLeixing = string.Empty,
                        jywTime = string.Empty, kbjiGuid = string.Empty, surl = string.Empty,
                        attachId = string.Empty, attachFileGroupGuid = string.Empty;

                if (!string.IsNullOrWhiteSpace(attachJson))
                {
                    JavaScriptSerializer newSerializer = new JavaScriptSerializer();
                    Dictionary<string, object> newTypeJson = (Dictionary<string, object>)newSerializer.DeserializeObject(attachJson);
                    Dictionary<string, object> kdInfo = (Dictionary<string, object>)newTypeJson["kbJiLu"];

                    try
                    {
                        attachId = Convert.ToString(kdInfo["kbJiLuGuid"]);
                        attachFileGroupGuid = Convert.ToString(kdInfo["attachFileGroupGuid"]);
                    }
                    catch { }
                    gcLeixing = Convert.ToString(kdInfo["gcLeiXing"]);
                    jywTime = Convert.ToString(kdInfo["jywFaBuEndTime"]);
                    //https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/kbJiLu_View.do?kbJiLuGuid=9cb75eb8-66b6-441c-9686-471dfa357ff5
                    surl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/kbJiLu_View.do?kbJiLuGuid=" + attachFileGroupGuid;
                    attachJson = this.ToolWebSite.GetHtmlByUrl(surl);

                    HtmlTxt = attachJson;
                    Parser parserNew = new Parser(new Lexer(HtmlTxt));
                    NodeList tableNode = parserNew.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "de_tab1")));
                    if (tableNode != null && tableNode.Count > 0)
                    {
                        HtmlTxt = tableNode.AsHtml();
                        HtmlTxt = HtmlTxt.GetReplace("<td id=\"bdBH\">&nbsp;</td>", "<td id=\"bdBH\">&nbsp;" + code + "</td>");
                        HtmlTxt = HtmlTxt.GetReplace("<td id=\"bdName\">&nbsp;</td>", "<td  id=\"bdName\">&nbsp;" + prjName + "</td>");
                        HtmlTxt = HtmlTxt.GetReplace("<td id=\"gcLeiXing\">&nbsp;</td>", "<td id=\"gcLeiXing\">&nbsp;" + gcLeixing + "</td>");
                        HtmlTxt = HtmlTxt.GetReplace("<td id=\"jieZhiTime\">&nbsp;</td>", "<td id=\"jieZhiTime\">&nbsp;" + jywTime + "</td>");
                        ctx = HtmlTxt.Replace("</tr>", "\r\n").ToCtxString();


                    }
                }
                ctx = HtmlTxt.ToCtxString();
                string saveUrl = Convert.ToString(dic["detailUrl"]);
                msgType = "深圳市建设工程交易中心宝安分中心";
                BidSituation info = ToolDb.GetBidSituation("广东省", "深圳宝安区工程", "宝安区", code, prjName, PublicityEndDate, msgType, InfoUrl, ctx, HtmlTxt, beginDate);
                sqlCount++;
                if (!crawlAll && sqlCount >= this.MaxCount) return list;
                if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                {
                    if (!string.IsNullOrWhiteSpace(attachFileGroupGuid))
                    {
                        string moJson = string.Empty;
                        string sUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/filegroup/queryByGroupGuidZS.do?groupGuid=" + attachFileGroupGuid;
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

                                try
                                {
                                    BaseAttach attach = null;
                                    string link = string.Empty;
                                    if (tag.Link.ToLower().Contains("http"))
                                    {
                                        link = tag.Link;
                                        if (link.Contains("\\"))
                                            link = link.Replace("\\", "");
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
            return list;
        }
    }
}
