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
    public class InviteAnHuiZtb : WebSiteCrawller
    {
        public InviteAnHuiZtb()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "安徽省发展和改革委员会招标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取安徽省发展和改革委员会招标信息";
            this.SiteUrl = "http://www.ahtba.org.cn/Notice/AnhuiNoticeSearch?spid=714&scid=597";
            this.MaxCount = 400;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter( new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagination f_right")),true),new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count-1].GetATagValue("onclick").GetRegexBegEnd("Info", ",").GetReplace("(");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&pageSize=15&pageNum=" + i);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "newsList")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        string area = node.ToNodePlainString().GetReplace("[", "【").GetReplace("]", "】").GetRegexBegEnd("【","】");
                        prjName = aTag.GetAttribute("title");
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.ahtba.org.cn" + aTag.Link.GetReplace("amp;");
                        string id = aTag.Link.Substring(aTag.Link.IndexOf("id="), aTag.Link.Length - aTag.Link.IndexOf("id=")).GetReplace("id=");
                        string htmldtl = string.Empty;
                        try
                        {
                            NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                            "id"
                            },new string[]{
                             id
                            });
                            htmldtl = this.ToolWebSite.GetHtmlByUrl("http://www.ahtba.org.cn/Notice/NoticeContent",nvc).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "new_detail")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            buildUnit = inviteCtx.GetReplace(" ").GetBuildRegex();
                            code = inviteCtx.GetReplace(" ").GetCodeRegex().GetCodeDel();
                            prjAddress = inviteCtx.GetReplace(" ").GetAddressRegex();
                            if (buildUnit.Contains("管理局"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("管理局")) + "管理局";
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            msgType = "安徽省发展和改革委员会";
                            specType = inviteType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("安徽省", "安徽省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
