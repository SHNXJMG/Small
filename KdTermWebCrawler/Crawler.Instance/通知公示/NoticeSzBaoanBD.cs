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
    public class NoticeSzBaoanBD : WebSiteCrawller
    {
        public NoticeSzBaoanBD()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市宝安区标底公示";
            this.Description = "自动抓取广东省深圳宝安区标底公示";
            this.SiteUrl = "http://www.bajsjy.com/Bdgs";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>(); 
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "input-group-addon")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                try
                {
                    string reTemp = tdNodes.AsString().GetRegexBegEnd("共", "项");
                    string pageTemp = tdNodes.AsString().GetRegexBegEnd("项", "页").GetReplace("共,项,页," + reTemp + ",，");
                    pageInt = int.Parse(pageTemp);
                }
                catch (Exception) { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "?pi=" + (i-1), Encoding.UTF8);
                    }
                    catch { continue; }
                }
                  parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "inside_table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, htmlTxt = string.Empty, prjCode = string.Empty,buildUnit=string.Empty,prjType=string.Empty;
                        TableRow tr = table.Rows[j];
                        prjCode = tr.Columns[1].ToNodePlainString().Replace(" ", "");
                        InfoTitle = tr.Columns[2].ToPlainTextString().Trim();
                        buildUnit = tr.Columns[3].ToNodePlainString();
                        prjType = tr.Columns[4].ToNodePlainString();
                        InfoType = "标底公示";
                        PublistTime = tr.Columns[5].ToPlainTextString().Trim();
                        InfoUrl = "http://www.bajsjy.com/" + tr.Columns[2].GetATagHref();
                        string ctxhtml = string.Empty;
                        try
                        {
                            ctxhtml = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                        }
                        catch  
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(ctxhtml));
                        NodeList dtnode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "showContent"), new TagNameFilter("div")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            htmlTxt = dtnode.AsHtml();
                            InfoCtx = htmlTxt.ToLower().Replace("</p>", "\r\n").Replace("<br>", "\r\n").Replace("<br/>", "\r\n").ToCtxString();
                            NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳宝安区工程", string.Empty,string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "深圳市建设工程交易中心宝安分中心", InfoUrl, prjCode, buildUnit,string.Empty, string.Empty, prjType, string.Empty, htmlTxt); 
                            list.Add(info);
                            parser = new Parser(new Lexer(htmlTxt));
                            NodeList fileNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "inside_table")));
                            if (fileNode != null && fileNode.Count > 0 && fileNode[0] is TableTag)
                            {
                                TableTag fileTable = fileNode[0] as TableTag;
                                for (int f = 1; f < fileTable.Rows.Length; f++)
                                {
                                    BaseAttach attach = ToolDb.GenBaseAttach(fileTable.Rows[f].Columns[1].ToPlainTextString().Trim(), info.Id, "http://www.bajsjy.com/" + (fileTable.Rows[f].Columns[1].SearchFor(typeof(ATag), true)[0] as ATag).Link.Replace("../", "/"));
                                    base.AttachList.Add(attach);
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
