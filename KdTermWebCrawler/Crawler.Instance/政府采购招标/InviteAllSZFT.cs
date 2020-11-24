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
using System.IO;
using System.Web.UI.MobileControls;
using System.Collections.Generic;
namespace Crawler.Instance
{
    public class InviteAllSZFT : WebSiteCrawller
    {
        public InviteAllSZFT()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "深圳市政府采购全区站点（福田）招标";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市政府采购全区站点福田招标";
            this.SiteUrl = "http://61.144.240.26:58080/news/publicnews/12121";
            this.MaxCount = 300;
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "paging")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string pageos = pageNode.AsString().GetRegexBegEnd("/", "页");
                try
                {
                    pageInt = int.Parse(pageos);

                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://61.144.240.26:58080/news/publicnews/12121?pageIndex=" + i, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("id", "newsList")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {

                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                            prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                            specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                            remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                            CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty,
                            HtmlTxt = string.Empty;
                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.LinkText.GetReplace("· ", "");
                        InfoUrl = "http://61.144.240.26:58080" + aTag.Link;
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        string htmlDtl = string.Empty;
                        try
                        {
                            htmlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmlDtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "98%")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString().Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            if (string.IsNullOrWhiteSpace(code))
                                code = inviteCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = inviteCtx.GetRegex("采购人名称");
                            if(string.IsNullOrWhiteSpace(buildUnit))
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetRegex("采购人联系地址");
                            if(string.IsNullOrWhiteSpace(prjAddress))
                            prjAddress = inviteCtx.GetAddressRegex();
                            inviteType = prjName.GetInviteBidType();
                            if (code.Contains("）"))
                                code = code.GetReplace("）", "");
                            if (string.IsNullOrWhiteSpace(buildUnit))
                                buildUnit = inviteCtx.GetRegex("单位名称");
                            if (string.IsNullOrWhiteSpace(prjAddress))
                                prjAddress = inviteCtx.GetRegex("联系地址");


                            specType = "政府采购";
                            msgType = "深圳政府采购";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "政府采购", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
