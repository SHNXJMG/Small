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
    public class BidSztws : WebSiteCrawller
    {
        public BidSztws()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "深圳市天威视讯股份有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳市天威视讯股份有限公司招标信息";
            this.SiteUrl = "http://www.topway.com.cn/6/13/default.htm";
            this.MaxCount = 1000;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "pagination")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 2].GetATag().Link.Replace("31", "").Replace(".htm", "");
                    pageInt = int.Parse(temp);
                }
                catch
                {
                    pageInt = 27;
                }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.topway.com.cn/6/13/default_" + (i - 1).ToString() + ".htm", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("id", "newsList")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        prjName = listNode[j].GetATagValue("title");
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();

                        InfoUrl = "http://www.topway.com.cn/6/13/" + listNode[j].GetATagHref();

                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "fwRight float_l marginL_10")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml().Replace("<br>", "\r\n").Replace("<br/>", "\r\n").Replace("<br />", "\r\n");
                            bidCtx = HtmlTxt.ToCtxString();
                            code = bidCtx.GetRegexBegEnd("招标编号为", "的", 50).Replace("“", "").Replace("”", "");
                            bidUnit = bidCtx.GetRegexBegEnd("第一中标候选人为", "，");
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegexBegEnd("公司确定", "为第一部分改造");
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegexBegEnd("确定中标人是", "。");
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegexBegEnd("公司确定", "为本次");
                            specType = "其他";
                            bidType = prjName.GetInviteBidType();
                            msgType = "深圳市天威视讯股份有限公司";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
