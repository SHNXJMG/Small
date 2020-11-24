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
    public class ItemPlanGuiZhouFg : WebSiteCrawller
    {
        public ItemPlanGuiZhouFg()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "贵州省发展和改革委员会项目立项";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取贵州省发展和改革委员会项目立项";
            this.SiteUrl = "http://www.gzdpc.gov.cn/col/col402/index.html";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 400;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ItemPlan>();
            string html = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("script"), new HasAttributeFilter("language", "javascript")));
            if (pageNode != null && pageNode.Count > 3)
            {
                string strHtml = pageNode[3].ToHtml();
                int startLen = strHtml.IndexOf("i=0;");
                int endLen = strHtml.IndexOf("formatstr");
                string ctx = strHtml.Substring(startLen, endLen - startLen).GetReplace("var,i=0;").GetReplace("i++;", "&");
                string[] htmls = ctx.Split('&');
                foreach (string str in htmls)
                {
                    if (string.IsNullOrEmpty(str)) continue;
                    string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                    InfoUrl = "http://www.gzdpc.gov.cn" + str.GetRegexBegEnd("urls", ";").GetReplace("=,[i],'");
                    string y = str.GetRegexBegEnd("year", ";").GetReplace("=,[i],'");
                    string m = str.GetRegexBegEnd("month", ";").GetReplace("=,[i],'");
                    string d = str.GetRegexBegEnd("day", ";").GetReplace("=,[i],'");
                    PlanDate = y + "-" + m + "-" + d;
                    ItemName = str.GetRegexBegEnd("headers", ";").GetReplace("=,[i],',·");
                    string htmldtl = string.Empty;
                    try
                    {
                        htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                    }
                    catch { continue; }
                    parser = new Parser(new Lexer(htmldtl));
                    NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "article")));
                    if (dtlNode != null && dtlNode.Count > 0)
                    {
                        CtxHtml = dtlNode.AsHtml();
                        ItemCtx = CtxHtml.ToLower().GetReplace("<br/>,<br>,</p>", "").ToCtxString().GetReplace("begin,end,<--,-->");
                        if (ItemName.Contains("..."))
                        {
                            parser = new Parser(new Lexer(CtxHtml));
                            NodeList titleNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "title")));
                            if (titleNode != null && titleNode.Count > 0)
                            {
                                ItemName = titleNode[0].ToNodePlainString().GetReplace("begin,end,<$[标题]>");
                            }
                        }
                        TotalInvest = ItemCtx.GetRegex("总投资").GetReplace("万元");
                        BuildNature = ItemCtx.GetRegex("建设性质");
                        BuildUnit = ItemCtx.GetBuildRegex();
                        ItemAddress = ItemCtx.GetAddressRegex();
                        PlanType = "项目信息";
                        MsgType = "贵州省发展和改革委员会";
                        ItemPlan info = ToolDb.GenItemPlan("贵州省", "贵州省及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
