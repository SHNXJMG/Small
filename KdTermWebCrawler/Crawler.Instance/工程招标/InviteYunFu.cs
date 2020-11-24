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
    public class InviteYunFu : WebSiteCrawller
    {
        public InviteYunFu()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省云浮工程建设招标信息";
            this.Description = "自动抓取广东省云浮工程建设招标信息";
            this.ExistCompareFields = "ProjectName,InfoUrl";
            this.MaxCount = 10;
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.SiteUrl = "http://gcjs.yunfu.gov.cn/gcjs/xmnews.jsp?columnid=014002002002";
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
            NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "fanyie")));
            if (tableNodeList != null && tableNodeList.Count > 0)
            {
                Regex regexPage = new Regex(@"共\d+页");
                page = int.Parse(regexPage.Match(tableNodeList.AsString()).Value.Trim(new char[] { '共', '页' }));
            }
            for (int j = 1; j < page; j++)
            {
                //if (j > 1)
                //{
                //    try
                //    {
                //        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(GetStartUrl() + "&ipage=" + j.ToString()), Encoding.Default);
                //    }
                //    catch (Exception ex) { continue; }
                //}
                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "list_headnews")), true), new TagNameFilter("li")));
                for (int i = 0; i < nodeList.Count; i++)
                {
                    ATag aTag = nodeList.SearchFor(typeof(ATag), true)[i] as ATag;
                    string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                               prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                               specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                               remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                               CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                    prjName = nodeList[i].ToPlainTextString().Replace(" ", "");
                    InfoUrl = "http://gcjs.yunfu.gov.cn" + aTag.Link;
                    Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                    beginDate = regDate.Match(prjName).Value.Trim();
                    prjName = prjName.Replace(beginDate, "").Trim();
                    if (prjName.Contains("招标公告") || prjName.Contains("补充公告"))
                    {
                        inviteType = ToolHtml.GetInviteTypes(prjName);
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            Logger.Error("InviteYunFu");
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "STYLE10")));
                        if (dtnode.Count <= 0)
                        {
                            parserdetail = new Parser(new Lexer(htmldetail));
                            dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("TABLE"), new HasAttributeFilter("id", "Table1")));
                        }
                        if (dtnode.Count > 0)
                        {
                            inviteCtx = dtnode.AsString().Replace("\n", "\r\n");
                            HtmlTxt = dtnode.AsHtml();
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            inviteCtx = regexHtml.Replace(inviteCtx, "");
                            Regex regBuidUnit = new Regex(@"(招 标 人|招标人|招 标人)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(inviteCtx).Value.Replace("招 标 人：", "").Replace("招 标人：", "").Replace("招标人：", "").Trim();
                            if (buildUnit.Contains("招标代理"))
                            {
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理")).ToString().Trim();
                            }
                            msgType = "云浮市工程建设交易中心";
                            specType = "建设工程";
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            prjAddress = "见招标信息";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "云浮市区", "",
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
