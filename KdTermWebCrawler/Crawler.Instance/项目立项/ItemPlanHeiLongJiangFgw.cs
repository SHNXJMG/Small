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
    public class ItemPlanHeiLongJiangFgw : WebSiteCrawller
    {
        public ItemPlanHeiLongJiangFgw()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "黑龙江省发展和改革委员会项目立项";
            this.Description = "自动抓取黑龙江省发展和改革委员会项目立项";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.hljdpc.gov.cn/xzgs/index.jhtml";
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
            catch { }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagebar")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("/","页");
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.hljdpc.gov.cn/xzgs/index_" + i + ".jhtml");
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "right-list")), true), new TagNameFilter("dl")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    { 
                        ATag aTag = listNode[j].GetATag();
                        if (aTag == null) continue;
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        ItemName = aTag.GetAttribute("title");
                        PlanDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.hljdpc.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoNormalTable")));
                        if (dtlNode == null || dtlNode.Count < 1)
                        {
                            parser.Reset();
                            dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "700")));
                        }
                        if (dtlNode == null || dtlNode.Count < 1)
                        {
                            parser.Reset();
                            dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "590")));
                        }
                        if (dtlNode == null || dtlNode.Count < 1)
                        {
                            parser.Reset();
                            dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter( new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content")),true),new TagNameFilter("table")));
                        }
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            TableTag table = dtlNode[0] as TableTag;
                            for (int r = 0; r < table.RowCount; r++)
                            {
                                for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                                {
                                    string temp = table.Rows[r].Columns[c].ToNodePlainString();
                                    if ((c + 1) % 2 == 0)
                                        ItemCtx += temp.GetReplace(":,：") + "\r\n";
                                    else
                                        ItemCtx += temp.GetReplace(":,：") + "：";
                                }
                            }

                            ItemCode = ApprovalCode = ItemCtx.GetRegex("文件号");
                            ItemContent = ItemCtx.GetRegex("主要内容", true, 500);
                            ApprovalDate = ItemCtx.GetRegex("生成日期").GetDateRegex("yyyy年MM月dd日");
                            MsgUnit = ItemCtx.GetRegex("发布处室");
                            TotalInvest = ItemCtx.GetRegexBegEnd("总投资", "万元").GetChina();

                            PlanType = "行政公示 ";
                            MsgType = "黑龙江省发展和改革委员会";
                            ItemPlan info = ToolDb.GenItemPlan("黑龙江省", "黑龙江省及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
