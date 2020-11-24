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
    public class BidShanXiJsgc : WebSiteCrawller
    {
        public BidShanXiJsgc()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "陕西省建设工程招投标管理办公室中标信息";
            this.Description = "自动抓取陕西省建设工程招投标管理办公室中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 200;
            this.SiteUrl = "http://www.sxszbb.com/sxztb/jyxx/001002/MoreInfo.aspx?CategoryNum=001002";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
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
                        "__EVENTVALIDATION",
                        "MoreInfoList1$txtTitle"
                        },
                        new string[] { 
                        viewState,
                        "MoreInfoList1$Pager",
                        i.ToString(),
                        eventValidation,
                        ""
                        }
                        );
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
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
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty,area=string.Empty;
                         TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        string temp = tr.Columns[1].ToNodePlainString();
                        if (temp.Contains("[") && temp.Contains("]"))
                            area = temp.Substring(temp.IndexOf("["), temp.IndexOf("]") - temp.IndexOf("[")).GetReplace("[,]");
                        InfoUrl = "http://www.sxszbb.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();

                            buildUnit = bidCtx.GetBuildRegex(); 
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            bidMoney = bidCtx.GetMoneyRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("第一名");
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag tag = tableNode[0] as TableTag;
                                    bool isBreak = false,rBreak=false;
                                    for (int r = 0; r < tag.RowCount; r++)
                                    { 
                                        for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                        {
                                            string strTemp = tag.Rows[r].Columns[c].ToNodePlainString();
                                            if (strTemp.Contains("评标结果"))
                                            {
                                                isBreak = true;
                                                break;
                                            }
                                            if (isBreak)
                                            {
                                                rBreak = true;
                                                try
                                                {
                                                    ctx += tag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：") + "：";
                                                    ctx += tag.Rows[r + 1].Columns[c].ToNodePlainString().GetReplace(":,：") + "\r\n";
                                                }
                                                catch { }
                                            }
                                            else
                                            {
                                                if ((c + 1) % 2 == 0)
                                                    ctx += strTemp.GetReplace(":,：") + "\r\n";
                                                else
                                                    ctx += strTemp.GetReplace(":,：") + "：";
                                            }
                                        }
                                        if (rBreak) break;
                                    }
                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                        bidMoney = ctx.GetMoneyRegex();
                                    if (string.IsNullOrWhiteSpace(prjMgr))
                                        prjMgr = ctx.GetMgrRegex();
                                    if (string.IsNullOrWhiteSpace(buildUnit))
                                        buildUnit = ctx.GetBuildRegex();
                                    if (string.IsNullOrWhiteSpace(code))
                                        code = ctx.GetCodeRegex().GetCodeDel();
                                }
                            }
                            
                            
                            if (buildUnit.Contains("单位章"))
                                buildUnit = string.Empty;
                            if (buildUnit.Contains("联系人"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系人"));
                            if (prjMgr.Contains("中标"))
                                prjMgr = string.Empty;
                            specType = bidType= "建设工程";
                            msgType = "陕西省建设工程招标投标管理办公室";
                            BidInfo info = ToolDb.GenBidInfo("陕西省", "陕西省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
