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
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Threading;


namespace Crawler.Instance
{
    public class NotifyInfoJsBaoAn : WebSiteCrawller
    {
        public NotifyInfoJsBaoAn()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省深圳市建设工程交易中心宝安分中心通知公告";
            this.Description = "自动抓取广东省深圳市建设工程交易中心宝安分中心通知公告";
            this.PlanTime = "21:40";
            this.SiteUrl = "http://www.bajsjy.com/tongzhigonggao/tzgg.html";
            this.MaxCount = 555;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            //取得页码
            string newUrl = "http://www.bajsjy.com/tongzhigonggao/queryTongZhiGongGaoPagination.do?page=1&rows=10000&title=null&type=0";
            IList list = new List<NotifyInfo>();
            int sqlCount = 0;
            string html = string.Empty;


            JavaScriptSerializer serializer = new JavaScriptSerializer();
            html = this.ToolWebSite.GetHtmlByUrl(newUrl);
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
            object[] objvalues = smsTypeJson["rows"] as object[];

            foreach (object objValue in objvalues)
            {
                string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty,
                    infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                Dictionary<string, object> dic = (Dictionary<string, object>)objValue;
                headName = Convert.ToString(dic["title"]);
                releaseTime = Convert.ToString(dic["faBuStartDate"]);
                string tongZhiGuid = string.Empty;
                infoType = "通知公告";
                tongZhiGuid = Convert.ToString(dic["tongZhiGuid"]);
                string attachFileGroupGuid = string.Empty;
                attachFileGroupGuid = Convert.ToString(dic["attachFileGroupGuid"]);
                infoUrl = "http://www.bajsjy.com/tongzhigonggao/tzgg_view1.html?guid=" + tongZhiGuid;
                string htmldeil = string.Empty;
                string tzUrl = "http://www.bajsjy.com/common/nofilter/queryFuJianList.do?groupGuid=" + attachFileGroupGuid;
                string htmldtl = string.Empty;
                try
                {
                    htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.UTF8).GetJsString();
                }
                catch
                { }
                Parser parser = new Parser(new Lexer(htmldtl));
                NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page_contect bai_bg")));
                if (dtlNode != null && dtlNode.Count > 0)
                {
                    ctxHtml = dtlNode.AsHtml();
                    infoCtx = ctxHtml.ToCtxString();
                }


                try
                {
                    htmldeil = this.ToolWebSite.GetHtmlByUrl(tzUrl, Encoding.UTF8);
                    htmldeil = htmldeil.Replace("[", "").Replace("]", "");
                }
                catch { continue; }

                NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳宝安区工程", string.Empty, infoCtx, infoType);
                sqlCount++;
                if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                {
                    if (!string.IsNullOrWhiteSpace(attachFileGroupGuid))
                    {
                        string moJson = string.Empty;
                        try
                        {
                            moJson = this.ToolWebSite.GetHtmlByUrl(tzUrl);
                        }
                        catch { }
                        if (!string.IsNullOrWhiteSpace(moJson))
                        {
                            try
                            {

                                JavaScriptSerializer newSerializers = new JavaScriptSerializer();
                                Dictionary<string, object> newTypeJsons = (Dictionary<string, object>)newSerializers.DeserializeObject(htmldeil);
                                Dictionary<string, object> mofo = (Dictionary<string, object>)newTypeJsons;

                                foreach (object objAttach in mofo)
                                {
                                    Dictionary<string, object> attachs = (Dictionary<string, object>)mofo;
                                    string attachguid = Convert.ToString(attachs["attachGuid"]);
                                    string attachName = Convert.ToString(attachs["attachName"]);
                                    string link = "http://www.bajsjy.com/baoan-jyw-file/downloadFile?fileId=" + attachguid;
                                    BaseAttach attach = ToolHtml.GetBaseAttach(link, attachName, info.Id, "SiteManage\\Files\\Attach\\");
                                    if (attach != null)
                                        ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                }
                if (!crawlAll && sqlCount >= this.MaxCount) return list;

            }
            return list;
        }
    }
}
