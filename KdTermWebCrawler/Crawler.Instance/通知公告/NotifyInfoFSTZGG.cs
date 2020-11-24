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
    public class NotifyInfoFSTZGG : WebSiteCrawller
    {
        public NotifyInfoFSTZGG()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省佛山市建设公共资源交易网通知公告";
            this.Description = "自动抓取广东省佛山市建设公共资源交易网交易指南";
            this.PlanTime = "21:42";
            this.SiteUrl = "http://www.fsggzy.cn/tzgg/";
            this.MaxCount = 40;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        { 
            //取得页码
            int pageInt = 1,sqlCount=0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8);
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
                    string temp = pageList.AsString().GetRegexBegEnd("HTML", ",");
                    pageInt = int.Parse(temp.Replace("(", ""));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.fsggzy.cn/tzgg/index_" + (i - 1).ToString() + ".html", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html)); 
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "secondrightlistbox")), true), new TagNameFilter("ul")), true), new TagNameFilter("li")));

                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty,
                              infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        headName = nodeList[j].ToNodePlainString().Replace("[", "").Replace("]", "");
                        releaseTime = nodeList[j].ToPlainTextString().GetDateRegex();
                        headName = headName.Replace(releaseTime, "");
                        infoType = "通知公告";
                        infoUrl = "http://www.fsggzy.cn/tzgg/" + nodeList[j].GetATagHref().Replace("../", "").Replace("./", "");
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
                            infoCtx = dtlList.AsString().ToCtxString();
                            List<string> imgUrlLen = new List<string>();
                            parser = new Parser(new Lexer(ctxHtml));
                            NodeList imgList = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                            if (imgList != null && imgList.Count > 0)
                            {
                                for (int d = 0; d < imgList.Count; d++)
                                {
                                    ImageTag img = imgList[d] as ImageTag;
                                    string[] strLen = infoUrl.Split('/');
                                    string value = string.Empty;
                                    for (int k = 0; k < strLen.Length - 1; k++)
                                    {
                                        value += strLen[k] + "/";
                                    }
                                    string imgUrl = value + img.GetAttribute("src").Replace("../", "").Replace("./", "");
                                    imgUrlLen.Add(imgUrl);
                                }
                            }
                            msgType = MsgTypeCosnt.FouShanMsgType;
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "佛山市区", string.Empty, infoCtx, infoType);
                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return null;
                            }
                            else
                            {
                                sqlCount++;
                                if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                                {
                                    if (imgUrlLen != null && imgUrlLen.Count > 0)
                                    {
                                        for (int c = 0; c < imgUrlLen.Count; c++)
                                        {
                                            BaseAttach obj = ToolHtml.GetBaseAttach(imgUrlLen[c], headName, info.Id);
                                            if (obj != null)
                                            {
                                                ToolDb.SaveEntity(obj, string.Empty);
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
