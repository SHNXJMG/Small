using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    /// <summary>
    /// 广东省深圳市变更公示
    /// </summary>
    public class NoticeSzBGGS : WebSiteCrawller
    {
        public NoticeSzBGGS()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市变更公示";
            this.Description = "自动抓取广东省深圳变更公示";
            this.ExistCompareFields = "Prov,Area,Road,InfoTitle,InfoType,PublishTime,InfoUrl";
            this.SiteUrl = "http://www.szjsjy.com.cn/BusinessInfo/BGXXIndex.aspx";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {

            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            string viewState = "";
            string eventValidation = "";
            Parser parser = new Parser(new Lexer(html));
            //处理第一页
            DealHtml(list, html, crawlAll);
            NodeList sNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("class", "ListTablePager")));
            if (sNodes != null && sNodes.Count > 0)
            {
                TableRow tr = sNodes[0] as TableRow;
                TableTag table = tr.SearchFor(typeof(TableTag), true)[0] as TableTag;
                string str = table.Rows[0].Columns[0].ToPlainTextString();
                Regex regex = new Regex(@"，共[^页]+页");
                Match match = regex.Match(str);
                pageInt = int.Parse(match.Value.Replace("，共", "").Replace("页", "").Trim());
                parser.Reset();
                //处理后续页
                if (pageInt > 1)
                {
                    string cookiestr = string.Empty;
                    for (int i = 2; i <= pageInt; i++)
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "__VIEWSTATEENCRYPTED", "__EVENTVALIDATION", "ctl00$Header$drpSearchType", "ctl00$Header$beginDate", "ctl00$Header$endDate", "ctl00$Header$txtGcxm", "ctl00$hdnPageCount" },
                       new string[] { "ctl00$Content$GridView1", "Page$" + i.ToString(), viewState, "",eventValidation, "0","","","", pageInt.ToString() });
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                        }
                        catch (Exception ex)
                        {

                            continue;
                        }
                      
                        DealHtml(list, html, crawlAll);

                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }

            }
            return list;
        }

        public void DealHtml(IList list, string html, bool crawlAll)
        {
            Parser parserDtl = new Parser(new Lexer(html));
            NodeList aNodes = parserDtl.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_Content_GridView1")));
            if (aNodes != null && aNodes.Count > 0)
            {
                Type typs = typeof(ATag);
                TableTag table = aNodes[0] as TableTag;
                for (int t = 1; t < table.RowCount - 1; t++)
                {
                    string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, htmlTxt = string.Empty, prjCode = string.Empty;
                    TableRow tr = table.Rows[t] as TableRow;

                    prjCode = table.Rows[t].Columns[1].ToNodePlainString();
                    InfoTitle = table.Rows[t].Columns[3].ToPlainTextString().Trim();
                    InfoType = "变更公示";
                    PublistTime = table.Rows[t].Columns[6].ToPlainTextString().Trim();
                    InfoUrl = SiteUrl;
                    InfoCtx = "工程编号：" + table.Rows[t].Columns[1].ToPlainTextString().Trim()+"\n";
                    InfoCtx += "工程名称：" + table.Rows[t].Columns[2].ToPlainTextString().Trim() + "\n";
                    InfoCtx += "变更标题：" + table.Rows[t].Columns[3].ToPlainTextString().Trim() + "\n";
                    InfoCtx += "变更类型：" + table.Rows[t].Columns[4].ToPlainTextString().Trim() + "\n";
                    InfoCtx += "变更内容：" + table.Rows[t].Columns[5].ToPlainTextString().Trim() + "\n";
                    InfoCtx += "发布时间：" + table.Rows[t].Columns[6].ToPlainTextString().Trim() + "\n";


                    htmlTxt = InfoCtx;
                    NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳市工程", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "深圳市建设工程交易中心", InfoUrl, prjCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, htmlTxt);


                    list.Add(info);
 
                    if (!crawlAll && list.Count >= this.MaxCount) return;
                }
            }

        }
    }
}
