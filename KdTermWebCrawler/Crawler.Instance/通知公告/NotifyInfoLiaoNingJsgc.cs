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
    public class NotifyInfoLiaoNingJsgc : WebSiteCrawller
    {
        public NotifyInfoLiaoNingJsgc()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "辽宁省建设工程信息网通知公告";
            this.Description = "自动抓取辽宁省建设工程信息网通知公告";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.lnzb.cn/lnzbtb/tzgg/MoreInfo.aspx?CategoryNum=009";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {  
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1,sqlCount=0;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "MoreInfoList1_Pager")));
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
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                    "__VIEWSTATE",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__EVENTVALIDATION"
                    }, new string[] { 
                    viewState,
                    "MoreInfoList1$Pager",
                    i.ToString(),
                    eventValidation
                    });
                    try
                    {
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
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        infoType = "通知公告";
                        headName = aTag.GetAttribute("title");
                        releaseTime = tr.Columns[2].ToPlainTextString().GetDateRegex();

                        infoUrl = "http://www.lnzb.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "tblInfo")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        { 
                          ctxHtml = dtlNode.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            msgType = "辽宁省建设厅招标投标管理处";
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "辽宁省", "辽宁省及地市", "", infoCtx, infoType);
                            sqlCount++;
                            if (!crawlAll && sqlCount >= this.MaxCount) return null;
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                            { 
                            parser = new Parser(new Lexer(ctxHtml));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag a = aNode[k] as ATag;
                                        if (a.Link.ToLower().Contains("readattachfile") || a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = a.Link;
                                            else
                                                link = "http://www.lnzb.cn" + a.Link.GetReplace("../,./");
                                            if (Encoding.Default.GetByteCount(link) > 500)
                                                continue;
                                            try
                                            {
                                                BaseAttach attach = ToolHtml.GetBaseAttachByUrl(link, a.LinkText, info.Id);
                                                if (attach != null)
                                                    ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
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
            return null;
        }
    }
}
