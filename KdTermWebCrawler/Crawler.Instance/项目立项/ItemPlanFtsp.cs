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
    public class ItemPlanFtsp : WebSiteCrawller
    {
        public ItemPlanFtsp()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "深圳市福田政府在线项目审批信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市福田政府在线项目审批信息";
            this.SiteUrl = "http://www.szft.gov.cn/zf/gcjsly/gclyxmxx/xmspgkxx/xmspxx/";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pager")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().Replace("createPageHTML", "").Replace(" 0,", "").Replace("(", "").Replace(")", "").Replace("index", "").Replace("html", "").Replace(",", "").Replace("\"", "").Replace(";", "").Trim(); ;
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl+"index_"+(i-1).ToString()+".html");
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "listul")),true),new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        ATag aTag = listNode[j].GetATag();
                        ItemName = aTag.GetAttribute("title");
                        InfoUrl = "http://www.szft.gov.cn/" + aTag.Link.Replace("../","").Replace("./","");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "contenter")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            ItemCtx = CtxHtml.ToCtxString();
                            PlanDate = ItemCtx.GetRegex("信息发布日期").GetDateRegex();
                            if (string.IsNullOrEmpty(PlanDate))
                                PlanDate = ItemCtx.GetDateRegex();
                            if (string.IsNullOrEmpty(PlanDate))
                                PlanDate = DateTime.Now.ToString("yyyy-MM-dd");
                            ItemCode = ItemCtx.GetRegex("项目编码").Replace("　", "");
                            string ctx = string.Empty;
                            parser = new Parser(new Lexer(CtxHtml));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag table = tableNode[0] as TableTag;
                                if (table.RowCount >= 2)
                                {
                                    TableRow tr = table.Rows[0];
                                    TableRow trC = table.Rows[1];
                                    for (int k = 0; k < tr.ColumnCount; k++)
                                    {
                                        ctx += tr.Columns[k].ToNodePlainString() + "：";
                                        ctx += trC.Columns[k].ToNodePlainString() + "\r\n";
                                    }
                                    if (string.IsNullOrEmpty(ItemCode))
                                        ItemCode = ctx.GetRegex("序号");
                                    BuildUnit = ctx.GetRegex("建设单位");
                                    BuildNature = ctx.GetRegex("建设性质");
                                    TotalInvest = ctx.GetRegex("总投资（万元）,总投资");
                                    PlanInvest = ctx.GetRegex("本期计划（万元）,本期计划");
                                    IssuedPlan = ctx.GetRegex("累计已下达计划（万元）,累计已下达计划");
                                    InvestSource = ctx.GetRegex("资金来源");
                                    ItemContent = ctx.GetRegex("主要建设内容,建设内容");
                                    if (string.IsNullOrEmpty(ItemContent))
                                        ItemContent = trC.Columns[trC.ColumnCount - 1].ToNodePlainString();
                                }
                            }

                            PlanType = "项目审批信息";
                            MsgType = "深圳市福田区发展和改革局";

                            ItemPlan info = ToolDb.GenItemPlan("广东省", "深圳市区", "福田区", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
