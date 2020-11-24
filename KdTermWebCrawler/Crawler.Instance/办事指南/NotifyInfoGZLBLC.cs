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
    public class NotifyInfoGZLBLC : WebSiteCrawller
    {
        public NotifyInfoGZLBLC()
            : base()
        {
            this.Group = "办事指南";
            this.Title = "广东省广州市建设工程交易中心（招投标类别流程图）办事指南";
            this.Description = "自动抓取广东省广州市建设工程交易中心（招投标类别流程图）办事指南";
            this.PlanTime = "22:16";
            this.SiteUrl = "http://www.gzzb.gd.cn/cms/wz/view/fwzn/bszy_index.html?siteId=1&channelId=36";
            this.MaxCount = 500;
            this.ExistCompareFields = "InfoUrl";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        { 
            int sqlCount = 0;
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
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "font9grey1")));
            if (nodeList != null && nodeList.Count > 1)
            {
                for (int i = 0; i < 2; i++)
                {
                    TableTag table = nodeList[i] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        infoType = "办事指南";
                        headName = tr.Columns[1].ToNodePlainString();
                        releaseTime = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        infoUrl = "http://www.gzzb.gd.cn" + tr.Columns[1].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "contentDiv")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            ctxHtml = dtlList.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            msgType = MsgTypeCosnt.GuangZhouMsgType;
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "广州市区", string.Empty, infoCtx, infoType);
                            sqlCount++;
                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return null;
                            }
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                            {
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
                                                obj = ToolHtml.GetBaseAttach("http://www.gzzb.gd.cn" + imgTag.GetAttribute("src"), headName, info.Id);
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
                                                    obj = ToolHtml.GetBaseAttach("http://www.gzzb.gd.cn" + aTag.Link, aTag.LinkText, info.Id);
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
