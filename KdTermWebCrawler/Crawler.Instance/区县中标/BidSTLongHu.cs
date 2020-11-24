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

namespace Crawler.Instance
{
    public class BidSTLongHu : WebSiteCrawller
    {
        public BidSTLongHu()
            : base()
        {
            this.Group = "区县中标信息";
            this.Title = "广东省汕头市龙湖区政府";
            this.Description = "自动抓取广东省汕头市龙湖区政府";
            this.PlanTime = "9:36,10:38,14:28,16:39";
            this.SiteUrl = "http://www.gdlonghu.gov.cn/Gb/Home/ShowList.aspx?CateID=002160020";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("noWrap", "true")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().ToNodeString();
                    Regex reg = new Regex(@"/[^页]+页");
                    string page = reg.Match(temp).Value.Replace("/", "").Replace("页", "");
                    pageInt = Convert.ToInt32(page);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                { 
                    try
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                            new string[]
                            {"__VIEWSTATE",
                                "__EVENTTARGET",
                            "__EVENTARGUMENT",
                            "__EVENTVALIDATION",
                            "tbSearchText"},
                                new string[]{
                                viewState,
                                "pager",
                                i.ToString(),
                                eventValidation,
                                ""
                                });
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ShowListMiddleContent")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    parser = new Parser(new Lexer(nodeList.ToHtml()));
                    NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ShowListTitle")));
                    parser = new Parser(new Lexer(nodeList.ToHtml()));
                    NodeList timeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ShowListTime")));
                    if (viewList != null && viewList.Count > 0 && timeList != null && timeList.Count > 0 && timeList.Count == viewList.Count)
                    {
                        for (int j = 0; j < viewList.Count; j++)
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
                       prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                            prjName = viewList[j].GetATagValue("title");
                            bidType = prjName.GetInviteBidType();
                            beginDate = timeList[j].ToNodePlainString().GetDateRegex();
                            InfoUrl = "http://www.gdlonghu.gov.cn/Gb/Home/" + viewList[j].GetATagHref();
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }

                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "lblContent")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                bidCtx = HtmlTxt.ToCtxString();

                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex();
                                if (string.IsNullOrEmpty(bidUnit))
                                {
                                    string ctx = string.Empty;
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList tabList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                    if (tabList != null && tabList.Count > 0)
                                    {
                                        try
                                        {
                                            TableTag tab = tabList[0] as TableTag;
                                            if (tab.RowCount > 2 && tab.Rows[0].ColumnCount > 0)
                                            {
                                                for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                                {
                                                    ctx += tab.Rows[0].Columns[d].ToNodePlainString().Replace("价格得分", "得分").Replace("<?xml:namespaceprefix=ons=\"urn:schemas-microsoft-com:office:office\"/>", "") + "：";
                                                    ctx += tab.Rows[2].Columns[d].ToNodePlainString().Replace("\r", "").Replace("\n", "") + "\r\n";
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                    bidUnit = ctx.GetBidRegex();
                                    bidMoney = ctx.GetMoneyRegex();
                                    if (bidUnit.Contains("商务及技术"))
                                    {
                                        ctx = string.Empty;
                                        if (tabList != null && tabList.Count > 0)
                                        {
                                            try
                                            {
                                                TableTag tab = tabList[0] as TableTag;
                                                if (tab.RowCount > 0 && tab.Rows[0].ColumnCount > 1)
                                                {
                                                    ctx += tab.Rows[0].Columns[0].ToNodePlainString() + "：";
                                                    ctx += tab.Rows[0].Columns[1].ToNodePlainString() + "\r\n";
                                                }
                                            }
                                            catch { }
                                        }
                                        bidUnit = ctx.GetBidRegex();
                                    }
                                }
                                if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                {
                                    bidMoney.GetMoneyRegex();
                                }
                                prjAddress = bidCtx.GetAddressRegex();
                                code = bidCtx.GetCodeRegex();
                                buildUnit = bidCtx.GetBuildRegex();

                                msgType = "汕头龙湖区政府";
                                specType = "建设工程";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "汕头市区", "龙湖区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                       bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aList != null && aList.Count > 0)
                                {
                                    for (int c = 0; c < aList.Count; c++)
                                    {
                                        ATag a = aList[c] as ATag;
                                        if (a.Link.IsAtagAttach())
                                        {
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, "http://www.gdlonghu.gov.cn" + a.Link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }
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
