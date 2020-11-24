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
using System.Web.Script.Serialization;

namespace Crawler.Instance
{
    public class BidHeYuanCGYZ : WebSiteCrawller
    {
        public BidHeYuanCGYZ()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "河源市公共资源交易中心中标摇珠";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取河源市公共资源交易中心中标摇珠";
            this.SiteUrl = "http://www.hyggzy.com/zfzbggxxyz/index.jhtml";
            this.MaxCount = 100;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch((new TagNameFilter("div")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode[31].ToNodePlainString().GetRegexBegEnd("/", "页");
                pageInt = int.Parse(temp);
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        int emp = i - 1;
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.hyggzy.com/zfzbggxxyz/index_" + emp + ".shtml");
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "list1")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty,
                            bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty,
                            beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty,
                            specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty,
                            bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty,
                            prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty,
                            area = string.Empty;
                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        InfoUrl = aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tab-cnt-item current")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString();
                            beginDate = beginDate.GetDateRegex();
                            if (string.IsNullOrWhiteSpace(beginDate))
                            {
                                beginDate = bidCtx.GetRegex("发布时间");
                                beginDate = beginDate.GetDateRegex();
                            }
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidMoney))
                            {
                                try
                                {
                                    bidMoney = bidCtx.GetRegex("第一标段中标金额");
                                    bidMoney = bidMoney.GetRegexBegEnd("￥", "元");
                                }
                                catch { }
                            }
                            buildUnit = bidCtx.GetBuildRegex();
                            if (string.IsNullOrWhiteSpace(buildUnit))
                                buildUnit = bidCtx.GetRegex("委托单位名称");
                            code = bidCtx.GetCodeRegex();

                            if (string.IsNullOrWhiteSpace(code))
                                code = bidCtx.GetRegexBegEnd("编号", "采购").GetReplace("\r\n", "");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                try
                                {
                                    parser = new Parser(new Lexer(htmldtl));
                                    NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content-cnt")), true), new TagNameFilter("table")));
                                    if (dtl != null && dtl.Count > 0)
                                    {
                                        TableTag dl = dtl[0] as TableTag;
                                        string bidCtxt = string.Empty;
                                        for (int c = 0; c < dl.Rows[0].ColumnCount; c++)
                                        {
                                            bidCtxt += dl.Rows[0].Columns[c].ToNodePlainString() + "：";
                                            bidCtxt += dl.Rows[1].Columns[c].ToNodePlainString() + "\r\n";
                                        }
                                        if (string.IsNullOrWhiteSpace(bidUnit))
                                            bidUnit = bidCtxt.GetRegex("第一中标人");
                                    }
                                }
                                catch { }
                            }
                            try
                            {
                                if (Convert.ToDecimal(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            msgType = "河源市公共资源交易中心";
                            specType = "政府采购";
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "河源市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.hyggzy.com" + a.Link;
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
