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
    public class NoticeJiangSuJsgc:WebSiteCrawller
    {
        public NoticeJiangSuJsgc()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "江苏省建设工程招标投标网通知公示";
            this.Description = "自动抓取江苏省建设工程招标投标网通知公示";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.jszb.com.cn/jszb/YW_info/ZuiGaoXJ/MoreInfo_ZGXJ.aspx?categoryNum=012";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "MoreInfoList1_Pager")));
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
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    string __CSRFTOKEN = ToolHtml.GetHtmlInputValue(html, "__CSRFTOKEN");
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__CSRFTOKEN",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__LASTFOCUS",
                    "__VIEWSTATE",
                    "__VIEWSTATEGENERATOR",
                    "__EVENTVALIDATION",
                    "MoreInfoList1$txtProjectName",
                    "MoreInfoList1$txtBiaoDuanName",
                    "MoreInfoList1$txtBiaoDuanNo",
                    "MoreInfoList1$txtJSDW",
                    "MoreInfoList1$StartDate",
                    "MoreInfoList1$EndDate",
                    "MoreInfoList1$jpdDi",
                    "MoreInfoList1$jpdXian"
                    }, new string[]{
                    __CSRFTOKEN,
                    "MoreInfoList1$Pager",
                    i.ToString(),
                    "",
                    viewState,
                    "76D0A3AC",
                    eventValidation,
                    "","","","","","",
                    "-1","-1"
                    });
                    try
                    {
                        cookiestr = cookiestr.GetReplace("path=/; HttpOnly").Replace(",", "");
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookiestr);
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
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty,area=string.Empty,bgType=string.Empty;
                        InfoType = "最高限价公示";
                           TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        InfoTitle = aTag.GetAttribute("title").GetReplace(";");
                        area = InfoTitle.GetReplace("[", "【").GetReplace("]", "】").GetRegexBegEnd("【", "】");
                        if (!string.IsNullOrEmpty(area))
                            InfoTitle = InfoTitle.GetReplace("[" + area + "]");
                        bgType = tr.Columns[2].ToNodePlainString();
                        PublistTime = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.jszb.com.cn/jszb/YW_info/" + aTag.GetAttribute("onclick").Replace("(", "（").GetRegexBegEnd("（", ",").GetReplace("\",../,./");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            htmlTxt = dtlNode.AsHtml();
                            TableTag tag = dtlNode[0] as TableTag;
                            for (int r = 0; r < tag.RowCount; r++)
                            {
                                for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                {
                                    string temp = tag.Rows[r].Columns[c].ToNodePlainString();
                                    if ((c + 1) % 2 == 0)
                                        InfoCtx += temp + "\r\n";
                                    else
                                        InfoCtx += temp.GetReplace(":,：") + "：";
                                }
                            }
                            prjCode = InfoCtx.GetCodeRegex();
                            buildUnit = InfoCtx.GetBuildRegex();
                            NoticeInfo info = ToolDb.GenNoticeInfo("江苏省", "江苏省及地市", area, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "江苏省建设工程招标投标办公室", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, "建设工程", bgType, htmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(htmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k] as ATag;
                                    if (a.IsAtagAttach() || a.Link.ToLower().Contains("retrieveimagedata.aspx"))
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://www.jszb.com.cn/jszb/YW_info/ZuiGaoXJ/" + a.Link;
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
