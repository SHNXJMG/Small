using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;
namespace Crawler.Instance
{
    public class BidInfoJiangXiZhufh : WebSiteCrawller
    {
        public BidInfoJiangXiZhufh()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "江西省住房和城乡建设厅中标信息";
            this.Description = "自动抓取江西省住房和城乡建设厅中标信息";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.jxjst.gov.cn/zhongbiaoxinxi/";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "epages")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].GetATagHref().GetRegexBegEnd("index", "htm").Replace("_", "").Replace(".", "");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "index_" + i + ".html", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "gzcysublist")), true), new TagNameFilter("a")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        ATag aTag = listNode[j].GetATag();
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

                        prjName = aTag.GetAttribute("title");
                        beginDate = aTag.LinkText.GetDateRegex();
                        InfoUrl = "http://www.jxjst.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "detailContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.Replace("</div>","\r\n").ToCtxString();
                            prjAddress = bidCtx.GetAddressRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            bidType = prjName.GetInviteBidType();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("第一名");
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList dtlTable = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (dtlTable != null && dtlTable.Count > 0)
                                {
                                    TableTag table = dtlTable[dtlTable.Count-1] as TableTag;
                                    string ctx = string.Empty;
                                    if (dtlTable.AsHtml().Contains("第一排序人") || dtlTable.AsHtml().Contains("招标工程项目基本信息"))
                                    {
                                        for (int r = 0; r < table.RowCount; r++)
                                        {
                                            for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                                            {
                                                if ((c + 1) % 2 == 0)
                                                    ctx += table.Rows[r].Columns[c].ToNodePlainString() + "\r\n";
                                                else
                                                    ctx += table.Rows[r].Columns[c].ToNodePlainString().Replace("：","").Replace(":","") + "：";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (table.RowCount > 1)
                                        {
                                            for (int c = 0; c < table.Rows[0].ColumnCount; c++)
                                            {
                                                ctx += table.Rows[0].Columns[c].ToNodePlainString() + "：";
                                                try
                                                {
                                                    if (table.Rows[1].ColumnCount > table.Rows[0].ColumnCount)
                                                        ctx += table.Rows[1].Columns[c + 1].ToNodePlainString() + "\r\n";
                                                    else
                                                        ctx += table.Rows[1].Columns[c].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                    bidUnit = ctx.GetBidRegex();
                                    if (bidUnit.Contains("第一"))
                                        bidUnit = bidUnit.Replace("第一", "");
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("第一中标排序单位名称,第一排序人,投标单位名称,投标单位,投标人名称,投标人");
                                    if (bidMoney == "0")
                                        bidMoney = ctx.GetMoneyRegex();
                                }
                            }
                            if (bidUnit.Contains("第一"))
                                bidUnit = bidUnit.Replace("第一","");
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("排序第一的中标候选人名称,第一排序人");
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            specType = "建设工程";
                            msgType = "江西省住房和城乡建设厅";
                            BidInfo info = ToolDb.GenBidInfo("江西省", "江西省及地市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
