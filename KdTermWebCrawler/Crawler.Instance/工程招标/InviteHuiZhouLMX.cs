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
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    /// <summary>
    /// 广东省惠州市龙门县
    /// </summary>
    public class InviteHuiZhouLMX : WebSiteCrawller
    {
        public InviteHuiZhouLMX()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省惠州市龙门县";
            this.Description = "自动抓取广东省惠州市龙门县招标信息";
            this.ExistCompareFields = "Prov,City,Area,Road,Code,ProjectName,InfoUrl";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.SiteUrl = "http://www.ebc.huizhou.gov.cn/index/showList/000000000002/000000000416";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "scott"))), new HasChildFilter(new TagNameFilter("a")))).SearchFor(typeof(ATag), true);
            for (int i = 0; i < sNodes.Count; i++)
            {
                ATag aTag = sNodes[i] as ATag;
                if (aTag.ToPlainTextString().Contains(">>"))
                {
                    pageInt = int.Parse(aTag.Link.ToLower().Replace("gopage(", "").Replace(")", ""));
                }
            }
            parser.Reset();
            //处理后续页
            if (pageInt > 1)
            {
                string cookiestr = string.Empty;
                for (int i = 1; i <= pageInt; i++)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "boardId", "eTime", "newstitle", "pageNO", "sTime", "totalRows", "typeId" }, new string[] { "000000000201", string.Empty, string.Empty, i.ToString(), string.Empty, "0", "000000000002" });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default, ref cookiestr);

                        DealHtml(list, html, crawlAll);
                    }
                    catch (Exception ex)
                    {

                        continue;
                    }

                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }

            return list;
        }

        public void DealHtml(IList list, string html, bool crawlAll)
        {

            Parser parserDtl = new Parser(new Lexer(html));
            NodeList aNodes = parserDtl.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "lefttable")));
            if (aNodes != null && aNodes.Count > 0)
            {
                Type typs = typeof(ATag);
                TableTag table = aNodes[0] as TableTag;
                for (int t = 1; t < table.RowCount - 1; t++)
                {
                    string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty,
                         inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, ctx = string.Empty, CreateTime = string.Empty, HtmlTxt = string.Empty;

                    TableRow tr = table.Rows[t] as TableRow;
                    ATag aTag = tr.SearchFor(typeof(ATag), true)[0] as ATag;

                    InfoUrl = aTag.Link;
                    prjName = table.Rows[t].Columns[1].ToPlainTextString().Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", " ").Trim();
                    endDate = table.Rows[t].Columns[2].ToPlainTextString().Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", " ").Trim();
                    string htmlDtl = string.Empty;
                    try
                    {

                        htmlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                    }
                    catch (Exception ex)
                    {

                        continue;
                    }
                    Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                    htmlDtl = regexHtml.Replace(htmlDtl, "");
                    Parser parserCtx = new Parser(new Lexer(htmlDtl));

                    NodeList ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "printTb lefttable")));
                    if (ctxNode != null && ctxNode.Count > 0)
                    {
                        Parser parserdiv = new Parser(new Lexer(htmlDtl));
                        NodeList aNodesdiv = parserdiv.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "biuuu_button")));
                        HtmlTxt = ctxNode.AsHtml().Replace(aNodesdiv.AsHtml(), "").Trim();
                        Type tp = typeof(ATag);
                        TableTag tabTag = ctxNode[0] as TableTag;
                        string startTime = tabTag.Rows[1].Columns[0].ToPlainTextString().Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", " ").Trim();
                        Regex regex = new Regex(@"时间：\d{4}-\d{1,2}-\d{1,2} \d{1,2}:\d{1,2}:\d{1,2}");
                        Match math = regex.Match(startTime);
                        beginDate = math.Value.Replace("时间：", "").Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", " ").Trim();

                        Regex regexcode = new Regex("(工程编号|项目编号|招标编号)：[^\r\n]+[\r\n]{1}");
                        Match match = regexcode.Match(tabTag.ToPlainTextString());
                        code = match.Value.Substring(match.Value.IndexOf("：") + 1).Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", " ").Trim();

                        Regex regexBuildUnit = new Regex("(招标人|建设单位|招标采购单位)：[^\r\n]+[\r\n]{1}");
                        Match matchBuildUnit = regexBuildUnit.Match(tabTag.ToPlainTextString());
                        buildUnit = matchBuildUnit.Value.Substring(matchBuildUnit.Value.IndexOf("：") + 1).Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", " ").Trim();

                        Regex regexAddress = new Regex("(建设地点|项目地点|工程地点)：[^\r\n]+[\r\n]{1}");
                        Match matchAddress = regexAddress.Match(tabTag.ToPlainTextString());
                        prjAddress = matchAddress.Value.Substring(matchAddress.Value.IndexOf("：") + 1).Trim();
                        ctx = tabTag.Rows[2].Columns[0].ToPlainTextString().Replace("&nbsp;", " ").Replace("\r\n\r\n\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                        if (ctx.Length > 0)
                        {
                            Regex regexCtx = new Regex("<!--[^<]+-->");
                            ctx = regexCtx.Replace(ctx, "");
                        }
                        if (Encoding.Default.GetByteCount(code) > 50)
                        {
                            code = "";
                        }
                        if (buildUnit == "" || buildUnit == null)
                        {
                            buildUnit = "";
                        }
                        if (Encoding.Default.GetByteCount(buildUnit) > 150)
                        {
                            buildUnit = buildUnit.Substring(0, 150);
                        }
                        if (Encoding.Default.GetByteCount(prjAddress) > 200)
                        {
                            prjAddress = "见招标公告内容";
                        }
                        if (beginDate.Length > 0 && endDate.Length > 0)
                        {
                            DateTime begin = new DateTime();
                            DateTime end = new DateTime();
                            try
                            {
                                begin = DateTime.Parse(beginDate);
                                end = DateTime.Parse(endDate);
                            }
                            catch (Exception)
                            {

                            }
                            if (begin > end)
                            {
                                endDate = string.Empty;
                            }
                        }
                    }

                    parserCtx.Reset();

                    ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "toptd_bai")));
                    Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                    beginDate = regDate.Match(ctxNode.AsString()).Value.Trim();
                    if (beginDate == "")
                    {
                        beginDate = string.Empty;
                    }
                    inviteType = ToolHtml.GetInviteTypes(prjName);
                    InviteInfo info = ToolDb.GenInviteInfo("广东省", "惠州市区", "龙门县", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, ctx, remark, "惠州市建设工程交易中心", inviteType, "建设工程", string.Empty, InfoUrl, HtmlTxt);
                    list.Add(info);
                    ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("a"), new HasAttributeFilter("target", "_blank")));
                    NodeList aTagNodes = ctxNode.SearchFor(typeof(ATag), true);
                    for (int a = 0; a < aTagNodes.Count; a++)
                    {

                        ATag fileTage = aTagNodes[a] as ATag;
                        if (fileTage.Link.Contains("http://www.ebc.huizhou.gov.cn/index/loadNewsFile"))
                        {
                            string downloadURL = fileTage.Link;
                            BaseAttach attach = ToolDb.GenBaseAttach(fileTage.ToPlainTextString(), info.Id, downloadURL);
                            base.AttachList.Add(attach);
                        }
                    }
                    if (!crawlAll && list.Count >= this.MaxCount) return;
                }
            }
        }


    }

}
