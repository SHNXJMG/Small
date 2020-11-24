using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class NotifySTBSZN : WebSiteCrawller
    {
        public NotifySTBSZN()
            : base()
        {
            this.Group = "办事指南";
            this.Title = "广东省汕头建设网办事指南";
            this.PlanTime = "10:02";
            this.Description = "自动抓取广东省汕头建设网办事指南";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.stjs.gov.cn/bsdt/bszn.asp";
            this.MaxCount = 600;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookieStr = string.Empty;
            int pageInt = 1, sqlCount = 0;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default).GetJsString();
            }
            catch (Exception ex)
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("width", "700")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString().GetRegexBegEnd("/", "下");
                    pageInt = Convert.ToInt32(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "?page=" + i.ToString(), Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "98%")));
                if (nodeList != null && nodeList.Count > 1)
                {
                    TableTag table = nodeList[1] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j]; 
                        int attachCount = 0;
                        string temp = tr.Columns[1].GetAttribute("rowSpan");
                        infoType = "办事指南";
                        releaseTime = DateTime.Now.ToString("yyyy-MM-dd");
                        headName = tr.Columns[1].ToNodePlainString();
                        infoUrl = "http://www.stjs.gov.cn/bsdt/" + tr.Columns[1].GetATagHref();
                        msgType = MsgTypeCosnt.ShanTouMsgType;
                        NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "汕头市区", string.Empty, infoCtx, infoType);
                        sqlCount++;
                        if (!crawlAll && sqlCount >= this.MaxCount)
                        {
                            return null;
                        }
                        if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                        {
                            if (infoUrl.IsAtagAttach())
                            {
                                try
                                {
                                    BaseAttach obj = ToolHtml.GetBaseAttach(infoUrl, headName, info.Id);
                                    if (obj != null)
                                        ToolDb.SaveEntity(obj, string.Empty);
                                }
                                catch { }
                            }
                            if (!string.IsNullOrEmpty(temp))
                            {
                                attachCount = Convert.ToInt32(temp);
                                for (int a = 0; a < attachCount; a++)
                                {
                                    TableRow dr = table.Rows[j];
                                    ATag fileUrl = dr.Columns[dr.ColumnCount-1].GetATag();
                                    if (fileUrl.IsAtagAttach())
                                    {
                                        try
                                        {
                                            BaseAttach obj = ToolHtml.GetBaseAttach("http://www.stjs.gov.cn/bsdt/" + fileUrl.Link, fileUrl.LinkText, info.Id);
                                            if (obj != null)
                                                ToolDb.SaveEntity(obj, string.Empty);
                                        }
                                        catch { }
                                    }
                                    j++;
                                }
                                j--;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
