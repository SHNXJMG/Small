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
using System.IO;
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteSzLongGuangDiChan : WebSiteCrawller
    {
        public InviteSzLongGuangDiChan()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "广东省龙光地产采购";
            this.Description = "自动抓取广东省龙光地产采购招标信息";
            this.PlanTime = "21:08,11:12,14:02,16:33";
            this.SiteUrl = "http://pur.logan.com.cn/ZbygGrid_Grid.aspx";
            this.MaxCount = 300;
            this.ExistCompareFields = "InfoUrl";
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
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "AspNetPager1")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("总计", "条记录");
                    pageInt = int.Parse(temp)/10+1;
                }
                catch { }
            }
            for (int i = 2; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    try
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                            new string[]{
                            "__VIEWSTATE",
                            "__EVENTTARGET",
                            "__EVENTARGUMENT",
                            "__EVENTVALIDATION",
                            "hidZbygName",
                            "hidBuGUID",
                            "hidBmEndDateFrom",
                            "hidBmEndDateTo",
                            "AspNetPager1_input"
                            },
                            new string[]{
                            viewState,
                            "AspNetPager1",
                            i.ToString(),
                            eventValidation,
                            "","","","","1"
                            }
                            );
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl,nvc,Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "grid")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow row = table.Rows[j];
                        prjName = row.Columns[1].GetATagValue("title");
                        endDate = row.Columns[3].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://pur.logan.com.cn/" + row.Columns[1].GetATagHref();
                        string htmlDtl = string.Empty;
                        try
                        {
                            htmlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmlDtl));

                        NodeList nodeDtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "detial")));
                        if (nodeDtl != null && nodeDtl.Count > 0)
                        {
                            HtmlTxt = nodeDtl.AsHtml();
                            string itemHtml = string.Empty;
                            inviteCtx = "\t\t\t\t\t招标预告详情\r\n" + HtmlTxt.ToLower().Replace("<ul>", "\r\n").Replace("</ul>", "").ToCtxString() + "\r\n\r\n\r\n\r\n\t\t\t\t\t项目信息\r\n" + itemHtml.Replace("<input type='text' readonly='readonly' value='", "").Replace("<ul>", "\r\n").Replace("</ul>", "").ToCtxString().Replace("'/>", "");
                            beginDate = inviteCtx.GetRegex("发布日期").GetDateRegex("yyyy年MM月dd日");
                            HtmlTxt= inviteCtx.Replace("\r\n", "<br/>").Replace("\t", "&nbsp;&nbsp;&nbsp;");
                            code = inviteCtx.GetCodeRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                if (prjName.IndexOf("-") != -1)
                                {
                                    buildUnit = prjName.Remove(prjName.IndexOf("-"));
                                }
                            }
                            buildUnit = "龙光地产控股有限公司" + buildUnit;
                            specType = "其他";
                            inviteType = prjName.GetInviteBidType();
                            msgType = "龙光地产控股有限公司";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
