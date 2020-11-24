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
using System.Threading;

namespace Crawler.Instance
{
    public class BidInfoGuiZhouZfcg : WebSiteCrawller
    {
        public BidInfoGuiZhouZfcg()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "贵州省发展和改革委员会中标信息";
            this.Description = "自动抓取贵州省发展和改革委员会中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 500;
            this.SiteUrl = "http://220.197.198.65/zbgs/index.jhtml";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1, sqlCount = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "main container clearfix")), true), new TagNameFilter("div")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].ToString().GetRegexBegEnd("/", "页");
                    pageInt = int.Parse(temp.ToLower());
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://220.197.198.65/zbgs/index_" + i + ".jhtml", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "new_table")), true), new TagNameFilter("tr")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 1; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = node.GetATag();
                        InfoUrl = aTag.Link;
                        beginDate = node.ToPlainTextString().GetDateRegex();

                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content-txt")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.Replace("font", "span").Replace("td", "span").Replace("</div>", "\r\n").Replace("</p>", "\r\n").Replace("</tr>", "\r\n").Replace("<br/>", "\r\r").ToCtxString().Replace("\r\n\r\n", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("第一中标候选人\r\n", "第一中标候选人").Replace("报价", "\r\n报价").Replace("报价（人民币：元）\r\n", "报价").Replace("报价\r\n", "报价");

                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            if (string.IsNullOrWhiteSpace(prjAddress))
                                prjAddress = bidCtx.GetRegex("联系地址");
                            if (prjAddress.Contains("电话"))
                                prjAddress = prjAddress.Remove(prjAddress.IndexOf("电话"));

                            code = bidCtx.GetCodeRegex();
                            bidUnit = bidCtx.GetBidRegex();

                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                try
                                {

                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                    if (tableNode != null && tableNode.Count > 0)
                                    {
                                        TableTag table = tableNode[0] as TableTag;
                                        string ctx = string.Empty;
                                        if (table.RowCount == 3)
                                        {
                                            for (int c = 0; c < table.Rows[1].ColumnCount; c++)
                                            {
                                                try
                                                {
                                                    ctx += table.Rows[1].Columns[c].ToNodePlainString() + ":";
                                                    ctx += table.Rows[2].Columns[c].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                        }
                                        else if (table.RowCount == 2)
                                        {
                                            for (int c = 0; c < table.Rows[0].ColumnCount; c++)
                                            {
                                                try
                                                {
                                                    ctx += table.Rows[0].Columns[c].ToNodePlainString() + ":";
                                                    ctx += table.Rows[1].Columns[c].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                        }
                                        else {
                                            for (int r = 0; r < table.RowCount; r++)
                                            {
                                                for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                                                {
                                                    string temp = table.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                                    if ((c + 1) % 2 == 0)
                                                    {
                                                        ctx += temp + "\r\n";
                                                    }
                                                    else
                                                        ctx += temp + "：";
                                                }
                                            }
                                        }
                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrWhiteSpace(buildUnit))
                                            buildUnit = ctx.GetBuildRegex();
                                        if (string.IsNullOrWhiteSpace(bidUnit))
                                            bidUnit = bidCtx.GetRegex("中标供应商");
                                        if (string.IsNullOrWhiteSpace(prjAddress))
                                            prjAddress = ctx.GetAddressRegex();
                                        if (string.IsNullOrWhiteSpace(bidMoney))
                                            bidMoney = ctx.GetMoneyRegex();
                                        if (string.IsNullOrEmpty(prjMgr))
                                            prjMgr = ctx.GetMgrRegex();
                                    }
                                }
                                catch (Exception ex) { continue; }

                            }
                            prjMgr = bidCtx.GetMgrRegex();
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList btl = parser.ExtractAllNodesThatMatch(new TagNameFilter("h1"));
                            string ht = btl.AsHtml();
                            prjName = ht.ToCtxString();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("第一名");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("中标人名称");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("中标候选人");
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetBidRegex(null, false);
                            if (string.IsNullOrWhiteSpace(bidMoney))
                                bidMoney = bidCtx.GetMoneyRegex();
                            if (bidUnit.Contains("总分"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("总分"));
                            if (bidUnit.Contains("技术分"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("技术分"));
                            if (bidUnit.Contains("工程勘察"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("工程勘察"));
                            bidUnit = bidUnit == "公示" ? string.Empty : bidUnit;
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("单位"))
                                bidUnit = "";
                            else if (bidUnit.Contains("中标价"))
                                bidUnit = "";
                            specType = "政府采购";
                            bidType = prjName.GetInviteBidType();
                            msgType = "贵州省发展和改革委员会";
                            BidInfo info = ToolDb.GenBidInfo("贵州省", "贵州省及地市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                        try
                                        {
                                            link = "http://220.197.198.65" + a.Link; 
                                        }
                                        catch { link = a.Link.Replace("\\", ""); } 
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);

                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
