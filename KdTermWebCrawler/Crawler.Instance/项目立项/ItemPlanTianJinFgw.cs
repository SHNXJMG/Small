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
    public class ItemPlanTianJinFgw : WebSiteCrawller
    {
        public ItemPlanTianJinFgw()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "天津市发展和改革委员会项目立项";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取天津市发展和改革委员会项目立项";
            this.SiteUrl = "http://www.tjzfxxgk.gov.cn/tjep/showDepDirInfos1.jsp?year=2015&depcode=AAA02B";
            this.ExistCompareFields = "InfoUrl";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter( new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "text01")),true),new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count-1].GetATagHref().GetReplace("(","（").GetReplace(")","）").GetRegexBegEnd("（", "）");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&currentPage=" + i,Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[listNode.Count-1] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();
                        ItemName = aTag.GetAttribute("title"); 
                        PlanDate = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.tjzfxxgk.gov.cn/tjep/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "98%")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            ItemCtx = CtxHtml.ToLower().GetReplace("</p>,<br/>,<br>", "\r\n").ToCtxString();
                            string ctx = string.Empty;
                            parser = new Parser(new Lexer(CtxHtml));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("style", "width:100%")));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag tag = tableNode[0] as TableTag;
                                for (int r = 0; r < tag.RowCount; r++)
                                {
                                    for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = tag.Rows[r].Columns[c].ToNodePlainString();
                                        if ((c + 1) % 2 == 0)
                                            ctx += temp.GetReplace(":,：") + "\r\n";
                                        else
                                            ctx += temp.GetReplace(":,：") + "：";
                                    }
                                } 
                                ApprovalCode = ctx.GetRegex("文号");
                                ItemCode = ctx.GetRegex("索引号");
                                ApprovalDate = ctx.GetRegex("发文日期");
                                PlanType = ctx.GetRegex("主题分类");
                            }
                      
                            TotalInvest = ItemCtx.GetRegexBegEnd("投资", "万元").GetChina();
                            if (string.IsNullOrEmpty(TotalInvest))
                            {
                                TotalInvest = ItemCtx.GetRegexBegEnd("投资", "亿元").GetChina();
                                if (!string.IsNullOrEmpty(TotalInvest))
                                {
                                    try
                                    {
                                        TotalInvest = (decimal.Parse(TotalInvest) * 10000).ToString();
                                    }
                                    catch { }
                                }
                            }
                            MsgUnit = "天津市发展和改革委员会";  
                            MsgType = "天津市发展和改革委员会";
                            ItemPlan info = ToolDb.GenItemPlan("天津市", "天津市区", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);
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
