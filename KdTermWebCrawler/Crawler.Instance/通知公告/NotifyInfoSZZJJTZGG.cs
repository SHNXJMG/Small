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
using System.Web;

namespace Crawler.Instance
{
    public class NotifyInfoSZZJJTZGG : WebSiteCrawller
    {
        public NotifyInfoSZZJJTZGG()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省深圳市住房和建设局通知公告";
            this.Description = "自动抓取广东省深圳市住房和建设局通知公告";
            this.PlanTime = "21:52";
            this.SiteUrl = "http://www.szjs.gov.cn/ztfw/gcjs/gzgg/";
            this.MaxCount = 2000;
            this.ExistCompareFields = "InfoUrl";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            //取得页码
            int pageInt = 1, sqlCount = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiostr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            //NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter(""),new HasAttributeFilter("","")));
            //if (pageNode != null && pageNode.Count > 0)
            //{ 

            //}
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {

                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list")), true), new TagNameFilter("div")));
                if (nodeList != null && nodeList.Count > 0)
                {

                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        //continue;
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty,
                              infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        headName = nodeList[j].GetATagValue("title");
                        releaseTime = nodeList[j].ToPlainTextString().GetDateRegex();
                        infoType = "通知公告";
                        infoUrl = "http://www.szjs.gov.cn/ztfw/gcjs/gzgg/" + nodeList[j].GetATagHref().Replace("../", "").Replace("./", "");

                        string htldtl = string.Empty;
                        if (infoUrl.Contains("http://www.sz.gov.cn/"))
                        {
                            infoUrl = nodeList[j].GetATagHref();
                        }
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch
                        {
                            continue;
                            //try
                            //{
                            //    infoUrl = nodeList[j].GetATagHref();
                            //    htldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.Default).GetJsString();
                            //}
                            //catch { 
                                
                            //    continue;
                            //}
                        }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList noList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content")));
                        if (noList == null || noList.Count <= 0)
                        {
                            parser.Reset();
                            noList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "Custom_UnionStyle")));
                        }
                        if (noList == null || noList.Count <= 0)
                        {
                            parser.Reset();
                            noList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "contentWrap")));
                        }
                        if (noList != null && noList.Count > 0)
                        { 
                            ctxHtml = noList.AsHtml().Replace("<br/>", "\r\n").Replace("<BR/>", "");
                            infoCtx = noList.AsString().Replace(" ", "").Replace("&nbsp;", "").Replace("\t\t", "\t").Replace("\t\t", "\t");
                            infoCtx = Regex.Replace(infoCtx, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase).Replace(" ", "").Replace("\t", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            msgType = MsgTypeCosnt.ShenZhenZJJMsgType;
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
                                    parser = new Parser(new Lexer(htldtl));
                                    NodeList aList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "same")), true), new TagNameFilter("a")));
                                    if (aList == null || aList.Count <= 0)
                                    {
                                        parser.Reset();
                                        aList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "contentWrap")), true), new TagNameFilter("a")));
                                    }
                                    if (aList != null && aList.Count > 0)
                                    {
                                        for (int k = 0; k < aList.Count; k++)
                                        {
                                            ATag a = aList[k].GetATag();
                                            if (a.IsAtagAttach())
                                            {
                                                try
                                                {
                                                    string temp = nodeList[j].GetATagHref();
                                                    string link = string.Empty;
                                                    if (temp.Contains("http"))
                                                    {
                                                        string tem = temp.GetRegexBegEnd("tzgg/", "/");
                                                        link = "http://www.sz.gov.cn/jsj/qt/tzgg/" + tem + "/" + a.Link.Replace("./", "");
                                                    }
                                                    else
                                                    {
                                                        string tem = infoUrl.GetRegexBegEnd("gzgg/", "/");
                                                        link = "http://www.szjs.gov.cn/ztfw/gcjs/gzgg/" + tem + "/" + a.Link.Replace("./", "");
                                                    }
                                                    BaseAttach obj = ToolHtml.GetBaseAttach(link, a.LinkText, info.Id);
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
            }
            return null;
        }
    }
}
