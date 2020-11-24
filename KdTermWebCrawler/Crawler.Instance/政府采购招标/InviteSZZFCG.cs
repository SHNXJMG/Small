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
using System.Threading;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteSZZFCG : WebSiteCrawller
    {
        public InviteSZZFCG()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "广东省深圳市政府采购";
            this.Description = "自动抓取广东省深圳市政府采购招标信息";
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,10:20,10:50,11:30,14:05,14:25,14:50,15:25,16:00,16:50,19:00";
            this.MaxCount = 80;
            this.SiteUrl = "http://www.szzfcg.cn/portal/topicView.do?method=view&id=1660";
            this.ExistsHtlCtx = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int sqlCount = 0;
            int count = 0;
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("name", "__ec_pages")));
            if (pageNode != null && pageNode.Count > 0)
            {
                SelectTag selectTag = pageNode[0] as SelectTag;
                pageInt = selectTag.OptionTags.Length;
            }
            string cookiestr = string.Empty;
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "ec_i", "topicChrList_20070702_crd", "topicChrList_20070702_f_a", "topicChrList_20070702_p", "topicChrList_20070702_s_name", "id", "method", "__ec_pages", "topicChrList_20070702_rd", "topicChrList_20070702_f_name", "topicChrList_20070702_f_ldate" }, new string[] { "topicChrList_20070702", "20", string.Empty, i.ToString(), string.Empty, "1660", "view", (i - 1).ToString(), "20", string.Empty, string.Empty });
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { }
                }
                parser = new Parser(new Lexer(html));
                NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "topicChrList_20070702_table")));
                if (tdNodes != null && tdNodes.Count > 0)
                {
                    TableTag table = tdNodes[0] as TableTag;

                    for (int t = 3; t < table.RowCount; t++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[t];
                        prjName = tr.Columns[2].ToPlainTextString().Trim().ToRegString();
                        //try
                        //{
                            inviteType = tr.Columns[3].ToPlainTextString().Trim();
                            beginDate = tr.Columns[4].ToPlainTextString().Trim();
                        //}
                        //catch { DateTime beginDa = DateTime.Today; beginDate = beginDa.ToString("yyyy-MM-dd HH:mm:ss"); }





                        ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;

                        Regex regexLink = new Regex(@"id=[^-]+");
                        InfoUrl = "http://www.szzfcg.cn/portal/documentView.do?method=view&" + regexLink.Match(aTag.Link).Value;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                        }
                        catch (Exception ex) { }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        inviteCtx = dtnode.AsString().Replace(" ", "").Replace("\t", "").Trim("\r\n".ToCharArray()).Replace("&ldquo;", "“").Replace("&rdquo;", "”").Replace("双击鼠标自动滚屏[打印此页][关闭此页]", "");
                        inviteCtx = System.Web.HttpUtility.HtmlDecode(inviteCtx);
                        Regex regCtx = new Regex(@"[\r\n]+");
                        inviteCtx = regCtx.Replace(inviteCtx, "\r\n");
                        Regex regcode = new Regex(@"(招标编号|项目编号)(：|:)([0-9]|[A-Za-z]|[-])+");
                        code = regcode.Match(inviteCtx).Value.Replace("招标编号", "").Replace("项目编号", "").Replace("：", "").Replace(":", "").Trim();

                        if (string.IsNullOrEmpty(inviteCtx) || string.IsNullOrEmpty(HtmlTxt))
                        {
                            parser = new Parser(new Lexer(htmldetail));
                            NodeFilter filter = new TagNameFilter("body");
                            NodeList ctxList = parser.ExtractAllNodesThatMatch(filter);
                            inviteCtx = ctxList.AsString();
                            HtmlTxt = ctxList.AsHtml();
                        }
                        if (string.IsNullOrEmpty(inviteCtx) || string.IsNullOrEmpty(HtmlTxt))
                        {
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
                            HtmlTxt = regexHtml.Replace(htmldetail, "");
                            inviteCtx = Regex.Replace(HtmlTxt, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase).Replace(" ", "").Replace("\t", "");
                        }
                        msgType = "深圳政府采购";
                        specType = "政府采购";
                        prjAddress = "深圳市";
                        if (inviteType.Contains("160"))
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳政府采购", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, string.Empty, InfoUrl, HtmlTxt);
                        if (!crawlAll && sqlCount >= this.MaxCount) return null;
                        sqlCount++;
                        if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                        {
                            count++;
                            parser = new Parser(new Lexer(htmldetail));
                            NodeList fileNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (fileNode != null && fileNode.Count > 0)
                            {
                                for (int f = 0; f < fileNode.Count; f++)
                                {
                                    ATag tag = fileNode[f] as ATag;
                                    if (tag.IsAtagAttach())
                                    {
                                        try
                                        {
                                            BaseAttach attach = null;
                                            if (tag.Link.ToLower().Contains(".com") || tag.Link.ToLower().Contains(".cn"))
                                            {
                                                attach = ToolHtml.GetBaseAttachByUrl(tag.Link.Replace("&amp;", "&"), tag.LinkText, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                            }
                                            else
                                            {
                                                attach = ToolHtml.GetBaseAttachByUrl("http://www.szzfcg.cn" + tag.Link.Replace("&amp;", "&"), tag.LinkText, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                            }
                                            if (attach != null)
                                                ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                                        }
                                        catch { }
                                    }
                                }
                            }
                            if (count >= 10)
                            {
                                count = 0;
                                Thread.Sleep(1000 * 300);
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
