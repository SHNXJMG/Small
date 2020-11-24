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
    public class ItemPlanSzlh : WebSiteCrawller
    {
        public ItemPlanSzlh() :
            base()
        {
            this.Group = "项目立项";
            this.Title = "深圳市罗湖区工程领域项目审批信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市罗湖区工程领域项目审批信息";
            this.SiteUrl = "http://www.szlh.gov.cn/icatalog/04/gcjs/infoList.shtml?departmentCode=B201&id=5509";
            this.MaxCount = 500;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pageNavigate")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode.AsString().GetRegexBegEnd("/共", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&pageNo=" + i, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "listTable")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        MsgUnit = tr.Columns[3].ToNodePlainString();
                        ApprovalCode = tr.Columns[1].ToNodePlainString();
                        PlanDate = tr.Columns[4].ToPlainTextString().GetDateRegex("yyyy/MM/dd");
                        ATag aTag = tr.Columns[2].GetATag();
                        string tempName = aTag.LinkText.Replace("\n", "").Replace("\t", "").Replace("\r","").Trim();
                        ItemName = tempName.GetRegexBegEnd("关于下达", "项目");
                        if (string.IsNullOrEmpty(ItemName))
                            ItemName = tempName.GetRegexBegEnd("关于调整下达", "项目");
                        if (string.IsNullOrEmpty(ItemName))
                            ItemName = tempName.GetRegexBegEnd("关于预安排", "项目");
                        if (string.IsNullOrEmpty(ItemName))
                            ItemName = tempName.GetRegexBegEnd("关于追加", "项目");
                        if (string.IsNullOrEmpty(ItemName))
                            ItemName = tempName.GetRegexBegEnd("关于", "项目");
                        if (string.IsNullOrEmpty(ItemName))
                            ItemName = tempName;
                        InfoUrl = "http://www.szlh.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                        }
                        catch { continue; }

                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "main2")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            ItemCtx = CtxHtml.Replace("</p>","\r\n").Replace("</tr>","\r\n").ToCtxString();
                            TotalInvest = ItemCtx.GetRegexBegEnd("总投资控制在", "万元");
                            PlanInvest = ItemCtx.GetRegexBegEnd("计划安排建设资金", "万元");
                            if (string.IsNullOrEmpty(TotalInvest))
                            {
                                TotalInvest = ItemCtx.GetRegexBegEnd("计划项目总投资", "万元").Replace("为", ""); 
                            }
                            if (string.IsNullOrEmpty(TotalInvest))
                            {
                                TotalInvest = ItemCtx.GetRegexBegEnd("计划共安排建设资金", "万元");
                            }
                            if (string.IsNullOrEmpty(TotalInvest))
                            {
                                TotalInvest = ItemCtx.GetRegexBegEnd("计划共安排投资", "万元");
                            }
                            if (string.IsNullOrEmpty(TotalInvest) || string.IsNullOrEmpty(PlanInvest))
                            {  
                                parser = new Parser(new Lexer(CtxHtml));
                                NodeList inNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("align", "justify")));
                                if (inNode != null && inNode.Count > 0)
                                {
                                    string ctx = inNode.AsString();
                                    TotalInvest = ctx.GetRegexBegEnd("总投资控制在", "万元");
                                    PlanInvest = ctx.GetRegexBegEnd("计划安排建设资金", "万元");
                                    if (string.IsNullOrEmpty(TotalInvest))
                                    {
                                        TotalInvest = ItemCtx.GetRegexBegEnd("计划项目总投资", "万元").Replace("为","");
                                    }
                                    if (string.IsNullOrEmpty(TotalInvest))
                                    {
                                        TotalInvest = ItemCtx.GetRegexBegEnd("计划共安排建设资金", "万元");
                                    }
                                    if (string.IsNullOrEmpty(TotalInvest))
                                    {
                                        TotalInvest = ItemCtx.GetRegexBegEnd("计划共安排投资", "万元");
                                    }
                                }
                            }
                             
                            parser = new Parser(new Lexer(CtxHtml));
                            NodeList contentNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table_suoyin")));
                            if (contentNode != null && contentNode.Count > 0)
                            {
                                TableTag dtlTable = contentNode[0] as TableTag;
                                ItemContent = dtlTable.Rows[dtlTable.RowCount - 1].Columns[dtlTable.Rows[dtlTable.RowCount - 1].ColumnCount-1].ToNodePlainString();
                            }

                            PlanType = "项目审批信息";
                            MsgType = "深圳市罗湖区发改局";
                            ItemPlan info = ToolDb.GenItemPlan("广东省", "深圳市区", "罗湖区", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
