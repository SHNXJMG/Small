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
    public class BidAnHuiJsgc : WebSiteCrawller
    {
        public BidAnHuiJsgc()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "安徽省建设工程招标投投标中标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取安徽省建设工程招标投投标中标信息";
            this.SiteUrl = "http://www.act.org.cn/News.Asp?pid=171";
            this.MaxCount = 400;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "Page")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("页数：", "页").ToNodeString().GetReplace("|");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "sublist_list_ul")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
          bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;
                        prjName = aTag.LinkText;
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.act.org.cn/" + aTag.Link.GetReplace("amp;");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl.ToLower().GetReplace("<div", "<div ").GetReplace("<span", "<span ").GetReplace("<p", "<p ").GetReplace("<table", "<table ").GetReplace("<tr", "<tr ").GetReplace("<td", "<td ").GetReplace("<th", "<th ")));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "subcont_cont")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();

                            bidCtx = HtmlTxt.ToLower().GetReplace("<br/>,<br>,</p>", "\r\n").GetReplace("?").ToCtxString();
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList htmlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "subcont_cont")));
                            if (htmlNode != null && htmlNode.Count > 0)
                            {
                                string strHtml = htmlNode.AsHtml();
                                if (strHtml.Length < 600)
                                    HtmlTxt = bidCtx;
                            } 
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                string ctx = string.Empty;
                                TableTag tag = tableNode[0] as TableTag;
                                for (int r = 0; r < tag.RowCount; r++)
                                {
                                    for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = tag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                        if ((c + 1) % 2 == 0)
                                        {
                                            ctx += temp + "\r\n";
                                        }
                                        else
                                            ctx += temp + "：";
                                    }
                                }
                                buildUnit = ctx.GetBuildRegex();
                                code = ctx.GetCodeRegex();
                                bidUnit = ctx.GetBidRegex(null, true, 200).GetReplace("第一");
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = ctx.GetRegex("中标候选人名称", true, 200).GetReplace("第一,中标候选人");
                                bidMoney = ctx.GetMoneyRegex();
                                prjMgr = ctx.GetMgrRegex().GetReplace("第一中标候选单位");
                                if (string.IsNullOrEmpty(prjMgr))
                                    prjMgr = ctx.GetMgrRegex(null, false).GetReplace("第一中标候选单位");
                                if (bidUnit.Contains("单位") && !bidUnit.Contains("公司"))
                                {
                                    bidUnit = ctx.GetRegexBegEnd("单位名称\r\n", "：");
                                }
                                else if (!bidUnit.Contains("公司"))
                                {
                                    ctx = string.Empty;
                                    try
                                    {
                                        for (int r = 1; r < tag.Rows[4].ColumnCount; r++)
                                        {
                                            string temp = tag.Rows[4].Columns[r].ToNodePlainString().GetReplace(":,：");
                                            ctx += temp + "：";
                                            ctx += tag.Rows[5].Columns[r - 1].ToNodePlainString().GetReplace(":,：") + "\r\n";
                                        }
                                        bidUnit = ctx.GetBidRegex(null, true, 200);
                                        if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                            bidMoney = ctx.GetMoneyRegex();
                                        if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                            bidMoney = ctx.GetRegex("预中标金额").GetMoney();
                                        prjMgr = ctx.GetMgrRegex().GetReplace("第一中标候选单位");
                                        if (string.IsNullOrEmpty(prjMgr))
                                            prjMgr = ctx.GetMgrRegex(null, false).GetReplace("第一中标候选单位");
                                    }
                                    catch { }
                                }
                                if (!bidUnit.Contains("公司"))
                                {
                                    bidUnit = prjMgr = "";
                                }
                            }
                            else
                            {
                                string ctx = HtmlTxt.ToLower().GetReplace("</p>,<br/>,<br>", "\r\n").ToCtxString();
                                buildUnit = ctx.GetBuildRegex();
                                code = ctx.GetCodeRegex().GetCodeDel();
                                bidUnit = ctx.GetBidRegex().GetReplace("第一");
                                bidMoney = ctx.GetMoneyRegex();
                                if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                    bidMoney = ctx.GetRegex("预中标金额").GetMoney();
                                prjMgr = ctx.GetMgrRegex().GetReplace("第一中标候选单位");
                                if (string.IsNullOrEmpty(prjMgr))
                                    prjMgr = ctx.GetMgrRegex(null, false).GetReplace("第一中标候选单位");
                            }
                            if (prjMgr.Contains("第二"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("第二"));
                            if (prjMgr.Contains("电话"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("电话"));
                            if (prjMgr.Contains("2"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("2"));
                            if (prjMgr.Contains("注册"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("注册"));
                            if (prjMgr.Contains("投标")||prjMgr.IsNumber())
                                prjMgr = "";
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            bidUnit = bidUnit.GetReplace("名称,1,、");
                            prjMgr = prjMgr.GetReplace("1,、,一,第一中标人,第一中标,第中标人,第名").GetCodeDel();
                            try
                            {
                                if (Convert.ToDecimal(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            msgType = "安徽省建设工程招标投标办公室";
                            specType = bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("安徽省", "安徽省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
