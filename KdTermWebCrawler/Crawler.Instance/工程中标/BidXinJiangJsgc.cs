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
    public class BidXinJiangJsgc : WebSiteCrawller
    {
        public BidXinJiangJsgc()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "新疆维吾尔自治区建设工程招标投标网中标信息";
            this.Description = "自动抓取新疆维吾尔自治区建设工程招标投标网中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.xjztb.net/Homepage/webzbgs.aspx?BidType=%CA%A9%B9%A4";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_Repeater1_ctl16_lblpc")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString();
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + (i - 1).ToString(), Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "slist")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
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
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = DateTime.Now.Year + "-" + node.GetSpan().StringText.ToNodeString().GetReplace(" ");
                        area = node.ToNodePlainString().GetReplace("[", "【").GetReplace("]", "】").GetRegexBegEnd("【", "】");
                        InfoUrl = "http://www.xjztb.net/Homepage/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_Panel3")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag table = tableNode[tableNode.Count - 1] as TableTag;

                                bidCtx = string.Empty;
                                for (int r = 0; r < table.RowCount; r++)
                                {
                                    for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = table.Rows[r].Columns[c].ToNodePlainString();
                                         
                                        if ((c + 1) % 2 == 0)
                                            bidCtx += temp + "\r\n";
                                        else
                                            if (temp.Contains("工程师")||temp.Contains("注册证号"))
                                                bidCtx += temp + "\r\n";
                                            else
                                                bidCtx += temp.GetReplace(":,：") + "：";
                                    }
                                }
                            }
                            else
                                bidCtx = HtmlTxt.ToCtxString();
                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();

                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("单位名称");
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrEmpty(prjMgr))
                                prjMgr = bidCtx.GetRegex("总施工工程师");
                            msgType = "新疆维吾尔自治区建设工程招标投标监督管理办公室";
                            specType = "建设工程";
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
