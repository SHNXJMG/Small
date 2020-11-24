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
    public class BidNingXiaJsgc : WebSiteCrawller
    {
        public BidNingXiaJsgc()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "宁夏建设工程招标投标信息网中标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取宁夏建设工程招标投标信息网中标信息";
            this.SiteUrl = "http://www.nxzb.com.cn/SiteAcl.srv?id=1003979659";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            try
            {
                string temp = html.ToCtxString().GetRegexBegEnd("第1/", "页");
                pageInt = int.Parse(temp);
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "pageno",
                    "mode",
                    "linkname"
                    }, new string[]{
                   i.ToString(),
                   "query",
                   "currinfo"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "list")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[listNode.Count - 1] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();
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
                        InfoUrl = "http://www.nxzb.com.cn/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl.ToLower().GetReplace("th", "td")));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "page1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode[dtlNode.Count - 1].ToHtml();
                            code = HtmlTxt.ToCtxString().GetCodeRegex().GetCodeDel();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "zbcon")));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag tag = tableNode[0] as TableTag;
                                for (int r = 0; r < tag.RowCount; r++)
                                {
                                    if (r == 0)
                                        bidUnit = tag.Rows[r].Columns[0].ToNodePlainString();
                                    for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = tag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                        if ((c + 1) % 2 == 0)
                                            bidCtx += temp + "\r\n";
                                        else
                                            bidCtx += temp + "：";
                                    }
                                }
                                bidMoney = bidCtx.GetMoneyRegex();
                                prjMgr = bidCtx.GetMgrRegex();
                                buildUnit = bidCtx.GetBuildRegex();
                                Regex regDate = new Regex(@"\d{4}年\d{1,2}月\d{1,2}");
                                beginDate = regDate.Match(bidCtx).Value;
                            }
                            try
                            {
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch{}
                            msgType = "宁夏建设工程招标投标管理中心";
                            specType = "建设工程";
                            bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("宁夏回族自治区", "宁夏回族自治区及地市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
