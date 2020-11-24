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
    public class InviteHeYuanCGYZ : WebSiteCrawller
    {
        public InviteHeYuanCGYZ()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "河源市公共资源交易中心招标摇珠";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取河源市公共资源交易中心招标摇珠";
            this.SiteUrl = "http://www.hyggzy.gov.cn/zfcg/subpage.html";
            this.MaxCount = 100;
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            // NodeList pageNode = parser.ExtractAllNodesThatMatch((new TagNameFilter("div")));

            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "ewb-page-items clearfix")));
            if (pageNode != null && pageNode.Count > 0)
            {
               string sa = pageNode.AsHtml();
               string dd = sa.Replace("</p>", "\r\n").ToCtxString();
               string temp = dd.GetRegexBegEnd("1/", "\r").GetReplace("\r\n", "");
                pageInt = int.Parse(temp);

            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.hyggzy.gov.cn/zfcg/" + i + ".html");
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "ewb-com-item clearfix")));
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
                        InfoUrl = "http://www.hyggzy.gov.cn"+aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ewb-list-bd")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {

                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString();


                            prjAddress = inviteCtx.GetRegex("公开摇珠地点");
                            beginDate = inviteCtx.GetRegex("发布时间");
                            beginDate = beginDate.GetDateRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (string.IsNullOrWhiteSpace(prjAddress))
                                prjAddress = inviteCtx.GetAddressRegex();

                            
                            prjAddress = inviteCtx.GetRegex(new string[] { "开标地点" }).GetCodeDel();
                            if (string.IsNullOrWhiteSpace(prjAddress))
                                prjAddress = inviteCtx.GetRegex("投标及开标地点");


                            if (string.IsNullOrWhiteSpace(buildUnit))
                                buildUnit = inviteCtx.GetRegex("项目单位");

                            
                            if (string.IsNullOrWhiteSpace(endDate))
                            endDate = inviteCtx.GetRegexBegEnd("至", "止");
                            endDate = endDate.GetReplace("年", "-").GetReplace("月", "-").GetReplace("日", "");


                            code = inviteCtx.GetCodeRegex(new string[] { "项目编号"}).GetCodeDel();
                            if (string.IsNullOrWhiteSpace(code))
                                code = inviteCtx.GetRegexBegEnd("编号：", "采购").GetReplace("\r\n", "");
                            if (string.IsNullOrWhiteSpace(code))
                                code = inviteCtx.GetRegexBegEnd("项目（", "）").GetReplace("\r\n", "");




                            msgType = "河源市公共资源交易中心";
                            specType = "政府采购";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "河源市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k] as ATag;
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://www.hyggzy.gov.cn" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
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
