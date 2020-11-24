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
    public class BidFuJianJsgc : WebSiteCrawller
    {
        public BidFuJianJsgc()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "福建省建设工程交易网中标信息";
            this.Description = "自动抓取福建省建设工程交易网中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.fjjsjy.com/Front/ZBProxy_List/zbgs";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "mvcPager")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 2].GetATagHref().GetReplace("/Front/ZBProxy_List/zbgs?page=");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?page=" + i);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "div_001")), true), new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellspacing", "1"))));
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
                        buildUnit = tr.Columns[1].GetATagValue("title");
                        beginDate = tr.Columns[3].ToPlainTextString().GetDateRegex("yyyy/MM/dd");
                        endDate = tr.Columns[4].ToPlainTextString().GetDateRegex("yyyy/MM/dd");
                        InfoUrl = "http://www.fjjsjy.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table5")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("<br/>,<br />,<br>", "\r\n").ToCtxString();
                            prjAddress = bidCtx.GetAddressRegex().GetCodeDel();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            parser = new Parser(new Lexer(HtmlTxt.ToLower()));
                            NodeList bidNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "span33")));
                            if (bidNode != null && bidNode.Count > 0)
                                bidUnit = bidNode[0].ToNodePlainString().GetReplace(" ");
                            parser = new Parser(new Lexer(HtmlTxt.ToLower()));
                            NodeList moneyNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "span34")));
                            if (moneyNode != null && moneyNode.Count > 0)
                                bidMoney = moneyNode[0].ToNodePlainString().GetMoney();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetBidRegex().GetReplace("A标,B标,C标,第一标段");
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("详见")
                                || bidUnit.Contains("/"))
                                bidUnit = string.Empty;
                            if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetMoneyRegex();

                            prjMgr = bidCtx.GetMgrRegex();
                            if (prjMgr.Contains("证书"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("证书"));
                            if (prjMgr.Contains("等级"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("等级"));
                            if (prjMgr.Contains("注册"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("注册"));
                            if (prjMgr.Contains("岗位"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("岗位"));
                            if (prjMgr.Contains("（"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("（"));
                            if (prjMgr.Contains("("))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("("));
                            if (prjMgr.Contains("证号"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("证号"));
                            if (prjMgr.Contains("、"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("、"));
                            if (prjMgr.Contains("项目"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("项目"));
                            if (prjMgr.Contains("中标"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("中标"));
                            if (prjMgr.Contains("综合"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("综合"));
                            if (prjMgr.Contains("勘察"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("勘察"));
                            if (prjMgr.Contains("福建"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("福建"));
                            if (prjMgr.Contains("工期"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("工期"));
                            if (prjMgr.Contains("执业"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("执业"));
                            if (prjMgr.Contains("闽")&&prjMgr.IsNumber())
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("闽"));

                            msgType = "福建省建设工程交易中心";
                            specType = bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("福建省", "福建省及地市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.fjjsjy.com/" + a.Link.GetReplace("../,./");
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
