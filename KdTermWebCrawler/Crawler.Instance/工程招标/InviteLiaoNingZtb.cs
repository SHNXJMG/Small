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
    public class InviteLiaoNingZtb : WebSiteCrawller
    {
        public InviteLiaoNingZtb()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "辽宁省招标投标监管网招标信息";
            this.Description = "自动抓取辽宁省招标投标监管网招标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.lntb.gov.cn/Article_Class2.asp?ClassID=1";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch
            {
                return null;
            }
            try
            {
                string temp = html.GetRegexBegEnd("<strong>", "</strong>").GetReplace("<fontcolor=red>1</font>/");
                pageInt = int.Parse(temp);
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&SpecialID=0&page=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter( new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "200")),true),new TagNameFilter("a")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        ATag aTag = listNode[j] as ATag;
                        if (aTag == null) continue;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                     prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                     specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                     remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                     CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;
                        string temp = aTag.GetAttribute("title");
                        prjName = temp.GetRegex("文章标题");
                        code = temp.GetRegex("招标代码");
                        beginDate = temp.GetRegex("更新时间").GetDateRegex("yyyy/MM/dd");
                        InfoUrl = "http://www.lntb.gov.cn/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "200")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToLower().GetReplace("</p>,<br/>,<br>","\r\n").ToCtxString();
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            msgType = "辽宁省招标投标协调管理办公室";
                            specType = "建设工程";
                            inviteType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("辽宁省", "辽宁省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                        else
                        {
                            Logger.Error("无内容");
                            Logger.Error(InfoUrl);
                        }
                    }
                }
            }
            return list;
        }
    }
}
