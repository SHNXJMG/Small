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
using System.Threading;

namespace Crawler.Instance
{
    public class NoticeJinCaiW : WebSiteCrawller
    {
        public NoticeJinCaiW()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "中国金融集中采购网变更公示";
            this.Description = "自动抓取中国金融集中采购网变更公示";
            this.PlanTime = "9:40,11:40,14:40,16:40,18:40";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 500;
            this.SiteUrl = "http://www.cfcpn.com/front/notice/bggg_list.jsp";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            string html = string.Empty;
            int pageInt = 1;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pager")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode.AsString().GetRegexBegEnd("共", "页");
                try
                {
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?offset=" + (i * 20 - 20).ToString(), Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table_body")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty, area = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();
                        InfoTitle = aTag.GetAttribute("title");
                        InfoType = "变更公告";
                        PublistTime = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        string prov =  tr.Columns[2].ToNodePlainString();
                        if (prov.Contains("新疆"))
                            prov = "新疆维吾尔自治区";
                        if (prov.Contains("广西"))
                            prov = "广西壮族自治区";
                        if (prov.Contains("宁夏"))
                            prov = "宁夏回族自治区";
                        if (prov.Contains("内蒙"))
                            prov = "内蒙古自治区";
                        InfoUrl = "http://www.cfcpn.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            htmlTxt = dtlNode.AsHtml();
                            InfoCtx = htmlTxt.GetReplace("<br/>,</p>,<br>,<br />,</div>", "\r\n").ToCtxString();
                            buildUnit = InfoCtx.GetBuildRegex(); 
                            prjCode = InfoCtx.GetCodeRegex().GetCodeDel();


                            NoticeInfo info = ToolDb.GenNoticeInfo("全国", "金融专项采购", prov, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "中国金融集中采购网", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, "政府采购", string.Empty, htmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(htmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag file = aNode[a].GetATag();
                                    if (file.IsAtagAttach())
                                    {
                                        string link = file.Link;
                                        if (!link.ToLower().Contains("http"))
                                            link = "http://www.cfcpn.com/" + file.Link;
                                        base.AttachList.Add(ToolDb.GenBaseAttach(file.LinkText, info.Id, link));
                                    }
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
