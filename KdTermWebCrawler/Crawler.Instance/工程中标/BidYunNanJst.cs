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
    public class BidYunNanJst : WebSiteCrawller
    {
        public BidYunNanJst()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "云南省建设工程招投标监督管理网中标信息";
            this.Description = "自动抓取云南省建设工程招投标监督管理网中标信息";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.ynzb.com.cn/Project_ConfirmContractor.aspx";
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
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "LblPageCount")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString();
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
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "TBuildInc",
                    "TFindContractorName",
                    "SArea",
                    "SCCSort",
                    "txtGO",
                    "__EVENTVALIDATION"
                    }, new string[]{
                    "lbtnGO",
                    "",
                    viewState,
                    "","",
                    "0",
                    "",
                    i.ToString(),
                    eventValidation
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "gv_List")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,  bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        TableRow tr = table.Rows[j];
                        buildUnit = tr.Columns[1].ToNodePlainString();
                        if (buildUnit.Contains(".."))
                        {
                            Span builSpan = tr.Columns[1].GetSpan();
                            buildUnit = builSpan.GetAttribute("title");
                        }
                        ATag aTag = tr.Columns[2].GetATag();
                        prjName = aTag.GetAttribute("title");
                        bidUnit = tr.Columns[3].ToNodePlainString();
                        if (bidUnit.Contains(".."))
                        {
                            Span bidSpan = tr.Columns[3].GetSpan();
                            bidUnit = bidSpan.GetAttribute("title");
                        }
                        beginDate = tr.Columns[4].ToNodePlainString().GetDateRegex();
                        area  = tr.Columns[5].ToNodePlainString();
                        InfoUrl = "http://www.ynzb.com.cn/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = ToolHtml.GetHtmlByUrl(this.SiteUrl, InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellspacing", "1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            TableTag tag = dtlNode[0] as TableTag;
                            for (int r = 0; r < tag.RowCount; r++)
                            {
                                if (r == 0)
                                {
                                    bidCtx += tag.Rows[r].Columns[0].ToNodePlainString() + "\r\n";
                                    continue;
                                }
                                for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                {
                                    string temp = tag.Rows[r].Columns[c].ToNodePlainString();
                                    if ((c + 1) % 2 == 0)
                                        bidCtx += temp + "\r\n";
                                    else
                                        bidCtx += temp + "：";
                                }
                            }

                            prjAddress = bidCtx.GetAddressRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            if (prjMgr.Contains("/") || prjMgr.Contains("-"))
                                prjMgr = string.Empty;
                            bidMoney = bidCtx.GetMoneyRegex();
                            code = bidCtx.GetCodeRegex();
                            specType = "建设工程";
                            msgType = "云南省住房和城乡建设厅";
                            BidInfo info = ToolDb.GenBidInfo("云南省", "云南省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.ynzb.com.cn/" + a.Link;
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
