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
    public class NoticeGzzsgs : WebSiteCrawller
    {
        public NoticeGzzsgs()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省广州市建设工程资审结果公示";
            this.Description = "自动抓取广东省广州市建设工程资审结果公示";
            this.PlanTime = "21:55";
            this.SiteUrl = "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=504";
            this.MaxCount = 500;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagination page-mar")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("共", "条");
                    temp = pageNode.AsString().Replace("共" + temp + "条", "").GetRegexBegEnd("共","页");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "wsbs-table")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        InfoTitle = tr.Columns[1].ToNodePlainString();
                        InfoType = "资格审查";
                        PublistTime = tr.Columns[2].ToPlainTextString().GetDateRegex();

                        InfoUrl = "http://www.gzggzy.cn" + tr.Columns[1].GetATagHref();
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch
                        { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "xx-main")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            htmlTxt = dtlNode.AsHtml().GetJsString();
                            InfoCtx = htmlTxt.ToCtxString();
                            prjCode = InfoCtx.Replace("[", "kdxx").Replace("]", "xxdk").GetRegexBegEnd("kdxx","xxdk");
                            buildUnit = InfoCtx.GetBuildRegex();

                            NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "广州市区", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "广州公共资源交易中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty, htmlTxt);
                            parser = new Parser(new Lexer(htmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag aTag = aNode[k].GetATag();
                                    if (aTag.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (aTag.Link.ToLower().Contains("http"))
                                            link = aTag.Link;
                                        else
                                            link = "http://www.gzggzy.cn" + aTag.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(aTag.LinkText, info.Id, link);
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
