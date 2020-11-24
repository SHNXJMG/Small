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
    public class NoticeSZJYZXGG : WebSiteCrawller
    {
        public NoticeSZJYZXGG()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市交易中心通知公示公告";
            this.PlanTime = "9:29,11:29,14:19,17:29";
            this.Description = "自动抓取广东省深圳市交易中心通知公示公告";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.szjsjy.com.cn/Notify/AfficheBrows.aspx";
            this.MaxCount = 500;
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
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("valign", "top")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    TableRow tr = pageList[0] as TableRow;
                    string temp = tr.Columns[tr.ColumnCount - 1].ToNodePlainString();
                    temp = temp.Substring(temp.Length - 1, 1);
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
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                            new string[]{
                            "__EVENTTARGET",
                            "__EVENTARGUMENT",
                            "__VIEWSTATE",
                            "__VIEWSTATEENCRYPTED",
                            "__EVENTVALIDATION",
                            "sel",
                            "beginDate",
                            "endDate",
                            "infotitle"
                            },
                            new string[]{
                            "GridView1","Page$"+i.ToString(),
                            viewState,"",eventValidation,"1","","",""
                            }
                            );
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl,nvc,Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty,htmlTxt=string.Empty;

                        TableRow tr = table.Rows[j];
                        InfoType = "公告公示"; 
                        InfoTitle = tr.Columns[1].ToNodePlainString();
                        PublistTime = tr.Columns[3].ToPlainTextString();
                        InfoUrl = "http://www.szjsjy.com.cn/Notify/" + tr.Columns[1].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                            htldtl = htldtl.GetJsString();
                        }
                        catch { continue; }

                        NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳市工程", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.ShenZhenMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty, htmlTxt);
                         
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"),new HasAttributeFilter("width","750")));

                        if (dtlList != null && dtlList.Count > 0)
                        {
                            InfoCtx = dtlList.ToHtml().Replace("</tr>", "\r\n").ToCtxString().Replace("\r\n\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\t", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            parser = new Parser(new Lexer(dtlList.ToHtml()));
                            NodeList aList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aList != null && aList.Count > 0)
                            {
                                for (int k = 0; k < aList.Count; k++)
                                {
                                    ATag aTag = aList[k] as ATag;
                                    if (aTag.IsAtagAttach())
                                    {
                                        string alink = "http://www.szjsjy.com.cn/" + aTag.Link.Replace("../", "");
                                        BaseAttach attach = ToolDb.GenBaseAttach(aTag.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, alink);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            info.CtxHtml = dtlList.AsHtml();
                            info.InfoCtx = InfoCtx;
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
