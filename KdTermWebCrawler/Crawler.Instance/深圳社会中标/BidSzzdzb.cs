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
    public class BidSzzdzb : WebSiteCrawller
    {
        public BidSzzdzb()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "深圳市振东招标代理有限公司";
            this.Description = "自动抓取深圳市振东招标代理有限公司中标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.SiteUrl = "http://www.szzdzb.cn/Product-index-id-11.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.szzdzb.cn/Product-index-id-11-p-" + i + ".html", Encoding.UTF8);
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
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        code = tr.Columns[0].ToPlainTextString().Trim();
                        prjName = tr.Columns[1].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://www.szzdzb.cn" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").GetJsString();

                        }
                        catch { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "hangao27"))), new TagNameFilter("table")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,<br/>", "\r\n").ToCtxString();
                            beginDate = bidCtx.GetRegex("发布时间").GetDateRegex();
                            if (bidCtx.Contains("确定中标供应商"))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList nodeTab = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "pContent"))), new TagNameFilter("table")));
                                if (nodeTab != null && nodeTab.Count > 0)
                                {
                                    TableTag tabNode = nodeTab[0] as TableTag;
                                    for (int r = 0; r < tabNode.RowCount; r++)
                                    {
                                        try
                                        {
                                            if (tabNode.Rows[r].ToNodePlainString().Contains("确定中标供应商"))
                                            {
                                                bidUnit = tabNode.Rows[r + 1].Columns[1].ToNodePlainString();
                                                bidMoney = tabNode.Rows[r+2].Columns[1].ToNodePlainString().Replace(",", "").Replace("，", "").GetMoney("万元");
                                                break;
                                            }
                                        }
                                        catch { }
                                    }
                                }
                                if (string.IsNullOrWhiteSpace(bidUnit))
                                    bidUnit = bidCtx.GetBidRegex();
                                if (bidMoney == "0" || string.IsNullOrWhiteSpace(bidMoney))
                                    bidMoney = bidCtx.Replace(",", "").Replace("，", "").GetMoneyRegex();
                            }
                            else
                            {
                                bidUnit = bidCtx.GetBidRegex(new string[] { "第一备选供应商" });
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList nodeTab = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "pContent"))), new TagNameFilter("table")));
                                if (nodeTab != null && nodeTab.Count > 0)
                                {
                                    TableTag tabNode = nodeTab[0] as TableTag;
                                    for (int r = 0; r < tabNode.RowCount; r++)
                                    {
                                        try
                                        {
                                            if (tabNode.Rows[r].ToNodePlainString().Contains(bidUnit))
                                            {
                                                bidMoney = tabNode.Rows[r].Columns[2].ToNodePlainString().Replace(",", "").Replace("，", "").GetMoney();
                                                break;
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }

                            specType = "其他";
                            msgType = "深圳市振东招标代理有限公司";
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            prjName = ToolDb.GetPrjName(prjName);
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
