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
    public class InviteSzuho : WebSiteCrawller
    {
        public InviteSzuho()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "深圳市友和保险经纪有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳市友和保险经纪有限公司招标信息";
            this.SiteUrl = "http://www.uho.cn/main.aspx?flg=3&id=6";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8);
            }
            catch (Exception ex)
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "AspNetPager1")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                string htlPage = tdNodes.ToHtml();
                parser = new Parser(new Lexer(htlPage));
                NodeFilter filer = new TagNameFilter("a");
                NodeList pageList = parser.ExtractAllNodesThatMatch(filer);
                if (pageList != null && pageList.Count > 0)
                {
                    for (int i = pageList.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            ATag aTag = pageList.SearchFor(typeof(ATag), true)[i] as ATag;
                            string pageTemp = aTag.Link.Replace("main.aspx?flg=3&id=6&page=", "");
                            pageInt = int.Parse(pageTemp);
                            break;
                        }
                        catch (Exception ex) { }
                    }
                }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&page=" + i.ToString()), Encoding.UTF8);
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "760")));

                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {

                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j]; 

                        beginDate = tr.Columns[2].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                        prjName = aTag.LinkText;
                        InfoUrl = "http://www.uho.cn/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("bordercolor", "#FFFFFF"), new TagNameFilter("table")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).ToLower().Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n").Replace("<br/>", "\r\n");
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            htmldetail = regexHtml.Replace(htmldetail, "");
                        }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("bordercolor", "#ffffff"), new TagNameFilter("table")));
                        inviteCtx = dtnode.AsString();
                        specType = "其他";
                        msgType = "深圳市友和保险经纪有限公司";
                        inviteType = ToolHtml.GetInviteTypes(prjName);
                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                        list.Add(info);

                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }


            }
            return list;
        }

    }
}
