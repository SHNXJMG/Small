using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class BidMeiZhouGGZY : WebSiteCrawller
    {
        public BidMeiZhouGGZY()
        {
            this.Group = "中标信息";
            this.Title = "梅州市公共资源交易中心中标";
            this.Description = "自动抓取梅州市公共资源交易中心中标公告";
            this.PlanTime = "9:10,11:10,14:10,16:10,18:10";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 200;
            this.SiteUrl = "http://www.mzggzy.com/TPFront/jsgc/004004/";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception)
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "wb-page-li")), true), new TagNameFilter("a")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    ATag atag = pageList[pageList.Count - 2] as ATag;
                    string temp = atag.LinkText.GetReplace("1/", "");
                    pageInt = int.Parse(temp);
                }
                catch
                { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.mzggzy.com/TPFront/jsgc/004004/?pageing=" + i.ToString(), Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "ewb-data-items ewb-pt6")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {

                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
          bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;


                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.ChildrenHTML.GetReplace("\r\n", "").ToRegString();
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.mzggzy.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "infodetail")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            bidMoney = bidCtx.GetMoneyRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                              bidUnit = bidCtx.GetRegex("第一中标人");

                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch((new TagNameFilter("table")));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag tab = tableNode[0] as TableTag;
                                    for (int r = 1; r < tab.RowCount; r++)
                                    {
                                        for (int c = 0; c < tab.Rows[r].ColumnCount; c++)
                                        {
                                            if (c > 1) continue;
                                            string temp = tab.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                            if (c == 0)
                                                ctx += temp + "：";
                                            else
                                                ctx += temp + "\r\n";
                                        }
                                    }
                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("单位名称");
                                    bidMoney = ctx.GetMoneyRegex();
                                    if (string.IsNullOrWhiteSpace(bidMoney))
                                        bidMoney = ctx.GetRegex("投标报价（元）").GetReplace("￥", "");
                                    if (string.IsNullOrWhiteSpace(bidMoney))
                                        bidMoney = ctx.GetRegex("投标报价（元/米）");
                                    prjMgr = ctx.GetMgrRegex();
                                    if (string.IsNullOrWhiteSpace(prjMgr))
                                        prjMgr = ctx.GetRegex("编号").GetNotChina();
                                    if (prjMgr.Contains("粤"))
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("粤"));
                                    if (prjMgr.Contains("证书"))
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("证书"));
                                }
                            }
                            try
                            {
                                if (Convert.ToDecimal(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            msgType = "梅州市公共资源交易中心";
                            specType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "梅州市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
