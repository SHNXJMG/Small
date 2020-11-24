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
using System.Threading;

namespace Crawler.Instance
{
    public class InviteDgQingXi : WebSiteCrawller
    {
        public InviteDgQingXi()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省东莞市清溪镇人民政府信息招标中标公告";
            this.Description = "自动抓取东莞市清溪镇人民政府信息招标中标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://www.qingxi.gov.cn/qingxi/zbxx/list2.shtml";
            this.MaxCount = 150;
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content-right fr")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().GetRegexBegEnd("page_div',", ",");
                    pageInt = int.Parse(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.qingxi.gov.cn/qingxi/zbxx/list2" + "_" + i + ".shtml", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content-right fr")), true), new TagNameFilter("li")));

                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
                    {
                        ATag aTag = viewList[j].GetATag();
                        string beginDate = viewList[j].ToPlainTextString().GetDateRegex();

                        string tempName = aTag.LinkText;

                        if (!tempName.Contains("中标") && !tempName.Contains("招标"))
                            continue;

                        string InfoUrl = "http://www.qingxi.gov.cn/" + aTag.Link.GetReplace("./");
                        string htmDtl = string.Empty;
                        try
                        {
                            htmDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
                            htmDtl = regexHtml.Replace(htmDtl, "");
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zoom")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            if (tempName.Contains("中标"))
                            {
                                string buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, bidDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, code = string.Empty, prjName = string.Empty;


                                HtmlTxt = dtl.AsHtml().ToLower();
                                bidCtx = HtmlTxt.GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();

                                prjName = bidCtx.GetRegex("项目名称,工程名称");
                                if (string.IsNullOrEmpty(prjName))
                                    prjName = aTag.LinkText.GetRegexBegEnd("【", "】");
                                if (string.IsNullOrWhiteSpace(prjName))
                                {
                                    parser = new Parser(new Lexer(htmDtl));
                                    NodeList nameNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "text red font20 con-title padd-t")));
                                    if (nameNode != null && nameNode.Count > 0)
                                    {
                                        prjName = nameNode[0].ToNodePlainString();
                                    }
                                }
                                if (string.IsNullOrWhiteSpace(prjName))
                                {
                                    prjName = aTag.LinkText;
                                }

                                code = bidCtx.GetCodeRegex().GetCodeDel();
                                buildUnit = bidCtx.GetBuildRegex();
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex(new string[] { "中标值" });
                                if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                    bidMoney = bidCtx.GetMoneyRegex();
                                prjMgr = bidCtx.GetMgrRegex();
                                bidDate = bidCtx.GetRegex("中标时间").GetDateRegex();
                                if (string.IsNullOrWhiteSpace(bidDate))
                                    bidDate = beginDate;
                                specType = "政府采购";
                                bidType = prjName.GetInviteBidType();
                                msgType = "东莞市清溪镇政府";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "东莞市区", "清溪镇", string.Empty, code, prjName, buildUnit, bidDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                                link = "http://www.qingxi.gov.cn/" + a.Link;
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                            else
                            {
                                string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                                HtmlTxt = dtl.AsHtml();
                                inviteCtx = HtmlTxt.GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                                prjName = inviteCtx.GetRegex("项目名称,工程名称");
                                if (string.IsNullOrEmpty(prjName))
                                    prjName = aTag.LinkText.GetRegexBegEnd("【", "】");
                                if (string.IsNullOrWhiteSpace(prjName))
                                {
                                    parser = new Parser(new Lexer(htmDtl));
                                    NodeList nameNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "text red font20 con-title padd-t")));
                                    if (nameNode != null && nameNode.Count > 0)
                                    {
                                        prjName = nameNode[0].ToNodePlainString();
                                    }
                                }
                                if (string.IsNullOrWhiteSpace(prjName))
                                {
                                    prjName = aTag.LinkText;
                                }
                                inviteType = prjName.GetInviteBidType();
                                prjAddress = inviteCtx.GetAddressRegex();
                                buildUnit = inviteCtx.GetBuildRegex();

                                if (string.IsNullOrWhiteSpace(buildUnit))
                                    buildUnit = inviteCtx.GetRegex("招标人");
                                if (buildUnit.Contains("招标代理"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                                code = inviteCtx.GetCodeRegex().GetCodeDel();
                                msgType = "东莞市清溪镇政府";
                                specType = "政府采购";
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "东莞市区", "清溪镇", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                                link = "http://www.qingxi.gov.cn/" + a.Link;
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
