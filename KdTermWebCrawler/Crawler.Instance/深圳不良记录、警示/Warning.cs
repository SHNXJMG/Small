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
   public class Warning : WebSiteCrawller
    {
       public Warning()
           : base(true)
       {
           this.Group = "警示信息";
           this.Title = "诚信直接红黄色警示信息";
           this.Description = "自动抓取诚信直接红黄色警示信息";
           this.ExistCompareFields = "WarningName,Begindate";
           this.MaxCount = 20000; 
           this.SiteUrl = "http://61.144.226.2/jsxx/zjjsbrowse.aspx";
           this.PlanTime = "1 2:00";
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
                string WarningName = string.Empty, Color = string.Empty, Begindate = string.Empty, PrjName = string.Empty, DateStage = string.Empty, Score = string.Empty,
                    LastScore = string.Empty, CorpType = string.Empty, Number = string.Empty, UrlInfo = string.Empty, WarnCtx=string.Empty;
                TableTag table = (TableTag)nodeList[0];
                for (int j = 1; j < table.RowCount; j++)
                {
                    TableRow tr = table.Rows[j];
                    WarningName = tr.Columns[2].ToPlainTextString().Trim();
                
                    DateStage = "1";//0代表半年
                    ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                    ImageTag img = tr.Columns[1].SearchFor(typeof(ImageTag), true)[0] as ImageTag;
                    string ppp = img.ImageURL;
                    if (img.ImageURL.Contains("yellow"))
                    {
                        Color = "1";//0代表红色，1代表黄色
                    }
                    else
                    {
                        Color = "0";//0代表红色，1代表黄色
                    }
                    PrjName = tr.Columns[3].ToPlainTextString().Trim();
                    UrlInfo = "http://61.144.226.2/jsxx/zjjsdetail.aspx?ID=" + aTag.Link.Replace("GoView(", "").Replace(");", "").Trim();
                    string htmldetail = string.Empty;
                    try
                    {
                        htmldetail = ToolWeb.GetHtmlByUrl(ToolWeb.UrlEncode(UrlInfo), Encoding.GetEncoding("GB2312")).Replace("= 602;", "罚");
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    Parser dtlparser = new Parser(new Lexer(htmldetail));
                    NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "Table8"), new TagNameFilter("table")));
                    if (dtnode.Count > 0)
                    {
                        WarnCtx = dtnode.AsString().Replace("\t", "").Replace("&nbsp;", "").Replace("\r\n","").Trim();
                        Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                        if (WarnCtx.Contains("警示开始日期"))
                        {
                            Begindate = WarnCtx.Substring(WarnCtx.IndexOf("警示开始日期")).ToString().Replace("警示开始日期：", "").Trim();
                        }
                        Begindate = regDate.Match(Begindate).Value.Trim();
                        CorpWarning info = new CorpWarning();
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
