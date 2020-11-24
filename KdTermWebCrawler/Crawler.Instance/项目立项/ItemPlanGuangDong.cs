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
    public class ItemPlanGuangDong : WebSiteCrawller
    {
        public ItemPlanGuangDong()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "广东省发改委";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取广东省发改委";
            this.SiteUrl = "http://www.gdtz.gov.cn/project.action";
            this.ExistCompareFields = "InfoUrl,City";
            this.MaxCount = 800;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ItemPlan>();
            Dictionary<string, string> dic = GetCityList();
            if (dic == null || dic.Count < 1) return list;

            foreach (string key in dic.Keys)
            {
                string html = string.Empty;
                string cookiestr = string.Empty;
                string viewState = string.Empty;
                int pageInt = 1,sqlCount=0;
                string eventValidation = string.Empty;
                try
                {
                    this.ToolWebSite.GetHtmlByUrl(dic[key], Encoding.UTF8, ref cookiestr);
                    html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);

                }
                catch { }
                Parser parser = new Parser(new Lexer(html));
                NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "badoo")), true), new TagNameFilter("a")));
                if (pageNode != null && pageNode.Count > 0)
                {
                    try
                    {
                        string temp = pageNode[pageNode.Count - 1].GetATag().Link.Replace("javascript", "").Replace("jumpPage(", "").Replace(")", "");
                        pageInt = int.Parse(temp);
                    }
                    catch { }
                }
                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "param.name", "param.proofCode", "page.pageNo", "page.orderBy", "page.order" }, new string[] { 
                "","",i.ToString(),"",""
                });
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "hytab")));
                    if (listNode != null && listNode.Count > 0)
                    {
                        TableTag table = listNode[0] as TableTag;
                        for (int j = 1; j < table.RowCount; j++)
                        {
                            string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;
                            TableRow tr = table.Rows[j];
                            ItemName = tr.Columns[0].ToNodePlainString();
                            PlanDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                            InfoUrl = "http://www.gdtz.gov.cn" + tr.Columns[0].GetATagHref();
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "xmgknr")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                CtxHtml = dtlNode.AsHtml();
                                parser = new Parser(new Lexer(CtxHtml));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    TableTag tab = tableNode[0] as TableTag;
                                    for (int k = 1; k < tab.RowCount; k++)
                                    { 
                                       TableRow dr = tab.Rows[k];
                                       if (dr.ColumnCount < 2)
                                           break;
                                       try
                                       {
                                           ItemCtx += dr.Columns[0].ToNodePlainString() + "：";
                                           ItemCtx += dr.Columns[1].ToNodePlainString() + "\r\n";
                                       }
                                       catch(Exception ex) {
                                           Logger.Error(InfoUrl + ItemName + key + i);
                                           Logger.Error(ex);
                                       }
                                    }
                                }
                                else
                                    ItemCtx = CtxHtml.ToCtxString();
                                ApprovalCode = ItemCtx.GetRegex("备案项目编号");
                                ItemAddress = ItemCtx.GetRegex("项目所在地");
                                TotalInvest = ItemCtx.GetRegex("项目总投资").Replace("万元","").Replace("万","");
                                ItemContent = ItemCtx.GetRegex("项目规模及内容");
                                ApprovalUnit = ItemCtx.GetRegex("备案机关");
                                ApprovalDate = ItemCtx.GetRegex("复核通过日期");
                                string temp =ItemCtx.GetRegex("项目起止年限");
                                string[] tempPlan = temp.Split('-');
                                if (tempPlan.Length == 2)
                                {
                                    PlanBeginDate = tempPlan[0];
                                    PlanEndDate = tempPlan[1];
                                }
                                PlanType = "项目公开";
                                MsgType = "广东省发展和改革委员会";
                                string city = key;
                                if (key.Contains("顺德"))
                                    city = "佛山市区"; 
                                
                                ItemPlan info = ToolDb.GenItemPlan("广东省", city, "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);
                                list.Add(info);
                                sqlCount++;
                                if (!crawlAll && sqlCount >= this.MaxCount) goto type;
                            }
                         
                        }


                    }
                }
            type: continue;
            }
            return list;
        }

        private Dictionary<string, string> GetCityList()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string url = "http://www.gdtz.gov.cn/city.action";
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(url);
            }
            catch {
                return dic;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "citytitle")));
            if (listNode != null && listNode.Count > 0)
            {
                for (int i = 0; i < listNode.Count; i++)
                {
                    ATag aTag = listNode[i].GetATag();
                    string link = "http://www.gdtz.gov.cn" + aTag.Link;
                    string city = string.Empty;
                    if (aTag.LinkText.Contains("广东"))
                        city = "广东地区"; 
                    else
                        city = aTag.LinkText + "区";
                    dic.Add(city, link);
                }
            }
            return dic;
        }
    }
}
