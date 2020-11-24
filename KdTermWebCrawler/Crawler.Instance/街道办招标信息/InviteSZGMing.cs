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
    public class InviteSZGMing : WebSiteCrawller
    {
        public InviteSZGMing()
            : base(true)
        {
            this.Group = "招标信息";
            this.Title = "广东省深圳市光明区";
            this.Description = "自动抓取广东省深圳市光明区招标信息";
            this.PlanTime = "9:05,10:05,11:35,14:05,16:05,17:35,20:05";
            this.Disabled = false;
            this.SiteUrl = "http://app.szgm.gov.cn/gmproject/ProjectList.aspx?nodeid=4";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string htl = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8).Replace("&nbsp;", "");
            }
            catch (Exception ex)
            { 
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("colSpan", "3")), true), new TagNameFilter("table")));
            TableTag tableTwo = (TableTag)nodeList[7];
            pageInt = tableTwo.Rows[0].ColumnCount;
            for (int i = 1; i < pageInt; i++)
            {
                if (i > 1)
                {
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "gvlist")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                 prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                 specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                 remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                 CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[1].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[0].SearchFor(typeof(ATag), true)[1] as ATag;
                        code = tr.Columns[0].ToPlainTextString().Trim();
                        beginDate = tr.Columns[2].ToPlainTextString().Trim();
                    }

                }
            }
            return null;
        }
    }
}
