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
    public class NoticeTianJinZFCG : WebSiteCrawller
    {
        public NoticeTianJinZFCG()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "天津市政府采购网重要通知";
            this.Description = "自动抓取天津市政府采购网重要通知";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 1000;
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.tjgp.gov.cn/index!indexList?newsType=2";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page_bg")), true), new TagNameFilter("select")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    SelectTag select = pageNode[0] as SelectTag;
                    pageInt = int.Parse(select.OptionTags[select.OptionTags.Length - 1].Value);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "toPage","rowCount","keyword"
                    }, new string[]{
                    i.ToString(),"25",""
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "tablelist")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        InfoType = "重要通知";
                        InfoTitle = tr.Columns[1].ToNodePlainString();
                        if (string.IsNullOrEmpty(InfoTitle))
                            return list;
                        InfoTitle = System.Web.HttpUtility.HtmlDecode(InfoTitle);  
                        PublistTime = tr.Columns[2].ToPlainTextString().GetDateRegex(); 
                        InfoUrl = "http://www.tjgp.gov.cn/" + tr.Columns[1].GetATagHref();

                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "main1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            htmlTxt = dtlNode.AsHtml();
                            InfoCtx = System.Web.HttpUtility.HtmlDecode(htmlTxt.ToCtxString());
                            buildUnit = InfoCtx.GetBuildRegex();
                            NoticeInfo info = ToolDb.GenNoticeInfo("天津市", "天津市区", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "天津政府采购办公室", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty, htmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(htmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag aTag = aNode[a] as ATag;
                                    if (aTag.IsAtagAttach())
                                        base.AttachList.Add(ToolDb.GenBaseAttach(System.Web.HttpUtility.HtmlDecode(aTag.LinkText), info.Id, aTag.Link));
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
