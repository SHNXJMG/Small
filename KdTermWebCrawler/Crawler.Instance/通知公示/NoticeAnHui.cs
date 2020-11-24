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
    public class NoticeAnHui : WebSiteCrawller
    {
        public NoticeAnHui()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "安徽省发展和改革委员会通知公示";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取安徽省发展和改革委员会通知公示";
            this.SiteUrl = "http://www.ahtba.org.cn/Notice/AnhuiNoticeSearch?spid=714&scid=596";
            this.MaxCount = 400;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagination f_right")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].GetATagValue("onclick").GetRegexBegEnd("Info", ",").GetReplace("(");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&pageSize=15&pageNum=" + i);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "newsList")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;

                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty, area = string.Empty;

                        InfoType = "资格预审";
                        area = node.ToNodePlainString().GetReplace("[", "【").GetReplace("]", "】").GetRegexBegEnd("【", "】");
                        InfoTitle = aTag.GetAttribute("title");
                        PublistTime = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.ahtba.org.cn" + aTag.Link.GetReplace("amp;");
                        string id = aTag.Link.Substring(aTag.Link.IndexOf("id="), aTag.Link.Length - aTag.Link.IndexOf("id=")).GetReplace("id=");
                        string htmldtl = string.Empty;
                        try
                        {
                            NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                            "id"
                            }, new string[]{
                             id
                            });
                            htmldtl = this.ToolWebSite.GetHtmlByUrl("http://www.ahtba.org.cn/Notice/NoticeContent", nvc).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "new_detail")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            htmlTxt = dtlNode.AsHtml();
                            InfoCtx = htmlTxt.ToCtxString();
                            prjCode = InfoCtx.GetReplace(" ").GetCodeRegex().GetCodeDel();
                            buildUnit = InfoCtx.GetReplace(" ").GetBuildRegex();
                            NoticeInfo info = ToolDb.GenNoticeInfo("安徽省", "安徽省及地市", area, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "安徽省发展和改革委员会", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, "建设工程", string.Empty, htmlTxt);

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
                                            link = "http://www.ahtba.org.cn/" + a.Link;
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
