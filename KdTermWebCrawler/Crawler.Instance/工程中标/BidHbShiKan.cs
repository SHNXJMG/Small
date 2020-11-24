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
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class BidHbShiKan : WebSiteCrawller
    {
        public BidHbShiKan()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "十堰市公共资源交易中心信息中标信息";
            this.Description = "自动抓取十堰市公共资源交易中心信息中标信息";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://www.syggzyjy.org.cn/html/jyxx/zbgs/zj/";
            this.MaxCount = 120;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "pages")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 2].ToNodePlainString();
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + i + ".html", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "ul_text")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
                             bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty,
                             InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty,
                             prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;


                        ATag aTag = listNode[j].GetATag();
                        if (aTag == null) continue;
                        prjName = aTag.GetAttribute("title");
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</p>,<br />,<br/>", "\r\n").ToCtxString().GetReplace("\t", "\r\n");
                            prjAddress = bidCtx.GetAddressRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    TableTag tableTag = tableNode[0] as TableTag;
                                    string ctx = string.Empty;
                                    for (int r = 0; r < tableTag.RowCount; r++)
                                    {
                                        try
                                        {
                                            ctx += tableTag.Rows[r].Columns[0].ToNodePlainString().GetReplace(":,：") + "：";
                                            ctx += tableTag.Rows[r].Columns[1].ToNodePlainString().GetReplace(":,：") + "\r\n";
                                        }
                                        catch { ctx += "\r\n"; }
                                    }
                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("中标候选人名称");
                                    if (bidUnit.Contains("中标"))
                                        bidUnit = "";
                                    bidMoney = ctx.GetMoneyRegex();
                                    if (string.IsNullOrWhiteSpace(bidUnit))
                                    {
                                        ctx = "";
                                        for (int r = 0; r < tableTag.RowCount; r++)
                                        {
                                            for (int c = 0; c < tableTag.Rows[r].ColumnCount; c++)
                                            {
                                                string temp = tableTag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");

                                                if (c % 2 == 0)
                                                    ctx += temp + "：";
                                                else
                                                    ctx += temp + "\r\n";
                                            }
                                        }
                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrEmpty(bidUnit))
                                            bidUnit = ctx.GetRegex("中标候选人名称");
                                        if (bidUnit.Contains("中标"))
                                            bidUnit = "";
                                        bidMoney = ctx.GetMoneyRegex();
                                    }
                                    if (string.IsNullOrWhiteSpace(bidUnit)&&tableNode.Count>1)
                                    {
                                        tableTag = tableNode[1] as TableTag;
                                        if (tableTag != null)
                                        {
                                            ctx = string.Empty;
                                            for (int r = 0; r < tableTag.RowCount; r++)
                                            {
                                                try
                                                {
                                                    ctx += tableTag.Rows[r].Columns[0].ToNodePlainString().GetReplace(":,：") + "：";
                                                    ctx += tableTag.Rows[r].Columns[1].ToNodePlainString().GetReplace(":,：") + "\r\n";
                                                }
                                                catch { ctx += "\r\n"; }
                                            }
                                            bidUnit = ctx.GetBidRegex();
                                            if (string.IsNullOrEmpty(bidUnit))
                                                bidUnit = ctx.GetRegex("中标候选人名称");
                                            if (bidUnit.Contains("中标"))
                                                bidUnit = "";
                                            bidMoney = ctx.GetMoneyRegex();
                                            if (string.IsNullOrWhiteSpace(bidUnit))
                                            {
                                                ctx = "";
                                                for (int r = 0; r < tableTag.RowCount; r++)
                                                {
                                                    for (int c = 0; c < tableTag.Rows[r].ColumnCount; c++)
                                                    {
                                                        string temp = tableTag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");

                                                        if (c % 2 == 0)
                                                            ctx += temp + "：";
                                                        else
                                                            ctx += temp + "\r\n";
                                                    }
                                                }
                                                bidUnit = ctx.GetBidRegex();
                                                if (string.IsNullOrEmpty(bidUnit))
                                                    bidUnit = ctx.GetRegex("中标候选人名称");
                                                if (bidUnit.Contains("中标"))
                                                    bidUnit = "";
                                                bidMoney = ctx.GetMoneyRegex();
                                            }
                                        }
                                    }
                                }
                            }
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            if (buildUnit.Contains("指挥部"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("指挥部"));
                            code = bidCtx.GetCodeRegex().GetCodeDel().GetReplace(".");
                            if (bidUnit.Contains("日历天") || bidUnit.Contains("预期中标") || bidUnit.Contains("投标人") || bidUnit.Contains("中标价"))
                                bidUnit = string.Empty;
                            bidUnit = bidUnit.GetReplace("名称");

                            try
                            {
                                if (decimal.Parse(bidMoney) < 1)
                                    bidMoney = "0";
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }

                            msgType = "十堰市公共资源交易中心";
                            specType = "建设工程";
                            bidType = prjName.GetInviteBidType();
                            buildUnit = buildUnit.Replace(" ", "");
                            BidInfo info = ToolDb.GenBidInfo("湖北省", "湖北省及地市", "十堰市", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k] as ATag;
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://www.syggzyjy.org.cn/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                            if (list.Count % 30 == 0)
                            {
                                Thread.Sleep(1000 * 500);
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
