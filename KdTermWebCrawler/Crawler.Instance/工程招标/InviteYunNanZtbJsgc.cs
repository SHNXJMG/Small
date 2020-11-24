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
    public class InviteYunNanZtbJsgc : WebSiteCrawller
    {
        public InviteYunNanZtbJsgc()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "云南省公共资源建设工程招标信息";
            this.Description = "自动抓取云南省公共资源建设工程招标信息";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.ynggzy.net/bulletininfo.do?method=bulletinMore&hySort=2&bulletinclass=01";
            this.MaxCount = 400;
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "bulletininfotable_toolbarTable")));
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
                    string bulletininfotable_totalpages = ToolHtml.GetHtmlInputValue(html, "bulletininfotable_totalpages");
                    string bulletininfotable_totalrows = ToolHtml.GetHtmlInputValue(html, "bulletininfotable_totalrows");
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "ec_i",
                    "bulletininfotable_efn",
                    "bulletininfotable_crd",
                    "bulletininfotable_p",
                    "bulletininfotable_s_bulletintitle",
                    "bulletininfotable_s_finishday",
                    "hySort",
                    "findAjaxZoneAtClient",
                    "method",
                    "bulletinclass",
                    "bulletininfotable_totalpages",
                    "bulletininfotable_totalrows",
                    "bulletininfotable_pg",
                    "bulletininfotable_rd"
                    },
                        new string[]{
                            "bulletininfotable",
                            "",
                            "20",
                            i.ToString(),
                            "",
                            "",
                            "2",
                            "false",
                            "bulletinMore",
                            "01",
                            bulletininfotable_totalpages,
                            bulletininfotable_totalrows,
                            (i-1).ToString(),
                            "5"
                        });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html.Replace("tbody", "table")));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "bulletininfotable_table_body")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty,
                         city = string.Empty;

                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[0].ToNodePlainString();
                        beginDate = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.ynggzy.net/bulletin.do?method=showbulletin&bulletin_id=" + tr.GetAttribute("id");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = ToolHtml.GetHtmlByUrl(this.SiteUrl, InfoUrl, Encoding.Default);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToLower().GetReplace("</p>,<br />,<br/>,<br>", "\r\n").ToCtxString();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            inviteType = prjName.GetInviteBidType();
                            specType = "建设工程";
                            msgType = "云南省公共资源交易中心";
                            InviteInfo info = ToolDb.GenInviteInfo("云南省", "云南省及地市", city, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://www.ynggzy.net/" + a.Link;
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
