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
   public class InviteSzHos : WebSiteCrawller
   {
       public InviteSzHos()
           : base()
       { 
           this.Group = "代理机构招标信息";
           this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
           this.Title = "深圳人民医院招标信息";
           this.Description = "自动抓取深圳人民医院招标信息";
           this.SiteUrl = "http://www.szhospital.com/02news/index02.asp?ListId=1&KeyWord=";
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
            NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "cpx12_ff6600")));
            page = Convert.ToInt32(tableNodeList[1].ToPlainTextString());
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.szhospital.com/02news/index02.asp?ListId=36&currentpage=" + i.ToString() + "&keyWord="), Encoding.Default);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList NodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "tablebg")));
                if (NodeList != null && NodeList.Count > 0)
                {
                    TableTag table = new TableTag();
                    table = (TableTag)NodeList[0];
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, img = string.Empty,
                                         HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[2].ToPlainTextString().Trim();
                        if (prjName.Contains("结果") || prjName.Contains("开标时间") || prjName.Contains("通知"))
                        {
                            continue;
                        }
                        ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://www.szhospital.com/" + aTag.Link.Trim(); 
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "cpx12_000000")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "").Replace("<br/>", "\r\n").Replace("<br />", "\r\n");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "cpx12_000000")));
                        inviteCtx = dtnode.AsString();
                        Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                        inviteCtx = regexHtml.Replace(inviteCtx, "");
                        Regex regCode = new Regex(@"采购编号：[^\r\n]+\r\n");
                        code = regCode.Match(inviteCtx).Value.Replace("采购编号：", "").Trim();
                        Regex regdate = new Regex(@"公示日期(：|:)[^\r\n]+\r\n");
                        string date = regdate.Match(inviteCtx).Value.Replace("公示日期：", "").Replace(" ", "").Trim();
                        Regex regDate = new Regex(@"\d{4}年\d{1,2}月\d{1,2}日");
                        beginDate = regDate.Match(date).Value.Trim();
                        Regex regEndDate = new Regex(@"至\d{4}年\d{1,2}月\d{1,2}日");
                        endDate = regEndDate.Match(date).Value.Replace("至", "").Trim();
                        msgType = "深圳人民医院";
                        if (inviteType == "设备材料" || inviteType == "小型施工" || inviteType == "专业分包" || inviteType == "劳务分包" || inviteType == "服务" || inviteType == "勘察" || inviteType == "设计" || inviteType == "监理" || inviteType == "施工")
                        {
                            specType = "建设工程";
                        }
                        else
                        {
                            specType = "其他";
                        }
                        if (beginDate == "")
                        {
                            beginDate = tr.Columns[3].ToPlainTextString().Replace("(", "").Replace(")", "").Trim();
                        }
                        if (endDate == "")
                        {
                            endDate = string.Empty;
                        }
                        prjAddress =  "见招标信息";
                        buildUnit = "深圳人民医院";
                        inviteType = ToolHtml.GetInviteTypes(prjName);
                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "",
                               string.Empty, code, prjName, prjAddress, buildUnit,
                               beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
