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
    public class NotifyInfoHZSJZCFG:WebSiteCrawller
    {
        public NotifyInfoHZSJZCFG()
            : base()
        {
            this.Group = "政策法规";
            this.Title = "广东省惠州市建设工程交易中心（省级法规）政策法规";
            this.PlanTime = "1 22:32";
            this.Description = "自动抓取广东省惠州市建设工程交易中心（省级法规）政策法规";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.ebc.huizhou.gov.cn/index/showList/000000000007/000000000406";
            this.MaxCount = 500;
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
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "scott")), true), new TagNameFilter("a")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList[pageList.Count - 1].GetATagValue().Replace("(", "kdxx").Replace(")", "xxdk").GetRegexBegEnd("kdxx", "xxdk");
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
                        string typeId = ToolHtml.GetHtmlInputValue(html, "typeId");
                        string boardId = ToolHtml.GetHtmlInputValue(html, "boardId");
                        string totalRows = ToolHtml.GetHtmlInputValue(html, "totalRows");
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "typeId","boardId","totalRows","pageNO"
                        }, new string[]{
                        typeId,boardId,totalRows,i.ToString()
                        });
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "lefttable")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        infoType = "政策法规";

                        headName = tr.Columns[1].ToNodePlainString();
                        releaseTime = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        infoUrl = tr.Columns[1].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.Default).GetJsString().Replace("<?xml:namespace prefix = o ns = \"urn:schemas-microsoft-com:office:office\" />", "");
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "context_div")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            ctxHtml = dtlList.AsHtml();
                            infoCtx = ctxHtml.ToCtxString();
                            msgType = MsgTypeCosnt.HuiZhouMsgType;
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "惠州市区", string.Empty, infoCtx, infoType);
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
                                    NodeList imgList = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                                    if (imgList != null && imgList.Count > 0)
                                    {
                                        for (int img = 0; img < imgList.Count; img++)
                                        {
                                            ImageTag imgTag = imgList[img] as ImageTag;
                                            try
                                            {
                                                BaseAttach baseInfo = ToolHtml.GetBaseAttachByUrl(imgTag.GetAttribute("src"), headName, info.Id);
                                                if (baseInfo != null)
                                                    ToolDb.SaveEntity(baseInfo, string.Empty);
                                            }
                                            catch { }
                                        }
                                    }
                                    parser = new Parser(new Lexer(ctxHtml));
                                    NodeList attachList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                    if (attachList != null && attachList.Count > 0)
                                    {
                                        for (int a = 0; a < attachList.Count; a++)
                                        {
                                            ATag aTag = attachList[a] as ATag;
                                            if (aTag.IsAtagAttach())
                                            {
                                                try
                                                {
                                                    BaseAttach obj = ToolHtml.GetBaseAttachByUrl(aTag.Link, aTag.LinkText, info.Id);
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
