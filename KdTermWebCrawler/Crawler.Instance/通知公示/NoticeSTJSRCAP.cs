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
    public class NoticeSTJSRCAP : WebSiteCrawller
    {
        public NoticeSTJSRCAP()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省汕头建设网日程安排";
            this.PlanTime = "9:13,11:13,14:13,17:13";
            this.Description = "自动抓取广东省汕头建设网日程安排";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.stjs.gov.cn/zbtb/zhaobiao_richeng.asp";
            this.MaxCount = 600;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookieStr = string.Empty;
            int pageInt = 1;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default).GetJsString();
            }
            catch (Exception ex)
            {
                return list;
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
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "705")));
                if (nodeList != null && nodeList.Count > 2)
                {
                    TableTag table = nodeList[2] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        InfoTitle = tr.Columns[1].ToNodePlainString();
                        PublistTime = tr.Columns[2].ToPlainTextString().Replace(".", "-");
                        InfoType = "日程安排";
                        InfoUrl = "http://www.stjs.gov.cn/zbtb/" + tr.Columns[1].GetATagHref();

                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }

                        parser = new Parser(new Lexer(htldtl.ToLower()));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("bgcolor", "#e1e1e1")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            htmlTxt = dtlList[dtlList.Count - 1].ToHtml();
                            InfoCtx = htmlTxt.ToCtxString().Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace(" ", "");

                            NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "汕头市区", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.ShanTouMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty, htmlTxt);
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
