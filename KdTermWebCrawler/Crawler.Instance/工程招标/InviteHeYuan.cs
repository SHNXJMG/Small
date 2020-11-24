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
    public class InviteHeYuan : WebSiteCrawller
    {
        public InviteHeYuan()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省河源市工程建设招标信息";
            this.Description = "自动抓取广东省河源市工程建设招标信息";
            this.ExistCompareFields = "Code,ProjectName,InfoUrl";
            this.SiteUrl = "http://www.hyggzy.gov.cn/jsgc/subpage.html";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            // NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new TagNameFilter("select"));

            NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "ewb-page-items clearfix")));
            if (tableNodeList != null && tableNodeList.Count > 0)
            {
                try
                {
                    string sa = tableNodeList.AsHtml();
                    string dd = sa.Replace("</p>", "\r\n").ToCtxString();
                    string temp = dd.GetRegexBegEnd("1/", "\r").GetReplace("\r\n", "");
                    page = int.Parse(temp);
                }
                catch
                { }
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl("http://www.hyggzy.gov.cn/jsgc/" + i + ".html", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "ewb-com-item clearfix")));
                if (nodeList.Count > 0)
                {
                    
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                 prjAddress = string.Empty, inviteCtx = string.Empty, bidType = string.Empty,
                                 specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                 remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                 CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.GetAttribute("title");

                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                         
                        InfoUrl = "http://www.hyggzy.gov.cn" + aTag.Link.Replace("amp;", "").Trim();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ewb-list-bd")));
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();

                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();

                            msgType = "河源市公共资源交易中心";
                            specType = "建设工程";
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "河源市区", "",
                                              string.Empty, code, prjName, prjAddress, buildUnit,
                                 beginDate, endDate, inviteCtx, remark, msgType, bidType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return null;
        }
    }
}
