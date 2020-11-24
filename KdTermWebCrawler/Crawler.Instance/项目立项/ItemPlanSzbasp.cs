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
    public class ItemPlanSzbasp : WebSiteCrawller
    {
        public ItemPlanSzbasp() :
            base()
        {
            this.Group = "项目立项";
            this.Title = "深圳市宝安区工程领域项目审批信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市宝安区工程领域项目审批信息";
            this.SiteUrl = "http://www.baoan.gov.cn/ztlm/gcjszt/gcjs/xmsp/xusp/kxxyj/";
            this.MaxCount = 800;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            List<string> urlList = new List<string>();
            urlList.Add("http://www.baoan.gov.cn/ztlm/gcjszt/gcjs/xmsp/xusp/kxxyj/");
            urlList.Add("http://www.baoan.gov.cn/ztlm/gcjszt/gcjs/xmsp/xusp/cbsj/");
            urlList.Add("http://www.baoan.gov.cn/ztlm/gcjszt/gcjs/xmsp/xusp/hjyxpj/");
            IList list = new List<ItemPlan>();
            foreach (string url in urlList)
            {
                int count = 0;
                string html = string.Empty;
                string cookiestr = string.Empty;
                string viewState = string.Empty;
                int pageInt = 1;
                string eventValidation = string.Empty;
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
                }
                catch
                {
                    return list;
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "fenye")));
                if (pageNode != null && pageNode.Count > 0)
                {
                    string temp = pageNode.AsString().Replace("createPageHTML", "").Replace("0,", "").Replace("(", "").Replace(")", "").Replace("index", "").Replace("html", "").Replace(",", "").Replace("\"", "").Replace(";","").Trim() ;
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
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(url + "/index_" + (i - 1).ToString() + ".html", Encoding.UTF8);
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "97%")));
                    if (listNode != null && listNode.Count > 0)
                    {
                        TableTag table = listNode[0] as TableTag;
                        for (int j = 1; j < table.RowCount; j++)
                        {
                            TableRow tr = table.Rows[j];
                            string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                            ItemName = tr.Columns[1].ToNodePlainString();
                            ItemCode = tr.Columns[2].ToNodePlainString();
                            PlanDate = tr.Columns[3].ToPlainTextString().GetDateRegex();

                            InfoUrl =url+ tr.Columns[1].GetATagHref().Replace("../", "").Replace("./","");
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                            }
                            catch { continue; }

                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "900")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                CtxHtml = dtlNode.AsHtml();
                                TableTag dtlTable = dtlNode[0] as TableTag;
                                for (int k = 1; k < dtlTable.RowCount; k++)
                                {
                                    ItemCtx += dtlTable.Rows[k].Columns[0].ToNodePlainString() + "：";
                                    ItemCtx += dtlTable.Rows[k].Columns[1].ToNodePlainString() + "\r\n";
                                }
                                BuildUnit = ItemCtx.GetRegex("建设单位");
                                ApprovalCode = ItemCtx.GetRegex("审批文号");
                                ApprovalUnit = ItemCtx.GetRegex("审批单位");
                                ApprovalDate = ItemCtx.GetRegex("审批时间").Replace(".","-");

                                PlanType = "项目审批信息";
                                MsgType = "深圳市宝安区发改局";

                                ItemPlan info = ToolDb.GenItemPlan("广东省", "深圳市区", "宝安区", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);
                                count++;
                                list.Add(info);
                                if (!crawlAll && count >= this.MaxCount) return list;
                            }
                        }
                    }

                }
            }
            return list;
        }
    }

}
