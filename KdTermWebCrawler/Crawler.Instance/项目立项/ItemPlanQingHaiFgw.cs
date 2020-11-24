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
    public class ItemPlanQingHaiFgw : WebSiteCrawller
    {
        public ItemPlanQingHaiFgw()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "青海省发展和改革委员会项目立项";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取青海省发展和改革委员会项目立项";
            this.SiteUrl = "http://www.ztzl.qhfgw.gov.cn/xmjcb/xmxxgk/default.htm";
            this.MaxCount = 200;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ItemPlan>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty; 
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
            NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "table25")));
            if (listNode != null && listNode.Count > 0)
            {
                TableTag table = listNode[listNode.Count-1] as TableTag;
                for (int j = 1; j < table.RowCount; j++)
                {
                    TableRow tr = table.Rows[j];
                    string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                     
                    ATag aTag = tr.GetATag();
                    ItemName = aTag.LinkText.ToNodeString().GetReplace("   , ");
                    ItemCode = tr.Columns[0].ToNodePlainString().GetRegexBegEnd("【", "】").GetReplace("项目编号：");
                    InfoUrl = "http://www.ztzl.qhfgw.gov.cn/xmjcb/xmxxgk/" + aTag.Link.GetReplace("../,./");
                    string htmldtl = string.Empty;
                    try
                    {
                        htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                    }
                    catch { continue; }

                    parser = new Parser(new Lexer(htmldtl));
                    NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "table143")));
                    if (dtlNode != null && dtlNode.Count > 0)
                    {
                        CtxHtml = dtlNode.AsHtml();
                        ItemCtx = CtxHtml.Replace("</p>", "\r\n").Replace("</tr>", "\r\n").ToCtxString();
                        TotalInvest = ItemCtx.GetRegexBegEnd("总投资","万元");
                        PlanDate = ItemCtx.GetDateRegex();
                        PlanType = "项目信息";
                        MsgType = "青海省发展和改革委员会";
                        ItemPlan info = ToolDb.GenItemPlan("青海省", "青海省及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl); 
                        list.Add(info);
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
                                        link = "http://www.ztzl.qhfgw.gov.cn/" + a.Link.GetReplace("../,./");
                                    if (Encoding.Default.GetByteCount(link) > 500)
                                        continue;
                                    BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                    base.AttachList.Add(attach);
                                }
                            }
                        }
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            } 
            return list;
        }
    }
}
