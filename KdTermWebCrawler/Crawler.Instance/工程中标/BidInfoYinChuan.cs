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
using System.Web.UI.HtmlControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class BidInfoYinChuan : WebSiteCrawller
    {
        public BidInfoYinChuan()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "银川市公共资源交易中心中标信息";
            this.Description = "自动抓取银川市公共资源交易中心中标信息";
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.ycsggzy.cn/morelink.aspx?type=17&index=1";
            this.MaxCount = 500;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string cookiestr = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "pager")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    Regex reg = new Regex(@"[0-9]+");
                    string temp = reg.Match(pageNode[pageNode.Count - 1].GetATagHref().Replace("&#39;", "")).Value;
                    pageInt = int.Parse(temp);
                }
                catch
                {
                }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                        new string[] { "__VIEWSTATE",
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__VIEWSTATEENCRYPTED",
                        "__EVENTVALIDATION",
                        "hsa1$DD_LX",
                        "hsa1$wd",
                        "pager_input"},
                        new string[] {
                        viewState,
                        "pager",
                        i.ToString(),
                        "",
                        eventValidation,
                        "综合搜索",
                        "",
                        (i-1).ToString()
                        }
                        );
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GV1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string prjName = string.Empty,
                               buildUnit = string.Empty, bidUnit = string.Empty,
                               bidMoney = string.Empty, code = string.Empty,
                               bidDate = string.Empty,
                               beginDate = string.Empty,
                               endDate = string.Empty, bidType = string.Empty,
                               specType = string.Empty, InfoUrl = string.Empty,
                               msgType = string.Empty, bidCtx = string.Empty,
                               prjAddress = string.Empty, remark = string.Empty,
                               prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[0].ToNodePlainString();
                        beginDate = tr.Columns[1].ToPlainTextString();
                        InfoUrl = "http://www.ycsggzy.cn/" + tr.Columns[0].GetATagHref().Replace("&amp;", "&");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "Lb_nr")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml().GetReplace("<br>", "<br />");
                            bidCtx = HtmlTxt.ToCtxString();

                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetBidRegex(null, false);
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("第一名");
                            bidUnit = bidUnit.Replace("?", "");
                            bidMoney = bidCtx.GetMoneyRegex(null, false, "万元");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag tab = tableNode[0] as TableTag;
                                    for (int c = 0; c < tab.RowCount; c++)
                                    {
                                        if (tab.Rows[c].ColumnCount < 2) continue;
                                        ctx += tab.Rows[c].Columns[0].ToNodePlainString() + "：";
                                        ctx += tab.Rows[c].Columns[1].ToNodePlainString() + "\r\n";
                                    }
                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                        bidMoney = ctx.GetMoneyRegex(null, false, "万元");
                                }
                            }
                            bidUnit = bidUnit.Replace(" ", "");
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            code = bidCtx.GetCodeRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex().Replace(" ", "");
                            bidType = prjName.GetInviteBidType();
                            specType = "建设工程";
                            msgType = "银川市公共资源交易中心";
                            BidInfo info = ToolDb.GenBidInfo("宁夏回族自治区", "宁夏回族自治区及地市", "银川市", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
