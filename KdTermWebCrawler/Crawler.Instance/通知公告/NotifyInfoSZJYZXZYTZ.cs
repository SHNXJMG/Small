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
using System.IO;
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class NotifyInfoSZJYZXZYTZ : WebSiteCrawller
    {
        public NotifyInfoSZJYZXZYTZ()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省深圳市交易中心重要通知";
            this.Description = "自动抓取广东省深圳市交易中心重要通知";
            this.PlanTime = "21:48";
            this.SiteUrl = "http://www.szjsjy.com.cn/Notify/InformBrows.aspx";
            this.MaxCount = 600;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            //取得页码
            int pageInt = 1, sqlCount = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")), true), new TagNameFilter("table")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    TableTag table = pageList[0] as TableTag;
                    pageInt = table.Rows[0].ColumnCount + 1;
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    try
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { 
                        "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "__VIEWSTATEENCRYPTED",
                    "__EVENTVALIDATION",
                    "sel",
                    "beginDate",
                    "endDate",
                    "infotitle"},
                    new string[] {
                    "GridView1","Page$"+i.ToString(),viewState,"",eventValidation,"1","","",""});
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty,
                              infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        headName = tr.Columns[1].ToNodePlainString();
                        releaseTime = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        if (string.IsNullOrEmpty(releaseTime))
                            releaseTime = tr.Columns[3].ToPlainTextString().GetDateRegex("yyyy/MM/dd");
                        infoScorce = tr.Columns[2].ToNodePlainString();
                        infoType = "通知公告";
                        infoUrl = "http://www.szjsjy.com.cn/Notify/" + tr.Columns[1].GetATagHref();//"http://www.szjsjy.com.cn/Notify/InformContent.aspx?id=117750";//
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList noList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("background", "../img/A-3_17.gif")));
                        if (noList != null && noList.Count > 0)
                        {
                            ctxHtml = noList.AsHtml().Replace("<br/>", "\r\n").Replace("<BR/>", "");
                            infoCtx = noList.AsString().Replace(" ", "").Replace("&nbsp;", "").Replace("\t\t", "\t").Replace("\t\t", "\t");
                            infoCtx = Regex.Replace(infoCtx, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase).Replace(" ", "").Replace("\t", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            msgType = MsgTypeCosnt.ShenZhenMsgType;
                            infoScorce = infoScorce.Replace("&nbsp;", "");
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳市工程", string.Empty, infoCtx, infoType);
                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return null;
                            }
                            else
                            {
                                sqlCount++;
                                if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                                {
                                    parser = new Parser(new Lexer(ctxHtml));
                                    NodeFilter aLink = new TagNameFilter("a");
                                    NodeList aList = parser.ExtractAllNodesThatMatch(aLink);
                                    if (aList != null && aList.Count > 0)
                                    {
                                        for (int k = 0; k < aList.Count; k++)
                                        {
                                            ATag a = aList[k].GetATag();
                                            if (a != null)
                                            {
                                                if (!a.LinkText.Contains("返回"))
                                                {
                                                    try
                                                    {
                                                        BaseAttach obj = ToolHtml.GetBaseAttach("http://www.szjsjy.com.cn/" + a.Link.Replace("../", ""), a.LinkText, info.Id);
                                                        if (obj != null)
                                                        {
                                                            ToolDb.SaveEntity(obj, string.Empty);
                                                        }
                                                    }
                                                    catch { }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
