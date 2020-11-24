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
    public class BidJiangSuJsgc : WebSiteCrawller
    {
        public BidJiangSuJsgc()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "江苏省建设工程招标投标网中标信息";
            this.Description = "自动抓取江苏省建设工程招标投标网中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.jszb.com.cn/jszb/YW_info/ZhongBiaoGS/MoreInfo_ZBGS.aspx?categoryNum=012";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
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
                        string prjName = string.Empty,
                              buildUnit = string.Empty, bidUnit = string.Empty,
                              bidMoney = string.Empty, code = string.Empty,
                              bidDate = string.Empty,
                              beginDate = string.Empty,
                              endDate = string.Empty, bidType = string.Empty,
                              specType = string.Empty, InfoUrl = string.Empty,
                              msgType = string.Empty, bidCtx = string.Empty,
                              prjAddress = string.Empty, remark = string.Empty,
                              prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.GetAttribute("title").GetReplace(";");
                        area = prjName.GetReplace("[", "【").GetReplace("]", "】").GetRegexBegEnd("【", "】");
                        if (!string.IsNullOrEmpty(area))
                            prjName = prjName.GetReplace("[" + area + "]");
                        else
                            prjName = prjName.GetReplace("[,]");
                        beginDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
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
                            HtmlTxt = dtlNode.AsHtml();
                            TableTag tag = dtlNode[0] as TableTag;
                            for (int r = 0; r < tag.RowCount; r++)
                            {
                                for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                {
                                    string temp = tag.Rows[r].Columns[c].ToNodePlainString();
                                    if ((c + 1) % 2 == 0)
                                        bidCtx += temp + "\r\n";
                                    else
                                        bidCtx += temp.GetReplace(":,：") + "：";
                                }
                            }
                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("第一中标候选单位为,第一名");
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                bidMoney = bidCtx.GetMoneyRegex(null, true);
                            prjMgr = bidCtx.GetMgrRegex();
                            msgType = "江苏省建设工程招标投标办公室";
                            specType = "建设工程";
                            bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("江苏省", "江苏省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
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
                                            link = "http://www.jszb.com.cn/" + a.Link;
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
