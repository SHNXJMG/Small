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
    public class BidSzGongmjdb : WebSiteCrawller
    {
        public BidSzGongmjdb()
            : base()
        {
            this.Group = "街道办中标信息";
            this.Title = "广东省深圳市光明新区公明街道办事处中标信息";
            this.Description = "自动抓取广东省深圳市光明新区公明街道办事处中标信息";
            this.PlanTime = "9:18,13:49";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.szgm.gov.cn/gmbsc/143049/143173/143181/index.html";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "Normal")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().GetRegexBegEnd("/", "跳");
                    pageInt = int.Parse(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.szgm.gov.cn/gmbsc/143049/143173/143181/ecdc3e5c-" + i.ToString() + ".html", Encoding.UTF8);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hyxejc")), true), new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%"))));
                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 1; j < viewList.Count - 1; j++)
                    {
                        TableRow tr = (viewList[j] as TableTag).Rows[0];
                        ATag aTag = tr.GetATag();
                        if (aTag == null || tr.ColumnCount != 3) continue;

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

                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        prjName = aTag.GetAttribute("title");

                        InfoUrl = "http://www.szgm.gov.cn" + aTag.Link;
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
                            htlDtl = regexHtml.Replace(htlDtl, "");
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page_con")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            string tempName = bidCtx.GetRegex("工程名称");
                            if (!string.IsNullOrWhiteSpace(tempName))
                                prjName = tempName;
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("委托单位");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegexBegEnd("确认", "为中标单位");
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetRegex("合同价").GetMoney();
                            if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetRegexBegEnd("人民币", "元").GetMoney();

                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("border", "1")));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    TableTag tableBid = tableNode[0] as TableTag;
                                    if (tableBid.RowCount > 1)
                                    {
                                        string ctx = string.Empty;
                                        for (int c = 0; c < tableBid.Rows[0].ColumnCount; c++)
                                        {
                                            try
                                            {
                                                ctx += tableBid.Rows[0].Columns[c].ToNodePlainString() + "：";
                                                ctx += tableBid.Rows[1].Columns[c].ToNodePlainString() + "\r\n";
                                            }
                                            catch { }

                                        }

                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                            bidMoney = ctx.GetMoneyRegex(null, false, "万元");
                                    }
                                }
                            }
                            try
                            {
                                if (decimal.Parse(bidMoney) > 50000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }


                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList imgNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                            if (imgNode != null && imgNode.Count > 0)
                            {
                                for (int img = 0; img < imgNode.Count; img++)
                                {
                                    ImageTag image = imgNode[img] as ImageTag;
                                    string url = image.GetAttribute("src");
                                    string saveUrl = "http://www.szgm.gov.cn" + url;
                                    HtmlTxt = HtmlTxt.Replace(url, saveUrl);
                                }
                            }

                            msgType = "深圳市光明新区公明街道办事处";
                            if (string.IsNullOrEmpty(prjAddress))
                            { prjAddress = "见招标信息"; }
                            specType = "政府采购";
                            bidType = "小型工程";
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "深圳市光明新区公明街道办事处";
                            }
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳区及街道工程", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                   bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
