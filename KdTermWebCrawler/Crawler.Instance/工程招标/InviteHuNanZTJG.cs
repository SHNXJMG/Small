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
    public class InviteHuNanZTJG : WebSiteCrawller
    {
        public InviteHuNanZTJG()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "湖南省招标投标监管网招标公示";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取湖南省招标投标监管网招标信息";
            this.SiteUrl = "http://www.bidding.hunan.gov.cn/item/itemInviteBaseInfo.aspx";
            this.MaxCount = 200;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "WebPager1_LabelLbtMsg")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("共", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                         "__EVENTTARGET",
                         "__EVENTARGUMENT",
                         "__VIEWSTATE",
                         "__VIEWSTATEENCRYPTED",
                         "__EVENTVALIDATION",
                         "tbProclaimTitle",
                         "cldReleasetTimeFrom_year",
                         "cldReleasetTimeFrom_month",
                         "cldReleasetTimeFrom_day",
                         "cldReleasetTimeFrom",
                         "cldReleasetTimeTo_year",
                         "cldReleasetTimeTo_month",
                         "cldReleasetTimeTo_day",
                         "cldReleasetTimeTo",
                         "ucArea$ddlCodeList",
                         "ucType$ddlCodeList",
                         "tsubmitPerson",
                         "tbagentOrg",
                         "WebPager1$tbLbtPage"

                    }, new string[] {
                         "WebPager1$lbtNavGo",
                        "",
                        viewState,
                        "",
                        eventValidation,
                        "","","","","","","","","","-1","-1","","",i.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "gvItemBuild")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        if (aTag == null) continue;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;


                        prjName = aTag.LinkText;
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.bidding.hunan.gov.cn/item/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        Parser parserdetail = new Parser(new Lexer(htmldtl));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("align", "center")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            if (string.IsNullOrWhiteSpace(buildUnit))
                                buildUnit = inviteCtx.GetRegex("招　 标　人");

                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("联系")|| buildUnit.Contains("电话"))
                                buildUnit = "";
                            msgType = "湖南省发展和改革委员会";
                            specType = "建设工程";
                            inviteType = ToolHtml.GetInviteTypes(prjName);

                            InviteInfo info = ToolDb.GenInviteInfo("湖南省", "湖南省及地市", "",
                             string.Empty, code, prjName, prjAddress, buildUnit,
                             beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://www.bidding.hunan.gov.cn/" + a.Link;
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
