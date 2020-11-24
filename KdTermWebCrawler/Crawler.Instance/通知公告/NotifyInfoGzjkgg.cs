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
    public class NotifyInfoGzjkgg:WebSiteCrawller
    {
        public NotifyInfoGzjkgg()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省广州市建库公告";
            this.Description = "自动抓取广东省广州市建库公告";
            this.PlanTime = "21:55";
            this.SiteUrl = "http://www.gzggzy.cn/cms/wz/view/index/layout2/tzgglist.jsp?siteId=1&channelId=42";
            this.MaxCount = 900;
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagination page-mar")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("/共", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "wsbs-table")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        headName = tr.Columns[1].ToNodePlainString();
                        releaseTime = tr.Columns[2].ToPlainTextString().GetDateRegex();

                        infoUrl = "http://www.gzggzy.cn" + tr.Columns[1].GetATagHref();
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.Default);
                        }
                        catch
                        { continue; }

                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "xx-main")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            ctxHtml = dtlNode.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            msgType = "广州公共资源交易中心";
                            infoType = "通知公告";
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "广州市区", string.Empty, infoCtx, infoType);
                            sqlCount++;
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                            {
                                parser = new Parser(new Lexer(ctxHtml));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag aTag = aNode[k].GetATag();
                                        if (aTag.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (aTag.Link.ToLower().Contains("http"))
                                                link = aTag.Link;
                                            else
                                                link = "http://www.gzggzy.cn" + aTag.Link;
                                            BaseAttach entity = null;
                                            try
                                            {
                                                entity = ToolHtml.GetBaseAttach(link, aTag.LinkText, info.Id);
                                                if (entity != null)
                                                    ToolDb.SaveEntity(entity, string.Empty);
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }
                            if (crawlAll && sqlCount >= this.MaxCount) return null;
                        }
                    }
                }
            }
            return null;
        }
    }
}
