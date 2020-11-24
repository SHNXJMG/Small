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
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class BidSzPsXxgcZfcg : WebSiteCrawller
    {
        public BidSzPsXxgcZfcg()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "深圳市坪山新区公共资源交易中心小型工程中标信息（施工）";
            this.Description = "自动抓取深圳市坪山新区公共资源交易中心小型工程中标信息（施工）";
            this.PlanTime = "9:22,13:53";
            this.SiteUrl = "http://ps.szzfcg.cn/portal/topicView.do?method=view1&id=2887104&siteId=9&underwayFlag=undefined&tstmp=17%3A24%3A26%20GMT%2B0800&page=1";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "clearfix")), true), new TagNameFilter("a")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[sNode.Count - 1].GetATag().GetAttribute("onclick").Replace("(", "kdxx").Replace(",", "xxdk");
                    pageInt = int.Parse(temp.GetRegexBegEnd("kdxx", "xxdk"));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://ps.szzfcg.cn/portal/topicView.do?method=view1&id=2887104&siteId=9&underwayFlag=undefined&tstmp=17%3A24%3A26%20GMT%2B0800&page=" + i, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "fixed")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        beginDate = tr.Columns[1].ToNodePlainString().GetDateRegex("yyyy/MM/dd");
                        ATag aTag = tr.Columns[0].GetATag();
                        prjName = aTag.GetAttribute("title");

                        Regex regexLink = new Regex(@"id=[^-]+");
                        string id = regexLink.Match(aTag.Link).Value;
                        InfoUrl = "http://ps.szzfcg.cn/portal/documentView.do?method=view&" + id;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
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
                            if (!string.IsNullOrEmpty(bidUnit) && (string.IsNullOrEmpty(bidMoney) || bidMoney == "0"))
                            {
                                bidMoney = bidCtx.GetMoneyRegex(new string[] { "中标金额", "金额" }, false, "万元");
                            }
                            string ctx = string.Empty;
                            #region 多table匹配
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(htmldtl));
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
                                parser = new Parser(new Lexer(htmldtl));
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
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("中标（成交）供应商");
                                    if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
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
                                        if (string.IsNullOrEmpty(bidUnit))
                                            bidUnit = ctx.GetRegex("中标（成交）供应商");
                                        if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
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
                            bidUnit = bidUnit.Replace("名称", "");
                            code = code.Replace("（", "").Replace("(", "").Replace("）", "").Replace(")", "");
                            msgType = "深圳市坪山新区公共资源交易中心";
                            specType = "政府采购";
                            bidType = "施工";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳政府采购", "坪山新区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);

                            list.Add(info);

                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aTagNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aTagNode != null && aTagNode.Count > 0)
                            {
                                for (int k = 0; k < aTagNode.Count; k++)
                                {
                                    ATag aFile = aTagNode[k].GetATag();
                                    if (aFile.IsAtagAttach() || aFile.Link.ToLower().Contains("down"))
                                    {
                                        string link = string.Empty;
                                        if (aFile.Link.Contains("http"))
                                            link = aFile.Link;
                                        else
                                            link = "http://ps.szzfcg.cn/" + aFile.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(aFile.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
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
                    || table.Rows[i].ToNodePlainString().Contains("中标单位")
                    || table.Rows[i].ToNodePlainString().Contains("中标（成交）供应商"))
                    return true;
            }
            return false;
        }
    }
}
