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
    public class BidYunNanZtb:WebSiteCrawller
    {
        public BidYunNanZtb()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "云南省招投标信息网中标信息";
            this.Description = "自动抓取云南省招投标信息网中标信息";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.ynbidding.net/classlist.aspx?no-cache=0.7657945008653804&id=032430863326&id=://www.ynbidding.net/list&page=1&_=";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagebox")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("共", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.ynbidding.net/classlist.aspx?no-cache=0.7657945008653804&id=032430863326&id=://www.ynbidding.net/list&page=" + i + "&_=",Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.GetATag();
                        if (aTag == null) continue;
                        prjName = aTag.LinkText;
                        beginDate = tr.Columns[0].ToNodePlainString().GetDateRegex("yyyy/MM/dd");
                        InfoUrl = "http://www.ynbidding.net" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "Content")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            code = bidCtx.GetCodeRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("第一候选供应商");
                            if (bidUnit.Contains("第一"))
                                bidUnit = string.Empty;

                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList bidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (bidNode != null && bidNode.Count > 1)
                                {
                                    string ctx = string.Empty;
                                    TableTag tag = bidNode[1] as TableTag;
                                    if (tag.RowCount > 1)
                                    {
                                        for (int c = 0; c < tag.Rows[0].ColumnCount; c++)
                                        {
                                            try
                                            {
                                                ctx += tag.Rows[0].Columns[c].ToNodePlainString().GetReplace("sourcefromwww.ynbidding.net") + "：";
                                                ctx += tag.Rows[1].Columns[c].ToNodePlainString().GetReplace("sourcefromwww.ynbidding.net") + "\r\n";
                                            }
                                            catch { }
                                        }
                                    }
                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrWhiteSpace(bidUnit))
                                        bidUnit = ctx.GetRegex("投标单位,单位名称,投标人名称");
                                    if (bidMoney == "0" || string.IsNullOrWhiteSpace(bidMoney))
                                        bidMoney = ctx.GetMoneyRegex();
                                    if (bidMoney == "0" || string.IsNullOrWhiteSpace(bidMoney))
                                        bidMoney = ctx.GetMoneyString().GetMoney();
                                }
                            }

                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("第一"))
                                bidUnit = string.Empty;
                            bidType = prjName.GetInviteBidType();
                            specType = "建设工程";
                            msgType = "云南省发展和改革委员会"; 
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
                                            link = "http://www.ynbidding.net/" + a.Link;
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
