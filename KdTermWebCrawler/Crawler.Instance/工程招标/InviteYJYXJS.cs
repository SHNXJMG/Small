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
    public class InviteYJYXJS : WebSiteCrawller
    {
        public InviteYJYXJS()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省阳江市阳西县建设工程招标信息";
            this.Description = "自动抓取广东省阳江市阳西县建设工程招标信息";
            this.SiteUrl = "http://www.yjgcjy.cn/ZBList.aspx?ItemID=91&SiteId=23";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
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
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default, ref cookiestr);
                Regex regexHtml = new Regex(@"<script[^<]*</script>");
                htl = regexHtml.Replace(htl, "");
            }
            catch (Exception ex)
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("colSpan", "6")));
            if (nodeList != null && nodeList.Count > 0)
            {
                Regex regexPage = new Regex(@"共\d+页");
                page = int.Parse(regexPage.Match(nodeList.AsString()).Value.Trim(new char[] { '共', '页' }));
            }
            for (int i = 1; i < page; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(htl);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__VIEWSTATE",
                        "key",
                        "AxGridView1$ctl23$ctl07",
                        "AxGridView1$ctl23$pageList",
                        "__VIEWSTATEENCRYPTED",
                        "__EVENTVALIDATION"
                    }, new string[]{
                        "AxGridView1$ctl23$ctl03",
                        string.Empty,
                        viewState,
                         string.Empty,
                        "20",
                        (i-1).ToString(),
                        string .Empty,
                        eventValidation
                    });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "AxGridView1")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                            prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                            specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                            remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                            CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        code = tr.Columns[2].ToPlainTextString().Trim();
                        prjName = tr.Columns[3].ToPlainTextString().Trim();
                        //endDate = tr.Columns[4].ToPlainTextString().Replace("&nbsp; ", "").Trim().Substring(0, 10);
                        ATag aTag = tr.Columns[5].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://www.yjgcjy.cn/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            Logger.Error("InviteYJYXJS");
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellSpacing", "1")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            TableTag tableRow = (TableTag)dtnode[0];
                            for (int k = 1; k < tableRow.RowCount; k++)
                            {
                                TableRow trow = tableRow.Rows[k];
                                for (int c = 0; c < trow.ColumnCount; c++)
                                {
                                    string tr1 = string.Empty;
                                    tr1 = trow.Columns[c].ToPlainTextString().Trim();
                                    inviteCtx += tr1;
                                }
                                inviteCtx += "\r\n";
                            }
                            Regex regPrjAddr = new Regex(@"工程建设地址：[^\r\n]+\r\n");
                            try
                            {
                                prjAddress = regPrjAddr.Match(inviteCtx).Value.Replace("工程建设地址", "").Replace("：", "").Replace("。", "").Replace("、", "").Replace("；", "").Replace("，", "").Trim();
                                if (Encoding.Default.GetByteCount(prjAddress) > 200 || prjAddress == "")
                                {
                                    prjAddress = "见招标详细信息";
                                }
                            }
                            catch (Exception)
                            {
                                prjAddress = "见招标详细信息";
                            }
                            Regex regBegin = new Regex(@"公告发布时间：[^\r\n]+[\r\n]{1}");
                            beginDate = regBegin.Match(inviteCtx).Value.Replace("公告发布时间：", "").Trim();
                            string date = beginDate.Replace(" ", "").Trim();
                            Regex regDate = new Regex(@"\d{4}年\d{1,2}月\d{1,2}日");
                            beginDate = regDate.Match(date).Value.Trim();
                            if (beginDate == "")
                            {
                                Regex regDateT = new Regex(@"[u4e00-u9fa5]{4}年[u4e00-u9fa5]{1,2}月[u4e00-u9fa5]{1,2}日");
                                beginDate = regDateT.Match(inviteCtx).Value.Replace("公告发布时间：", "").Trim();
                            }
                            if (beginDate == "")
                            {
                                beginDate = string.Empty;
                            }
                            Regex bildUnit = new Regex(@"建设单位：[^\r\n]+[\r\n]{1}");
                            buildUnit = bildUnit.Match(inviteCtx).Value.Replace("建设单位：", "").Trim();
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            msgType = "阳江市建设工程交易中心";
                            specType = "建设工程";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            inviteCtx = inviteCtx.Replace("<?", "").Replace("xml:namespace prefix = o ns = ", "").Replace("urn:schemas-microsoft-com:office:office", "").Replace("/>", "").Trim();
                            inviteCtx = inviteCtx.Replace("<?", "").Replace("xml:namespace prefix = ns0 ns = ", "").Replace("urn:schemas-microsoft-com:office:smarttags", "").Replace("/>", "").Trim();
                            inviteCtx = inviteCtx.Replace("<?", "").Replace("xml:namespace prefix = st1 ns = ", "").Replace("urn:schemas-microsoft-com:office:smarttags", "").Replace("/>", "").Trim();
                            inviteCtx = inviteCtx.Replace("xml:namespace prefix = st1", "").Trim();
                            inviteCtx = inviteCtx.Replace("点击进入留言", "").Trim();
                            code = code.Replace("；", "").Replace("：", "").Trim();
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "阳江市区", "阳西县", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parserdetail.Reset();
                            NodeList fileNode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellSpacing", "1")));
                            if (fileNode != null && fileNode.Count > 0 && fileNode[0] is TableTag)
                            {
                                TableTag fileTable = fileNode[0] as TableTag;
                                for (int f = 10; f < fileTable.RowCount; f++)
                                {
                                    TableRow trowFile = fileTable.Rows[f];
                                    for (int z = 0; z < 1; z++)
                                    {
                                        string tr1 = string.Empty;
                                        tr1 = trowFile.Columns[z].ToPlainTextString().Trim();
                                        if (tr1.Contains("下载招标文件：") || tr1.Contains("下载工程量清单：") || tr1.Contains("下载图纸："))
                                        {
                                            if (fileTable.Rows[f].Columns[z + 1].ToPlainTextString().Trim() != "")
                                            {

                                                int tt = fileTable.Rows[f].Columns[z + 1].SearchFor(typeof(ATag), true).Count;
                                                for (int ii = 0; ii < tt; ii++)
                                                {
                                                    string st3 = fileTable.Rows[f].Columns[z + 1].SearchFor(typeof(ATag), true)[ii].ToPlainTextString().Trim();
                                                    ATag aTagCh = fileTable.Rows[f].Columns[z + 1].SearchFor(typeof(ATag), true)[ii] as ATag;
                                                    string urlValues = "http://www.yjgcjy.cn" + aTagCh.Link;
                                                    if (aTagCh.Link.Contains("http://www.yjgcjy.cn"))
                                                    {
                                                        urlValues = aTagCh.Link;
                                                    }
                                                    if (st3 != "")
                                                    {
                                                        BaseAttach attach = ToolDb.GenBaseAttach(st3, info.Id, urlValues);
                                                        base.AttachList.Add(attach);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                }

                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }

                        else
                        {
                            code = "";
                            Parser parserdetailtwo = new Parser(new Lexer(htmldetail));
                            NodeList dtnodetwo = parserdetailtwo.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "nr")));
                            if (dtnodetwo != null && dtnodetwo.Count > 0)
                            {
                                HtmlTxt = dtnodetwo.AsHtml();
                                inviteCtx = dtnodetwo.AsString().Replace("。", "").Trim();
                                Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                                inviteCtx = regexHtml.Replace(inviteCtx, "").Replace("O", "〇");
                                Regex regPrjAddr = new Regex(@"(工程建设地点|工程地点)：[^\r\n]+\r\n");
                                prjAddress = regPrjAddr.Match(inviteCtx).Value.Replace("工程建设地点", "").Replace("工程地点", "").Replace("：", "").Trim();
                                if (prjAddress == "")
                                {
                                    prjAddress = "见招标详细信息";
                                }
                                Regex regDateT = new Regex(@"[^u4e00-u9fa5]{4}年[^u4e00-u9fa5]{1,3}月[^u4e00-u9fa5]{1,3}日");
                                beginDate = regDateT.Match(inviteCtx).Value.Trim();
                                beginDate = returnS(beginDate);
                                if (beginDate == "")
                                {
                                    beginDate = string.Empty;
                                }
                                Regex bildUnit = new Regex(@"发包人：[^\r\n]+[\r\n]{1}");
                                buildUnit = bildUnit.Match(inviteCtx).Value.Replace("发包人：", "").Trim();
                                if (buildUnit == "")
                                {
                                    buildUnit = "";
                                }
                                msgType = "阳江市建设工程交易中心";
                                specType = "建设工程";
                                inviteType = ToolHtml.GetInviteTypes(prjName);
                                inviteCtx = inviteCtx.Replace("<?", "").Replace("xml:namespace prefix = o ns = ", "").Replace("urn:schemas-microsoft-com:office:office", "").Replace("/>", "").Trim();
                                inviteCtx = inviteCtx.Replace("<?", "").Replace("xml:namespace prefix = ns0 ns = ", "").Replace("urn:schemas-microsoft-com:office:smarttags", "").Replace("/>", "").Trim();
                                inviteCtx = inviteCtx.Replace("<?", "").Replace("xml:namespace prefix = st1 ns = ", "").Replace("urn:schemas-microsoft-com:office:smarttags", "").Replace("/>", "").Trim();
                                inviteCtx = inviteCtx.Replace("xml:namespace prefix = st1", "").Trim();
                                inviteCtx = inviteCtx.Replace("点击进入留言", "").Trim();
                                inviteCtx = inviteCtx.Replace("〇", "0");
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "阳江市区", "阳西县", string.Empty, code, prjName,
                                    prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType,
                                    inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }

                    }
                }
            }
            return null;
        }
        private string returnS(string st)
        {
            if (st.Contains("十月"))
            {
                st = st.Replace("十月", "10月").ToString();
            }
            if (st.Contains("十日"))
            {
                st = st.Replace("十日", "10").ToString();
            }
            if (st.Contains("二十"))
            {
                st = st.Replace("二十", "2").ToString();
            }
            if (st.Contains("三十"))
            {
                st = st.Replace("三十", "3").ToString();
            }
            if (st.Contains("十"))
            {
                st = st.Replace("十", "1").ToString();
            }
            if (st.Contains("一"))
            {
                st = st.Replace("一", "1").ToString();
            }
            if (st.Contains("二"))
            {
                st = st.Replace("二", "2").ToString();
            }
            if (st.Contains("三"))
            {
                st = st.Replace("三", "3").ToString();
            }
            if (st.Contains("四"))
            {
                st = st.Replace("四", "4").ToString();
            }
            if (st.Contains("五"))
            {
                st = st.Replace("五", "5").ToString();
            }
            if (st.Contains("六"))
            {
                st = st.Replace("六", "6").ToString();
            }
            if (st.Contains("七"))
            {
                st = st.Replace("七", "7").ToString();
            }
            if (st.Contains("八"))
            {
                st = st.Replace("八", "8").ToString();
            }
            if (st.Contains("九"))
            {
                st = st.Replace("九", "9").ToString();
            }
            if (st.Contains("〇") || st.Contains("零"))
            {
                st = st.Replace("〇", "0").ToString();
            }
            return st;
        }
    }
}
