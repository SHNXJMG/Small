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
    public class InviteSzCmcGCZB : WebSiteCrawller
    { 
        public InviteSzCmcGCZB()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "中国机械进出口（集团）有限公司（工程招标）";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取中国机械进出口（集团）有限公司招标信息（工程招标）";
            this.SiteUrl = "http://zb.cmc.com.cn/TenderNotice/List.aspx?id=1";
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
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("style", "float:left;")));
            if (tdNodes != null&&tdNodes.Count>0)
            { 
                try
                {
                    string pageTemp = tdNodes[0].ToPlainTextString().Replace("&nbsp;", "").Trim();
                    Regex regpage = new Regex(@"共\d+页");
                    //string ss = regpage.Match(pageTemp).Value.Replace("页", "").Replace("共", "").Trim();
                    pageInt = int.Parse(regpage.Match(pageTemp).Value.Replace("共", "").Replace("页", "").Trim());
                }
                catch (Exception ex) { }

                string cookiestr = string.Empty;
                for (int i = 1; i <= pageInt; i++)
                {

                    if (i > 1)
                    {
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&page=" + i.ToString()), Encoding.UTF8);
                        }
                        catch (Exception ex)
                        {

                            continue;
                        }
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "tb_list")));
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        TableTag table = nodeList[0] as TableTag;
                        for (int j = 1; j < table.RowCount; j++)
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt=string.Empty;
                            TableRow tr = table.Rows[j];
                            beginDate = tr.Columns[4].ToPlainTextString().Trim();
                            endDate = tr.Columns[5].ToPlainTextString().Trim();
                            prjName = tr.Columns[1].ToPlainTextString().Trim().Replace(" ", "");
                            ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                            InfoUrl = "http://zb.cmc.com.cn/TenderNotice/" + aTag.Link;
                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = ToolHtml.GetHtmlByUrl(SiteUrl,InfoUrl).Replace("&nbsp;", "").Trim();
                                Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                                NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "ct"), new TagNameFilter("div")));
                                HtmlTxt = dtnodeHTML.AsHtml();
                                htmldetail = ToolHtml.GetHtmlByUrl(SiteUrl, InfoUrl).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                                Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                                htmldetail = regexHtml.Replace(htmldetail, "");
                            }
                            catch (Exception ex) { continue; }
                            Parser dtlparser = new Parser(new Lexer(htmldetail));
                            NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "ct"), new TagNameFilter("div")));
                           
                            inviteCtx = dtnode.AsString().Trim().Replace(" ", "");
                            Regex regCtx = new Regex(@"([\r\n]+)|([\t]+)|(\[该信息共被浏览了[0-9]+次\]\[关闭\])");
                            inviteCtx = regCtx.Replace(inviteCtx, "\r\n"); 

                            Regex regcode = new Regex(@"编号(：|:)[^\r\n]+\r\n");
                            code = regcode.Match(inviteCtx).Value.Replace("编号", "").Replace("：", "").Replace(":", "").Trim();
                            if (Encoding.Default.GetByteCount(code) > 50)
                            {
                                code = "";
                            }  
                            msgType = "中国机械进出口（集团）有限公司";
                            specType = "建设工程";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
