using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class NotifyInfoJiangXiFzw : WebSiteCrawller
    {
        public NotifyInfoJiangXiFzw()
            : base()
        {
            this.Group = "通知公告";
            this.PlanTime = "12:00,03:20";
            this.Title = "江西省发展和改革委员会通知公告";
            this.MaxCount = 300;
            this.Description = "自动抓取江西省发展和改革委员会通知公告";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.jxdpc.gov.cn/tztg/";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NotifyInfo>();
            int pageInt = 1,sqlCount=0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tdfont")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().Replace("function createPageHTML", "").GetRegexBegEnd("createPageHTML", ",").Replace("(", "");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "index_" + (i - 1) + ".htm", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellspacing", "3")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        if (table.Rows[j].ColumnCount < 2) continue;
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                           ATag aTag = tr.Columns[1].GetATag();
                        headName = aTag.GetAttribute("title");
                        releaseTime = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        if (aTag.Link.ToLower().Contains("departmentsite"))
                        {
                            infoUrl = "http://www.jxdpc.gov.cn/" + aTag.Link.Replace("../", "");
                        }
                        else if (aTag.Link.ToLower().Contains("gztz"))
                        {
                            infoUrl = "http://www.jxdpc.gov.cn/" + "tztg/" + aTag.Link.Replace("./", "");
                        }
                        else if (aTag.Link.ToLower().Contains("gsgg"))
                        {
                            infoUrl = "http://www.jxdpc.gov.cn/" + aTag.Link.Replace("../", "");
                        }
                        else
                        {
                            infoUrl = aTag.Link.Replace("./","");
                        }
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.Default);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "artibody")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            ctxHtml = dtlNode[0].ToHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            msgType = "江西省发展和改革委员会";
                            infoType = "通知公告";
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "江西省", "江西省及地市", string.Empty, infoCtx, infoType);
                            if (!crawlAll && sqlCount >= this.MaxCount) return null;
                            sqlCount++;
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                            {
                                parser = new Parser(new Lexer(htmldtl));
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
                                            {
                                                string temp = string.Empty;
                                                temp = temp.Remove(temp.LastIndexOf("/"));
                                                link = temp + a.Link.Replace("./", "/");
                                            }
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
                          
                        }
                    }
                }
            }
            return list;
        }
    }
}
