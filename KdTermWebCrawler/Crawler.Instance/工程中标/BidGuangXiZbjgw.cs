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
    public class BidGuangXiZbjgw : WebSiteCrawller
    {
        public BidGuangXiZbjgw()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广西壮族自治区发展和改革委员会中标候选人及中标信息";
            this.Description = "自动抓取广西壮族自治区发展和改革委员会中标候选人及中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://ztb.gxi.gov.cn/ztbgg/zbhxrgs/";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            List<BidInfo> list = new List<BidInfo>();

            List<BidInfo> zbgss = AddZbgs();
            if (zbgss != null && zbgss.Count > 0)
                list.AddRange(zbgss);
            List<BidInfo> zbhxrgss = AddZbhxrgx();
            if (zbhxrgss != null && zbhxrgss.Count > 0)
                list.AddRange(zbhxrgss);
            return list as IList;
        }

        protected List<BidInfo> AddZbgs()
        {
            string url = "http://ztb.gxi.gov.cn/ztbgg/zbgs/";
            List<BidInfo> list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(url, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "pl")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().Replace("(", "kdxx").Replace(",", "xxdk").GetRegexBegEnd("kdxx", "xxdk");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(url + "index_" + i + ".htm", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("id", "OutlineContent")), true), new TagNameFilter("table")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {

                        string prjName = string.Empty,
                             buildUnit = string.Empty, bidUnit = string.Empty,
                             bidMoney = string.Empty, code = string.Empty,
                             bidDate = string.Empty, beginDate = string.Empty,
                             endDate = string.Empty, bidType = string.Empty,
                             specType = string.Empty, InfoUrl = string.Empty,
                             msgType = string.Empty, bidCtx = string.Empty,
                             prjAddress = string.Empty, remark = string.Empty,
                             prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = tr.Columns[1].ToPlainTextString().GetDateRegex();

                        InfoUrl = "http://ztb.gxi.gov.cn/ztbgg/zbgs/" + aTag.Link.GetReplace("./");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "p1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoNormalTable")));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                string ctx = string.Empty;
                                TableTag dtlTable = tableNode[0] as TableTag;
                                for (int r = 0; r < dtlTable.RowCount; r++)
                                {
                                    for (int c = 0; c < dtlTable.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = dtlTable.Rows[r].Columns[c].ToNodePlainString();
                                        if (c % 2 == 0)
                                            ctx += temp + "：";
                                        else
                                            ctx += temp + "\r\n";
                                    }
                                }
                          
                                string projectName = ctx.GetRegex("项目名称,工程名称");
                                if (!string.IsNullOrWhiteSpace(projectName))
                                    prjName = projectName;
                                code = ctx.GetCodeRegex().GetCodeDel();

                                bidUnit = ctx.GetBidRegex();
                                if (bidUnit.Contains("单位名称") || string.IsNullOrWhiteSpace(bidUnit))
                                    bidUnit = ctx.GetRegex("单位名称");
                                bidMoney = ctx.GetMoneyRegex(null, false, "万元");

                                prjMgr = ctx.GetMgrRegex();
                                buildUnit = ctx.GetBuildRegex();
                                if (bidUnit.IsNumber())
                                {
                                    if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                        bidMoney = bidUnit;
                                    bidUnit = ctx.GetRegex("单位名称");
                                }
                            }
                            else
                            {
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex();
                                prjMgr = bidCtx.GetMgrRegex();
                                buildUnit = bidCtx.GetBuildRegex();
                                code = bidCtx.GetCodeRegex().GetCodeDel();
                            }

                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.LastIndexOf("公司")) + "公司";
                            if (bidUnit.Contains("确定为"))
                                bidUnit = bidUnit.Remove(0, bidUnit.IndexOf("确定为")).Replace("确定为", "");
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (prjMgr.Contains("（"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("（"));
                            if (prjMgr.Contains("("))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("("));
                            if (prjMgr.Contains("项目总工"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("项目总工"));
                            if (prjMgr.Contains("注册"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("注册"));

                            try
                            {
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                                if (decimal.Parse(bidMoney) < 1)
                                    bidMoney = "0";
                            }
                            catch { }
                            bidUnit = bidUnit.Replace("　", "");
                            prjMgr = prjMgr.Replace("　", "");
                            if (bidUnit.Contains("中标价"))
                                bidUnit = "";
                            specType = "政府采购";
                            bidType = prjName.GetInviteBidType();
                            msgType = "广西壮族自治区发展和改革委员会";
                            BidInfo info = ToolDb.GenBidInfo("广西壮族自治区", "广西壮族自治区及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://ztb.gxi.gov.cn/" + a.Link.GetReplace("../,./");
                                        if (Encoding.Default.GetByteCount(link) > 500)
                                            continue;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }

            return list; 
        }

        protected List<BidInfo> AddZbhxrgx()
        {
            string url = "http://ztb.gxi.gov.cn/ztbgg/zbhxrgs/";
            List<BidInfo> list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(url, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "pl")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().Replace("(", "kdxx").Replace(",", "xxdk").GetRegexBegEnd("kdxx", "xxdk");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(url + "index_" + i + ".htm", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("id", "OutlineContent")), true), new TagNameFilter("table")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {

                        string prjName = string.Empty,
                             buildUnit = string.Empty, bidUnit = string.Empty,
                             bidMoney = string.Empty, code = string.Empty,
                             bidDate = string.Empty, beginDate = string.Empty,
                             endDate = string.Empty, bidType = string.Empty,
                             specType = string.Empty, InfoUrl = string.Empty,
                             msgType = string.Empty, bidCtx = string.Empty,
                             prjAddress = string.Empty, remark = string.Empty,
                             prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = tr.Columns[1].ToPlainTextString().GetDateRegex();

                        InfoUrl = "http://ztb.gxi.gov.cn/ztbgg/zbhxrgs/" + aTag.Link.GetReplace("./");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "p1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoNormalTable")));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                string ctx = string.Empty;
                                TableTag dtlTable = tableNode[0] as TableTag;
                                for (int r = 0; r < dtlTable.RowCount; r++)
                                {
                                    for (int c = 0; c < dtlTable.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = dtlTable.Rows[r].Columns[c].ToNodePlainString();
                                        if (c % 2 == 0)
                                            ctx += temp + "：";
                                        else
                                            ctx += temp + "\r\n";
                                    }
                                }
                                string projectName = ctx.GetRegex("项目名称,工程名称");
                                if (!string.IsNullOrWhiteSpace(projectName))
                                    prjName = projectName;
                                code = ctx.GetCodeRegex().GetCodeDel();

                                bidUnit = ctx.GetBidRegex();
                                if (bidUnit.Contains("单位名称") || string.IsNullOrWhiteSpace(bidUnit))
                                    bidUnit = ctx.GetRegex("单位名称");
                                bidMoney = ctx.GetMoneyRegex(null, false, "万元");

                                prjMgr = ctx.GetMgrRegex();
                                buildUnit = ctx.GetBuildRegex();
                                if (bidUnit.IsNumber())
                                {
                                    if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                        bidMoney = bidUnit;
                                    bidUnit = ctx.GetRegex("单位名称");
                                }
                            }
                            else
                            {
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex();
                                prjMgr = bidCtx.GetMgrRegex();
                                buildUnit = bidCtx.GetBuildRegex();
                                code = bidCtx.GetCodeRegex().GetCodeDel();
                            }

                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.LastIndexOf("公司")) + "公司";
                            if (bidUnit.Contains("确定为"))
                                bidUnit = bidUnit.Remove(0, bidUnit.IndexOf("确定为")).Replace("确定为", "");
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (prjMgr.Contains("（"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("（"));
                            if (prjMgr.Contains("("))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("("));
                            if (prjMgr.Contains("项目总工"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("项目总工"));
                            if (prjMgr.Contains("注册"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("注册"));

                            try
                            {
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                                if (decimal.Parse(bidMoney) < 1)
                                    bidMoney = "0";
                            }
                            catch { }
                            bidUnit = bidUnit.Replace("　", "");
                            prjMgr = prjMgr.Replace("　", "");
                            if (bidUnit.Contains("中标价"))
                                bidUnit = "";
                            specType = "政府采购";
                            bidType = prjName.GetInviteBidType();
                            msgType = "广西壮族自治区发展和改革委员会";
                            BidInfo info = ToolDb.GenBidInfo("广西壮族自治区", "广西壮族自治区及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://ztb.gxi.gov.cn/" + a.Link.GetReplace("../,./");
                                        if (Encoding.Default.GetByteCount(link) > 500)
                                            continue;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }

            return list;
        }
    }
}
