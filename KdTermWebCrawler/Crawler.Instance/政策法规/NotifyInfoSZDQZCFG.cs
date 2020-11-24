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
    public class NotifyInfoSZDQZCFG : WebSiteCrawller
    {
        public NotifyInfoSZDQZCFG()
            : base()
        {
            this.Group = "政策法规";
            this.Title = "广东省深圳市建设工程交易中心深圳政策法规";
            this.Description = "自动抓取广东省深圳市建设工程交易中心深圳政策法规";
            this.PlanTime = "1 22:22";
            this.SiteUrl = "http://www.szjsjy.com.cn/ServiceGuide/PolicyRegulations.aspx?id=633";
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
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView3")));
            if (nodeList != null && nodeList.Count > 0)
            {
                TableTag table = nodeList[0] as TableTag;
                for (int j = 1; j < table.RowCount; j++)
                {
                    string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                    TableRow tr = table.Rows[j];
                    infoType = "政策法规";
                    infoScorce = tr.Columns[2].ToNodePlainString();
                    headName = tr.Columns[1].ToNodePlainString();
                    releaseTime = tr.Columns[3].ToPlainTextString().GetDateRegex();
                    infoUrl = "http://www.szjsjy.com.cn/ServiceGuide/" + tr.Columns[1].GetATagHref();
                    string htldtl = string.Empty;
                    try
                    {
                        htldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.UTF8).GetJsString();
                    }
                    catch { continue; }
                    parser = new Parser(new Lexer(htldtl));
                    NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table3")));
                    if (dtList != null && dtList.Count > 0)
                    {
                        ctxHtml = dtList.AsHtml();
                        infoCtx = ctxHtml.ToCtxString();
                        msgType = MsgTypeCosnt.ShenZhenMsgType;
                        NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳市工程", string.Empty, infoCtx, infoType);
                        sqlCount++;
                        if (!crawlAll && sqlCount >= this.MaxCount)
                        {
                            return null;
                        }
                        if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                        {
                            parser = new Parser(new Lexer(ctxHtml));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag aTag = aNode[a] as ATag;
                                    if (aTag.IsAtagAttach())
                                    {
                                        try
                                        {
                                            BaseAttach baseAttach = ToolHtml.GetBaseAttach("http://www.szjsjy.com.cn/" + aTag.Link.Replace("../", "").Replace("./", ""), aTag.LinkText, info.Id);
                                            if (baseAttach != null)
                                                ToolDb.SaveEntity(baseAttach, string.Empty);
                                        }
                                        catch { }
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
