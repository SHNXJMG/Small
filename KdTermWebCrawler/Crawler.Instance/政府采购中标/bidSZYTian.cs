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
    public class bidSZYTian : WebSiteCrawller
    {
        public bidSZYTian()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "广东省深圳市盐田区中标信息";
            this.Description = "自动抓取广东省深圳市盐田区中标信息";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.yantian.gov.cn/cn/zwgk/zfcg/zb/index.shtml";
            this.MaxCount = 50;
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
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("align", "right")));
            if (tableNodeList != null && tableNodeList.Count > 0)
            {
                Regex regexPage = new Regex(@"共\d+页");
                page = int.Parse(regexPage.Match(tableNodeList.AsString()).Value.Trim(new char[] { '共', '页' }));
            }
            for (int i = 1; i <= page; i++)
            {

                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.yantian.gov.cn/cn/zwgk/zfcg/zb/index_" + (i - 1).ToString() + ".shtml"), Encoding.UTF8);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "565")), true), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    string url = string.Empty;
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string beg = nodeList[j].ToPlainTextString().GetDateRegex();
                        if (string.IsNullOrEmpty(beg))
                            continue;
                        else if (j > 0 && nodeList[j].GetATagHref() == url)
                        {
                            continue;
                        }
                        url = nodeList[j].GetATagHref().GetReplace("&#61;", "=").GetReplace("&amp;", "&");
                        TableTag table = nodeList[j] as TableTag;
                        string prjName = string.Empty,
                          buildUnit = string.Empty, bidUnit = string.Empty,
                          bidMoney = string.Empty, code = string.Empty,
                          bidDate = string.Empty, beginDate = string.Empty,
                          endDate = string.Empty, bidType = string.Empty,
                          specType = string.Empty, InfoUrl = string.Empty,
                          msgType = string.Empty, bidCtx = string.Empty,
                          prjAddress = string.Empty, remark = string.Empty,
                          prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        prjName = table.GetATagValue("title");
                        beginDate = beg;
                        string htmldetail = string.Empty;
                        if (!url.ToLower().Contains("http"))
                            InfoUrl = "http://www.yantian.gov.cn" + table.GetATagHref();
                        else
                            InfoUrl = url;
                        string htmltext = string.Empty;
                        string htmldttext = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Replace("<br />", "\r\n").Trim();
                            htmltext = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                        }
                        catch
                        {
                            continue;
                        }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        Parser texthtml = new Parser(new Lexer(htmltext));
                        NodeList listtext = texthtml.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            htmldttext = listtext.AsHtml();
                            bidCtx = dtnode.AsString().Replace(" ", "").Trim();
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            bidCtx = regexHtml.Replace(bidCtx, "");
                            dtlparser = new Parser(new Lexer(HtmlTxt));
                            NodeList spanNode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "holder")), true), new TagNameFilter("table")));
                            if (spanNode != null && spanNode.Count > 1)
                            {
                                TableTag spanTable = spanNode[1] as TableTag;
                                string ctx = string.Empty;
                                for (int t = 1; t < spanTable.RowCount; t++)
                                {
                                    for (int c = 0; c < spanTable.Rows[t].ColumnCount; c++)
                                    {
                                        ctx += spanTable.Rows[0].Columns[c].ToNodePlainString() + "：" + spanTable.Rows[t].Columns[c].ToNodePlainString() + "\r\n";
                                    }
                                }
                                bidUnit = ctx.GetBidRegex();
                                if (!string.IsNullOrEmpty(bidUnit))
                                {
                                    bool isBreak = false;
                                    spanTable = spanNode[0] as TableTag;
                                    for (int t = 1; t < spanTable.RowCount; t++)
                                    {
                                        for (int c = 0; c < spanTable.Rows[t].ColumnCount; c++)
                                        {
                                            string unit = spanTable.Rows[t].Columns[c].ToNodePlainString();
                                            if (unit == bidUnit)
                                            {
                                                try
                                                {
                                                    bidMoney = spanTable.Rows[t].Columns[c + 1].ToNodePlainString().Replace(",", "").GetMoney();
                                                }
                                                catch { }
                                                isBreak = true;
                                                break;
                                            }
                                        }
                                        if (isBreak)
                                            break;
                                    }
                                }
                            }
                            else if (spanNode != null && spanNode.Count > 0)
                            {
                                TableTag spanTable = spanNode[0] as TableTag;
                                string ctx = string.Empty;
                                for (int t = 1; t < spanTable.RowCount; t++)
                                {
                                    for (int c = 0; c < spanTable.Rows[t].ColumnCount; c++)
                                    {
                                        ctx += spanTable.Rows[0].Columns[c].ToNodePlainString() + "：" + spanTable.Rows[t].Columns[c].ToNodePlainString() + "\r\n";
                                    }
                                }
                                bidUnit = ctx.GetBidRegex();
                                bidMoney = ctx.GetMoneyRegex();
                            }
                            else
                            {
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex();
                            }
                            if (bidCtx.Contains("项目编号："))
                            {
                                Regex regCode = new Regex(@"\w{14}");
                                code = regCode.Match(bidCtx.Substring(bidCtx.IndexOf("项目编号："))).Value.Trim();
                            }
                            buildUnit = bidCtx.GetBuildRegex();

                            bidCtx = bidCtx.Replace("<ahref=", "").Replace("/service/", "").Replace("</a>", "").Replace("您是第", "").Replace("位访问者粤ICP备06000803号", "").Replace(">", "").Trim();
                            bidCtx = bidCtx.Replace("&lt;chsdatest=&quot;on&quot;year=&quot;2012&quot;month=&quot;01&quot;day=&quot;16&quot;islunardate=&quot;False&quot;isrocdate=&quot;False&quot;&gt;", "").Replace("&lt;/chsdate&gt;", "").Trim();
                            prjName = prjName.Replace("&ldquo;", "").Replace("&rdquo;", "").Trim();
                            msgType = "深圳市盐田区政府采购中心";
                            specType = "建设工程";
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            prjName = ToolDb.GetPrjName(prjName);
                            if (Encoding.Default.GetByteCount(code) > 50)
                                code = "";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳区及街道工程", "", string.Empty, code, prjName, buildUnit, beginDate,
                                       bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                       bidMoney, InfoUrl, prjMgr, htmldttext);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                        else
                        {
                            BidInfo info = GetBidInfo(prjName, InfoUrl, beginDate, htmldetail);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }


        private BidInfo GetBidInfo(string itemName, string url,string beg,string html)
        {
            string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
            Parser parser = new Parser(new Lexer(html));
            NodeList dtnodeHTML = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
            HtmlTxt = dtnodeHTML.AsHtml();
            
            prjName = itemName;
            beginDate = beg;
            InfoUrl = url;

            bidCtx = HtmlTxt.ToCtxString().Replace("\r\n\r\n\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\t\r\n\t\r\n", "\r\n\t").Replace("\r\n\t\r\n\t\r\n", "\r\n\t").Replace("\r\n\t\r\n\t\r\n", "\r\n\t").Replace("\r\n\t\r\n\t\r\n", "\r\n\t");
            bool isOk = true;
            bidCtx = System.Web.HttpUtility.HtmlDecode(bidCtx);
            while (isOk)
            {
                string str = bidCtx.GetRegexBegEnd("&#", ";");
                if (!string.IsNullOrEmpty(str))
                {
                    bidCtx = bidCtx.Replace("&#" + str + ";", "");
                }
                else
                    isOk = false;
            }

            buildUnit = bidCtx.GetBuildRegex();
            prjAddress = bidCtx.GetAddressRegex();
            bidUnit = bidCtx.GetBidRegex();
            bidMoney = bidCtx.GetMoneyRegex();
            if (!string.IsNullOrEmpty(bidUnit) && bidMoney == "0")
            {
                bidMoney = bidCtx.GetMoneyRegex(null, true, "万元");
            }
            string ctx = string.Empty;
            #region 多table匹配
            if (string.IsNullOrEmpty(bidUnit))
            {
                parser = new Parser(new Lexer(html));
                NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "holder")), true), new TagNameFilter("table")));
                if (dtList != null && dtList.Count > 0)
                {
                    for (int c = 0; c < dtList.Count; c++)
                    {
                        TableTag tab = dtList[c] as TableTag;
                        if (IsTableBid(tab))
                        {
                            for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                            {
                                try
                                {
                                    ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                    ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                }
                                catch { }
                            }
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(ctx))
                    {
                        if (dtList.Count > 3)
                        {
                            TableTag tab = dtList[2] as TableTag;
                            if (tab.RowCount > 1)
                            {
                                for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                {
                                    ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                    ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                }
                            }
                            if (!ctx.Contains("投标供应商") || !ctx.Contains("成交供应商") || !ctx.Contains("中标供应商"))
                            {
                                ctx = string.Empty;
                                tab = dtList[1] as TableTag;
                                if (tab.RowCount > 1)
                                {
                                    for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                    {
                                        ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                        ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                    }
                                }
                            }
                        }
                        else if (dtList.Count > 2)
                        {
                            TableTag tab = dtList[1] as TableTag;
                            if (tab.RowCount > 1)
                            {
                                for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                {
                                    ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                    ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                }
                            }
                        }
                        else
                        {
                            TableTag tab = dtList[0] as TableTag;
                            if (tab.RowCount > 1)
                            {

                                for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                {
                                    string start = System.Web.HttpUtility.HtmlDecode(tab.Rows[0].Columns[d].ToNodePlainString());
                                    string end = System.Web.HttpUtility.HtmlDecode(tab.Rows[1].Columns[d].ToNodePlainString());
                                    ctx += start + "：";
                                    ctx += end + "\r\n";
                                }
                            }
                        }
                    }
                    bidUnit = ctx.GetBidRegex();
                    bidMoney = ctx.GetMoneyRegex(new string[] { "成交金额" });
                    if (bidMoney == "" || bidMoney == "0")
                        bidMoney = ctx.GetMoneyRegex();
                    if (!string.IsNullOrEmpty(bidUnit) && bidMoney == "0")
                    {
                        string dtlCtx = string.Empty, unit = string.Empty, money = string.Empty;
                        TableTag tab = dtList[0] as TableTag;
                        for (int c = 0; c < tab.RowCount; c++)
                        {
                            if ((c + 2) <= tab.RowCount)
                            {
                                if (tab.Rows[c].ToNodePlainString().Contains(bidUnit))
                                {
                                    for (int d = 0; d < tab.Rows[c].ColumnCount; d++)
                                    {
                                        dtlCtx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                        dtlCtx += tab.Rows[c].Columns[d].ToNodePlainString() + "\r\n";
                                    }
                                    break;
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(dtlCtx))
                        {
                            Parser tableParser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = tableParser.ExtractAllNodesThatMatch(new TagNameFilter("table"));

                            if (string.IsNullOrEmpty(dtlCtx) && tableNode.Count > 1)
                            {
                                tab = tableNode[1] as TableTag;
                                for (int c = 0; c < tab.RowCount; c++)
                                {
                                    if ((c + 2) <= tab.RowCount)
                                    {
                                        if (tab.Rows[c].ToNodePlainString().Contains(bidUnit))
                                        {
                                            for (int d = 0; d < tab.Rows[c].ColumnCount; d++)
                                            {
                                                dtlCtx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                dtlCtx += tab.Rows[c].Columns[d].ToNodePlainString() + "\r\n";
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(dtlCtx) && tableNode.Count > 2)
                            {
                                tab = tableNode[2] as TableTag;
                                for (int c = 0; c < tab.RowCount; c++)
                                {
                                    if ((c + 2) <= tab.RowCount)
                                    {
                                        if (tab.Rows[c].ToNodePlainString().Contains(bidUnit))
                                        {
                                            for (int d = 0; d < tab.Rows[c].ColumnCount; d++)
                                            {
                                                dtlCtx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                dtlCtx += tab.Rows[c].Columns[d].ToNodePlainString() + "\r\n";
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(dtlCtx) && tableNode.Count > 3)
                            {
                                tab = tableNode[3] as TableTag;
                                for (int c = 0; c < tab.RowCount; c++)
                                {
                                    if ((c + 2) <= tab.RowCount)
                                    {
                                        if (tab.Rows[c].ToNodePlainString().Contains(bidUnit))
                                        {
                                            for (int d = 0; d < tab.Rows[c].ColumnCount; d++)
                                            {
                                                dtlCtx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                dtlCtx += tab.Rows[c].Columns[d].ToNodePlainString() + "\r\n";
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(dtlCtx) && tableNode.Count > 4)
                            {
                                tab = tableNode[4] as TableTag;
                                for (int c = 0; c < tab.RowCount; c++)
                                {
                                    if ((c + 2) <= tab.RowCount)
                                    {
                                        if (tab.Rows[c].ToNodePlainString().Contains(bidUnit))
                                        {
                                            for (int d = 0; d < tab.Rows[c].ColumnCount; d++)
                                            {
                                                dtlCtx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                dtlCtx += tab.Rows[c].Columns[d].ToNodePlainString() + "\r\n";
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(dtlCtx) && tableNode.Count > 5)
                            {
                                tab = tableNode[5] as TableTag;
                                for (int c = 0; c < tab.RowCount; c++)
                                {
                                    if ((c + 2) <= tab.RowCount)
                                    {
                                        if (tab.Rows[c].ToNodePlainString().Contains(bidUnit))
                                        {
                                            for (int d = 0; d < tab.Rows[c].ColumnCount; d++)
                                            {
                                                dtlCtx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                dtlCtx += tab.Rows[c].Columns[d].ToNodePlainString() + "\r\n";
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        unit = dtlCtx.GetBidRegex();
                        money = dtlCtx.GetMoneyRegex();
                        if (bidUnit == unit)
                        {
                            bidMoney = money;
                        }
                    }
                    if (bidUnit.Contains("无中标") || bidUnit.Contains("没有"))
                    {
                        bidUnit = "没有中标商";
                        bidMoney = "0";
                    }
                }
            }
            if (string.IsNullOrEmpty(bidUnit))
            {
                parser = new Parser(new Lexer(html));
                NodeList dtList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                if (dtList != null && dtList.Count > 0)
                {
                    for (int c = 0; c < dtList.Count; c++)
                    {
                        TableTag tab = dtList[c] as TableTag;
                        if (IsTableBid(tab))
                        {
                            for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                            {
                                try
                                {
                                    ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                    ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                }
                                catch { }
                            }
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(ctx))
                    {
                        if (dtList.Count > 3)
                        {
                            TableTag tab = dtList[2] as TableTag;
                            if (tab.RowCount > 1)
                            {
                                for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                {
                                    try
                                    {
                                        ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                        ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                    }
                                    catch { }
                                }
                            }
                        }
                        else if (dtList.Count > 2)
                        {
                            TableTag tab = dtList[1] as TableTag;
                            if (tab.RowCount > 1)
                            {
                                for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                {
                                    try
                                    {
                                        ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                        ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                    }
                                    catch { }
                                }
                            }
                        }
                        else if (dtList.Count > 1)
                        {
                            TableTag tab = dtList[1] as TableTag;
                            if (tab.RowCount > 1)
                            {
                                for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                {
                                    try
                                    {
                                        ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                        ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                    }
                                    catch { }
                                }
                            }
                        }
                        else
                        {
                            TableTag tab = dtList[0] as TableTag;
                            if (tab.RowCount > 1)
                            {
                                for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                {
                                    try
                                    {
                                        ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                        ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    bidUnit = ctx.GetBidRegex();
                    if (string.IsNullOrEmpty(bidUnit))
                        bidUnit = ctx.GetRegex("中标承包商");
                    bidMoney = ctx.GetMoneyRegex();
                    if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                        bidMoney = bidCtx.GetRegex("中标价").GetMoney();
                    if (string.IsNullOrEmpty(bidUnit))
                    {
                        if (dtList.Count > 4)
                        {
                            TableTag tab = dtList[dtList.Count - 1] as TableTag;
                            if (tab.RowCount > 1)
                            {
                                for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                {
                                    try
                                    {
                                        ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                        ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                    }
                                    catch { }
                                }
                            }
                        }
                        bidUnit = ctx.GetBidRegex();
                        if (string.IsNullOrEmpty(bidUnit))
                            bidUnit = ctx.GetRegex("中标承包商");
                        bidMoney = ctx.GetMoneyRegex();
                        if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                            bidMoney = bidCtx.GetRegex("中标价").GetMoney();
                    }
                    if (bidUnit.Contains("无中标") || bidUnit.Contains("没有"))
                    {
                        bidUnit = "没有中标商";
                        bidMoney = "0";
                    }
                }
            }
            #endregion
            if (string.IsNullOrEmpty(bidUnit))
            {
                if (bidCtx.Contains("供应商不足"))
                {
                    bidUnit = "没有中标商";
                    bidMoney = "0";
                }
            }
            if (bidMoney != "0")
            {
                try
                {
                    decimal mon = decimal.Parse(bidMoney);
                    if (mon > 100000)
                    {
                        bidMoney = bidMoney.GetMoney();
                    }

                }
                catch { }
            }
            bidType = prjName.GetInviteBidType();
            string[] CodeRegex = { "工程编号", "项目编号", "招标编号", "中标编号" };
            code = bidCtx.GetCodeRegex(CodeRegex).GetCodeDel();
     
            if (string.IsNullOrEmpty(code))
            {
                code = bidCtx.Replace("）", "kdxx").Replace(")", "kdxx").GetRegexBegEnd("招标编号", "kdxx").Replace("：", "").Replace(":", "");
            }
            if (string.IsNullOrEmpty(code))
            {
                code = bidCtx.Replace("）", "kdxx").Replace(")", "kdxx").GetRegexBegEnd("项目编号", "kdxx").Replace("：", "").Replace(":", "");
            }
            if (string.IsNullOrEmpty(code))
            {
                code = bidCtx.Replace("）", "kdxx").Replace(")", "kdxx").GetRegexBegEnd("工程编号", "kdxx").Replace("：", "").Replace(":", "");
            }
            if (string.IsNullOrEmpty(code))
            {
                code = bidCtx.Replace("）", "kdxx").Replace(")", "kdxx").GetRegexBegEnd("编号", "kdxx").Replace("：", "").Replace(":", "");
            }
            if (Encoding.Default.GetByteCount(code) > 50)
            {
                code = string.Empty;
            }
            if (!string.IsNullOrEmpty(code))
            {
                code = code.GetChina();
            }

            msgType = "深圳市盐田区政府采购中心";
            specType = "建设工程";
            bidType = ToolHtml.GetInviteTypes(prjName);
            prjName = ToolDb.GetPrjName(prjName);
            if (Encoding.Default.GetByteCount(code) > 50)
                code = "";
            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳区及街道工程", "", string.Empty, code, prjName, buildUnit, beginDate,
                       bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                       bidMoney, InfoUrl, prjMgr, HtmlTxt);

            return info;
        }

        private bool IsTableBid(TableTag table)
        {
            Parser tableparser = new Parser(new Lexer(table.ToHtml()));
            NodeList nodeList = tableparser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
            if (nodeList != null && nodeList.Count > 1) return false;
            for (int i = 0; i < table.RowCount; i++)
            {
                if (table.Rows[i].ToNodePlainString().Contains("中标供应商")
                    || table.Rows[i].ToNodePlainString().Contains("成交供应商")
                    || table.Rows[i].ToNodePlainString().Contains("中标单位"))
                    return true;
            }
            return false;
        }
    }
}
