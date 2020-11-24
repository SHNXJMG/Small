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
    public class BidBeiJingSg : WebSiteCrawller
    {
        public BidBeiJingSg()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "北京市建设工程发包承包交易中心中标信息(施工)";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取北京市建设工程发包承包交易中心中标信息(施工)";
            this.SiteUrl = "http://www.bcactc.com/home/gcxx/now_sgzbgs.aspx";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "lblPageCount")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString();
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
                    "__LASTFOCUS",
                    "__VIEWSTATE",
                    "__VIEWSTATEGENERATOR",
                    "__EVENTVALIDATION",
                    "gcbh_Text_Box",
                    "gcmc_TextBox",
                    "num_TextBox",
                    "ImageButton3.x",
                    "ImageButton3.y"
                    }, new string[]{
                    "","","",
                    viewState,
                    "B0108473",
                    eventValidation,
                    "","","",
                    "5","12"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "DataGrid1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[2].GetATag();
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
                        code = tr.Columns[1].ToNodePlainString();
                        prjName = aTag.LinkText.GetReplace(" ");
                        beginDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.bcactc.com/home/gcxx/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "hei_text")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            TableTag dtlTable = dtlNode[0] as TableTag;
                            HtmlTxt = dtlTable.ToHtml();
                            for (int r = 0; r < dtlTable.RowCount; r++)
                            {
                                for (int c = 0; c < dtlTable.Rows[r].ColumnCount; c++)
                                {
                                    string temp = dtlTable.Rows[r].Columns[c].ToHtml().GetReplace("<br>,<br/>", "\r\n").ToCtxString();
                                    if (!temp.Contains("\r\n"))
                                        temp = dtlTable.Rows[r].Columns[c].ToNodePlainString();
                                    if (!IsTable(dtlTable.Rows[r].ToHtml()))
                                    {
                                        if ((c + 1) % 2 == 0)
                                            bidCtx += temp + "\r\n";
                                        else
                                            bidCtx += temp.GetReplace(":,：") + "：";
                                    }
                                    else
                                    {
                                        bidCtx += GetTableBid(dtlTable.Rows[r].ToHtml());
                                    }
                                }
                            }
                            bidCtx = bidCtx.GetReplace("：\r\n", "：");
                            if (code.Contains(".."))
                                code = bidCtx.GetCodeRegex();

                            buildUnit = bidCtx.GetBuildRegex();
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = bidCtx.GetRegex("建设单位名称");

                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("中标侯选人");
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();

                            msgType = "北京市建设工程发包承包交易中心";
                            specType = "建设工程";
                            bidType = "施工";
                            BidInfo info = ToolDb.GenBidInfo("北京市", "北京市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.bcactc.com/" + a.Link;
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

        private bool IsTable(string htmld)
        {
            Parser p = new Parser(new Lexer(htmld));
            NodeList node = p.ExtractAllNodesThatMatch(new TagNameFilter("table"));
            return node != null && node.Count > 0;
        }

        private string GetTableBid(string htmld)
        {
            string ctx = string.Empty;
            Parser p = new Parser(new Lexer(htmld));
            NodeList node = p.ExtractAllNodesThatMatch(new TagNameFilter("table"));
            if (node != null && node.Count > 0)
            {
                TableTag table = node[0] as TableTag;
                for (int r = 0; r < table.Rows[0].ColumnCount; r++)
                {
                    ctx += table.Rows[0].Columns[r].ToNodePlainString().GetReplace(":,：") + "：";
                    ctx += table.Rows[1].Columns[r].ToNodePlainString().GetReplace(":,：") + "\r\n";
                }
            }
            return ctx;
        }
    }
}
