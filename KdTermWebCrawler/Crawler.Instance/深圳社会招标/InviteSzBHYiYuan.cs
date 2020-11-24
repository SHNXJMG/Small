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
   public class InviteSzBHYiYuan : WebSiteCrawller
    {
       public InviteSzBHYiYuan()
           : base(true)
       {
           this.Group = "代理机构招标信息";
           this.Title = "深圳滨海医院招标信息";
           this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
           this.Description = "自动抓取深圳滨海医院招标信息";
           this.SiteUrl = "http://www.szbhyy.com/dolist.asp?type=zb";
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
            NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "30")));
            if (tableNodeList != null && tableNodeList.Count > 0)
            {
                try
                {
                    string s = tableNodeList.AsString();
                    Regex regexPage = new Regex(@"/\d+页");
                    page = Convert.ToInt32(regexPage.Match(tableNodeList.AsString()).Value.Replace("/", "").Replace("页", "").Trim());
                }
                catch { page = 1; }
            }
            
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&sub_type=&page=" + i.ToString()), Encoding.Default);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList NodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "730")));
                if (NodeList != null && NodeList.Count > 0)
                {
                    for (int j = 0; j < NodeList.Count; j++)
                    {
                        TableTag table = new TableTag();
                        try
                        {
                           table = (TableTag)NodeList[j];
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, img = string.Empty,
                                      HtmlTxt = string.Empty;
                            TableRow tr = table.Rows[0];
                            prjName = tr.Columns[2].ToPlainTextString().Trim();
                            beginDate = tr.Columns[3].ToPlainTextString().Trim();
                            ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                            InfoUrl = "http://www.szbhyy.com/" + aTag.Link.Trim();  
                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Trim();
                                Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                                NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content_info")));
                                HtmlTxt = dtnodeHTML.AsHtml();
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "").Replace("<br/>", "\r\n").Replace("<br />", "\r\n");
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            Parser parserdetail = new Parser(new Lexer(htmldetail));
                            NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content_info")));
                            inviteCtx = dtnode.AsString();
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            inviteCtx = regexHtml.Replace(inviteCtx, "");
                            Regex regCode = new Regex(@"(工 程 编 号|招标编号)：[^\r\n]+\r\n");
                            code = regCode.Match(inviteCtx).Value.Replace("工 程 编 号：", "").Replace("招标编号：", "").Trim();
                            Regex regBuidUnit = new Regex(@"(招标人|招标单位|招 标 人)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(inviteCtx).Value.Replace("招标单位：", "").Replace("招标人：", "").Replace("招 标 人：", "").Trim();
                            Regex regprjAddress = new Regex(@"工 程 地 点(：|:)[^\r\n]+\r\n");
                            prjAddress = regprjAddress.Match(inviteCtx).Value.Replace("工 程 地 点：", "").Trim();
                            msgType = "香港大学深圳医院";
                            if (inviteType == "设备材料" || inviteType == "小型施工" || inviteType == "专业分包" || inviteType == "劳务分包" || inviteType == "服务" || inviteType == "勘察" || inviteType == "设计" || inviteType == "监理" || inviteType == "施工")
                            {
                                specType = "建设工程";
                            }
                            else
                            {
                                specType = "其他";
                            }
                            if (prjAddress == "")
                            {
                                prjAddress = "见招标信息";
                            }
                            if (buildUnit == "")
                            {
                                buildUnit = "香港大学深圳医院";
                            }
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
