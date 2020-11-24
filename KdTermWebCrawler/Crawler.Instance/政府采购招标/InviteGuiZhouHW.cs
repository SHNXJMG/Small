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
    public class InviteGuiZhouHW:WebSiteCrawller
    {
        public InviteGuiZhouHW()
            : base(true)
        {
            this.Group = "政府采购招标信息";
            this.Title = "贵州省发展和改革委员会货物招标";
            this.Description = "自动抓取贵州省发展和改革委员会货物招标";
            this.PlanTime = "9:40,11:40,14:40,16:40,18:40";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 500;
            this.SiteUrl = "http://www.gzzbw.cn/plus/list.php?tid=31";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1,sqlCount=0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].GetATagHref();
                    temp = temp.Substring(temp.ToLower().IndexOf("pageno="), temp.Length - temp.ToLower().IndexOf("pageno="));
                    pageInt = int.Parse(temp.ToLower().Replace("pageno=", ""));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&PageNo=" + i, Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("id", "sb_list1")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        ATag aTag = node.GetATag();
                        prjName = aTag.LinkText;
                        InfoUrl = "http://www.gzzbw.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "box")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();

                            beginDate = inviteCtx.GetRegex("发布时间").GetDateRegex();
                            if (string.IsNullOrEmpty(beginDate))
                                beginDate = DateTime.Now.ToString();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex();
                            specType = "政府采购";
                            inviteType = "货物";
                            msgType = "贵州省发展和改革委员会";

                            InviteInfo info = ToolDb.GenInviteInfo("贵州省", "贵州省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            sqlCount++;
                            if (sqlCount >= 30)
                            {
                                sqlCount = 0;
                                Thread.Sleep(1000 * 800);
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
