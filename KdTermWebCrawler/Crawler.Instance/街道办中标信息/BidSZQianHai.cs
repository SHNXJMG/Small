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
    public class BidSZQianHai : WebSiteCrawller
    {
        public BidSZQianHai()
            : base()
        {
            this.Group = "街道办中标信息";
            this.Title = "深圳市前海深港现代服务业合作区管理局中标公告";
            this.Description = "自动深圳市前海深港现代服务业合作区管理局中标公告";
            this.PlanTime = "9:10,11:10,14:10,16:10,18:10";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 100;
            this.SiteUrl = "http://www.szqh.gov.cn/sygnan/xxgk/xxgkml/zbcg/zbgs/";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page")), true), new TagNameFilter("a")));
            if (sNode != null && sNode.Count > 0)
            {
                string temp = sNode[sNode.Count - 1].ToNodePlainString();
                try
                {
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
                        int emp = i - 1;
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.szqh.gov.cn/sygnan/xxgk/xxgkml/zbcg/zbgs/index_" + emp + ".shtml");
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "gl-news-box-02")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        INode node = nodeList[j];
                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty,
                            bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty,
                            beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty,
                            specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty,
                            bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty,
                            prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty,
                            area = string.Empty;
                        prjName = aTag.GetAttribute("title");
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.szqh.gov.cn/sygnan/xxgk/xxgkml/zbcg/zbgs/" + aTag.Link.GetReplace("./", "");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToLower().ToCtxString();
                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            if (string.IsNullOrWhiteSpace(code))
                                code = bidCtx.GetRegexBegEnd("编号：", "）");
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("中标候选人名称,中签单位,第一成交候选人,成交候选人,中标（成交）供应商1,中标人");
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                bidMoney = bidCtx.GetMoneyRegex(null, true);
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                bidMoney = bidCtx.GetRegex("总额").GetMoney();
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "MsoNormalTable"), new TagNameFilter("table")));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    for (int t = 0; t < tableNode.Count; t++)
                                    {
                                        TableTag table = tableNode[t] as TableTag;
                                        if ((table.Rows[0].ToHtml().Contains("中标") || table.Rows[0].ToHtml().Contains("成交"))&& !table.Rows[0].ToHtml().Contains("候选"))
                                        {
                                            string ctx = string.Empty;
                                            for (int r = 0; r < table.Rows[0].ColumnCount; r++)
                                            {
                                                try
                                                {
                                                    ctx += table.Rows[0].Columns[r].ToNodePlainString() + "：";
                                                    ctx += table.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                            bidUnit = ctx.GetBidRegex();
                                            if (string.IsNullOrEmpty(bidUnit))
                                                bidUnit = ctx.GetRegex("中标（成交）供应商");
                                            break;
                                        }
                                    }
                                    for (int tb = 0; tb < tableNode.Count; tb++)
                                    {
                                        TableTag tables = tableNode[tb] as TableTag;
                                        if (tables.Rows[0].ToHtml().Contains("投标报价") || tables.Rows[0].ToHtml().Contains("总报价"))
                                        {
                                            string ctx = string.Empty;
                                            for (int r = 0; r < tables.Rows[0].ColumnCount; r++)
                                            {
                                                try
                                                {
                                                    ctx += tables.Rows[0].Columns[r].ToNodePlainString() + "：";
                                                    ctx += tables.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                            string unit = ctx.GetBidRegex();
                                            if (string.IsNullOrEmpty(unit))
                                                bidUnit = ctx.GetRegex("中标（成交）供应商");
                                            if (!string.IsNullOrEmpty(bidUnit) && bidUnit == unit)
                                            {
                                                bidMoney = ctx.GetMoneyRegex(null, false, "万元");
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            try
                            {
                                if (Convert.ToDecimal(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            if (bidUnit.Contains("金额"))
                                bidUnit = "";
                            if (bidUnit.Contains("&amp"))
                                bidUnit = bidUnit.Replace("&amp","&");
                            specType  = "建设工程";
                            bidType = prjName.GetInviteBidType();
                            msgType = "深圳市前海深港现代服务业合作区管理局";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳区及街道工程", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);

                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
        private bool IsTable(string htmld)
        {
            Parser p = new Parser(new Lexer(htmld));
            NodeList node = p.ExtractAllNodesThatMatch(new TagNameFilter("tr"));
            return node != null && node.Count > 0;
        }
        private string GetTableBid(string htmld)
        {
            string ctx = string.Empty;
            Parser p = new Parser(new Lexer(htmld));
            NodeList node = p.ExtractAllNodesThatMatch(new TagNameFilter("table"));
            if (node != null && node.Count > 0)
            {
                TableTag table = node[0] as TableTag;
                for (int r = 0; r < table.Rows[0].ColumnCount; r++)
                {
                    ctx += table.Rows[0].Columns[r].ToNodePlainString().GetReplace(":,：") + "：";
                    ctx += table.Rows[1].Columns[r].ToNodePlainString().GetReplace(":,：") + "\r\n";
                }
            }
            return ctx;
        }
    }
}
