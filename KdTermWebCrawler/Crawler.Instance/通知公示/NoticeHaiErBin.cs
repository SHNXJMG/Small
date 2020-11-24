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
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class NoticeHaiErBin : WebSiteCrawller
    {
        public NoticeHaiErBin()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "哈尔滨市建设工程信息网通知公示";
            this.Description = "自动抓取哈尔滨市建设工程信息网通知公示";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.hrbjjzx.cn/Bid_Front/More.aspx?t=5";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            int pageInt = 8;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "__VIEWSTATEENCRYPTED",
                    "__EVENTVALIDATION",
                    "searchIndex1$tbx_Content",
                    "searchIndex1$ddl_Type"
                    }, new string[]{
                    "GV_Data",
                    "Page$"+i,
                    viewState,
                    "",
                    eventValidation,
                    "--标题关键字--",
                    "4"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GV_Data")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount - 1; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty, area = string.Empty;
                        TableRow tr = table.Rows[j];

                        ATag aTag = tr.Columns[1].GetATag();
                        InfoType = "重要通知";
                        InfoTitle = aTag.LinkText;
                        if (InfoTitle.Contains("..."))
                            InfoTitle = aTag.GetAttribute("title");
                        PublistTime = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        string htmldtl = string.Empty;
                        string postid = aTag.GetAttribute("href").GetRegexBegEnd("'", "'");
                        try
                        {
                            htmldtl = System.Web.HttpUtility.HtmlDecode(GetHtml(html, postid).GetJsString());
                        }
                        catch { }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "Content1_lbl_Content")));
                         if (dtlNode != null && dtlNode.Count > 0)
                         {
                             htmlTxt = dtlNode.AsHtml();
                             parser.Reset();
                             NodeList formNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("form"), new HasAttributeFilter("id", "form1")));
                             if (formNode != null && formNode.Count > 0)
                                 InfoUrl = "http://www.hrbjjzx.cn/" + (formNode[0] as FormTag).GetAttribute("action"); 
                             else
                                 continue;
                             List<string> listImg = new List<string>();
                             parser = new Parser(new Lexer(htmlTxt));
                             NodeList imgNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                             if (imgNode != null && imgNode.Count > 0)
                             {
                                 for (int m = 0; m < imgNode.Count; m++)
                                 {
                                     string link = "http://www.hrbjjzx.cn" + (imgNode[m] as ImageTag).ImageURL;
                                     listImg.Add(link);
                                     htmlTxt = htmlTxt.GetReplace((imgNode[m] as ImageTag).ImageURL, link);
                                 }
                             }
                             InfoCtx = htmlTxt.ToCtxString();
                             buildUnit = InfoCtx.GetBuildRegex();
                             NoticeInfo info = ToolDb.GenNoticeInfo("黑龙江省", "黑龙江省及地市", "哈尔滨市", string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "哈尔滨建设工程交易中心", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, "建设工程", string.Empty, htmlTxt);
                             if (listImg.Count > 0)
                             {
                                 for (int a = 0; a < listImg.Count; a++)
                                 {
                                     BaseAttach attach = ToolDb.GenBaseAttach(InfoTitle, info.Id, listImg[0]);
                                     base.AttachList.Add(attach);
                                 }
                             }
                             list.Add(info);
                             if (!crawlAll && list.Count >= this.MaxCount) return list; 
                         }
                    }
                }
            } 
            return list;
        }

        private string GetHtml(string html, string postId)
        {
            string viewState = this.ToolWebSite.GetAspNetViewState(html);
            string eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
            NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__VIEWSTATE",
                        "__VIEWSTATEENCRYPTED",
                        "__EVENTVALIDATION",
                        "searchIndex1$tbx_Content",
                        "searchIndex1$ddl_Type"
                        }, new string[]{
                        postId,
                        "",
                        viewState,
                        "",
                        eventValidation,
                          "--标题关键字--",
                    "4"
                        });
            return this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
        }
    }
}
