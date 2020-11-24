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
using System.Security.Cryptography.X509Certificates;

namespace Crawler.Instance
{
    public class BidSituationSzJYZX : WebSiteCrawller
    {
        public BidSituationSzJYZX()
            : base()
        {
            this.Group = "开标定标";
            this.Title = "深圳市交易中心开标情况";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市交易中心开标情况";
            this.SiteUrl = "http://www.szjsjy.com.cn/BusinessInfo/KBQKGSList.aspx?xxlb=39";
            this.MaxCount = 500;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidSituation>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1, sqlCount = 0;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl("http://www.szjsjy.com.cn/HomePage.aspx", Encoding.UTF8, ref cookiestr);
                viewState = this.ToolWebSite.GetAspNetViewState(html);
                eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                NameValueCollection n = this.ToolWebSite.GetNameValueCollection(
                    new string[] { 
                "__VIEWSTATE",
                "__VIEWSTATEENCRYPTED",
                "__EVENTVALIDATION",
                "TextBox1",
                "ddl",
                "DDL_Govt",
                "DDL_Trade",
                "txtText",
                "hdnSN",
                "ImageButton2.x",
                "ImageButton2.y"
                },
                    new string[] { 
                viewState,
                "",
                eventValidation,
                "请输入关键字","0","0","0",
                "CN=年度施工投标人7,OU=1007,L=深圳市,ST=广东省,C=CN",
                "241EDFC1BA276AA7","19","13"
                }
                    ); 
                string tempCookie = string.Empty;  
                html = this.ToolWebSite.GetHtmlByUrl("http://www.szjsjy.com.cn/HomePage.aspx", n
                , Encoding.UTF8, ref tempCookie);
                cookiestr = tempCookie.Replace("path=/;", "").Replace("HttpOnly,", "").Replace("HttpOnly", "").Replace(" ", ""); //"_gscu_485601283=265607704dljg167; _gscs_485601283=32711103yul0an14|pv:5;" + tempCookie.Replace("path=/;", "").Replace("HttpOnly,", "").Replace("HttpOnly", "").Replace(" ", "");
                //tempCookie = tempCookie.Replace("path=/;", "").Replace("HttpOnly,", "").Replace("HttpOnly", "").Replace(" ", "");
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_Content_GridView1")));
            if (pageNode != null && pageNode.Count > 0)
            {
                TableTag table = pageNode[0] as TableTag;
                try
                {
                    string temp = table.Rows[table.RowCount - 1].ToNodePlainString().GetRegexBegEnd("，共", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "__VIEWSTATEENCRYPTED",
                    "__EVENTVALIDATION",
                    "ctl00$Content$drpSearchType",
                    "ctl00$Content$txtQymc",
                    "ctl00$Content$hdnOperate",
                    "ctl00$hdnPageCount"
                    },
                        new string[]{
                        "ctl00$Content$GridView1",
                        "Page$"+i,
                        viewState,
                        "",
                        eventValidation,
                        "0","","",pageInt.ToString()
                        });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_Content_GridView1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string code = string.Empty, prjName = string.Empty, PublicityEndDate = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, ctx = string.Empty, HtmlTxt = string.Empty,beginDate=string.Empty;

                        TableRow tr = table.Rows[j];
                        code = tr.Columns[1].ToNodePlainString();
                        prjName = tr.Columns[2].ToNodePlainString();
                        PublicityEndDate = tr.Columns[3].ToPlainTextString();
                        beginDate = DateTime.Now.ToString();
                        InfoUrl = "http://www.szjsjy.com.cn/BusinessInfo/" + tr.Columns[4].GetATagHref();
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8, ref cookiestr).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ContentContainer")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            ctx = HtmlTxt.ToCtxString();
                            msgType = "深圳市建设工程交易中心";
                            BidSituation info = ToolDb.GetBidSituation("广东省", "深圳市工程", "", code, prjName, PublicityEndDate, msgType, InfoUrl, ctx, HtmlTxt,beginDate);
                            sqlCount++;
                            if (!crawlAll && sqlCount >= this.MaxCount) return list;

                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int d = 0; d < aNode.Count; d++)
                                    {
                                        ATag aTag = aNode[0] as ATag;
                                        if (!aTag.IsAtagAttach()) continue;
                                        string url = "http://www.szjsjy.com.cn/" + aTag.Link.Replace("../", "");
                                        BaseAttach attach = null;
                                        try
                                        {
                                            attach = ToolHtml.GetBaseAttach(url, aTag.LinkText, info.Id, "SiteManage\\Files\\Attach\\");
                                            if (attach == null) attach = ToolHtml.GetBaseAttach(url, aTag.LinkText, info.Id, "SiteManage\\Files\\Attach\\");
                                        }
                                        catch { }
                                        if (attach != null)
                                            ToolDb.SaveEntity(attach, string.Empty);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
