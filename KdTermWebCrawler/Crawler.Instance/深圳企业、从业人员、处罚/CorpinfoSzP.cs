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

namespace Crawler.Instance
{
    public class CorpinfoSzP : WebSiteCrawller
    {
        public CorpinfoSzP()
            : base(true)
        {
            this.IsCrawlAll = true;
            this.PlanTime = "3-02 0:00,6-02 0:00,9-02 0:00,12-02 0:00";
            this.Group = "处罚信息";
            this.Title = "深圳市建设局处罚信息";
            this.Description = "自动抓取深圳市建设局处罚信息";
            this.ExistCompareFields = "GrantUnit,DocNo";
            this.MaxCount = 1000;
            this.SiteUrl = "http://61.144.226.2/cfxx/browse.aspx";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = ToolWeb.GetHtmlByUrl(ToolWeb.UrlEncode(SiteUrl), Encoding.Default, ref cookiestr);
            }
            catch (Exception ex)
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "list_page")));
            if (nodeList != null && nodeList.Count > 0)
            {
                Regex regexPage = new Regex(@"\d+页");
                page = int.Parse(regexPage.Match(nodeList.AsString()).Value.Trim(new char[] { '共', '页' }));
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    viewState = ToolWeb.GetAspNetViewState(htl);
                    eventValidation = ToolWeb.GetAspNetEventValidation(htl);
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[]{
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__VIEWSTATE",
                        "DOC_ID",
                        "CORP_NAME",
                        "APPYEAR",
                        "ucPageNumControl:gotopage",
                        "ucPageNumControl:NEXTpage"
                    }, new string[]{
                        string .Empty,
                        string.Empty,
                        viewState,
                         string.Empty,
                        string .Empty,
                        "2012",
                        (i-2).ToString(),
                        "下一页"
                    });
                    try
                    {
                        htl = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "dgConstBid")));
                if (tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string DocNo = string.Empty, PunishType = string.Empty, GrantUnit = string.Empty, DocDate = string.Empty, PunishCtx = string.Empty, GrantName = string.Empty, InfoUrl = string.Empty;
                        TableRow tr = table.Rows[j];
                        DocNo = tr.Columns[1].ToPlainTextString().Trim();
                        PunishType = tr.Columns[5].ToPlainTextString().Trim();
                        GrantUnit = tr.Columns[2].ToPlainTextString().Replace("&nbsp;", "").Trim();
                        DocDate = tr.Columns[3].ToPlainTextString().Trim();
                        if (GrantUnit.Length <= 5)
                        {
                            GrantName = GrantUnit;
                            GrantUnit = "";
                        }
                        else
                        {
                            GrantName = "";
                        }
                        ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://61.144.226.2/PUNHTML/" + aTag.Link.Replace("GoDetail('", "").Replace("');", "").Trim();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = ToolWeb.GetHtmlByUrl(ToolWeb.UrlEncode(InfoUrl), Encoding.GetEncoding("GB2312")).Replace("= 602;", "罚");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new HasParentFilter(new TagNameFilter("div")));
                        PunishCtx = dtnode.AsString().Replace("=\r\n", "").Replace("&nbsp;", "").Trim();
                        PunishCtx = System.Web.HttpUtility.HtmlDecode(PunishCtx).Replace("</p>", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Trim();
                        if (GrantUnit == "")
                        {
                            Regex regGrantUnit = new Regex(@"(工程位置|被处罚单位)(:|：)[^\r\n]+\r\n");
                            GrantUnit = regGrantUnit.Match(PunishCtx).Value.Replace("被处罚单位", "").Replace(":", "").Replace("：", "").Trim();
                        }
                        if (GrantName == "")
                        {
                            Regex regGrantName = new Regex(@"(工程位置|企业负责人)(:|：)[^\r\n]+\r\n");
                            GrantName = regGrantName.Match(PunishCtx).Value.Replace("企业负责人", "").Replace(":", "").Replace("：", "").Trim();
                        }
                        CorpPunish info = ToolDb.GenCorpPunish(string.Empty, DocNo, PunishType, GrantUnit, DocDate, PunishCtx, InfoUrl, GrantName, "1");
                        list.Add(info);
                    }
                }
            }
            return list;
        }
    }
}
