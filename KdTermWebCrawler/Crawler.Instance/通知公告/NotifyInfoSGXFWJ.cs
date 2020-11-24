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
    public class NotifyInfoSGXFWJ : WebSiteCrawller
    {
        public NotifyInfoSGXFWJ()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省韶关市建设与房地产信息网下发文件";
            this.Description = "自动抓取广东省韶关市建设与房地产信息网下发文件";
            this.PlanTime = "22:05";
            this.SiteUrl = "http://www.sgjsj.gov.cn/sgwebims/MessageSearchResult.aspx?ColumnID=243&KeyWord=";
            this.MaxCount = 50;
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default).GetJsString();
            }
            catch (Exception ex)
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "dataPager")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString().GetRegexBegEnd("共有：", "页");
                    pageInt = Convert.ToInt32(temp);
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
                        string dataPager_input = ToolHtml.GetHtmlInputValue(html, "dataPager_input");
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{ "__EVENTTARGET","__EVENTARGUMENT","__VIEWSTATE","searcher:txtKeyWord","searcher:tcInputDateTime:txtDateTime1",  "searcher:tcInputDateTime:txtDateTime2","searcher:ddlProvince","searcher:ddlCity1","searcher:ddlCity2","dataPager_input"
                        }, new string[]{
                        "dataPager",i.ToString(),viewState,"","","","-1","-1","-1",dataPager_input
                        });
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "p3")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = null;
                    if (nodeList.Count > 1) table = nodeList[1] as TableTag;
                    else table = nodeList[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        infoType = "通知公告";
                        headName = tr.Columns[0].ToNodePlainString();
                        releaseTime = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        infoUrl = "http://www.sgjsj.gov.cn/sgwebims/" + tr.Columns[0].GetATagValue("onclick").Replace("(", "kdxx").Replace(")", "xxdk").GetRegexBegEnd("kdxx", "xxdk").Replace("\"", "");
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table4")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            ctxHtml = dtlList.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            msgType = MsgTypeCosnt.ShaoGuanMsgType;
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "韶关市区", string.Empty, infoCtx, infoType);
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
                                    NodeList tabNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table1")));
                                    NodeList aNode = null;
                                    if (tabNode != null && tabNode.Count > 1)
                                    {
                                        parser = new Parser(new Lexer(tabNode[1].ToHtml()));
                                        aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                    }
                                    else if (tabNode != null && tabNode.Count > 0)
                                    {
                                        parser = new Parser(new Lexer(tabNode.AsHtml()));
                                        aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                    }
                                    if (aNode != null && aNode.Count > 0)
                                    {
                                        for (int a = 0; a < aNode.Count; a++)
                                        {
                                            ATag aTag = aNode[a] as ATag;
                                            if (aTag.IsAtagAttach())
                                            {
                                                try
                                                {
                                                    BaseAttach obj = ToolHtml.GetBaseAttach("http://www.sgjsj.gov.cn/sgwebims/" + aTag.Link.Replace("../", "").Replace("./", ""), aTag.LinkText, info.Id);
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
