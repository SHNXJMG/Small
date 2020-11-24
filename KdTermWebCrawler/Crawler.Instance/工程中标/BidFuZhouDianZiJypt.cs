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
    public class BidFuZhouDianZiJypt : WebSiteCrawller
    {
        public BidFuZhouDianZiJypt()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "福州建设工程电子招投标中标信息";
            this.Description = "自动抓取福州建设工程电子招投标中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.fzztb.com/bidding/project/listNotices.shtml?type=biddingPitchons";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "td")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString().GetRegexBegEnd("录共", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", " FCK__ShowTableBorders")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
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

                           code = tr.Columns[1].ToNodePlainString();
                        ATag aTag = tr.Columns[3].GetATag();
                        prjName = aTag.LinkText;
                        if (prjName.Contains(".."))
                            prjName = aTag.GetAttribute("title");
                        beginDate = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.fzztb.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("<br/>,<br />,<br>", "\r\n").ToCtxString();
                            prjAddress = bidCtx.GetAddressRegex().GetCodeDel();
                            buildUnit = bidCtx.GetBuildRegex();
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            bidUnit = bidCtx.GetBidRegex().GetReplace("A标,B标,C标,第一标段");
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("详见")
                                || bidUnit.Contains("/"))
                                bidUnit = string.Empty;
                            bidMoney = bidCtx.GetMoneyRegex();
                            try
                            {
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
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
                            if (prjMgr.Contains("闽") && prjMgr.IsNumber())
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("闽"));
                            if (prjMgr.Contains(":"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf(":"));
                            if (prjMgr.Contains("："))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("："));

                            msgType = "福州市城乡建设委员会";
                            specType = bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("福建省", "福建省及地市", "福州市", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.fzztb.com" + a.Link.GetReplace("../,./");
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
