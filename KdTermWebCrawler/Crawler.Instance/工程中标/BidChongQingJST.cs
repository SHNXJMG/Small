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
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class BidChongQingJST : WebSiteCrawller
    {
        public BidChongQingJST()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "重庆建设工程信息网中标信息";
            this.Description = "自动抓取重庆建设工程信息网中标信息";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www1.cqjsxx.com/webcqjg/GcxxFolder/zhongbiao.aspx";
            this.MaxCount = 400;
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "lblPageCount")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString();
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    //string str = System.Web.HttpUtility.("%A1%AA%A1%AA%C6%F3%D2%B5%A1%AA%A1%AA");
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "__EVENTVALIDATION",
                    "textfield",
                    "textfield",
                    "select",
                    "SearchName",
                    "SearchNo",
                    "txtSqlText",
                    "checkPage"
                    },
                        new string[]{
                        "Linkbutton3",
                        "",
                        viewState,
                        eventValidation,
                        "","",
                        "%A1%AA%A1%AA%C6%F3%D2%B5%A1%AA%A1%AA",
                        "","",
                        " FProjectName like ''%%'' and FTNO like ''%%''",
                        (i-1).ToString()
                        });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "dgData")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[0].ToNodePlainString();
                        bidUnit = tr.Columns[1].ToNodePlainString();
                        string city = tr.Columns[2].ToNodePlainString();
                        beginDate = tr.Columns[3].ToNodePlainString();
                             InfoUrl = "http://www1.cqjsxx.com/webcqjg/GcxxFolder/" + tr.Columns[0].GetATagHref();
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "DetailTable")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            TableTag tag = dtlNode[0] as TableTag;
                            for (int r = 0; r < tag.RowCount; r++)
                            {
                                for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                {
                                    if ((c + 1) % 2 == 0)
                                        bidCtx += tag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：") + "\r\n";
                                    else
                                        bidCtx += tag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：") + "：";
                                }
                            }
                            buildUnit = bidCtx.GetBuildRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            code = bidCtx.GetCodeRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            specType = "建设工程";
                            bidType = prjName.GetInviteBidType();
                            msgType = "重庆市工程建设招标投标交易中心";
                            BidInfo info = ToolDb.GenBidInfo("重庆市", "重庆市及区县", city, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k] as ATag;
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://www1.cqjsxx.com/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
