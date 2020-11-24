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
    public class BidSituationSzJyzxKbqk : WebSiteCrawller
    {
        public BidSituationSzJyzxKbqk()
            : base()
        {
            this.Group = "开标定标";
            this.Title = "深圳市交易中心开标情况(2015版)";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市交易中心开标情况(2015版)";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw/queryKBJiLuList.do?page=1&rows=";
            this.MaxCount = 150;
            this.ExistsUpdate = true;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidSituation>();
            int sqlCount = 0;
            string html = string.Empty;

            try
            {
                html = this.ToolWebSite.GetHtmlByUrl("https://www.szjsjy.com.cn:8001/jyw/queryKBJiLuList.do?page=1&rows=" + this.MaxCount);
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
                    string code = string.Empty, prjName = string.Empty, PublicityEndDate = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, ctx = string.Empty, HtmlTxt = string.Empty, beginDate = string.Empty;
                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;

                    code = Convert.ToString(dic["bdBH"]);
                    prjName = Convert.ToString(dic["bdName"]);
                   // if (!prjName.Contains("2016年交通拥堵综合治理（玉昌东路、炮台路、楼明路）项目施工")) continue;
                    beginDate = Convert.ToString(dic["faBuTime2"]);
                    InfoUrl = "https://www.szjsjy.com.cn:8001/jyw/queryOldDataDetail.do?type=5&id=" + code;// Convert.ToString(dic["bdGuid"]);
                    bool isJson = false;
                    string attachFileGroupGuid = string.Empty;
                    string rwfileGuid = string.Empty;
                    try
                    {
                        HtmlTxt = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                        if (string.IsNullOrEmpty(HtmlTxt))
                        {
                            string kbJiLuGuid = dic["kbJiLuGuid"].ToString();
                            string ggGuid = string.Empty;
                            string bdGuid = dic["bdGuid"].ToString();
                            string url = "https://www.szjsjy.com.cn:8001/jyw/querykbJiLuDetail.do?kbJiLuGuid=" + kbJiLuGuid + "&ggGuid=" + ggGuid + "&bdGuid=" + bdGuid;
                            isJson = true;
                            HtmlTxt = this.ToolWebSite.GetHtmlByUrl(url);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(prjName);
                        Logger.Error(ex);
                        continue;
                    }
                    if (!isJson)
                    {
                        ctx = HtmlTxt.ToCtxString();
                    }
                    else
                    {
                        try
                        {
                            JavaScriptSerializer newSerializer = new JavaScriptSerializer();
                            Dictionary<string, object> newTypeJson = (Dictionary<string, object>)newSerializer.DeserializeObject(HtmlTxt);
                            Dictionary<string, object> kbJiLu = newTypeJson["kbJiLu"] as Dictionary<string, object>;
                            Dictionary<string, object> ggBD = kbJiLu["ggBD"] as Dictionary<string, object>;
                            Dictionary<string, object> bd = ggBD["bd"] as Dictionary<string, object>;
                            attachFileGroupGuid = Convert.ToString(kbJiLu["attachFileGroupGuid"]);
                             rwfileGuid = Convert.ToString(kbJiLu["rwfileGuid"]);
                            string bdBH = Convert.ToString(bd["bdBH"]);
                            string bdName = Convert.ToString(bd["bdName"]);
                            string gcLeiXing = Convert.ToString(bd["gcLeiXing"]);
                            //string jieZhiTime = Convert.ToString(bd["jieZhiTime"]);
                            ctx = "标段编号：" + bdBH + "\r\n" + "标段名称：" + bdName;
                            HtmlTxt = "标段编号：" + bdBH + "</br>" + "标段名称：" + bdName;

                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                    msgType = "深圳市建设工程交易中心";
                    string saveUrl = Convert.ToString(dic["detailUrl"]);
                    BidSituation info = ToolDb.GetBidSituation("广东省", "深圳市工程", "", code, prjName, PublicityEndDate, msgType, saveUrl, ctx, HtmlTxt, beginDate);
                    sqlCount++;
                    if (!crawlAll && sqlCount >= this.MaxCount) return list;
                    if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                    {
                        if (this.ExistsUpdate)
                        {
                            object id = ToolDb.ExecuteScalar(string.Format("select Id from BidSituation where InfoUrl='{0}'", info.InfoUrl));
                            if (id != null)
                            {
                                string sql = string.Format("delete from BaseAttach where SourceID='{0}'", id);
                                ToolDb.ExecuteSql(sql);
                            }
                        }
                        if (isJson)
                        {

                            BaseAttach attach = null;
                            BaseAttach rwattach = null;

                            try
                            {
                                string url = "https://www.szjsjy.com.cn:8001/jyw/filegroup/queryByGroupGuidZS.do?groupGuid=" + attachFileGroupGuid;
                                string attachHtml = this.ToolWebSite.GetHtmlByUrl(url);
                                JavaScriptSerializer newSerializer = new JavaScriptSerializer();
                                Dictionary<string, object> newTypeJson = (Dictionary<string, object>)newSerializer.DeserializeObject(attachHtml);
                                string attachGuid = string.Empty;
                                foreach (KeyValuePair<string, object> newObj in newTypeJson)
                                {
                                    object[] newArray = (object[])newObj.Value;
                                    foreach (object newArr in newArray)
                                    {
                                        Dictionary<string, object> newDic = (Dictionary<string, object>)newArr;
                                        try
                                        {
                                            attachGuid = Convert.ToString(newDic["attachGuid"]);
                                            break;
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                }
                                string newUrl = "https://www.szjsjy.com.cn:8001/file/downloadFile?fileId=" + attachGuid;
                                //https://www.szjsjy.com.cn:8001/file/downloadFile?fileId=191b3e99-9ca8-4e7e-8939-87b322f5e094


                                attach = ToolHtml.GetBaseAttach(newUrl, "开标情况一览表.pdf", info.Id, "SiteManage\\Files\\Attach\\");
                                if (attach == null) attach = ToolHtml.GetBaseAttach(newUrl, "开标情况一览表.pdf", info.Id, "SiteManage\\Files\\Attach\\");
                            }
                            catch { }
                            if (attach != null)
                                ToolDb.SaveEntity(attach, string.Empty);

                            if (rwfileGuid != "")
                            {
                                try
                                {
                                    string url = "https://www.szjsjy.com.cn:8001/jyw/filegroup/queryByGroupGuidZS.do?groupGuid=" + rwfileGuid;
                                    string rwattachHtml = this.ToolWebSite.GetHtmlByUrl(url);
                                    JavaScriptSerializer rwnewSerializer = new JavaScriptSerializer();
                                    Dictionary<string, object> rwnewTypeJson = (Dictionary<string, object>)rwnewSerializer.DeserializeObject(rwattachHtml);
                                    string rwattachGuid = string.Empty;
                                    foreach (KeyValuePair<string, object> newObj in rwnewTypeJson)
                                    {
                                        object[] rwnewArray = (object[])newObj.Value;
                                        foreach (object newArr in rwnewArray)
                                        {
                                            Dictionary<string, object> rwnewDic = (Dictionary<string, object>)newArr;
                                            try
                                            {
                                                rwattachGuid = Convert.ToString(rwnewDic["attachGuid"]);
                                                break;
                                            }
                                            catch
                                            {
                                                continue;
                                            }
                                        }
                                        break;
                                    }
                                    string newUrl = "https://www.szjsjy.com.cn:8001/file/downloadFile?fileId=" + rwattachGuid;
                                    //https://www.szjsjy.com.cn:8001/file/downloadFile?fileId=191b3e99-9ca8-4e7e-8939-87b322f5e094


                                    rwattach = ToolHtml.GetBaseAttach(newUrl, "入围结果表.pdf", info.Id, "SiteManage\\Files\\Attach\\");
                                    if (rwattach == null) rwattach = ToolHtml.GetBaseAttach(newUrl, "入围结果表.pdf", info.Id, "SiteManage\\Files\\Attach\\");
                                }
                                catch { }
                                if (rwattach != null)
                                    ToolDb.SaveEntity(rwattach, string.Empty);
                            }
                        }
                        else
                        {
                            Parser parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int d = 0; d < aNode.Count; d++)
                                {
                                    ATag aTag = aNode[0] as ATag;
                                    if (!aTag.IsAtagAttach()) continue;

                                    string url = string.Empty;
                                    if (!aTag.Link.ToLower().Contains("http"))
                                        url = "http://www.szjsjy.com.cn/" + aTag.Link.GetReplace("../,\\");
                                    else
                                        url = aTag.Link.GetReplace("\\");
                                    BaseAttach attach = null;
                                    try
                                    {
                                        attach = ToolHtml.GetBaseAttach(url, aTag.LinkText, info.Id, "SiteManage\\Files\\Attach\\");
                                        if (attach == null) attach = ToolHtml.GetBaseAttach(url, aTag.LinkText, info.Id, "SiteManage\\Files\\Attach\\");
                                    }
                                    catch { }
                                    if (attach != null)
                                        ToolDb.SaveEntity(attach, string.Empty);
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
