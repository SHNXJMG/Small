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
    public class NoticeSGJSFDC : WebSiteCrawller
    {
        public NoticeSGJSFDC()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省韶关市建设与房地产信息网公告公示";
            this.PlanTime = "9:25,11:25,14:15,17:25";
            this.Description = "自动抓取广东省韶关市建设与房地产信息网公告公示";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.sgjsj.gov.cn/sgwebims/MessageSearchResult.aspx?ColumnID=247&KeyWord=";
            this.MaxCount = 40;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookieStr = string.Empty;
            int pageInt = 1;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "mypager")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString().GetRegexBegEnd("共有：", "页");
                    pageInt = int.Parse(temp.Replace("(", ""));
                }
                catch
                { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    try
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetViewState(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                            new string[]{
                            "__EVENTTARGET",
                            "__EVENTARGUMENT",
                            "__VIEWSTATE",
                            "searcher:txtKeyWord",
                            "searcher:tcInputDateTime:txtDateTime1",
                            "searcher:tcInputDateTime:txtDateTime2",
                            "searcher:ddlProvince",
                            "searcher:ddlCity1",
                            "searcher:ddlCity2",
                            "dataPager_input"
                            },
                            new string[]{
                            "dataPager",
                            i.ToString(),
                            viewState,
                            "","","",
                            "-1","-1","-1","1"
                            }
                            ); 
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"),new HasAttributeFilter("width","560")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty,htmlTxt=string.Empty;

                        TableRow tr = table.Rows[j];
                        InfoType = "公告公示";
                        InfoTitle = tr.Columns[0].ToNodePlainString();
                        PublistTime = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.sgjsj.gov.cn/sgwebims/" + tr.Columns[0].GetATagValue("onclick").Replace("window.open(\"", "").Replace("\");", "");
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table4")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            htmlTxt = dtlList.AsHtml();
                            InfoCtx = dtlList.AsHtml().ToCtxString().Replace("\r\n\r\n","\r\n");
                            buildUnit = InfoCtx.GetBuildRegex(); 
                        }
                        NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "韶关市区", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.ShaoGuanMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty,htmlTxt);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
