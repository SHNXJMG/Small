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
    public class BidHuNanZTJG : WebSiteCrawller
    {
        public BidHuNanZTJG()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "湖南省招标投标监管网中标公告";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取湖南省招标投标监管网中标公告";
            this.SiteUrl = "http://www.bidding.hunan.gov.cn/item/itemWinResult.aspx";
            this.MaxCount = 200;
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));

            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "WebPager1_LabelLbtMsg")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("共", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {

                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                         "__EVENTTARGET",
                         "__EVENTARGUMENT",
                         "__VIEWSTATE",
                         "__VIEWSTATEENCRYPTED",
                         "__EVENTVALIDATION",
                         "tbProclaimTitle",
                         "cldReleasetTimeFrom_year",
                         "cldReleasetTimeFrom_month",
                         "cldReleasetTimeFrom_day",
                         "cldReleasetTimeFrom",
                         "cldReleasetTimeTo_year",
                         "cldReleasetTimeTo_month",
                         "cldReleasetTimeTo_day",
                         "cldReleasetTimeTo",
                         "ucArea$ddlCodeList",
                         "ucType$ddlCodeList",
                         "tsubmitPerson",
                         "tbagentOrg",
                         "WebPager1$tbLbtPage"

                    }, new string[] {
                         "WebPager1$lbtNavNext",
                        "",
                        viewState,
                        "",
                        eventValidation,
                        "","","","","","","","","","-1","-1","","",i.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "gvItemBuild")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        if (aTag == null) continue;
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
                        prjName = aTag.LinkText.GetReplace(" ");
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.bidding.hunan.gov.cn/item/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtnode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("TABLE"), new HasAttributeFilter("id", "TABLE1")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</p>,</br>", "\r\n").GetReplace("<br />", "\r\n").ToCtxString();


                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("中标人");
                            bidMoney = bidCtx.GetMoneyRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            if (string.IsNullOrWhiteSpace(buildUnit))
                                buildUnit = bidCtx.GetRegex("招标人");
                            code = bidCtx.GetCodeRegex().GetCodeDel();

                            try
                            {
                                if (Convert.ToDecimal(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            msgType = "湖南省发展和改革委员会";
                            specType = "建设工程";
                            bidType = ToolHtml.GetInviteTypes(prjName);

                            BidInfo info = ToolDb.GenBidInfo("湖南省", "湖南省及地市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.bidding.hunan.gov.cn/" + a.Link;
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
