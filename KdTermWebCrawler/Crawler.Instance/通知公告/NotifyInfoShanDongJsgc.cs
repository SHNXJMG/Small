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
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class NotifyInfoShanDongJsgc:WebSiteCrawller
    {
        public NotifyInfoShanDongJsgc()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "山东省建设工程招投标管理信息网通知公告";
            this.Description = "自动抓取山东省建设工程招投标管理信息网通知公告";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.sdzb.gov.cn/morenews.aspx?classid=1";
            this.MaxCount = 100;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            int pageInt = 1, sqlCount = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "AspNetPager1")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].GetATagHref().GetRegexBegEnd(",'", "'");
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
                    "__VIEWSTATE",
                    "__VIEWSTATEGENERATOR",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__EVENTVALIDATION",
                    "TBKey",
                    "AspNetPager1_input"
                    }, new string[]{
                    viewState,
                    "E997B95C",
                    "AspNetPager1",
                    i.ToString(),
                    eventValidation,
                    "",
                    (i-1).ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        headName = aTag.LinkText;
                        infoType = "通知公告";
                        releaseTime = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        infoUrl = "http://www.sdzb.gov.cn/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "96%")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            ctxHtml = dtlNode.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            msgType = "山东省建设工程招标投标管理办公室";
                            List<string> attach = new List<string>();
                            parser = new Parser(new Lexer(ctxHtml));
                            NodeList imgNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                            if (imgNode != null && imgNode.Count > 0)
                            {
                                for (int p = 0; p < imgNode.Count; p++)
                                {
                                    ImageTag img = imgNode[p] as ImageTag;
                                    string link = "http://www.sdzb.gov.cn" + img.ImageURL.GetReplace("../,./");
                                    ctxHtml = ctxHtml.GetReplace(img.ImageURL, link);
                                    attach.Add(link);
                                }
                            }
                            
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "山东省", "山东省及地市", "", infoCtx, infoType);
                            sqlCount++;
                            if (!crawlAll && sqlCount >= this.MaxCount) return null;
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                            {
                                if (attach.Count > 0)
                                {
                                    for (int a = 0; a < attach.Count; a++)
                                    {
                                        try
                                        {
                                            BaseAttach entity = ToolHtml.GetBaseAttachByUrl(attach[a], headName, info.Id);
                                            if (entity != null)
                                                ToolDb.SaveEntity(entity, "SourceID,AttachServerPath");
                                        }
                                        catch { } 
                                    }
                                }
                                parser = new Parser(new Lexer(ctxHtml));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag a = aNode[k] as ATag;
                                        if (a.Link.ToLower().Contains("download")||a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = a.Link;
                                            else
                                                link = "http://www.sdzb.gov.cn" + a.Link.GetReplace("../,./");
                                            if (Encoding.Default.GetByteCount(link) > 500)
                                                continue;
                                            try
                                            {
                                                BaseAttach entity = ToolHtml.GetBaseAttachByUrl(link, a.LinkText, info.Id);
                                                if (entity != null)
                                                    ToolDb.SaveEntity(entity, "SourceID,AttachServerPath");
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
            return null;
        }
    }
}
