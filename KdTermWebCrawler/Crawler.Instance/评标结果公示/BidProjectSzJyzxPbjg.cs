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
    public class BidProjectSzJyzxPbjg : WebSiteCrawller
    {
        public BidProjectSzJyzxPbjg()
            : base()
        {
            this.Group = "评标结果公示";
            this.Title = "广东省深圳市建设工程评标结果公示(2015版)";
            this.Description = "自动抓取广东省深圳市建设工程评标结果公示(2015版)";
            this.PlanTime = "04:00";
            this.ExistCompareFields = "PrjNo,PrjName,Prov,City,Area";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw/queryPBJieGuoList.do?page=1&rows=";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidProject>();
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
                    string bProv = string.Empty, bCity = string.Empty, bArea = string.Empty, bPrjno = string.Empty,
                               bPrjname = string.Empty, bBidresultendtime = string.Empty,
                               bBaseprice = string.Empty, bBiddate = string.Empty, bBuildunit = string.Empty, bBidmethod = string.Empty, bRemark = string.Empty, bInfourl = string.Empty;

                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    bPrjno = Convert.ToString(dic["bdBH"]);
                    bPrjname = Convert.ToString(dic["bdName"]);
                    string saveUrl = Convert.ToString(dic["detailUrl"]);
                    bInfourl = "https://www.szjsjy.com.cn:8001/jyw/queryOldDataDetail.do?type=7&id=" + bPrjno;
                    string htmldtl = string.Empty;
                    try
                    {
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "guid", "ggbdguid" }, new string[] {
                        dic["guid"].ToString(),dic["tpfaGgBdGuid"].ToString()
                        });
                        htmldtl = this.ToolWebSite.GetHtmlByUrl("https://www.szjsjy.com.cn:8001/jyw/queryPBById.do", nvc);

                        JavaScriptSerializer attachSerializer = new JavaScriptSerializer();
                        Dictionary<string, object> attachJson = (Dictionary<string, object>)attachSerializer.DeserializeObject(htmldtl);
                        Dictionary<string, object> kbJiLu = attachJson["vo"] as Dictionary<string, object>;
                        string attachId = Convert.ToString(kbJiLu["attachFileGuid"]);
                        htmldtl = this.ToolWebSite.GetHtmlByUrl("https://www.szjsjy.com.cn:8001/jyw/filegroup/queryByGroupGuidZS.do?groupGuid=" + attachId).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                    }
                    catch
                    {
                        try
                        {
                            string url = "https://www.szjsjy.com.cn:8001/jyw/queryOldDataDetail.do?id=" + bPrjno + "&type=7";
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(url);
                        }
                        catch { }
                    }
                    BidProject info = ToolDb.GenResultProject("广东省", "深圳市", "", bPrjno, bPrjname, bBidresultendtime, bBaseprice, bBiddate, bBuildunit, bBidmethod, bRemark, saveUrl);
                    string sql = string.Format("select Id from BidProject where 1=1 and PrjNo='{0}' and PrjName='{1}'", info.PrjNo, info.PrjName);
                    string result = Convert.ToString(ToolDb.ExecuteScalar(sql));
                    if (!string.IsNullOrEmpty(result))
                    {
                        SaveAttach(info, htmldtl, result, true);
                    }
                    else
                    {
                        try
                        {

                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                                SaveAttach(info, htmldtl, result, false);
                        }
                        catch (Exception ex)
                        {

                        }
                    }


                }
            }
            return list;
        }

        private void SaveAttach(BidProject info, string htmltxt, string result, bool isUpdate)
        {
            List<BaseAttach> list = new List<BaseAttach>();

            if (htmltxt.Contains("http"))
            {
                Parser parser = new Parser(new Lexer(htmltxt));
                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                if (aNode != null && aNode.Count > 0)
                {
                    for (int j = 0; j < aNode.Count; j++)
                    {
                        ATag aTag = aNode[j].GetATag();
                        string attachName = aTag.LinkText;
                        string aurl = string.Empty;
                        if (!aTag.Link.ToLower().Contains("http"))
                            aurl = "https://www.szjsjy.com.cn:8001/" + aTag.Link.GetReplace("\\");
                        else
                            aurl = aTag.Link.GetReplace("\\");
                        if (string.IsNullOrWhiteSpace(attachName))
                            attachName = info.PrjName;
                        try
                        {
                            string url = System.Web.HttpUtility.UrlDecode(aurl);
                            string[] urls = url.Split('&');
                            url = urls[0] + "&" + urls[2] + "&" + urls[1];
                            BaseAttach entity = null;
                            if (isUpdate)
                                entity = ToolHtml.GetBaseAttach(url.Replace("\"", ""), attachName, result, "SiteManage\\Files\\Attach\\");
                            else
                                entity = ToolHtml.GetBaseAttach(url.Replace("\"", ""), attachName, info.Id, "SiteManage\\Files\\Attach\\");
                            if (entity != null) list.Add(entity);
                        }
                        catch { }
                    }

                }
            }
            else
            {
                System.Data.DataTable dtlDtl = ToolHtml.JsonToDataTable(htmltxt);

                if (dtlDtl != null && dtlDtl.Rows.Count > 0)
                {
                    for (int i = 0; i < dtlDtl.Rows.Count; i++)
                    {
                        System.Data.DataRow row = dtlDtl.Rows[i];
                        string attachName = Convert.ToString(row["attachName"]);
                        if (string.IsNullOrWhiteSpace(attachName))
                            attachName = info.PrjName;
                        string attachGuid = Convert.ToString(row["attachGuid"]);
                        string url = "https://www.szjsjy.com.cn:8001/file/downloadFile?fileId=" + attachGuid;
                        try
                        {
                            BaseAttach entity = null;
                            if (isUpdate)
                                entity = ToolHtml.GetBaseAttachByUrl(url, attachName, result, "SiteManage\\Files\\Attach\\");
                            else
                                entity = ToolHtml.GetBaseAttachByUrl(url, attachName, info.Id, "SiteManage\\Files\\Attach\\");
                            if (entity != null) list.Add(entity);
                        }
                        catch { }
                    }
                }

            }
            if (list.Count > 0)
            {
                if (isUpdate)
                {
                    string delSql = string.Format("delete from BaseAttach where SourceID='{0}'", result);
                    ToolFile.Delete(result);
                    int count = ToolDb.ExecuteSql(delSql);
                }
                foreach (BaseAttach attach in list)
                {
                    ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                }
            }
        }

    }
}
