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
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class NotifyInfoYinChuan : WebSiteCrawller
    {
        public NotifyInfoYinChuan()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "银川市公共资源交易中心通知公告";
            this.Description = "自动抓取银川市公共资源交易中心通知公告";
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.ycsggzy.cn/morelink.aspx?type=9&index=1";
            this.MaxCount = 500;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NotifyInfo>();
            int pageInt = 1,sqlCount=0;
            string html = string.Empty;
            string viewState = string.Empty;
            string cookiestr = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "pager")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    Regex reg = new Regex(@"[0-9]+");
                    string temp = reg.Match(pageNode[pageNode.Count - 1].GetATagHref().Replace("&#39;", "")).Value;
                    pageInt = int.Parse(temp);
                }
                catch
                {
                }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                        new string[] { "__VIEWSTATE",
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__VIEWSTATEENCRYPTED",
                        "__EVENTVALIDATION",
                        "hsa1$DD_LX",
                        "hsa1$wd",
                        "pager_input"},
                        new string[] {
                        viewState,
                        "pager",
                        i.ToString(),
                        "",
                        eventValidation,
                        "综合搜索",
                        "",
                        (i-1).ToString()
                        }
                        );
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GV1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        headName = tr.Columns[0].ToNodePlainString();
                        releaseTime = tr.Columns[1].ToPlainTextString();
                        infoUrl = "http://www.ycsggzy.cn/" + tr.Columns[0].GetATagHref().Replace("&amp;", "&");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "Lb_nr")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            ctxHtml = dtlNode.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();

                       
                            msgType = "银川市公共资源交易中心";
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "宁夏回族自治区", "宁夏回族自治区及地市", "银川市", infoCtx, "通知公告");
                            sqlCount++;
                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return null;
                            }
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                            {
                                parser = new Parser(new Lexer(ctxHtml));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag fileATag = aNode[k].GetATag();
                                        if (fileATag.IsAtagAttach())
                                        {
                                            BaseAttach obj = null;
                                            try
                                            {
                                                if (fileATag.Link.ToLower().Contains("http"))
                                                {
                                                    obj = ToolHtml.GetBaseAttach(fileATag.Link, headName, info.Id);
                                                }
                                                else
                                                {
                                                    obj = ToolHtml.GetBaseAttach("http://www.ycsggzy.cn" + fileATag.Link, headName, info.Id);
                                                }
                                            }
                                            catch { }
                                            if (obj != null)
                                                ToolDb.SaveEntity(obj, string.Empty);
                                        }
                                    }
                                }
                                else
                                {
                                    parser.Reset();
                                    NodeList imgNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                                    if (imgNode != null && imgNode.Count > 0)
                                    {
                                        for (int k = 0; k < imgNode.Count; k++)
                                        {
                                            ImageTag img = imgNode[0] as ImageTag;
                                            BaseAttach obj = null;
                                            try
                                            {
                                                if (img.ImageURL.ToLower().Contains("http"))
                                                {
                                                    obj = ToolHtml.GetBaseAttach(img.ImageURL, headName, info.Id);
                                                }
                                                else
                                                {
                                                    obj = ToolHtml.GetBaseAttach("http://www.ycsggzy.cn" + img.ImageURL, headName, info.Id);
                                                }
                                            }
                                            catch { }
                                            if (obj != null)
                                                ToolDb.SaveEntity(obj, string.Empty);
                                        }
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
