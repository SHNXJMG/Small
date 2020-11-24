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
    public class NoticeLiaoNingZtb : WebSiteCrawller
    {
        public NoticeLiaoNingZtb()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "辽宁省招标投标监管网通知公示";
            this.Description = "自动抓取辽宁省招标投标监管网通知公示";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.lntb.gov.cn/Article_Class2.asp?ClassID=31";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch
            {
                return null;
            }
            try
            {
                string temp = html.GetRegexBegEnd("<strong>", "</strong>").GetReplace("<fontcolor=red>1</font>/");
                pageInt = int.Parse(temp);
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&SpecialID=0&page=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "200")), true), new TagNameFilter("a")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        ATag aTag = listNode[j].GetATag();
                        if (aTag == null) continue;
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty, area = string.Empty;
                        string temp = aTag.GetAttribute("title");
                        InfoType = "资格预审";
                        InfoTitle = temp.GetRegex("文章标题");
                        prjCode = temp.GetRegex("招标代码");
                        PublistTime = temp.GetRegex("更新时间").GetDateRegex("yyyy/MM/dd");
                        InfoUrl = "http://www.lntb.gov.cn/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "200")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            htmlTxt = dtlNode.AsHtml().GetJsString();
                            InfoCtx = htmlTxt.ToLower().GetReplace("</p>,<br/>,<br>", "\r\n").ToCtxString();
                            buildUnit = InfoCtx.GetBuildRegex();

                            NoticeInfo info = ToolDb.GenNoticeInfo("辽宁省", "辽宁省及地市", area, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "辽宁省招标投标协调管理办公室", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, "建设工程", "建设工程", htmlTxt);
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
                                            link = "http://www.lntb.gov.cn/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }

                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                        else
                        {
                            Logger.Error("无内容");
                            Logger.Error(InfoUrl);
                        }
                    }
                }
            }
            return list;
        }
    }
}
