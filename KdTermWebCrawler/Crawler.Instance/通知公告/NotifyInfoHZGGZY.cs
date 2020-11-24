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
    public class NotifyInfoHZGGZY : WebSiteCrawller
    {
        public NotifyInfoHZGGZY()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省惠州市公共资源交易中心";
            this.Description = "自动抓取广东省惠州市公共资源交易中心通知公告";
            this.PlanTime = "21:54";
            this.MaxCount = 500;
            this.SiteUrl = "http://zyjy.huizhou.gov.cn/pages/cms/hzggzyjyzx/html/artList.html?cataId=8297356fa5684b0abd9898ec535fe535";
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
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagination")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString();
                    pageInt = Convert.ToInt32(temp.GetRegexBegEnd("/", "页"));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "&pageNo=" + i.ToString(), Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "div_list")),true),new TagNameFilter("ul")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        headName = nodeList[j].GetATagValue("title");
                        releaseTime = nodeList[j].ToPlainTextString().GetDateRegex();

                        infoType = "通知公告";

                        infoUrl = "http://zyjy.huizhou.gov.cn" + nodeList[j].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.UTF8);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(html));
                        NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "divZoom")));
                        if (dtList != null && dtList.Count > 0)
                        {
                            ctxHtml = dtList.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            msgType = "惠州市公共资源交易中心";

                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "惠州市区", string.Empty, infoCtx, infoType);
                            sqlCount++;
                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return null;
                            }
                            ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate);
                        }
                    }
                }
            }
            return null;
        }
    }
}
