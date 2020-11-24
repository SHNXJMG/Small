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
    public class BidAnHuiZtb : WebSiteCrawller
    {
        public BidAnHuiZtb()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "安徽省发展和改革委员会中标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取安徽省发展和改革委员会中标信息";
            this.SiteUrl = "http://www.ahtba.org.cn/Notice/AnhuiNoticeSearch?spid=714&scid=600";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagination f_right")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].GetATagValue("onclick").GetRegexBegEnd("Info", ",").GetReplace("(");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&pageSize=15&pageNum=" + i);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "newsList")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
     bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;
                        area = node.ToNodePlainString().GetReplace("[", "【").GetReplace("]", "】").GetRegexBegEnd("【", "】");
                        prjName = aTag.GetAttribute("title");
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.ahtba.org.cn" + aTag.Link.GetReplace("amp;");
                        string id = aTag.Link.Substring(aTag.Link.IndexOf("id="), aTag.Link.Length - aTag.Link.IndexOf("id=")).GetReplace("id=");
                        string htmldtl = string.Empty;
                        try
                        {
                            NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                            "id"
                            }, new string[]{
                             id
                            });
                            htmldtl = this.ToolWebSite.GetHtmlByUrl("http://www.ahtba.org.cn/Notice/NoticeContent", nvc).GetJsString();
                        }
                        catch { }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "new_detail")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,<br/>,<br>","\r\n").ToCtxString();
                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();

                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("中标候选人名称,中签单位,第一成交候选人,成交候选人");
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                bidMoney = bidCtx.GetMoneyRegex(null,true);
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                bidMoney = bidCtx.GetRegex("总额").GetMoney();
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    for (int t = 0; t < tableNode.Count; t++)
                                    {
                                        TableTag tag = tableNode[t] as TableTag;
                                        string classStr =tag.GetAttribute("class");
                                        if (!string.IsNullOrEmpty(classStr) && classStr.ToLower().Contains("table_detail")) continue;

                                        string ctx = string.Empty; 
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
                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrEmpty(bidUnit))
                                            bidUnit = ctx.GetRegex("成交候选人,中标单位名称,第一中标候选人,第一成交候选人");
                                        if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                            bidMoney = ctx.GetMoneyRegex();
                                        if (string.IsNullOrEmpty(prjMgr))
                                            prjMgr = ctx.GetMgrRegex();
                                        if (string.IsNullOrEmpty(prjMgr))
                                            prjMgr = ctx.GetRegex("拟任总监,拟任项目经理");

                                        if (!bidUnit.Contains("公司"))
                                        {
                                            ctx = string.Empty;
                                            try
                                            {
                                                for (int r = 1; r < tag.Rows[4].ColumnCount; r++)
                                                {
                                                    string temp = tag.Rows[4].Columns[r].ToNodePlainString().GetReplace(":,：");
                                                    ctx += temp + "：";
                                                    ctx += tag.Rows[5].Columns[r].ToNodePlainString().GetReplace(":,：") + "\r\n";
                                                }
                                                bidUnit = ctx.GetBidRegex(null, true, 200);
                                                if (string.IsNullOrEmpty(bidUnit))
                                                    bidUnit = ctx.GetRegex("成交候选人,中标单位名称,第一中标候选人,第一成交候选人");
                                                if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                                    bidMoney = ctx.GetMoneyRegex();
                                                if (string.IsNullOrEmpty(prjMgr))
                                                    prjMgr = ctx.GetMgrRegex();
                                                if (string.IsNullOrEmpty(prjMgr))
                                                    prjMgr = ctx.GetRegex("拟任总监,拟任项目经理");
                                            }
                                            catch { }
                                        }
                                    }
                                    
                                }
                            }
                            try
                            {
                                if (Convert.ToDecimal(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            if (prjMgr.Contains("联系"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("联系"));
                            if (prjMgr.Contains("电话"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("电话"));
                            if (prjMgr.Contains("2"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("2"));
                            if (prjMgr.Contains("（"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("（"));
                            if (prjMgr.Contains("("))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("("));
                            if (prjMgr.Contains("二"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("二"));
                            if (prjMgr.Contains("注册"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("注册"));
                            if (prjMgr.Contains("业绩"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("业绩"));
                            if (prjMgr.Contains("I"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("I"));
                            if (prjMgr.Contains("投标") || prjMgr.IsNumber())
                                prjMgr = "";
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            bidUnit = bidUnit.GetReplace("名称,1,、I标段");
                            prjMgr = prjMgr.GetReplace("1,、,一,第一中标人,第一中标,第中标人,第名,I标段,第中标候选人,标段").GetCodeDel();
                            specType = bidType = "建设工程";
                            msgType = "安徽省发展和改革委员会";
                            BidInfo info = ToolDb.GenBidInfo("安徽省", "安徽省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
