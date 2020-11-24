using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using Crawler;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Util;

namespace Crawler.Instance
{
    public class BidZheJiangJsgc : WebSiteCrawller
    {
        public BidZheJiangJsgc()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "浙江省公共资源交易中心中标信息";
            this.Description = "自动抓取浙江省公共资源交易中心中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.zmctc.com/zjgcjy/Notice/tblOSInfoMore.aspx";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "TblOSInfoList1_Pager")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString().GetRegexBegEnd("总页数：", "当前");
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
                    "__VIEWSTATE",
                    "TblOSInfoList1:KeyWord",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT" 
                    }, new string[]{
                    viewState,
                    "",
                    "TblOSInfoList1:Pager",
                    i.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "TblOSInfoList1_DataGrid1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
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
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.GetAttribute("title").GetReplace(";");
                        code = tr.Columns[1].ToNodePlainString().GetReplace("[", "【").GetReplace("]", "】").GetRegexBegEnd("【", "】");
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        if (!aTag.Link.Contains("http"))
                        {
                            string f = aTag.Link;
                            InfoUrl = "http://www.zmctc.com/zjgcjy/Notice/" + aTag.Link;
                        }
                        else
                            InfoUrl = aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                            if(htmldtl.Contains("�"))
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "news")));
                        if (dtlNode == null || dtlNode.Count < 1)
                        {
                            parser.Reset();
                            dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "olds")));
                        }
                        if (dtlNode == null || dtlNode.Count < 1)
                        {
                            parser.Reset();
                            dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                        }
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Pbgginfodetailnew1_company")));
                            if (tableNode == null || tableNode.Count < 1)
                            {
                                parser.Reset();
                                tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "PbggInfoDetail1_company")));
                            }
                            if (tableNode == null || tableNode.Count < 1)
                            {
                                parser.Reset();
                                tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("rules", "all")));
                            }
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag tag = tableNode[0] as TableTag;
                                string ctx = string.Empty;
                                for (int r = 0; r < tag.Rows[0].ColumnCount; r++)
                                {
                                    try
                                    {
                                        ctx += tag.Rows[0].Columns[r].ToNodePlainString() + "：";
                                        ctx += tag.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                    }
                                    catch { }
                                }
                                if (!ctx.Contains("否决投标"))
                                {
                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("中标候选人");
                                    bidMoney = ctx.GetMoneyRegex();
                                    prjMgr = ctx.GetMgrRegex();
                                    if (string.IsNullOrEmpty(prjMgr))
                                        prjMgr = ctx.GetRegex("项目经理/总监");
                                }
                            }
                            msgType = "浙江省公共资源交易中心";
                            specType = "政府采购";
                            bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("浙江省", "浙江省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://downc.zmctc.com/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                           
                        } 
                        if (!crawlAll && list.Count >= this.MaxCount) return list;

                    }
                }
            }
            return list;
        }
    }
}
