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
    public class NotifyInfoSTZCFG : WebSiteCrawller
    {
        public NotifyInfoSTZCFG()
            : base()
        {
            this.Group = "政策法规";
            this.Title = "广东省汕头建设网政策法规";
            this.PlanTime = "1 22:32";
            this.Description = "自动抓取广东省汕头建设网政策法规";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.stjs.gov.cn/zbtb/zcfg.asp";
            this.MaxCount = 500;
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
                  NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "5")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j]; 
                        string temp = tr.Columns[1].GetAttribute("rowSpan");
                        infoType = "政策法规";
                        releaseTime = DateTime.Now.ToString("yyyy-MM-dd");
                        headName = tr.Columns[1].ToNodePlainString();
                        infoUrl = "http://www.stjs.gov.cn/zbtb/" + tr.Columns[1].GetATagHref();
                        msgType = MsgTypeCosnt.ShanTouMsgType;
                        NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "汕头市区", string.Empty, infoCtx, infoType);
                        sqlCount++;
                        if (!crawlAll && sqlCount >= this.MaxCount)
                        {
                            return null;
                        }
                        if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                        {
                            try
                            {
                                BaseAttach attach = ToolHtml.GetBaseAttach(infoUrl, headName, info.Id);
                                if (attach != null)
                                    ToolDb.SaveEntity(attach, string.Empty);
                            }
                            catch { }
                        }
                    }
                }
            }
            return null;
        }
    }
}
