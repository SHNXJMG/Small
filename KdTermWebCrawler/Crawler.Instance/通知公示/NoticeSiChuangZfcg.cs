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
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class NoticeSiChuangZfcg : WebSiteCrawller
    {
        public NoticeSiChuangZfcg()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "四川省公共资源交易中心政府采购通知公示";
            this.Description = "自动抓取四川省公共资源交易中心政府采购通知公示";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.spprec.com/sczw/jyfwpt/005002/005002005/MoreInfo.aspx?CategoryNum=005002005";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("vAlign", "bottom")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("总页数：", "当前");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    string __CSRFTOKEN = ToolHtml.GetHtmlInputValue(html, "__CSRFTOKEN");
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__CSRFTOKEN",
                    "__VIEWSTATE",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT"
                    },
                        new string[]{
                     __CSRFTOKEN,
                        viewState,
                              "MoreInfoList1$Pager",
                        i.ToString()
                        });
                    try
                    {
                        cookiestr = cookiestr.GetReplace(new string[] { "path=/;", "HttpOnly", "," });
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "MoreInfoList1_DataGrid1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, htmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        InfoTitle = aTag.GetAttribute("title");
                        if (Encoding.Default.GetByteCount(InfoTitle) > 150)
                            InfoTitle = aTag.LinkText;
                        PublistTime = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.spprec.com" + aTag.Link;
                        InfoType = "变更公告";
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                        if (dtlNode == null || dtlNode.Count < 1)
                        {
                            dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ivs_content")));
                        }

                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            htmlTxt = dtlNode.AsHtml();
                            InfoCtx = htmlTxt.GetReplace("<br />,<br/>,<br>,</p>", "\r\n").ToCtxString();

                            NoticeInfo info = ToolDb.GenNoticeInfo("四川省", "四川省及地市", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "四川省公共资源交易中心", InfoUrl, string.Empty, string.Empty, string.Empty, string.Empty, "政府采购", string.Empty, htmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(htmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag tag = aNode[k] as ATag;
                                    if (tag.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (tag.Link.ToLower().Contains("http"))
                                            link = tag.Link;
                                        else
                                            link = "http://www.spprec.com" + tag.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(tag.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                        else
                            Logger.Error("无内容" + InfoUrl);

                    }

                }
            }
            return list;
        }

    }
}
