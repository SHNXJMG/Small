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
    public class ItemPlanShangHai : WebSiteCrawller
    {
        public ItemPlanShangHai()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "上海市发展和改革委员会项目立项审批信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取上海市发展和改革委员会项目立项审批信息";
            this.SiteUrl = "http://www.shdrc.gov.cn/sub1_new.jsp?main_colid=534&top_id=312";
            this.MaxCount = 400;
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl,Encoding.Default);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("class", "form")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    SelectTag tag = pageNode[0] as SelectTag; 
                    string temp = tag.OptionTags[tag.OptionTags.Length - 1].Value;
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&dqy=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "700")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount-2; j++)
                    {
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;
                        TableRow tr = table.Rows[j].GetTableTag().Rows[0];

                        ATag aTag = tr.Columns[1].GetATag();
                        ItemName = aTag.GetAttribute("title");
                        ItemCode = tr.Columns[0].ToNodePlainString();
                        ApprovalCode = tr.Columns[2].ToNodePlainString();
                        PlanDate ="20"+ tr.Columns[3].ToPlainTextString().GetDateRegex("yy-MM-dd");
                        InfoUrl = "http://www.shdrc.gov.cn/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "maintitle2")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            ItemCtx = CtxHtml.GetReplace("<!--", "<span>").GetReplace("-->", "<span>").ToCtxString().GetReplace("begin,end,-->,<--");
                            parser = new Parser(new Lexer(CtxHtml));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "text3")));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                string ctx = string.Empty;
                                TableTag tag = tableNode[0] as TableTag;
                                for (int r = 0; r < tag.RowCount; r++)
                                {
                                    for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                    {
                                        if ((c + 1) % 2 == 0)
                                            ctx += tag.Rows[r].Columns[c].ToNodePlainString().GetReplace("begin,end").ToCtxString()+"\r\n";
                                        else
                                            ctx += tag.Rows[r].Columns[c].ToNodePlainString().GetReplace("begin,end").ToCtxString() + "：";
                                    }
                                }
                                string code = ctx.GetRegex("项目编码");
                                ItemCode = code == "" ? ItemCode : code;
                                ItemContent = ctx.GetRegex("内容", true, 500);
                                ApprovalUnit = ctx.GetRegex("批复机关");
                                ApprovalDate = ctx.GetRegex("批复时间").GetDateRegex();
                            }
                            MsgUnit = "上海市发展和改革委员会";
                            PlanType = "项目审批信息";
                            MsgType = "上海市发展和改革委员会";
                            ItemPlan info = ToolDb.GenItemPlan("上海市", "上海市区", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
