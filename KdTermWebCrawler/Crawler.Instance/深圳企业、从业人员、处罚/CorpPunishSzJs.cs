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
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.IO;

namespace Crawler.Instance
{
    public class CorpPunishSzJs : WebSiteCrawller
    {
        public CorpPunishSzJs()
            : base()
        {
            this.PlanTime = "1-28 2:00,4-28 2:00,7-28 2:00,10-28 2:00";
            this.Group = "处罚信息";
            this.Title = "深圳市建设局处罚信息(2014新版)";
            this.Description = "自动抓取深圳市建设局处罚信息(2014新版)";
            this.ExistCompareFields = "GrantUnit,DocNo,IsShow";
            this.MaxCount = 1000;
            this.SiteUrl = "http://61.144.226.2:8001/web/cxda/xzcfAction.do?method=toList";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            int count = 1;
            IList list = new List<CorpPunish>();
            string htl = string.Empty; 
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("a"), new HasAttributeFilter("id", "lx")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.GetATagHref().GetRegexBegEnd("page=", "&");
                    pageInt = int.Parse(temp);
                }
                catch
                {
                }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = ToolWeb.GetHtmlByUrl(this.SiteUrl + "&page=" + i.ToString(), Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "bean")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string DocNo = string.Empty, PunishType = string.Empty, GrantUnit = string.Empty, DocDate = string.Empty, PunishCtx = string.Empty, GrantName = string.Empty, InfoUrl = string.Empty;
                        TableRow tr = table.Rows[j];
                        DocNo = tr.Columns[1].ToNodePlainString();
                        GrantName = tr.Columns[2].ToNodePlainString();
                        DocDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        PunishType = tr.Columns[5].ToNodePlainString();
                        InfoUrl = tr.Columns[1].GetATagHref();

                        CorpPunish info = ToolDb.GenCorpPunish(string.Empty, DocNo, PunishType, GrantUnit, DocDate, PunishCtx, InfoUrl, GrantName, "1");

                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount)
                            return list;

                        count++;
                        if (count >= 50)
                        {
                            count = 1;
                            Thread.Sleep(480000);
                        }
                    }
                }
            }
            return list;
        }
    }
}
