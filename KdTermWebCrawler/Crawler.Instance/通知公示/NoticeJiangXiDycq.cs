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
    public class NoticeJiangXiDycq : WebSiteCrawller
    {
        public NoticeJiangXiDycq()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "江西省公共资源交易中心水利工程通知公示(答疑澄清补充通知)";
            this.Description = "自动抓取江西省公共资源交易中心水利工程通知公示(答疑澄清补充通知)";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.jxsggzy.cn/web/jyxx/002003/002003002/jyxx.html";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;//http://free.qidian.com/Free/ReadChapter.aspx?bookid=1999914&chapterid=37164498
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "wb-page-li")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("/", "\r");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {


                SiteUrl = "http://www.jxsggzy.cn/web/jyxx/002003/002003002/" + i + ".html";
                try
                {

                    html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
                }
                catch { continue; }
                    parser = new Parser(new Lexer(html));
                    NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "ewb-list-node clearfix")));
                    if (listNode != null && listNode.Count > 0)
                    {
                        for (int j = 0; j < listNode.Count; j++)
                        {
                            string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty, area = string.Empty;
                           
                        ATag aTag = listNode[j].GetATag();
                        InfoTitle = aTag.GetAttribute("title");

                        if (string.IsNullOrWhiteSpace(InfoTitle))
                            InfoTitle = aTag.LinkText;
                        InfoType = "答疑澄清补充通知";
                            PublistTime = listNode[j].ToPlainTextString().GetDateRegex();
                        if (InfoTitle[2].Equals('县') || InfoTitle[2].Equals('区') || InfoTitle[2].Equals('市'))
                                area = InfoTitle.Substring(0, 3);
                            InfoUrl = "http://www.jxsggzy.cn" + aTag.Link;
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ewb-detail-box")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                htmlTxt = dtlNode.AsHtml().GetJsString();
                                InfoCtx = htmlTxt.ToCtxString();
                                buildUnit = InfoCtx.GetBuildRegex();
                                NoticeInfo info = ToolDb.GenNoticeInfo("江西省", "江西省及地市", area, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "江西省公共资源交易中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty, htmlTxt);
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
                                                link = "http://jjggzy.jiangxi.gov.cn" + a.Link;
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
