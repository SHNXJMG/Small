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
    public class NoticeMZJSGC : WebSiteCrawller
    {
        public NoticeMZJSGC()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省梅州市建设工程交易中心资格审查结果";
            this.PlanTime = "9:30,11:30,14:30,17:30";
            this.Description = "自动抓取广东省梅州市建设工程交易中心资格审查结果";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://market.meizhou.gov.cn/deptWebsiteAction.do?action=secondIndex&deptId=1925&issueTypeCode=009011001&issueTypeName=资格审查结果";
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
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "28")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string tem = pageList.AsString().GetRegexBegEnd("，共", "页");
                    pageInt = Convert.ToInt32(tem.Replace("goPage(", "").Replace(")", ""));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "&pageNum=" + i.ToString(), Encoding.Default).GetJsString();
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "95%")));
                if (nodeList != null && nodeList.Count > 1)
                {
                    TableTag table = nodeList[1] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty,htmlTxt=string.Empty;

                        TableRow tr = table.Rows[j];

                        InfoTitle = tr.Columns[0].ToNodePlainString();
                        PublistTime = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        InfoType = "资格审查";
                        InfoUrl = "http://market.meizhou.gov.cn" + tr.Columns[0].GetATagValue("onclick").GetRegexBegEnd("','", "',");

                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { Logger.Error("NoticeMZJSGC"); continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            htmlTxt = dtlList.ToHtml();
                            string ctx = string.Empty;
                            for (int k = 0; k < dtlList.Count; k++)
                            {
                                TableTag tab = dtlList[k] as TableTag;
                                for (int d = 0; d < tab.RowCount; d++)
                                {
                                    for (int c = 0; c < tab.Rows[d].ColumnCount; c++)
                                    {
                                        ctx += tab.Rows[d].Columns[c].ToNodePlainString().Replace("&gt;", "") + "\t";
                                    }
                                    ctx += "\r\n";
                                }
                            }
                            InfoCtx = ctx;
                            NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "梅州市区", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.MeiZhouMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty,htmlTxt);
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
