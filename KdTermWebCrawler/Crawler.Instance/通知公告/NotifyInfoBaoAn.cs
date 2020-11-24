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
    public class NotifyInfoBaoAn : WebSiteCrawller
    {
        public NotifyInfoBaoAn()
            : base()
        {
            this.Group = "通知公告";
            this.PlanTime = "12:00,03:20";
            this.Title = "深圳市宝安区政府采购中心通知公告";
            this.MaxCount = 100000;
            this.Description = "自动抓取深圳市宝安区政府采购中心通知公告";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://cgzx.baoan.gov.cn/gzdt/list_32.aspx";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        { 
            int pageInt = 1,sqlCount=0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "Pages")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode.AsString().GetRegexBegEnd("共有", "页");
                try
                {
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?page=" + i);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "News_list")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;
                        INode node = listNode[j];
                        releaseTime = node.ToPlainTextString().GetDateRegex();
                        ATag aTag = node.GetATag();
                        headName = aTag.GetAttribute("title");
                        infoUrl = "http://cgzx.baoan.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "Scroller-1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            ctxHtml = dtlNode.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            msgType = "深圳市宝安区政府采购中心";
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳政府采购", "宝安区", infoCtx, "通知公告");
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
                                            if (fileATag.Link.ToLower().Contains("http"))
                                            {
                                                obj = ToolHtml.GetBaseAttach(fileATag.Link, headName, info.Id);
                                            }
                                            else
                                            {
                                                obj = ToolHtml.GetBaseAttach("http://ba.szzfcg.cn"+fileATag.Link, headName, info.Id);
                                            }
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
            return null;
        }
    }
}
