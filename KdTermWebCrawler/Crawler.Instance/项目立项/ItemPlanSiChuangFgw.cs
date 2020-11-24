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
    public class ItemPlanSiChuangFgw : WebSiteCrawller
    {
        public ItemPlanSiChuangFgw()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "四川省发展和改革委员会项目核准备案";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取四川省发展和改革委员会项目核准备案";
            this.SiteUrl = "http://www.scdrc.gov.cn/Scdr2Aspx/title1.aspx?TOP=1111&NAME=%e9%a1%b9%e7%9b%ae%e6%a0%b8%e5%87%86%e5%a4%87%e6%a1%88&SKIP=";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "-1");
            }
            catch { return null; }

            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "m_COUNT")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString().GetRegexBegEnd("/", "）");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + ((i - 1) * 24));
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "m_TAB")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        if (aTag == null) continue;
                        ItemName = tr.Columns[1].ToNodePlainString();
                        if (ItemName.Contains("..."))
                            aTag.GetAttribute("title");
                        PlanDate = "20" + tr.Columns[2].ToPlainTextString().GetDateRegex("yy-MM-dd");

                        InfoUrl = "http://www.scdrc.gov.cn" + aTag.Link;//aTag.Link.GetReplace(".htm", "_1.htm");

                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList IsNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("iframe"), new HasAttributeFilter("id", "m_FRAME")));
                        if (IsNode != null && IsNode.Count > 0)
                        {
                            try
                            {
                                InfoUrl = "http://www.scdrc.gov.cn" + aTag.Link.GetReplace(".htm", "_1.htm");
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                            }
                            catch { continue; }
                        }


                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "m_TEXT")));
                        if (dtlNode == null || dtlNode.Count < 1)
                        {
                            parser.Reset();
                            dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        }
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            parser = new Parser(new Lexer(CtxHtml));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag tag = tableNode[0] as TableTag;
                                for (int r = 0; r < tag.RowCount; r++)
                                {
                                    for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = tag.Rows[r].Columns[c].ToNodePlainString();
                                        if ((c + 1) % 2 == 0)
                                            ItemCtx += temp.GetReplace(":,：") + "\r\n";
                                        else
                                            ItemCtx += temp.GetReplace(":,：") + "：";
                                    }
                                }
                            }
                            else
                                ItemCtx = CtxHtml.ToCtxString();
                            ItemContent = ItemCtx.GetRegex("内容", true, 1000);
                            ApprovalUnit = ItemCtx.GetRegex("批复单位");
                            ApprovalDate = ItemCtx.GetRegex("批复日期,批复时间");
                            ApprovalCode = ItemCtx.GetRegex("批复文号（备案号）");
                            TotalInvest = ItemCtx.GetRegex("总投资").GetMoney();
                            PlanBeginDate = ItemCtx.GetRegex("开工时间");
                            ItemAddress = ItemCtx.GetRegex("所属地区");
                            PlanType = ItemCtx.GetRegex("项目类型");
                            MsgType = "四川省发展和改革委员会";
                            ItemName = ItemName.GetReplace("四川省发展和改革委员会");
                            ItemPlan info = ToolDb.GenItemPlan("四川省", "四川省及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);
                            parser = new Parser(new Lexer(CtxHtml));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k] as ATag;
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://www.scdrc.gov.cn/dir1111/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
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
