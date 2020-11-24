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
    public class BidSzLongHuaZFCG : WebSiteCrawller
    {
        public BidSzLongHuaZFCG()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "深圳市龙华新区公告资源交易中心政府采购中标公告";
            this.Description = "自动抓取深圳市龙华新区公告资源交易中心政府采购中标公告";
            this.PlanTime = "9:22,13:53";
            this.SiteUrl = "http://www.szlhxq.gov.cn/lhxinqu/zdlyxxgkzl/ggzyjy/zfcgzbgg50/index.html";
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
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "zfcg_feiyeR")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToNodePlainString().GetRegexBegEnd("/", "跳");

                    pageInt = int.Parse(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.szlhxq.gov.cn/lhxinqu/zdlyxxgkzl/ggzyjy/zfcgzbgg50/9611-" + i + ".html", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "zfcg_zbggUl")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        ATag aTag = listNode[j].GetATag();
                        prjName = aTag.LinkText;
                        InfoUrl = "http://www.szlhxq.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "min")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString();
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex(new string[]{"中标金额","金额"},false,"万元");
                            prjMgr = bidCtx.GetMgrRegex();
                            bidCtx = HtmlTxt.ToCtxString();
                            bidType = prjName.GetInviteBidType();
                            prjAddress = bidType.GetAddressRegex();
                            buildUnit = bidType.GetBuildRegex();
                            code = bidType.GetCodeRegex().GetCodeDel();
                            msgType = "深圳市龙华新区公共资源交易中心";
                            specType = "政府采购";
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList bidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (bidNode != null && bidNode.Count > 0)
                                {
                                    TableTag table = bidNode[0] as TableTag;
                                    if (table.RowCount > 1)
                                    {
                                        string ctx = string.Empty;
                                        for (int c = 0; c < table.Rows[0].ColumnCount; c++)
                                        {
                                            ctx += table.Rows[0].Columns[c].ToNodePlainString() + "：";
                                            if (table.Rows[0].Columns[c].ToNodePlainString().Contains("万元"))
                                                ctx += table.Rows[1].Columns[c].ToNodePlainString() + "万元\r\n";
                                            else
                                                ctx += table.Rows[1].Columns[c].ToNodePlainString() + "\r\n";
                                        }
                                        bidUnit = ctx.GetBidRegex();
                                        if (bidMoney == "0"||string.IsNullOrWhiteSpace(bidMoney))
                                            bidMoney = ctx.GetMoneyRegex();
                                    }
                                    if (string.IsNullOrWhiteSpace(bidUnit) && bidNode.Count > 1)
                                    {
                                        table = bidNode[1] as TableTag;
                                        if (table.RowCount > 1)
                                        {
                                            string ctx = string.Empty;
                                            for (int c = 0; c < table.Rows[0].ColumnCount; c++)
                                            {
                                                ctx += table.Rows[0].Columns[c].ToNodePlainString() + "：";
                                                if (table.Rows[0].Columns[c].ToNodePlainString().Contains("万元"))
                                                    ctx += table.Rows[1].Columns[c].ToNodePlainString() + "万元\r\n";
                                                else
                                                    ctx += table.Rows[1].Columns[c].ToNodePlainString() + "\r\n";
                                            }
                                            bidUnit = ctx.GetBidRegex();
                                            //if (bidMoney == "0" || string.IsNullOrWhiteSpace(bidMoney))
                                                bidMoney = ctx.GetMoneyRegex();
                                        }
                                    }
                                }
                            }
                            if (bidUnit.Contains("&"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("&"));
                            bidUnit = bidUnit.Replace("　", "");
                            bidUnit = bidUnit.Replace("名称","");
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳政府采购", "龙华新区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
