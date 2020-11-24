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
using System.Web;
namespace Crawler.Instance
{
    public class NotifyInfoSZZJJZCFG : WebSiteCrawller
    {
        public NotifyInfoSZZJJZCFG()
            : base()
        {
            this.Group = "政策法规";
            this.Title = "广东省深圳市住房和建设局政策法规";
            this.Description = "自动抓取广东省深圳市住房和建设局政策法规";
            this.PlanTime = "1 21:52";
            this.SiteUrl = "http://www.szjs.gov.cn/ztfw/gcjs/zcwj/";
            this.MaxCount = 2000;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            //取得页码 
            int pageInt = 1, sqlCount = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiostr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return null;
            }

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                }

                Parser parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list")),true),new TagNameFilter("div")));
                if (nodeList != null && nodeList.Count > 0)
                { 
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty,
                              infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        headName = nodeList[j].GetATagValue("title");
                        releaseTime = nodeList[j].ToPlainTextString().GetDateRegex();
                        infoType = "政策法规";
                        infoUrl = "http://www.szjs.gov.cn/ztfw/gcjs/zcwj/"+nodeList[j].GetATagHref().Replace("./","");

                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList noList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content")));
                        if (noList != null && noList.Count > 0)
                        {
                            ctxHtml = noList.AsHtml().Replace("<br/>", "\r\n").Replace("<BR/>", "");
                            infoCtx = ctxHtml.ToCtxString().Replace(" ", "").Replace("&nbsp;", "").Replace("\t\t", "\t").Replace("\t\t", "\t");
                            infoCtx = Regex.Replace(infoCtx, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase).Replace(" ", "").Replace("\t", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            msgType = MsgTypeCosnt.ShenZhenZJJMsgType;
                            infoScorce = infoScorce.Replace("&nbsp;", "");
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳市工程", string.Empty, infoCtx, infoType);
                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return null;
                            }
                            else
                            {
                                sqlCount++;
                                if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                                {
                                    parser = new Parser(new Lexer(ctxHtml));
                                    NodeList imgList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                    if (imgList != null && imgList.Count > 0)
                                    {
                                        for (int img = 0; img < imgList.Count; img++)
                                        {
                                            ATag imgTag = imgList[img] as ATag;
                                            if (imgTag.IsAtagAttach() || imgTag.IsImage())
                                            {
                                                try
                                                {
                                                    BaseAttach obj = null;
                                                    if (imgTag.Link.Contains("http"))
                                                    {
                                                        obj = ToolHtml.GetBaseAttach(imgTag.Link, imgTag.LinkText, info.Id);
                                                    }
                                                    else
                                                    {
                                                        obj = ToolHtml.GetBaseAttach("http://www.szjs.gov.cn/" + "" + "/" + imgTag.Link, imgTag.LinkText, info.Id);
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
            }
            return null;
        }

    }
}
