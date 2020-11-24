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
    public class BidWuHanGgzy:WebSiteCrawller
    {
        public BidWuHanGgzy()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "武汉建设信息网中标信息";
            this.Description = "自动抓取武汉建设信息网中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.whzbtb.com/cx/cx_5.aspx"; 
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "lb_page")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("分", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?page=" + (i - 1).ToString(), Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "list")));
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
                        ATag aTag = tr.Columns[2].GetATag();
                        prjName = aTag.LinkText;
                        bidUnit = tr.Columns[4].ToNodePlainString();
                        bidMoney = tr.Columns[5].ToNodePlainString();
                        endDate = tr.Columns[6].ToPlainTextString().GetDateRegex("yyyy/MM/dd");
                        InfoUrl = "http://www.whzbtb.com/" + aTag.Link.GetReplace("../,./").Replace("&amp;","&");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("width", "683")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 1)
                            {
                                TableTag tableTag = tableNode[1] as TableTag;
                                for (int r = 0; r < tableTag.RowCount; r++)
                                {
                                    for (int c = 0; c < tableTag.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = tableTag.Rows[r].Columns[c].ToPlainTextString().GetReplace("　");
                                        if (string.IsNullOrWhiteSpace(temp)) continue;

                                        if ((c + 1) % 2 == 0)
                                            bidCtx += temp.GetReplace("：,:") + "\r\n";
                                        else
                                            bidCtx += temp.GetReplace("：,:") + "：";
                                    }
                                }
                            }
                            else
                                bidCtx = HtmlTxt.ToCtxString();

                            code = bidCtx.GetCodeRegex().GetReplace("　");
                            buildUnit = bidCtx.GetBuildRegex().GetReplace("　");
                            prjMgr = bidCtx.GetMgrRegex().GetReplace("　");
                            if (string.IsNullOrEmpty(prjMgr))
                                prjMgr = bidCtx.GetRegex("建筑师/总监/负责人").GetReplace("　");
                            beginDate = bidCtx.GetRegex("中标公示时段").GetDateRegex("yyyy/MM/dd");
                            if(string.IsNullOrEmpty(beginDate))
                                bidCtx.GetRegex("中标公示时段").GetDateRegex();
                            if (string.IsNullOrEmpty(beginDate))
                                bidCtx.GetRegex("开标时间").GetDateRegex("yyyy/MM/dd");
                            if (string.IsNullOrEmpty(beginDate))
                                bidCtx.GetRegex("开标时间").GetDateRegex();
                            msgType = "武汉市公共资源交易中心";
                            specType = "政府采购";
                            bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("湖北省", "湖北省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.whzbtb.com/" + a.Link.GetReplace("../,./");
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
