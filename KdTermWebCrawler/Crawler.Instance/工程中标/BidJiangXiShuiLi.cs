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
    public class BidJiangXiShuiLi : WebSiteCrawller
    {
        public BidJiangXiShuiLi()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "江西省公共资源交易网中标信息(水利工程)";
            this.Description = "自动抓取江西省公共资源交易网中标信息(水利工程)";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.jxsggzy.cn/web/jyxx/002003/002003004/jyxx.html";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "wb-page-li")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("/", "\r");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {

                SiteUrl = "http://www.jxsggzy.cn/web/jyxx/002003/002003004/" + i + ".html";
                try
                {

                    html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
                }
                catch { continue; }
                
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "ewb-list-node clearfix")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
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
                            prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        ATag aTag = listNode[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        if (string.IsNullOrWhiteSpace(prjName))
                            prjName = aTag.LinkText;
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        if (prjName[2].Equals('县') || prjName[2].Equals('区') || prjName[2].Equals('市'))
                            area = prjName.Substring(0, 3);
                        InfoUrl = "http://www.jxsggzy.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "article-info")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml().Replace("<br />", "").Replace("<br/>", "").Replace("<br>", "");
                            bidCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList dtlBidNode = parser.ExtractAllNodesThatMatch(new AndFilter( new TagNameFilter("table"),new HasAttributeFilter("class","MsoNormalTable")));
                            if (dtlBidNode != null && dtlBidNode.Count > 0)
                            {
                                TableTag bidTable = dtlBidNode[dtlBidNode.Count-1] as TableTag;
                                string ctx = string.Empty;
                                if (bidTable.RowCount > 1)
                                {
                                    for (int r = 0; r < bidTable.Rows[0].ColumnCount; r++)
                                    {
                                        ctx += bidTable.Rows[0].Columns[r].ToNodePlainString() + "：";
                                        try
                                        {
                                            ctx += bidTable.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                        }
                                        catch { }
                                    }
                                }
                                
                                prjAddress = ctx.GetAddressRegex();
                                buildUnit = ctx.GetBuildRegex();
                                bidUnit = ctx.GetBidRegex();
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = ctx.GetRegex("第一排序人,投标人名称,中标候选人名单,中标供应商名称,中标供应商");
                                if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                    bidMoney = ctx.GetRegex("投标报价,中标金（人民币）", false).GetMoney();
                                prjMgr = ctx.GetMgrRegex();
                                if(string.IsNullOrEmpty(prjMgr))
                                    prjMgr = ctx.GetRegex("项目负责人");
                                code = ctx.GetCodeRegex();

                                if (bidUnit.Contains("投标报价")||string.IsNullOrEmpty(bidUnit))
                                {
                                    ctx = string.Empty;
                                    for (int r = 0; r < bidTable.RowCount; r++)
                                    {
                                        for (int c = 0; c < bidTable.Rows[r].ColumnCount; c++)
                                        {
                                            string temp = bidTable.Rows[r].Columns[c].ToNodePlainString();
                                            if (string.IsNullOrEmpty(temp)) continue;
                                            if ((c + 1) % 2 == 0)
                                                ctx += temp + "\r\n";
                                            else
                                                ctx += temp + "：";
                                        }
                                    }
                                    prjAddress = ctx.GetAddressRegex();
                                    buildUnit = ctx.GetBuildRegex();
                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("第一排序人,投标人名称,中标候选人名单,中标供应商名称,中标供应商");
                                    if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                        bidMoney = ctx.GetRegex("投标报价,中标金（人民币）", false).GetMoney();
                                    prjMgr = ctx.GetMgrRegex();
                                    if (string.IsNullOrEmpty(prjMgr))
                                        prjMgr = ctx.GetRegex("项目负责人");
                                    code = ctx.GetCodeRegex();
                                }
                                if (string.IsNullOrEmpty(bidUnit) && (string.IsNullOrEmpty(bidMoney) || bidMoney == "0"))
                                {
                                    //MsoTableGrid
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList bidNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoTableGrid")));
                                    if (bidNode != null && bidNode.Count > 0)
                                    {
                                        TableTag bidDt = dtlBidNode[dtlBidNode.Count - 1] as TableTag;
                                        ctx = string.Empty;
                                        if (bidDt.RowCount > 1)
                                        {
                                            for (int r = 0; r < bidDt.Rows[0].ColumnCount; r++)
                                            {
                                                ctx += bidDt.Rows[0].Columns[r].ToNodePlainString() + "：";
                                                try
                                                {
                                                    ctx += bidDt.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                        }
                                        prjAddress = ctx.GetAddressRegex();
                                        buildUnit = ctx.GetBuildRegex();
                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrEmpty(bidUnit))
                                            bidUnit = ctx.GetRegex("第一排序人,投标人名称,中标候选人名单,中标供应商名称,中标供应商");
                                        if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                            bidMoney = ctx.GetRegex("投标报价,中标金（人民币）", false).GetMoney();
                                        prjMgr = ctx.GetMgrRegex();
                                        if (string.IsNullOrEmpty(prjMgr))
                                            prjMgr = ctx.GetRegex("项目负责人");
                                        code = ctx.GetCodeRegex();
                                        if (bidUnit.Contains("投标报价") || string.IsNullOrEmpty(bidUnit))
                                        {
                                            ctx = string.Empty;
                                            for (int r = 0; r < bidTable.RowCount; r++)
                                            {
                                                for (int c = 0; c < bidTable.Rows[r].ColumnCount; c++)
                                                {
                                                    string temp = bidTable.Rows[r].Columns[c].ToNodePlainString();
                                                    if (string.IsNullOrEmpty(temp)) continue;
                                                    if ((c + 1) % 2 == 0)
                                                        ctx += temp + "\r\n";
                                                    else
                                                        ctx += temp + "：";
                                                }
                                            }
                                            prjAddress = ctx.GetAddressRegex();
                                            buildUnit = ctx.GetBuildRegex();
                                            bidUnit = ctx.GetBidRegex();
                                            if (string.IsNullOrEmpty(bidUnit))
                                                bidUnit = ctx.GetRegex("第一排序人,投标人名称,中标候选人名单,中标供应商名称,中标供应商");
                                            if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                                bidMoney = ctx.GetRegex("投标报价,中标金（人民币）", false).GetMoney();
                                            prjMgr = ctx.GetMgrRegex();
                                            if (string.IsNullOrEmpty(prjMgr))
                                                prjMgr = ctx.GetRegex("项目负责人");
                                            code = ctx.GetCodeRegex();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                prjAddress = bidCtx.GetAddressRegex();
                                buildUnit = bidCtx.GetBuildRegex();
                                bidUnit = bidCtx.GetBidRegex();
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = bidCtx.GetRegex("第一中标排序人");
                                bidMoney = bidCtx.GetMoneyRegex();
                                prjMgr = bidCtx.GetMgrRegex();
                                if (string.IsNullOrEmpty(prjMgr))
                                    prjMgr = bidCtx.GetRegex("注册监理工程师");
                                code = bidCtx.GetCodeRegex();
                            }
                            if (prjMgr.Contains("编号") || prjMgr.Contains("名称"))
                                prjMgr = string.Empty;
                            if (string.IsNullOrEmpty(bidMoney))
                            {
                                try
                                {
                                    if (decimal.Parse(bidMoney) > 10000)
                                    {
                                        bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                                    } 
                                }
                                catch { }
                            }
                            bidType = "水利工程";
                            specType = "政府采购";
                            msgType = "江西省公共资源交易中心";
                            BidInfo info = ToolDb.GenBidInfo("江西省", "江西省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
