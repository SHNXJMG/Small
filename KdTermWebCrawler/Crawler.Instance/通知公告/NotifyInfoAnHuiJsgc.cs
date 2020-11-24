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
    public class NotifyInfoAnHuiJsgc : WebSiteCrawller
    {
        public NotifyInfoAnHuiJsgc()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "安徽省建设工程招标投投标通知公告";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取安徽省建设工程招标投投标通知公告";
            this.SiteUrl = "http://www.act.org.cn/news.asp?pid=123";
            this.MaxCount = 400;
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

                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "Page")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("页数：", "页").ToNodeString().GetReplace("|");
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
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "sublist_list_ul")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;

                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        headName = aTag.LinkText;
                        releaseTime = node.ToPlainTextString().GetDateRegex();
                        infoUrl = "http://www.act.org.cn/" + aTag.Link.GetReplace("amp;");
                        string htmldtl = string.Empty;
                        try 
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "subcont_cont")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            ctxHtml = dtlNode.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            infoType = "通知公告";
                            msgType = "安徽省建设工程招标投标办公室";
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "安徽省", "安徽省及地市", string.Empty, infoCtx, infoType);
                            sqlCount++;
                            if (!crawlAll && sqlCount >= this.MaxCount) return null;

                            if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                            {
                                parser = new Parser(new Lexer(ctxHtml));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int a = 0; a < aNode.Count; a++)
                                    {
                                        ATag tag = aNode[a] as ATag;
                                        if (tag.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (tag.Link.ToLower().Contains("http"))
                                                link = tag.Link;
                                            else
                                                link = "http://www.act.org.cn" + tag.Link;
                                            try
                                            {
                                                BaseAttach obj = ToolHtml.GetBaseAttach(link, tag.LinkText, info.Id);
                                                if (obj != null)
                                                    ToolDb.SaveEntity(obj, string.Empty);
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
