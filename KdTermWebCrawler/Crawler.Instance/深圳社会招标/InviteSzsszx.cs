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
    public class InviteSzsszx : WebSiteCrawller
    {
        public InviteSzsszx()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "深圳市深水水务咨询有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳市深水水务咨询有限公司招标信息";
            this.SiteUrl = "http://www.szsszx.com/tender/pager?key=caigou&pagenumber=20&pageindex=1";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl("http://www.szsszx.com/tender/pager?key=caigou&pagenumber=20&pageindex=1", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagelist")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                string pageTemp = tdNodes.AsString().Replace("&nbsp;", "");
                Regex regpage = new Regex(@"1/\d+");
                try
                {
                    pageInt = int.Parse(regpage.Match(pageTemp).Value.Replace("1/", ""));
                }
                catch (Exception ex) { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.szsszx.com/tender/pager?key=caigou&pagenumber=20&pageindex=" + i.ToString()), Encoding.UTF8);
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new TagNameFilter("li"));

                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        CompositeTag obj = nodeList[j] as CompositeTag;

                        ATag aTag = obj.SearchFor(typeof(ATag), true)[0] as ATag;
                        Span dateSpan = obj.SearchFor(typeof(Span), true)[0] as Span;
                        prjName = aTag.GetAttribute("title");
                        beginDate = dateSpan.ToPlainTextString().Trim(new char[] { '[', ']' });
                        InfoUrl = "http://www.szsszx.com" + aTag.Link; 
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "news-content"), new TagNameFilter("div")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            htmldetail = regexHtml.Replace(htmldetail, "");
                        }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "news-content"), new TagNameFilter("div")));

                        inviteCtx = dtnode.AsString().Replace(" ", "").Replace("[ifgtemso11]", "").Replace("[endif]", "").Replace("<!", "");
                        inviteCtx = Regex.Replace(inviteCtx, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase).Replace("<", "").Replace(">", "").Replace("\n\n\n\t", "\r\n").Replace("\n\n", "\r\n");
                        code = inviteCtx.GetRegex("招标编号,编号", true, 50).Replace("开标时间安排如下", "");
                        Regex regbuildUnit = new Regex(@"(采购人|采购单位)(：|:)[^\r\n]+\r\n");
                        buildUnit = regbuildUnit.Match(inviteCtx).Value.Replace("采购人", "").Replace("采购单位", "").Replace("：", "").Replace(":", "").Trim(); 
                        if (string.IsNullOrEmpty(beginDate))
                        {
                            beginDate = DateTime.Now.ToString("yyyy-MM-dd");
                        }
                        Regex regprjAddress = new Regex(@"地点(：|:)[^\r\n]+\r\n");
                        prjAddress = regprjAddress.Match(inviteCtx).Value.Replace("地点", "").Replace("：", "").Replace(":", "").Trim();
                        specType = "其他";
                        msgType = "深圳市深水水务咨询有限公司";
                        inviteType = ToolHtml.GetInviteTypes(prjName);
                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }

            }

            return list;
        }
    }
}
