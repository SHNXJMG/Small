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
    public class BidSituationSzLongGang : WebSiteCrawller
    {
        public BidSituationSzLongGang()
            : base(true)
        {
            this.Group = "开标定标";
            this.Title = "深圳市交易中心(龙岗分中心)开标情况";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市交易中心(龙岗分中心)开标情况";
            this.SiteUrl = "http://jyzx.cb.gov.cn/LGjyzxWeb/SiteManage/KBQKGSList.aspx?MenuName=PublicInformation&ModeId=10&ItemId=KBQKGS&ItemName=%e5%bc%80%e6%a0%87%e6%83%85%e5%86%b5%e5%85%ac%e7%a4%ba&clearpaging=true";
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
            string tempCookie = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl("http://jyzx.cb.gov.cn/LGjyzxWeb/SiteManage/Index.aspx", Encoding.UTF8, ref cookiestr);
                viewState = this.ToolWebSite.GetAspNetViewState(html);
                eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);

                NameValueCollection n = this.ToolWebSite.GetNameValueCollection(
                    new string[] { 
                "ctl00$ScriptManager1",
                "__EVENTTARGET",
                "__EVENTARGUMENT",
                "__VIEWSTATE",
                "ctl00$cph_context$Login1$hfCertTitle",
                "ctl00$cph_context$DropDownList1",
                "ctl00$cph_context$DropDownList2",
                "select3",
                "textfield",
                "ctl00$cph_context$Login1$btnLogin.x",
                "ctl00$cph_context$Login1$btnLogin.y"
                },
                    new string[] { 
                "ctl00$cph_context$Login1$upLogin|ctl00$cph_context$Login1$btnLogin",
                "","", 
                viewState,
                "CN=年度施工投标人7,OU=1007,L=深圳市,ST=广东省,C=CN",
                "",
                "",
                "=全文检索=",
                "输入查询内容",
                "22",
                "8"
                }
                    );

                html = this.ToolWebSite.GetHtmlByUrl("http://jyzx.cb.gov.cn/LGjyzxWeb/SiteManage/Index.aspx", n, Encoding.UTF8, ref tempCookie); 
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref tempCookie);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "inside_table3_bottom")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToPlainTextString().GetRegexBegEnd("，共", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                    "ctl00$ScriptManager1",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "ctl00$cph_context$KBQKGSList$ddlSearchType",
                    "ctl00$cph_context$KBQKGSList$txtQymc",
                    "ctl00$cph_context$KBQKGSList$GridViewPaging1$txtGridViewPagingForwardTo",
                    "__VIEWSTATEENCRYPTED",
                    "ctl00$cph_context$KBQKGSList$GridViewPaging1$btnNext.x",
                    "ctl00$cph_context$KBQKGSList$GridViewPaging1$btnNext.y"
                    }, new string[] { 
                    "ctl00$cph_context$KBQKGSList$UpdatePanel2|ctl00$cph_context$KBQKGSList$GridViewPaging1$btnNext",
                    "","",
                    viewState,
                    "A.Gcbh",
                    "",
                    (i-1).ToString(),
                    "","5","6"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref tempCookie);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_KBQKGSList_GridView1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, prjName = string.Empty, PublicityEndDate = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, ctx = string.Empty, HtmlTxt = string.Empty,beginDate=string.Empty;

                        TableRow tr = table.Rows[j];
                        code = tr.Columns[1].ToNodePlainString();
                        prjName = tr.Columns[2].ToNodePlainString();
                        PublicityEndDate = tr.Columns[3].ToPlainTextString();
                        beginDate = DateTime.Now.ToString();
                        InfoUrl = "http://jyzx.cb.gov.cn/LGjyzxWeb/SiteManage/" + tr.Columns[4].GetATagHref();
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8, ref tempCookie).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                         NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "listtable")));
                         if (dtlNode != null && dtlNode.Count > 0)
                         {
                             HtmlTxt = dtlNode.AsHtml();
                             ctx = HtmlTxt.ToCtxString();
                             msgType = "深圳市建设工程交易中心龙岗分中心";
                             BidSituation info = ToolDb.GetBidSituation("广东省", "深圳龙岗区工程", "龙岗区", code, prjName, PublicityEndDate, msgType, InfoUrl, ctx, HtmlTxt, beginDate);
                             sqlCount++;
                             if (!crawlAll && sqlCount >= this.MaxCount) return list;

                             if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                             {
                                 if (this.ExistsUpdate)
                                 {
                                     object id = ToolDb.ExecuteScalar(string.Format("select Id from BidSituation where InfoUrl='{0}'", info.InfoUrl));
                                     if (id != null)
                                     {
                                         string sql = string.Format("delete from BaseAttach where SourceID='{0}'", id);
                                         ToolDb.ExecuteSql(sql);
                                     }
                                 }
                                 parser = new Parser(new Lexer(HtmlTxt));
                                 NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                 if (aNode != null && aNode.Count > 0)
                                 {
                                     for (int d = 0; d < aNode.Count; d++)
                                     {
                                         ATag aTag = aNode[0] as ATag;
                                         if (!aTag.IsAtagAttach()) continue;
                                         string url = "http://jyzx.cb.gov.cn/LGjyzxWeb/" + aTag.Link.Replace("../", "");
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
