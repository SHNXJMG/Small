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
    public class InviteDgDongChen : WebSiteCrawller
    {
        public InviteDgDongChen()
            : base(true)
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省东莞市东城区办事处招标中标通知公示公告";
            this.Description = "自动抓取东莞市东城区办事处中标通知公示公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://dongcheng.dg.gov.cn/publicfiles//business/htmlfiles/dgdc/s42859/list.htm";
            this.MaxCount = 150;
            //this.Disabled = true;
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
            catch { return list; }
            int startIndex = html.IndexOf("<xml");
            int endIndex = html.IndexOf("</xml>");
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
                    string infoUrl = "http://dongcheng.dg.gov.cn/publicfiles//business/htmlfiles/" + urlNode[0].ToPlainTextString();
                    string htmldtl = string.Empty;
                    try
                    {
                        htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl).GetJsString();
                    }
                    catch { continue; }
                    parser = new Parser(new Lexer(htmldtl));
                    NodeList titleNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("title"));
                    string prjName = titleNode[0].ToNodePlainString();
                    if (prjName.Contains("_"))
                        prjName = prjName.Remove(prjName.IndexOf("_"));
                    if (prjName.Contains("中标"))
                    {
                        string buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        parser.Reset();
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("valign", "top")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</p>", "\r\n").ToCtxString();

                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetRegex("中标值").GetMoney();
                            if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            code = bidCtx.GetCodeRegex();

                            specType = "政府采购";
                            bidType = prjName.GetInviteBidType();
                            msgType = "东莞市东城区办事处";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "东莞市区", "东城区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, infoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://dongcheng.dg.gov.cn/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                        }
                    }
                    else if (prjName.Contains("通知"))
                    { 
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty, area = string.Empty;

                        parser.Reset();
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("valign", "top")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            InfoTitle = prjName;
                            PublistTime = beginDate;
                            htmlTxt = dtlNode.AsHtml();
                            Parser imgParser = new Parser(new Lexer(htmlTxt.ToLower()));
                            NodeList imgNode = imgParser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                            string src = string.Empty;
                            if (imgNode != null && imgNode.Count > 0)
                            {
                                string imgUrl = (imgNode[0] as ImageTag).GetAttribute("src");
                                src = "http://dongcheng.dg.gov.cn/" + imgUrl;
                                htmlTxt = htmlTxt.ToLower().GetReplace(imgUrl, src);
                            }
                            InfoCtx = htmlTxt.ToCtxString();

                            NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "东莞市区", "东城区", string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "东莞市东城区办事处", infoUrl, prjCode, buildUnit, string.Empty, string.Empty, "政府采购", string.Empty, htmlTxt);
                            list.Add(info);
                            if (!string.IsNullOrEmpty(src))
                            {
                                string sql = string.Format("select Id from InviteInfo where InfoUrl='{0}'", info.InfoUrl);
                                object obj = ToolDb.ExecuteScalar(sql);
                                if (obj == null || obj.ToString() == "")
                                {
                                    try
                                    {
                                        BaseAttach attach = ToolHtml.GetBaseAttach(src, prjName, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                        if (attach != null)
                                            ToolDb.SaveEntity(attach, "");
                                    }
                                    catch { }
                                }
                            }
                            parser = new Parser(new Lexer(htmlTxt));
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
                                            link = "http://dongcheng.dg.gov.cn/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
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
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("valign", "top")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode[0].ToHtml();

                            Parser imgParser = new Parser(new Lexer(HtmlTxt.ToLower()));
                            NodeList imgNode = imgParser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                            string src = string.Empty;
                            if (imgNode != null && imgNode.Count > 0)
                            {
                                string imgUrl = (imgNode[0] as ImageTag).GetAttribute("src");
                                src = "http://dongcheng.dg.gov.cn/" + imgUrl;
                                HtmlTxt = HtmlTxt.ToLower().GetReplace(imgUrl, src);
                            }
                            inviteCtx = HtmlTxt.GetReplace("</p>", "\r\n").ToCtxString();

                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex();

                            specType = "政府采购";
                            inviteType = prjName.GetInviteBidType();
                            msgType = "东莞市东城区办事处";

                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "东莞市区", "东城区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, infoUrl, HtmlTxt);
                            list.Add(info);
                            if (!string.IsNullOrEmpty(src))
                            {
                                string sql = string.Format("select Id from InviteInfo where InfoUrl='{0}'", info.InfoUrl);
                                object obj = ToolDb.ExecuteScalar(sql);
                                if (obj == null || obj.ToString() == "")
                                {
                                    try
                                    {
                                        BaseAttach attach = ToolHtml.GetBaseAttach(src, prjName, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                        if (attach != null)
                                            ToolDb.SaveEntity(attach, "");
                                    }
                                    catch { }
                                }
                            }
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
                                            link = "http://dongcheng.dg.gov.cn/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                        }
                    }
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }
            return list;
        }
    }
}
