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
    public class BidYangJiangZF : WebSiteCrawller
    {
        public BidYangJiangZF()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省阳江市住房和城市规划建设工程中标信息";
            this.Description = "自动抓取广东省阳江市住房和城市规划建设工程中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "ProjectName,InfoUrl";
            this.SiteUrl = "http://www.yjjs.gov.cn/category/zhongbiaogonggao";
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
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch 
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("align", "center")));
            if (nodeList != null && nodeList.Count > 0)
            {
                Regex regexPage = new Regex(@"\d+页");
                page = int.Parse(regexPage.Match(nodeList.AsString()).Value.Trim(new char[] { '页' }));
            }
            for (int i = 1; i < page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&otype=&page=" + i.ToString()), Encoding.UTF8);
                    }
                    catch { continue; }

                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("height", "23")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableRow tr = new TableRow();
                    for (int j = 0; j < tableNodeList.Count; j++)
                    {
                        string InfoUrl = string.Empty,tempName=string.Empty,tempDate=string.Empty;
                        TableTag table = tableNodeList.SearchFor(typeof(TableTag), true)[j] as TableTag;
                        for (int k = 0; k < 1; k++)
                        {
                            tr = table.Rows[k];
                            ATag aTag = tr.Columns[1].GetATag();
                            string url = "http://www.yjjs.gov.cn/news_Info.asp?rs_id=" + aTag.GetAttribute("onclick").Replace("titlelinks(", "");
                            int ii = url.LastIndexOf("''");

                            tempName = aTag.LinkText.ToNodeString();
                            tempDate = tr.Columns[2].ToNodePlainString().GetReplace(".", "-").GetDateRegex();
                            InfoUrl = url.Remove(ii).Replace(",", "").Replace("'", "").Replace("javascript:", "").Trim();

                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                            }
                            catch
                            {
                                continue;
                            }
                            Parser parserdetail = new Parser(new Lexer(htmldetail));
                            NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")), true), new TagNameFilter("table")));
                            if (dtnode != null && dtnode.Count > 0)
                            {
                                TableTag dtlTable = dtnode[0] as TableTag;
                                for (int r = 1; r < dtlTable.RowCount; r++)
                                {
                                    string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;


                                    for (int c = 1; c < dtlTable.Rows[r].ColumnCount; c++)
                                    {
                                        try
                                        {
                                            string temp = dtlTable.Rows[r].Columns[c].ToNodePlainString();
                                            string title = dtlTable.Rows[0].Columns[c].ToNodePlainString();
                                            HtmlTxt += title + "：" + temp + "</br>";
                                            bidCtx += title + "：" + temp + "\r\n";
                                        }
                                        catch { continue; }
                                    }

                                    prjName = bidCtx.GetRegex("工程项目名称,项目名称，工程名称", true, 200);
                                    buildUnit = bidCtx.GetRegex("建设单位");
                                    beginDate = bidCtx.GetRegex("中标日期");
                                    bidMoney = bidCtx.GetMoneyRegex();
                                    bidUnit = bidCtx.GetRegex("中标单位名称");
                                    prjMgr = bidCtx.GetMgrRegex();
                                    prjAddress = bidCtx.GetAddressRegex();
                                    bidType = bidCtx.GetRegex("中标单位资质类别");

                                    msgType = "阳江市建设工程交易中心";
                                    specType = "建设工程";

                                    BidInfo info = ToolDb.GenBidInfo("广东省", "阳江市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                    list.Add(info); 
                                    if (!crawlAll && list.Count >= this.MaxCount)
                                        return list;
                                }

                            }
                            else
                            {
                                parserdetail.Reset();
                                NodeList dtlNode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));
                                if (dtlNode != null && dtlNode.Count > 0)
                                {
                                    string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                                    HtmlTxt = dtlNode.AsHtml();
                                    bidCtx = HtmlTxt.ToCtxString();
                                     
                                    buildUnit = bidCtx.GetBuildRegex(); 
                                    bidMoney = bidCtx.GetMoneyRegex();
                                    bidUnit = bidCtx.GetBidRegex();
                                    prjMgr = bidCtx.GetMgrRegex();
                                    prjAddress = bidCtx.GetAddressRegex();

                                    bidType = tempName.GetInviteBidType();

                                    msgType = "阳江市建设工程交易中心";
                                    specType = "建设工程";

                                    BidInfo info = ToolDb.GenBidInfo("广东省", "阳江市区", "", string.Empty, code, tempName, buildUnit, tempDate, bidUnit, tempDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                    list.Add(info);
                                    if (!crawlAll && list.Count >= this.MaxCount)
                                        return list;

                                }

                            }


                        }
                    }
                }
            }
            return list;
        }
    }
}
