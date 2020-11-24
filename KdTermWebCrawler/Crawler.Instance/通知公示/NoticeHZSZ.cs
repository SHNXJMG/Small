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
    public class NoticeHZSZ : WebSiteCrawller
    {
        public NoticeHZSZ()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省惠州市建设工程交易中心资格预审";
            this.PlanTime = "9:20,11:20,14:20,17:20";
            this.Description = "自动抓取广东省惠州市建设工程交易中心资格预审";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.ebc.huizhou.gov.cn/index/showList/000000000003/000000000417";
            this.MaxCount = 500;
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
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "toptd1")), true), new TagNameFilter("a")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList[pageList.Count - 1].GetATagValue();
                    pageInt = Convert.ToInt32(temp.Replace("javascript:goPage(", "").Replace(")", ""));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    try
                    {
                        string typeId = ToolHtml.GetHtmlInputValue(html, "typeId");
                        string boardId = ToolHtml.GetHtmlInputValue(html, "boardId");
                        string totalRows = ToolHtml.GetHtmlInputValue(html, "totalRows");
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                            new string[]{
                        "typeId","boardId","newstitle","sTime","eTime","totalRows","pageNO"
                        
                        },
                            new string[] { typeId, boardId, "", "", "", totalRows, i.ToString() }
                            );
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl,nvc, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "lefttable")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount -1; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty,htmlTxt=string.Empty;

                        TableRow tr = table.Rows[j];
                        InfoTitle = tr.Columns[1].ToNodePlainString();
                        string endDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoType = "资格预审";
                        InfoUrl =  tr.Columns[1].GetATagHref();

                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }

                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "context_div")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            htmlTxt = dtlList.ToHtml();
                            InfoCtx = dtlList.ToHtml().ToCtxString().Replace("<?xml:namespace prefix = o ns = \"urn:schemas-microsoft-com:office:office\" />","");
                            PublistTime = InfoCtx.GetDateRegex("yyyy年MM月dd日").Replace("年","-").Replace("月","-").Replace("日","");
                            if(string.IsNullOrEmpty(PublistTime))
                                PublistTime = InfoCtx.GetDateRegex();
                            if (string.IsNullOrEmpty(PublistTime))
                                PublistTime = endDate;
                            NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "惠州市区", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.HuiZhouMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty,htmlTxt);
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
