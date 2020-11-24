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
    public class NotifyInfoSZHYZS : WebSiteCrawller
    {
        public NotifyInfoSZHYZS()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省深圳市装饰网协会通知公告";
            this.Description = "自动抓取广东省深圳市装饰网协会通知公告";
            this.PlanTime = "21:54";
            this.SiteUrl = "http://www.szzszx.com.cn/news/newsList.aspx?typeid=202";
            this.MaxCount = 500;
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
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "divPage")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString();
                    pageInt = Convert.ToInt32(temp.GetRegexBegEnd("共", "页"));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl+"&pageindex="+i.ToString(), Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "list")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        infoType = "通知公告";
                        releaseTime = nodeList[j].ToPlainTextString().GetDateRegex();
                        headName = nodeList[j].GetATag().LinkText;
                        //try
                        //{
                        //    headName = headName.Substring(3, headName.Length - 3).Replace(".","");
                        //}
                        //catch { headName = nodeList[j].ToNodePlainString().Replace(releaseTime, ""); }
                        infoUrl = "http://www.szzszx.com.cn" + nodeList[j].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = ToolHtml.GetHtmlByUrl(infoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch {  }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList noList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));
                        if (noList != null && noList.Count > 0)
                        {
                            ctxHtml = noList[0].ToHtml().Replace("<br/>", "\r\n").Replace("<BR/>", "");
                            infoCtx = ctxHtml.ToCtxString().Replace(" ", "").Replace("&nbsp;", "").Replace("\t\t", "\t").Replace("\t\t", "\t");
                            infoCtx = Regex.Replace(infoCtx, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase).Replace(" ", "").Replace("\t", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            msgType = MsgTypeCosnt.ShenZhenZSWMsgType;
                            infoScorce = infoScorce.Replace("&nbsp;", "");
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳市工程", string.Empty, infoCtx, infoType);
                            sqlCount++;
                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return null;
                            }
                            ToolDb.SaveEntity(info, this.ExistCompareFields, ExistsUpdate);
                            //if (ToolDb.SaveEntity(info, this.ExistCompareFields,ExistsUpdate))
                            //{
                            //    #region 抓取附件
                            //    parser = new Parser(new Lexer(ctxHtml));
                            //    NodeList imgList = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                            //    if (imgList != null && imgList.Count > 0)
                            //    {
                            //        for (int m = 0; m < imgList.Count; m++)
                            //        {
                            //            try
                            //            {
                            //                ImageTag img = imgList[m] as ImageTag;
                            //                string src = img.GetAttribute("src");
                            //                if (src.ToLower().Contains(".gif"))
                            //                    continue;
                            //                BaseAttach obj = null;
                            //                if (src.Contains("http"))
                            //                {
                            //                    obj = ToolHtml.GetBaseAttach(src, headName, info.Id);
                            //                }
                            //                else
                            //                {
                            //                    obj = ToolHtml.GetBaseAttach("http://www.szzszx.com.cn" + src.Replace("../", "/").Replace("./", "/"), headName, info.Id);
                            //                }
                            //                if (obj != null)
                            //                    ToolDb.SaveEntity(obj, string.Empty);
                            //            }
                            //            catch { }
                            //        }
                            //    }
                            //    parser = new Parser(new Lexer(ctxHtml));
                            //    NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            //    if (aNode != null && aNode.Count > 0)
                            //    {
                            //        for (int a = 0; a < aNode.Count; a++)
                            //        {
                            //            ATag aTag = aNode[a] as ATag;
                            //            string s = aTag.Link;
                            //            if (aTag.IsAtagAttach())
                            //            {
                            //                try
                            //                {
                            //                    BaseAttach obj = null;
                            //                    string href = aTag.GetATagHref();
                            //                    if (href.Contains("http"))
                            //                    {
                            //                        obj = ToolHtml.GetBaseAttach(href, aTag.LinkText, info.Id);
                            //                    }
                            //                    else
                            //                    {
                            //                        obj = ToolHtml.GetBaseAttach("http://www.szzszx.com.cn"+href.Replace("../","/").Replace("./","/"), aTag.LinkText, info.Id);
                            //                    }
                            //                    if (obj != null)
                            //                        ToolDb.SaveEntity(obj, string.Empty);
                            //                }
                            //                catch { continue; }
                            //            }
                            //        }
                            //    }
                            //    #endregion
                            //}
                        }
                    }
                }
            }
            return null;
        }
    }
}
