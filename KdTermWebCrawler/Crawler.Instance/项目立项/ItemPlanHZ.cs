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
    public class ItemPlanHZ : WebSiteCrawller
    {
        public ItemPlanHZ()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "湖南省发展和改革委员会项目立项核准信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取湖南省发展和改革委员会项目立项核准信息";
            this.SiteUrl = "http://www.hnfgw.gov.cn/xxgk/spgbhz/Index.html";
            this.MaxCount = 1200;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ItemPlan>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "pagenav")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].GetATagHref();
                    pageInt = int.Parse(temp.GetReplace(new string[] { "/xxgk/spgbhz/Index_", ".html" }));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.hnfgw.gov.cn/xxgk/spgbhz/Index_" + i + ".html");
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "list14bline")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        ATag aTag = listNode[j].GetATag();
                        if (aTag == null) continue;
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        ItemName = aTag.GetAttribute("title").Replace("我委核准", "");
                        PlanDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.hnfgw.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "PrintTxt")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            ItemCtx = CtxHtml.ToCtxString();
                            TotalInvest = ItemCtx.GetRegexBegEnd("总投资", "万元").GetChina();

                            PlanType = "项目核准信息";
                            MsgType = "湖南省发展和改革委员会";
                            ItemPlan info = ToolDb.GenItemPlan("湖南省", "湖南省及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
