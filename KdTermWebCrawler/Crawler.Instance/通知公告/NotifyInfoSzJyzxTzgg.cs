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
using System.Web.Script.Serialization;

namespace Crawler.Instance
{
    public class NotifyInfoSzJyzxTzgg : WebSiteCrawller
    {
        public NotifyInfoSzJyzxTzgg()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省深圳市交易中心通知公告(2015版)";
            this.PlanTime = "9:27,11:27,14:17,17:27";
            this.Description = "自动抓取广东省深圳市交易中心通知公告(2015版)";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.szjs.gov.cn/jsjy/zxsz/tzgg/";
            this.MaxCount = 200;
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "pages")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("共", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "index_" + (i - 1) + ".htm");
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "page_list_ul")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 1; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        ATag aTag = node.GetATag();
                        headName = aTag.GetAttribute("title");
                        releaseTime = node.ToPlainTextString().GetDateRegex();
                        infoType = "通知公告";
                        msgType = MsgTypeCosnt.ShenZhenMsgType;
                        infoUrl = "http://www.szjs.gov.cn/jsjy/zxsz/tzgg/" + aTag.Link.GetReplace("../,./");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl).GetJsString();
                        }
                        catch { continue; }
                        if (aTag.IsAtagAttach())
                        {
                            sqlCount++;
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳市工程", "", infoCtx, infoType);
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                            {
                                try
                                {
                                    BaseAttach obj = ToolHtml.GetBaseAttach(infoUrl, headName, info.Id);
                                    if (obj != null)
                                        ToolDb.SaveEntity(obj, string.Empty);
                                }
                                catch { }
                            }
                            continue;
                        }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "detail_contect")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            ctxHtml = dtlNode.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳市工程", "", infoCtx, infoType);

                            if (!crawlAll && sqlCount >= this.MaxCount) return null;
                            sqlCount++;
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields,this.ExistsUpdate))
                            {
                                parser = new Parser(new Lexer(ctxHtml));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag tag = aNode[k].GetATag();
                                        if (tag.IsAtagAttach())
                                        {
                                            string temp =aTag.Link.GetReplace("../,./");
                                            string alink = "http://www.szjs.gov.cn/jsjy/zxsz/tzgg/" + temp.Substring(0,temp.LastIndexOf("/"))+ tag.Link.GetReplace("./","/");
                                            try
                                            {
                                                BaseAttach obj = ToolHtml.GetBaseAttach(alink, tag.LinkText, info.Id);
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
