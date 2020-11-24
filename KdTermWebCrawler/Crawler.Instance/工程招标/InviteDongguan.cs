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
    /// <summary>
    /// 广东省东莞市
    /// </summary>
    public class InviteDongguan : WebSiteCrawller
    {
        public InviteDongguan()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省东莞市";
            this.Description = "自动抓取广东省东莞市区招标信息";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "https://www.dgzb.com.cn/ggzy/website/WebPagesManagement/jslist?fcInfotype=1&tenderkind=A&projecttendersite=SS&TypeIndex=0&KindIndex=0";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38"; 
            this.MaxCount = 50; 
        } 

        protected override IList ExecuteCrawl(bool crawlAll)
        {

            IList list = new List<InviteInfo>();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
           // NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "kkpager"), new TagNameFilter("span")));

            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("script"), new HasAttributeFilter("type", "text/javascript")));
            string pageString = sNode.AsString();
            Regex regexPage = new Regex(@"共[^页]+页");
            Match pageMatch = regexPage.Match(pageString);
            try { pageInt = int.Parse(pageMatch.Value.Replace("共", "").Replace("页", "").Trim()); }
            catch (Exception) { }

            string cookiestr = string.Empty;
            for (int i = 1; i < pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__LASTFOCUS", "__VIEWSTATE", "__EVENTVALIDATION", "ctl00$cph_context$drp_selSeach", "ctl00$cph_context$txt_strWhere", "ctl00$cph_context$drp_Rq", "ctl00$cph_context$GridViewPaingTwo1$txtGridViewPagingForwardTo", "ctl00$cph_context$GridViewPaingTwo1$btnNext.x", "ctl00$cph_context$GridViewPaingTwo1$btnNext.y" }, new string[] { string.Empty, string.Empty, string.Empty, viewState, eventValidation, "1", string.Empty, "3", (i - 1).ToString(), "9", "7" });
                    try { html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr); }
                    catch (Exception ex) { continue; }
                }

                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_GridView1"), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j] as TableRow;
                        code = tr.Columns[1].ToPlainTextString().Trim();
                        prjName = tr.Columns[2].ToPlainTextString().Trim();
                        buildUnit = tr.Columns[3].ToPlainTextString().Trim();
                        beginDate = tr.Columns[4].ToPlainTextString().Trim().GetReplace(" - ", "&").Split('&')[0].Trim();
                        try
                        {
                            endDate = tr.Columns[4].ToPlainTextString().Trim().GetReplace(" - ", "&").Split('&')[1].Trim();
                        }
                        catch { }
                        ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://www.dgzb.com.cn:8080/dgjyweb/sitemanage/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_span_MetContent"), new TagNameFilter("span")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n").Replace("<br/>", "\r\n");
                        }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_span_MetContent"), new TagNameFilter("span")));

                        inviteCtx = dtnode.AsString().Replace(" ", "");
                        Regex regPrjAdd = new Regex(@"(工程地点|工程地址)：[^\r\n]+[\r\n]{1}");
                        prjAddress = regPrjAdd.Match(inviteCtx).Value.Replace("工程地点：", "").Replace("工程地址：", "").Trim();
                        msgType = "东莞市建设工程交易中心";
                        specType = "建设工程";
                        inviteType = ToolHtml.GetInviteTypes(prjName);
                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "东莞市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                        list.Add(info);
                        dtlparser.Reset();
                        NodeList fileNode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_DownLoadFiles1_GridView2"), new TagNameFilter("table")));
                        if (fileNode != null && fileNode.Count > 0 && fileNode[0] is TableTag)
                        {
                            TableTag fileTable = fileNode[0] as TableTag;
                            for (int f = 1; f < fileTable.Rows.Length; f++)
                            {
                                BaseAttach attach = ToolDb.GenBaseAttach(fileTable.Rows[f].Columns[1].ToPlainTextString().Trim(), info.Id, "http://www.dgzb.com.cn:8080/dgjyweb/sitemanage/" + (fileTable.Rows[f].Columns[1].SearchFor(typeof(ATag), true)[0] as ATag).Link);
                                base.AttachList.Add(attach);
                            }

                        }
                        if (!crawlAll && list.Count >= this.MaxCount) return list;


                    }
                }
            }
            return list;

        }

    }
}
