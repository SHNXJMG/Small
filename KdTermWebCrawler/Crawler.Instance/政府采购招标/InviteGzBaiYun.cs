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
using System.Text;

namespace Crawler.Instance
{
    public class InviteGzBaiYun : WebSiteCrawller
    {
        public InviteGzBaiYun()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "广州市白云区政府办公室招标、中标公告";
            this.Description = "自动抓取广州市白云区政府办公室招标、中标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://www.by.gov.cn/publicfiles//business/htmlfiles/BY00/076/list.html";
            this.MaxCount = 120;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 31;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }

            int startIndex = html.IndexOf("<xml");
            int endIndex = html.IndexOf("</xml>");
            string xmlstr = html.Substring(startIndex, endIndex - startIndex).ToLower().GetReplace("infourl", "span").GetReplace("info", "div").GetReplace("publishedtime", "p");
            Parser parser = new Parser(new Lexer(xmlstr));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("div"));
            if (pageNode != null && pageNode.Count > 0)
            {
                for (int i = 0; i < pageNode.Count; i++)
                {
                    string prjName = string.Empty, InfoUrl = string.Empty, beginDate = string.Empty, HtmlTxt = string.Empty;
                    parser = new Parser(new Lexer(pageNode[i].ToHtml()));
                    NodeList dateNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("p"));
                    beginDate = dateNode[0].ToPlainTextString().GetDateRegex();
                    parser.Reset();
                    NodeList urlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("span"));
                    InfoUrl = "http://www.by.gov.cn/publicfiles/business/htmlfiles/" + urlNode[0].ToPlainTextString().ToUpper().GetReplace(".HTML", ".html");
                    parser.Reset();
                    NodeList prjNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("title"));
                    prjName = prjNode[0].ToNodePlainString();
                    string htmldtl = string.Empty;
                    try
                    {
                        htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                    }
                    catch { continue; }
                    parser = new Parser(new Lexer(htmldtl));
                    NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zoomcon")));
                    if (dtl != null && dtl.Count > 0)
                    {
                        HtmlTxt = dtl.AsHtml();
                        string ctx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();

                        if (prjName.Contains("中标") || prjName.Contains("成交") || prjName.Contains("结果"))
                        {
                            string buildUnit = string.Empty, bidUnit = string.Empty,
                      bidMoney = string.Empty, code = string.Empty,
                      bidDate = string.Empty,
                      endDate = string.Empty, bidType = string.Empty,
                      specType = string.Empty,
                      msgType = string.Empty, bidCtx = string.Empty,
                      prjAddress = string.Empty, remark = string.Empty,
                      prjMgr = string.Empty, otherType = string.Empty;
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                            if (prjName[prjName.Length - 1] == '）')
                            {
                                code = prjName.GetRegexBegEnd("（", "）");
                                prjName = prjName.GetReplace("（" + code + "）");
                            }
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = bidCtx.GetBuildRegex();
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";

                            bidUnit = bidCtx.GetBidRegex();
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            bidMoney = bidCtx.GetMoneyRegex(null, false, "万", 100, "；");
                            if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetMoneyRegex(new string[] { "投标总报价" }, false, "万", 100, "；");
                            bidUnit = bidUnit.GetReplace("名称");
                            try
                            {
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = "广州市白云区政府办公室";
                            msgType = "广州市白云区政府办公室";
                            specType = "政府采购";
                            bidType = prjName.GetInviteBidType();
                            BidInfo info = ToolDb.GenBidInfo("广东省", "广州政府采购", "白云区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                        {

                                            link = "http://www.by.gov.cn/" + a.Link.GetReplace("./");
                                        }
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
                   CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty;

                            inviteCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                            inviteType = prjName.GetInviteBidType();

                            //string tempName = inviteCtx.GetRegex("工程名称,项目名称");
                            //if (!string.IsNullOrEmpty(tempName))
                            //    prjName = tempName;
                            if (prjName[prjName.Length - 1] == '）')
                            {
                                code = prjName.GetRegexBegEnd("（", "）");
                                prjName = prjName.GetReplace("（" + code + "）");
                            }
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = "广州市白云区政府办公室";

                            msgType = "广州市白云区政府办公室";
                            specType = "政府采购";

                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "广州政府采购", "白云区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                        {
                                            link = "http://www.by.gov.cn/" + a.Link.GetReplace("./");
                                        }
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
