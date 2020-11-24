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
    public class BidShanXiJsgcztb : WebSiteCrawller
    {
        public BidShanXiJsgcztb()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "山西省建设工程招标投标站中标信息";
            this.Description = "自动抓取山西省建设工程招标投标站中标信息";
            this.PlanTime = "9:10,11:10,14:10,16:10,18:10";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.SiteUrl = "http://www.sxztbw.cn:20046/zbgg2List.jsp?page=1";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "wenzi12")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("共", "页").GetReplace("&nbsp;");
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.sxztbw.cn:20046/zbgg2List.jsp?page=" + i);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "98%")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string prjName = string.Empty,
                           buildUnit = string.Empty, bidUnit = string.Empty,
                           bidMoney = string.Empty, code = string.Empty,
                           bidDate = string.Empty, beginDate = string.Empty,
                           endDate = string.Empty, bidType = string.Empty,
                           specType = string.Empty, InfoUrl = string.Empty,
                           msgType = string.Empty, bidCtx = string.Empty,
                           prjAddress = string.Empty, remark = string.Empty,
                           prjMgr = string.Empty, otherType = string.Empty,
                           HtmlTxt = string.Empty, area = string.Empty;
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.LinkText.GetReplace(" ");
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.sxztbw.cn:20046/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "wenzi12")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = "";
                            TableTag tag = dtlNode[0] as TableTag;
                            for (int r = 0; r < tag.RowCount; r++)
                            {
                                for (int c = 1; c < tag.Rows[r].ColumnCount-1; c++)
                                {
                                    string temp = tag.Rows[r].Columns[c].ToNodePlainString();
                                    if (string.IsNullOrEmpty(temp)) continue;
                                    if ((c + 1) % 2 == 0)
                                        bidCtx += temp.GetReplace(":,：") + "：";
                                    else
                                        bidCtx += temp.GetReplace(":,：") + "\r\n";
                                }
                            }

                            code = bidCtx.GetCodeRegex();
                            if (string.IsNullOrEmpty(code))
                                code = bidCtx.GetRegex("工程编码");

                            if (string.IsNullOrEmpty(beginDate))
                            {
                                string temp = bidCtx.GetRegex("中标时间,中标日期");
                                beginDate = temp.GetDateRegex();
                                if (string.IsNullOrEmpty(beginDate))
                                    beginDate = temp.GetDateRegex("yyyy年MM月dd日");
                                if (string.IsNullOrEmpty(beginDate))
                                    beginDate = temp.GetDateRegex("yyyy/MM/dd");
                            }
                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            msgType = "山西省建设工程招标投标站";
                            specType = bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("山西省", "山西省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.sxztbw.cn:20046/" + a.Link.GetReplace("../,./");
                                        if (Encoding.Default.GetByteCount(link) > 500)
                                            continue;
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
