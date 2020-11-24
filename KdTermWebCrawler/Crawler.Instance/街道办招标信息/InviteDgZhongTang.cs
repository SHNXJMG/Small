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
    public class InviteDgZhongTang : WebSiteCrawller
    {
        public InviteDgZhongTang()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省东莞中堂镇政府信息招标中标公告";
            this.Description = "自动抓取东莞中堂镇政府信息招标中标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://www.zhongtang.gov.cn/business/htmlfiles/zhongtang/s8782/list.htm";
            this.MaxCount = 150;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
                }
                catch { return list; }
            }
            int startIndex = html.LastIndexOf("<xml");
            int endIndex = html.LastIndexOf("</xml>");
            string xmlstr = html.Substring(startIndex, endIndex - startIndex).ToLower().GetReplace("infourl", "span").GetReplace("info", "div").GetReplace("publishedtime", "p");
            Parser parser = new Parser(new Lexer(xmlstr));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("div"));
            if (pageNode != null && pageNode.Count > 0)
            {
                for (int i = 0; i < pageNode.Count; i++)
                {
                    parser = new Parser(new Lexer(pageNode[i].ToHtml()));
                    NodeList dateNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("p"));
                    string beginDate = dateNode[0].ToPlainTextString().GetDateRegex();
                    parser.Reset();
                    NodeList urlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("span"));
                    string InfoUrl = "http://www.zhongtang.gov.cn/business/htmlfiles/" + urlNode[0].ToPlainTextString();
                    string htmldtl = string.Empty;
                    try
                    {
                        htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                    }
                    catch
                    {
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(htmldtl));
                    NodeList titleNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zoomtitl")));
                    string prjName = string.Empty;
                    if (titleNode != null && titleNode.Count > 0)
                        prjName = titleNode[0].ToNodePlainString().GetReplace(" ");
                    parser.Reset();
                    NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("width", "778")));
                    if (dtlNode != null && dtlNode.Count > 0)
                    {
                        if (prjName.Contains("中标"))
                        {
                            string buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            HtmlTxt = dtlNode[0].ToHtml();
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();

                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            bidUnit = bidCtx.GetBidRegex();

                            bidMoney = bidCtx.GetRegex("中标值").GetMoney();
                            if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetMoneyRegex();
                            try
                            {
                                if (decimal.Parse(bidMoney) < 1)
                                    bidMoney = "0";
                            }
                            catch { }
                            prjMgr = bidCtx.GetMgrRegex();

                            specType = "政府采购";
                            bidType = prjName.GetInviteBidType();
                            msgType = "东莞市中堂镇政府";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "东莞市区", "中堂镇", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.zhongtang.gov.cn/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                        else
                        {
                            string code = string.Empty, buildUnit = string.Empty,
                 prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                 specType = string.Empty, endDate = string.Empty,
                 remark = string.Empty, inviteCon = string.Empty,
                 CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                            HtmlTxt = dtlNode[0].ToHtml();
                            inviteCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();

                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));

                            specType = "政府采购";
                            inviteType = prjName.GetInviteBidType();
                            msgType = "东莞市中堂镇政府";

                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "东莞市区", "中堂镇", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://www.zhongtang.gov.cn/" + a.Link;
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
