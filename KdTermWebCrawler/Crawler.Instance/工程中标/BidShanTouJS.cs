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
    public class BidShanTouJS : WebSiteCrawller
    {
        public BidShanTouJS()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省汕头市建设工程中标信息";
            this.Description = "自动抓取广东省汕头建设工程中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "ProjectName,InfoUrl";
            this.SiteUrl = "http://www.stjs.org.cn/zbtb/zhaobiao_ZBgonggao.asp";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch 
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("width", "700")));
            if (nodeList != null && nodeList.Count > 0)
            {
                try
                {
                    string ooooo = Regex.Replace(nodeList[0].ToPlainTextString().Trim().Replace(":", "").Replace("：", "").Replace("&nbsp;", ""), @"[\u4e00-\u9fa5]", "");
                    page = int.Parse(ooooo.Substring(ooooo.IndexOf("/")).Replace("/", "").Trim());
                }
                catch { }
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "?page=" + i.ToString(), Encoding.Default);
                    }
                    catch  { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellPadding", "5")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
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
                             prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        beginDate = tr.Columns[2].ToPlainTextString().Trim();
                        prjName = tr.Columns[1].ToPlainTextString().Replace("&#8226;", "").Trim();
                        ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://www.stjs.org.cn/zbtb/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "");
                        }
                        catch
                        { 
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellPadding", "4")));
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            TableTag tableRow = dtnode.SearchFor(typeof(TableTag), true)[0] as TableTag;
                            for (int row = 0; row < tableRow.RowCount; row++)
                            {
                                TableRow r = tableRow.Rows[row];

                                for (int k = 0; k < r.ColumnCount; k++)
                                {
                                    string st = string.Empty;
                                    string st1 = string.Empty;
                                    st = r.Columns[k].ToPlainTextString().Trim();
                                    if (k + 1 < r.ColumnCount)
                                    {
                                        st1 = r.Columns[k + 1].ToPlainTextString().Trim();
                                    }
                                    bidCtx += st + "：" + st1 + "\r\n";
                                    if (k + 1 <= r.ColumnCount)
                                    {
                                        k++;
                                    }
                                }
                            }
                            code = bidCtx.GetCodeRegex().GetReplace("/");
                            Regex regBuidUnit = new Regex(@"(招标人|建设单位)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(bidCtx).Value.Replace("招标人：", "").Replace("建设单位：", "").Trim();
                            Regex regBidUnit = new Regex(@"中标单位(：|:)[^\r\n]+\r\n");
                            bidUnit = regBidUnit.Match(bidCtx).Value.Replace("中标单位：", "").Replace("/", "").Trim();

                            bidMoney = bidCtx.GetMoneyRegex();

                            string[] prjNames = prjName.Split(':');
                            prjName = prjNames[prjNames.Length - 1];
                            beginDate = beginDate.GetReplace(".", "-");
                            string temp = bidCtx.GetRegex("工程名称", false);
                            if (!string.IsNullOrWhiteSpace(temp))
                                prjName = temp;
                            
                            msgType = "汕头市建设工程交易中心";
                            specType = "建设工程";
                            bidType = ToolHtml.GetInviteTypes(prjName);
                             
                            prjName = ToolDb.GetPrjName(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "汕头市区", "", string.Empty, code, prjName, buildUnit, beginDate,
                               bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                               bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return null;
        }
    }
}
