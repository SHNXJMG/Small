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
    public class InviteFoshanSD : WebSiteCrawller
    {
        public InviteFoshanSD()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省佛山市顺德区";
            this.Description = "自动抓取广东省佛山市顺德区招标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.SiteUrl = "http://www.sdcin.com.cn/page.php?singleid=3&ClassID=1";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            int crawlMax = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(this.SiteUrl + "&page=0"), Encoding.Default).Replace("&nbsp;", "");
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("id", "page_PageList")));
            if (sNode != null && sNode.Count > 0)
            {
                SelectTag select = sNode[0] as SelectTag;
                pageInt = select.OptionTags.Length;
            }

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try { html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + (i - 1).ToString(), Encoding.Default); }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(html));
                sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("onmouseover", "this.style.backgroundColor=\"#EFFCD0\";")));
                if (sNode != null && sNode.Count > 0)
                {
                    for (int n = 0; n < sNode.Count; n++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = sNode[n] as TableRow;
                        prjName = tr.Columns[0].ToPlainTextString().Trim();
                        code = tr.Columns[1].ToPlainTextString().Trim();
                        beginDate = tr.Columns[2].ToPlainTextString().Trim();
                        ATag aTag = tr.GetATag();
                        if (aTag == null) continue;
                        Regex regexLink = new Regex(@"id=[^-]+");
                        InfoUrl = "http://www.sdcin.com.cn/viewzbggnew.php?" + regexLink.Match(aTag.Link).Value;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "zbtgHTML"), new TagNameFilter("td")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = htmldetail.Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n").Replace("<br/>", "\r\n");
                        }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "zbtgHTML"), new TagNameFilter("td")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            inviteCtx = HtmlTxt.ToCtxString().Replace("startprint", "");
                            TableTag table = dtnode[0] as TableTag;
                            if (table != null && table.RowCount > 0)
                            {
                                for (int t = 0; t < table.RowCount; t++)
                                {
                                    for (int c = 0; c < table.Rows[t].ColumnCount; c++)
                                    {
                                        if (table.Rows[t].Columns[c].ToPlainTextString().Replace(" ", "").Contains("招标人"))
                                        {
                                            if (string.IsNullOrEmpty(buildUnit))
                                            {
                                                buildUnit = table.Rows[t].Columns[c + 1].ToPlainTextString().Trim();
                                            }
                                        }
                                        else if (table.Rows[t].Columns[c].ToPlainTextString().Replace(" ", "").Contains("公告时间"))
                                        {
                                            if (string.IsNullOrEmpty(beginDate))
                                            {
                                                beginDate = table.Rows[t].Columns[c + 1].ToPlainTextString().Trim().Replace("年", "-").Replace("月", "-").Replace("日", "");
                                            }
                                        }
                                        else if (table.Rows[t].Columns[c].ToPlainTextString().Replace(" ", "").Contains("工程地点"))
                                        {
                                            if (string.IsNullOrEmpty(prjAddress))
                                            {
                                                prjAddress = table.Rows[t].Columns[c + 1].ToPlainTextString().Trim();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(beginDate))
                        {
                            Regex regDate = new Regex(@"请于\d{4}年\d{1,2}月\d{1,2}日");
                            beginDate = regDate.Match(inviteCtx.Replace("\r\n", "").Replace(" ", "").Replace("\t", "")).Value.Replace("请于", "");
                        }
                        if (string.IsNullOrEmpty(beginDate))
                        {
                            Regex regDate = new Regex(@"请于\d{4}-\d{1,2}-\d{1,2}");
                            beginDate = regDate.Match(inviteCtx.Replace("\r\n", "").Replace(" ", "").Replace("\t", "")).Value.Replace("请于", "");
                        }
                        if (string.IsNullOrEmpty(beginDate))
                        {
                            if (inviteCtx.Length > 250)
                            {
                                Regex regDate = new Regex(@"\d{4}年\d{1,2}月\d{1,2}日");
                                beginDate = regDate.Match(inviteCtx.Substring(inviteCtx.Length - 250, 250).Replace("\r\n", "").Replace(" ", "").Replace("\t", "")).Value;
                            }
                        }
                        if (string.IsNullOrEmpty(beginDate))
                        {
                            if (inviteCtx.Length > 250)
                            {
                                Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                                beginDate = regDate.Match(inviteCtx.Substring(inviteCtx.Length - 250, 250).Replace("\r\n", "").Replace(" ", "").Replace("\t", "")).Value;
                            }
                        }
                        if (string.IsNullOrEmpty(beginDate))
                        {
                            beginDate = DateTime.Now.ToString();
                        }
                        msgType = "佛山市顺德区建设工程交易中心";
                        specType = "建设工程";
                        inviteType = ToolHtml.GetInviteTypes(prjName);
                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "佛山市区", "顺德区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                        list.Add(info);
                        NodeList filenode = dtnode.SearchFor(typeof(ATag), true);
                        if (filenode != null && filenode.Count > 0)
                        {
                            for (int f = 0; f < filenode.Count; f++)
                            {
                                ATag fileTag = filenode[f] as ATag;
                                if (fileTag.IsAtagAttach())
                                {
                                    BaseAttach attach = ToolDb.GenBaseAttach(fileTag.ToPlainTextString().Trim(), info.Id, "http://www.sdcin.com.cn" + fileTag.Link);
                                    base.AttachList.Add(attach);
                                }
                            }
                        }
                        if (!crawlAll && list.Count >= this.MaxCount) return list;

                    }
                }

            }
            return list;
        }
    }
}
