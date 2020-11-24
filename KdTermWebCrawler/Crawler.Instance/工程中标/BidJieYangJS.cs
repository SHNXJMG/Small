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

namespace Crawler.Instance
{
    public class BidJieYangJS : WebSiteCrawller
    {
        public BidJieYangJS()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省揭阳市建设工程中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.Description = "自动抓取广东省揭阳市建设工程中标信息";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.jysggzy.com/TPFront/jsgc/004003/";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("a"), new HasAttributeFilter("class", "wb-page-default wb-page-number wb-page-family")));
            if (nodeList != null && nodeList.Count > 0)
            {
                try
                {
                    string temp = nodeList.AsString().Replace("1/", "");
                    page = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {

                        htl = this.ToolWebSite.GetHtmlByUrl("http://www.jysggzy.com/TPFront/jsgc/004003/?pageing=" + i, Encoding.Default);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "ewb-data-items ewb-pt6")), true), new TagNameFilter("li")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    for (int j = 0; j < tableNodeList.Count; j++)
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

                        prjName = tableNodeList[j].ToNodePlainString().Replace(" ", "");
                        beginDate = tableNodeList[j].ToPlainTextString().GetDateRegex();
                        if (!string.IsNullOrEmpty(beginDate))
                            prjName = prjName.Replace(beginDate, "");
                        InfoUrl = "http://www.jysggzy.com/" + tableNodeList[j].GetATagHref();
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToLower().Replace("<br>", "\r\n").Replace("</br>", "\r\n").Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace("</ br>", "\r\n").ToCtxString();
                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetRegexBegEnd("同意", "为该项目的第一中标候选人");
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegexBegEnd("推荐", "为第一中标候选人");
                            bidMoney = bidCtx.GetRegexBegEnd("投标报价为", "；").GetMoney();
                            if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetRegexBegEnd("投标报价", "；").GetMoney();
                            prjAddress = bidCtx.GetAddressRegex();
                            code = bidCtx.GetCodeRegex();
                            try
                            {
                                if (string.IsNullOrWhiteSpace(bidUnit))
                                {
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                    if (tableNode != null && tableNode.Count > 0)
                                    {
                                        string ctx = string.Empty;
                                        TableTag tag = tableNode[0] as TableTag;
                                        for (int r = 0; r < tag.RowCount; r++)
                                        {
                                            for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                            {
                                                string temp = tag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                                if ((c + 1) % 2 == 0)
                                                {
                                                    ctx += temp + "\r\n";
                                                }
                                                else
                                                    ctx += temp + "：";
                                            }

                                        }
                                        bidUnit = ctx.GetRegex("第一中标候选人");
                                        buildUnit = ctx.GetRegex("项目单位");
                                        if (string.IsNullOrWhiteSpace(prjAddress))
                                            prjAddress = ctx.GetRegex("项目所在地区");
                                        if (string.IsNullOrWhiteSpace(buildUnit))
                                            buildUnit = ctx.GetBuildRegex();
                                        if (string.IsNullOrWhiteSpace(prjMgr))
                                            prjMgr = ctx.GetRegex("项目经理");
                                        if (string.IsNullOrWhiteSpace(prjMgr))
                                            prjMgr = ctx.GetRegex("总监理工程师");
                                        try
                                        {
                                            if (string.IsNullOrWhiteSpace(bidUnit))
                                            {
                                                parser = new Parser(new Lexer(HtmlTxt));
                                                NodeList tablese = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                                if (tablese != null && tablese.Count > 0)
                                                {
                                                    string ctxx = string.Empty;
                                                    TableTag tagx = tablese[1] as TableTag;
                                                    for (int r = 0; r < tagx.RowCount; r++)
                                                    {
                                                        for (int c = 0; c < tagx.Rows[r].ColumnCount; c++)
                                                        {
                                                            string temp = tagx.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                                            if ((c + 1) % 2 == 0)
                                                            {
                                                                ctxx += temp + "\r\n";
                                                            }
                                                            else
                                                                ctxx += temp + "：";
                                                        }
                                                    }
                                                    bidMoney = ctxx.GetMoneyRegex();
                                                    if (string.IsNullOrWhiteSpace(prjMgr))
                                                        prjMgr = ctxx.GetRegex("总监理工程师");
                                                }
                                            }
                                        }
                                        catch { }
                                        try
                                        {
                                            if (string.IsNullOrWhiteSpace(bidUnit))
                                            {
                                                if (string.IsNullOrWhiteSpace(bidUnit))
                                                {
                                                    parser = new Parser(new Lexer(HtmlTxt));
                                                    NodeList tableNodex = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                                    if (tableNodex != null && tableNodex.Count > 0)
                                                    {

                                                        TableTag table = tableNodex[1] as TableTag;

                                                        string ctxxx = string.Empty;
                                                        for (int r = 0; r < table.Rows[0].ColumnCount; r++)
                                                        {
                                                            try
                                                            {
                                                                ctxxx += table.Rows[0].Columns[r].ToNodePlainString() + "：";
                                                                ctxxx += table.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                                            }
                                                            catch { }
                                                        }

                                                        if (string.IsNullOrEmpty(bidUnit))
                                                            bidUnit = ctxxx.GetRegex("第一中标候选人");
                                                        if (string.IsNullOrWhiteSpace(bidUnit))
                                                            bidUnit = ctxxx.GetRegex("第一候选人");
                                                    }
                                                }
                                            }
                                        }
                                        catch { }
                                        try
                                        {
                                            if (string.IsNullOrWhiteSpace(bidUnit))
                                            {
                                                if (string.IsNullOrWhiteSpace(bidUnit))
                                                {
                                                    parser = new Parser(new Lexer(HtmlTxt));
                                                    NodeList tableNodexa = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                                    if (tableNodexa != null && tableNodexa.Count > 0)
                                                    {

                                                        TableTag tablea = tableNodexa[0] as TableTag;

                                                        string ctxxxa = string.Empty;
                                                        for (int r = 0; r < tablea.Rows[0].ColumnCount; r++)
                                                        {
                                                            try
                                                            {
                                                                ctxxxa += tablea.Rows[0].Columns[r].ToNodePlainString() + "：";
                                                                ctxxxa += tablea.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                                            }
                                                            catch { }
                                                        }

                                                        if (string.IsNullOrEmpty(bidUnit))
                                                            bidUnit = ctxxxa.GetRegex("第一中标候选人");
                                                        if (string.IsNullOrWhiteSpace(bidUnit))
                                                            bidUnit = ctxxxa.GetRegex("第一候选人");


                                                    }
                                                }
                                            }
                                        }
                                        catch { }
                                        if (string.IsNullOrWhiteSpace(bidMoney))
                                            bidMoney = ctx.GetMoneyRegex();
                                        if (string.IsNullOrWhiteSpace(prjMgr))
                                            prjMgr = ctx.GetMgrRegex();
                                    }
                                }
                            }
                            catch  { }
                            if (code.Contains("）"))
                            {
                                code = code.Remove(code.IndexOf("）"));
                            }

                            if (!string.IsNullOrEmpty(bidMoney) && bidMoney != "0")
                            {
                                if (decimal.Parse(bidMoney) > 10000)
                                {
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                                }
                            }
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (prjMgr.Contains("/"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                            if (prjMgr.Contains("1"))
                                prjMgr = "";
                            msgType = "揭阳市建设工程交易中心";
                            specType = "建设工程";
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "揭阳市区", "", string.Empty, code, prjName, buildUnit, beginDate,
                                  bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                  bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.gdzbtb.gov.cn/" + a.Link;

                                        if (Encoding.Default.GetByteCount(link) <= 500)
                                        {
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
            }
            return list;
        }
    }
}
