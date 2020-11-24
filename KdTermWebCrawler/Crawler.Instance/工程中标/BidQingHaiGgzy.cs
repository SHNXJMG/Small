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
    public class BidQingHaiGgzy : WebSiteCrawller
    {
        public BidQingHaiGgzy()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "青海省公共资源交易平台中标信息";
            this.Description = "自动抓取青海省公共资源交易平台中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 200;
            this.SiteUrl = "http://qhzbtb.qhwszwdt.gov.cn/qhweb/jyxx/005001/005001004/MoreInfo.aspx?CategoryNum=005001004";
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
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("nowrap", "true")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("总页数：", "当");
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
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                        new string[] { 
                        "__VIEWSTATE",
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__EVENTVALIDATION"
                        },
                        new string[] { 
                        viewState,
                        "MoreInfoList1$Pager",
                        i.ToString(),
                        eventValidation
                        }
                        );
                    try
                    {
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
                        try
                        {
                            prjName = aTag.GetAttribute("title");
                        }
                        catch
                        {
                            continue;
                        }
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();

                        InfoUrl = "http://qhzbtb.qhwszwdt.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList dtlBidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (dtlBidNode != null && dtlBidNode.Count > 0)
                            {
                                TableTag bidTable = dtlBidNode[0] as TableTag;
                                string ctx = string.Empty;
                                for (int r = 0; r < bidTable.RowCount; r++)
                                {
                                    for (int c = 0; c < bidTable.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = bidTable.Rows[r].Columns[c].ToNodePlainString();
                                        if (string.IsNullOrEmpty(temp)) continue;
                                      
                                        if ((c + 1) % 2 == 0)
                                            ctx += temp + "\r\n";
                                        else
                                            ctx += temp + "：";
                                    }
                                }
                                prjAddress = ctx.GetAddressRegex();
                                buildUnit = ctx.GetBuildRegex();
                                bidUnit = ctx.GetBidRegex().GetReplace("第一名,第一");
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = ctx.GetRegex("第一名");
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = ctx.GetRegex("第一");
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = ctx.GetRegex("1");
                                bidMoney = ctx.GetMoneyRegex();
                                prjMgr = ctx.GetMgrRegex();
                                if (string.IsNullOrEmpty(prjMgr))
                                    prjMgr = ctx.GetMgrRegex(new string[] { "建造师姓名" });
                                code = ctx.GetCodeRegex();

                                if (string.IsNullOrEmpty(bidUnit) || bidUnit.Contains("中标价"))
                                {
                                    ctx = string.Empty;
                                    for (int r = 0; r < bidTable.RowCount; r++)
                                    {
                                        string rowName = bidTable.Rows[r].ToNodePlainString();
                                        for (int c = 0; c < bidTable.Rows[r].ColumnCount; c++)
                                        {
                                            if (rowName.Contains("中标人") || rowName.Contains("中标价"))
                                            {
                                                try
                                                {
                                                    ctx += bidTable.Rows[r].Columns[c].ToNodePlainString() + "：";
                                                    ctx += bidTable.Rows[r + 1].Columns[c].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                            else
                                            {
                                                string temp = bidTable.Rows[r].Columns[c].ToNodePlainString();
                                         
                                                if ((c + 1) % 2 == 0)
                                                    ctx += temp + "\r\n";
                                                else
                                                    ctx += temp + "：";
                                            }
                                        }
                                    }
                                    if (string.IsNullOrEmpty(buildUnit))
                                        buildUnit = ctx.GetBuildRegex();
                                    bidUnit = ctx.GetBidRegex().GetReplace("第一名,第一"); ;
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("第一名");
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("第一");
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("1");
                                    if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                        bidMoney = ctx.GetMoneyRegex();
                                    if (string.IsNullOrEmpty(prjMgr) || prjMgr.IsNumber())
                                        prjMgr = ctx.GetMgrRegex();
                                    if (string.IsNullOrEmpty(prjMgr) || prjMgr.IsNumber())
                                        prjMgr = ctx.GetMgrRegex(new string[] { "建造师姓名" });
                                    if (string.IsNullOrEmpty(code))
                                        code = ctx.GetCodeRegex();
                                }
                            }
                            else
                            {
                                prjAddress = bidCtx.GetAddressRegex();
                                buildUnit = bidCtx.GetBuildRegex();
                                bidUnit = bidCtx.GetBidRegex().GetReplace("第一名,第一"); ;
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = bidCtx.GetRegex("第一中标排序人");
                                bidMoney = bidCtx.GetMoneyRegex();
                                prjMgr = bidCtx.GetMgrRegex();
                                if (string.IsNullOrEmpty(prjMgr))
                                    prjMgr = bidCtx.GetRegex("注册监理工程师");
                                code = bidCtx.GetCodeRegex();
                            }
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));

                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("联系"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("联系"));
                            if (bidUnit.Contains("地址"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("地址"));
                            buildUnit = buildUnit.Replace(" ", "");
                            bidUnit = bidUnit.GetReplace("一标段");
                            if (bidUnit.IsNumber() || bidUnit.Contains("中标") || bidUnit.Contains("投标")||bidUnit.Contains("合格"))
                                bidUnit = string.Empty;
                            code = code.Replace(" ", "");
                            try
                            {
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            prjMgr = prjMgr.Replace(" ", "");
                            if (prjMgr.IsNumber()||prjMgr.Contains("注册")||prjMgr.Contains("中标")||prjMgr.Contains("证书"))
                                prjMgr = string.Empty; 
                            bidType = "建设工程";
                            specType = "政府采购";
                            msgType = "青海省公共资源交易监督管理局";
                            BidInfo info = ToolDb.GenBidInfo("青海省", "青海省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://qhzbtb.qhwszwdt.gov.cn/" + a.Link.GetReplace("../,./");
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
