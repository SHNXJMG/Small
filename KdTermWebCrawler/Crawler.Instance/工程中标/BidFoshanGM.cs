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
    public class BidFoshanGM : WebSiteCrawller
    {
        public BidFoshanGM()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省佛山市高明区";
        //http://10.197.252.13:8081/test/open
            this.Description = "自动抓取广东省佛山市高明区中标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.ExistCompareFields = "Prov,City,InfoUrl";
            this.SiteUrl = "http://ztb.gaoming.gov.cn/jsgc/zbjg/";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        { 
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            int crawlMax = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default).Replace("&nbsp;", "");
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ny_21 tc")));
            if (sNode != null && sNode.Count > 0)
            {
                string pageString = sNode.AsString().Trim();
                Regex regexPage = new Regex(@"createPageHTML\([^\)]+\)");
                Match pageMatch = regexPage.Match(pageString);
                try { pageInt = int.Parse(pageMatch.Value.Replace("createPageHTML(", "").Replace(")", "").Split(',')[0].Trim()); }
                catch (Exception) { }
            }
            string cookiestr = string.Empty;

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {

                    try { html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "index_" + (i - 1).ToString() + ".html", Encoding.Default); }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(html));
                sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "ny_22"))), new TagNameFilter("li")));
                if (sNode != null && sNode.Count > 0)
                {
                    for (int j = 0; j < sNode.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt=string.Empty;
                        INode node = sNode[j];
                        ATag aTag = node.Children.SearchFor(typeof(ATag), true)[0] as ATag;
                        Div divTag = node.Children.SearchFor(typeof(Div), true)[1] as Div;
                        prjName = aTag.ToPlainTextString().Trim();
                        beginDate = divTag.ToPlainTextString().Trim(new char[] { '[', ']', ' ' });
                        InfoUrl = aTag.Link.Replace("./", "http://ztb.gaoming.gov.cn/jsgc/zbjg/");
                     
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new HasParentFilter(new AndFilter(new HasAttributeFilter("class", "con_10 tl"), new TagNameFilter("div"))));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n").Replace("<br/>", "\r\n"); }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new HasParentFilter(new AndFilter(new HasAttributeFilter("class", "con_10 tl"), new TagNameFilter("div"))));

                        if (dtnode != null && dtnode.Count > 0)
                        {
                          
                            Regex regCtx = new Regex(@"[\n]+");
                            bidCtx = regCtx.Replace(dtnode.AsString().Replace(" ", "").Trim(), "\r\n");
                            TableTag table = dtnode.SearchFor(typeof(TableTag), true)[0] as TableTag;
                            for (int dl = 0; dl < table.RowCount; dl++)
                            {
                                TableRow tr = table.Rows[dl];
                                if (tr.Columns[0].ToPlainTextString().Contains("编号"))
                                {
                                    code = tr.Columns[1].ToPlainTextString().Trim();
                                }
                                else if (tr.Columns[0].ToPlainTextString().Contains("招标单位"))
                                {
                                    buildUnit = tr.Columns[1].ToPlainTextString().Trim();
                                }
                                else if (tr.Columns[0].ToPlainTextString().Contains("中标单位"))
                                {
                                    bidUnit = tr.Columns[1].ToPlainTextString().Trim();
                                }
                                else if (tr.Columns[0].ToPlainTextString().Contains("建造师") || tr.Columns[0].ToPlainTextString().Contains("负责人") || tr.Columns[0].ToPlainTextString().Contains("法定代表人"))
                                {
                                    prjMgr = tr.Columns[1].ToPlainTextString().Replace(" ", "").Trim();
                                }
                                else if (tr.Columns[0].ToPlainTextString().Contains("中标价"))
                                {
                                    Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                                    Regex regmoneyctx = new Regex(@"[0-9]+[\%]");
                                    string bidMoneyctx = regmoneyctx.Replace(tr.Columns[1].ToPlainTextString(), "");
                                    if (!string.IsNullOrEmpty(bidMoneyctx))
                                    {

                                        if (tr.Columns[1].ToPlainTextString().Contains("万元"))
                                        {
                                            bidMoney = regBidMoney.Match(bidMoneyctx).Value;
                                        }
                                        else
                                        {
                                            try
                                            {
                                                bidMoney = (decimal.Parse(regBidMoney.Match(bidMoneyctx).Value) / 10000).ToString();
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
                                }


                            }


                        }
                        if (Encoding.Default.GetByteCount(bidUnit) > 150)
                        {
                            try
                            {
                                if (bidUnit.Contains("第二标段"))
                                {
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("\n")).Replace("第一标段", "").Replace("：", "").Replace(":", "");
                                }
                            }
                            catch { }
                        }

                        msgType = "佛山市高明区建设工程交易中心";
                        specType = "建设工程";
                        prjName = ToolDb.GetPrjName(prjName);
                        bidType = ToolHtml.GetInviteTypes(prjName);
                        BidInfo info = ToolDb.GenBidInfo("广东省", "佛山市区", "高明区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;

                    }



                }
            }
            return list;
        }

    }
}
