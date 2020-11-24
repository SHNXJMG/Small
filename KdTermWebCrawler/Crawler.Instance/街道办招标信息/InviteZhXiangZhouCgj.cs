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
    public class InviteZhXiangZhouCgj : WebSiteCrawller
    {
        public InviteZhXiangZhouCgj()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省香洲区城管局信息招标、中标公告";
            this.Description = "自动抓取广东省香洲区城管局信息招标、中标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://cgj.zhxz.cn/cgdt/tztg/";
            this.MaxCount = 100;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pages")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode.AsString();
                try
                {
                    pageInt = int.Parse(temp.GetRegexBegEnd("HTML", ",").GetReplace("("));
                }
                catch { }
            }
            for (int i = 0; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "index_" + i + ".html");
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "art_list")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        ATag aTag = listNode[j].GetATag();
                        string beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        string infoUrl = "http://cgj.zhxz.cn/cgdt/tztg/" + aTag.Link.GetReplace("./");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList titleNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("h1"));
                        string prjName = titleNode[0].ToNodePlainString();
                        if (prjName.Contains("中标") || prjName.Contains("成交"))
                        {
                            string buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            parser.Reset();
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "art_info")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.AsHtml().ToLower();
                                bidCtx = HtmlTxt.GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    TableTag tag = tableNode[0] as TableTag;
                                    if (tag.RowCount > 1)
                                    {
                                        string ctx = string.Empty;
                                        try
                                        {
                                            for (int r = 0; r < tag.Rows[0].ColumnCount; r++)
                                            {
                                                ctx += tag.Rows[0].Columns[r].ToNodePlainString() + "：";
                                                ctx += tag.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                            }
                                        }
                                        catch { }
                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrEmpty(bidUnit))
                                            bidUnit = ctx.GetRegex("投标单位");
                                        if (!string.IsNullOrEmpty(bidUnit))
                                            bidMoney = ctx.GetMoneyRegex();
                                        prjMgr = ctx.GetMgrRegex();
                                    }
                                }
                                else
                                {
                                    bidUnit = bidCtx.GetBidRegex();
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = bidCtx.GetRegex("投标人名称,成交供应商");
                                    bidMoney = bidCtx.GetMoneyRegex(new string[] { "成交价" });
                                    if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                        bidMoney = bidCtx.GetMoneyRegex();
                                    prjMgr = bidCtx.GetMgrRegex();
                                }
                                if (bidUnit.Contains("中标价"))
                                {
                                    bidMoney = "0";
                                    bidUnit = "";
                                }
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = bidCtx.GetBidRegex();
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = bidCtx.GetRegex("投标人名称,成交供应商");
                                if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                    bidMoney = bidCtx.GetMoneyRegex(new string[] { "成交价", "中标价","中标金额" }, false, "万元");
                                if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                    bidMoney = bidCtx.GetMoneyRegex(null,false,"万元");
                                buildUnit = bidCtx.GetBuildRegex();

                                prjMgr = bidCtx.GetMgrRegex();
                                code = bidCtx.GetCodeRegex().GetCodeDel();
                                try
                                {
                                    if (decimal.Parse(bidMoney) < 1)
                                        bidMoney = "0";
                                    if (decimal.Parse(bidMoney) > 100000)
                                        bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                                }
                                catch { }

                                specType = "政府采购";
                                bidType = prjName.GetInviteBidType();
                                msgType = "珠海市香洲城市管理局";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "珠海市区", "香洲区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, infoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag a = aNode[k].GetATag();
                                        if (a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = a.Link;
                                            else
                                                link = "http://cgj.zhxz.cn/" + a.Link;
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                        else
                        {
                            string code = string.Empty, buildUnit = string.Empty,
                           prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                           specType = string.Empty, endDate = string.Empty,
                           remark = string.Empty, inviteCon = string.Empty,
                           CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;
                            parser.Reset();
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "art_info")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode[0].ToHtml();

                                inviteCtx = HtmlTxt.GetReplace("</p>", "\r\n").ToCtxString();

                                buildUnit = inviteCtx.GetBuildRegex();
                                prjAddress = inviteCtx.GetAddressRegex();
                                code = inviteCtx.GetCodeRegex().GetCodeDel();

                                specType = "政府采购";
                                inviteType = prjName.GetInviteBidType();
                                msgType = "珠海市香洲城市管理局";

                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "珠海市区", "香洲区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, infoUrl, HtmlTxt);
                                list.Add(info);
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag a = aNode[k].GetATag();
                                        if (a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = a.Link;
                                            else
                                                link = "http://cgj.zhxz.cn/" + a.Link;
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
            }
            return list;
        }
    }
}
