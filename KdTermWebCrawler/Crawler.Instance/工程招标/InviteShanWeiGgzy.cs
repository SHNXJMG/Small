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
using Winista.Text.HtmlParser.Data;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteShanWeiGgzy : WebSiteCrawller
    {
        public InviteShanWeiGgzy()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省汕尾市公共资源交易中心招标信息";
            this.Description = "自动抓取广东省汕尾市公共资源交易中心";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "ProjectName,InfoUrl";
            this.SiteUrl = "http://www.swggzy.cn/newslist.aspx?Id=4793892710e942bdb8edb883822dbab4";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl,Encoding.UTF8,ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_total_page_count")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString();
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "__EVENTVALIDATION",
                    "ctl00$ContentPlaceHolder1$CP"
                    }, new string[] { 
                    "ctl00$ContentPlaceHolder1$DownPage",
                    "",
                    viewState,
                    eventValidation,
                    i.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "secondrightlistbox")), true), new TagNameFilter("li")));
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
                        prjName = aTag.GetAttribute("title");
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.swggzy.cn/" + aTag.Link.GetReplace("amp;");
                        string htmldtl = string.Empty;
                        try
                        { 
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "contentbox2")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.GetReplace("</p>","\r\n").ToCtxString();
                            buildUnit = inviteCtx.GetReplace(" ").GetBuildRegex();
                            code = inviteCtx.GetReplace(" ").GetCodeRegex().GetCodeDel();
                            prjAddress = inviteCtx.GetReplace(" ").GetAddressRegex();
                            if (buildUnit.Contains("管理局"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("管理局")) + "管理局";
                            if (buildUnit.Contains("建设局"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("建设局")) + "建设局";
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            msgType = "汕尾市公共资源交易中心";
                            specType = inviteType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "汕尾市区", string.Empty, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag fileTag = aNode[a] as ATag;
                                    if (fileTag.IsAtagAttach())
                                    {
                                        string fileUrl = string.Empty;
                                        if (fileTag.Link.Contains("http"))
                                            fileUrl = fileTag.Link;
                                        else
                                            fileUrl = "http://www.swggzy.cn/" + fileTag.Link;

                                        base.AttachList.Add(ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, fileUrl));
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
