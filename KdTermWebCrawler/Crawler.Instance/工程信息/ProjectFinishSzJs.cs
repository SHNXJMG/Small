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
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Threading;

namespace Crawler.Instance
{
    public class ProjectFinishSzJs : WebSiteCrawller
    {
        public ProjectFinishSzJs()
            : base()
        {
            this.Group = "工程信息";
            this.PlanTime = "12:02,03:35";
            this.Title = "深圳市住房和建设局竣工验收信息（2014新版）";
            this.Description = "自动抓取深圳市住房和建设局竣工验收信息（2014新版）";
            this.ExistCompareFields = "PrjEndCode";
            this.SiteUrl = "http://portal.szjs.gov.cn:8888/gongshi/jgbaList.html";
            this.MaxCount = 100000;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ProjectFinish>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1, count = 1;
            string eventValidation = string.Empty;

            try
            {

                htl = ToolHtml.GetHtmlByUrlEncode(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "pageLinkTd")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                try
                {
                    string temp = tdNodes.AsString().ToNodeString();
                    string s = temp.GetRegexBegEnd("总页数", "页").Replace(":", "");
                    pageInt = int.Parse(s);
                }
                catch (Exception ex) { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]
                    {
                        "page",
                        "qymc",
                        "ann_serial",
                        "pro_name"


                    }, new string[] {
                        i.ToString(),
                        "",
                        "",
                        ""

                    });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "tblPrjConstBid")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = (TableTag)listNode[0];
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {

                        string pUrl = string.Empty, pInfoSource = string.Empty, pEndDate = string.Empty,
                                       pConstUnit = string.Empty, pSuperUnit = string.Empty, pDesignUnit = string.Empty,
                                       prjEndDesc = string.Empty, pPrjAddress = string.Empty, pBuildUnit = string.Empty,
                                       pPrjCode = string.Empty, PrjName = string.Empty, pRecordUnit = string.Empty,
                                       pCreatetime = string.Empty, pLicUnit = string.Empty;

                        TableRow tr = table.Rows[j];
                        pPrjCode = tr.Columns[0].ToNodePlainString();
                        PrjName = tr.Columns[1].ToNodePlainString();
                        pBuildUnit = tr.Columns[2].ToNodePlainString();
                        pEndDate = tr.Columns[3].ToNodePlainString().GetDateRegex();


                        if (string.IsNullOrEmpty(pRecordUnit))
                        {
                            pRecordUnit = "深圳市住房和建设局";
                        }
                        ProjectFinish info = ToolDb.GenProjectFinish("广东省", pUrl, "深圳市区", pInfoSource, pEndDate, pConstUnit, pSuperUnit, pDesignUnit, prjEndDesc, pPrjAddress, pBuildUnit, pPrjCode, PrjName, pRecordUnit, pCreatetime, "深圳市住房和建设局", pLicUnit);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount)
                            return list;
                        count++;
                        if (count >= 200)
                        {
                            count = 1;
                            Thread.Sleep(600 * 1000);
                        }
                    }
                }
            }
            return list;
        }
    }
}
