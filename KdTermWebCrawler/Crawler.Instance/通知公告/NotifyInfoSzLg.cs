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
    public class NotifyInfoSzLg : WebSiteCrawller
    {
        public NotifyInfoSzLg() : base()
        {
            this.Group = "通知公告";
            this.PlanTime = "12:00,03:20";
            this.Title = "深圳市龙岗区交易中心通知公告";
            this.MaxCount = 20;
            this.Description = "自动抓取深圳市龙岗区交易中心通知公告";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://jyzx.cb.gov.cn/tongzhigonggao/queryTongZhiGongGaoPagination.do?page=1&rows=";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            int sqlCount = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
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
                if (obj.Key != "rows") continue;
                object[] array = (object[])obj.Value;
                foreach (object arrValue in array)
                {

                    string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    headName = Convert.ToString(dic["title"]);
                    releaseTime = Convert.ToString(dic["faBuStartDate"]);
                    infoUrl = "http://jyzx.cb.gov.cn/tongzhigonggao/tzgg_view.html?guid=" + Convert.ToString(dic["tongZhiGuid"]);

                    string attachId = Convert.ToString(dic["attachFileGroupGuid"]);
                    string attachUrl = "http://jyzx.cb.gov.cn/common/nofilter/queryFuJianList.do?groupGuid=" + attachId;

                    ctxHtml = Convert.ToString(dic["content"]);
                    infoCtx = ctxHtml.ToCtxString();
                    infoType = "通知公告";
                    msgType = "深圳市建设工程交易中心龙岗分中心";

                    NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳龙岗区工程", "龙岗区", infoCtx, infoType);
                    sqlCount++;
                    if (!crawlAll && sqlCount > this.MaxCount) return null;

                    if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                    {
                        if(!string.IsNullOrWhiteSpace(attachId))
                        {
                            string attchJson = this.ToolWebSite.GetHtmlByUrl(attachUrl);
                            JavaScriptSerializer attachSerializer = new JavaScriptSerializer();
                            object[] dicTypeJson = (object[])attachSerializer.DeserializeObject(attchJson);
                            foreach(object objJson in dicTypeJson)
                            {
                                Dictionary<string, object> dicFiles = objJson as Dictionary<string, object>;
                                string attachName = Convert.ToString(dicFiles["attachName"]);
                                string attachDelId = Convert.ToString(dicFiles["attachGuid"]);
                                string attachDelUrl = "http://jyzx.cb.gov.cn/longgang-jyw-file/downloadFile?fileId="+ attachDelId;

                                BaseAttach attach = null;
                                try
                                {
                                    attach = ToolHtml.GetBaseAttach(attachDelUrl, attachName, info.Id);
                                }
                                catch { }

                                if(attach!=null)
                                {
                                    ToolDb.SaveEntity(attach, "AttachServerPath,SourceID");
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
