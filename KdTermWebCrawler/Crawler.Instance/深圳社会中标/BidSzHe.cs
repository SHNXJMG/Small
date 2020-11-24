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

namespace Crawler.Instance
{
    public class BidSzHe : WebSiteCrawller
    {
        public BidSzHe()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "广东省中广核工程有限公司中标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取广东省中广核工程有限公司中标信息";
            this.SiteUrl = "http://bidding.cnpec.com.cn/member/tenderinformation.action?&filter_EQ_isinternational=0&filter_EQ_type=2";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 3;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8, ref cookiestr);
            }
            catch (Exception ex)
            {

                return list;
            }

            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&page.number=" + i.ToString()), Encoding.UTF8);
                    }
                    catch (Exception ex) { continue; }
                }
                Parser parser = new Parser(new Lexer(htl));
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table_title3")));
                if (tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty,
                            code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                            bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty,
                            bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty,
                            otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[4].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[4].SearchFor(typeof(ATag), true)[0] as ATag;
                        beginDate = tr.Columns[6].ToPlainTextString().Trim();
                        endDate = tr.Columns[8].ToPlainTextString().Trim();

                        InfoUrl = "http://bidding.cnpec.com.cn/member/" + aTag.Link.Replace("amp;", "").Trim();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                        if (dtnode.Count > 0)
                        {
                            TableTag tableNode = (TableTag)dtnode[0];
                            HtmlTxt = dtnode.AsHtml();
                            for (int k = 1; k < tableNode.RowCount; k++)
                            {
                                TableRow trow = tableNode.Rows[k];
                                for (int c = 0; c < trow.ColumnCount; c++)
                                {
                                    string tr1 = string.Empty;
                                    tr1 = trow.Columns[c].ToPlainTextString().Trim();
                                    if (tr1.Contains("中标候选人") && k + 1 < tableNode.RowCount)
                                    {
                                        bidUnit = tableNode.Rows[k + 1].Columns[0].ToPlainTextString().Trim();
                                    }
                                    bidCtx += "\r\n" + tr1;
                                }
                            }
                            bidCtx += "\r\n";
                            Regex regCode = new Regex(@"招标编号(：|:)[^\r\n]+\r\n");
                            code = regCode.Match(bidCtx).Value.Replace("招标编号：", "").Trim();
                            Regex regBuidUnit = new Regex(@"(招标人|建设单位)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(bidCtx).Value.Replace("招标人：", "").Replace("建设单位:", "").Trim();
                            Regex regMoney = new Regex(@"中标价(：|:)[^\r\n]+\r\n");
                            bidMoney = regMoney.Match(bidCtx).Value.Replace("中标价：", "").Replace(",", "").Replace("RMB", "").Trim();
                            Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                            if (bidMoney.Contains("EUR"))
                            {
                                bidMoney = "0";
                            }
                            if (!string.IsNullOrEmpty(regBidMoney.Match(bidMoney).Value))
                            {
                                if (bidMoney.Contains("万元") || bidMoney.Contains("EUR") || bidMoney.Contains("万"))
                                {
                                    bidMoney = regBidMoney.Match(bidMoney).Value;
                                }
                                else
                                {
                                    try
                                    {
                                        bidMoney = (decimal.Parse(regBidMoney.Match(bidMoney).Value) / 10000).ToString();
                                        if (decimal.Parse(bidMoney) < decimal.Parse("0.1"))
                                        {
                                            bidMoney = "0";
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        bidMoney = "0";
                                    }
                                }
                            }
                            msgType = "中广核工程有限公司";
                            specType = "建设工程";
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            InfoUrl = InfoUrl.Replace("filter_EQ_isinternational=0", "filter_EQ_isinternational=1"); 
                            prjAddress = "见中标信息";
                            prjName = ToolDb.GetPrjName(prjName);
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate,
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
