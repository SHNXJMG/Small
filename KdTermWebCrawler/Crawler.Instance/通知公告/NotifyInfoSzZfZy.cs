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

namespace Crawler.Instance
{
    public class NotifyInfoSzZfZy : WebSiteCrawller
    {
        public NotifyInfoSzZfZy()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省深圳市政府采购通知公告";
            this.Description = "自动抓取广东省深圳市政府采购通知公告";
            this.PlanTime = "21:00";
            this.SiteUrl = "http://www.szzfcg.cn/portal/topicView.do?method=view&id=709";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("name", "__ec_pages")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    SelectTag selectTag = pageList[0] as SelectTag;
                    pageInt = selectTag.OptionTags.Length;
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "ec_i","topicChrList_20070702_crd","topicChrList_20070702_f_a",
                    "topicChrList_20070702_p","topicChrList_20070702_s_name","id","method","__ec_pages",
                    "topicChrList_20070702_rd","topicChrList_20070702_f_name","topicChrList_20070702_f_ldate"}, new string[] { 
                    "topicChrList_20070702","20","",i.ToString(),"","709","view",i.ToString(),"20","",""});
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "topicChrList_20070702_table"), new TagNameFilter("table")));
                if (dtList != null && dtList.Count > 0)
                {
                    TableTag table = dtList[0] as TableTag;
                    for (int j = 3; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty,
                            infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty,infoType=string.Empty;
                        headName = tr.Columns[1].ToPlainTextString().Trim();
                        releaseTime = tr.Columns[2].ToPlainTextString().Trim();
                        infoType = "通知公告";
                        ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                        infoUrl = "http://www.szzfcg.cn/portal/documentView.do?method=view&id=" + aTag.Link.Replace("/viewer.do?id=", "");
                        string htmldeil = string.Empty;
                        try
                        {
                            htmldeil = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(infoUrl), Encoding.UTF8);
                        }
                        catch { continue; }
                        Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
                        htmldeil = regexHtml.Replace(htmldeil, "");

                        parser = new Parser(new Lexer(htmldeil));
                        NodeFilter filter = new TagNameFilter("body");
                        NodeList noList = parser.ExtractAllNodesThatMatch(filter);
                        if (noList != null && noList.Count > 0)
                        {
                            ctxHtml = noList.AsHtml().Replace("<br/>", "\r\n").Replace("<BR/>", "");
                            infoCtx = noList.AsString().Replace(" ", "").Replace("&nbsp;", "").Replace("\t\t", "\t").Replace("\t\t", "\t");
                            infoCtx = Regex.Replace(infoCtx, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase).Replace(" ", "").Replace("\t", "");
                            msgType = "深圳政府采购";
                            infoScorce = infoScorce.Replace("&nbsp;", "");
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳市工程", string.Empty, infoCtx, infoType);
                            list.Add(info);
                            if (crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
