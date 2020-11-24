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
    public class ItemPlanGz : WebSiteCrawller
    {
        public ItemPlanGz()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "广东省招标投标监管网";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓广东省招标投标监管网";
            this.SiteUrl = "http://www.gdzbtb.gov.cn/zbsxhz/index.htm";
            this.MaxCount = 1200;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "cn6")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().Replace("createPageHTML", "").Replace(" 0,", "").Replace("(", "").Replace(")", "").Replace("index", "").Replace("htm", "").Replace(",", "").Replace("\"", "").Replace(";", "").Trim(); ;
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.gdzbtb.gov.cn/zbsxhz/index_" + (i - 1).ToString() + ".htm",Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "position2")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        ATag aTag = listNode[j].GetATag();
                        ItemName = aTag.GetAttribute("title");
                        InfoUrl = "http://www.gdzbtb.gov.cn/zbsxhz/" + aTag.Link.Replace("../", "").Replace("./", "");
                        string tempCity = ItemName.Replace("[", "kdxx").Replace("]", "xxdk").GetRegexBegEnd("kdxx", "xxdk");
                        ItemName = ItemName.Replace("["+tempCity+"]-", "");
                       
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.Default).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                        if (dtlNode != null && dtlNode.Count > 3)
                        {
                            TableTag table = dtlNode[3] as TableTag;
                            CtxHtml = dtlNode.AsHtml();

                            for (int k = 0; k < table.RowCount; k++)
                            {
                                ItemCtx += table.Rows[k].Columns[0].ToNodePlainString() + "：";
                                ItemCtx += table.Rows[k].Columns[1].ToNodePlainString() + "\r\n";
                            }
                            PlanDate = ItemCtx.GetRegex("批复日期").GetDateRegex();
                            if (string.IsNullOrEmpty(PlanDate))
                                PlanDate = ItemCtx.GetDateRegex();
                            if (string.IsNullOrEmpty(PlanDate))
                                PlanDate = DateTime.Now.ToString("yyyy-MM-dd");
                            ItemCode = ItemCtx.GetRegex("项目编码").Replace("　", "");
                            BuildUnit = ItemCtx.GetRegex("项目单位");
                            ApprovalUnit = ItemCtx.GetRegex("核准部门");
                            ApprovalDate = PlanDate;
                            ApprovalCode = ItemCtx.GetRegex("批复文号");
                            ItemContent = ItemCtx.GetRegex("规模及内容",true,1000);
                            string city = string.Empty;
                            if (tempCity == "广东")
                                city = "广州市区";
                            else
                                city = tempCity + "市区"; 
                            PlanType = "项目核准信息";
                            MsgType = "广东省招标投标监管网";

                            ItemPlan info = ToolDb.GenItemPlan("广东省", city, "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
