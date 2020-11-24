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
    public class InivteLongGangFZX : WebSiteCrawller
    {
        public InivteLongGangFZX()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "深圳市建设交易服中心龙岗分中心招标公告";
            this.Description = "自动抓取深圳市建设交易服中心龙岗分中心招标公告";
            this.PlanTime = "9:10,11:10,14:10,16:10,18:10";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 100;
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/queryGongGaoList.do?page=1&rows=";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
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
                    string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                     prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                     specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                     remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                     CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty,
                     HtmlTxt = string.Empty;

                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    code = Convert.ToString(dic["gcBH"]);
                    prjName = Convert.ToString(dic["gcName"]);
                    beginDate = Convert.ToString(dic["ggStartTime2"]);
                    string saveUrl= Convert.ToString(dic["detailUrl"]); 
                    InfoUrl = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/queryOldOTDataDetail.do?type=1&id=" + dic["gcBH"];

                    try
                    {
                        HtmlTxt = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                        
                        if (string.IsNullOrWhiteSpace(HtmlTxt))
                        {
                            string url = "https://www.szjsjy.com.cn:8001/jyw-lg/jyxx/showGongGao.do?ggGuid=" + dic["ggGuid"];
                            string htmldtl = this.ToolWebSite.GetHtmlByUrl(url);

                            JavaScriptSerializer Newserializer = new JavaScriptSerializer();
                            Dictionary<string, object> newTypeJson = (Dictionary<string, object>)Newserializer.DeserializeObject(htmldtl);
                            HtmlTxt = Convert.ToString(newTypeJson["html"]);
                        }
                    }
                    catch(Exception ex) { continue; }
                    inviteCtx = HtmlTxt.Replace("</span>", "\r\n").Replace("<br />", "\r\n").ToCtxString();

                    prjAddress = inviteCtx.GetAddressRegex();
                    buildUnit = inviteCtx.GetBuildRegex();  
                    if (string.IsNullOrEmpty(code))
                        code = inviteCtx.GetCodeRegex();
                    msgType = "深圳市建设工程交易中心龙岗分中心";
                    specType = "建设工程";
                    inviteType = ToolHtml.GetInviteTypes(prjName);
                  
                    InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳龙岗区工程", "龙岗区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, string.Empty, saveUrl, HtmlTxt);
                    
                    if (!crawlAll && sqlCount >= this.MaxCount) return null;

                    sqlCount++;
                    if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                    {
                        Parser parser = new Parser(new Lexer(HtmlTxt));
                        NodeList fileNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        if (fileNode != null && fileNode.Count > 0)
                        {
                            for (int f = 0; f < fileNode.Count; f++)
                            {
                                ATag tag = fileNode[f] as ATag;
                                if (tag.IsAtagAttach()|| tag.Link.ToLower().Contains("downloadfile"))
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
                                            link = link.GetReplace("\\","");
                                        }
                                        else
                                            link = "https://www.szjsjy.com.cn:8001/" + tag.Link;
                                        attach = ToolHtml.GetBaseAttachByUrl(link, tag.LinkText, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                        if (attach != null)
                                            ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                                    }
                                    catch { continue; }
                                }
                            }
                        }                    
                    }                
                }
            }  
            return list;
        }
    }
}
