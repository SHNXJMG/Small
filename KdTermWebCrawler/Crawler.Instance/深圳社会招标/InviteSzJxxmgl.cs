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
    public class InviteSzJxxmgl : WebSiteCrawller
    {
        public InviteSzJxxmgl()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "广东省深圳市建星项目管理顾问有限公司";
            this.Description = "自动抓取广东省深圳市建星项目管理顾问有限公司招标信息";
            this.PlanTime = "9:09,10:19,14:11,16:12";
            this.SiteUrl = "http://www.sz-jstar.com/zbjy_list/pmcId=42&pageNo_FrontProducts_list01-1476959936861=1&pageSize_FrontProducts_list01-1476959936861=10.html";
            this.MaxCount = 200;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "number"))), new TagNameFilter("a")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string d = pageList[1].ToHtml();
                    d = d.Replace("(", "xu");
                    string temp = d.GetRegexBegEnd("xu", ",10");
                    pageInt = int.Parse(temp);
                }
                catch
                {
                    pageInt = 1;
                }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        string a = "http://www.sz-jstar.com/zbjy_list/pmcId=42&pageNo_FrontProducts_list01-1476959936861=" + i + "&pageSize_FrontProducts_list01-1476959936861=10.html";
                        html = this.ToolWebSite.GetHtmlByUrl(a, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                            prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                            specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                            remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                            CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        code = tr.Columns[0].ToNodePlainString();
                        beginDate = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        prjName = tr.Columns[2].ToNodePlainString();
                        string p= tr.Columns[2].GetATagHref();
                        InfoUrl = "http://www.sz-jstar.com/" + tr.Columns[2].GetATagHref();
                        inviteType = tr.Columns[3].ToNodePlainString();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "FrontProducts_detail02-1476965450267")));
                        if (dtList != null && dtList.Count > 0)
                        {
                            HtmlTxt = dtList.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString().Replace("&ldquo;", "");

                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            msgType = "深圳市建星项目管理顾问有限公司";
                            specType = "其他";

                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aList != null && aList.Count > 0)
                            {
                                for (int c = 0; c < aList.Count; c++)
                                {
                                    ATag a = aList[c] as ATag;
                                    if (a.Link.IsAtagAttach())
                                    {
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, a.Link);
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
