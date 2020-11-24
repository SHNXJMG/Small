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
using System.Web.Script.Serialization;

namespace Crawler.Instance
{
    public class BidSzDpGgzyZfcg : WebSiteCrawller
    {
        public BidSzDpGgzyZfcg()
            : base()
        {
            this.Group = "街道办中标信息";
            this.Title = "深圳市大鹏新区公共资源交易中心政府采购中标公告";
            this.Description = "自动抓取深圳市大鹏新区公共资源交易中心政府采购中标公告";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://113.105.69.184:51201/info_data/PT001-PT00103?pageIndex=1&pageSize=";
            this.MaxCount = 150;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + MaxCount, Encoding.UTF8);
            }
            catch { return list; }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = 50000000;
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
            object[] dicList = (object[])smsTypeJson["data"];
            foreach (object obj in dicList)
            {
                Dictionary<string, object> dic = obj as Dictionary<string, object>;
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

                prjName = Convert.ToString(dic["TITLE"]);
                beginDate = Convert.ToString(dic["CREATED_ON"]);
                InfoUrl = Convert.ToString(dic["URL"]);
                string htldtl = string.Empty;
                try
                {
                    htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                }
                catch { continue; }
                Parser parser = new Parser(new Lexer(htldtl));
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
                    string ctx = string.Empty;
                    #region 多table匹配
                    if (string.IsNullOrEmpty(bidUnit))
                    {
                        parser = new Parser(new Lexer(htldtl));
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
                        parser = new Parser(new Lexer(htldtl));
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
                    
                    prjName = prjName.Replace("成交", "");
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
                    if (code.Contains("("))
                        code = "";
                    bidType = prjName.GetInviteBidType();
                    specType = "政府采购";
                    msgType = "大鹏新区公共资源交易中心";
                    BidInfo info = ToolDb.GenBidInfo("广东省", "深圳区及街道工程", "大鹏新区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                          bidMoney, InfoUrl, prjMgr, HtmlTxt);
                    list.Add(info);

                    if (!crawlAll && list.Count >= this.MaxCount) return list;
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
                    || table.Rows[i].ToNodePlainString().Contains("中标单位"))
                    return true;
            }
            return false;
        }
    }
}
