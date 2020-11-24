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
using System.Data;

namespace Crawler.Instance
{
    public class BidExpertSZ : WebSiteCrawller
    {
        public BidExpertSZ()
            : base()
        {
            this.Group = "评标结果公示";
            this.Title = "广东省深圳市建设工程专家评标工程";
            this.Description = "自动抓取广东省深圳市建设工程专家评标工程";
            this.PlanTime = "04:02";
            this.ExistCompareFields = "PrjNo,PrjName,ExpertEndTime,BidResultEndTime";
            this.SiteUrl = "http://www.szjsjy.com.cn/BusinessInfo/PWMDGSList.aspx";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(htl));
            NodeList dList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("cellSpacing", "2"), new TagNameFilter("table")));
            if (dList != null && dList.Count > 0)
            {
                string pageString = dList.AsString();
                Regex regexPage = new Regex(@"共[^页]+页，");
                Match pageMatch = regexPage.Match(pageString);
                try
                {
                    pageInt = int.Parse(pageMatch.Value.Replace("共", "").Replace("页，", "").Replace(" ", ""));
                }
                catch { }
            }
            for (int j = 1; j <= pageInt; j++)
            { 
                if (j > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(htl);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { 
                            "__EVENTTARGET",  
                            "__EVENTARGUMENT", 
                            "__VIEWSTATE",
                            "__VIEWSTATEENCRYPTED",
                            "__EVENTVALIDATION",
                            "ctl00$Header$drpSearchType",
                            "ctl00$Header$txtGcxm", 
                            "ctl00$Content$hdnId",
                            "ctl00$Content$hdnOperate",
                            "ctl00$hdnPageCount"
	
                        }, new string[] { 
                            "ctl00$Content$GridView1",
                            "Page$"+j.ToString(),
                            viewState,
                            "",
                            eventValidation,
                            "0",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            pageInt.ToString()
                        });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { return list; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_Content_GridView1"), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    if (pageInt > 1)
                    {
                        for (int i = 1; i < table.RowCount-1; i++)
                        {
                            string bProv = string.Empty, bCity = string.Empty, bArea = string.Empty, bPrjno = string.Empty, bPrjname = string.Empty,
                                bExpertendtime = string.Empty, bBidresultendtime = string.Empty, bBaseprice = string.Empty, bBiddate = string.Empty, 
                                bBuildunit = string.Empty, bBidmethod = string.Empty,
                             bRemark = string.Empty, bInfourl = string.Empty;
                            TableRow tr = table.Rows[i];
                            bPrjno = tr.Columns[1].ToPlainTextString().Trim();
                            bPrjname = tr.Columns[2].ToPlainTextString().Trim();
                            bExpertendtime = tr.Columns[3].ToPlainTextString().Trim();
                            bInfourl = SiteUrl;
                            ATag aTag = table.SearchFor(typeof(ATag), true)[i - 1] as ATag;
                            string aLink = aTag.Link.Replace("viewPwmd('", "").Replace("')", "");
                            try
                            {
                                string[] link = aLink.Split(',');
                                if (link.Length > 1)
                                {
                                    
                                    string a1 = link[0].Replace(" ", "").Replace("'", "").Replace("'", "");
                                    string a2 = link[1].Replace(" ", "").Replace("'", "").Replace("'", "");  
                                    byte[] byStr = System.Text.Encoding.UTF8.GetBytes(a2); //默认是System.Text.Encoding.Default.GetBytes(str)
                                    StringBuilder sb = new StringBuilder();
                                    for (int d = 0; d < byStr.Length; d++)
                                    {
                                        sb.Append(@"%" + Convert.ToString(byStr[d], 16));
                                    }
                                    aLink = a1 + "&gcmc=" + sb.ToString();
                                } 
                            }
                            catch { }
                            BidProject info = ToolDb.GenExpertProject("广东省", "深圳市", "", bPrjno, bPrjname, bExpertendtime, bBaseprice, bBiddate, bBuildunit, bBidmethod, bRemark, bInfourl);
                            string sql = string.Format("select * from BidProject where 1=1 and PrjNo='{0}' and PrjName='{1}'", info.PrjNo, info.PrjName);
                            DataTable dt = ToolDb.GetDbData(sql);
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                sql = string.Format("select * from BidProject where 1=1 and PrjNo='{0}' and PrjName='{1}' and ExpertEndTime is null", info.PrjNo, info.PrjName);
                                DataTable dtinfo = ToolDb.GetDbData(sql);
                                if (dtinfo != null && dtinfo.Rows.Count > 0)
                                {
                                    string id = Convert.ToString(dtinfo.Rows[0]["Id"]);
                                    string strSql = string.Format("update BidProject set ExpertEndTime='{0}' where Id='{1}'", info.ExpertEndTime, id);
                                    int result = ToolDb.ExecuteSql(strSql);
                                    if (result > 0)
                                    {
                                        string url = "http://www.szjsjy.com.cn/BusinessInfo/PWMDGSViewForm.aspx?GCBH=" + aLink;
                                        string htmldetail = string.Empty;
                                        try
                                        {
                                            htmldetail = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
                                        }
                                        catch { }
                                        if (!string.IsNullOrEmpty(htmldetail))
                                        {
                                            AddExpert(htmldetail, url, id);
                                        }
                                    }
                                } 
                            }
                            else
                            {
                                ToolDb.SaveEntity(info, "");
                                string url = "http://www.szjsjy.com.cn/BusinessInfo/PWMDGSViewForm.aspx?GCBH=" + aLink;
                                string htmldetail = string.Empty;
                                try
                                {
                                    htmldetail = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
                                }
                                catch { }
                                if (!string.IsNullOrEmpty(htmldetail))
                                {
                                    AddExpert(htmldetail, url, info.Id);
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                    else
                    {
                        for (int i = 1; i < table.RowCount; i++)
                        {
                            string bProv = string.Empty, bCity = string.Empty, bArea = string.Empty, bPrjno = string.Empty, bPrjname = string.Empty, bExpertendtime = string.Empty, bBidresultendtime = string.Empty, bBaseprice = string.Empty, bBiddate = string.Empty, bBuildunit = string.Empty, bBidmethod = string.Empty,
                             bRemark = string.Empty, bInfourl = string.Empty;
                            TableRow tr = table.Rows[i];
                            bPrjno = tr.Columns[1].ToPlainTextString().Trim();
                            bPrjname = tr.Columns[2].ToPlainTextString().Trim();
                            bExpertendtime = tr.Columns[3].ToPlainTextString().Trim();
                            bInfourl = SiteUrl;
                            ATag aTag = table.SearchFor(typeof(ATag), true)[i - 1] as ATag;
                            string aLink = aTag.Link.Replace("viewPwmd('", "").Replace("')", "");
                            try
                            {
                                string[] link = aLink.Split(',');
                                if (link.Length > 1)
                                {

                                    string a1 = link[0].Replace(" ", "").Replace("'", "").Replace("'", "");
                                    string a2 = link[1].Replace(" ", "").Replace("'", "").Replace("'", "");
                                    byte[] byStr = System.Text.Encoding.UTF8.GetBytes(a2); //默认是System.Text.Encoding.Default.GetBytes(str)
                                    StringBuilder sb = new StringBuilder();
                                    for (int d = 0; d < byStr.Length; d++)
                                    {
                                        sb.Append(@"%" + Convert.ToString(byStr[d], 16));
                                    }
                                    aLink = a1 + "&gcmc=" + sb.ToString();
                                }
                            }
                            catch { }
                            BidProject info = ToolDb.GenExpertProject("广东省", "深圳市", "", bPrjno, bPrjname, bExpertendtime, bBaseprice, bBiddate, bBuildunit, bBidmethod, bRemark, bInfourl);
                            string sql = string.Format("select * from BidProject where 1=1 and PrjNo='{0}' and PrjName='{1}'", info.PrjNo, info.PrjName);
                            DataTable dt = ToolDb.GetDbData(sql);
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                sql = string.Format("select * from BidProject where 1=1 and PrjNo='{0}' and PrjName='{1}' and ExpertEndTime is null", info.PrjNo, info.PrjName);
                                DataTable dtinfo = ToolDb.GetDbData(sql);
                                if (dtinfo != null && dtinfo.Rows.Count > 0)
                                {
                                    string id = Convert.ToString(dtinfo.Rows[0]["Id"]);
                                    string strSql = string.Format("update BidProject set ExpertEndTime='{0}' where Id='{1}'", info.ExpertEndTime, id);
                                    int result = ToolDb.ExecuteSql(strSql);
                                    if (result > 0)
                                    {
                                        string url = "http://www.szjsjy.com.cn/BusinessInfo/PWMDGSViewForm.aspx?GCBH=" + aLink;
                                        string htmldetail = string.Empty;
                                        try
                                        {
                                            htmldetail = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
                                        }
                                        catch { }
                                        if (!string.IsNullOrEmpty(htmldetail))
                                        {
                                            AddExpert(htmldetail, url, id);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ToolDb.SaveEntity(info, this.ExistCompareFields);
                                string url = "http://www.szjsjy.com.cn/BusinessInfo/PWMDGSViewForm.aspx?GCBH=" + aLink;
                                string htmldetail = string.Empty;
                                try
                                {
                                    htmldetail = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
                                }
                                catch { }
                                if (!string.IsNullOrEmpty(htmldetail))
                                {
                                    AddExpert(htmldetail, url, info.Id);
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }

        public void AddExpert(string html, string infourl, string id)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodelist = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_GridView1"), new TagNameFilter("table")));
            if (nodelist != null && nodelist.Count > 0)
            {
                string bBidProjectId = string.Empty, bExpertname = string.Empty, bBidtype = string.Empty, bExpertspec = string.Empty,
                bExpertunit = string.Empty,
                bRemark = string.Empty, bInfourl = string.Empty, bCreator = string.Empty, bCreatetime = string.Empty, bLastmodifier = string.Empty,
                bLastmodifytime = string.Empty;
                TableTag table = nodelist[0] as TableTag;
                for (int i = 1; i < table.RowCount; i++)
                {
                    TableRow tr = table.Rows[i];
                    bExpertname = tr.Columns[0].ToPlainTextString();
                    bBidtype = tr.Columns[1].ToPlainTextString();
                    bExpertspec = tr.Columns[2].ToPlainTextString();
                    bExpertunit = tr.Columns[3].ToPlainTextString();
                    BidProjectExpert info = ToolDb.GenProjectExpert(id,bExpertname,bBidtype,bExpertspec,bExpertunit,string.Empty,infourl);
                    ToolDb.SaveEntity(info, "");
                }
            }
        }
    }
}
