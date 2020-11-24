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
    public class InviteSzLhXxgcSgZfcg : WebSiteCrawller
    {
        public InviteSzLhXxgcSgZfcg()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "深圳市龙华新区公共资源交易中心小型工程招标信息";
            this.Description = "自动抓取深圳市龙华新区公共资源交易中心小型工程招标信息";
            this.PlanTime = "9:22,13:53";
            this.SiteUrl = "http://lhxq.szzfcg.cn/portal/topicView.do?method=view1&id=500100301&siteId=11&tstmp=09%3A36%3A41%20GMT%2B0800&page=1";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
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
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "clearfix")), true), new TagNameFilter("a")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[sNode.Count - 1].GetATag().GetAttribute("onclick").Replace("(", "kdxx").Replace(",", "xxdk");
                    pageInt = int.Parse(temp.GetRegexBegEnd("kdxx", "xxdk"));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://lhxq.szzfcg.cn/portal/topicView.do?method=view1&id=500100301&siteId=11&tstmp=09%3A36%3A41%20GMT%2B0800&page=" + i, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("li"));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count - 1; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                          prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                          specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                          remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                          CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        beginDate = listNode[j].ToNodePlainString().GetDateRegex("yyyy/MM/dd");
                        ATag aTag = listNode[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        Regex regexLink = new Regex(@"id=[^-]+");
                        string id = regexLink.Match(aTag.Link).Value;
                        InfoUrl = "http://lhxq.szzfcg.cn/portal/documentView.do?method=view&" + id;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            msgType = "深圳市龙华新区公共资源交易中心";
                            specType = "政府采购";
                            inviteType = "施工";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳政府采购", "龙华新区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);

                            list.Add(info);

                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aTagNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aTagNode != null && aTagNode.Count > 0)
                            {
                                for (int k = 0; k < aTagNode.Count; k++)
                                {
                                    ATag aFile = aTagNode[k].GetATag();
                                    if (aFile.IsAtagAttach() || aFile.Link.ToLower().Contains("down"))
                                    {
                                        string link = string.Empty;
                                        if (aFile.Link.Contains("http"))
                                            link = aFile.Link;
                                        else
                                            link = "http://lhxq.szzfcg.cn/" + aFile.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(aFile.LinkText, info.Id, link);
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
