using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class BidHuiNanGgzy : WebSiteCrawller
    {
        public BidHuiNanGgzy()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "湖南省公共资源交易中心中标信息";
            this.Description = "自动抓取湖南省公共资源交易中心中标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.SiteUrl = "http://www.hngzzx.com/HomePage/ShowList.aspx?tbid=1&TypeAll=2";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToPlainTextString().GetRegexBegEnd("共", "页");
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
                        html= this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&Page=" + i);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_con_main_bulcon")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {

                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
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
                             prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        prjName = aTag.GetAttribute("title");
                        beginDate = node.ToNodePlainString().GetDateRegex();
                        string linkId = aTag.Link.GetRegexBegEnd("Id=", "&");
                        InfoUrl = "http://www.hngzzx.com/HomePage/ShowInfoDetail.aspx?Id=" + linkId + "&TableID=1";
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "detail_con")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</p>,<br/>", "\r\n").ToCtxString();
                            prjAddress = bidCtx.GetAddressRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();

                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("第一名",false);
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();

                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("br"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    bool isOk = false;
                                    string ctx = string.Empty;
                                    for (int t = 0; t < tableNode.Count; t++)
                                    {
                                        if (tableNode[t].ToPlainTextString().Contains("供应商名称"))
                                        {
                                            isOk = true;
                                            TableTag tag = tableNode[t] as TableTag;
                                            if (tag.RowCount > 2)
                                            {
                                                for (int c = 0; c < tag.Rows[0].ColumnCount; c++)
                                                {
                                                    ctx += tag.Rows[0].Columns[c].ToNodePlainString() + "：";
                                                    try
                                                    {
                                                        ctx += tag.Rows[1].Columns[c].ToNodePlainString() + "\r\n";
                                                    }
                                                    catch { }
                                                }
                                            }
                                            break;
                                        }
                                    }
                                    if (!isOk)
                                    {
                                        for (int t = 0; t < tableNode.Count; t++)
                                        {
                                            if (tableNode.AsString().Contains("中标候选人"))
                                            {
                                                isOk = true;
                                                TableTag tag = tableNode[t] as TableTag;
                                                if (tag.RowCount > 2)
                                                {
                                                    for (int c = 0; c < tag.Rows[0].ColumnCount; c++)
                                                    {
                                                        ctx += tag.Rows[0].Columns[0].ToNodePlainString() + "：";
                                                        try
                                                        {
                                                            ctx += tag.Rows[1].Columns[0].ToNodePlainString() + "\r\n";
                                                        }
                                                        catch { }
                                                    }
                                                }
                                                break;
                                            }
                                        }
                                        if (!isOk)
                                        {
                                            for (int t = 0; t < tableNode.Count; t++)
                                            {
                                                if (tableNode.AsString().Contains("中标单位") || tableNode.AsString().Contains("中标候选单位") || tableNode.AsString().Contains("投标人名称"))
                                                {
                                                    isOk = true;
                                                    TableTag tag = tableNode[t] as TableTag;
                                                    if (tag.RowCount > 2)
                                                    {
                                                        for (int c = 0; c < tag.Rows[0].ColumnCount; c++)
                                                        {
                                                            ctx += tag.Rows[0].Columns[0].ToNodePlainString() + "：";
                                                            try
                                                            {
                                                                ctx += tag.Rows[1].Columns[0].ToNodePlainString() + "\r\n";
                                                            }
                                                            catch { }
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("第一名,供应商名称,投标人名称");
                                    string money = ctx.GetMoneyRegex();
                                    if (string.IsNullOrEmpty(money) || bidMoney != money)
                                        bidMoney = money;
                                    if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                        bidMoney = ctx.GetRegex("中标金额（单位：元）,最终报价,投标报价（元）", false).GetMoney();
                                    if (string.IsNullOrEmpty(prjMgr))
                                        prjMgr = bidCtx.GetMgrRegex();
                                }
                            }
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("研究院"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("研究院")) + "研究院";
                            if (bidUnit.Contains("开发局"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("开发局")) + "开发局";
                            if (bidUnit.Contains("名称") || bidUnit.Contains("联系人") || bidUnit.Contains("报价") || bidUnit.Contains("内容"))
                                bidUnit = string.Empty;
                            bidUnit = bidUnit.GetReplace("1,2,3,、");
                            if (code.Contains("代理"))
                                code = code.Remove(code.IndexOf("代理"));
                            try
                            {
                                if (decimal.Parse(bidMoney) > 10000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            specType = bidType = "政府采购";
                            msgType = "湖南省公共资源交易中心";

                            BidInfo info = ToolDb.GenBidInfo("湖南省", "湖南省及地市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
