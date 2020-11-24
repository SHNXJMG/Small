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
    public class InviteZhJianJS : WebSiteCrawller
    {
        public InviteZhJianJS()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省湛江市建设工程网络招标信息";
            this.Description = "自动抓取广东省湛江市建设工程网络招标信息";
            this.ExistCompareFields = "Code,ProjectName,InfoUrl";
            this.SiteUrl = "http://zb.zjcic.net/Default.aspx?tabid=65";
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
            }
            catch (Exception ex)
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "dnn_ctr395_ProjectList_pager")));
            if (nodeList != null && nodeList.Count > 0)
            {

                Regex regDate = new Regex(@"\d下一页");
                page = Convert.ToInt32(regDate.Match(nodeList.AsString().Trim()).ToString().Replace("下一页", "").Trim());
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&page=" + i.ToString()), Encoding.Default);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "dnn_ctr395_ProjectList_grdData")));
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
                        code = tr.Columns[0].ToPlainTextString().Trim();
                        prjName = tr.Columns[1].ToPlainTextString().Trim();
                        endDate = tr.Columns[4].ToPlainTextString().Replace("&nbsp; ", "").Trim().Substring(0, 10);
                        beginDate = tr.Columns[3].ToPlainTextString().Replace("&nbsp; ", "").Trim().Substring(0, 10);
                        ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;

                        InfoUrl = "http://zb.zjcic.net" + aTag.Link.Replace("amp;", "").Trim();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            Logger.Error("InviteZhJianJS");
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "dnn_ctr408_ProjectView_INSTRUCTION")));
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            inviteCtx = dtnode.AsString().Replace("&#160;", "").Trim();
                            Regex regBuidUnit = new Regex(@"(招标单位|招标人)：[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(inviteCtx).Value.Replace("招标单位：", "").Replace("：", "").Replace("&#160;", "").Trim();
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            Regex regPrjAddr = new Regex(@"(工程地点|工程地址|工程建设地点)(：|:)[^\r\n]+\r\n");
                            prjAddress = regPrjAddr.Match(inviteCtx).Value.Replace("工程地点：", "").Replace("工程地址", "").Replace("工程建设地点", "").Replace("：", "").Trim();
                            msgType = "湛江市建设工程交易中心";
                            specType = "建设工程";
                            if (prjAddress == "")
                            {
                                prjAddress = "见招标信息";
                            }
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "湛江市区", "",
                                string.Empty, code, prjName, prjAddress, buildUnit,
                                beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
