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
    public class BidDongguan : WebSiteCrawller
    {
        public BidDongguan()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省东莞市";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.Description = "自动抓取广东省东莞市区中标信息";
            this.SiteUrl = "http://www.dgzb.com.cn:8080/dgjyweb/sitemanage/GxInfo_List.aspx?ModeId=4";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {

            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8);
            }
            catch (Exception ex)
            {

                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_GridViewPaingTwo1_lblGridViewPagingDesc"), new TagNameFilter("span")));
            string pageString = sNode.AsString();
            Regex regexPage = new Regex(@"共[^页]+页");
            Match pageMatch = regexPage.Match(pageString);
            try { pageInt = int.Parse(pageMatch.Value.Replace("共", "").Replace("页", "").Trim()); }
            catch (Exception) { }

            string cookiestr = string.Empty;
            for (int i = 1; i < pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__LASTFOCUS", "__VIEWSTATE", "__EVENTVALIDATION", "ctl00$cph_context$drp_selSeach", "ctl00$cph_context$txt_strWhere", "ctl00$cph_context$drp_Rq", "ctl00$cph_context$GridViewPaingTwo1$txtGridViewPagingForwardTo", "ctl00$cph_context$GridViewPaingTwo1$btnNext.x", "ctl00$cph_context$GridViewPaingTwo1$btnNext.y" }, new string[] { string.Empty, string.Empty, string.Empty, viewState, eventValidation, "1", string.Empty, "3", (i - 1).ToString(), "9", "7" });
                    try { html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr); }
                    catch (Exception ex) { continue; }
                }

                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_GridView1"), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
            bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j] as TableRow;
                        code = tr.Columns[1].ToPlainTextString().Trim();
                        prjName = tr.Columns[2].ToPlainTextString().Trim(); 
                        buildUnit = tr.Columns[3].ToPlainTextString().Trim();
                        beginDate = tr.Columns[4].ToPlainTextString().Trim().GetReplace(" - ", "&").Split('&')[0].Trim();
                        try
                        {
                            endDate = tr.Columns[4].ToPlainTextString().Trim().GetReplace(" - ", "&").Split('&')[1].Trim();
                        }
                        catch { }
                        ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://www.dgzb.com.cn:8080/dgjyweb/sitemanage/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_span_MetContent"), new TagNameFilter("span")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n").Replace("<br/>", "\r\n"); }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_span_MetContent"), new TagNameFilter("span")));
                        bidCtx = dtnode.AsString().Replace(" ", "");
                        Regex regPrjAdd = new Regex(@"(工程地点|工程地址)：[^\r\n]+[\r\n]{1}");
                        prjAddress = regPrjAdd.Match(bidCtx).Value.Replace("工程地点：", "").Replace("工程地址：", "").Trim();
                        msgType = "东莞市建设工程交易中心";
                        specType = "建设工程";
                        Regex regMoney = new Regex(@"(中标价|中标值)：[^元]+元{1}");

                        string moneystr = regMoney.Match(bidCtx).Value.Replace("中标价：", "").Replace("中标值：", "").Replace("元", "").Trim();
                        if (moneystr.Contains("万"))
                        {
                            bidMoney = moneystr.Replace("万", "").Replace("元","");
                        }
                        else
                        {
                            try  {  bidMoney = (decimal.Parse(moneystr) / 10000).ToString(); }  catch (Exception) {  }
                          
                        }
                        bidMoney = regMoney.Match(bidCtx).Value.Replace("中标价：", "").Replace("中标值：", "").Replace("元", "").Trim();
                        Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                        if (!string.IsNullOrEmpty(regBidMoney.Match(bidMoney).Value))
                        {
                            if (bidMoney.Contains("万元") || bidMoney.Contains("万美元") || bidMoney.Contains("万"))
                            {
                                bidMoney = regBidMoney.Match(bidMoney).Value;
                            }
                            else
                            {
                                try
                                {
                                    bidMoney = (decimal.Parse(regBidMoney.Match(bidMoney).Value) / 10000).ToString();
                                    if (decimal.Parse(bidMoney) < decimal.Parse("0.1"))
                                    {
                                        bidMoney = "0";
                                    }
                                }
                                catch (Exception)
                                {
                                    bidMoney = "0";
                                }
                            }
                        }
                        Regex regBidUnit = new Regex(@"(中标人|中标单位)：[^\r\n]+[\r\n]{1}");
                        bidUnit = regBidUnit.Match(bidCtx).Value.Replace("中标人：", "").Replace("中标单位：", "").Trim();
                        Regex regprjMgr = new Regex(@"(项目经理)：[^\r\n]+[\r\n]{1}");
                        prjMgr = regprjMgr.Match(bidCtx).Value.Replace("项目经理：", "").Trim();

                        prjName = ToolDb.GetPrjName(prjName);
                        bidType = ToolHtml.GetInviteTypes(prjName);
                        if (!string.IsNullOrEmpty(bidUnit))
                        {
                            string[] unit = bidUnit.Split(',');
                            if (unit.Length > 0)
                            {
                                bidUnit = unit[0];
                            }
                        }
                        if (Encoding.Default.GetByteCount(bidUnit) > 150)
                            bidUnit = string.Empty;
                        BidInfo info = ToolDb.GenBidInfo("广东省", "东莞市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        list.Add(info);
                        dtlparser.Reset();
                        NodeList fileNode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_DownLoadFiles1_GridView2"), new TagNameFilter("table")));
                        if (fileNode != null && fileNode.Count > 0 && fileNode[0] is TableTag)
                        {
                            TableTag fileTable = fileNode[0] as TableTag;
                            for (int f = 1; f < fileTable.Rows.Length; f++)
                            {
                                BaseAttach attach = ToolDb.GenBaseAttach(fileTable.Rows[f].Columns[1].ToPlainTextString().Trim(), info.Id, "http://www.dgzb.com.cn/dgjyweb/sitemanage/" + (fileTable.Rows[f].Columns[1].SearchFor(typeof(ATag), true)[0] as ATag).Link);
                                base.AttachList.Add(attach);
                            }

                        }
                        if (!crawlAll && list.Count >= this.MaxCount) return list;


                    }
                }
            }
            return list;

        }
    }
}
