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
    public class BidAllSZFT : WebSiteCrawller
    {
        public BidAllSZFT()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "深圳市政府采购全区站点（福田）中标";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市政府采购全区站点（福田）中标";
            this.SiteUrl = "http://61.144.240.26:58080/news/publicnews/12131";
            this.MaxCount = 300;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "paging")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string pageos = pageNode.AsString().GetRegexBegEnd("/", "页");
                try
                {
                    pageInt = int.Parse(pageos);
                    
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://61.144.240.26:58080/news/publicnews/12131?pageIndex="+ i, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("id", "newsList")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                   
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty,
                            bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty,
                            beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty,
                            specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty,
                            bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty,
                            prjMgr = string.Empty, otherType = string.Empty,
                            HtmlTxt = string.Empty;

                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.LinkText.GetReplace("· ", "");
                        if (prjName.Contains("FTCG"))
                            try
                            {
                                code = prjName.Remove(prjName.IndexOf("-"));
                            }
                            catch{}
                       
                        InfoUrl = "http://61.144.240.26:58080" + aTag.Link;
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
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
                            if(string.IsNullOrWhiteSpace(code))
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
                               
                                string Ctx = string.Empty;
                                if (bidNode != null && bidNode.Count > 0)
                                {
                                   
                                    if (bidNode.Count == 2 || bidNode.Count == 3)
                                    {
                                        TableTag table = bidNode[1] as TableTag;
                                        for (int r = 0; r < table.Rows[0].ColumnCount; r++)
                                        {
                                            try
                                            {
                                                Ctx += table.Rows[0].Columns[r].ToNodePlainString() + "：";
                                                Ctx += table.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                            }
                                            catch { }
                                        }
                                        
                                        bidUnit = Ctx.GetBidRegex();
                                        bidMoney = Ctx.GetMoneyRegex();
                                    }
                                    if (bidNode.Count == 6)
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
                            if (string.IsNullOrWhiteSpace(code))
                            {
                                parser = new Parser(new Lexer(htmlDtl));
                                NodeList codeNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "holder")));
                                for (int r = 0; r < codeNode.Count; r++)
                                {
                                    string ctx = string.Empty;
                                    string clnd = codeNode[r].ToNodePlainString();
                                    if (clnd.Contains("FTCG"))
                                    {
                                        try
                                        {
                                            ctx += clnd.ToString();
                                            code = ctx;
                                            break;
                                        }
                                        catch { }
                                    }         
                                }
                             }
                            else
                                code = bidCtx.GetCodeRegex();
                            if (code.Contains("）"))
                                code = code.GetReplace("）", "");
                            if (bidUnit.Contains("没有")) bidUnit = "没有中标商";
                            if (buildUnit.Contains("没有")) buildUnit = "";
                            if (code.Length > 50)
                                code = "";

                            specType = "政府采购";
                            msgType = "深圳政府采购";
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
