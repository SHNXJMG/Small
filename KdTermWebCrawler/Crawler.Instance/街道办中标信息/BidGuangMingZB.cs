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
    public class BidGuangMingZB : WebSiteCrawller
    {
        public BidGuangMingZB()
            : base()
        {
            this.Group = "街道办中标信息";
            this.Title = "广东省深圳市光明新区中标公告";
            this.Description = "自动抓取广东省深圳市光明新区中标公告";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.SiteUrl = "http://www.szgm.gov.cn/szgm/132100/xwdt17/135204/151250/index.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "box"))), new TagNameFilter("a")));
            if (nodeList != null && nodeList.Count > 0)
            {
                try
                {
                    ATag aTag = nodeList[nodeList.Count - 1] as ATag;
                    if (aTag.ToPlainTextString().Contains("末页"))
                    {
                        page = int.Parse(aTag.GetAttribute("tagname").ToLower().Replace("/szgm/132100/xwdt17/135204/151246/8d25503a-", "").Replace(".html", ""));
                    }
                }
                catch { }
            }
            if (page == 1)
                page = 82;
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.szgm.gov.cn/szgm/132100/xwdt17/135204/151250/897d248a-" + i.ToString() + ".html"), Encoding.UTF8);
                    }
                    catch { return list; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tabList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page_co")), true), new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%"))));
                if (tabList != null && tabList.Count > 0)
                {
                    for (int j = 0; j < tabList.Count; j++)
                    {
                        TableRow tr = (tabList[j] as TableTag).Rows[0];
                        ATag aTag = tr.GetATag();
                        if (aTag == null || tr.ColumnCount != 3) continue;

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

                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        prjName = aTag.GetAttribute("title");

                        InfoUrl = "http://www.szgm.gov.cn" + aTag.Link;
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
                            htlDtl = regexHtml.Replace(htlDtl, "");
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "article_body")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            string tempName = bidCtx.GetRegex("工程名称");
                            if (!string.IsNullOrWhiteSpace(tempName))
                                prjName = tempName;
                            code = bidCtx.GetCodeRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("委托单位");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegexBegEnd("确认", "为中标单位");
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetRegex("合同价").GetMoney();
                            if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetRegexBegEnd("人民币", "元").GetMoney();

                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "holder")), true), new TagNameFilter("table")));
                                if (tableNode == null || tableNode.Count < 1)
                                {
                                    parser.Reset();
                                    tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoNormalTable")));
                                }
                                string ctx = string.Empty;
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    TableTag table = tableNode[0] as TableTag;
                                    if (table.RowCount >= 2)
                                    {
                                        for (int r = 0; r < table.Rows[0].ColumnCount; r++)
                                        {
                                            string temp = table.Rows[0].Columns[r].ToNodePlainString();
                                            if (temp.Contains("控制金额")) continue;
                                            ctx += temp + "：";
                                            ctx += table.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                        }
                                    }
                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                        bidMoney = ctx.GetMoneyRegex();

                                    if (string.IsNullOrWhiteSpace(code))
                                        code = ctx.GetCodeRegex();
                                }
                            }
                            try
                            {
                                if (decimal.Parse(bidMoney) > 50000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            code = code.GetCodeDel();
                            msgType = "深圳市光明新区";
                            if (string.IsNullOrEmpty(prjAddress))
                            { prjAddress = "见招标信息"; }
                            specType = "政府采购";
                            bidType = prjName.GetInviteBidType();
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "深圳市光明新区公明街道办事处";
                            }
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳区及街道工程", "光明新区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                   bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
