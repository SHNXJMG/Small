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
    public class BidLiaoNingZtb:WebSiteCrawller
    {
        public BidLiaoNingZtb()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "辽宁省招标投标监管网中标信息";
            this.Description = "自动抓取辽宁省招标投标监管网中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 300;
            this.SiteUrl = "http://www.lntb.gov.cn/Article_Class2.asp?ClassID=3";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch
            {
                return null;
            }
            try
            {
                string temp = html.GetRegexBegEnd("<strong>", "</strong>").GetReplace("<fontcolor=red>1</font>/");
                pageInt = int.Parse(temp);
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&SpecialID=0&page=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "200")));
                if (listNode != null && listNode.Count > 0)
                {
                    parser = new Parser(new Lexer(listNode.AsHtml()));
                    NodeList fontNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                    if (fontNode != null && fontNode.Count > 0)
                    {
                        for (int j = 0; j < fontNode.Count; j++)
                        {
                            ATag aTag = fontNode[j] as ATag;
                            if (aTag == null) continue;
                            string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
           bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;
                            string temp = aTag.GetAttribute("title");
                            prjName = temp.GetRegex("文章标题");
                            code = temp.GetRegex("招标代码");
                            beginDate = temp.GetRegex("更新时间").GetDateRegex("yyyy/MM/dd");
                            InfoUrl = "http://www.lntb.gov.cn/" + aTag.Link;
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "200")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.AsHtml();
                                bidCtx = HtmlTxt.ToLower().GetReplace("</p>,<br/>,<br>", "\r\n").ToCtxString();
                                
                                buildUnit = bidCtx.GetBuildRegex();
                                bidUnit = bidCtx.GetBidRegex().GetReplace("名称");
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = bidCtx.GetRegex("中标人名称").GetReplace("名称");
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = bidCtx.GetReplace("中标人名称：,:中标人名称,中标人：,中标人:", "\r\n").GetBidRegex().GetReplace("名称");
                                if (string.IsNullOrEmpty(bidUnit))
                                {
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                    if (tableNode != null && tableNode.Count > 0)
                                    {
                                        TableTag tag = tableNode[tableNode.Count-1] as TableTag;
                                        string ctx = string.Empty;
                                        for (int r = 0; r < tag.RowCount; r++)
                                        {
                                            for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                            {
                                                string tempStr = tag.Rows[r].Columns[c].ToNodePlainString();
                                                if ((c + 1) % 2 == 0)
                                                    ctx += tempStr.GetReplace(":,：") + "\r\n";
                                                else
                                                    ctx += tempStr.GetReplace(":,：") + "：";
                                            }
                                        }
                                        bidUnit = ctx.GetBidRegex().GetReplace("名称");
                                        if (string.IsNullOrEmpty(bidUnit))
                                            bidUnit = ctx.GetRegex("中标人名称").GetReplace("名称");
                                    }
                                }
                                bidMoney = bidCtx.GetMoneyRegex();
                                prjMgr = bidCtx.GetMgrRegex();
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                                if (buildUnit.Contains("地址"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                                msgType = "辽宁省招标投标协调管理办公室";
                                specType = "建设工程";
                                bidType = "建设工程";
                                BidInfo info = ToolDb.GenBidInfo("辽宁省", "辽宁省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
