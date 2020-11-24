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
    public class NoticeGZJS : WebSiteCrawller
    {
        public NoticeGZJS()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省广州市建设工程交易中心资审结果";
            this.PlanTime = "9:22,11:22,14:12,17:22";
            this.Description = "自动抓取广东省广州市建设工程交易中心资审结果";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.gzzb.gd.cn/cms/view/jyList?channelId=16&siteId=1";
            this.MaxCount = 20;
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tzgg_right_page")), true), new TagNameFilter("span")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    Span temp = pageList[pageList.Count - 1] as Span;
                    string tem = temp.GetAttribute("onclick");
                    pageInt = Convert.ToInt32(tem.Replace("goPage(", "").Replace(")", ""));
                }
                catch { pageInt = 1; }
            } 
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    try
                    {
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "page","xmlb","xmjdbmid","method","SearchBar","PageSize"
                        },new string[]{
                        i.ToString(),"","","","Y","15"
                        });
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default);
                    }
                    catch 
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "table1")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty,htmlTxt=string.Empty;

                        TableRow tr = table.Rows[j];
                        InfoTitle = tr.Columns[1].ToNodePlainString(); 
                        PublistTime = tr.Columns[2].ToPlainTextString();
                        InfoType = "资审公示";
                        InfoUrl = "http://www.gzzb.gd.cn"+ tr.Columns[1].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                            htldtl = htldtl.GetJsString();
                        }
                        catch
                        {
                            continue;
                        } 
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("valign", "top")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            htmlTxt = dtlList.ToHtml();
                            InfoCtx = dtlList.AsString().ToCtxString().Replace("\r\n\r\n","\r\n").Replace("\r\n\r\n","\r\n").Replace("\r\n\r\n","\r\n").Replace("\r\n\r\n","\r\n");
                        }
                        NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "广州市区", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.GuangZhouMsgType, InfoUrl, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, htmlTxt);
                        list.Add(info);
                        parser = new Parser(new Lexer(dtlList.AsHtml()));
                        NodeList aList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        if (aList != null && aList.Count > 0)
                        {
                            for (int c = 0; c < aList.Count; c++)
                            {
                                ATag aTag = aList[c].GetATag();
                                if (aTag.IsAtagAttach())
                                {
                                    string alink = "http://www.gzzb.gd.cn" + aTag.Link;
                                    BaseAttach attach = ToolDb.GenBaseAttach(aTag.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, alink);
                                    base.AttachList.Add(attach);
                                }
                            }
                        }
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
