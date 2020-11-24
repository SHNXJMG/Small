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
    public class ItemPlanSzns : WebSiteCrawller
    {
        public ItemPlanSzns()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "深圳市南山区工程领域项目核准信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市南山区工程领域项目核准信息";
            this.SiteUrl = "http://www.szns.gov.cn/publish/main/1/36/10178/10179/10181/index.html";
            this.MaxCount = 1200;
            this.ExistCompareFields = "ItemName,InfoUrl";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hyxma03")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString().GetRegexBegEnd("/共", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.szns.gov.cn/publish/main/1/36/10178/10179/10181/index_"+i+".html",Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hyxmalb")),true),new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    { 
                        ATag aTag = listNode[j].GetATag();
                        string  PlanDate = listNode[j].ToPlainTextString().GetDateRegex();
                        string InfoUrl = "http://www.szns.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoNormalTable")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            TableTag table = dtlNode[0] as TableTag;
                            int num = 1;
                            DateTime tempPlanDate = Convert.ToDateTime(PlanDate);
                            if (tempPlanDate <= DateTime.Parse("2010-09-21"))
                                num = 2;
                            for (int k = num; k < table.RowCount; k++)
                            {
                                string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty,   PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty,  MsgType = string.Empty;

                                TableRow tr = table.Rows[k];
                                DateTime planDate = Convert.ToDateTime(PlanDate);
                                if (planDate <= DateTime.Parse("2010-09-21"))
                                {
                                    ItemName = tr.Columns[1].ToNodePlainString();
                                    BuildNature = tr.Columns[2].ToNodePlainString();
                                    TotalInvest = tr.Columns[3].ToNodePlainString();
                                    string temp = tr.Columns[5].ToNodePlainString();
                                    if (temp.Contains("至"))
                                    {
                                        string[] date = temp.Split('至');
                                        PlanBeginDate = date[0];
                                        PlanEndDate = date[1];
                                    }
                                    else
                                    {
                                        PlanBeginDate = temp;
                                    }
                                    ItemAddress = tr.Columns[6].ToNodePlainString();
                                    ApprovalCode = tr.Columns[7].ToNodePlainString();
                                }
                                else
                                {
                                    ItemName = tr.Columns[2].ToNodePlainString();
                                    BuildUnit = tr.Columns[3].ToNodePlainString();
                                    BuildNature = tr.Columns[4].ToNodePlainString();
                                    TotalInvest = tr.Columns[5].ToNodePlainString();
                                    ItemAddress = tr.Columns[7].ToNodePlainString();
                                    string temp = tr.Columns[6].ToNodePlainString().Replace("&mdash;", "$");
                                    if (temp.Contains("$"))
                                    {
                                        string[] date = temp.Split('$');
                                        PlanBeginDate = date[0];
                                        PlanEndDate = date[1];
                                    }
                                    else
                                    {
                                        PlanBeginDate = temp;
                                    }
                                    try
                                    {
                                        ApprovalCode = tr.Columns[8].ToNodePlainString();
                                    }
                                    catch
                                    {
                                    }
                                    try
                                    {
                                        ApprovalDate = tr.Columns[9].ToNodePlainString();
                                    }
                                    catch
                                    { 
                                    }
                                }
                                PlanType = "项目核准信息";
                                MsgType = "深圳市南山区发改局";
                                ItemPlan info = ToolDb.GenItemPlan("广东省", "深圳市区", "南山区", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                    }
                }
            }
                return list;
        }
    }
}
