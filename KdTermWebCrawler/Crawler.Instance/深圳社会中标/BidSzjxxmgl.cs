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
    public class BidSzjxxmgl : WebSiteCrawller
    {
        public BidSzjxxmgl()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Title = "深圳市建星项目管理顾问有限公司";
            this.Description = "自动抓取深圳市建星项目管理顾问有限公司中标信息";
            this.SiteUrl = "http://www.sz-jstar.com/zbjy_list/pmcId=49&pageNo_FrontProducts_list01-1476959936861=1&pageSize_FrontProducts_list01-1476959936861=10.html";
            this.MaxCount = 100;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            //NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "number")));

            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "number"))), new TagNameFilter("a")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string  d = pageList[1].ToHtml();
                    d = d.Replace("(", "xu");
                    string temp = d.GetRegexBegEnd("xu", ",10");
                    pageInt = int.Parse(temp);
                }
                catch
                {
                    pageInt = 1;
                }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        string a = "http://www.sz-jstar.com/zbjy_list/pmcId=49&pageNo_FrontProducts_list01-1476959936861=" + i + "&pageSize_FrontProducts_list01-1476959936861=10.html";
                        html = this.ToolWebSite.GetHtmlByUrl(a, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "list")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        code = tr.Columns[1].ToNodePlainString();
                        prjName = tr.Columns[2].ToNodePlainString();
                        prjName = prjName.GetBidPrjName();
                        if (prjName.Contains("招标编号"))
                        {
                            if (prjName.IndexOf("（") != -1)
                            {
                                prjName = prjName.Remove(prjName.IndexOf("（"));
                            }
                            else if (prjName.IndexOf("(") != -1)
                            {
                                prjName = prjName.Remove(prjName.IndexOf("("));
                            }
                            else
                            {
                                prjName = prjName.Remove(prjName.IndexOf("招标编号"));
                            }
                        }
                        bidUnit = tr.Columns[3].ToNodePlainString();
                        bidUnit = bidUnit.Replace("类型：", "");
                        string b = tr.Columns[2].GetATagHref();
                        InfoUrl = "http://www.sz-jstar.com" + tr.Columns[2].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        //NodeList dtList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                        //if (dtList != null && dtList.Count > 0)
                        //{
                        // HtmlTxt = dtList[0].ToHtml();
                        bidCtx = htldtl.ToCtxString().Replace("&ldquo;", "");
                        prjAddress = bidCtx.GetAddressRegex();
                        buildUnit = bidCtx.GetBuildRegex();
                        bidType = prjName.GetInviteBidType();
                        bidMoney = bidCtx.GetMoneyRegex();
                        if (bidMoney == "0")
                        {
                            bidMoney = bidCtx.GetMoneyRegex(new string[] { "（￥", "$" });
                        }
                        if (bidMoney == "0")
                        {
                            bidMoney = bidCtx.GetMoneyRegex(new string[] { "￥", "$" }, false, "万元");
                        }
                        msgType = "深圳市建星项目管理顾问有限公司";
                        specType = "其他";
                        BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        list.Add(info);
                        parser = new Parser(new Lexer(HtmlTxt));
                        NodeList aList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        if (aList != null && aList.Count > 0)
                        {
                            for (int c = 0; c < aList.Count; c++)
                            {
                                ATag a = aList[c] as ATag;
                                if (a.Link.IsAtagAttach())
                                {
                                    BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, a.Link);
                                    base.AttachList.Add(attach);
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
