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
    public class BidYJHLJS : WebSiteCrawller
    {
        public BidYJHLJS()
            : base()  
        {
            this.Group = "中标信息";
            this.Title = "广东省阳江市海陵县建设工程中标信息";
            this.Description = "自动抓取广东省阳江市海陵县建设工程中标信息";
            this.SiteUrl = "http://www.yjgcjy.cn/BeingList.aspx?SiteId=25";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
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
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("colSpan", "6")));
            if (nodeList != null && nodeList.Count > 0)
            {
                Regex regexPage = new Regex(@"共\d+页");
                page = int.Parse(regexPage.Match(nodeList.AsString()).Value.Trim(new char[] { '共', '页' }));
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(htl);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__VIEWSTATE",
                        "key",
                        "AxGridView1$ctl23$ctl07",
                        "AxGridView1$ctl23$pageList",
                        "__VIEWSTATEENCRYPTED",
                        "__EVENTVALIDATION"
                    }, new string[]{
                        "AxGridView1$ctl23$ctl03",
                        string.Empty,
                        viewState,
                         string.Empty,
                        "20",
                        (i-1).ToString(),
                        string .Empty,
                        eventValidation
                    });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "AxGridView1")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
                    for (int j = 1; j < table.RowCount - 1; j++)
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
                        code = tr.Columns[2].ToPlainTextString().Trim();
                        prjName = tr.Columns[3].ToPlainTextString().Trim();
                        bidUnit = tr.Columns[4].ToPlainTextString().Trim();
                        //beginDate = DateTime.Today.ToString();
                        ATag aTag = tr.Columns[5].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://www.yjgcjy.cn/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            Logger.Error("BidYJHLJS"); 
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "nr")));
                        string regd = dtnode.AsString().Replace("：", "").Replace("。", "").Trim();
                        Regex regDate = new Regex(@"\d{4}年\d{1,2}月\d{1,2}日");
                        Regex regDate1 = new Regex(@"\d{4}年\d{1,2}月\d{1,2}、\d{1,2}日");
                        Regex regDate2 = new Regex(@"\d{4} 年 \d{1,2} 月 \d{1,2} 日");
                        Regex regDate3 = new Regex(@"\d{4}年\d{1,2}月\d{1,2}至\d{1,2}日");
                        beginDate = regDate.Match(regd).ToString();
                        if (beginDate == "")
                        {
                            try
                            {
                                beginDate = regDate1.Match(regd).ToString();
                                beginDate = beginDate.Remove(beginDate.IndexOf("、")).Trim();
                            }
                            catch (Exception)
                            {
                                beginDate = "";
                            }
                        }
                        if (beginDate == "")
                        {
                            try
                            {
                                beginDate = regDate3.Match(regd).ToString();
                                beginDate = beginDate.Remove(beginDate.IndexOf("至")).Trim();
                            }
                            catch (Exception)
                            {
                                beginDate = "";
                            }
                        }
                        if (beginDate == "")
                        {
                            try
                            {
                                beginDate = regDate2.Match(regd).ToString().Trim();
                            }
                            catch (Exception)
                            {
                                beginDate = "";
                            }
                        }
                        if (beginDate == "")
                        {
                            continue;
                        }
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            TableTag tabledetail = dtnode.SearchFor(typeof(TableTag), true)[0] as TableTag;
                            if (tabledetail != null && tabledetail.RowCount > 0)
                            {
                                for (int r = 0; r < tabledetail.RowCount; r++)
                                {
                                    TableRow trdetail = tabledetail.Rows[r];
                                    for (int c = 0; c < trdetail.ColumnCount; c++)
                                    {
                                        string tr1 = string.Empty;
                                        string tr2 = string.Empty;
                                        tr1 = trdetail.Columns[c].ToPlainTextString().Trim();
                                        if (c + 1 < trdetail.ColumnCount)
                                        {
                                            tr2 = trdetail.Columns[c + 1].ToPlainTextString().Trim();
                                        }

                                        bidCtx += tr1 + "：" + tr2 + "：";
                                        if (trdetail.ColumnCount > (c + 1))
                                        {
                                            c = c + 1;
                                        }

                                    }
                                    bidCtx += "\r\n";
                                }
                                bidCtx = bidCtx.Replace("（盖章）", "").Replace("（元）", "").Replace("(元)", "").Replace("(盖章)", "").Trim();
                                Regex bildUnit = new Regex(@"(招标人|承包人)：[^\r\n]+[\r\n]{1}");
                                buildUnit = bildUnit.Match(bidCtx).Value.Replace("招标人：", "").Replace("：", "").Replace("承包人", "").Trim();
                                if (buildUnit == "")
                                {
                                    Regex bildUnittwo = new Regex(@"招标人\r\n\r\n\r\n[^\r\n]+[\r\n]{1}");
                                    buildUnit = bildUnittwo.Match(bidCtx).Value.Replace("招标人\r\n\r\n\r\n", "").Replace("：", "").Trim();
                                    if (buildUnit == "")
                                    {
                                        buildUnit = "";
                                    }
                                }
                                Regex regMoney = new Regex(@"(中标价|发包价|总投资)：[^\r\n]+\r\n");
                                bidMoney = regMoney.Match(bidCtx).Value.Replace("中标价：", "").Replace("发包价：", "").Replace("总投资：", "").Trim();
                                Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                                if (bidMoney.Contains("："))
                                {
                                    bidMoney = bidMoney.Remove(bidMoney.IndexOf("：")).Trim();
                                }
                                if (bidMoney.Contains("万元"))
                                {
                                    bidMoney = bidMoney.Remove(bidMoney.IndexOf("万元")).Trim();
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

                                Regex regprjMgr = new Regex(@"(项目负责人|项目总监|项目经理)：[^\r\n]+\r\n");
                                prjMgr = regprjMgr.Match(bidCtx).Value.Replace("项目负责人：", "").Replace("项目总监：", "").Replace("项目经理：", "").Trim();
                                if (string.IsNullOrEmpty(prjMgr))
                                {
                                    prjMgr = string.Empty;
                                }
                                else
                                {
                                    prjMgr = prjMgr.Remove(prjMgr.IndexOf("：")).Trim();
                                    if (Encoding.Default.GetByteCount(prjMgr) > 12 || prjMgr == "")
                                    {
                                        prjMgr = "见中标详细信息";
                                    }
                                }
                                msgType = "阳江市建设工程交易中心";
                                specType = "建设工程";
                                if (bidMoney == "0")
                                {
                                    //TableTag tabledetail = dtnode.SearchFor(typeof(TableTag), true)[0] as TableTag;
                                    if (tabledetail.RowCount >= 2)
                                    {
                                        TableRow trdetail = tabledetail.Rows[1];
                                        if (trdetail.ChildCount > 2)
                                        {
                                            Regex regBidMoneyR = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                                            bidMoney = trdetail.Columns[2].ToPlainTextString().Trim();
                                            try
                                            {
                                                bidMoney = (decimal.Parse(regBidMoneyR.Match(bidMoney).Value) / 10000).ToString();
                                                bidUnit = trdetail.Columns[1].ToPlainTextString().Trim();
                                            }
                                            catch (Exception)
                                            {
                                                bidMoney = "0";
                                                bidUnit = "";
                                            }

                                        }
                                    }
                                }
                                bidCtx = bidCtx.Replace("<?", "").Replace("xml:namespace prefix = st1 ns = ", "").Replace("urn:schemas-microsoft-com:office:smarttags", "").Replace("/>", "").Replace("：", "").Trim();
                                bidCtx = bidCtx.Replace("<?", "").Replace("xml:namespace prefix = o ns = ", "").Replace("urn:schemas-microsoft-com:office:office", "").Replace("/>", "").Replace("：", "").Trim();
                                bidCtx = bidCtx.Replace(" xml:namespace prefix = st1 ns = ", "").Trim();
                                bidType = ToolHtml.GetInviteTypes(prjName);
                                BidInfo info = ToolDb.GenBidInfo("广东省", "阳江市区", "海陵区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                       bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }

                            else
                            {
                                Parser parserdetailtwo = new Parser(new Lexer(htmldetail));
                                NodeList dtnodetwo = parserdetailtwo.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("P"), new HasAttributeFilter("class", "MsoNormal")));
                                HtmlTxt = dtnodetwo.AsHtml();
                                string text = string.Empty;
                                bidCtx = dtnodetwo.AsString().Trim();
                                bidCtx = bidCtx.Replace("。", "\r\n").Trim();
                                bidCtx = System.Web.HttpUtility.HtmlDecode(bidCtx);
                                for (int r = 0; r < dtnodetwo.Count; r++)
                                {
                                    if (dtnodetwo[r].ToPlainTextString().Trim() == "单位名称")
                                    {
                                        bidUnit = dtnodetwo[r + 1].ToPlainTextString().Trim();
                                    }
                                    if (dtnodetwo[r].ToPlainTextString().Trim() == "总投资额")
                                    {
                                        bidMoney = dtnodetwo[r + 1].ToPlainTextString().Trim();
                                    }
                                    text += dtnodetwo[r].ToPlainTextString().Trim() + "\r\n";
                                }
                                Regex regMoney = new Regex(@"(投标报价|发包价)：[^\r\n]+\r\n");
                                bidMoney = regMoney.Match(text).Value.Replace("投标报价：", "").Trim();
                                Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                                if (bidMoney.Contains("万元"))
                                {
                                    bidMoney = bidMoney.Remove(bidMoney.IndexOf("万元")).Trim();
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
                                msgType = "阳江市建设工程交易中心";
                                specType = "建设工程";

                                bidCtx = bidCtx.Replace("<?", "").Replace("xml:namespace prefix = st1 ns = ", "").Replace("urn:schemas-microsoft-com:office:smarttags", "").Replace("/>", "").Replace("：", "").Trim();
                                bidCtx = bidCtx.Replace("<?", "").Replace("xml:namespace prefix = o ns = ", "").Replace("urn:schemas-microsoft-com:office:office", "").Replace("/>", "").Replace("：", "").Trim();
                                bidCtx = bidCtx.Replace(" xml:namespace prefix = st1 ns = ", "").Trim();
                                prjName = ToolDb.GetPrjName(prjName);
                                BidInfo info = ToolDb.GenBidInfo("广东省", "阳江市区", "海陵区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                       bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                    }
                }

            } return list;
        }
    }
}
