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
    public class NotifyInfoHaiKou : WebSiteCrawller
    {
        public NotifyInfoHaiKou()
            : base(true)
        {
            this.Group = "通知公告";
            this.Title = "海口市建设工程信息网通知公告";
            this.Description = "自动抓取海口市建设工程信息网中标信息通知公告";
            this.PlanTime = "22:00";
            this.SiteUrl = "http://www.hkcein.com/Front/Hydt?typeId=9";
            this.MaxCount = 200;
            this.ExistCompareFields = "InfoUrl";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            int pageInt = 0, sqlCount = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")), true), new TagNameFilter("tr")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[17].ToNodePlainString().GetRegexBegEnd("共", "条");
                    int page = int.Parse(temp);
                    int result = page / 15;
                    if (page % 15 != 0)
                        pageInt = result + 1;
                    else
                        pageInt = result;
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.hkcein.com/Front/Hydt/" + i + "?typeId=9");
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "95%")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();
                        headName = tr.Columns[0].ToNodePlainString();
                        releaseTime = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        infoUrl = "http://www.hkcein.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "nrkd left")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            ctxHtml = dtlNode.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            infoType = "通知公告";
                            msgType = "海口市住房和城乡建设局";
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "海南省", "海南省及地市", "海口市", infoCtx, infoType);
                            sqlCount++;
                            if (!crawlAll && sqlCount >= this.MaxCount) return null;
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                            {
                                parser = new Parser(new Lexer(ctxHtml));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int a = 0; a < aNode.Count; a++)
                                    {
                                        ATag tag = aNode[a] as ATag;
                                        if (tag.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (tag.Link.ToLower().Contains("http"))
                                                link = tag.Link;
                                            else
                                                link = "http://www.hkcein.com/" + tag.Link.GetReplace("../,./");
                                            try
                                            {
                                                BaseAttach baseInfo = ToolHtml.GetBaseAttach(link, tag.LinkText, info.Id);
                                                if (baseInfo != null)
                                                {
                                                    ToolDb.SaveEntity(baseInfo, string.Empty);
                                                }
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
