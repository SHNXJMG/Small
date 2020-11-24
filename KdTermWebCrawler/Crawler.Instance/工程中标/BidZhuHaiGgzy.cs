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
using System.Web.Script.Serialization;
using System.Data;

namespace Crawler.Instance
{
    public class BidZhuHaiGgzy : WebSiteCrawller
    {
        public BidZhuHaiGgzy()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "珠海市公共资源交易中心中标信息";
            this.Description = "自动抓取珠海市公共资源交易中心中标信息";
            this.MaxCount = 50;
            this.PlanTime = "09:05,09:50,10:50,14:25,16:50";
            this.SiteUrl = "http://ggzy.zhuhai.gov.cn//zbjj/index.htm";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToPlainTextString().GetRegexBegEnd("共", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://ggzy.zhuhai.gov.cn//zbjj/index_" + i + ".htm",Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "news")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;

                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, HtmlTxt = string.Empty;


                        prjName = aTag.GetAttribute("title");
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link;

                        string htmldtl = string.Empty;
                        try
                        {

                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "m_r m_r_g")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            parser = new Parser(new Lexer(HtmlTxt.ToLower().Replace("th","td")));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "bordertb")));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                string ctx = string.Empty;
                                TableTag table = tableNode[0] as TableTag;
                                for (int r = 1; r < table.RowCount; r++)
                                {
                                    for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = table.Rows[r].Columns[c].ToNodePlainString();
                                        if (c % 2 == 0)
                                            ctx += temp + "：";
                                        else
                                            ctx += temp + "\r\n";
                                    }
                                }

                                buildUnit = ctx.GetBuildRegex();
                                code = ctx.GetCodeRegex().GetCodeDel();
                                bidUnit = ctx.GetBidRegex();
                                bidMoney = ctx.GetMoneyRegex();
                                prjMgr = ctx.GetMgrRegex();
                            }

                            msgType = "珠海市公共资源交易中心";
                            specType = "建设工程";
                            bidType = prjName.GetInviteBidType();
                            BidInfo info = ToolDb.GenBidInfo("广东省", "珠海市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, string.Empty, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://ggzy.zhuhai.gov.cn/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
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
