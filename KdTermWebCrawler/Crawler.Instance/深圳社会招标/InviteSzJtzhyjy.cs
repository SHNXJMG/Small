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
    public class InviteSzJtzhyjy : WebSiteCrawller
    {
        public InviteSzJtzhyjy()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Title = "深圳市综合交通设计研究院";
            this.Description = "自动抓取深圳市综合交通设计研究院招标信息";
            this.SiteUrl = "http://www.ctdri.com/web2/list/?115_1.html";
            this.MaxCount = 120;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "wp-pagenavi")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString().GetRegexBegEnd("共", "页").Replace("　", "").Replace("共", "");
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.ctdri.com/web2/list/?115_" + i.ToString()+".html", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "sidelist_news")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                             prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                             specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                             remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                             CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, img = string.Empty, HtmlTxt = string.Empty;

                        prjName = nodeList[j].GetATagValue("title");
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.ctdri.com" + nodeList[j].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "text")));
                        if (dtList != null && dtList.Count > 0)
                        {
                            HtmlTxt = dtList.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();

                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.Replace(" ", "").GetAddressRegex();
                            code = inviteCtx.GetCodeRegex().GetChina();
                            specType = "其他";
                            msgType = "深圳市综合交通设计研究院";
                            inviteType = prjName.GetInviteBidType();
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "",
                               string.Empty, code, prjName, prjAddress, buildUnit,
                               beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;

                        }
                    }
                }
            }
            return list;
        }
    }
}
