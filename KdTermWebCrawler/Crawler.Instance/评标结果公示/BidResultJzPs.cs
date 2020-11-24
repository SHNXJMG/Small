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
    public class BidResultJzPs : WebSiteCrawller
    {
        public BidResultJzPs()
            : base()
        {
            this.Group = "评标结果公示";
            this.Title = "广东省深圳市建设工程评标结果公示(建筑方案评审)";
            this.Description = "自动抓取广东省深圳市建设工程评标结果公示(建筑方案评审)";
            this.PlanTime = "04:00";
            this.ExistCompareFields = "PrjNo";
            this.MaxCount = 20;
            this.SiteUrl = "http://www.szjsjy.com.cn/BusinessInfo/PBYQZJGSList.aspx";
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
                            "__VIEWSTATEENCRYPTED",
                            "__EVENTVALIDATION",
                            "ctl00$Header$drpSearchType",
                            "ctl00$Header$txtGcxm", 
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
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
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
                        string bProv = string.Empty, bCity = string.Empty, bArea = string.Empty, bPrjno = string.Empty, bPrjname = string.Empty,  bExpertendtime = string.Empty, bBidresultendtime = string.Empty, bBaseprice = string.Empty, bBiddate = string.Empty, bBuildunit = string.Empty, bBidmethod = string.Empty,  bRemark = string.Empty, bInfourl = string.Empty;

                        TableRow tr = table.Rows[i] as TableRow;
                        bPrjno = tr.Columns[1].ToPlainTextString();
                        bPrjname = tr.Columns[2].ToPlainTextString();
                        bExpertendtime = tr.Columns[3].ToPlainTextString();
                        string aLink = tr.Columns[4].GetATagHref().GetRegexBegEnd("'",",").Replace("'","").Replace(",","");
                        string bLink = tr.Columns[4].GetATagHref().GetRegexBegEnd(",'", "'").Replace("'", "").Replace(",", "");

                        bInfourl = "http://www.szjsjy.com.cn/BusinessInfo/PBYQZJGSViewForm.aspx?GCBH="+aLink+"&GCMC="+this.ToolWebSite.UrlEncode(bLink);
                        string htmlDtl = string.Empty;
                        try
                        {
                            htmlDtl = this.ToolWebSite.GetHtmlByUrl(bInfourl, Encoding.UTF8);
                        }
                        catch { }
                        BidProject info = ToolDb.GenExpertProject("广东省", "深圳市", "", bPrjno, bPrjname, bExpertendtime, bBaseprice, bBiddate, bBuildunit, bBidmethod, bRemark, bInfourl);
                        sqlCount++;
                        if (sqlCount > this.MaxCount) return null;
                        if (ToolDb.SaveEntity(info, ExistCompareFields, true, this.ExistsHtlCtx, null))
                        {
                            AddExpert(htmlDtl, bInfourl,info.Id);
                        }
                    }
                }

            }
            return list;
        }

        public void AddExpert(string html, string infourl, string id)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodelist = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "ListTable"), new TagNameFilter("table")));
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
                    BidProjectExpert info = ToolDb.GenProjectExpert(id, bExpertname, bBidtype, bExpertspec, bExpertunit, string.Empty, infourl);
                    ToolDb.SaveEntity(info, "BidProjectId,ExpertName,BidType", true);
                }
            }
        }
    }
}
