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
    public class BidSZZFCG : WebSiteCrawller
    {
        public BidSZZFCG()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "广东省深圳市政府采购";
            this.Description = "自动抓取广东省深圳市政府采购中标信息";
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,10:20,10:50,11:30,14:05,14:25,14:50,15:25,16:00,16:50,19:00";
            this.SiteUrl = "http://www.szzfcg.cn/portal/topicView.do?method=view&id=2014";
            this.MaxCount = 50;
            this.ExistsHtlCtx = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            //string url = "http://183.56.159.138:8090/services/Sms?wsdl";

            //string strHtml = this.ToolWebSite.GetHtmlByUrl(url);

            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("name", "__ec_pages")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    SelectTag selectTag = pageNode[0] as SelectTag;
                    pageInt = selectTag.OptionTags.Length;
                }
                catch { }
            }
            string cookiestr = string.Empty;
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "ec_i", "topicChrList_20070702_crd", "topicChrList_20070702_f_a", "topicChrList_20070702_p", "topicChrList_20070702_s_name", "id", "method", "__ec_pages", "topicChrList_20070702_rd", "topicChrList_20070702_f_name", "topicChrList_20070702_f_ldate" }, new string[] { "topicChrList_20070702", "20", string.Empty, i.ToString(), string.Empty, "2014", "view", (i - 1).ToString(), "20", string.Empty, string.Empty });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { }
                }
                parser = new Parser(new Lexer(html));
                NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "topicChrList_20070702_table")));
                if (tdNodes != null && tdNodes.Count > 0)
                {
                    TableTag table = tdNodes[0] as TableTag;
                    for (int t = 3; t < table.RowCount; t++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[t];
                        prjName = tr.Columns[2].ToPlainTextString().Trim();
                        beginDate = tr.Columns[4].ToPlainTextString().Trim();
                        Regex regexLink = new Regex(@"id=[^-]+");
                        InfoUrl = "http://www.szzfcg.cn/portal/documentView.do?method=view&" + regexLink.Match(tr.Columns[2].GetATagHref()).Value;
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString().Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");

                        }
                        catch { continue; }
                        Parser dtlparserHTML = new Parser(new Lexer(htldtl));
                        NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtnodeHTML == null || dtnodeHTML.Count < 1)
                        {
                            try
                            {
                                htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString().Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                            }
                            catch { }
                            dtlparserHTML = new Parser(new Lexer(htldtl));
                            dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        }
                        HtmlTxt = dtnodeHTML.AsHtml();
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
                        msgType = "深圳政府采购";
                        specType = "政府采购";
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
                        prjName = prjName.GetBidPrjName();
                        code = code.Replace("（", "").Replace("(", "").Replace("）", "").Replace(")", "");
                        BidInfo info = ToolDb.GenBidInfo("广东省", "深圳政府采购", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        list.Add(info);
                        parser = new Parser(new Lexer(HtmlTxt));
                        NodeList fileNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        if (fileNode != null && fileNode.Count > 0)
                        {
                            for (int f = 0; f < fileNode.Count; f++)
                            {
                                ATag tag = fileNode[f] as ATag;
                                if (tag.IsAtagAttach())
                                {
                                    string alink = string.Empty;
                                    if (!tag.Link.ToLower().Contains("http"))
                                        alink = "http://www.szzfcg.cn" + tag.Link.Replace("&amp;", "&");
                                    else
                                        alink = tag.Link.Replace("&amp;", "&");
                                    BaseAttach attach = ToolDb.GenBaseAttach(tag.Link.Replace("&amp;", "&"), info.Id, alink);
                                    base.AttachList.Add(attach);
                                }
                            }
                        }
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
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
                    || table.Rows[i].ToNodePlainString().Contains("中标单位"))
                    return true;
            }
            return false;
        }
    }
}
