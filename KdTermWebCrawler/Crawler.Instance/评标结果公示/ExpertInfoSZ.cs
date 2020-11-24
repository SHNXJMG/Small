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

namespace Crawler.Instance
{
    public class ExpertInfoSZ : WebSiteCrawller
    {
        public ExpertInfoSZ()
            : base()
        {
            this.Group = "评标结果公示";
            this.Title = "广东省深圳市建设工程交易工程专家库";
            this.MaxCount = 2000;
            this.Description = "自动抓取广东省深圳市建设工程交易工程专家库";
            this.PlanTime = "1 2:30,15 2:30";
            this.ExistCompareFields = "ExpertName,WorkUnit,Profession";
            this.SiteUrl = "http://www.szjsjy.com.cn/BusinessInfo/ZJKGSList.aspx";
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
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8,ref cookiestr);
            }
            catch { return null; }
             Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("cellSpacing", "2"), new TagNameFilter("table")));
            if (nodeList != null && nodeList.Count > 0)
            {
                string pageString = nodeList.AsString();
                Regex regexPage = new Regex(@"共[^页]+页，");
                Match pageMatch = regexPage.Match(pageString);
                try
                {
                    pageInt = int.Parse(pageMatch.Value.Replace("共", "").Replace("页，", "").Replace(" ", ""));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
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
                            "Page$"+i.ToString(),
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
                nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_Content_GridView1"), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string expertName = string.Empty, workUnit = string.Empty, profession = string.Empty, remark = string.Empty, infourl = string.Empty, creator = string.Empty, CreateTime = string.Empty, lastModifTime = string.Empty, lastModifier = string.Empty;
                        TableRow tr = table.Rows[j];
                        expertName = tr.Columns[1].ToPlainTextString().Trim();
                        workUnit = tr.Columns[2].ToPlainTextString().Trim();
                        profession = tr.Columns[3].ToPlainTextString().Replace("&lt;", "").Replace("br", "").Replace("&gt;", "").Trim();
                        infourl = SiteUrl;
                        ExpertInfo info = ToolDb.GenExpertInfo(expertName, workUnit, profession, remark, infourl, creator, CreateTime, lastModifier, lastModifTime);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
