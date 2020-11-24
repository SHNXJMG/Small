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
    public class InviteInfoGuiZhouFGW : WebSiteCrawller
    {
        public InviteInfoGuiZhouFGW()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "贵州省发展和改革委员会工程招标";
            this.Description = "自动抓取贵州省发展和改革委员会工程招标";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 500;
            this.SiteUrl = "http://www.gzzbw.cn/zbgg/index.jhtml";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1, sqlCount = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "main container clearfix")), true), new TagNameFilter("div")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].ToString().GetRegexBegEnd("/", "页");
                    pageInt = int.Parse(temp.ToLower());
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.gzzbw.cn/zbgg/index_" + i + ".jhtml", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "new_table")), true), new TagNameFilter("tr")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 1; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        ATag aTag = node.GetATag();
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content-txt")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.Replace("</p>", "\r\n").Replace("<br/>", "\r\n").ToCtxString();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            if (string.IsNullOrWhiteSpace(prjAddress))
                                prjAddress = inviteCtx.GetRegex("地址");
                            code = inviteCtx.GetCodeRegex();
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList btl = parser.ExtractAllNodesThatMatch(new TagNameFilter("h1"));
                            string ht = btl.AsHtml();
                            prjName = ht.ToCtxString();
                            specType = "政府采购";
                            inviteType = "工程";
                            msgType = "贵州省发展和改革委员会";

                            InviteInfo info = ToolDb.GenInviteInfo("贵州省", "贵州省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            sqlCount++;
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
