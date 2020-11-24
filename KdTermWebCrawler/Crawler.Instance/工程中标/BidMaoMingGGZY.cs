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
    public class BidMaoMingGGZY : WebSiteCrawller
    {
        public BidMaoMingGGZY()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省茂名市公共资源交易网中标信息";
            this.Description = "自动抓取广东省茂名市公共资源交易网中标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.SiteUrl = "http://jyzx.maoming.gov.cn/mmzbtb/jyxx/033001/033001001/033001001003/033001001003001/";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8).Replace("&nbsp;", "");
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("noWrap", "true")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().GetRegexBegEnd("总页数：", "当");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            string cookiestr = string.Empty;

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?Paging=" + i);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("valign", "top")));
                if (sNode != null && sNode.Count > 0)
                {
                    TableTag table = sNode[0] as TableTag;
                    for (int t = 0; t < table.RowCount - 1; t++)
                    {
                        TableRow tr = table.Rows[t];
                        ATag aTag = tr.GetATag();
                        if (aTag == null) continue;
                        string prjName = string.Empty,
                            buildUnit = string.Empty, bidUnit = string.Empty,
                            bidMoney = string.Empty, code = string.Empty,
                            bidDate = string.Empty,
                            beginDate = string.Empty,
                            endDate = string.Empty, bidType = string.Empty,
                            specType = string.Empty, InfoUrl = string.Empty,
                            msgType = string.Empty, bidCtx = string.Empty,
                            prjAddress = string.Empty, remark = string.Empty,
                            prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;


                        prjName = aTag.GetAttribute("title");
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://jyzx.maoming.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</p>", "\r\n").ToCtxString();

                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            code = bidCtx.GetCodeRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag tab = tableNode[0] as TableTag;
                                    for (int r = 0; r < tab.RowCount; r++)
                                    {
                                        for (int c = 0; c < tab.Rows[r].ColumnCount; c++)
                                        {
                                            string temp = tab.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                            if (tab.Rows[r].Columns[c].ToNodePlainString().Contains("中标人"))
                                            {
                                                if (((c + 1) % 2) == 0)
                                                    ctx += temp + "：";
                                                else
                                                    ctx += temp + "\r\n";
                                            }
                                            else
                                            {
                                                if (((c + 1) % 2) == 0)
                                                    ctx += temp + "\r\n";
                                                else
                                                    ctx += temp + "：";
                                            }
                                        }
                                    }
                                    if (string.IsNullOrEmpty(buildUnit))
                                        buildUnit = ctx.GetBuildRegex();
                                    if (string.IsNullOrEmpty(code))
                                        code = ctx.GetCodeRegex();
                                    if (string.IsNullOrEmpty(prjAddress))
                                        prjAddress = ctx.GetAddressRegex();
                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrEmpty(bidUnit) || bidUnit.Length <= 5)
                                        bidUnit = ctx.GetRegex("第一中标候选人");
                                    if (bidMoney == "0")
                                        bidMoney = ctx.GetMoneyRegex();
                                    if (string.IsNullOrEmpty(prjMgr))
                                        prjMgr = ctx.GetMgrRegex();
                                    if (string.IsNullOrEmpty(prjMgr))
                                        prjMgr = ctx.GetRegex("项目（经理）负责人");
                                    if (prjMgr.Contains("/"))
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                                    if (prjMgr.Contains("粤"))
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("粤"));
                                    if (prjMgr.Contains("证书"))
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("证书"));
                                }
                            }

                            if (string.IsNullOrEmpty(bidUnit) || bidMoney == "0")
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag tab = tableNode[0] as TableTag;
                                    if (tab.RowCount >= 2)
                                    {
                                        for (int c = 1; c < tab.Rows[0].ColumnCount; c++)
                                        {
                                            try
                                            {
                                                TableColumn td = tab.Rows[0].Columns[0];
                                                if (td.ToNodePlainString().Contains("中标候选人"))
                                                {
                                                    ctx += tab.Rows[0].Columns[c + 1].ToNodePlainString().GetReplace(":,：") + "：";
                                                    ctx += tab.Rows[1].Columns[c].ToNodePlainString().GetReplace(":,：") + "\r\n";
                                                }
                                                else
                                                {
                                                    ctx += tab.Rows[0].Columns[c].ToNodePlainString().GetReplace(":,：") + "：";
                                                    ctx += tab.Rows[1].Columns[c].ToNodePlainString().GetReplace(":,：") + "\r\n";
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                    if (string.IsNullOrEmpty(buildUnit))
                                        buildUnit = ctx.GetBuildRegex();
                                    if (string.IsNullOrEmpty(code))
                                        code = ctx.GetCodeRegex();
                                    if (string.IsNullOrEmpty(prjAddress))
                                        prjAddress = ctx.GetAddressRegex();
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("单位名称");
                                    if (bidMoney == "0")
                                        bidMoney = ctx.GetMoneyRegex();
                                    if (string.IsNullOrEmpty(prjMgr))
                                        prjMgr = ctx.GetMgrRegex();
                                    if (string.IsNullOrEmpty(prjMgr))
                                        prjMgr = ctx.GetRegex("项目（经理）负责人");
                                    if (prjMgr.Contains("/"))
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                                    if (prjMgr.Contains("粤"))
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("粤"));
                                    if (prjMgr.Contains("证书"))
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("证书"));
                                }
                            }
                            prjMgr = prjMgr.GetReplace("项目技术负责人");

                            code = "";
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            msgType = "茂名市公共资源交易网";
                            specType = "建设工程";
                            bidType = prjName.GetInviteBidType();
                            BidInfo info = ToolDb.GenBidInfo("广东省", "茂名市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                        if (a.Link.Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://jyzx.maoming.gov.cn/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, a.Link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
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
