using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class NoticeInfoSZLGJB : WebSiteCrawller
    {
        public NoticeInfoSZLGJB()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市龙岗区截标信息";
            this.PlanTime = "9:25,11:25,14:15,17:25";
            this.Description = "自动抓取广东省深圳市龙岗区截标信息";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://jyzx.cb.gov.cn/LGjyzxWeb/SiteManage/JbxxList.aspx?MenuName=PublicInformation&ModeId=2&ItemId=jbxx&ItemName=%e6%88%aa%e6%a0%87%e4%bf%a1%e6%81%af&clearpaging=true";
            this.MaxCount = 200;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookieStr = string.Empty;
            int pageInt = 1;
            string endDate = DateTime.Now.AddDays(15).ToString("yyyy-MM-dd");
            string beginDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookieStr);
                viewState = this.ToolWebSite.GetAspNetViewState(html);
                eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);

                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                "ctl00$ScriptManager1",
                "__EVENTTARGET",
                "__EVENTARGUMENT",
                "__VIEWSTATE",
                "ctl00$cph_context$JBXXList2$ddlSearch",
                "ctl00$cph_context$JBXXList2$txtProjectName",
                "ctl00$cph_context$JBXXList2$txtStartTime",
                "ctl00$cph_context$JBXXList2$txtEndTime",
                "ctl00$cph_context$JBXXList2$GridViewPaging1$txtGridViewPagingForwardTo",
                "__EVENTVALIDATION",
                "ctl00$cph_context$JBXXList2$ImageButton1.x",
                "ctl00$cph_context$JBXXList2$ImageButton1.y"},
                    new string[]{
                    "ctl00$cph_context$JBXXList2$UpdatePanel1|ctl00$cph_context$JBXXList2$ImageButton1",
                    "",
                "",
                viewState,
                "fbrq", 
                "",
                beginDate,
                endDate,
                "1",eventValidation,"24","9"
                    });

                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookieStr);
            }
            catch { return null; }

            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "ctl00_cph_context_JBXXList2_GridViewPaging1_PagingDescTd")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("，共", "页").ToLower().Replace("&nbsp;", "");
                    pageInt = int.Parse(temp);
                }
                catch
                {
                    pageInt = 1;
                }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    try
                    { 
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                "ctl00$ScriptManager1",
                "__EVENTTARGET",
                "__EVENTARGUMENT",
                "__VIEWSTATE",
                "ctl00$cph_context$JBXXList2$ddlSearch",
                "ctl00$cph_context$JBXXList2$txtProjectName",
                "ctl00$cph_context$JBXXList2$txtStartTime",
                "ctl00$cph_context$JBXXList2$txtEndTime",
                "ctl00$cph_context$JBXXList2$GridViewPaging1$txtGridViewPagingForwardTo",
                "__EVENTVALIDATION",
                "ctl00$cph_context$JBXXList2$GridViewPaging1$btnForwardToPage"},
                            new string[]{
                    "ctl00$cph_context$JBXXList2$update1|ctl00$cph_context$JBXXList2$GridViewPaging1$btnForwardToPage",
                    "",
                "",
                viewState,
                "fbrq", 
                "",
                beginDate,endDate,i.ToString(),eventValidation,"Go"
                    });
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookieStr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_JBXXList2_GridView1")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty, prjType = string.Empty;

                        TableRow tr = table.Rows[j];
                        InfoType = "截标信息";
                        InfoTitle = tr.Columns[2].ToNodePlainString();
                        prjCode = tr.Columns[1].ToNodePlainString();
                        prjType = tr.Columns[3].ToNodePlainString();
                        PublistTime = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://jyzx.cb.gov.cn/LGjyzxWeb/SiteManage/" + tr.Columns[2].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "text")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            htmlTxt = dtlNode.ToHtml();
                            InfoCtx = htmlTxt.ToCtxString();
                            NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳龙岗区工程", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "深圳市建设工程交易中心龙岗分中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, prjType, string.Empty, htmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(htmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag aTag = aNode[k] as ATag;
                                    if (aTag.IsAtagAttach())
                                    {
                                        string alink = "http://www.bajsjy.com/" + aTag.Link.Replace("../", "");
                                        BaseAttach attach = ToolDb.GenBaseAttach(aTag.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, alink);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
