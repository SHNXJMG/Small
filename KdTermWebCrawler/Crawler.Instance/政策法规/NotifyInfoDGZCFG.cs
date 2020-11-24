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
    public class NotifyInfoDGZCFG : WebSiteCrawller
    {
        public NotifyInfoDGZCFG()
            : base()
        {
            this.Group = "政策法规";
            this.Title = "广东省东莞市建设工程交易中心地方政策法规";
            this.Description = "自动抓取广东省东莞市建设工程交易中心地方政策法规";
            this.PlanTime = "1 22:24";
            this.SiteUrl = "http://www.dgzb.com.cn/DGJYWEB/SiteManage/Policy_List.aspx?ModeId=2";
            this.MaxCount = 500;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            //取得页码
            int pageInt = 1, sqlCount = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_cph_context_GridViewPaingTwo1_lblGridViewPagingDesc")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString();
                    pageInt = Convert.ToInt32(temp.GetRegexBegEnd("共", "页"));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
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
                            "__EVENTVALIDATION",
                            "ctl00$cph_context$GridViewPaingTwo1$txtGridViewPagingForwardTo",
                            "ctl00$cph_context$GridViewPaingTwo1$btnForwardToPage"},
                            new string[]{
                            viewState,eventValidation,i.ToString(),"GO"
                            }
                            );
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_GridView1")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty,
                              infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        headName = tr.Columns[1].ToNodePlainString();
                        releaseTime = tr.Columns[2].ToNodePlainString();
                        infoType = "政策法规";
                        infoUrl = "http://www.dgzb.com.cn/DGJYWEB/SiteManage/" + tr.Columns[1].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = ToolHtml.GetHtmlByUrl(infoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList noList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_cph_context_span_MetContent")));
                        if (noList != null && noList.Count > 0)
                        {
                            ctxHtml = noList.AsHtml().Replace("<br/>", "\r\n").Replace("<BR/>", "");
                            infoCtx = noList.AsString().Replace(" ", "").Replace("&nbsp;", "").Replace("\t\t", "\t").Replace("\t\t", "\t");
                            infoCtx = Regex.Replace(infoCtx, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase).Replace(" ", "").Replace("\t", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            msgType = MsgTypeCosnt.DongGuanMsgType;
                            infoScorce = infoScorce.Replace("&nbsp;", "");
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "东莞市区", string.Empty, infoCtx, infoType);

                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return null;
                            }
                            else
                            {
                                sqlCount++;
                                if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                                {
                                    parser = new Parser(new Lexer(htldtl));
                                    NodeList attachList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_DownLoadFiles1_GridView1")));
                                    if (attachList != null && attachList.Count > 0)
                                    {
                                        TableTag tabTag = attachList[0] as TableTag;
                                        for (int k = 1; k < tabTag.RowCount; k++)
                                        {
                                            TableRow dr = tabTag.Rows[k];
                                            try
                                            {
                                                string attName = string.IsNullOrEmpty(dr.Columns[1].ToNodePlainString()) ? headName : dr.Columns[1].ToNodePlainString();
                                                BaseAttach baseInfo = ToolHtml.GetBaseAttachByUrl("http://www.dgzb.com.cn/DGJYWEB/SiteManage/" + dr.Columns[1].GetATagHref(), attName, info.Id);
                                                if (baseInfo != null)
                                                {
                                                    ToolDb.SaveEntity(baseInfo, string.Empty);
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
