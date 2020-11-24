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
    public class BidZheJiangZBTB : WebSiteCrawller
    {
        public BidZheJiangZBTB()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "浙江省招标投标网中标公示";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取浙江省招标投标网中标信息";
            this.SiteUrl = "http://www.zjbid.cn/zjwz/template/default/GGInfo.aspx?CategoryNum=001001009";
            this.MaxCount = 200;
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));

            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "MoreInfoListGG_Pager")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("页数：", "当前");
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
                         "__VIEWSTATE","__EVENTTARGET","__EVENTARGUMENT"
                    }, new string[] {
                        viewState ,"MoreInfoListGG$Pager",i.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "MoreInfoListGG_DataGrid1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
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
                        string xian = aTag.LinkText.GetRegexBegEnd("【", "】");
                        prjName = aTag.GetAttribute("title");
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.zjbid.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtnode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "infodetail")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</p>,</br>", "\r\n").GetReplace("<br />", "\r\n").ToCtxString();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("中标人");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("预中标单位（第一名）");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                try
                                {
                                    parser = new Parser(new Lexer(htmldtl));
                                    NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "_Sheet1")));
                                    if (dtlNode != null && dtlNode.Count > 0)
                                    {
                                        TableTag dtlTable = dtlNode[0] as TableTag;
                                        HtmlTxt = dtlTable.ToHtml();
                                        string ctx = "";
                                        for (int r = 1; r < dtlTable.RowCount; r++)
                                        {
                                            for (int c = 0; c < dtlTable.Rows[r].ColumnCount; c++)
                                            {
                                                string temp = dtlTable.Rows[r].Columns[c].ToHtml().GetReplace("<br>,<br/>", "\r\n").ToCtxString();
                                                if (!temp.Contains("\r\n"))
                                                    temp = dtlTable.Rows[r].Columns[c].ToNodePlainString();
                                                if (!IsTable(dtlTable.Rows[r].ToHtml()))
                                                {
                                                    if ((c + 1) % 2 == 0)
                                                        ctx += temp + "\r\n";
                                                    else
                                                        ctx += temp.GetReplace(":,：") + "：";
                                                }
                                                else
                                                {
                                                    ctx += GetTableBid(dtlTable.Rows[r].ToHtml());
                                                }
                                            }
                                        }
                                        ctx = ctx.GetReplace("：\r\n", "：");
                                        code = ctx.GetCodeRegex();
                                        if (string.IsNullOrWhiteSpace(code))
                                            code = ctx.GetRegex("工程编码");
                                        buildUnit = ctx.GetBuildRegex();
                                        if (string.IsNullOrEmpty(buildUnit))
                                            buildUnit = ctx.GetRegex("建设单位");
                                        if (string.IsNullOrWhiteSpace(buildUnit))
                                            buildUnit = ctx.GetRegex("采购人名称");
                                    }
                                }
                                catch { }
                            }
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                try
                                {
                                    parser = new Parser(new Lexer(htmldtl));
                                    NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "_Sheet1_6_1")));
                                    if (dtlNode != null && dtlNode.Count > 0)
                                    {
                                        TableTag dtlTable = dtlNode[0] as TableTag;
                                        string Html = dtlTable.ToHtml();
                                        string bidCtxt = string.Empty;
                                        for (int c = 0; c < dtlTable.Rows[0].ColumnCount; c++)
                                        {
                                            bidCtxt += dtlTable.Rows[1].Columns[c].ToNodePlainString() + "：";
                                            bidCtxt += dtlTable.Rows[2].Columns[c].ToNodePlainString() + "\r\n";
                                        }
                                       
                                        bidCtxt = bidCtxt.GetReplace("：\r\n", "：");
                                        bidCtxt = bidCtxt.Replace("%", "");
                                        bidUnit = bidCtxt.GetBidRegex(); 
                                        if (string.IsNullOrEmpty(bidUnit))
                                            bidUnit = bidCtxt.GetRegex("拟中标单位");
                                        bidMoney = bidCtxt.GetMoneyRegex();
                                        if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                            bidMoney = bidCtxt.GetRegex("中标价:").GetMoney();
                                        prjMgr = bidCtxt.GetMgrRegex();
                                        if (string.IsNullOrWhiteSpace(prjMgr))
                                            prjMgr = bidCtxt.GetRegex("项目经理");
                                    }
                                }
                                catch { }
                            }

                            if (string.IsNullOrWhiteSpace(bidMoney))
                                bidMoney = bidCtx.GetMoneyRegex();
                            if (string.IsNullOrWhiteSpace(bidMoney))
                            {
                                try
                                {

                                    parser = new Parser(new Lexer(htmldtl));
                                    NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "_Sheet1_13_0")));
                                    if (dtlNode != null && dtlNode.Count > 0)
                                    {
                                        TableTag dtlTable = dtlNode[0] as TableTag;
                                        string Html = dtlTable.ToHtml();
                                        string bidCtxt = string.Empty;
                                        for (int c = 0; c < dtlTable.Rows[0].ColumnCount; c++)
                                        {
                                            bidCtxt += dtlTable.Rows[1].Columns[c].ToNodePlainString() + "：";
                                            bidCtxt += dtlTable.Rows[2].Columns[c].ToNodePlainString() + "\r\n";
                                        }
                                        if (string.IsNullOrWhiteSpace(bidUnit))
                                            bidUnit = bidCtxt.GetRegex("中标供应商");
                                        if (string.IsNullOrWhiteSpace(bidMoney))
                                            bidMoney = bidCtxt.GetRegex("价格（元）");

                                    }
                                }
                                catch { }
                            }
                            if(string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList node = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "_Sheet1_13_0")));
                                if(node!=null&&node.Count>0)
                                {
                                    TableTag bidTable = node[0] as TableTag;
                                    string ctx = string.Empty;
                                    if (bidTable.RowCount >= 3)
                                    {
                                        for (int r = 0; r < bidTable.Rows[1].ColumnCount; r++)
                                        {
                                            try
                                            {
                                                ctx += bidTable.Rows[1].Columns[r].ToNodePlainString() + "：";
                                                ctx += bidTable.Rows[2].Columns[r].ToNodePlainString() + "\r\n";
                                            }
                                            catch { }
                                        }
                                        bidUnit = ctx.GetBidRegex();
                                        bidMoney = ctx.GetMoneyRegex(null, false, "万元");
                                    }
                                }
                            }

                            if (string.IsNullOrWhiteSpace(buildUnit))
                                buildUnit = bidCtx.GetBuildRegex();
                            if (string.IsNullOrWhiteSpace(buildUnit))
                                buildUnit = bidCtx.GetRegex("招标人");
                            if (string.IsNullOrWhiteSpace(buildUnit))
                                buildUnit = bidCtx.GetRegex("采购人名称");
                            if (string.IsNullOrWhiteSpace(code))
                                code = bidCtx.GetCodeRegex().GetCodeDel();
                            if (!string.IsNullOrWhiteSpace(code))
                                if (code[code.Length - 1] != '号')
                                    code = "";
                            if (string.IsNullOrWhiteSpace(code))
                                code = bidCtx.GetRegex("采购项目编号");

                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("开标"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("开标"));
                            try
                            {
                                if (Convert.ToDecimal(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }

                            msgType = "浙江省招标投标办公室";
                            specType = "建设工程";
                            bidType = ToolHtml.GetInviteTypes(prjName);

                            BidInfo info = ToolDb.GenBidInfo("浙江省", "浙江省及地市", xian, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
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
