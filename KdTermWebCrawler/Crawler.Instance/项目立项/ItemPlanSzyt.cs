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
    public class ItemPlanSzyt : WebSiteCrawller
    {
        public ItemPlanSzyt() :
            base()
        {
            this.Group = "项目立项";
            this.Title = "深圳市盐田区工程领域项目审批信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市盐田区工程领域项目审批信息";
            this.SiteUrl = "http://www.yantian.gov.cn/icatalog/bm/fzhggj/04/gcjszl/gkml/xmsp/xmspxx/";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "right")), true), new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("align", "right"))));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode.AsString().GetRegexBegEnd("共", "页");
                try
                {
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "/index_" + (i - 1).ToNodeString() + ".shtml", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"),new HasAttributeFilter("width","100%")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        ATag aTag = tr.Columns[1].GetATag();
                        string tempName = aTag.GetAttribute("title");
                        PlanDate = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        ItemName = tempName.GetRegexBegEnd("&ldquo;", "&rdquo;");
                        if (string.IsNullOrEmpty(ItemName))
                            ItemName = tempName.GetRegexBegEnd("关于下达","政府投资项目");
                        if (string.IsNullOrEmpty(ItemName))
                            ItemName = tempName.Replace("关于下达", "").Replace("&rdquo;", "");

                        InfoUrl = "http://www.yantian.gov.cn" + aTag.Link;

                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            ItemCtx = CtxHtml.ToCtxString();
                            parser = new Parser(new Lexer( CtxHtml));
                            NodeList pNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("p"));
                            if (pNode != null && pNode.Count > 0)
                            {
                                BuildUnit = pNode[0].ToNodePlainString().Replace("：","").Replace(":","");
                            }
                            TotalInvest = ItemCtx.GetRegexBegEnd("项目总投资", "万元");
                            if (string.IsNullOrEmpty(TotalInvest))
                                TotalInvest = ItemCtx.GetRegexBegEnd("项目总投资共", "万元");
                            IssuedPlan = ItemCtx.GetRegexBegEnd("本次下达资金", "万元");
                            if (string.IsNullOrEmpty(IssuedPlan))
                                IssuedPlan = ItemCtx.GetRegexBegEnd("下达资金", "万元");
                            if (string.IsNullOrEmpty(IssuedPlan))
                                IssuedPlan = ItemCtx.GetRegexBegEnd("本次下达前期费用", "万元");

                            PlanType = "项目审批信息";
                            MsgType = "深圳市盐田区";

                            ItemPlan info = ToolDb.GenItemPlan("广东省", "深圳市区", "盐田区", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
