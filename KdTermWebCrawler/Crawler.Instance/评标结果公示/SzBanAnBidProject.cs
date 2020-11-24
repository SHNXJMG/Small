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
using System.Data;
using System.Data.SqlClient;


namespace Crawler.Instance
{
    public class SzBanAnBidProject : WebSiteCrawller
    {
        public SzBanAnBidProject()
            : base()
        {
            this.Group = "评标结果公示";
            this.Title = "广东省深圳市宝安区评标结果公示";
            this.Description = "自动抓取广东省深圳市建设工程评标结果公示";
            this.PlanTime = "04:50";
            this.ExistCompareFields = "PrjNo,PrjName,ExpertEndTime,BidResultEndTime";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryPBJieGuoList.do?page=1&rows=";
            this.MaxCount = 200;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            int sqlCount = 0;
            string viewState = string.Empty;
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
                string bProv = string.Empty, bCity = string.Empty, bArea = string.Empty, bPrjno = string.Empty,
                            bPrjname = string.Empty, bBidresultendtime = string.Empty,
                            bBaseprice = string.Empty, bBiddate = string.Empty, bBuildunit = string.Empty, bBidmethod = string.Empty,
                             bRemark = string.Empty, bInfourl = string.Empty;

                bPrjno = Convert.ToString(dic["bdBH"]);
                bPrjname = Convert.ToString(dic["bdName"]);
                string gulId = Convert.ToString(dic["guid"]);
                bInfourl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryOldOTDataDetail.do?type=7&id=" + gulId;
                string htmldtl = string.Empty;
                htmldtl = this.ToolWebSite.GetHtmlByUrl(bInfourl);
                BidProject info = ToolDb.GenResultProject("广东省", "深圳市", "宝安区", bPrjno, bPrjname, bBidresultendtime, bBaseprice, bBiddate, bBuildunit, bBidmethod, bRemark, bInfourl);
                string sql = string.Format("select Id from BidProject where 1=1 and PrjNo='{0}' and PrjName='{1}'", info.PrjNo, info.PrjName);
                string result = Convert.ToString(ToolDb.ExecuteScalar(sql));
                if (!string.IsNullOrEmpty(result))
                {
                    SaveAttach(info, htmldtl, result, true);
                }
                else
                {
                    if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                        SaveAttach(info, htmldtl, result, false);
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

                        aurl = aTag.Link.GetReplace("\\\"", "");
                        if (string.IsNullOrWhiteSpace(attachName))
                            attachName = info.PrjName;
                        try
                        {
                            string url = System.Web.HttpUtility.UrlDecode(aurl);
                            string[] urls = url.Split('&');

                            BaseAttach entity = null;
                            if (isUpdate)
                                entity = ToolHtml.GetBaseAttach(url, attachName, result, "SiteManage\\Files\\Attach\\");
                            else
                                entity = ToolHtml.GetBaseAttach(url, attachName, info.Id, "SiteManage\\Files\\Attach\\");
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
