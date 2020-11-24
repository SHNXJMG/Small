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
    public class NoticeZheJiangJsgc : WebSiteCrawller
    {
        public NoticeZheJiangJsgc()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "浙江省公共资源交易中心通知公示";
            this.Description = "自动抓取浙江省公共资源交易中心通知公示";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.zmctc.com/zjgcjy/Notice/BcwjMore.aspx";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch { }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "BcwjInfoList1_Pager")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString().GetRegexBegEnd("1/", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__VIEWSTATE",
                    "BcwjInfoList1:KeyWord",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT" 
                    }, new string[]{
                    viewState,
                    "",
                    "BcwjInfoList1:Pager",
                    i.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "BcwjInfoList1_DataGrid1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty;
                        InfoType = "补充通知";
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        InfoTitle = aTag.GetAttribute("title").GetReplace(";");
                        prjCode = tr.Columns[1].ToNodePlainString().GetReplace("[", "【").GetReplace("]", "】").GetRegexBegEnd("【", "】");
                        PublistTime = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl =  aTag.Link;
                        if (!InfoUrl.Contains("http"))
                            continue;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "spnShow")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            htmlTxt = dtlNode.AsHtml();
                            InfoCtx = htmlTxt.GetReplace("</p>,<br />,<br/>", "\r\n").ToCtxString();
                            NoticeInfo info = ToolDb.GenNoticeInfo("浙江省", "浙江省及地市", "", string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "浙江省公共资源交易中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, "政府采购", "建设工程", htmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(htmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k] as ATag;
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://downc.zmctc.com/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
