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
    public class NoticeFSGGZY : WebSiteCrawller
    {
        public NoticeFSGGZY()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省佛山市公共资源交易网招标预告";
            this.PlanTime = "9:20,11:20,14:20,17:20";
            this.Description = "自动抓取广东省佛山市公共资源交易网招标预告";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.fsggzy.cn/gcjy/gc_zbyg/gc_ygsz/";
            this.MaxCount = 1000;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookieStr = string.Empty;
            int pageInt = 1;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString().GetRegexBegEnd("HTML", ",");
                    pageInt = int.Parse(temp.Replace("(", ""));
                }
                catch
                { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.fsggzy.cn/gcjy/gc_zbyg/gc_ygsz/index_" + (i - 1).ToString() + ".html", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "secondrightlistbox")), true), new TagNameFilter("ul")), true), new TagNameFilter("li"))); 
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty,htmlTxt=string.Empty;
                        InfoType = "招标预告";
                        InfoTitle = nodeList[j].ToNodePlainString().Replace("]", "").Replace("[", "");
                        PublistTime = nodeList[j].ToNodePlainString().GetDateRegex();
                        InfoTitle = InfoTitle.Replace(PublistTime, "");
                        InfoUrl = "http://www.fsggzy.cn/gcjy/gc_zbyg/gc_ygsz/" + nodeList[j].GetATagHref().Replace("./", ""); 
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                            htldtl = htldtl.GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "contentrightlistbox2")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            htmlTxt = dtlList.ToHtml();
                            InfoCtx = dtlList.ToHtml().Replace("</tr>", "\r\n").ToCtxString().Replace("\r\n\t", "\r\n").Replace("\r\n\r\n", "\r\n");
                            buildUnit = InfoCtx.GetBuildRegex();
                            NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "佛山市区", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.FouShanMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty,htmlTxt);
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
