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
    public class CorpWarningSzJs : WebSiteCrawller
    {
        public CorpWarningSzJs()
           : base()
       {
           this.Group = "警示信息";
           this.Title = "深圳市住房和建设局直接红黄色警示信息";
           this.Description = "自动抓取深圳市住房和建设局直接红黄色警示信息";
           this.ExistCompareFields = "WarningName,PrjName,Color";
           this.MaxCount = 20000;
           this.SiteUrl = "http://61.144.226.2:8001/web/cxda/hldjsAction.do?method=toZJList";
           this.PlanTime = "1 4:00";
       }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            int count = 1;
            IList list = new List<CorpWarning>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch
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
                {    }
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
                        string code = string.Empty, warningName = string.Empty, deliveryDate = string.Empty, warningType = string.Empty, punishmentType = string.Empty, prjNumber = string.Empty, totalScore = string.Empty, resultScore = string.Empty, corpType = string.Empty, publicEndDate = string.Empty, warningEndDate = string.Empty, prjName = string.Empty, badInfo = string.Empty, msgType = string.Empty,color=string.Empty;
                         
                        TableRow tr = table.Rows[j]; 
                        ImageTag img = tr.Columns[1].SearchFor(typeof(ImageTag), true)[0] as ImageTag; 
                        color = img.GetAttribute("src").ToLower().Contains("red") ? "1" : "0";
                        warningName = tr.Columns[2].ToNodePlainString();
                        prjName = tr.Columns[3].ToNodePlainString();
                        warningType = "直接红黄色警示";
                        msgType = "深圳市住房和建设局";
                        CorpWarning info = ToolDb.GenCorpWarning("广东省", "深圳市区", "", code, warningName, deliveryDate, warningType, punishmentType, prjNumber, totalScore, resultScore, corpType, publicEndDate, warningEndDate, prjName, badInfo, msgType,color);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                        count++;
                        if (count >= 200)
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
