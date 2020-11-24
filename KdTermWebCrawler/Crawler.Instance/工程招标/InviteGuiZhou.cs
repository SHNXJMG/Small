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
    public class InviteGuiZhou : WebSiteCrawller
    {
        public InviteGuiZhou()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "贵州省住房和城乡建设厅招标信息";
            this.Description = "自动抓取贵州省住房和城乡建设厅招标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 500;
            this.SiteUrl = "http://www.gzjyfw.gov.cn/gcms/queryZjt.jspx?title=&businessCatalog=&businessType=JYGG&inDates=0&ext=&origin=ALL";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "pages-list")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("/", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    string pageUrl = string.Format("http://www.gzjyfw.gov.cn/gcms/queryZjt_" + i + ".jspx?title=&businessCatalog=&businessType=JYGG&inDates=0&ext=&origin=ALL");
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(pageUrl);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("id", "news_list1")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string prjName = string.Empty;
                        ATag aTag = listNode[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        string code = string.Empty, buildUnit = string.Empty,
             prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
             specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
             remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
             CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        area = listNode[j].GetSpan().ToNodePlainString();
                        InfoUrl = aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "contents")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = System.Web.HttpUtility.HtmlDecode(dtlNode.AsHtml()).Replace(" ", ""); ;
                            inviteCtx = HtmlTxt.ToCtxString().Replace(" ", ""); ;

                            code = inviteCtx.GetCodeRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            specType = "建设工程";
                            inviteType = prjName.GetInviteBidType();
                            msgType = "贵州省住房和城乡建设厅";
                            if (buildUnit.Contains("运输局"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("运输局")) + "运输局";
                            if (buildUnit.Contains("管理局"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("管理局")) + "管理局";
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                            InviteInfo info = ToolDb.GenInviteInfo("贵州省", "贵州省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag fileTag = aNode[a].GetATag();
                                    if (fileTag.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (fileTag.Link.Contains("http"))
                                            link = fileTag.Link;
                                        else
                                            link = "http://www.gzjyfw.gov.cn/" + fileTag.Link;
                                        base.AttachList.Add(ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, link));
                                    }
                                }
                            }
                            if (!crawlAll && list.Count > this.MaxCount) return list;
                        }

                    }
                }
            }
            return list;
        }
    }
}
