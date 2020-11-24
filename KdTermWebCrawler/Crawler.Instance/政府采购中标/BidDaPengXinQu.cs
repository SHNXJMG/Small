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
using System.IO;
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class BidDaPengXinQu : WebSiteCrawller
    {
        public BidDaPengXinQu()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "大鹏新区政府在线中标采购";
            this.Description = "自动抓取大鹏新区政府在线中标采购";
            this.PlanTime = "21:08,11:12,14:02,16:33";
            this.SiteUrl = "http://dp.szzfcg.cn/portal/topicView.do?method=view&siteId=10&id=2014";
            this.MaxCount = 100;
            this.ExistCompareFields = "InfoUrl";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "statusBar")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string pageos = pageNode.AsString().GetRegexBegEnd("找到","条");
                try
                {
                    pageInt = int.Parse(pageos.Replace(",",""));
                    //string temp = pageNode.AsString().GetRegexBegEnd("/", "页");
                    //pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://dp.szzfcg.cn/portal/topicView.do?method=view&siteId=10&id=2014", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "topicChrList_20070702_table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 3; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty,
                            bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty,
                            beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty,
                            specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty,
                            bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty,
                            prjMgr = string.Empty, otherType = string.Empty, 
                            HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.GetATag(); 
                        if (aTag == null)
                            continue;
                        beginDate = tr.ToString().GetDateRegex(); 

                        prjName = aTag.LinkText.ToNodeString();

                        string itemName = aTag.Link.ToString().Replace("/viewer.do?id=", "");
                        InfoUrl = "http://dp.szzfcg.cn/portal/documentView.do?method=view&id=" + itemName;
                        string htmlDtl = string.Empty;
                        try
                        {
                            htmlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmlDtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "98%")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString().Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");

                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            bidType = prjName.GetInviteBidType();
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList bidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (bidNode != null && bidNode.Count > 0)
                                {
                                    if (bidNode.Count == 2 || bidNode.Count == 3)
                                    {
                                        string unitCtx = string.Empty;
                                        //  TableTag table = null;
                                        if (bidNode.Count == 2) table = bidNode[1] as TableTag;
                                        if (bidNode.Count == 3) table = bidNode[2] as TableTag;
                                        if (table.RowCount > 1)
                                        {
                                            for (int k = 0; k < table.Rows[3].ColumnCount; k++)
                                            {
                                                unitCtx += table.Rows[3].Columns[k].ToNodePlainString() + "：";
                                                unitCtx += table.Rows[4].Columns[k].ToNodePlainString() + "\r\n";
                                            }
                                        }
                                        bidUnit = unitCtx.GetBidRegex();
                                        bidMoney = unitCtx.GetMoneyRegex();
                                    }
                                    if (bidNode.Count == 5)
                                    {
                                        string unitCtx = string.Empty;
                                        TableTag table1 = bidNode[3] as TableTag;
                                        TableTag table2 = bidNode[1] as TableTag;
                                        if (table1.RowCount > 1)
                                        {
                                            for (int k = 0; k < table1.Rows[0].ColumnCount; k++)
                                            {
                                                unitCtx += table1.Rows[0].Columns[k].ToNodePlainString() + "：";
                                                unitCtx += table1.Rows[1].Columns[k].ToNodePlainString() + "\r\n";
                                            }
                                        }
                                        bidUnit = unitCtx.GetRegex("中标（成交）供应商");
                                        if (string.IsNullOrEmpty(bidUnit))
                                            unitCtx.GetBidRegex();
                                        if (string.IsNullOrEmpty(bidUnit)) bidUnit = unitCtx.GetRegex("供应商");
                                        if (table2.RowCount > 1)
                                        {
                                            bool isOk = false;
                                            for (int h = 0; h < table2.RowCount; h++)
                                            {
                                                string monCtx = string.Empty;
                                                for (int k = 0; k < table2.Rows[h].ColumnCount; k++)
                                                {
                                                    if (bidUnit == table2.Rows[h].Columns[k].ToNodePlainString())
                                                    {
                                                        bidMoney = table2.Rows[h].Columns[table2.Rows[h].ColumnCount - 1].ToNodePlainString();
                                                        isOk = true;
                                                        break;
                                                    }
                                                }
                                                if (isOk) break;
                                            }
                                        }
                                        if (table2.ToHtml().Contains("万")) bidMoney = (bidMoney + "万").GetMoney();
                                        else bidMoney = bidMoney.GetMoney();
                                    }
                                }
                            }
                            if (bidUnit.Contains("没有")) bidUnit = "没有中标商";
                            if (buildUnit.Contains("没有")) buildUnit = "";
                            specType = "建设工程";
                            msgType = "大鹏新区公共资源交易中心";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳政府采购", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
