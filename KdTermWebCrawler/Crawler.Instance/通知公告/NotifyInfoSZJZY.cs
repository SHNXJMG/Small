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
    public class NotifyInfoSZJZY : WebSiteCrawller
    {
        public NotifyInfoSZJZY()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省深圳市建筑业网协会通知公告";
            this.Description = "自动抓取广东省深圳市建筑业网协会通知公告";
            this.PlanTime = "21:57";
            this.SiteUrl = "http://www.jianzhuxh.com/news/index.asp?typeId=01";
            this.MaxCount = 1200;
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
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "f12")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString();
                    pageInt = Convert.ToInt32(temp.GetRegexBegEnd("/", "页"));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    try
                    { 
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl+"&page="+i.ToString(), Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"),new HasAttributeFilter("height","32")),true),new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 0; j < table.RowCount  -1; j++)
                    {
                        if ((j + 1) % 2 == 0)
                            continue;
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        infoType = "通知公告";
                        releaseTime = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        headName = tr.Columns[1].ToNodePlainString();
                        infoUrl = "http://www.jianzhuxh.com/news/" + tr.Columns[1].GetATagValue("onclick").GetRegexBegEnd("'", "'");
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = ToolHtml.GetHtmlByUrl(infoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList noList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "text18")));
                        if (noList != null && noList.Count > 0)
                        {
                            ctxHtml = noList[0].ToHtml().Replace("<br/>", "\r\n").Replace("<BR/>", "");
                            infoCtx = ctxHtml.ToCtxString().Replace(" ", "").Replace("&nbsp;", "").Replace("\t\t", "\t").Replace("\t\t", "\t");
                            infoCtx = Regex.Replace(infoCtx, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase).Replace(" ", "").Replace("\t", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            msgType = MsgTypeCosnt.ShenZhenJZYMsgType;
                            infoScorce = infoScorce.Replace("&nbsp;", "");
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳市工程", string.Empty, infoCtx, infoType);
                            sqlCount++;
                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return null;
                            }
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                            {
                                parser = new Parser(new Lexer(ctxHtml));
                                NodeList imgList = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                                if (imgList != null && imgList.Count > 0)
                                {
                                    for (int m = 0; m < imgList.Count; m++)
                                    {
                                        try
                                        {
                                            ImageTag img = imgList[m] as ImageTag;
                                            string src = img.GetAttribute("src");
                                            if (src.ToLower().Contains(".gif"))
                                                continue;
                                            BaseAttach obj = null;
                                            if (src.Contains("http"))
                                            {
                                                obj = ToolHtml.GetBaseAttach(src, headName, info.Id);
                                            }
                                            else
                                            {
                                                obj = ToolHtml.GetBaseAttach("http://www.jianzhuxh.com" + src.Replace("../", "/").Replace("./", "/"), headName, info.Id);
                                            }
                                            if (obj != null)
                                                ToolDb.SaveEntity(obj, string.Empty);
                                        }
                                        catch { }
                                    }
                                }
                                parser = new Parser(new Lexer(ctxHtml));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int a = 0; a < aNode.Count; a++)
                                    {
                                        ATag aTag = aNode[a] as ATag;
                                        if (aTag.IsAtagAttach())
                                        {
                                            try
                                            {
                                                BaseAttach obj = null;
                                                string href = aTag.GetATagHref();
                                                if (href.Contains("http"))
                                                {
                                                    obj = ToolHtml.GetBaseAttach(href, aTag.LinkText, info.Id);
                                                }
                                                else
                                                {
                                                    obj = ToolHtml.GetBaseAttach("http://www.jianzhuxh.com" + href.Replace("../", "/").Replace("./", "/"), aTag.LinkText, info.Id);
                                                }
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
