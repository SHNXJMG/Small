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
    public class MeetGz : WebSiteCrawller
    {
        public MeetGz()
            : base()
        {
            this.Group = "会议信息";
            this.Title = "广东省广州市";
            this.Description = "自动抓取广东省广州市会议信息";
            this.ExistCompareFields = "Prov,Area,Road,MeetName,MeetPlace,ProjectName,BeginDate";
            this.MaxCount = 1000;
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,11:30,14:05,14:25,14:50,15:45,16:50,19:00";
            this.SiteUrl = "http://oa.gzzb.gd.cn/gcpbcOA/module/placeSearch/nosso_chooseCd2.jsp?jykssj=";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<MeetInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty; 
            string eventValidation = string.Empty;
            string arguments = "%5B%7B%22jykssj%22%3A%222014-12-10+00%3A00%3A00%22%2C%22jyjssj%22%3A%222014-12-30+23%3A59%3A59%22%2C%22bmkey%22%3A%22%22%2C%22cdzj%22%3A%22CD0036%22%2C%22cdmc%22%3A%22%E7%AC%AC01%E8%AF%84%E6%A0%87%E5%AE%A4%22%2C%22rnrs%22%3A%229%22%2C%22zt%22%3A%2202%22%2C%22cdmj%22%3A%22105%22%2C%22tyy%22%3A%2201%22%2C%22dzbb%22%3A%2201%22%2C%22mkf%22%3A%2201%22%2C%22spcqsb%22%3A%2202%22%2C%22dlkt%22%3A%2202%22%2C%22sfsydzkpb%22%3A%2201%22%2C%22dn%22%3A%2210%22%2C%22ssbm%22%3A%22%E4%B8%AD%E5%BF%83%E6%9C%AC%E9%83%A8%22%2C%22cdlx%22%3A%22%E8%AF%84%E6%A0%87%E5%AE%A4%22%2C%22gm%22%3A%22%E5%A4%A7%22%2C%22dd%22%3A%22%E5%9B%9B%E6%A5%BC%22%2C%22zw%22%3A%22null%22%2C%22zjzy%22%3A%22null%22%2C%22zjdn%22%3A%22null%22%2C%22dldn%22%3A%22null%22%2C%22jhyzy%22%3A%22null%22%2C%22dyj%22%3A%2202%22%2C%22xsq%22%3A%2202%22%2C%22ipdjj%22%3A%2202%22%2C%22znjhp%22%3A%2202%22%2C%22yzj%22%3A%2202%22%2C%22sfxsmx%22%3A%2202%22%7D%5D";
            string method = "findPlaceByCQSCD";

            try
            {
                cookiestr = System.Web.HttpUtility.UrlDecode(arguments);
            }
            catch (Exception ex){ }
            NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
            "service","arguments","method"
            },
                new string[]{
                "PlaceLentManagerBS",
                cookiestr,
                "findPlaceByCQSCD"
                });
            html = this.ToolWebSite.GetHtmlByUrl("http://oa.gzzb.gd.cn/gcpbcOA/json/", nvc, Encoding.UTF8);
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "myTab0_Content0")), true), new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "gridtable"))));
            if (pageNode != null && pageNode.Count > 0)
            {
                TableTag table = pageNode[0] as TableTag;
                foreach (TableRow row in table.Rows)
                {
                    parser = new Parser(new Lexer(row.ToHtml()));
                    NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                    parser.Reset();
                    NodeList hNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("h5"));
                    if (hNode != null && hNode.Count > 0 && tableNode != null && tableNode.Count > 0)
                    {
                        string address = hNode[0].ToNodePlainString();
                        TableTag cTable = tableNode[0] as TableTag;
                        foreach (TableRow cRow in cTable.Rows)
                        {
                            foreach (TableColumn col in cRow.Columns)
                            {
                                parser = new Parser(new Lexer(col.ToHtml()));
                                NodeList divNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "allcd")));
                                if (divNode != null && divNode.Count > 0)
                                {
                                    Div div = divNode[0] as Div;
                                    string url = div.GetAttribute("id");

                                }
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
