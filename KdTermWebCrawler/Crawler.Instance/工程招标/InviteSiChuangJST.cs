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
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteSiChuangJST : WebSiteCrawller
    {
        public InviteSiChuangJST()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "四川省住房和城乡建设厅招标信息";
            this.Description = "自动抓取四川省住房和城乡建设厅招标信息";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.scjst.gov.cn/main/034/034002/1.html";
            this.MaxCount = 200;
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl);
            }
            catch 
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pageConent")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("共", "页");
                    pageInt = int.Parse(temp.GetReplace("&nbsp;"));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if(i>1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.scjst.gov.cn/main/034/034002/" + i + ".html");
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "info_panel")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count - 1; j++)
                    {
                        INode node = listNode[j];
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                   prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                   specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                   remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                   CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty,
                   city = string.Empty;
                        ATag aTag = node.GetATag();
                        prjName = aTag.GetAttribute("title");
                        InfoUrl = "http://www.scjst.gov.cn/main/034/034002/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }

                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "detailcon")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.GetReplace("</span>,<br/>,<br>", "\r\n").ToCtxString();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            if (inviteCtx.IndexOf("发布日期") != -1)
                            {
                                string ctx = inviteCtx.Substring(inviteCtx.IndexOf("发布日期"), inviteCtx.Length - inviteCtx.IndexOf("发布日期"));
                                beginDate = ctx.GetDateRegex();
                            }
                            else if (inviteCtx.IndexOf("发布时间") != -1)
                            {
                                string ctx = inviteCtx.Substring(inviteCtx.IndexOf("发布时间"), inviteCtx.Length - inviteCtx.IndexOf("发布时间"));
                                beginDate = ctx.GetDateRegex();
                            }
                            if (string.IsNullOrEmpty(beginDate))
                            {
                                beginDate = DateTime.Now.ToString("yyyy-MM-dd");
                            }
                            inviteType = prjName.GetInviteBidType();
                            specType = "建设工程";
                            msgType = "四川省住房和城乡建设厅";
                            InviteInfo info = ToolDb.GenInviteInfo("四川省", "四川省及地市", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag tag = aNode[k] as ATag;
                                    if (tag.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (tag.Link.ToLower().Contains("http"))
                                            link = tag.Link;
                                        else
                                            link = "http://www.scjst.gov.cn/" + tag.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(tag.LinkText, info.Id, link);
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
