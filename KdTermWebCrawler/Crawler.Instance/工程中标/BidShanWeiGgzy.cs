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
using Winista.Text.HtmlParser.Data;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class BidShanWeiGgzy : WebSiteCrawller
    {
        public BidShanWeiGgzy()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省汕尾市公共资源交易中心中标信息";
            this.Description = "自动抓取广东省汕尾市公共资源交易中心";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "ProjectName,InfoUrl";
            this.SiteUrl = "http://www.swggzy.cn/newslist.aspx?Id=089ddf2ff80c4e259ac5b208e9154469";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            int sqlCount = 0;
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_total_page_count")));
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
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "__EVENTVALIDATION",
                    "ctl00$ContentPlaceHolder1$CP"
                    }, new string[] { 
                    "ctl00$ContentPlaceHolder1$DownPage",
                    "",
                    viewState,
                    eventValidation,
                    i.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "secondrightlistbox")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
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
                        prjName = aTag.GetAttribute("title");
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.swggzy.cn/" + aTag.Link.GetReplace("amp;");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "contentbox2")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidType = prjName.GetInviteBidType();
                            msgType = "汕尾市公共资源交易中心";
                            specType =   "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "汕尾市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            sqlCount++;
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int a = 0; a < aNode.Count; a++)
                                    {
                                        ImageTag img = aNode[a] as ImageTag;

                                        try
                                        {
                                            BaseAttach attach = ToolHtml.GetBaseAttach(img.GetAttribute("src"), prjName, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                            if (attach != null)
                                                ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                                        }
                                        catch { }
                                    }
                                }
                            }
                            if (!crawlAll && sqlCount >= this.MaxCount) return null;
                        }

                    }
                }
            }
            return null;
        }
    }
}
