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
    public class InviteSzzdzb : WebSiteCrawller
    {
        public InviteSzzdzb()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "深圳市振东招标代理有限公司";
            this.Description = "自动抓取深圳市振东招标代理有限公司招标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.SiteUrl = "http://www.szzdzb.cn/Product-index-id-8.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("bgColor", "#EEF4F9")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                string pageTemp = tdNodes.AsString().Replace("&nbsp;", "").Replace(" ", "").Trim();
                Regex regpage = new Regex(@"1/[0-9]+页");
                try
                {
                    pageInt = int.Parse(regpage.Match(pageTemp).Value.Split('/')[1].Replace("页", "").Trim());
                }
                catch (Exception ex) { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.szzdzb.cn/Product-index-id-8-p-" + i + ".html", Encoding.UTF8);
                    }
                    catch  
                    {
                        continue;
                    }
                }

                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "hangao27"))), new TagNameFilter("table")));

                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        code = tr.Columns[0].ToPlainTextString().Trim();
                        prjName = tr.Columns[1].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://www.szzdzb.cn" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "hangao27"))), new TagNameFilter("table")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            htmldetail = regexHtml.Replace(htmldetail, "");
                        }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "hangao27"))), new TagNameFilter("table")));

                        inviteCtx = System.Web.HttpUtility.HtmlDecode(dtnode.AsString().Replace("打印本页 || 关闭窗口", ""));
                        Regex regCtx = new Regex(@"([\r\n]+)|([\t]+)");
                        inviteCtx = regCtx.Replace(inviteCtx, "\r\n");
                        Regex regBeginDate = new Regex(@"发布时间：[^\r\n]+\r\n");
                        beginDate = regBeginDate.Match(inviteCtx).Value.Replace("发布时间", "").Replace("：", "").Trim();
                        specType = "其他";
                        msgType = "深圳市振东招标代理有限公司";
                        inviteType = ToolHtml.GetInviteTypes(prjName);
                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                        list.Add(info);
                        dtlparser.Reset();
                        dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("bgColor", "#CCCCCC")));
                        NodeList FileTag = dtnode.SearchFor(typeof(ATag), true);
                        if (FileTag != null && FileTag.Count > 0)
                        {
                            for (int f = 0; f < FileTag.Count; f++)
                            {
                                ATag file = FileTag[f] as ATag;
                                if (file.Link.ToUpper().Contains(".DOC"))
                                {
                                    BaseAttach attach = ToolDb.GenBaseAttach(file.ToPlainTextString(), info.Id, "http://www.szzdzb.cn" + file.Link);
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
