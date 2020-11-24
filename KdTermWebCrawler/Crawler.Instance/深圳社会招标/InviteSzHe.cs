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
   public class InviteSzHe : WebSiteCrawller
    {
       public InviteSzHe()
           : base()
       {
           this.Group = "代理机构招标信息";
           this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
           this.Title = "广东省中广核工程有限公司招标信息";
           this.Description = "自动抓取广东省中广核工程有限公司招标信息";
           this.SiteUrl = "http://bidding.cnpec.com.cn/member/tenderinformation.action?&filter_EQ_isinternational=0&filter_EQ_type=0";
       }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 4;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8, ref cookiestr);
            }
            catch (Exception ex)
            {

                return list;
            }
           
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&page.number=" + i.ToString()), Encoding.UTF8);
                    }
                    catch (Exception ex) { continue; }
                }
                Parser parser = new Parser(new Lexer(htl));
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table_title3")));
                if (tableNodeList.Count > 0)
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
                        prjName = tr.Columns[4].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[4].SearchFor(typeof(ATag), true)[0] as ATag;
                        beginDate = tr.Columns[6].ToPlainTextString().Trim();
                        endDate = tr.Columns[8].ToPlainTextString().Trim();
                         
                        InfoUrl = "http://bidding.cnpec.com.cn/member/" + aTag.Link.Replace("amp;", "").Trim();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                        if (dtnode.Count > 0)
                        {
                            TableTag tableNode = (TableTag)dtnode[0];
                            HtmlTxt = dtnode.AsHtml();
                            for (int k = 1; k < tableNode.RowCount; k++)
                            {
                                TableRow trow = tableNode.Rows[k];
                                for (int c = 0; c < trow.ColumnCount; c++)
                                {
                                    string tr1 = string.Empty;
                                    tr1 = trow.Columns[c].ToPlainTextString().Trim();
                                    inviteCtx += "\r\n" + tr1;
                                }
                            }
                            inviteCtx += "\r\n";
                            inviteCtx = inviteCtx.Replace("&amp;", "").Replace("nbsp;", "").Replace("&rdquo;", "").Replace("&ldquo;", "").Trim();
                            Regex regCode = new Regex(@"招标编号(：|:)[^\r\n]+\r\n");
                            code = regCode.Match(inviteCtx).Value.Replace("招标编号：", "").Trim();
                            Regex regBuidUnit = new Regex(@"(招标人|建设单位)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(inviteCtx).Value.Replace("招标人：", "").Replace("建设单位:", "").Trim();
                            msgType = "中广核工程有限公司";
                            specType = "建设工程";
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            InfoUrl = InfoUrl.Replace("filter_EQ_isinternational=0", "filter_EQ_isinternational=1"); 
                            prjAddress = "见招标信息";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "",
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
