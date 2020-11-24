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
    public class BidHuiNanZtb : WebSiteCrawller
    {
        public BidHuiNanZtb()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "湖南省建设工程招标投标网中标信息";
            this.Description = "自动抓取湖南省建设工程招标投标网中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.SiteUrl = "http://www.hnztb.org/Index.aspx?action=ucBiddingList&modelCode=0004&ItemCode=000009002&name=%u5efa%u7b51%u5de5%u7a0b%u4e2d%u6807%u516c%u793a";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ID_ucBiddingList_ucPager1_lbPage")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().Replace("1/", "");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "ID_ucBiddingList$txtTitle",
                    "ID_ucBiddingList$ucPager1$listPage"
                    }, new string[]{
                    "ID_ucBiddingList$ucPager1$btnNext",
                    "",
                    viewState,
                    "",
                    (i-1).ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                 NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellPadding", "0")));
                 if (listNode != null && listNode.Count > 2)
                 {
                     TableTag table = listNode[2] as TableTag;
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
                                prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                             TableRow tr = table.Rows[j];
                        ATag aTag = tr.GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = tr.Columns[0].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.hnztb.org" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table3")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            prjAddress = bidCtx.GetAddressRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("第一名");
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            try
                            {
                                if (decimal.Parse(bidMoney) > 10000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            bidUnit = bidUnit.GetReplace(new string[] { "_" });
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            bidType = prjName.GetInviteBidType();
                            specType = "建设工程";
                            msgType = "湖南省住房和城乡建设厅";
                            BidInfo info = ToolDb.GenBidInfo("湖南省", "湖南省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
