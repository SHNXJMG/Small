using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Util;
using System.Collections.Generic;


namespace Crawler.Instance
{
    public class BidSzldzb : WebSiteCrawller
    {
        public BidSzldzb()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "深圳龙达招标有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳龙达招标有限公司中标信息";
            this.SiteUrl = "http://www.szldzb.com/information.aspx?ClassID=25";
            this.MaxCount = 100;
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
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "digg")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                string pageTemp = tdNodes.AsString().Replace("&nbsp;", "").Trim();
                Regex regpage = new Regex(@"共\d+页");
                try
                {
                    pageInt = int.Parse(regpage.Match(pageTemp).Value.Replace("共", "").Replace("页", "").Trim());
                }
                catch (Exception ex) { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&page=" + i.ToString()), Encoding.UTF8);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("width", "100%"), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = (TableTag)nodeList[4];
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        code = table.Rows[j].Columns[0].ToPlainTextString().Trim();
                        prjName = table.Rows[j].Columns[1].ToPlainTextString().Trim();
                        beginDate = table.Rows[j].Columns[2].ToPlainTextString().GetDateRegex();
                        ATag aTag = table.Rows[j].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://www.szldzb.com/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString().Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("width", "620"), new TagNameFilter("table")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                        }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("width", "620"), new TagNameFilter("table")));

                        bidCtx = dtnode.AsString().Trim().ToLower().Replace(" ", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "").Replace("<?xml:namespaceprefix=st1/>", "").Replace("startfragment", "").Replace("endfragment", "");

                        bidUnit = bidCtx.GetBidRegex();
                        if (string.IsNullOrEmpty(bidUnit))
                            bidUnit = bidCtx.GetBidRegex(new string[] { "成交人" });
                        bidMoney = bidCtx.GetMoneyRegex(null, false, "万元整,万元");
                        string monerystr = string.Empty;
                        if (string.IsNullOrEmpty(bidUnit) && (bidMoney == "0" || string.IsNullOrEmpty(bidMoney)))
                        {
                            Parser par = new Parser(new Lexer(HtmlTxt));
                            NodeList listCon = par.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoNormalTable")));
                            if (listCon != null && listCon.Count > 0)
                            {

                                TableTag tab = listCon[0] as TableTag;
                                string txt1 = string.Empty;
                                string txt2 = string.Empty;
                                try
                                {
                                    for (int k = 0; k < 1; k++)
                                    {
                                        for (int d = 0; d < tab.Rows[k].ColumnCount; d++)
                                        {
                                            txt1 = tab.Rows[k].Columns[d].ToPlainTextString().Trim() + "：";
                                            txt2 += txt1 + tab.Rows[k + 1].Columns[d].ToPlainTextString().Trim() + "\r\n";
                                        }
                                    }
                                }
                                catch { } 
                                bidUnit = txt2.GetBidRegex();  
                                bidMoney = txt2.GetMoneyRegex();
                            }
                        }
                        if (!string.IsNullOrEmpty(bidMoney) && bidMoney != "0")
                        {
                            if (decimal.Parse(bidMoney) > 100000)
                                bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                        }
                        specType = "其他";
                        msgType = "深圳龙达招标有限公司";
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
