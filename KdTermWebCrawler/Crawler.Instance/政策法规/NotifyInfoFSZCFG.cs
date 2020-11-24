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
    public class NotifyInfoFSZCFG : WebSiteCrawller
    {
        public NotifyInfoFSZCFG()
            : base()
        {
            this.Group = "政策法规";
            this.Title = "广东省佛山市公共资源交易网政策法规";
            this.Description = "自动抓取广东省佛山市公共资源交易网政策法规";
            this.PlanTime = "1 22:18";
            this.SiteUrl = "http://www.fsggzy.cn/gcjy/gc_zcfg/";
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
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString().GetRegexBegEnd("HTML", ",").Replace("(", "");
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.fsggzy.cn/gcjy/gc_zcfg/index_" + (i - 1).ToString() + ".html", Encoding.UTF8).GetJsString();
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "secondrightlistbox")), true), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        if (j % 2 == 0)
                            continue;
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        infoType = "政策法规";
                        headName = tr.Columns[0].ToNodePlainString();
                        releaseTime = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        infoUrl = "http://www.fsggzy.cn/gcjy/gc_zcfg/" + tr.Columns[0].GetATagHref().Replace("../","").Replace("./","");
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content2")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            ctxHtml = dtlList.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            msgType = MsgTypeCosnt.GuangZhouMsgType;
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "佛山市区", string.Empty, infoCtx, infoType);
                            sqlCount++;
                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return null;
                            }
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                            {
                                string str = string.Empty;
                                if (infoUrl.IndexOf("/")!=-1)
                                {
                                    str = infoUrl.Remove(infoUrl.LastIndexOf("/")); 
                                }
                                parser = new Parser(new Lexer(ctxHtml));
                                NodeList imgList = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                                if (imgList != null && imgList.Count > 0)
                                {
                                    for (int img = 0; img < imgList.Count; img++)
                                    {
                                        ImageTag imgTag = imgList[img] as ImageTag;
                                        try
                                        {
                                            BaseAttach obj = null;
                                            if (imgTag.GetAttribute("src").Contains("http"))
                                            {
                                                obj = ToolHtml.GetBaseAttach(imgTag.GetAttribute("src"), headName, info.Id);
                                            }
                                            else
                                            {
                                                obj = ToolHtml.GetBaseAttach(str +imgTag.GetAttribute("src").Replace("../", "/").Replace("./", "/"), headName, info.Id);
                                            }
                                            if (obj != null)
                                                ToolDb.SaveEntity(obj, string.Empty);
                                        }
                                        catch { }
                                    }
                                }
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
                                                BaseAttach obj = null;
                                                if (aTag.Link.Contains("http"))
                                                {
                                                    obj = ToolHtml.GetBaseAttach(aTag.Link, aTag.LinkText, info.Id);
                                                }
                                                else
                                                {
                                                    obj = ToolHtml.GetBaseAttach(str + aTag.Link.Replace("../", "/").Replace("./", "/"), aTag.LinkText, info.Id);
                                                }
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
            return null;
        }
    }
}
