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
    public class InviteSzymcw : WebSiteCrawller
    {
        public InviteSzymcw()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "深圳市裕明财务咨询有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳市裕明财务咨询有限公司招标信息";
            this.SiteUrl = "http://www.ymcw.com/message1.htm";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {

                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("TABLE"), new HasAttributeFilter("style", "margin: 0")));
            if (nodeList != null && nodeList.Count > 0)
            {
                TableTag table = nodeList[0] as TableTag;
                for (int j = 6; j < table.RowCount - 3; j++)
                {
                    string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt=string.Empty;
                    TableRow tr = table.Rows[j];
                    code = tr.Columns[2].ToPlainTextString().Trim();
                    prjName = tr.Columns[3].ToPlainTextString().Trim();
                    //beginDate = tr.Columns[4].ToPlainTextString().Split('-')[0].Replace(".", "-").Trim();

                    ATag aTag = tr.Columns[3].SearchFor(typeof(ATag), true)[0] as ATag;
                    InfoUrl = "http://www.ymcw.com/" + aTag.Link;
                    string htmldetail = string.Empty;
                    try
                    {
                        htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Trim();
                        Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                        NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                        HtmlTxt = dtnodeHTML.AsHtml();
                        htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).ToLower().Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                        Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                        htmldetail = regexHtml.Replace(htmldetail, "");
                    }
                    catch (Exception ex) { continue; }
                    Parser dtlparser = new Parser(new Lexer(htmldetail));
                    NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                    inviteCtx =  System.Web.HttpUtility.HtmlDecode(dtnode[0].ToPlainTextString().Trim());
                    Regex regCtx = new Regex(@"([\r\n]+)|([\t]+)");
                    beginDate = DateTime.Now.ToString("yyyy-MM-dd");
                    inviteCtx = regCtx.Replace(inviteCtx, "\r\n");
                    specType = "其他";
                    msgType = "深圳市裕明财务咨询有限公司";
                    inviteType = ToolHtml.GetInviteTypes(prjName);
                    InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                    list.Add(info);
                    if (!crawlAll && list.Count >= this.MaxCount) 
                        return list;
                }
            }
            return list;
        }

    }
}
