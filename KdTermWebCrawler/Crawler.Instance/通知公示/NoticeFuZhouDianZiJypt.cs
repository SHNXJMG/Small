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
    public class NoticeFuZhouDianZiJypt : WebSiteCrawller
    {
        public NoticeFuZhouDianZiJypt()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "福州建设工程电子招投标通知公示";
            this.Description = "自动抓取福州建设工程电子招投标通知公示";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.fzztb.com/bidding/project/listNotices.shtml?type=biddingSupplements";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            int pageInt = 1;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "td")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString().GetRegexBegEnd("录共", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "biddingSupplements")), true), new TagNameFilter("table")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty, area = string.Empty;

                        InfoType = "补充通知";
                        prjCode = tr.Columns[1].ToNodePlainString();
                        ATag aTag = tr.Columns[3].GetATag();
                        InfoTitle = aTag.LinkText;
                        if (InfoTitle.Contains(".."))
                            InfoTitle = aTag.GetAttribute("title");
                        PublistTime = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.fzztb.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            htmlTxt = dtlNode.AsHtml().GetJsString();
                            InfoCtx = htmlTxt.ToCtxString();
                            buildUnit = InfoCtx.GetBuildRegex();
                            NoticeInfo info = ToolDb.GenNoticeInfo("福建省", "福建省及地市", "福州市", string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "福州市城乡建设委员会", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, "建设工程", string.Empty, htmlTxt);
                            parser = new Parser(new Lexer(htmlTxt));
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
                                            link = "http://www.fzztb.com" + a.Link;
                                        if (Encoding.Default.GetByteCount(link) > 500)
                                            continue;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            } 
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list; 
                        }
                    }
                }
            }
            return list;
        }
    }
}
