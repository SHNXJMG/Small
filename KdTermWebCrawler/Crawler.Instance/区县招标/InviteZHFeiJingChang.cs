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

namespace Crawler.Instance
{
    public class InviteZHFeiJingChang : WebSiteCrawller
    {
        public InviteZHFeiJingChang()
            : base()
        {
            this.Group = "区县招标信息";
            this.Title = "广东省珠海市区非进场";
            this.Description = "自动抓取广东省珠海市区非进场";
            this.PlanTime = "9:01,10:23,14:14,16:15";
            this.SiteUrl = "http://www.cpinfo.com.cn/index/showList/000000000001/000000000427";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "scott")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.ToHtml().GetATagHref(5);
                    string page = temp.Replace("goPage(", "").Replace(")", "");
                    pageInt = Convert.ToInt32(page);
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
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                            new string[] { "newtitle", "totalRows", "pageNO" }, new string[] { "", "0", i.ToString() }
                            );
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "cnewslist")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 2; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                        prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                        specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                        remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                        CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[0].ToPlainTextString().ToNodeString();
                        endDate = tr.Columns[1].ToPlainTextString();
                        inviteType = prjName.GetInviteBidType();
                        InfoUrl = tr.Columns[0].ToHtml().GetATagHref();
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                            htlDtl = htlDtl.GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "pagedeteil")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            HtmlTxt = dtlList.ToHtml();
                            string ctx = string.Empty;
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList ctxList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                            if (ctxList != null && ctxList.Count > 0)
                            {
                                TableTag tab = ctxList[0] as TableTag;
                                for (int k = 0; k < tab.RowCount; k++)
                                {
                                    for (int d = 0; d < tab.Rows[k].ColumnCount; d++)
                                    {
                                        if (d % 2 == 0)
                                            ctx += tab.Rows[k].Columns[d].ToPlainTextString().Replace(" ", "") + "：";
                                        else
                                            ctx += tab.Rows[k].Columns[d].ToPlainTextString().Replace(" ", "") + "\r\n";
                                    }
                                }
                            }
                            inviteCtx = HtmlTxt.ToCtxString().Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");

                            buildUnit = ctx.GetBuildRegex();
                            code = ctx.GetCodeRegex();
                            prjAddress = ctx.GetAddressRegex();
                            beginDate = inviteCtx.GetRegex("报名时间").GetDateRegex();
                            if (string.IsNullOrEmpty(beginDate) || DateTime.Parse(beginDate) > DateTime.Now)
                                beginDate = DateTime.Now.ToString("yyyy-MM-dd");
                            msgType = "珠海市建设工程信息网";
                            specType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "珠海市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
