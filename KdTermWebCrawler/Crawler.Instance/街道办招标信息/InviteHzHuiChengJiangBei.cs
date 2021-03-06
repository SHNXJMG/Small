﻿using System.Text;
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
using System.Threading;

namespace Crawler.Instance
{
    public class InviteHzHuiChengJiangBei : WebSiteCrawller
    {
        public InviteHzHuiChengJiangBei()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省惠城区江北街道办事处招标、中标公告";
            this.Description = "自动抓取广东省惠城区江北街道办事处招标、中标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://jbb.hcq.gov.cn/news/95.aspx";
            this.MaxCount = 100;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
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
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page")), true), new TagNameFilter("a")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[sNode.Count - 2].ToNodePlainString();
                    pageInt = Convert.ToInt32(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://jbb.hcq.gov.cn/news/95/" + i + ".aspx", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ny_new")), true), new TagNameFilter("li")));
                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
                    {

                        string prjName = string.Empty;
                        ATag aTag = viewList[j].GetATag();
                        prjName = aTag.LinkText.Trim();

                        if (prjName.Contains("中标") || prjName.Contains("成交") || prjName.Contains("结果"))
                        {
                            string
                            buildUnit = string.Empty, bidUnit = string.Empty,
                            bidMoney = string.Empty, code = string.Empty,
                            bidDate = string.Empty,
                            beginDate = string.Empty,
                            endDate = string.Empty, bidType = string.Empty,
                            specType = string.Empty, InfoUrl = string.Empty,
                            msgType = string.Empty, bidCtx = string.Empty,
                            prjAddress = string.Empty, remark = string.Empty,
                            prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                            InfoUrl = "http://jbb.hcq.gov.cn" + aTag.Link.GetReplace("../");
                            beginDate = viewList[j].ToPlainTextString().GetDateRegex();
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ny_con")));
                            if (dtl != null && dtl.Count > 0)
                            {
                                HtmlTxt = dtl.AsHtml();
                                bidCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                                bidType = prjName.GetInviteBidType();

                                code = bidCtx.GetCodeRegex().GetCodeDel();
                                buildUnit = bidCtx.GetBuildRegex();
                                if (buildUnit.Contains("招标代理"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";

                                bidUnit = bidCtx.GetBidRegex();

                                bidMoney = bidCtx.GetMoneyRegex();
                                try
                                {
                                    if (decimal.Parse(bidMoney) < 1)
                                        bidMoney = "0";
                                    if (decimal.Parse(bidMoney) > 100000)
                                        bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                                }
                                catch { }
                                msgType = "惠州市惠城区江北街道办事处";
                                specType = "政府采购";

                                BidInfo info = ToolDb.GenBidInfo("广东省", "惠州市区", "惠城区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                      bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                                link = "http://jbb.hcq.gov.cn/" + a.Link;
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
                        specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                        remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                        CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                            InfoUrl = "http://jbb.hcq.gov.cn" + aTag.Link.GetReplace("../");
                            beginDate = viewList[j].ToPlainTextString().GetDateRegex();
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ny_con")));
                            if (dtl != null && dtl.Count > 0)
                            {
                                HtmlTxt = dtl.AsHtml();
                                inviteCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                                inviteType = prjName.GetInviteBidType();

                                code = inviteCtx.GetCodeRegex().GetCodeDel();
                                buildUnit = inviteCtx.GetBuildRegex();
                                prjAddress = inviteCtx.GetAddressRegex();
                                if (buildUnit.Contains("招标代理"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                                msgType = "惠州市惠城区江北街道办事处";

                                specType = "政府采购";

                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "惠州市区", "惠城区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                                link = "http://jbb.hcq.gov.cn/" + a.Link;
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
