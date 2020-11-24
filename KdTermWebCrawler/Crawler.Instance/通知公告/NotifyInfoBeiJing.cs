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
    public class NotifyInfoBeiJing : WebSiteCrawller
    {
        public NotifyInfoBeiJing()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "北京工程建设交易信息网通知公告(建设公告)";
            this.Description = "自动抓取北京工程建设交易信息网通知公告(建设公告)";
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.bcactc.com/home/news/newslist.aspx?type=1";
            this.MaxCount = 200;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NotifyInfo>();
            int pageInt = 1, sqlCount = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string cookiestr = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "gridview_PagerRow")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("/", "&nbsp");
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
                        new string[] { "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__LASTFOCUS",
                        "__VIEWSTATE",
                        "__VIEWSTATEGENERATOR",
                        "__EVENTVALIDATION",
                        "keyTextBox",
                        "PagerControl1:_ctl4",
                        "PagerControl1:_ctl2.x",
                        "PagerControl1:_ctl2.y"},
                        new string[] {
                            "","","",
                        viewState,
                        "7CE136E4", 
                        eventValidation,
                        "",
                        "",
                        "3","5"
                        }
                        );
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "MyGridView1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        headName = aTag.LinkText;
                        releaseTime = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        infoUrl = aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl,Encoding.Default).GetJsString();
                        }
                        catch { Logger.Error(headName); Logger.Error(pageInt); continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "PopupBody_context")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            if (Encoding.Default.GetByteCount(headName) > 200)
                                headName = headName.Substring(0, 100);
                            ctxHtml = dtlNode.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            List<string> listImg = new List<string>();
                            parser = new Parser(new Lexer(ctxHtml));
                            NodeList imgNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                            if (imgNode != null && imgNode.Count > 0)
                            {
                                for (int m = 0; m < imgNode.Count; m++)
                                {
                                    string link = "http://publish.bcactc.com" + (imgNode[m] as ImageTag).ImageURL;
                                    listImg.Add(link);
                                    ctxHtml = ctxHtml.GetReplace((imgNode[m] as ImageTag).ImageURL, link);
                                }
                            }

                            msgType = "北京市建设工程发包承包交易中心";
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "北京市", "北京市区", "", infoCtx, "通知公告");
                            sqlCount++; 
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                            {
                                if (listImg.Count > 0)
                                {
                                    for (int a = 0; a < listImg.Count; a++)
                                    {
                                        BaseAttach entity = null;
                                        try
                                        {
                                            entity = ToolHtml.GetBaseAttach(listImg[0], headName, info.Id);
                                            if (entity != null)
                                                ToolDb.SaveEntity(entity, string.Empty);
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
                                        ATag fileATag = aNode[k].GetATag();
                                        if (fileATag.IsAtagAttach())
                                        {
                                            BaseAttach obj = null;
                                            try
                                            {
                                                if (fileATag.Link.ToLower().Contains("http")) 
                                                    obj = ToolHtml.GetBaseAttach(fileATag.Link, headName, info.Id); 
                                                else 
                                                    obj = ToolHtml.GetBaseAttach("http://publish.bcactc.com/" + fileATag.Link, headName, info.Id); 
                                            }
                                            catch { }
                                            if (obj != null)
                                                ToolDb.SaveEntity(obj, string.Empty);
                                        }
                                    }
                                } 
                            }
                            if (!crawlAll && sqlCount >= this.MaxCount) return null;
                        }
                    }
                }
            }
            return list;
        }
    }
}
