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

namespace Crawler.Instance
{
    public class BidResultJz : WebSiteCrawller
    {
        public BidResultJz()
            : base()
        {
            this.Group = "评标结果公示";
            this.Title = "广东省深圳市建设工程评标结果公示(建筑方案)";
            this.Description = "自动抓取广东省深圳市建设工程评标结果公示(建筑方案)";
            this.PlanTime = "04:00";
            this.ExistCompareFields = "PrjNo";
            this.MaxCount = 20;
            this.SiteUrl = "http://www.szjsjy.com.cn/BusinessInfo/PsgsList.aspx";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int sqlCount = 0;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));

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
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { 
                            "__EVENTTARGET",  
                            "__EVENTARGUMENT", 
                            "__VIEWSTATE",
                            "__EVENTVALIDATION",
                            "ctl00$Header$drpSearchType",
                            "ctl00$Header$txtQymc",
                            "ctl00$Content$hdnOperate", 
                            "ctl00$hdnPageCount" 
	
                        }, new string[] { 
                            "ctl00$Content$GridView1",
                            "Page$"+j.ToString(),
                            viewState,
                            eventValidation,
                            "0",
                            string.Empty,
                            string.Empty, 
                            pageInt.ToString()
                        });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_Content_GridView1")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    int rows = table.RowCount;
                    if (pageInt > 1)
                        rows = rows - 1;
                    for (int i = 1; i < rows; i++)
                    {
                        string bProv = string.Empty, bCity = string.Empty, bArea = string.Empty, bPrjno = string.Empty,
                            bPrjname = string.Empty, bBidresultendtime = string.Empty,
                            bBaseprice = string.Empty, bBiddate = string.Empty, bBuildunit = string.Empty, bBidmethod = string.Empty,
                             bRemark = string.Empty, bInfourl = string.Empty;

                        TableRow tr = table.Rows[i] as TableRow;
                        bPrjno = tr.Columns[1].ToPlainTextString();
                        bPrjname = tr.Columns[2].ToPlainTextString();
                        bBidresultendtime = tr.Columns[3].ToPlainTextString();
                        bInfourl = "http://www.szjsjy.com.cn/BusinessInfo/" + tr.Columns[4].GetATagHref();
                        string htmlDtl = string.Empty;
                        try
                        {
                            htmlDtl = this.ToolWebSite.GetHtmlByUrl(bInfourl, Encoding.UTF8);
                        }
                        catch { }
                        BidProject info = ToolDb.GenResultProject("广东省", "深圳市", "", bPrjno, bPrjname, bBidresultendtime, bBaseprice, bBiddate, bBuildunit, bBidmethod, bRemark, bInfourl);
                        sqlCount++;
                        if (sqlCount > this.MaxCount) return null;
                        if (ToolDb.SaveEntity(info, ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx, null))
                        {
                            Parser dtparser = new Parser(new Lexer(htmlDtl));
                            NodeList dtList = dtparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_GridView1"), new TagNameFilter("table")));
                            if (dtList != null && dtList.Count > 0)
                            {
                                TableTag dttable = dtList[0] as TableTag;
                                for (int t = 1; t < dttable.RowCount; t++)
                                {
                                    ATag file = dttable.SearchFor(typeof(ATag), true)[t - 1] as ATag;
                                    if (file.IsAtagAttach())
                                    {
                                        string url = "http://www.szjsjy.com.cn/" + file.Link.Replace("../", "").Replace("./", "");
                                        BaseAttach entity = ToolHtml.GetBaseAttach(url, file.LinkText, info.Id, "SiteManage\\Files\\Attach\\");
                                        if (entity != null)
                                        {
                                            ToolDb.SaveEntity(entity, string.Empty);
                                        }
                                    }
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
