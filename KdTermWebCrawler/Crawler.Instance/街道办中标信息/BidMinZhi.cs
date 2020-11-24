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
    public class BidMinZhi : WebSiteCrawller
    {
        public BidMinZhi()
            : base()
        {
            this.Group = "街道办中标信息";
            this.Title = "广东省深圳市民治街道办事处";
            this.Description = "自动抓取广东省深圳市民治街道办事处中标信息";
            this.PlanTime = "9:22,13:53";
            this.SiteUrl = "http://www.szlhxq.gov.cn/mzbsc/zwgk69/cgzb/zbgg21/index.html";
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
            catch  
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "yesh fl")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToNodePlainString().GetRegexBegEnd("/", "页");
                    pageInt = int.Parse(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.szlhxq.gov.cn/mzbsc/zwgk69/cgzb/zbgg21/14844-" + i.ToString() + ".html", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "news1_list")), true), new TagNameFilter("li")));
                if (viewList != null && viewList.Count > 0)
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

                        beginDate = viewList[j].ToNodePlainString().GetDateRegex();
                        ATag aTag = viewList[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        InfoUrl = "http://www.szlhxq.gov.cn" + aTag.Link;
                        string htmDtl = string.Empty;
                        try
                        {
                            htmDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tit-content")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml();
                            bidCtx = System.Text.RegularExpressions.Regex.Replace(HtmlTxt, "(<script)[\\s\\S]*?(</script>)", "");
                            bidCtx = System.Text.RegularExpressions.Regex.Replace(bidCtx.Replace("<br/>", "\r\n").Replace("<BR/>", "\r\n").Replace("<BR>", "\r\n").Replace("<br>", "\r\n"), "<[^>]*>", "").Replace("&nbsp;", "").Replace(" ", "").Replace("\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\r\n", "\r\n").Replace("\r\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\t", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");

                            bidType = prjName.GetInviteBidType();
                            if (string.IsNullOrEmpty(bidType))
                            {
                                bidType = "工程";
                            }
                            code = ToolHtml.GetRegexString(bidCtx, ToolHtml.CodeRegex, true, 50);
                            buildUnit = ToolHtml.GetRegexString(bidCtx, ToolHtml.BuildRegex, true, 150);
                            bidMoney = ToolHtml.GetRegexString(bidCtx, ToolHtml.MoneyRegex, false);
                            bidUnit = ToolHtml.GetRegexString(bidCtx, ToolHtml.BidRegex, true, 150);
                            prjMgr = ToolHtml.GetRegexString(bidCtx, ToolHtml.MgrRegex, true, 50);
                            bidMoney = ToolHtml.GetRegexMoney(bidMoney);
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "深圳市龙华新区民治街道办事处";
                            }
                            msgType = "深圳市龙华新区民治街道办事处";
                            specType = "建设工程";
                            bidType = "小型工程";
                            prjName = ToolDb.GetPrjName(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳区及街道工程", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                   bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
