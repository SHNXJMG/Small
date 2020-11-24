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
    public class BidSzYanCao : WebSiteCrawller
    {
        public BidSzYanCao()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "深圳烟草工业有限责任公司中标信息";
            this.Description = "自动抓取深圳烟草工业有限责任公司中标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.SiteUrl = "http://www.szjyc.com/ExPortal/InfoListCommand.aspx?CmdType=getlist&FuWuQiBiaoShi=CPCNS_XinXiMenHu&LanMuBiaoShi=169&CurPage=1&PageSize=12&RecCount=-1&rnd=0.3884795850959004";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string Mylist = string.Empty;
            int page = 3;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8, ref cookiestr);
            }
            catch (Exception ex)
            {
                return list;
            }
            for (int i = 1; i < page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.szjyc.com/ExPortal/InfoListCommand.aspx?CmdType=getlist&FuWuQiBiaoShi=CPCNS_XinXiMenHu&LanMuBiaoShi=169&CurPage=" + i.ToString() + "&PageSize=12&RecCount=-1&rnd=0.3884795850959004"), Encoding.UTF8, ref cookiestr);
                    }
                    catch (Exception ex) { continue; }
                }
                Mylist = htl.ToString().Replace("[", "").Replace("]", "").Trim();
                string[] str = Mylist.Split('}');
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == "")
                    {
                        continue;
                    }
                    string prjName = string.Empty,
                         buildUnit = string.Empty, bidUnit = string.Empty,
                         bidMoney = string.Empty, code = string.Empty,
                         bidDate = string.Empty,
                         beginDate = string.Empty,
                         endDate = string.Empty, bidType = string.Empty,
                         specType = string.Empty, InfoUrl = string.Empty,
                         msgType = string.Empty, bidCtx = string.Empty,
                         prjAddress = string.Empty, remark = string.Empty,
                         prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, HangBiaoShi = string.Empty;
                    string[] str1 = str[j].Split(',');
                    for (int k = 0; k < str1.Length; k++)
                    {
                        HangBiaoShi = str1[k].ToString();
                        if (HangBiaoShi == "")
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    Regex regNum = new Regex(@"\d{1,9}");
                    HangBiaoShi = regNum.Match(HangBiaoShi).Value.Trim();
                    InfoUrl = "http://www.szjyc.com/ExPortal/Details.aspx?HangBiaoShi=" + HangBiaoShi + "&LanMuBiaoShi=169&rnd=0.26078120219395623";
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
                    NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("h1"), new HasAttributeFilter("class", "infoTitle")));
                    prjName = dtnode.AsString().Replace("\r\n", "").Trim();
                    if (prjName.Contains("结果公示")||prjName.Contains("中标公告"))
                    {
                        parserdetail = new Parser(new Lexer(htmldetail));
                        dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "infoContent")));
                        HtmlTxt = dtnode.AsHtml();
                        bidCtx = dtnode.AsString().Replace("UIDataBegin", "").Trim();
                        parserdetail = new Parser(new Lexer(htmldetail));
                        dtnode = parserdetail.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                        if (dtnode!=null&&dtnode.Count > 0)
                        {
                            TableTag table = (TableTag)dtnode[0];
                            for (int r = 1; r < table.RowCount; r++)
                            {
                                TableRow row = table.Rows[r];
                                for (int c = 0; c < row.ColumnCount; c++)
                                {
                                    if (row.Columns[c].ToPlainTextString().Contains("招标编号") && c + 1 < row.ColumnCount)
                                    {
                                        code = row.Columns[c + 1].ToPlainTextString().Trim();
                                    }
                                    if (row.Columns[c].ToPlainTextString().Contains("招标人") && c + 1 < row.ColumnCount)
                                    {
                                        buildUnit = row.Columns[c + 1].ToPlainTextString().Trim();
                                    }
                                    if (row.Columns[c].ToPlainTextString().Contains("中标人") && c + 1 < row.ColumnCount)
                                    {
                                        bidUnit = row.Columns[c + 1].ToPlainTextString().Trim();
                                    }
                                    if (row.Columns[c].ToPlainTextString().Contains("项目价格") && c + 1 < row.ColumnCount)
                                    {
                                        bidMoney = row.Columns[c + 1].ToPlainTextString().Trim();
                                    }
                                }
                            }
                        }
                        Regex regEndDate = new Regex(@"\d{4}年\d{1,2}月\d{1,2}日");
                        parserdetail = new Parser(new Lexer(htmldetail));
                        dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "infoOther")));
                        beginDate = regEndDate.Match(dtnode.AsString()).Value.Trim();
                        msgType = "深圳烟草工业有限责任公司";
                        specType = "其他";
                        bidType = ToolHtml.GetInviteTypes(prjName);
                        if (buildUnit == "")
                        {
                            buildUnit = "深圳烟草工业有限责任公司";
                        }
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
                        if (beginDate == "")
                        {
                            beginDate = string.Empty;
                        }
                        if (endDate == "")
                        {
                            endDate = string.Empty;
                        }
                        prjName = ToolDb.GetPrjName(prjName);
                        BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate,
                                        bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                        bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
