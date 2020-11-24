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
using System.Threading;

namespace Crawler.Instance
{
    public class BidInfoGuiAn : WebSiteCrawller
    {
        public BidInfoGuiAn()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "贵安新区公共资源交易中心中标信息";
            this.Description = "自动抓取贵安新区公共资源交易中心中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 500;
            this.SiteUrl = "http://www.gaxqjyzx.com/gaxqztb/jyxx/001001/001001006/MoreInfo.aspx?CategoryNum=262662";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "MoreInfoList1_Pager")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToPlainTextString().GetRegexBegEnd("总页数", "当前").Replace("：", "");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                        new string[] { 
                        "__VIEWSTATE",
                        "__EVENTTARGET",
                        "__EVENTARGUMENT"
                        },
                        new string[] { 
                        viewState,
                        "MoreInfoList1$Pager",
                        i.ToString()
                        }
                        );
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                 parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "MoreInfoList1_DataGrid1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                          TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.gaxqjyzx.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</p>,</span>", "\r\n").ToCtxString();
                            string ctx = HtmlTxt.GetReplace("</p>", "\r\n").ToCtxString();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = ctx.GetBidRegex();
                            bidMoney = ctx.GetMoneyRegex();
                            if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetMoneyRegex();

                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                string bidUnitCtx = HtmlTxt.GetReplace("第一中标候选人", "kdxx").GetReplace("</span>", "xxdk").ToCtxString().GetReplace("kdxx：xxdk","kdxx");
                                bidUnit = bidUnitCtx.GetRegexBegEnd("kdxx", "xxdk");
                            }
                            if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                            {
                                string bidUnitCtx = HtmlTxt.GetReplace("报价（人民币）,投标报价（人民币）,投标报价,中标价,报价,中标金额", "kdxx").GetReplace("</span>", "xxdk").ToCtxString().GetReplace("kdxx：xxdk", "kdxx");
                                bidMoney = bidUnitCtx.GetRegexBegEnd("kdxx", "xxdk");
                            }

                            code = bidCtx.GetCodeRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            specType = "建设工程";
                            msgType = "贵安新区公共资源交易中心";
                            BidInfo info = ToolDb.GenBidInfo("贵州省", "贵州省及地市", "贵安新区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
