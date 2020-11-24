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

namespace Crawler.Instance
{
    public class InviteSTLongHu : WebSiteCrawller
    {
        public InviteSTLongHu()
            : base()
        {
            this.Group = "区县招标信息";
            this.Title = "广东省汕头市龙湖区政府";
            this.Description = "自动抓取广东省汕头市龙湖区政府";
            this.PlanTime = "9:03,10:32,14:09,16:07";
            this.SiteUrl = "http://www.gdlonghu.gov.cn/Gb/Home/ShowList.aspx?CateID=002160010&WorkGuide=";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("noWrap", "true")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().ToNodeString();
                    Regex reg = new Regex(@"/[^页]+页");
                    string page = reg.Match(temp).Value.Replace("/", "").Replace("页", "");
                    pageInt = Convert.ToInt32(page);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    try
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                            new string[]
                            {"__VIEWSTATE",
                                "__EVENTTARGET",
                            "__EVENTARGUMENT",
                            "__EVENTVALIDATION",
                            "tbSearchText"},
                                new string[]{
                                viewState,
                                "pager",
                                i.ToString(),
                                eventValidation,
                                ""
                                });
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ShowListMiddleContent")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    parser = new Parser(new Lexer(nodeList.ToHtml()));
                    NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ShowListTitle")));
                    parser = new Parser(new Lexer(nodeList.ToHtml()));
                    NodeList timeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ShowListTime")));
                    if (viewList != null && viewList.Count > 0 && timeList != null && timeList.Count > 0 && timeList.Count == viewList.Count)
                    {
                        for (int j = 0; j < viewList.Count; j++)
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                       prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                       specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                       remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                       CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            prjName = viewList[j].GetATagValue("title");
                            inviteType = prjName.GetInviteBidType();
                            beginDate = timeList[j].ToNodePlainString().GetDateRegex();
                            InfoUrl = "http://www.gdlonghu.gov.cn/Gb/Home/" + viewList[j].GetATagHref();
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "lblContent")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                inviteCtx = HtmlTxt.Replace("<p>", "\r\n").Replace("</p>", "\r\n").ToCtxString();//.Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");

                                buildUnit = inviteCtx.GetBuildRegex();
                                code = inviteCtx.GetCodeRegex().Replace("/", "");
                                prjAddress = inviteCtx.GetAddressRegex();

                                msgType = "汕头龙湖区政府";
                                specType = "建设工程";
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "汕头市区", "龙湖区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aList != null && aList.Count > 0)
                                {
                                    for (int c = 0; c < aList.Count; c++)
                                    {
                                        ATag a = aList[c] as ATag;
                                        if (a.Link.IsAtagAttach())
                                        {
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, "http://www.gdlonghu.gov.cn" + a.Link);
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
