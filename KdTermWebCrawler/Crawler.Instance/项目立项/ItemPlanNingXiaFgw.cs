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
    public class ItemPlanNingXiaFgw : WebSiteCrawller
    {
        public ItemPlanNingXiaFgw()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "宁夏回族自治区发展和改革委员会项目立项信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取宁夏回族自治区发展和改革委员会项目立项信息";
            this.SiteUrl = "http://www.nxdrc.gov.cn/zfxxgk/zfxxgkml/index.htm";
            this.MaxCount = 400;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ItemPlan>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 100;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl,Encoding.Default);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_sort")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("分","页");
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.nxdrc.gov.cn/zfxxgk/zfxxgkml/index" + (i - 1).ToString() + ".htm",Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter( new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_v01")),true),new TagNameFilter("table")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;
                        TableRow tr = table.Rows[j];
                        ItemCode = tr.Columns[0].ToNodePlainString();
                        ATag aTag = tr.Columns[1].GetATag();
                        ItemName = aTag.LinkText.GetReplace("自治区发展改革委批准,自治区发展改革委批复,自治区发改委");
                     
                        PlanDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.nxdrc.gov.cn/zfxxgk/zfxxgkml/" + aTag.Link.GetReplace("../,./");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "main3")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {

                            CtxHtml = dtlNode.AsHtml();
                            ItemCtx = CtxHtml.ToCtxString();
                            TotalInvest = ItemCtx.GetRegexBegEnd("总投资","万元");
                            parser = new Parser(new Lexer(CtxHtml));
                            NodeList conNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("", "id")));
                            if (conNode != null && conNode.Count > 0)
                            {
                                ItemContent = conNode[0].ToNodePlainString();
                                if (Encoding.Default.GetByteCount(ItemContent) > 2000)
                                    ItemContent = "";
                            }
                            
                            PlanType = "项目审批信息";
                            MsgType = "宁夏回族自治区发展和改革委员会";

                            ItemPlan info = ToolDb.GenItemPlan("宁夏回族自治区", "宁夏回族自治区及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);
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
