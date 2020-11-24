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
    public class NoticeGanSuGgzySlgc : WebSiteCrawller
    {
        public NoticeGanSuGgzySlgc()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "甘肃省公共资源交易中心通知公示(水利及其他工程)";
            this.Description = "自动抓取甘肃省公共资源交易中心通知公示(水利及其他工程)";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 200;
            this.SiteUrl = "http://www.gsggzyjy.cn/InfoPage/InfoList.aspx?SiteItem=146&InfoType=7";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            int pageInt = 37;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            string url = "http://www.gsggzyjy.cn/ajax/Controls_InfoList,App_Web_rzplwhmc.ashx?_method=getCurrentData&_session=rw";
            try
            {
                this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
                string post = "currentPage=1\r\nQuery=";
                html = ToolHtml.GetHtmlByUrlPost(url, post, Encoding.UTF8, ref cookiestr);
            }
            catch { return null; }

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        string post = "currentPage=" + i + "\r\nQuery=";
                        html = ToolHtml.GetHtmlByUrlPost(url, post, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("li"));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty, area = string.Empty;
                        INode node = listNode[j];

                        ATag aTag = node.GetATag();
                        InfoTitle = aTag.GetAttribute("title");
                        InfoType = "控制价公示";
                        PublistTime = node.GetSpan().StringText;
                        InfoUrl = "http://www.gsggzyjy.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ContentPlaceHolder1_InfoHtml")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            htmlTxt = dtlNode.AsHtml();
                            InfoCtx = htmlTxt.ToCtxString();
                            parser = new Parser(new Lexer(htmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                string ctx = string.Empty;
                                TableTag table = tableNode[0] as TableTag;
                                for (int r = 0; r < table.RowCount; r++)
                                {
                                    for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = table.Rows[r].Columns[c].ToNodePlainString();
                                        if ((c + 1) % 2 == 0)
                                            ctx += temp.GetReplace(":,：") + "\r\n";
                                        else
                                            ctx += temp.GetReplace(":,：") + "：";
                                    }
                                }
                                buildUnit = ctx.GetBuildRegex();
                            }
                            NoticeInfo info = ToolDb.GenNoticeInfo("甘肃省", "甘肃省及地市", area, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "甘肃省公共资源交易中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, "水利及其他工程", string.Empty, htmlTxt);

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
                                            link = "http://www.gsggzyjy.cn/" + a.Link;
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
