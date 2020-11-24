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
    public class ItemPlanBeiJingFgw : WebSiteCrawller
    {
        public ItemPlanBeiJingFgw()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "北京市发展和改革委员会项目立项";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取北京市发展和改革委员会项目立项";
            this.SiteUrl = "http://www.bjpc.gov.cn/gcjs/";
            this.MaxCount = 1200;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ItemPlan>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 24;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "z_12")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].GetATagHref();
                    pageInt = int.Parse(temp.GetReplace("index_,.htm"));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "index_" +( i-1) + ".htm", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter( new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("valign", "top")),true),new AndFilter(new TagNameFilter("table"),new HasAttributeFilter("width","730"))));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    { 
                        TableRow tr = ( listNode[j] as TableTag).Rows[0];
                        ATag aTag = tr.Columns[1].GetATag();
                        if (aTag == null) continue;

                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        ItemName = aTag.LinkText;
                        PlanDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.bjpc.gov.cn/gcjs/" + aTag.Link.GetReplace("../,./");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "2")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml(); 
                            TableTag dtlTable = dtlNode[0] as TableTag;
                            for (int r = 0; r < dtlTable.RowCount; r++)
                            {
                                for (int c = 0; c < dtlTable.Rows[r].ColumnCount; c++)
                                {
                                    if ((c + 1) % 2 == 0)
                                        ItemCtx += dtlTable.Rows[r].Columns[c].ToNodePlainString() + "\r\n";
                                    else
                                        ItemCtx += dtlTable.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：") + "：";
                                }
                            }
                            ItemContent = ItemCtx.GetRegex("内容摘要", true, 500);
                            ItemCode = ApprovalCode = ItemCtx.GetRegex("审批文号");
                            ApprovalUnit = ItemCtx.GetRegex("批复单位");
                            ApprovalDate = ItemCtx.GetRegex("批复时间").GetDateRegex();
                            TotalInvest = ItemCtx.GetRegexBegEnd("总投资", "万元").GetChina();
                            if (ItemName.Contains(".."))
                            {
                                string temp = ItemCtx.GetRegex("项目名称");
                                ItemName = string.IsNullOrEmpty(temp) ? ItemName : temp;
                            }
                            PlanType = "项目信息";
                            MsgType = "北京市发展和改革委员会";
                            ItemPlan info = ToolDb.GenItemPlan("北京市", "北京市区", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
