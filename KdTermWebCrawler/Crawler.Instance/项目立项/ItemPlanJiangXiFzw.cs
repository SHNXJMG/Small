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
    public class ItemPlanJiangXiFzw : WebSiteCrawller
    {
        public ItemPlanJiangXiFzw()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "江西省发展和改革委员会重点项目信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取江西省发展和改革委员会重点项目信息";
            this.SiteUrl = "http://www.jxdpc.gov.cn/zdxm/zdxmxx/";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tdfont")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().Replace("function createPageHTML", "").GetRegexBegEnd("createPageHTML", ",").Replace("(", "");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "index_" + (i - 1) + ".htm", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellspacing", "3")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        if (table.Rows[j].ColumnCount < 2) continue;
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        ItemName= System.Web.HttpUtility.UrlDecode( aTag.GetAttribute("title"));
                        PlanDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        if (aTag.Link.ToLower().Contains("departmentsite")|| aTag.Link.ToLower().Contains("fagaiing"))
                        {
                            InfoUrl = "http://www.jxdpc.gov.cn/" + aTag.Link.Replace("../", "");
                        }
                        else
                        {
                            InfoUrl = "http://www.jxdpc.gov.cn/" + "zdxm/zdxmxx/" + aTag.Link.Replace("./", "");
                        }

                     
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "artibody")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            ItemCtx = CtxHtml.ToCtxString(); 
                          

                            PlanType = "建设工程";
                            MsgType = "江西省发展和改革委员会";

                            ItemPlan info = ToolDb.GenItemPlan("江西省", "江西省及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
