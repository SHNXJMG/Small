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
    public class BidLiaoNingJsgcSg : WebSiteCrawller
    {
        public BidLiaoNingJsgcSg()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "辽宁省建设工程信息网中标信息（施工）";
            this.Description = "自动抓取辽宁省建设工程信息网中标信息（施工）";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.lnzb.cn/lnzbtb/ShowInfo/zbjgmore.aspx?categorynum=003003003&QuYu=";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zbjgmore2_Pager")));
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
                    "zbjgmore2$Pager",
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
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "zbjgmore2_DataGrid1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
             bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;
                            TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.GetAttribute("title");
                        area = tr.ToNodePlainString().GetRegexBegEnd("【", "】");
                       
                        InfoUrl = "http://www.lnzb.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "tblInfo")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>","\r\n").ToCtxString();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "_Sheet1")));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag tag = tableNode[0] as TableTag;
                                string ctx = string.Empty;
                                for (int r = 0; r < tag.RowCount; r++)
                                {
                                    for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = tag.Rows[r].Columns[c].ToNodePlainString();
                                        if ((c + 1) % 2 == 0)
                                            ctx += temp.GetReplace(":,：") + "\r\n";
                                        else
                                            ctx += temp.GetReplace(":,：") + "：";
                                    }
                                }
                                buildUnit = ctx.GetBuildRegex();
                                bidUnit = ctx.GetBidRegex();
                                bidMoney = ctx.GetMoneyRegex();
                                code = ctx.GetCodeRegex();
                                prjMgr = ctx.GetMgrRegex();
                                if (string.IsNullOrEmpty(prjMgr))
                                    prjMgr = ctx.GetRegex("项目负责人姓名");
                            }
                            beginDate = bidCtx.GetRegex("发布时间").GetDateRegex("yyyy/MM/dd");
                            msgType = "辽宁省建设厅招标投标管理处";
                            specType = "建设工程";
                            bidType = "施工";
                            BidInfo info = ToolDb.GenBidInfo("辽宁省", "辽宁省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.lnzb.cn/" + a.Link.GetReplace("../,./");
                                        if (Encoding.Default.GetByteCount(link) > 500)
                                            continue;
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
