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
    public class BidSzNsJiaoYu:WebSiteCrawller
    {
        public BidSzNsJiaoYu()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "深圳市南山区教育局中标信息";
            this.Description = "自动抓取深圳市南山区教育局中标信息";
            this.PlanTime = "9:40,11:40,14:40,16:40,18:40";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 100;
            this.SiteUrl = "http://www.szns.gov.cn/jyj/xxgk6/qt74/zbgs/index.html";

        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string html = string.Empty;
            int pageInt = 1;
            string nextPage = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "Normal")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode.AsString().GetRegexBegEnd("总记录数:", ",每页显示");
                string sum = pageNode.AsString().GetRegexBegEnd("每页显示", "条记录");
                try
                {
                    pageInt = int.Parse(temp) / int.Parse(sum) + 1;
                }
                catch { }
                parser = new Parser(new Lexer(pageNode.AsHtml()));
                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                if (aNode != null && aNode.Count > 0)
                {
                    for (int a = 0; a < aNode.Count; a++)
                    {
                        ATag aTag = aNode[a].GetATag();
                        if (aTag.LinkText.Contains("下一页"))
                        {
                            nextPage = "http://www.szns.gov.cn" + aTag.Link;
                            break;
                        }
                    }
                }
            }
            for (int i = 2; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        string m = "http://www.szns.gov.cn/jyj/xxgk6/qt74/zbgs/0239458d-" + i + ".html";
                        html = this.ToolWebSite.GetHtmlByUrl(m);
                    }
                    catch { continue; }
                    parser = new Parser(new Lexer(html));
                    NodeList aNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "partPage")), true), new TagNameFilter("a")));
                    if (aNode != null && aNode.Count > 0)
                    {
                        for (int a = 0; a < aNode.Count; a++)
                        {
                            ATag aTag = aNode[a].GetATag();
                            if (aTag.LinkText.Contains("下一页"))
                            {
                                nextPage = "http://www.szns.gov.cn" + aTag.Link;
                                break;
                            }
                        }
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("style", "width:100%;border-collapse:collapse;")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();
                        prjName = aTag.GetAttribute("title");
                        string m = tr.ChildrenHTML.ToString();
                        beginDate = m.GetRegexBegEnd("<span>", "</span>").GetDateRegex();

                        InfoUrl = aTag.Link.GetReplace("&amp;", "&");
                        InfoUrl = "http://www.szns.gov.cn" + InfoUrl;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page_con")));
                        if (dtlNode == null || dtlNode.Count < 1)
                        {
                            parser.Reset();
                            dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        }
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("<br/>,</p>,<br>,<br />", "\r\n").ToCtxString();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();

                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("成交候选人,第一中标候选人名称").GetReplace("名称");
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (bidMoney == "0"||string.IsNullOrEmpty(bidMoney))
                                bidMoney = bidCtx.GetRegexBegEnd("预中标价：", "\r").GetMoney();
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("style", "border-collapse: collapse; width: 98%;")));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    TableTag dtlTable = tableNode[0] as TableTag;
                                    string ctx = string.Empty;
                                    if (dtlTable.RowCount == 2)
                                    {
                                        try
                                        {
                                            for (int r = 0; r < dtlTable.Rows[0].ColumnCount; r++)
                                            {
                                                ctx += dtlTable.Rows[0].Columns[r].ToNodePlainString() + "：";
                                                ctx += dtlTable.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                            }
                                        }
                                        catch { }
                                        bidUnit = ctx.GetBidRegex();
                                        if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                            bidMoney = ctx.GetRegex("中标金额(万元)");
                                        if (string.IsNullOrEmpty(bidMoney))
                                            bidMoney = ctx.GetMoneyRegex();
                                        if (string.IsNullOrEmpty(prjMgr))
                                            prjMgr = ctx.GetMgrRegex();
                                    }
                                }
                            }
                            try
                            {
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            if (!string.IsNullOrEmpty(bidUnit) && bidUnit[0] == '为')
                                bidUnit = bidUnit.Substring(1, bidUnit.Length - 1);

                            specType = "政府采购";
                            bidType = prjName.GetInviteBidType();
                            msgType = "深圳市南山区教育局";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag file = aNode[a].GetATag();
                                    if (file.IsAtagAttach())
                                    {
                                        string link = file.Link;
                                        if (!link.ToLower().Contains("http"))
                                            link = "http://exoa.nsjy.com" + file.Link;
                                        base.AttachList.Add(ToolDb.GenBaseAttach(file.LinkText, info.Id, link));
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
