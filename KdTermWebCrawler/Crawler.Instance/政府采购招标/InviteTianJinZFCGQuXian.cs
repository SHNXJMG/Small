using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class InviteTianJinZFCGQuXian : WebSiteCrawller
    {
        public InviteTianJinZFCGQuXian()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "天津市政府采购网(区县级采购信息)";
            this.Description = "自动抓取天津市政府采购网(区县级采购信息)";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 1000;
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.tjgp.gov.cn/portal/topicView.do?method=view&view=Infor&id=1664&ver=2&stmp=1484299193522";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "pagesColumn")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    pageInt = int.Parse(pageNode[0].ToNodePlainString().GetRegexBegEnd("共", "页"));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                   "method",
                   "page",
                   "id",
                   "step",
                   "view", 
                   "ldateQGE",
                   "ldateQLE"
                    }, new string[]{
                   "view",
                   i.ToString(),
                   "1664",
                   "1",
                   "Infor",
                   "",""
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "reflshPage")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        ATag aTag = listNode[j].GetATag();
                        if (aTag == null) continue;
                        prjName = aTag.GetAttribute("title");
                        string tempCode = prjName.GetReplace("(项", "kdxx").GetReplace(")", "）").GetRegexBegEnd("kdxx", "）");
                        code = tempCode.GetReplace("目编号：,目编号:");
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.tjgp.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = System.Web.HttpUtility.HtmlDecode(HtmlTxt.ToCtxString());
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            inviteType = prjName.GetInviteBidType();
                            specType = "政府采购";
                            msgType = "天津政府采购办公室";
                            InviteInfo info = ToolDb.GenInviteInfo("天津市", "天津市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag aFile = aNode[a] as ATag;
                                    if (aFile.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (aFile.Link.Contains("http"))
                                            link = aFile.Link;
                                        else
                                            link = "http://www.tjgp.gov.cn/" + aFile.Link;
                                        string text = System.Web.HttpUtility.HtmlDecode(aFile.LinkText);
                                        base.AttachList.Add(ToolDb.GenBaseAttach(text, info.Id, link));
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
