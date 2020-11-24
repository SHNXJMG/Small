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
    public class ItemPlanShanDongFgw : WebSiteCrawller
    {
        public ItemPlanShanDongFgw()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "山东省发展和改革委员会项目立项";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取山东省发展和改革委员会项目立项";
            this.SiteUrl = "http://xxgk.sd.gov.cn/GovInfoOpen/InfoOpenDir/InfoOpenDirTwo/InfoList.aspx?InfoType=zpfl&id=0000051&DeptId=1ad98907-723c-402e-9890-d4e07a9f70c0";
            this.MaxCount = 200;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "Webpager1")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode.AsString().GetRegexBegEnd("共", "页");
                try
                {
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__VIEWSTATE",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "deptKey",
                    "key",
                    "Webpager1_input"
                    }, new string[]{
                    viewState,
                    "Webpager1",
                    i.ToString(),
                    "","",
                    (i-1).ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl,nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellSpacing", "1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;


                        MsgUnit = tr.Columns[2].ToNodePlainString();
                        PlanDate = tr.Columns[1].ToPlainTextString().GetDateRegex("yyyy/MM/dd");
                        ATag aTag = tr.Columns[0].GetATag();
                        ItemName = aTag.LinkText.ToNodeString().GetReplace("   , ");

                        InfoUrl = "http://xxgk.sd.gov.cn/GovInfoOpen/InfoOpenDir/InfoOpenDirTwo/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }

                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "contents")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            ItemCtx = CtxHtml.Replace("</p>", "\r\n").Replace("</tr>", "\r\n").ToCtxString();
                            parser = new Parser(new Lexer(CtxHtml));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "90%")));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                string ctx = string.Empty;
                                TableTag tableTag = tableNode[0] as TableTag;
                                for (int r = 0; r < tableTag.RowCount; r++)
                                {
                                    for (int c = 0; c < tableTag.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = tableTag.Rows[r].Columns[c].ToNodePlainString();
                                        if ((c + 1) % 2 == 0)
                                            ctx += temp.GetReplace(":,：") + "\r\n";
                                        else
                                            ctx += temp.GetReplace(":,：") + "：";
                                    }
                                }
                                ItemCode = ctx.GetRegex("索引号");
                            }

                            parser.Reset();
                            tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zoom")), true), new TagNameFilter("table")));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                string ctx = string.Empty;
                                TableTag tableTag = tableNode[0] as TableTag;
                                for (int c = 0; c < tableTag.Rows[0].ColumnCount; c++)
                                {
                                    try
                                    {
                                        ctx += tableTag.Rows[0].Columns[c].ToNodePlainString() + "：";
                                        ctx += tableTag.Rows[1].Columns[c].ToNodePlainString() + "\r\n";
                                    }
                                    catch { }
                                }
                                ApprovalCode = ctx.GetRegex("批准文号");
                                ApprovalUnit = ctx.GetRegex("项目申请人");
                                ApprovalDate = ctx.GetRegex("批准时间");
                                ItemContent = ctx.GetRegex("主要建设内容", true, 500);
                            }
                           

                            PlanType = "项目信息";
                            MsgType = "山东省发展和改革委员会";
                            ItemPlan info = ToolDb.GenItemPlan("山东省", "山东省及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
