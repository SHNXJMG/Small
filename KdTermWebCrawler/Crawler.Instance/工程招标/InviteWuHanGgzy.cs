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
    public class InviteWuHanGgzy : WebSiteCrawller
    {
        public InviteWuHanGgzy()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "武汉建设信息网招标信息";
            this.Description = "自动抓取武汉建设信息网招标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.whzbtb.com/cx/cx_2.aspx"; 
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "lb_page")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("分","页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt;i++ )
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?page=" + (i-1).ToString(), Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"),new HasAttributeFilter("id","list")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                            prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                            specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                            remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                            CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[2].GetATag();
                        prjName = aTag.LinkText;
                        inviteType = tr.Columns[3].ToNodePlainString();
                        endDate = tr.Columns[6].ToPlainTextString().GetDateRegex("yyyy/MM/dd");
                        InfoUrl = "http://www.whzbtb.com/" + aTag.Link.GetReplace("../,./").Replace("&amp;","&");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("width", "683")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.GetReplace("div,td", "span").GetReplace("</tr>", "\r\n").ToCtxString().GetReplace("\r\n\r\n\r\n\r\n\r\n\r\n", "\r\n").GetReplace("\r\n\r\n\r\n\r\n\r\n\r\n", "\r\n").GetReplace("\r\n\r\n", "\r\n").GetReplace("\r\n\r\n", "\r\n").GetReplace("\r\n\r\n", "\r\n").GetReplace("\r\n\r\n", "\r\n").GetReplace("\r\n\r\n", "\r\n").GetReplace("\r\n\r\n", "\r\n").GetReplace("\r\n\r\n", "\r\n").GetReplace("\r\n\r\n", "\r\n").GetReplace(":\r\n", "：").GetReplace("：\r\n", "：");
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            beginDate = dtlNode.AsString().GetDateRegex("yyyy/MM/dd");

                            msgType = "武汉市公共资源交易中心";
                            specType = "政府采购";
                            inviteType = "建设工程";
                            buildUnit = buildUnit.Replace(" ", "");
                            InviteInfo info = ToolDb.GenInviteInfo("湖北省", "湖北省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://www.whzbtb.com/" + a.Link.GetReplace("../,./");
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
