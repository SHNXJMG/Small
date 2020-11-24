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
    public class InviteInfoGz:WebSiteCrawller
    {
        public InviteInfoGz()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "广州市政府采购网";
            this.Description = "自动抓取广州是政府采购网";
            this.PlanTime = "21:08,11:12,14:02,16:33";
            this.SiteUrl = "http://gzg2b.gzfinance.gov.cn/Sites/_Layouts/ApplicationPages/News/News.aspx?ColumnName=%e6%8b%9b%e6%a0%87%e9%87%87%e8%b4%ad%e5%85%ac%e5%91%8a";
            this.MaxCount = 100000;
            this.ExistCompareFields = "InfoUrl";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter( new TagNameFilter("div"), new HasAttributeFilter("class", "pagerbtn")),true),new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].GetATagValue("href").Replace("javascript", "").Replace("__doPostBack", "").Replace(":", "").Replace("(", "").Replace("ctl00$main$pager", "").Replace("ctl00$main$pagerHeader", "").Replace(")", "").Replace("'", "").Replace(",","");
                    pageInt = int.Parse(temp);
                }
                catch { }

                for (int i = 1; i <= pageInt; i++)
                { 
                    if (i > 1)
                    {
                        try
                        {
                            viewState = this.ToolWebSite.GetAspNetViewState(html); 
                            NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                            "__WPPS",
                            "__EVENTTARGET",
                            "__EVENTARGUMENT",
                            "__VIEWSTATE",
                            "ctl00$hfGlobalToolbar",
                            "ctl00$main$pagerHeader_input",
                            "ctl00$main$pager_input"
                            },
                                new string[]{
                                "u","ctl00$main$pagerHeader",
                                i.ToString(),
                                viewState,
                                "","1","1"
                                });
                            html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "list3")),true),new TagNameFilter("li")));
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        for (int j = 0; j < nodeList.Count; j++)
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            prjName = nodeList[j].GetATagValue("title");
                            beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                            InfoUrl = "http://gzg2b.gzfinance.gov.cn" + nodeList[j].GetATagHref();
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "note_container")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.ToHtml();
                                inviteCtx = HtmlTxt.ToCtxString().Replace("&rdquo;", "");
                                prjAddress = inviteCtx.GetAddressRegex();
                                buildUnit = inviteCtx.GetBuildRegex();
                                inviteType = prjName.GetInviteBidType();
                                code = inviteCtx.GetCodeRegex();
                                if (buildUnit.Contains("联系人"))
                                {
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系人"));
                                }
                                if (buildUnit.Contains("联系电话"))
                                {
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系电话"));
                                }
                                specType = "政府采购";
                                msgType = "广州市政府采购网";
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "广州政府采购", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
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
