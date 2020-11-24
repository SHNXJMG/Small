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

namespace Crawler.Instance
{
    public class SZBehavior : WebSiteCrawller
    {
        public SZBehavior()
            : base(true)
        {
            this.Group = "不良记录信息";
            this.Title = "诚信不良记录信息";
            this.Description = "自动抓取诚信不良记录信息";
            this.ExistCompareFields = "CorpName,Othery1";
            this.MaxCount = 20000;
            this.PlanTime = "1 4:00"; 
            this.SiteUrl = "http://61.144.226.2/CXDA_BLXW/browse.aspx";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            int sqlCount = 0;
            try
            {
                htl = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "list_table")));
            if (nodeList.Count > 0)
            {
                string CorpName = string.Empty, CorpType = string.Empty, Behavior = string.Empty, BehaviorCtx = string.Empty, BeginDate = string.Empty,
                    Othery1 = string.Empty, othery2 = string.Empty, othery3 = string.Empty,infoUrl=string.Empty;
                 TableTag table = (TableTag)nodeList[0];
                 for (int j = 1; j < table.RowCount; j++)
                 {
                     TableRow tr = table.Rows[j];
                     CorpName = tr.Columns[1].ToPlainTextString().Trim();
                     CorpType = tr.Columns[2].ToPlainTextString().Trim();
                     Behavior = tr.Columns[3].ToPlainTextString().Trim();
                     BeginDate = tr.Columns[4].ToPlainTextString().Trim();
                     ATag aTag = tr.Columns[3].SearchFor(typeof(ATag), true)[0] as ATag;
                     infoUrl = "http://61.144.226.2/CXDA_BLXW/Detail.aspx?Doc_ID=" + aTag.Link.Replace("GoAttachView('", "").Replace("');", "").Trim();
                     string htmldetail = string.Empty;
                     try
                     {
                         htmldetail = ToolWeb.GetHtmlByUrl(ToolWeb.UrlEncode(infoUrl), Encoding.GetEncoding("GB2312")).Replace("= 602;", "罚");
                     }
                     catch (Exception)
                     {
                         continue;
                     }
                     Parser dtlparser = new Parser(new Lexer(htmldetail));
                     NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "Table1"), new TagNameFilter("table")));
                     if (dtnode.Count > 0)
                     {
                         BehaviorCtx = dtnode.AsString().Replace("\t", "").Replace("&nbsp;","").Replace("\r\n","").Trim();
                         CorpBehavior info = ToolDb.GenCorpBehavior(CorpName, CorpType, Behavior, BehaviorCtx, infoUrl, string.Empty, string.Empty, BeginDate);
                         if (sqlCount <= this.MaxCount)
                         {
                             ToolDb.SaveEntity(info, this.ExistCompareFields);
                             sqlCount++;
                         }
                         else
                             return list;
                     }
                 }
            }
            return list;
        }
    }
}
