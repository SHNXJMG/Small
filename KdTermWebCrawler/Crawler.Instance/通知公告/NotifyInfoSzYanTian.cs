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
    public class NotifyInfoSzYanTian : WebSiteCrawller
    {
        public NotifyInfoSzYanTian()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省深圳市盐田区通知公告";
            this.Description = "自动抓取广东省深圳市盐田区通知公告";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.yantian.gov.cn/icatalog/qzf/08/tzgg/index.shtml"; 
            this.MaxCount = 120;
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("select"));
            if (pageNode != null && pageNode.Count > 0)
            {
                SelectTag selTag = pageNode[0] as SelectTag;
                try
                {
                    string temp = selTag.OptionTags[selTag.OptionTags.Length - 1].OptionText;
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.yantian.gov.cn/icatalog/qzf/08/tzgg/index_"+(i-1).ToString()+".shtml");
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;
                        TableRow tr = table.Rows[j];

                        ATag aTag = tr.Columns[2].GetATag();
                        headName = aTag.GetAttribute("title");
                        releaseTime = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        infoUrl = "http://www.yantian.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            ctxHtml = dtlNode.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();


                            msgType = "深圳市盐田区政府采购中心";
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳区及街道工程", "盐田区", infoCtx, "通知公告");
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
                                                    obj = ToolHtml.GetBaseAttach("http://www.yantian.gov.cn/" + fileATag.Link, headName, info.Id);
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
                                                    obj = ToolHtml.GetBaseAttach("http://www.yantian.gov.cn/" + img.ImageURL, headName, info.Id);
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
