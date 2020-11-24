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
    public class BidSzZhiYuan : WebSiteCrawller
    {
        public BidSzZhiYuan()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "深职院中标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深职院中标信息";
            this.SiteUrl = "http://zhaobiao.szpt.edu.cn/article_list.asp?classid=3";
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
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("align", "right")));
            Regex regexPage = new Regex(@"\d+页");
            try
            {
                page = Convert.ToInt32(regexPage.Match(nodeList.AsString()).Value.Replace("页", "").Trim());
            }
            catch (Exception)
            { }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&page=" + i.ToString()), Encoding.Default);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("align", "center")));
                if (tableNodeList != null && tableNodeList.Count > 1)
                {
                    TableTag table = (TableTag)tableNodeList[3];
                    for (int j = 0; j < table.RowCount - 1; j++)
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
                        ATag aTag = tr.Columns[0].SearchFor(typeof(ATag), true)[1] as ATag;
                        prjName = aTag.LinkText;
                        beginDate = tr.Columns[1].ToPlainTextString().Trim();
                        InfoUrl = "http://zhaobiao.szpt.edu.cn/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new TagNameFilter("p"));
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            Regex regeximg = new Regex(@"<IMG[^>]*>");//去掉图片
                            HtmlTxt = regeximg.Replace(HtmlTxt, "");
                            for (int z = 0; z < dtnode.Count; z++)
                            {
                                bidCtx += dtnode[z].ToPlainTextString().Replace("&nbsp;", "").Trim() + "\r\n";
                            }
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            bidCtx = regexHtml.Replace(bidCtx, "");
                            Regex regcode = new Regex(@"(项目编号|招标编号)(：|:)[^\r\n]+\r\n");
                            code = regcode.Match(bidCtx).Value.Replace("项目编号：", "").Replace("招标编号：", "").Replace("：", "").Trim();
                            Regex regBidUnit = new Regex(@"(成交单位|中标单位)(：|:)[^\r\n]+\r\n");
                            bidUnit = regBidUnit.Match(bidCtx).Value.Replace("成交单位：", "").Replace("中标单位：", "").Replace("中标折扣率：72.5%", "").Trim();
                            Regex regMoney = new Regex(@"(中标价|中标价格)(：|:)[^\r\n]+\r\n");
                            bidMoney = regMoney.Match(bidCtx).Value.Replace("中标价：", "").Replace("中标价格：", "").Replace(",", "").Trim();
                            Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
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
                            if (bidType == "设备材料" || bidType == "小型施工" || bidType == "专业分包" || bidType == "劳务分包" || bidType == "服务" || bidType == "勘察" || bidType == "设计" || bidType == "监理" || bidType == "施工")
                            {
                                specType = "建设工程";
                            }
                            else
                            {
                                specType = "其他";
                            }
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            msgType = "深职院";
                            bidType = ToolHtml.GetInviteTypes(bidType);
                            prjName = ToolDb.GetPrjName(prjName);
                            if (Encoding.Default.GetByteCount(code) > 50)
                                code = string.Empty;
                            if (Encoding.Default.GetByteCount(buildUnit) > 150)
                                buildUnit = string.Empty;
                            if (Encoding.Default.GetByteCount(bidUnit) > 150)
                                bidUnit = string.Empty;
                            if (Encoding.Default.GetByteCount(prjAddress) > 150)
                                prjAddress = string.Empty;
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
