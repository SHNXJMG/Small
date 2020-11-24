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
    public class BidHaiKouJST : WebSiteCrawller
    {
        public BidHaiKouJST()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "海口市建设工程信息网中标信息";
            this.Description = "自动抓取海口市建设工程信息网中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 300;
            this.SiteUrl = "http://www.ggzy.hi.gov.cn/jgzbgs/index.jhtml";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("colspan", "4")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString().GetRegexBegEnd("/", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.ggzy.hi.gov.cn/jgzbgs/index_" + i + ".jhtml");
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "newtable")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
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
                        area = tr.Columns[1].ToString().GetNotChina();
                        ATag aTag = tr.Columns[2].GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "newsCon")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 1)
                            {
                                string Ctx = string.Empty;
                                TableTag tableTag = tableNode[1] as TableTag;
                                for (int r = 0; r < tableTag.RowCount; r++)
                                {
                                    for (int c = 0; c < tableTag.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = tableTag.Rows[r].Columns[c].ToNodePlainString().GetReplace("，", "\r\n");
                                        if ((c + 1) % 2 == 0)
                                        {
                                            Ctx += temp + "\r\n";
                                        }
                                        else
                                            Ctx += temp + "";
                                    }
                                }
                                bidUnit = Ctx.GetRegex("第一中标候选人");
                                prjMgr = Ctx.GetRegex("项目经理（总监）");
                                bidMoney = Ctx.GetMoneyRegex();
                                code = Ctx.GetCodeRegex();
                            }
                            bidCtx = HtmlTxt.ToCtxString();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();

                            msgType = "海南省公共资源交易服务中心";
                            specType = "建设工程";
                            bidType = prjName.GetInviteBidType();
                            BidInfo info = ToolDb.GenBidInfo("海南省", "海南省及地市", string.Empty, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.ggzy.hi.gov.cn/" + a.Link.GetReplace("../,./");
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
