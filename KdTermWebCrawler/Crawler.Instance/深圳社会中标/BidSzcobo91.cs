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
    public class BidSzcobo91 : WebSiteCrawller
    {
        public BidSzcobo91()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "中邦国际招标&邦迪工程顾问";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取中邦国际招标&邦迪工程顾问中标信息";
            this.SiteUrl = "http://www.cobo91.com/project/affiche.aspx?typeId=3";
            this.MaxCount = 20;

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
                html = this.ToolWebSite.GetHtmlByUrl("http://www.cobo91.com/project/affiche.aspx?typeId=3", Encoding.Default, ref cookiestr);
            }
            catch (Exception ex)
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "PageDataList")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                string pageTemp = tdNodes.AsString().Replace("&nbsp;", "").Replace(" ", "").Trim();
                Regex regpage = new Regex(@"共[0-9]+条");
                try
                {
                    int pageCount = int.Parse(regpage.Match(pageTemp).Value.Replace("共", "").Replace("条", "").Trim());
                    if (pageCount % 15 > 0)
                    {
                        pageInt = (pageCount / 15) + 1;
                    }
                    else
                    {
                        pageInt = pageCount / 15;
                    }
                }
                catch (Exception ex) { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { 
                            "cataId",
                            "find_yn",
                            "key_word",
                            "typeId",
                            "__EVENTARGUMENT",
                            "__EVENTTARGET",
                            "__EVENTVALIDATION",
                            "__VIEWSTATE"
                        
                        }, new string[] { 
                            "1,2,3,4,5,6,7,8,",
                            string.Empty,
                            string.Empty,
                            "1,2,3,4,5,6,7,8,",
                            string.Empty,
                            "PageDataList$ctl12$LinkButton1",
                            eventValidation,
                            viewState
                        });
                    try { html = this.ToolWebSite.GetHtmlByUrl("http://www.cobo91.com/project/affiche.aspx?typeId=3", nvc, Encoding.Default, ref cookiestr); }
                    catch (Exception ex) { continue; }

                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "StaffList"), new TagNameFilter("table")));

                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        code = tr.Columns[0].ToPlainTextString().Trim();
                        prjName = tr.Columns[1].ToPlainTextString().Trim();
                        beginDate = tr.Columns[2].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://www.cobo91.com/project/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("bgcolor", "#ffffff"), new TagNameFilter("table")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).ToLower().Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n").Replace("<p>", "\r\n").Replace("</p>", "\r\n");
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            htmldetail = regexHtml.Replace(htmldetail, "");
                        }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("bgcolor", "#ffffff"), new TagNameFilter("table")));

                        bidCtx = System.Web.HttpUtility.HtmlDecode(dtnode.AsString().Replace("【打印本页】", "").Replace("【关闭窗口】", "").Replace("版权所有：中邦国际招标&邦迪工程顾问", ""));

                        Regex regBidUnit = new Regex(@"单位(：|:)[^\r\n]+\r\n");
                        bidUnit = regBidUnit.Match(bidCtx).Value.Replace("单位", "").Replace("：", "").Replace(":", "").Trim();

                        if (bidUnit == "" || bidUnit == null)
                        {
                            bidUnit = "";
                        }
                        if (Encoding.Default.GetByteCount(bidUnit) > 150)
                        {
                            bidUnit = bidUnit.Substring(0, 150);
                        }
                        Regex regBidMoneystr = new Regex(@"(金额|价格)(：|:)[^\r\n]+\r\n");
                        string monerystr = regBidMoneystr.Match(bidCtx).Value.Replace("金额", "").Replace("价格", "").Replace("：", "").Replace(":", "").Replace(",", "").Replace("，", "").Trim();
                        Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");

                        if (!string.IsNullOrEmpty(regBidMoney.Match(monerystr).Value))
                        {

                            if ((monerystr.Contains("万元") || monerystr.Contains("万美元")))
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
                        if (string.IsNullOrEmpty(buildUnit))
                            buildUnit = "深圳市中邦国际招标有限公司";
                        specType = "其他";
                        msgType = "中邦国际招标&邦迪工程顾问";
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

