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
    public class BidGuanLan : WebSiteCrawller
    {
        public BidGuanLan()
            : base()
        {
            this.Group = "街道办中标信息";
            this.Title = "广东省深圳市观澜街道办事处";
            this.Description = "自动抓取广东省深圳市观澜街道办事处";
            this.PlanTime = "9:22,13:53";
            this.SiteUrl = "http://glbsc.szlhxq.gov.cn/glbsc/zwgk70/zbcg5/zbxxgs93/index.html";
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
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "Normal")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToNodePlainString().GetRegexBegEnd("/", "跳转");
                    pageInt = Convert.ToInt32(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://glbsc.szlhxq.gov.cn/glbsc/zwgk70/zbcg5/zbxxgs93/15159-" + i + ".html", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("style", "border-bottom: 1px dashed #333;")));
                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
                    {
                        TableTag table = viewList[j] as TableTag;
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
                        beginDate = table.ToPlainTextString().GetDateRegex();
                        ATag aTag = table.GetATag();
                        prjName = aTag.GetAttribute("title");
                        InfoUrl = "http://glbsc.szlhxq.gov.cn" + aTag.Link;
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "contentbox")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</p>,</br>", "\r\n").ToCtxString();

                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex(null, false, "万元");
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList bidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                TableTag bidTable = null;
                                string ctx = string.Empty;
                                if (bidNode != null && bidNode.Count > 1)
                                {

                                    bidTable = bidNode[1] as TableTag;
                                }
                                else if (bidNode != null && bidNode.Count > 0)
                                    bidTable = bidNode[0] as TableTag;
                                if (bidTable != null)
                                {
                                    for (int r = 0; r < bidTable.RowCount; r++)
                                    {
                                        for (int c = 0; c < bidTable.Rows[r].ColumnCount; c++)
                                        {
                                            if ((c + 1) % 2 == 0)
                                                ctx += bidTable.Rows[r].Columns[c].ToNodePlainString() + "\r\n";
                                            else
                                                ctx += bidTable.Rows[r].Columns[c].ToNodePlainString() + "：";
                                        }
                                    }

                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                        bidMoney = ctx.GetMoneyString().GetMoney("万元");
                                    if (string.IsNullOrEmpty(prjAddress))
                                        prjAddress = ctx.GetAddressRegex();
                                    if (string.IsNullOrEmpty(buildUnit))
                                        buildUnit = ctx.GetBuildRegex();
                                    if (string.IsNullOrEmpty(code))
                                        code = ctx.GetCodeRegex().GetCodeDel();
                                    if (bidUnit.Contains("推荐") || bidUnit.Contains("中标") || bidUnit.Contains("地址"))
                                        bidUnit = string.Empty;
                                    if (string.IsNullOrWhiteSpace(prjMgr))
                                        prjMgr = ctx.GetMgrRegex();
                                    if (string.IsNullOrEmpty(bidUnit))
                                    {
                                        if (bidTable.RowCount > 1)
                                        {
                                            ctx = string.Empty;
                                            for (int d = 0; d < bidTable.Rows[0].ColumnCount; d++)
                                            {
                                                ctx += bidTable.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                try
                                                {
                                                    ctx += bidTable.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                            bidUnit = ctx.GetBidRegex();
                                            if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                                bidMoney = ctx.GetMoneyString().GetMoney();
                                            if (string.IsNullOrEmpty(prjAddress))
                                                prjAddress = ctx.GetAddressRegex();
                                            if (string.IsNullOrEmpty(buildUnit))
                                                buildUnit = ctx.GetBuildRegex();
                                            if (string.IsNullOrEmpty(code))
                                                code = ctx.GetCodeRegex().GetCodeDel();
                                        }
                                    }
                                }
                            }
                        }
                        try
                        {
                            if (decimal.Parse(bidMoney) > 1000000)
                                bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                        }
                        catch { }


                        if (string.IsNullOrEmpty(buildUnit))
                        {
                            buildUnit = "深圳市龙华新区观澜街道办事处";
                        }
                        msgType = "深圳市龙华新区观澜街道办事处";
                        specType = "建设工程";
                        bidType = "小型工程";
                        prjName = ToolDb.GetPrjName(prjName);
                        BidInfo info = ToolDb.GenBidInfo("广东省", "深圳区及街道工程", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                               bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
