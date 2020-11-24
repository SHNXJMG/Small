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
    public class BidXinJiangGgzy : WebSiteCrawller
    {
        public BidXinJiangGgzy()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "新疆生产建设兵团公共资源交易中心中标信息";
            this.Description = "自动抓取新疆生产建设兵团公共资源交易中心中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://ggzy.xjbt.gov.cn/TPFront/jyxx/004001/004001005/"; 
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
            catch { }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "huifont")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString().GetReplace("1/");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?Paging=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "98%")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.GetATag();
                        if (aTag == null) continue;
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
                         prjName = aTag.GetAttribute("title");
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://ggzy.xjbt.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml().ToLower().GetReplace("<a href='http://22'>22</a>.");
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,<br/>","\r\n").ToCtxString();
                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("第一中标候选单位为,第一名");
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                bidMoney = bidCtx.GetMoneyRegex(null, true);
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    TableTag tag = tableNode[tableNode.Count - 1] as TableTag;
                                    string ctx = string.Empty; 
                                    for (int r = 0; r < tag.RowCount; r++)
                                    {
                                        for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                        {
                                            string temp = tag.Rows[r].Columns[c].ToNodePlainString();
                                            if ((c + 1) % 2 == 0)
                                                ctx += temp + "\r\n";
                                            else
                                                if (temp.Contains("工程师") || temp.Contains("注册证号"))
                                                    ctx += temp + "\r\n";
                                                else
                                                    ctx += temp.GetReplace(":,：") + "：";
                                        }
                                    }
                                    ctx = ctx.GetReplace("单位名称\r\n", "单位名称：");
                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("单位名称");
                                    if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                        bidMoney = ctx.GetMoneyRegex();
                                    if (string.IsNullOrEmpty(prjMgr))
                                        prjMgr = ctx.GetMgrRegex();
                                    if (string.IsNullOrEmpty(prjMgr))
                                        prjMgr = ctx.GetRegex("建造师姓名");
                                    if (string.IsNullOrEmpty(code))
                                        code = ctx.GetCodeRegex();
                                    if (string.IsNullOrEmpty(buildUnit))
                                        buildUnit = ctx.GetBuildRegex();
                                }
                            }
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove( bidUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("小写"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("小写"));
                            try
                            {
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            msgType = "新疆生产建设兵团公共资源交易中心";
                            specType = "政府采购";
                            bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("新疆维吾尔自治区", "新疆维吾尔自治区及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
