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

namespace Crawler.Instance
{
    public class InviteZhWanZai : WebSiteCrawller
    {
        public InviteZhWanZai()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省珠海市湾仔街道办招标、中标公告";
            this.Description = "自动抓取广东省珠海市湾仔街道办招标、中标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://wanzai.zhxz.cn/xxgks/tztg/";
            this.MaxCount = 120;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 3;
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

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "index_" + (i - 1).ToString() + ".html", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "newlisttable")));
                if (viewList != null && viewList.Count > 0)
                {
                    TableTag table = viewList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string prjName = string.Empty, InfoUrl = string.Empty, beginDate = string.Empty, HtmlTxt = string.Empty;
                        ATag aTag = tr.GetATag();
                        if (aTag == null) continue;

                        prjName = aTag.LinkText.Trim();
                        InfoUrl = this.SiteUrl + aTag.Link.GetReplace("./");
                        beginDate = tr.ToPlainTextString().GetDateRegex();
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("colspan", "3")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml();
                            string ctx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();

                            parser.Reset();
                            NodeList nodeName = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "chinese4")));
                            if (nodeName != null && nodeName.Count > 0)
                            {
                                prjName = nodeName[0].ToNodePlainString().GetReplace(" ").Trim();
                            }
                            else
                                continue;
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

                                prjName = prjName.GetReplace("中标公示-");
                                code = bidCtx.GetCodeRegex().GetCodeDel();
                                buildUnit = bidCtx.GetBuildRegex();
                                if (buildUnit.Contains("招标代理"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";

                                bidUnit = bidCtx.GetBidRegex();
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = bidCtx.GetRegexBegEnd("中标单位是", "，").GetReplace(":,：");
                                bidMoney = bidCtx.GetMoneyRegex();
                                if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                    bidMoney = bidCtx.GetRegexBegEnd("中标价为", "元").GetMoney().GetReplace(":,：");
                                try
                                {
                                    if (decimal.Parse(bidMoney) > 100000)
                                        bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                                }
                                catch { }
                                if (string.IsNullOrEmpty(buildUnit))
                                    buildUnit = "珠海市香洲区湾仔街道办事处";
                                msgType = "珠海市香洲区湾仔街道办事处";
                                specType = "政府采购";
                                bidType = prjName.GetInviteBidType();
                                BidInfo info = ToolDb.GenBidInfo("广东省", "珠海市区", "香洲区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                                int leng = InfoUrl.LastIndexOf("/");
                                                if (leng > 0)
                                                    link = InfoUrl.Remove(leng) + "/" + a.Link.GetReplace("./");
                                                else
                                                    link = this.SiteUrl + a.Link.GetReplace("./");
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
                                prjName = prjName.GetReplace("招标公告-");
                                code = inviteCtx.GetCodeRegex().GetCodeDel();
                                buildUnit = inviteCtx.GetBuildRegex();
                                prjAddress = inviteCtx.GetAddressRegex();
                                if (buildUnit.Contains("招标代理"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                                if (string.IsNullOrEmpty(buildUnit))
                                    buildUnit = "珠海市香洲区湾仔街道办事处";

                                msgType = "珠海市香洲区湾仔街道办事处";
                                specType = "政府采购";

                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "珠海市区", "香洲区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                                int leng = InfoUrl.LastIndexOf("/");
                                                if (leng > 0)
                                                    link = InfoUrl.Remove(leng) + "/" + a.Link.GetReplace("./");
                                                else
                                                    link = this.SiteUrl + a.Link.GetReplace("./");
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

            }
            return list;
        }
    }
}
