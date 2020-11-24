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
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Data;

namespace Crawler.Instance
{
    public class CorpAwardSKY : WebSiteCrawller
    {
        public CorpAwardSKY()
            : base()
        {
            this.PlanTime = "1 23:01,8 23:01,16 23:01,24 23:01";
            this.Group = "企业信息";
            this.Title = "广东省三库一平台获奖信息";
            this.Description = "自动抓取广东省三库一平台获奖信息";
            this.ExistCompareFields = "Url";
            this.ExistsUpdate = true;
            this.MaxCount = 50000;
            this.SiteUrl = "http://113.108.219.40/PlatForm/SearchCenter/AwardSearch.aspx";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            Parser parser = null;
            string eventValidation = string.Empty; 
            DateTime dateTime = DateTime.Now;
            DateTime begin = DateTime.Parse("1980-01-01"); 
            for (DateTime t = begin; t <= dateTime;t = t.AddDays(30))
            {
                string endDate = t.AddDays(30).ToString("yyyy-MM-dd");
                try
                {
                    html = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
                    parser = new Parser(new Lexer(html));
                    NodeList pageInputNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("input"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_ValidateCode1_txtRanNum")));
                    string pageValiCode = string.Empty;
                    if (pageInputNode != null && pageInputNode.Count > 0) pageValiCode = (pageInputNode[0] as InputTag).GetAttribute("value");
                    viewState = ToolWeb.GetAspNetViewState(html);
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(
                           new string[]{
                        "ctl00_ContentPlaceHolder1_toolkitScriptManager1_HiddenField",
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__VIEWSTATE",
                        "ctl00$ContentPlaceHolder1$txtEnt_name",
                        "ctl00$ContentPlaceHolder1$txtAWARD_NAME",
                        "ctl00$ContentPlaceHolder1$txtStartDate",
                        "ctl00$ContentPlaceHolder1$txtEndDate",
                        "ctl00$ContentPlaceHolder1$ValidateCode1$txtValidateCode",
                        "ctl00$ContentPlaceHolder1$ValidateCode1$txtRanNum"
                        },
                           new string[]{
                        "",
                        "ctl00$ContentPlaceHolder1$AspNetPager2",
                       "1",
                        viewState,
                        "","",t.ToString("yyyy-MM-dd"),endDate,"",
                        pageValiCode
                        });
                    html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                }
                catch
                {
                    return null;
                }

                string opValue = string.Empty;
                parser = new Parser(new Lexer(html));
                NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_AspNetPager2")));
                if (pageList != null && pageList.Count > 0)
                {
                    try
                    {
                        string temp = pageList[0].ToPlainTextString().GetRegexBegEnd("共", "条");
                        int page = int.Parse(temp);
                        int result = page / 15;
                        if (page % 15 != 0)
                            pageInt = result + 1;
                        else
                            pageInt = result;
                    }
                    catch { pageInt = 1; }
                }
                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        parser = new Parser(new Lexer(html));
                        NodeList pageInputNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("input"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_ValidateCode1_txtRanNum")));
                        string pageValiCode = string.Empty;
                        if (pageInputNode != null && pageInputNode.Count > 0) pageValiCode = (pageInputNode[0] as InputTag).GetAttribute("value");
                        viewState = ToolWeb.GetAspNetViewState(html);
                        NameValueCollection nvc = ToolWeb.GetNameValueCollection(
                            new string[]{
                        "ctl00_ContentPlaceHolder1_toolkitScriptManager1_HiddenField",
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__VIEWSTATE",
                        "ctl00$ContentPlaceHolder1$txtEnt_name",
                        "ctl00$ContentPlaceHolder1$txtAWARD_NAME",
                        "ctl00$ContentPlaceHolder1$txtStartDate",
                        "ctl00$ContentPlaceHolder1$txtEndDate",
                        "ctl00$ContentPlaceHolder1$ValidateCode1$txtValidateCode",
                        "ctl00$ContentPlaceHolder1$ValidateCode1$txtRanNum"
                        },
                            new string[]{
                        "",
                        "ctl00$ContentPlaceHolder1$AspNetPager2",
                        i.ToString(),
                        viewState,
                        "","",t.ToString("yyyy-MM-dd"),endDate,"",
                        pageValiCode
                        });
                        try
                        {
                            html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                        }
                        catch { continue; }
                    } 
                    parser = new Parser(new Lexer(html));
                    NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "tab_ent")));
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        TableTag table = nodeList[0] as TableTag;
                        for (int j = 1; j < table.RowCount; j++)
                        {
                            string CorpCode = string.Empty, CorpName = string.Empty, MeritYear = string.Empty, MeritName = string.Empty, MeritDate = string.Empty, MeritLevel = string.Empty, MeritRegion = string.Empty, MeritSector = string.Empty, MeritPrjName = string.Empty, PrjSupporter = string.Empty, Source = string.Empty, Url = string.Empty, Remark = string.Empty, Details = string.Empty;

                              
                            TableRow tr = table.Rows[j];

                            CorpName = tr.Columns[2].ToNodePlainString();
                            MeritName = tr.Columns[1].ToNodePlainString();
                            MeritDate = tr.Columns[3].ToPlainTextString().GetDateRegex();

                            Url = "http://113.108.219.40/PlatForm/SearchCenter/" + tr.Columns[1].GetATagHref();
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = ToolWeb.GetHtmlByUrl(Url, Encoding.UTF8);
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                TableTag tab = dtlList[0] as TableTag;
                                string ctx = string.Empty;
                                for (int k = 0; k < tab.RowCount; k++)
                                {
                                    for (int d = 0; d < tab.Rows[k].ColumnCount; d++)
                                    {
                                        if ((d + 1) % 2 == 0)
                                            ctx += tab.Rows[k].Columns[d].ToNodePlainString() + "\r\n";
                                        else
                                            ctx += tab.Rows[k].Columns[d].ToNodePlainString().Replace("：", "").Replace(":", "") + "：";
                                    }
                                }
                                MeritLevel = ctx.GetRegex("获奖等级");
                                Remark = ctx.GetRegex("备注");
                                Details = ctx.GetRegex("表彰内容描述");
                                Source = "广东省住房和城乡建设厅";
                                if (Remark.Contains("无备注")||Remark == "无") Remark = null;
                                CorpMerit info = ToolDb.GenCorpMerit("广东省", "广东地区", "", CorpCode, CorpName, MeritYear, MeritName, MeritDate, MeritLevel, MeritRegion, MeritSector, MeritPrjName, PrjSupporter, Source, Url, Remark, Details);
                                ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx);
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
