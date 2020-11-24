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
    public class BidShanDongJsgc : WebSiteCrawller
    {
        public BidShanDongJsgc()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "山东省建设工程招投标管理信息网中标信息";
            this.Description = "自动抓取山东省建设工程招投标管理信息网中标信息";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.sdzb.gov.cn/ztbZbTzs.aspx";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter( new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "AspNetPager1")),true),new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].GetATagHref().GetRegexBegEnd(",'", "'");
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
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "ScriptManager1",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__LASTFOCUS",
                    "__VIEWSTATE",
                    "__VIEWSTATEGENERATOR",
                    "__EVENTVALIDATION"
                    }, new string[]{
                    "UpdatePanel1|AspNetPager1",
                    "AspNetPager1",
                    i.ToString(),
                    "",
                    viewState,
                    "89EEEF6E",
                    eventValidation
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
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

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();
                        prjName = aTag.GetAttribute("title");
                        code = tr.Columns[1].ToNodePlainString();
                        bidUnit = tr.Columns[2].GetSpan().GetAttribute("title");
                        prjMgr = tr.Columns[3].ToNodePlainString();
                        bidMoney = tr.Columns[4].ToNodePlainString().GetMoney();
                        beginDate = tr.Columns[5].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.sdzb.gov.cn/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "htmlTable")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();

                            prjAddress = bidCtx.GetAddressRegex().GetCodeDel().GetReplace("　");
                            buildUnit = bidCtx.GetBuildRegex().GetReplace("　");
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (buildUnit.Contains("_"))
                                buildUnit = string.Empty;
                            if (prjMgr == "/")
                                prjMgr = string.Empty;
                            msgType = "山东省建设工程招标投标管理办公室";
                            specType = bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("山东省", "山东省及地市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.sdzb.gov.cn/" + a.Link.GetReplace("../,./");
                                        if (Encoding.Default.GetByteCount(link) > 500)
                                            continue;
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
