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
    public class NoticeGZYJ : WebSiteCrawller
    {
        public NoticeGZYJ()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省广州市建设工程交易中心业绩公示";
            this.PlanTime = "9:24,11:24,14:24,17:24";
            this.Description = "自动抓取广东省广州市建设工程交易中心业绩公示";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.gzzb.gd.cn/cms/wz/view/tzygg/enterpriseAchievementServlet?xmmc=&xmbh=&channelId=19";
            this.MaxCount = 100;
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
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tzgg_right_page")), true), new TagNameFilter("a")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    ATag aTag = pageList[pageList.Count - 2] as ATag;
                    string tem = aTag.LinkText;
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
                        string url = "http://www.gzzb.gd.cn/cms/wz/view/tzygg/enterpriseAchievementServlet?name=&number=&projectName=&projectNumber=&siteId=1&channelId=19&pager.offset=" + i.ToString() + "0";
                        html = this.ToolWebSite.GetHtmlByUrl(url, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "table1")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty,htmlTxt=string.Empty;

                        TableRow tr = table.Rows[j];
                        prjCode = tr.Columns[1].ToNodePlainString();
                        InfoTitle = tr.Columns[2].ToNodePlainString();
                        buildUnit = tr.Columns[4].ToNodePlainString();
                        PublistTime = tr.Columns[5].ToPlainTextString();
                        InfoType = "业绩公示";
                        InfoUrl = "http://www.gzzb.gd.cn" + tr.Columns[2].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = ToolHtml.GetHtmlByUrl(InfoUrl, Encoding.Default);
                            htldtl = htldtl.GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "block-body")));
                        //if (dtlList != null && dtlList.Count > 0)
                        //{
                        //    InfoCtx = dtlList.AsString().ToCtxString().Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                        //}
                        InfoCtx = "项目编号：" + prjCode + "\r\n项目名称：" + InfoTitle + "\r\n单位编号：" + tr.Columns[3].ToNodePlainString() + "\r\n单位名称：" + buildUnit + "\r\n审核时间：" + PublistTime;
                        htmlTxt = InfoCtx;
                        NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "广州市区", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.GuangZhouMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty,htmlTxt);
                        list.Add(info);
                        //parser = new Parser(new Lexer(dtlList.AsHtml()));
                        //NodeList aList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        //if (aList != null && aList.Count > 0)
                        //{
                        //    for (int c = 0; c < aList.Count; c++)
                        //    {
                        //        ATag aTag = aList[c].GetATag();
                        //        if (aTag.IsAtagAttach())
                        //        {
                        //            string alink = "http://www.gzzb.gd.cn" + aTag.Link;
                        //            BaseAttach attach = ToolDb.GenBaseAttach(aTag.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, alink);
                        //            base.AttachList.Add(attach);
                        //        }
                        //    }
                        //}
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
