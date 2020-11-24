using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class NoticeSzLonggangBDGS : WebSiteCrawller
    {
        public NoticeSzLonggangBDGS()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市龙岗区标底公示";
            this.Description = "自动抓取广东省深圳市龙岗区标底公示";
            this.ExistCompareFields = "Prov,Area,Road,InfoTitle,InfoType,PublishTime,InfoUrl,PrjCode";
            this.SiteUrl = "http://jyzx.cb.gov.cn/LGjyzxWeb/SiteManage/BdggList.aspx?MenuName=PublicInformation&ModeId=1&ItemId=bdgs&ItemName=%e6%a0%87%e5%ba%95%e5%85%ac%e7%a4%ba&clearpaging=true";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {

            IList list = new ArrayList();

            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            string viewState = "";
            string eventValidation = "";


            //处理第一页
            DealHtml(list, html, crawlAll);
            int pageInt = 1;
            parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "ctl00_cph_context_BDGSList2_GridViewPaging1_PagingDescTd")));
            if (tdNodes != null)
            {
                string pageTemp = tdNodes[0].ToPlainTextString().Trim();
                try
                {
                    pageTemp = pageTemp.Substring(pageTemp.IndexOf("页，共")).Replace("页，共", string.Empty).Replace("页", string.Empty);
                    pageInt = int.Parse(pageTemp);
                }
                catch (Exception) { }
            }
            parser.Reset();

            //处理后续页
            if (pageInt > 1)
            {
                string cookiestr = string.Empty;
                for (int i = 2; i <= pageInt; i++)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "__VIEWSTATE","__EVENTTARGET","__EVENTARGUMENT",
                        "ctl00$cph_context$BDGSList2$GridViewPaging1$btnNext.x","ctl00$cph_context$BDGSList2$GridViewPaging1$btnNext.y",
                        "ctl00$ScriptManager1","__LASTFOCUS","ctl00$cph_context$BDGSList2$ddlProjectType",
                         "ctl00$cph_context$BDGSList1$ddlSearch",  
                        "ctl00$cph_context$BDGSList2$txtProjectName",
                        "ctl00$cph_context$BDGSList2$GridViewPaging1$txtGridViewPagingForwardTo",
                        "__VIEWSTATEENCRYPTED",  "__EVENTVALIDATION" },
                        new string[] { viewState, string.Empty, string.Empty, "2", "2", string.Empty, string.Empty, string.Empty, "gcbh", string.Empty, (i - 1).ToString(), string.Empty, eventValidation });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                        //处理后续页
                        DealHtml(list, html, crawlAll);
                    }
                    catch (Exception ex)
                    {

                        continue;
                    }

                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }
            return list;
        }

        public void DealHtml(IList list, string html, bool crawlAll)
        {
            Parser parserDtl = new Parser(new Lexer(html));
            NodeList aNodes = parserDtl.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_BDGSList2_GridView1")));
            if (aNodes != null && aNodes.Count == 1 && aNodes[0] is TableTag)
            {
                TableTag table = (TableTag)aNodes[0];
                for (int i = 1; i < table.Rows.Length; i++)
                {
                    if (table.Rows[i].Columns.Length == 7)
                    {

                        Type typs = typeof(ATag);
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, prjType = string.Empty, htmlTxt = string.Empty;
                        prjCode = table.Rows[i].Columns[1].ToPlainTextString().Trim();
                        InfoTitle = table.Rows[i].Columns[2].ToPlainTextString().Trim();
                        buildUnit = table.Rows[i].Columns[3].ToPlainTextString().Trim();
                        prjType = table.Rows[i].Columns[4].ToPlainTextString().Trim();
                        InfoType = "标底公示";
                        PublistTime = table.Rows[i].Columns[5].ToPlainTextString().Trim();
                        string urlSpilt = (table.Rows[i].Columns[2].Children.SearchFor(typs, true)[0] as ATag).Link;

                        string url = urlSpilt.Substring(urlSpilt.IndexOf("\"")).Replace("\"", "").Replace(")", "");
                        InfoUrl = "http://jyzx.cb.gov.cn/LGjyzxWeb/SiteManage/" + url;
                        string ctxhtml = string.Empty;
                        try
                        {
                            ctxhtml = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                        }
                        catch (Exception ex)
                        { 
                            continue;
                        }

                        Parser parserCtx = new Parser(new Lexer(ctxhtml));


                        NodeList ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "text")));

                        InfoCtx = ctxNode.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_cph_context_lblContent")), true).AsString().Replace("&nbsp;", "");

                        htmlTxt = ctxNode.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_cph_context_lblContent")), true).AsHtml();

                        NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳龙岗区工程", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "深圳市建设工程交易中心龙岗分中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, prjType, string.Empty, htmlTxt);

                        list.Add(info);

                        NodeList fileNode = ctxNode.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_AccessoriesControl1_GridView1")), true);
                        if (fileNode != null && fileNode.Count > 0 && fileNode[0] is TableTag)
                        {
                            TableTag fileTable = fileNode[0] as TableTag;
                            for (int j = 1; j < fileTable.Rows.Length; j++)
                            {
                                BaseAttach attach = ToolDb.GenBaseAttach(fileTable.Rows[j].Columns[0].ToPlainTextString().Trim(), info.Id, "http://jyzx.cb.gov.cn/LGjyzxWeb/" + (fileTable.Rows[j].Columns[0].SearchFor(typs, true)[0] as ATag).Link.Replace("../", ""));
                                base.AttachList.Add(attach);
                            }

                        }

                        if (!crawlAll && list.Count >= this.MaxCount) return;
                    }
                }
            }
            parserDtl.Reset();
        }
    }
}
