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
    public class BidShangHaiJz : WebSiteCrawller
    {
        public BidShangHaiJz()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "上海市建筑建材业中标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取上海市建筑建材业中标信息";
            this.SiteUrl = "http://www.ciac.sh.cn/XmZtbbaWeb/Gsqk/GsFbList.aspx";
            this.MaxCount = 400;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("class", "pagestyle")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    ATag aTag = pageNode[pageNode.Count - 1] as ATag;
                    string temp = aTag.LinkText;
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
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "__VIEWSTATEGENERATOR",
                    "__EVENTVALIDATION",
                    "ddlZblx",
                    "txtgsrq",
                    "txtTogsrq",
                    "txttbr",
                    "txtzbhxr"
                    }, new string[]{
                    "gvList",
                    "Page$"+i,
                    viewState,
                    "17E6FEBA",
                    eventValidation,
                    "","","","",
                    ""
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "gvList")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();
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

                        prjName = aTag.LinkText;
                        beginDate = tr.Columns[1].ToPlainTextString().GetDateRegex("yyyy年MM月dd日");
                        InfoUrl = "http://www.ciac.sh.cn/XmZtbbaWeb/Gsqk/GsFb.aspx?zbid=" + aTag.GetAttribute("onclick").GetRegexBegEnd("ShowGs", ",").GetReplace("(");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table01")));
                        if (dtlNode != null && dtlNode.Count > 1)
                        {
                            TableTag dtlTable = dtlNode[0] as TableTag;
                            HtmlTxt = dtlNode[0].ToHtml();//dtlNode.AsHtml();
                            bidCtx = "";
                            for (int r = 0; r < dtlTable.RowCount; r++)
                            {
                                for (int c = 0; c < dtlTable.Rows[r].ColumnCount; c++)
                                {
                                    if ((c + 1) % 2 == 0)
                                        bidCtx += dtlTable.Rows[r].Columns[c].ToNodePlainString() + "\r\n";
                                    else
                                        bidCtx += dtlTable.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：") + "：";
                                }
                            }
                            string ctx = string.Empty;
                            TableTag bidTable = dtlNode[1] as TableTag;
                            for (int r = 0; r < bidTable.Rows[0].ColumnCount; r++)
                            {
                                try
                                {
                                    ctx += bidTable.Rows[0].Columns[r].ToNodePlainString() + "：";
                                    ctx += bidTable.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                }
                                catch { }
                            }
                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            bidUnit = ctx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = ctx.GetRegex("投标人");
                            bidMoney = ctx.GetMoneyRegex();
                            prjMgr = ctx.GetMgrRegex();
                            if (string.IsNullOrEmpty(prjMgr))
                                prjMgr = ctx.GetRegex("项目负责人姓名,总监姓名");
                            msgType = "上海市建筑业管理办公室";
                            specType = bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("上海市", "上海市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
