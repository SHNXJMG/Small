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
using System.Web;
using System.Web.UI;
using System.Data;
using System.Collections.Generic;



namespace Crawler.Instance
{
    public class BidResultSZ : WebSiteCrawller
    {
        public BidResultSZ()
            : base()
        {
            this.Group = "评标结果公示";
            this.Title = "广东省深圳市建设工程评标结果公示";
            this.Description = "自动抓取广东省深圳市建设工程评标结果公示";
            this.PlanTime = "04:00";
            this.ExistCompareFields = "PrjNo,PrjName,ExpertEndTime,BidResultEndTime";
            this.SiteUrl = "http://www.szjsjy.com.cn/BusinessInfo/PBJGGSList.aspx";
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
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("cellspacing", "2"), new TagNameFilter("table")));
            if (pageList != null && pageList.Count > 0)
            {
                string pageString = pageList.AsString();
                Regex regexPage = new Regex(@"共[^页]+页，");
                Match pageMatch = regexPage.Match(pageString);
                try
                {
                    pageInt = int.Parse(pageMatch.Value.Replace("共", "").Replace("页，", "").Replace(" ", ""));
                }
                catch { pageInt = 1; }
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
                            "ctl00$Header$txtQymc", 
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
                            pageInt.ToString()
                        });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { }
                }
                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_Content_GridView1"), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    if (pageInt > 1)
                    {
                        for (int i = 1; i < table.RowCount - 1; i++)
                        {
                            string bProv = string.Empty, bCity = string.Empty, bArea = string.Empty, bPrjno = string.Empty,
                                bPrjname = string.Empty, bBidresultendtime = string.Empty,
                                bBaseprice = string.Empty, bBiddate = string.Empty, bBuildunit = string.Empty, bBidmethod = string.Empty,
                                 bRemark = string.Empty, bInfourl = string.Empty;
                            TableRow tr = table.Rows[i] as TableRow;
                            bPrjno = tr.Columns[1].ToPlainTextString();
                            bPrjname = tr.Columns[2].ToPlainTextString();
                            bBidresultendtime = tr.Columns[3].ToPlainTextString();
                            ATag aTag = table.SearchFor(typeof(ATag), true)[i - 1] as ATag;
                            string aLink = "http://www.szjsjy.com.cn/BusinessInfo/" + aTag.Link;
                            bInfourl = aLink;
                            BidProject info = ToolDb.GenResultProject("广东省", "深圳市", "", bPrjno, bPrjname, bBidresultendtime, bBaseprice, bBiddate, bBuildunit, bBidmethod, bRemark, bInfourl);

                            string sql = string.Format("select Id from BidProject where 1=1 and PrjNo='{0}' and PrjName='{1}'", info.PrjNo, info.PrjName);
                            object obj = ToolDb.ExecuteScalar(sql);
                            //判断是否存在该条记录
                            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                            {
                                sql = string.Format("select Id from BidProject where 1=1 and PrjNo='{0}' and PrjName='{1}' and BidResultEndTime is null", info.PrjNo, info.PrjName);
                                object dtinfo = ToolDb.ExecuteScalar(sql);
                                if (dtinfo != null && !string.IsNullOrEmpty(dtinfo.ToString()))
                                {
                                    string id = dtinfo.ToString();
                                    string strSql = string.Format("update BidProject set BidResultEndTime='{0}' where Id='{1}'", info.BidResultEndTime, id);
                                    int result = ToolDb.ExecuteSql(strSql);
                                    if (result > 0)
                                    {
                                        SaveAttach(aLink, id);
                                    }
                                  
                                }
                                else
                                {
                                    SaveAttach(aLink, obj.ToString());
                                }
                            }
                            else
                            {
                                ToolDb.SaveEntity(info, "");
                                SaveAttach(aLink, info.Id);
                            }

                        }
                    }
                    else
                    {
                        for (int i = 1; i < table.RowCount; i++)
                        {
                            string bProv = string.Empty, bCity = string.Empty, bArea = string.Empty, bPrjno = string.Empty,
                                bPrjname = string.Empty, bBidresultendtime = string.Empty,
                                bBaseprice = string.Empty, bBiddate = string.Empty, bBuildunit = string.Empty, bBidmethod = string.Empty,
                                 bRemark = string.Empty, bInfourl = string.Empty;
                            TableRow tr = table.Rows[i] as TableRow;
                            bPrjno = tr.Columns[1].ToPlainTextString();
                            bPrjname = tr.Columns[2].ToPlainTextString();
                            bBidresultendtime = tr.Columns[3].ToPlainTextString();
                            bInfourl = SiteUrl;
                            ATag aTag = table.SearchFor(typeof(ATag), true)[i - 1] as ATag;
                            string aLink = "http://www.szjsjy.com.cn/BusinessInfo/" + aTag.Link;
                            BidProject info = ToolDb.GenResultProject("广东省", "深圳市", "", bPrjno, bPrjname, bBidresultendtime, bBaseprice, bBiddate, bBuildunit, bBidmethod, bRemark, bInfourl);

                            string sql = string.Format("select Id from BidProject where 1=1 and PrjNo='{0}' and PrjName='{1}'", info.PrjNo, info.PrjName);
                            object obj = ToolDb.ExecuteScalar(sql);
                            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                            {
                                sql = string.Format("select Id from BidProject where 1=1 and PrjNo='{0}' and PrjName='{1}' and BidResultEndTime is null", info.PrjNo, info.PrjName);
                                object dtinfo = ToolDb.ExecuteScalar(sql);
                                if (dtinfo != null && !string.IsNullOrEmpty(dtinfo.ToString()))
                                {
                                    string id = dtinfo.ToString();
                                    string strSql = string.Format("update BidProject set BidResultEndTime='{0}' where Id='{1}'", info.BidResultEndTime, id);
                                    int result = ToolDb.ExecuteSql(strSql);
                                    if (result > 0)
                                    {
                                        SaveAttach(aLink, id);
                                    }
                                }
                                else
                                {
                                    SaveAttach(aLink, obj.ToString());
                                }
                            }
                            else
                            {
                                ToolDb.SaveEntity(info, "");
                                SaveAttach(aLink, info.Id);
                            }
                        }
                    }

                }
            }
            return list;

        }

        protected void SaveAttach(string url, string sourceId)
        {
            List<BaseAttach> attach = new List<BaseAttach>();
            string htmlAnnex = string.Empty;
            try
            {
                htmlAnnex = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
            }
            catch { }
            Parser dtparser = new Parser(new Lexer(htmlAnnex));
            NodeList dtList = dtparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_GridView1"), new TagNameFilter("table")));
            if (dtList != null && dtList.Count > 0)
            {
                TableTag dttable = dtList[0] as TableTag;
                for (int t = 1; t < dttable.RowCount; t++)
                {
                    ATag file = dttable.SearchFor(typeof(ATag), true)[t - 1] as ATag;
                    if (file.IsAtagAttach())
                    {
                        string aurl = "http://www.szjsjy.com.cn/" + file.Link.Replace("../", "").Replace("./", "");
                        try
                        {
                            BaseAttach entity = ToolHtml.GetBaseAttach(aurl, file.LinkText, sourceId, "SiteManage\\Files\\Attach\\");
                            if (entity != null)
                            {
                                attach.Add(entity);
                            }
                        }
                        catch { }
                    }
                }
            }
            if (attach.Count > 0)
            {
                string delSql = string.Format("delete from BaseAttach where SourceID='{0}'", sourceId);
                ToolFile.Delete(sourceId);
                int count = ToolDb.ExecuteSql(delSql);
                ToolDb.SaveDatas(attach, string.Empty);
            }
        }
    }
}
