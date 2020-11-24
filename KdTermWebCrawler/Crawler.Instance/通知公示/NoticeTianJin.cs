using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class NoticeTianJin : WebSiteCrawller
    {
        public NoticeTianJin()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "天津建设工程信息网";
            this.Description = "自动抓取天津建设工程信息网";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 1000;
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.tjconstruct.cn/xwbd.aspx?type=ggtz";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8).GetJsString();
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_ctl00_AspNetPager1")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode.AsString();
                try
                {
                    string page = temp.GetRegexBegEnd(",共", "页");
                    pageInt = int.Parse(page);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "745")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount - 2; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];

                        InfoTitle = tr.Columns[0].ToNodePlainString();
                        try
                        {
                            PublistTime = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            Logger.Error(InfoTitle);
                            Logger.Error("===>>" + list.Count);
                            Logger.Error("===>>" + i);
                        }
                        InfoType = "公告通知";
                        InfoUrl = "http://www.tjconstruct.cn/" + tr.Columns[0].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }

                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "WordSection1")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            htmlTxt = dtlList.ToHtml();
                            InfoCtx = htmlTxt.Replace("</p>", "\r\n").ToCtxString();
                            buildUnit = InfoCtx.GetBuildRegex();
                            prjCode = InfoCtx.GetCodeRegex();
                            NoticeInfo info = ToolDb.GenNoticeInfo("天津市", "天津市区", "", string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "天津市工程建设交易服务中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty, htmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                        else
                        {
                            NoticeInfo info = ToolDb.GenNoticeInfo("天津市", "天津市区", "", string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "天津市工程建设交易服务中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty, htmlTxt);
                            list.Add(info);
                            parser.Reset();
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag aTag = aNode[a] as ATag;
                                    if (aTag.IsAtagAttach())
                                    {
                                        if (Encoding.Default.GetByteCount(aTag.LinkText) < 400)
                                            base.AttachList.Add(ToolDb.GenBaseAttach(aTag.LinkText, info.Id, aTag.Link));
                                    }
                                }
                            }
                            base.AttachList.Add(ToolDb.GenBaseAttach(InfoTitle, info.Id, InfoUrl));
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }

    }
}
