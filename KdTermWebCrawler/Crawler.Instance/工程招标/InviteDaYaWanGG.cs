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
    public class InviteDaYaWanGG : WebSiteCrawller
    {
        public InviteDaYaWanGG()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "惠州大亚湾经济技术开发区公共资源交易中心招标公告";
            this.Description = "自动抓取惠州大亚湾经济技术开发区公共资源交易中心招标公告";
            this.PlanTime = "9:10,11:10,14:10,16:10,18:10";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 200;
            this.SiteUrl = "http://zyjy.dayawan.gov.cn/website/zyjyzx/html/artList.html?cataId=201501031614328124";
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
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));

            NodeList pageo = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "leftnav")), true), new TagNameFilter("span")));
            if (pageo != null && pageo.Count > 0)
            {
                string pages = pageo.AsString().GetRegexBegEnd("条", "页");
                try
                {

                    pageInt = int.Parse(pages.Replace("/", "")); 
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&pageNo=" + i, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "div_list")), true), new TagNameFilter("ul")));
                if (nodeList != null && nodeList.Count > 0)
                {

                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                        prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                        specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                        remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                        CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://zyjy.dayawan.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "div_view")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString();
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();

                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            if (!string.IsNullOrWhiteSpace(code))
                                if (code[code.Length - 1] != '号')
                                    code = "";
                            msgType = "惠州大亚湾经济技术开发区公共资源交易中心";
                            specType = "建设工程";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            buildUnit = buildUnit.Replace(" ", "");

                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "惠州市区", "大亚湾区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://zyjy.dayawan.gov.cn/" + aFile.Link;
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
