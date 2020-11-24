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
    public class ItemPlanSzHz : WebSiteCrawller
    {
        public ItemPlanSzHz() :
            base()
        {
            this.Group = "项目立项";
            this.Title = "深圳市发展和改革委员会项目核准";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市发展和改革委员会项目核准";
            this.SiteUrl = "http://www.szpb.gov.cn/fgzl/gcjslyxm/xmsp/xmhz/";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "arContent")));
            if (pageNode != null && pageNode.Count > 0)
            {
                TableTag pageTable = pageNode[0] as TableTag;
                string temp = pageTable.Rows[pageTable.RowCount - 1].ToNodePlainString().Replace("createPageHTML", "").Replace("0,", "").Replace("(", "").Replace(")", "").Replace("index", "").Replace("htm", "").Replace(",", "").Replace("\"", "").Replace(";", "").Trim();
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "/index_" + (i - 1).ToString() + ".htm", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "arContent")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        ATag aTag = tr.Columns[1].GetATag();
                        ItemName = aTag.GetAttribute("title");
                        ItemCode = tr.Columns[2].ToNodePlainString();
                        PlanDate = tr.Columns[3].ToPlainTextString().GetDateRegex();

                        InfoUrl = this.SiteUrl + aTag.Link.Replace("../", "").Replace("./", "");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "detail")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            ItemCtx = CtxHtml.ToCtxString();
                            string ctx = string.Empty;
                            parser = new Parser(new Lexer(CtxHtml));
                            NodeList dtlTable = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                            if (dtlTable != null && dtlTable.Count > 0)
                            {
                                TableTag tableTag = dtlTable[0] as TableTag;
                                for (int k = 0; k < tableTag.RowCount; k++)
                                {
                                    for (int c = 0; c < tableTag.Rows[k].ColumnCount; c++)
                                    {
                                        if (c % 2 == 0)
                                            ctx += tableTag.Rows[k].Columns[c].ToNodePlainString().Replace("：", "").Replace(":", "") + "：";
                                        else
                                            ctx += tableTag.Rows[k].Columns[c].ToNodePlainString() + "\r\n";
                                    }
                                }
                            }

                            MsgUnit = ctx.GetRegex("发布单位");
                            if (string.IsNullOrEmpty(MsgUnit))
                                MsgUnit = "发改委";
                            PlanType = "项目核准信息";
                            MsgType = "深圳市发展和改革委员会";

                            ItemPlan info = ToolDb.GenItemPlan("广东省", "深圳市区", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
