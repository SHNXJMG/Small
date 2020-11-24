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
    public class NotifyInfoGuiZhou : WebSiteCrawller
    {
        public NotifyInfoGuiZhou()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "贵安新区公告资源交易中心通知公告";
            this.Description = "自动抓取贵安新区公告资源交易中心通知公告";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.gaxqjyzx.com/gaxqztb/tzgg/MoreInfo.aspx?CategoryNum=28";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 500;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NotifyInfo>();
            int pageInt = 1,sqlCount=0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "MoreInfoList1_Pager")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToPlainTextString().GetRegexBegEnd("总页数", "当前").Replace("：", "");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                        new string[] { 
                        "__VIEWSTATE",
                        "__EVENTTARGET",
                        "__EVENTARGUMENT"
                        },
                        new string[] { 
                        viewState,
                        "MoreInfoList1$Pager",
                        i.ToString()
                        }
                        );
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "MoreInfoList1_DataGrid1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        headName = aTag.GetAttribute("title");
                        releaseTime = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        infoUrl = "http://www.gaxqjyzx.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            ctxHtml = dtlNode.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();

                         
                            msgType = "贵安新区公共资源交易中心";
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "贵州省", "贵州省及地市", "贵安新区", infoCtx, "通知公告");
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
                                                    obj = ToolHtml.GetBaseAttach("http://www.gaxqjyzx.com" + fileATag.Link, headName, info.Id);
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
                                                    obj = ToolHtml.GetBaseAttach("http://www.gaxqjyzx.com" + img.ImageURL, headName, info.Id);
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
