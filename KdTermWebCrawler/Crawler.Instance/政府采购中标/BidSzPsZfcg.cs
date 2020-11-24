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
    public class BidSzPsZfcg:WebSiteCrawller
    {
        public BidSzPsZfcg()
            : base()
        {
            this.Enabled = false;
            this.Group = "政府采购中标信息";
            this.Title = "深圳市坪山新区政府采购中标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市坪山新区政府采购中标信息";
            this.SiteUrl = "http://ps.szzfcg.cn/portal/topicView.do?method=view&siteId=9&id=2014";
            this.MaxCount = 1500;
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("name", "__ec_pages")), true), new TagNameFilter("option")));

            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    OptionTag selectTag = pageNode[pageNode.Count - 1] as OptionTag;
                    pageInt = int.Parse(selectTag.Value);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "ec_i", "topicChrList_20070702_crd", "topicChrList_20070702_f_a", "topicChrList_20070702_p", "topicChrList_20070702_s_name", "id", "method", "__ec_pages", "topicChrList_20070702_rd", "topicChrList_20070702_f_name", "topicChrList_20070702_f_ldate" }, new string[] { "topicChrList_20070702", "20", string.Empty, i.ToString(), string.Empty, "1660", "view", (i - 1).ToString(), "20", string.Empty, string.Empty });
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html.Replace("tbody", "table")));
                 NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "tableBody")));
                 if (tdNodes != null && tdNodes.Count > 0)
                 {
                     TableTag table = tdNodes[0] as TableTag;
                     for (int j = 0; j < table.RowCount; j++)
                     {
                         string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                         TableRow tr = table.Rows[j];
                         prjName = tr.Columns[1].ToPlainTextString().Trim();
                         beginDate = tr.Columns[3].ToPlainTextString().Trim();
                         InfoUrl = "http://ps.szzfcg.cn" + tr.Columns[1].GetATagHref();
                         string htldtl = string.Empty;
                         try
                         {
                             htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                         }
                         catch { continue; }
                         parser = new Parser(new Lexer(htldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            string[] MoneyRegex = { "中标（成交）金额人民币", "中标金额为", "预中标金额（元）", "成交金额", "中标总金额", "中标（成交）金额", "中标价", "中标金额", "中标金额（元）", "成交总金额（人民币）", "中标金额（包一）", "投标价", "其中标价为", "投标报价", "预中标人", "总投资", "发包价", "投标报价", "价格",  "总价", "报价" };
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString().Replace("\r\n\r\n\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\t\r\n\t\r\n", "\r\n\t").Replace("\r\n\t\r\n\t\r\n", "\r\n\t").Replace("\r\n\t\r\n\t\r\n", "\r\n\t").Replace("\r\n\t\r\n\t\r\n", "\r\n\t");
                            bool isOk = true;
                            bidCtx = System.Web.HttpUtility.HtmlDecode(bidCtx);
                            while (isOk)
                            {
                                string str = bidCtx.GetRegexBegEnd("&#", ";");
                                if (!string.IsNullOrEmpty(str))
                                {
                                    bidCtx = bidCtx.Replace("&#" + str + ";", "");
                                }
                                else
                                    isOk = false;
                            }

                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (!string.IsNullOrEmpty(bidUnit) && bidMoney == "0")
                            {
                                bidMoney = bidCtx.GetMoneyRegex(null, true, "万元");
                            }
                            string ctx = string.Empty;
                            #region 多table匹配
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(htldtl));
                                NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "holder")), true), new TagNameFilter("table")));
                                if (dtList != null && dtList.Count > 0)
                                {
                                    if (dtList.Count > 3)
                                    {
                                        TableTag tab = dtList[2] as TableTag;
                                        if (tab.RowCount > 1)
                                        {
                                            for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                            {
                                                ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                            }
                                        }
                                    }
                                    else if (dtList.Count > 2)
                                    {
                                        TableTag tab = dtList[1] as TableTag;
                                        if (tab.RowCount > 1)
                                        {
                                            for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                            {
                                                ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        TableTag tab = dtList[0] as TableTag;
                                        if (tab.RowCount > 1)
                                        {

                                            for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                            {
                                                string start = System.Web.HttpUtility.HtmlDecode(tab.Rows[0].Columns[d].ToNodePlainString());
                                                string end = System.Web.HttpUtility.HtmlDecode(tab.Rows[1].Columns[d].ToNodePlainString());
                                                ctx += start + "：";
                                                ctx += end + "\r\n";
                                            }
                                        }
                                    }
                                    bidUnit = ctx.GetBidRegex();
                                    bidMoney = ctx.GetMoneyRegex(new string[] { "成交金额" });
                                    if (bidMoney == "" || bidMoney == "0")
                                        bidMoney = ctx.GetMoneyRegex(MoneyRegex);
                                    if (!string.IsNullOrEmpty(bidUnit) && bidMoney == "0")
                                    {
                                        string dtlCtx = string.Empty, unit = string.Empty, money = string.Empty;
                                        TableTag tab = dtList[0] as TableTag;
                                        for (int c = 0; c < tab.RowCount; c++)
                                        {
                                            if ((c + 2) <= tab.RowCount)
                                            {
                                                if (tab.Rows[c].ToNodePlainString().Contains(bidUnit))
                                                {
                                                    for (int d = 0; d < tab.Rows[c].ColumnCount; d++)
                                                    {
                                                        dtlCtx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                        dtlCtx += tab.Rows[c].Columns[d].ToNodePlainString() + "\r\n";
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                        unit = dtlCtx.GetBidRegex();
                                        money = dtlCtx.GetMoneyRegex(MoneyRegex);
                                        if (bidUnit == unit)
                                        {
                                            bidMoney = money;
                                        }
                                    }
                                    if (bidUnit.Contains("无中标") || bidUnit.Contains("没有"))
                                    {
                                        bidUnit = "没有中标商";
                                        bidMoney = "0";
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(htldtl));
                                NodeList dtList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (dtList != null && dtList.Count > 0)
                                {
                                    if (dtList.Count > 3)
                                    {
                                        TableTag tab = dtList[2] as TableTag;
                                        if (tab.RowCount > 1)
                                        {
                                            for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                            {
                                                try
                                                {
                                                    ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                    ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                    else if (dtList.Count > 2)
                                    {
                                        TableTag tab = dtList[1] as TableTag;
                                        if (tab.RowCount > 1)
                                        {
                                            for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                            {
                                                try
                                                {
                                                    ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                    ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                    else if (dtList.Count > 1)
                                    {
                                        TableTag tab = dtList[1] as TableTag;
                                        if (tab.RowCount > 1)
                                        {
                                            for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                            {
                                                try
                                                {
                                                    ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                    ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        TableTag tab = dtList[0] as TableTag;
                                        if (tab.RowCount > 1)
                                        {
                                            for (int d = 0; d < tab.Rows[0].ColumnCount; d++)
                                            {
                                                try
                                                {
                                                    ctx += tab.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                    ctx += tab.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                    bidUnit = ctx.GetBidRegex();
                                    bidMoney = ctx.GetMoneyRegex(MoneyRegex);
                                    if (bidUnit.Contains("无中标") || bidUnit.Contains("没有"))
                                    {
                                        bidUnit = "没有中标商";
                                        bidMoney = "0";
                                    }
                                }
                            }
                            #endregion
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                if (bidCtx.Contains("供应商不足"))
                                {
                                    bidUnit = "没有中标商";
                                    bidMoney = "0";
                                }
                            }
                            if (bidMoney != "0")
                            {
                                try
                                {
                                    decimal mon = decimal.Parse(bidMoney);
                                    if (mon > 100000)
                                    {
                                        bidMoney = bidMoney.GetMoney();
                                    }
                                }
                                catch { }
                            }
                            bidType = prjName.GetInviteBidType();
                            string[] CodeRegex = { "工程编号", "项目编号", "招标编号", "中标编号" };
                            code = bidCtx.GetCodeRegex(CodeRegex);
                            
                            prjName = prjName.Replace("成交", "");
                            if (string.IsNullOrEmpty(code))
                            {
                                code = bidCtx.Replace("）", "kdxx").Replace(")", "kdxx").GetRegexBegEnd("招标编号", "kdxx").Replace("：", "").Replace(":", "");
                            }
                            if (string.IsNullOrEmpty(code))
                            {
                                code = bidCtx.Replace("）", "kdxx").Replace(")", "kdxx").GetRegexBegEnd("项目编号", "kdxx").Replace("：", "").Replace(":", "");
                            }
                            if (string.IsNullOrEmpty(code))
                            {
                                code = bidCtx.Replace("）", "kdxx").Replace(")", "kdxx").GetRegexBegEnd("工程编号", "kdxx").Replace("：", "").Replace(":", "");
                            }
                            if (string.IsNullOrEmpty(code))
                            {
                                code = bidCtx.Replace("）", "kdxx").Replace(")", "kdxx").GetRegexBegEnd("编号", "kdxx").Replace("：", "").Replace(":", "");
                            }
                            if (Encoding.Default.GetByteCount(code) > 50)
                            {
                                code = string.Empty;
                            }
                            if (!string.IsNullOrEmpty(code))
                            {
                                code = code.GetChina();
                            }
                            prjName = prjName.GetBidPrjName();
                            code = code.Replace("（", "").Replace("(", "").Replace("）", "").Replace(")", "");
                            msgType = "深圳市坪山新区政府采购中心";
                            specType = "政府采购";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳政府采购", "坪山新区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
