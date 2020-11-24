using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class BidInfoTianJinZTB : WebSiteCrawller
    {
        public BidInfoTianJinZTB()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "天津市招投标网";
            this.Description = "自动抓取天津市招投标网";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 1000;
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.tjztb.gov.cn/zbgg1/";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_1_page_text")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode.AsString();
                try
                {
                    pageInt = int.Parse(temp.GetRegexBegEnd("HTML", ",").Replace("(", ""));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "index_" + (i - 1).ToString() + ".shtml");
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_1_right_list")), true), new TagNameFilter("ul")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
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

                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        ATag aTag = listNode[j].GetATag();
                        prjName = aTag.LinkText;
                        InfoUrl = this.SiteUrl + aTag.Link.Replace("../", "").Replace("./", "").Replace(" ","");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "text")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList namNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "text_bt")));
                            if (namNode != null && namNode.Count > 0)
                            {
                                string temp = prjName;
                                prjName = namNode[0].ToNodePlainString();
                                if (!prjName.Contains(temp.Replace(".", "")))
                                    prjName = temp;
                            }
                            bidType = prjName.GetInviteBidType() ;
                            specType = "建设工程";
                            msgType = "天津市发展和改革委员会";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "天津市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
