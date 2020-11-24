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
    public class BidSzCmc : WebSiteCrawller
    {
        public BidSzCmc()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "中国机械进出口（集团）有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取中国机械进出口（集团）有限公司中标信息";
            this.SiteUrl = "http://zb.cmc.com.cn/TenderPublicity/";
            this.MaxCount = 100;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            { 
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8); 
            }
            catch (Exception ex)
            { 
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("style", "float:left;")));
            if (tdNodes != null&&tdNodes.Count>0)
            { 
                try
                {
                    string pageTemp = tdNodes[0].ToPlainTextString().Replace("&nbsp;", "").Trim();
                    Regex regpage = new Regex(@"共\d+页");
                    //string ss = regpage.Match(pageTemp).Value.Replace("页", "").Replace("共", "").Trim();
                    pageInt = int.Parse(regpage.Match(pageTemp).Value.Replace("共", "").Replace("页", "").Trim());
                }
                catch (Exception ex) { }

                string cookiestr = string.Empty;
                for (int i = 1; i <= pageInt; i++)
                {

                    if (i > 1)
                    {
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "?page=" + i.ToString()), Encoding.UTF8);
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "tb_list")));
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        TableTag table = nodeList[0] as TableTag;
                        for (int j = 1; j < table.RowCount; j++)
                        {
                            string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt=string.Empty;
                            TableRow tr = table.Rows[j]; 
                            prjName = tr.Columns[1].ToPlainTextString().Trim().Replace(" ", "");
                            ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                            InfoUrl = "http://zb.cmc.com.cn/TenderPublicity/" + aTag.Link;
                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = ToolHtml.GetHtmlByUrl(SiteUrl,InfoUrl).Replace("&nbsp;", "").Trim();
                                Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                                NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "ct"), new TagNameFilter("div")));
                                HtmlTxt = dtnodeHTML.AsHtml();
                                htmldetail = ToolHtml.GetHtmlByUrl(SiteUrl, InfoUrl).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                                Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                                htmldetail = regexHtml.Replace(htmldetail, "");
                            }
                            catch (Exception ex) { continue; }
                            parser = new Parser(new Lexer(htmldetail));
                            NodeList ldata = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tm")));
                            if (ldata != null && ldata.Count > 0)
                            {
                                 string datactx = ldata.AsString();
                                 Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                                 beginDate = regDate.Match(datactx).Value;
                            }
                            if (string.IsNullOrEmpty(beginDate))
                            {
                                beginDate = DateTime.Now.ToString();
                            }
                            Parser dtlparser = new Parser(new Lexer(htmldetail));

                            NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "ct"), new TagNameFilter("div")));
                            bidCtx = dtnode.AsString().Trim().Replace(" ", "");
                            Regex regCtx = new Regex(@"([\r\n]+)|([\t]+)|(\[该信息共被浏览了[0-9]+次\]\[关闭\])");
                            bidCtx = regCtx.Replace(bidCtx, "\r\n") + "\r\n";
                             
                            Regex regBidUnit = new Regex(@"(成交供应商|中标人名称|中标单位|中标人)(：|:)[^\r\n]+\r\n");
                            bidUnit = regBidUnit.Match(bidCtx).Value.Replace("成交供应商", "").Replace("中标单位", "").Replace("中标人名称", "").Replace("成交", "").Replace("中标人","").Replace("：", "").Replace(":", "").Trim();
                            Regex regcode = new Regex(@"编号(：|:)[^\r\n]+\r\n");
                            code = regcode.Match(bidCtx).Value.Replace("编号", "").Replace("：", "").Replace(":", "").Trim();
                            if (Encoding.Default.GetByteCount(code) > 50)
                            {
                                code = "";
                            }

                            Regex regBidMoneystr = new Regex(@"金额(：|:)[^\r\n]+\r\n");
                            string monerystr = regBidMoneystr.Match(bidCtx).Value.Replace("金额", "").Replace(":", "").Replace("：", "").Replace(",", "").Replace("，", "").Trim();
                            Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                            if (string.IsNullOrEmpty(regBidMoney.Match(monerystr).Value))
                            {
                                Regex regBidMoneystr1 = new Regex(@"小写为(：|:)[^\r\n]+\r\n");
                                monerystr = regBidMoneystr1.Match(bidCtx).Value.Replace("小写为", "").Replace(":", "").Replace("：", "").Replace(",", "").Replace("，", "").Trim();
                            }
                            if (string.IsNullOrEmpty(regBidMoney.Match(monerystr).Value))
                            {
                                Regex regBidMoneystr1 = new Regex(@"(￥|$)[^\r\n]+\r\n");
                                monerystr = regBidMoneystr1.Match(bidCtx).Value.Replace("￥", "").Replace("$", "").Replace(":", "").Replace("：", "").Replace(",", "").Replace("，", "").Trim();
                            }
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
                            msgType = "中国机械进出口（集团）有限公司";
                            prjName = ToolDb.GetPrjName(prjName);
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
