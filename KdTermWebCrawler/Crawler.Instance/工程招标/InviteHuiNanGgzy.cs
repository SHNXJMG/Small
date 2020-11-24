using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
namespace Crawler.Instance
{
    public class InviteHuiNanGgzy : WebSiteCrawller
    {
        public InviteHuiNanGgzy()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "湖南省公共资源交易中心招标信息";
            this.Description = "自动抓取湖南省公共资源交易中心招标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.SiteUrl = "http://www.hngzzx.com/HomePage/ShowList.aspx?tbid=1&TypeAll=1";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_pager1")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToPlainTextString().GetRegexBegEnd("共", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                    "__VIEWSTATE",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "input"
                    },
                        new string[] {
                        viewState,
                        "ctl00$ContentPlaceHolder1$pager1",
                        i.ToString(),
                        ""
                        });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                        //string posts = string.Empty;
                        //for (int j = 0; j < nvc.Count; j++)
                        //{
                        //    posts += nvc.AllKeys[j] + "=" + nvc[j] + "&";
                        //}
                        //posts = posts.Substring(0, posts.Length - 1);
                        //html = ToolHtml.GetHtmlByUrlPost(this.SiteUrl, posts, Encoding.UTF8, ref cookiestr);
                    }
                    catch { }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_con_main_bulcon")), true), new TagNameFilter("li")));

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
                            CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        prjName = aTag.GetAttribute("title");
                        beginDate = node.ToPlainTextString().GetDateRegex();

                        string LinkId = aTag.Link.GetRegexBegEnd("Id=", "&");
                        InfoUrl = "http://www.hngzzx.com/HomePage/ShowInfoDetail.aspx?Id=" + LinkId + "&TableID=1";
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "detail_con")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.GetReplace("</p>", "\r\n").ToCtxString();

                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();

                            specType = inviteType = "政府采购";
                            msgType = "湖南省公共资源交易中心";

                            InviteInfo info = ToolDb.GenInviteInfo("湖南省", "湖南省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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

