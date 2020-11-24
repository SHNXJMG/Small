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
    public class BidSzsszx : WebSiteCrawller
    {
        public BidSzsszx()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "深圳市深水水务咨询有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳市深水水务咨询有限公司中标信息";
            this.SiteUrl = "http://www.szsszx.com/tender/list/zhongbiao";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.szsszx.com/tender/pager?key=zhongbiao&pagenumber=20&pageindex=1"), Encoding.UTF8);
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
                    html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.szsszx.com/tender/pager?key=zhongbiao&pagenumber=20&pageindex=" + i.ToString()), Encoding.UTF8);
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new TagNameFilter("li"));

                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
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

                        bidCtx = dtnode.ToHtml().ToCtxString();
                        bidCtx = Regex.Replace(bidCtx, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase).Replace("<", "").Replace(">", "").Replace("\n\n\n\t", "\r\n").Replace("\n\n", "\r\n");
                        Regex regCode = new Regex(@"招标编号(：|:)[^\r\n]+\r\n");
                        code = regCode.Match(bidCtx).Value.Replace("招标编号", "").Replace("：", "").Replace(":", "").Trim();
                        if (Encoding.Default.GetByteCount(code) > 50)
                        {
                            code = "";
                        }
                        Regex regbuildUnit = new Regex(@"(采购人|采购单位|采购代理机构)(：|:)[^\r\n]+\r\n");
                        buildUnit = regbuildUnit.Match(bidCtx).Value.Replace("采购人", "").Replace("采购单位", "").Replace("采购代理机构", "").Replace("：", "").Replace(":", "").Trim(); 
                        prjAddress = bidCtx.GetAddressRegex(); 
                        bidUnit = bidCtx.GetBidRegex();// regBidUnit.Match(bidCtx).Value.Replace("中标单位", "").Replace("：", "").Replace(":", "").Trim();

                        Regex regBidMoneystr = new Regex(@"(中标价|价格|金额)(：|:)[^\r\n]+\r\n");
                        string monerystr = regBidMoneystr.Match(bidCtx).Value.Replace("中标价", "").Replace("价格", "").Replace("金额", "").Replace("万元整", "").Replace("：", "").Replace(":", "").Replace(",", "").Replace("，", "").Trim();
                        Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");

                        if (!string.IsNullOrEmpty(regBidMoney.Match(monerystr).Value))
                        {
                            if ((monerystr.Contains("万元") || monerystr.Contains("万美元")) && !monerystr.Contains("万元整"))
                            {
                                bidMoney = regBidMoney.Match(monerystr).Value;
                            }
                            else
                            {
                                try
                                {
                                    bidMoney = (decimal.Parse(regBidMoney.Match(monerystr).Value) / 10000).ToString();
                                    if (decimal.Parse(bidMoney) < decimal.Parse("0.1"))
                                    {
                                        bidMoney = "0";
                                    }
                                }
                                catch (Exception)
                                {
                                    bidMoney = "0";
                                }
                            }
                        }
                        specType = "其他";
                        msgType = "深圳市深水水务咨询有限公司";
                        prjName = ToolDb.GetPrjName(prjName);
                        bidType = ToolHtml.GetInviteTypes(prjName);
                        BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }


            }
            return list;
        }
    }
}
