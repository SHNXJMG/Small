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
    public class BidJiLinGgzy : WebSiteCrawller
    {
        public BidJiLinGgzy()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "吉林省公共资源交易中心中标信息";
            this.Description = "自动抓取吉林省公共资源交易中心中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://ggzyjy.jl.gov.cn/JiLinZtb/Template/Default/MoreInfoJYXX.aspx?CategoryNum=004002";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch { }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("nowrap", "true")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("总页数：", "当前");
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
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__LASTFOCUS",
                    "__VIEWSTATE",
                    "__EVENTVALIDATION"
                    }, new string[]{
                    "Pager",
                    i.ToString(),
                    "",
                    viewState,
                    eventValidation
                   
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("tr"));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        TableRow tr = listNode[j] as TableRow;
                        if (tr.ColumnCount != 6) continue;
                        ATag aTag = tr.GetATag();
                        if (aTag == null) continue;
                        string prjType = tr.Columns[2].ToNodePlainString().GetReplace("[", "【").GetReplace("]", "】").GetRegexBegEnd("【", "】");
                        if (!prjType.Contains("水利工程") && !prjType.Contains("建设工程") && !prjType.Contains("交通工程"))
                            continue;

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
                        prjName = aTag.GetAttribute("title");
                        InfoUrl = "http://ggzyjy.jl.gov.cn/JiLinZtb/" + aTag.Link.GetReplace("../,./");
                        area = tr.Columns[3].ToNodePlainString().GetReplace("[", "【").GetReplace("]", "】").GetRegexBegEnd("【", "】");
                        beginDate = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,<br/>,</tr>", "\r\n").ToCtxString();
                            string ctx = string.Empty;
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "_Sheet1")));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag tag = tableNode[0] as TableTag;
                                for (int r = 0; r < tag.Rows[0].ColumnCount; r++)
                                {
                                    try
                                    {
                                        ctx += tag.Rows[0].Columns[r].ToNodePlainString() + "：";
                                        ctx += tag.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                    }
                                    catch { }
                                }
                            }
                            bidUnit = ctx.GetBidRegex();
                            bidMoney = ctx.GetMoneyRegex();
                            prjMgr = ctx.GetMgrRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                //_Sheet1_3_0
                                ctx = string.Empty;
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tagNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "_Sheet1_3_0")));
                                if (tagNode != null && tagNode.Count > 0)
                                {
                                    TableTag tag = tagNode[0] as TableTag;
                                    if (tag.RowCount > 1)
                                    {
                                        for (int r = 0; r < tag.Rows[1].ColumnCount; r++)
                                        {
                                            try
                                            {
                                                ctx += tag.Rows[1].Columns[r].ToNodePlainString() + "：";
                                                ctx += tag.Rows[2].Columns[r].ToNodePlainString() + "\r\n";
                                            }
                                            catch { }
                                        }
                                    }
                                }
                                bidUnit = ctx.GetBidRegex();
                                if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                    bidMoney = ctx.GetMoneyRegex();
                                if (string.IsNullOrEmpty(prjMgr))
                                    prjMgr = ctx.GetMgrRegex();
                            }
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                bidUnit = bidCtx.GetBidRegex();
                                if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                    bidMoney = bidCtx.GetMoneyRegex();
                                if (string.IsNullOrEmpty(prjMgr))
                                    prjMgr = bidCtx.GetMgrRegex();
                            }

                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            msgType = "吉林省公共资源交易中心";
                            specType = "建设工程";
                            bidType = prjType;
                            BidInfo info = ToolDb.GenBidInfo("吉林省", "吉林省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
