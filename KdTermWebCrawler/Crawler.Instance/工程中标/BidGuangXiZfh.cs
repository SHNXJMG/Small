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
    public class BidGuangXiZfh : WebSiteCrawller
    {
        public BidGuangXiZfh()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广西省住房和城乡建设厅中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.Description = "自动抓取广西省住房和城乡建设厅中标信息";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.gxcic.net/ztb/zblist.aspx";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("nowrap", "true")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("总页数", "当前页").Replace("：", "");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__VIEWSTATE",
                    "__VIEWSTATEGENERATOR",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__EVENTVALIDATION",
                    "txtKeyText",
                    "pid",
                    "keyword",
                    "starttime",
                    "endtime",
                    "ChannelId1",
                    "ChannelId2",
                    "ChannelId3",
                    "AspNetPager1_input"
                    }, new string[]{
                    viewState,
                    "742B441D",
                    "AspNetPager1",
                    i.ToString(),
                    eventValidation,
                    "站内搜索",
                    "18",
                    "",
                    "",
                    "",
                    "450000",
                    "0",
                    "0",
                    (i-1).ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "4")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
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

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();
                        prjName = aTag.GetAttribute("title");
                        if (prjName[2].Equals('县') || prjName[2].Equals('区') || prjName[2].Equals('市'))
                            area = prjName.Substring(0, 3);
                        beginDate = "20" + tr.Columns[1].ToNodePlainString().Replace(".", "-");
                        InfoUrl = "http://www.gxcic.net/ztb/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page-right-box")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            prjAddress = bidCtx.GetAddressRegex(); 
                            buildUnit = bidCtx.GetBuildRegex();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList bidNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "1")));
                            if (bidNode != null && bidNode.Count > 0)
                            {
                                TableTag tableTag = bidNode[0] as TableTag;
                                string ctx = string.Empty;
                                for (int r = 0; r < tableTag.RowCount; r++)
                                {
                                    for (int d = 0; d < tableTag.Rows[r].ColumnCount; d++)
                                    {
                                        if ((d + 1) % 2 == 0)
                                            ctx += tableTag.Rows[r].Columns[d].ToNodePlainString() + "\r\n";
                                        else
                                            ctx += tableTag.Rows[r].Columns[d].ToNodePlainString().GetReplace(new string[]{"：",":"}) + "：";
                                    }
                                }
                                bidUnit = ctx.GetBidRegex();
                                bidMoney = ctx.GetMoneyRegex();
                                prjMgr = ctx.GetMgrRegex();
                                try
                                {
                                    if (decimal.Parse(bidMoney) > 10000)
                                        bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                                }
                                catch { }
                            }
                            if (bidUnit.Contains("详见内文"))
                                bidUnit = string.Empty;
                            
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            specType = "建设工程";
                            msgType = "广西壮族自治区住房和城乡建设厅";
                            BidInfo info = ToolDb.GenBidInfo("广西壮族自治区", "广西壮族自治区及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
