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
    public class ItemPlanLiaoLingFgw : WebSiteCrawller
    {
        public ItemPlanLiaoLingFgw()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "辽宁省发展和改革委员会项目立项";
            this.Description = "自动抓取辽宁省发展和改革委员会项目立项";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.lndp.gov.cn/Article_Class2.asp?ClassID=16";
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
                return null;
            }
            try
            {
                string temp = html.GetRegexBegEnd("<strong>", "</strong>").GetReplace("<fontcolor=red>1</font>/");//pageNode[0].ToNodePlainString().GetReplace("1/");
                pageInt = int.Parse(temp);
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellspacing", "5")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        ATag aTag = listNode[j].GetATag();
                        if (aTag == null) continue;
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        ItemName = aTag.LinkText.GetReplace("省发展改革委、,省发展改革委,&nbsp;");
                        PlanDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.lndp.gov.cn/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "200")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            parser = new Parser(new Lexer(CtxHtml));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag table = tableNode[0] as TableTag;
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
                            }
                            ItemContent = ItemCtx.GetRegex("内容概述", true, 500);
                            TotalInvest = ItemCtx.GetRegexBegEnd("总投资", "万元").GetChina();
                            ItemCode = ApprovalCode = ItemCtx.GetCodeRegex();
                            if (string.IsNullOrEmpty(ItemCode))
                                ItemCode = ApprovalCode = ItemCtx.GetRegex("编　　号");
                            PlanType = "项目信息";
                            MsgType = "辽宁省发展和改革委员会";
                            ItemPlan info = ToolDb.GenItemPlan("辽宁省", "辽宁省及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
