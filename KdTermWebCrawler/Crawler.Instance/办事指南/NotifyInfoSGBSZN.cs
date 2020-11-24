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
    public class NotifyInfoSGBSZN : WebSiteCrawller
    {
        public NotifyInfoSGBSZN()
            : base()
        {
            this.Group = "办事指南";
            this.Title = "广东省韶关市建设与房地产信息网办事指南";
            this.Description = "自动抓取广东省韶关市建设与房地产信息网办事指南";
            this.PlanTime = "21:00";
            this.SiteUrl = "http://www.sgjsj.gov.cn/html/service/banshi/";
            this.MaxCount = 50;
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
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
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "__EVENTTARGET","__EVENTARGUMENT","__VIEWSTATE","searcher:txtKeyWord","searcher:tcInputDateTime:txtDateTime1",
                        "searcher:tcInputDateTime:txtDateTime2","searcher:ddlProvince","searcher:ddlCity1","searcher:ddlCity2"
                        }, new string[]{
                        "dataPager",i.ToString(),viewState,"","","","-1","-1","-1"
                        });
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "list")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;
                        
                        infoType = "办事指南";
                        headName = nodeList[j].GetATagValue("Txt");
                         releaseTime = nodeList[j].ToPlainTextString().GetDateRegex();
                        infoUrl= nodeList[j].GetATagHref();
                       // infoUrl = "http://www.sgjsj.gov.cn/sgwebims/" + tr.Columns[0].GetATagValue("onclick").Replace("(", "kdxx").Replace(")", "xxdk").GetRegexBegEnd("kdxx", "xxdk").Replace("\"", "");
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "crt fr")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            ctxHtml = dtlList.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            msgType = MsgTypeCosnt.ShaoGuanMsgType;
                            headName = infoCtx.GetRegexBegEnd("列表\r\n", "\r\n");
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
