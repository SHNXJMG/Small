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
    public class SzBaoAnExpert : WebSiteCrawller
    {
        public SzBaoAnExpert()
            : base()
        {
            this.Group = "评标结果公示";
            this.Title = "广东省深圳市宝安区专家评标结果公示";
            this.MaxCount = 200;
            this.Description = "自动抓取广东省深圳市宝安区专家评标结果公示";
            this.PlanTime = "4:30";
            this.ExistCompareFields = "PrjNo,PrjName,ExpertEndTime,BidResultEndTime";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryPWList.do?page=1&rows=";
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
                string bProv = string.Empty, bCity = string.Empty, bArea = string.Empty, bPrjno = string.Empty, bPrjname = string.Empty,
                                bExpertendtime = string.Empty, bBidresultendtime = string.Empty, bBaseprice = string.Empty, bBiddate = string.Empty,
                                bBuildunit = string.Empty, bBidmethod = string.Empty,
                             bRemark = string.Empty, bInfourl = string.Empty;
                bPrjno = Convert.ToString(dic["bdBH"]);
                bPrjname = Convert.ToString(dic["bdName"]);
                string htmldtl = string.Empty;
                bInfourl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryOldOTDataDetail.do?type=6&id=" + bPrjno;
                try
                {
                    htmldtl = this.ToolWebSite.GetHtmlByUrl(bInfourl).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                }
                catch { continue; }
                BidProject info = ToolDb.GenExpertProject("广东省", "深圳市", "宝安区", bPrjno, bPrjname, bExpertendtime, bBaseprice, bBiddate, bBuildunit, bBidmethod, bRemark, bInfourl);
                string sql = string.Format("select Id from BidProject where 1=1 and PrjNo='{0}' and PrjName='{1}'", info.PrjNo, info.PrjName);
                string result = Convert.ToString(ToolDb.ExecuteScalar(sql));
                if (!string.IsNullOrEmpty(result))
                {
                        SaveExpert(result, bInfourl, htmldtl, true);
                }
                else
                {
                    if (ToolDb.SaveEntity(info, this.ExistCompareFields,this.ExistsUpdate))
                            SaveExpert(info.Id, bInfourl, htmldtl, false);
                }
            }
                return list;
        }
        private void SaveExpert(string guid, string url, string htmltxt, bool isUpdate)
        {
            List<BidProjectExpert> list = new List<BidProjectExpert>();
            Parser parser = new Parser(new Lexer(htmltxt));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
            if (pageNode != null && pageNode.Count > 1)
            {
                TableTag table = pageNode[1] as TableTag;
                for (int i = 1; i < table.RowCount; i++)
                {
                    string bBidProjectId = string.Empty, bExpertname = string.Empty, bBidtype = string.Empty, bExpertspec = string.Empty,
                   bExpertunit = string.Empty,
                   bRemark = string.Empty, bInfourl = string.Empty, bCreator = string.Empty, bCreatetime = string.Empty, bLastmodifier = string.Empty,
                   bLastmodifytime = string.Empty;
                    TableRow tr = table.Rows[i];
                    bExpertname = tr.Columns[0].ToNodePlainString();
                    bBidtype = tr.Columns[2].ToPlainTextString();
                    bExpertspec = tr.Columns[3].ToPlainTextString();
                    try
                    {
                        bExpertunit = tr.Columns[4].ToPlainTextString();
                    }
                    catch
                    { }
                    BidProjectExpert info = ToolDb.GenProjectExpert(guid, bExpertname, bBidtype, bExpertspec, bExpertunit, string.Empty, url);
                    list.Add(info);
                }
            }
            if (list.Count > 0)
            {
                if (isUpdate)
                {
                    string delSql = string.Format("delete from BidProjectExpert where BidProjectId='{0}'", guid);
                    string result = Convert.ToString(ToolDb.ExecuteScalar(delSql));
                    if (string.IsNullOrEmpty(result))
                        ToolDb.SaveDatas(list, "");
                }
                else
                    ToolDb.SaveDatas(list, "");
            }
        }
    }
}
