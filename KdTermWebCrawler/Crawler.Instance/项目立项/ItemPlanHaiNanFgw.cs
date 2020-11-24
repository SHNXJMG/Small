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
    public class ItemPlanHaiNanFgw : WebSiteCrawller
    {
        public ItemPlanHaiNanFgw()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "海南省发展和改革委员会项目审批信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取海南省发展和改革委员会项目审批信息";
            this.SiteUrl = "http://plan.hainan.gov.cn/fzggzl/xmsp/";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 800;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("dt"), new HasAttributeFilter("class", "ny_my")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().Replace("(", "（").GetRegexBegEnd("（", ",");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "index_" + (i - 1) + ".html");
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("dt"), new HasAttributeFilter("class", "ny_news")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        ItemName = aTag.LinkText;
                        PlanDate = node.ToPlainTextString().GetDateRegex();
                        if (aTag.Link.ToLower().Contains("http"))
                            InfoUrl = aTag.Link;
                        else
                            InfoUrl = "http://plan.hainan.gov.cn/fzggzl/xmsp/" + aTag.Link.GetReplace("../,./");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "1000")));
                        if (dtlNode != null && dtlNode.Count > 1)
                        {
                            CtxHtml = dtlNode[0].ToHtml() + dtlNode[1].ToHtml();
                            ItemCtx = CtxHtml.ToCtxString();

                            ApprovalUnit = ItemCtx.GetRegex("发文机构");
                            ItemCode = ItemCtx.GetRegex("索引号");
                            ApprovalCode = ItemCtx.GetRegex("文号");
                            ApprovalDate = ItemCtx.GetDateRegex("yyyy年MM月dd日");
                            PlanType = "项目审批信息";
                            MsgType = "海南省发展和改革委员会";
                            ItemPlan info = ToolDb.GenItemPlan("海南省", "海南省及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);
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
