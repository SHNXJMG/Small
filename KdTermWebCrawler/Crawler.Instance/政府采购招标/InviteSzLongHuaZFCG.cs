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
    public class InviteSzLongHuaZFCG : WebSiteCrawller
    {
        public InviteSzLongHuaZFCG()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "深圳市龙华新区公告资源交易中心政府采购招标公告";
            this.Description = "自动抓取深圳市龙华新区公告资源交易中心政府采购招标公告";
            this.PlanTime = "9:22,13:53";
            this.SiteUrl = "http://www.szlhxq.gov.cn/lhxinqu/zdlyxxgkzl/ggzyjy/zfcgzbgg/index.html";
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
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "zfcg_feiyeR")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToNodePlainString().GetRegexBegEnd("/", "跳");

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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.szlhxq.gov.cn/lhxinqu/zdlyxxgkzl/ggzyjy/zfcgzbgg/9604-" + i+".html", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "zfcg_zbggUl")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                          prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                          specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                          remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                          CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        beginDate = listNode[j].ToNodePlainString().GetDateRegex();
                        ATag aTag = listNode[j].GetATag();
                        prjName = aTag.LinkText;
                        InfoUrl = "http://www.szlhxq.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "min")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            inviteType = prjName.GetInviteBidType();
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            msgType = "深圳市龙华新区公共资源交易中心";
                            specType = "政府采购"; 
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳政府采购", "龙华新区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
