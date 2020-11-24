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
    public class NotifyInfoShangHai : WebSiteCrawller
    {
        public NotifyInfoShangHai()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "上海市建筑建材业通知公告";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取上海市建筑建材业通知公告";
            this.SiteUrl = "http://www.ciac.sh.cn/gsgg_new.aspx?lb=zxgg";
            this.MaxCount = 400;
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch 
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("name", "PageListControl1$ctl06")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    SelectTag tag = pageNode[0] as SelectTag;
                    string temp = tag.OptionTags[tag.OptionTags.Length - 1].Value;
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__VIEWSTATE",
                    "__EVENTVALIDATION",
                    "PageListControl1$ctl03",
                    "PageListControl1$ctl06",
                    "select2"
                    }, new string[]{
                    viewState,
                    eventValidation,
                    "下一页",
                    (i-1).ToString(),
                    "** 站点链接 **"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "Listbody")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount - 1; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;
                        msgType = "上海市建筑业管理办公室";
                        infoType = "通知公告";
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();
                        headName = aTag.LinkText.GetReplace("·, ");
                        releaseTime = tr.Columns[1].ToPlainTextString().GetDateRegex();

                        infoUrl = "http://www.ciac.sh.cn/newsdata/" + aTag.GetAttribute("onclick").GetRegexBegEnd("'", "'");
                        if (infoUrl.IsAtagAttach())
                        { 
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "上海市", "上海市区", string.Empty, infoCtx, infoType);
                            sqlCount++;
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                            {
                                BaseAttach entity = null;
                                try
                                {
                                    entity = ToolHtml.GetBaseAttach(infoUrl, headName, info.Id);
                                    if (entity != null)
                                        ToolDb.SaveEntity(entity, string.Empty);
                                }
                                catch { }
                            }
                            if (!crawlAll && sqlCount >= this.MaxCount) return null;
                            continue;
                        }
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("width", "771")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            ctxHtml = dtlNode.AsHtml();
                            if (headName.Contains("..."))
                            {
                                parser = new Parser(new Lexer(ctxHtml));
                                NodeList pNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("p"), new HasAttributeFilter("class", "bb")));
                                if (pNode != null && pNode.Count > 0)
                                {
                                    string temp = pNode[0].ToNodePlainString();
                                    headName = string.IsNullOrEmpty(temp) ? headName : temp;
                                }
                            }
                            infoCtx = ctxHtml.ToCtxString();
                            List<string> listImg = new List<string>();
                            parser = new Parser(new Lexer(ctxHtml));
                            NodeList imgNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                            if (imgNode != null && imgNode.Count > 0)
                            {
                                for (int m = 0; m < imgNode.Count; m++)
                                {
                                    string link = "http://www.ciac.sh.cn/newsdata/" + (imgNode[m] as ImageTag).ImageURL;
                                    listImg.Add(link);
                                    ctxHtml = ctxHtml.GetReplace((imgNode[m] as ImageTag).ImageURL, link);
                                }
                            }
                        
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "上海市", "上海市区", string.Empty, infoCtx, infoType);
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
                                        ATag a = aNode[k].GetATag();
                                        if (a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = a.Link;
                                            else
                                                link = "http://www.ciac.sh.cn/newsdata/" + a.Link;
                                            BaseAttach entity = null;
                                            try
                                            {
                                                entity = ToolHtml.GetBaseAttach(link, a.LinkText, info.Id);
                                                if (entity != null)
                                                    ToolDb.SaveEntity(entity, string.Empty);
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }
                            if (!crawlAll && sqlCount >= this.MaxCount) return null;
                        }
                    }
                }
            }
            return null;
        }
    }
}
