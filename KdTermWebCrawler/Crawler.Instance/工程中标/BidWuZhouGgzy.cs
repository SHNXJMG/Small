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
    public class BidWuZhouGgzy : WebSiteCrawller
    {
        public BidWuZhouGgzy()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "梧州市公共资源交易中心中标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取梧州市公共资源交易中心中标信息";
            this.SiteUrl = "http://www.wzgcjsx2.gx.cn/SonTextList.aspx?type=zbgg2";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "PagingControl_lblPage")));
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
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    string _VIEWSTATEGENERATOR = ToolHtml.GetHtmlInputValue(html, "__VIEWSTATEGENERATOR");
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "__VIEWSTATEGENERATOR",
                    "__EVENTVALIDATION",
                    "PagingControl$tbxpidex",
                    "PagingControl$btnGo"
                    }, new string[]{
                    "",
                    "",
                    viewState,
                    _VIEWSTATEGENERATOR,
                    eventValidation,
                    i.ToString(),
                    "go"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "right")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];

                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;

                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty,
                          bidMoney = string.Empty, code = string.Empty,
                          bidDate = string.Empty, beginDate = string.Empty,
                          endDate = string.Empty, bidType = string.Empty,
                          specType = string.Empty, InfoUrl = string.Empty,
                          msgType = string.Empty, bidCtx = string.Empty,
                          prjAddress = string.Empty, remark = string.Empty,
                          prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        prjName = aTag.GetAttribute("title").Replace(" ", "");
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.wzgcjsx2.gx.cn/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "Center_Introduction")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</p>,</br>","\r\n").ToCtxString();
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex(null, false, "万元");
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag table = tableNode[0] as TableTag;
                                    for (int r = 0; r < table.RowCount; r++)
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
                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                        bidMoney = ctx.GetMoneyRegex(null, false, "万元");
                                    if (string.IsNullOrWhiteSpace(prjMgr))
                                        prjMgr = ctx.GetMgrRegex();
                                    code = ctx.GetCodeRegex().GetCodeDel(); 
                                    buildUnit = ctx.GetBuildRegex();
                                }
                                else
                                {
                                    code = bidCtx.GetCodeRegex().GetCodeDel();
                                    buildUnit = bidCtx.GetBuildRegex();
                                }
                            }
                            else
                            {
                                code = bidCtx.GetCodeRegex().GetCodeDel(); 
                                buildUnit = bidCtx.GetBuildRegex();
                            }
                            try
                            {
                                if (decimal.Parse(bidMoney) > 10000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            msgType = "梧州市公共资源交易中心";
                            specType = "建设工程";
                            bidType = prjName.GetInviteBidType();
                            BidInfo info = ToolDb.GenBidInfo("广西壮族自治区", "广西壮族自治区及地市", "梧州市", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.wzgcjsx2.gx.cn/" + a.Link.GetReplace("../,./");
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
