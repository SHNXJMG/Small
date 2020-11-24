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
    public class NotifyInfoHeNanZtb : WebSiteCrawller
    {
        public NotifyInfoHeNanZtb()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "河南省招投标网通知公告";
            this.Description = "自动抓取河南省招投标网通知公告";
            this.PlanTime = "21:55";
            this.SiteUrl = "http://www.hnsztb.com.cn/gsgg/tztg.asp";
            this.MaxCount = 600;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NotifyInfo>();
            int pageInt = 1, sqlCount = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "style1")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString().GetRegexBegEnd("/", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?page=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "99%")), true), new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%"))));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        TableRow tr = (listNode[j] as TableTag).Rows[0];
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;
                        ATag aTag = tr.GetATag();
                        headName = aTag.LinkText;
                        if (Encoding.Default.GetByteCount(headName) > 200)
                            headName = headName.Substring(0, 99);
                        infoUrl = "http://www.hnsztb.com.cn/gsgg/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "800")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            TableTag table = dtlNode[0] as TableTag;
                            if (table.RowCount > 1)
                                ctxHtml = table.Rows[1].ToHtml();
                            else
                                ctxHtml = table.ToHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            releaseTime = infoCtx.GetDateRegex();
                            if (string.IsNullOrEmpty(releaseTime))
                                releaseTime = infoCtx.GetDateRegex("yyyy年MM月dd日");
                            if (string.IsNullOrEmpty(releaseTime))
                                releaseTime = infoCtx.GetDateRegex("yyyy/MM/dd");
                            if (string.IsNullOrEmpty(releaseTime))
                                releaseTime = infoCtx.GetChinaTime();
                            msgType = "河南省建设工程招标投标协会";
                            infoType = "通知公告";
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "河南省", "河南省及地市", string.Empty, infoCtx, infoType);
                            sqlCount++;
                            if (crawlAll && sqlCount >= this.MaxCount) return null;
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                            {
                                parser = new Parser(new Lexer(ctxHtml));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag a = aNode[k].GetATag();
                                        if (a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = aTag.Link;
                                            else
                                                link = "http://www.hnsztb.com.cn/" + a.Link;
                                            BaseAttach entity = null;
                                            try
                                            {
                                                entity = ToolHtml.GetBaseAttach(link, a.LinkText, info.Id);
                                                if(entity==null)
                                                    entity = ToolHtml.GetBaseAttachByUrl(link, a.LinkText, info.Id);
                                                if (entity != null)
                                                    ToolDb.SaveEntity(entity, string.Empty);
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
            return list;
        }
    }
}
