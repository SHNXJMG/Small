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
    public class InviteGdZfcg : WebSiteCrawller
    {
        public InviteGdZfcg()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "广东省政府采购网招标信息";
            this.Description = "自动抓取广东省政府采购网招标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 200;
            this.SiteUrl = "http://www.gdgpo.gov.cn/queryMoreInfoList/channelCode/0005.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string postUrl = "http://www.gdgpo.gov.cn/queryMoreInfoList.do";
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("form"), new HasAttributeFilter("name", "qPageForm")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString();
                    temp = temp.GetRegexBegEnd("共", "条");
                    int total = int.Parse(temp);
                    pageInt = total / 15 + 1;
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                    "channelCode",
                    "pointPageIndexId",
                    "pageIndex",
                    "pageSize"
                    }, new string[] {
                    "0005",
                    "1",
                    i.ToString(),
                    "15"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(postUrl, nvc);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "m_m_c_list")), true), new TagNameFilter("li")));
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

                        ATag aTag = node.GetATag(1);
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        prjName = aTag.GetAttribute("title");
                        InfoUrl = "http://www.gdgpo.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "zw_c_c_cont")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.GetReplace("</p>,<br/>","\r\n").ToCtxString();
                             
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            inviteType = prjName.GetInviteBidType();
                            specType = "政府采购";
                            msgType = "广东省财政厅政府采购";

                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "广州政府采购", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNodes = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNodes != null && aNodes.Count > 0)
                            {
                                for (int a = 0; a < aNodes.Count; a++)
                                {
                                    ATag aFile = aNodes[a] as ATag;
                                    if (aFile.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (aFile.Link.ToLower().Contains("http"))
                                            link = aFile.Link;
                                        else
                                            link = "http://www.gdgpo.gov.cn/" + aFile.Link;
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
