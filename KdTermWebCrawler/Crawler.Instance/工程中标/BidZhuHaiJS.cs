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
    public class BidZhuHaiJS : WebSiteCrawller
    {
        public BidZhuHaiJS()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省珠海市建设工程中标信息";
            this.Description = "自动抓取广东省珠海市建设工程中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.SiteUrl = "http://www.cpinfo.com.cn/index/showList/000000000003/000000000417";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default, ref cookiestr);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "scott")), true), new TagNameFilter("a")));
            Regex regexPage = new Regex(@"共\d+页");
            try
            {
                Regex numpage = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                ATag link = (ATag)nodeList[nodeList.Count - 1];
                page = Convert.ToInt32(numpage.Match(link.Link).Value.Trim());
            }
            catch (Exception)
            { }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(htl);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "newtitle",
                        "totalRows",
                        "pageNO"  
                    }, new string[]{
                        string.Empty,
                        "0",
                        i.ToString()
                    });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default, ref cookiestr).Replace("<th", "<td").Replace("</th>", "</td>").Replace("&nbsp;", "");
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "cnewslist")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
                    for (int j = 1; j < table.RowCount - 2; j++)
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
                        prjName = tr.Columns[0].ToPlainTextString().Trim();
                        beginDate = tr.Columns[1].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[0].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("<th", "<td").Replace("</th>", "</td>").Replace("</TH>", "</td>").Replace("<TH", "<td").Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            Logger.Error("BidZhuHaiJS"); 
                            continue;
                        }
                        bool htmlBool = true;
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "maintable")));
                        if (dtnode.Count <= 0)
                        {
                            parserdetail = new Parser(new Lexer(htmldetail));
                            dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "printTb")), true), new TagNameFilter("table")));
                        }
                        if (dtnode.Count <= 0)
                        {
                            parserdetail = new Parser(new Lexer(htmldetail));
                            dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "printTb")), true), new TagNameFilter("p")));
                            htmlBool = false;
                        }
                        if (dtnode.Count <= 0)
                        {
                            parserdetail = new Parser(new Lexer(htmldetail));
                            dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "fwinProjectForHand"), new TagNameFilter("div")));
                        }
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            if (htmlBool)
                            {
                                TableTag tabletwo = (TableTag)dtnode[0];
                                for (int row = 0; row < tabletwo.RowCount; row++)
                                {
                                    TableRow r = tabletwo.Rows[row];
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
                            }
                            else
                            {
                                for (int k = 0; k < dtnode.Count; k++)
                                {
                                    bidCtx += dtnode[k].ToPlainTextString() + "\r\n";
                                }
                            }
                            bidCtx = bidCtx.Replace("(单价)", "").Trim();
                            Regex regendDate = new Regex(@"(公告发布时间|公示日期)：[^\r\n]+[\r\n]{1}");
                            endDate = regendDate.Match(bidCtx).Value.Replace("公告发布时间：", "").Replace("公示日期：", "").Trim();
                            string date = endDate.Replace(" ", "").Trim();
                            Regex regDate = new Regex(@"至\d{4}-\d{1,2}-\d{1,2}");
                            endDate = regDate.Match(date).Value.Replace("至", "").Trim();
                            if (endDate == "")
                            {
                                Regex regDateT = new Regex(@"--\d{4}-\d{1,2}-\d{1,2}");
                                endDate = regDateT.Match(date).Value.Replace("--", "").Trim();
                            }
                            if (endDate == "")
                            {
                                Regex regDateT = new Regex(@"至\d{4}年\d{1,2}月\d{1,2}日");
                                endDate = regDateT.Match(date).Value.Replace("--", "").Trim();
                            }
                            if (endDate == "")
                            {
                                Regex regDateT = new Regex(@"--\d{4}年\d{1,2}月\d{1,2}日");
                                endDate = regDateT.Match(date).Value.Replace("--", "").Trim();
                            }
                            if (endDate == "")
                            {
                                Regex regDateT = new Regex(@"-\d{4}年\d{1,2}月\d{1,2}日");
                                endDate = regDateT.Match(date).Value.Replace("-", "").Trim();
                            }
                            if (endDate == "")
                            {
                                endDate = string.Empty;
                            }
                            Regex regBidUnit = new Regex(@"(第一中标候选人|中标人|中标单位)(：|:)[^\r\n]+\r\n");
                            bidUnit = regBidUnit.Match(bidCtx).Value.Replace("第一中标候选人", "").Replace("中标人：", "").Replace("中标单位：", "").Replace("：", "").Replace(":","").Trim();
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                Regex regBidUnit1 = new Regex(@"(第一中标候选人|中标人|中标单位)[^\r\n]+\r\n");
                                bidUnit = regBidUnit1.Match(bidCtx).Value.Replace("第一中标候选人", "").Replace("中标人", "").Replace("中标单位", "").Trim();
                            }
                            Regex regbidMoney = new Regex(@"中标价(：|:)[^\r\n]+\r\n");
                            bidMoney = regbidMoney.Match(bidCtx).Value.Trim();
                            if (string.IsNullOrEmpty(bidMoney))
                            {
                                Regex regbidMoney1 = new Regex(@"中标价[^\r\n]+\r\n");
                                bidMoney = regbidMoney1.Match(bidCtx).Value.Trim();
                            }
                            Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                            if (bidMoney.Contains(","))
                            {
                                bidMoney = bidMoney.Replace(",", "").Trim();
                            }
                            if (bidMoney.Contains("万"))
                            {
                                bidMoney = bidMoney.Remove(bidMoney.IndexOf("万")).Trim();
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
                            Regex regprjMgr = new Regex(@"(项目负责人|项目经理|项目总监)(：|:)[^\r\n]+\r\n");
                            prjMgr = regprjMgr.Match(bidCtx).Value.Replace("项目负责人：", "").Replace("项目经理：", "").Replace("项目总监：", "").Trim();
                            Regex regcode = new Regex(@"项目编号(：|:)[^\r\n]+\r\n");
                            code = regcode.Match(bidCtx).Value.Replace("项目编号：", "").Replace("：", "").Trim();
                            msgType = "珠海市建设工程交易中心";
                            specType = "建设工程";
                            bidCtx = bidCtx.Replace("<?", "").Replace("xml:namespace prefix = st1 ns = ", "").Replace("urn:schemas-microsoft-com:office:smarttags", "").Replace("/>", "").Trim();
                            bidCtx = bidCtx.Replace("<?", "").Replace("xml:namespace prefix = o ns = ", "").Replace("urn:schemas-microsoft-com:office:office", "").Replace("/>", "").Trim();
                            Regex regInvType = new Regex(@"[^\r\n]+[\r\n]{1}");
                            buildUnit = "";
                            if (bidUnit == "")
                            {
                                bidUnit = "";
                            }
                            prjName = ToolDb.GetPrjName(prjName);
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "珠海市区", "", string.Empty, code, prjName, buildUnit, beginDate,bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
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
