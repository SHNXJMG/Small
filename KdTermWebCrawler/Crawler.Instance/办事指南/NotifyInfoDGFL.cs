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
    public class NotifyInfoDGFL : WebSiteCrawller
    {
        public NotifyInfoDGFL()
            : base()
        {
            this.Group = "办事指南";
            this.Title = "广东省东莞市建设工程交易中心（附录）办事指南";
            this.Description = "自动抓取广东省东莞市建设工程交易中心（附录）办事指南";
            this.PlanTime = "21:14";
            this.SiteUrl = "http://www.dgzb.com.cn/DGJYWEB/SiteManage/Bszl_List.aspx?modeId=6";
            this.MaxCount = 500;
            this.ExistCompareFields = "InfoUrl";
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
                    string temp = pageList.AsString().GetRegexBegEnd("共", "页");
                    pageInt = Convert.ToInt32(temp);
                }
                catch { pageInt = 1; }
            } 
            for (int i = 1; i <= pageInt; i++)
            {
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_GridView1")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        infoType = "办事指南";
                        headName = tr.Columns[1].ToNodePlainString();
                        releaseTime = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        infoUrl = "http://www.dgzb.com.cn/DGJYWEB/SiteManage/" + tr.Columns[1].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = ToolHtml.GetHtmlByUrlEncode(infoUrl, Encoding.UTF8);
                        }
                        catch {  }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "line")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            ctxHtml = dtlList.AsHtml();
                            infoCtx = dtlList.AsString();
                            msgType = MsgTypeCosnt.DongGuanMsgType;
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
                                    NodeList aNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_DownLoadFiles1_GridView1")));
                                    if (aNode != null && aNode.Count > 0)
                                    {
                                        TableTag tab = aNode[0] as TableTag;
                                        for (int a = 1; a < tab.RowCount; a++)
                                        {
                                            TableRow dr = tab.Rows[a];
                                            ATag aTag = dr.Columns[1].GetATag();
                                            if (aTag.IsAtagAttach())
                                            {
                                                try
                                                {
                                                    BaseAttach obj = ToolHtml.GetBaseAttach("http://www.dgzb.com.cn/DGJYWEB/SiteManage/" + aTag.Link, aTag.LinkText, info.Id);
                                                    if (obj != null)
                                                        ToolDb.SaveEntity(obj, string.Empty);
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
            }
            return null;
        }
    }
}
