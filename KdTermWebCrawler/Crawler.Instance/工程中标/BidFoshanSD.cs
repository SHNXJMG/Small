using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Util;

namespace Crawler.Instance
{
    public class BidFoshanSD : WebSiteCrawller
    {
        public BidFoshanSD()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省佛山市顺德区";
            this.Description = "自动抓取广东省佛山市顺德区中标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.SiteUrl = "http://www.sdcin.com.cn/page.php?singleid=3&ClassID=5";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&page=0"), Encoding.Default).Replace("&nbsp;", "");
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("id", "page_PageList")));
            if (sNode != null && sNode.Count > 0)
            {
                SelectTag select = sNode[0] as SelectTag;
                pageInt = select.OptionTags.Length;
            }

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try { html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "&page=" + (i - 1).ToString(), Encoding.Default); }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(html));
                sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("onmouseover", "this.style.backgroundColor=\"#EFFCD0\";")));
                if (sNode != null && sNode.Count > 0)
                {
                    for (int n = 0; n < sNode.Count; n++)
                    {

                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = sNode[n] as TableRow;
                        prjName = tr.Columns[0].ToPlainTextString().Trim();
                        bidUnit = tr.Columns[1].ToPlainTextString().Trim(); 

                        ATag aTag = tr.Columns[0].SearchFor(typeof(ATag), true)[0] as ATag;
                        Regex regexLink = new Regex(@"id=[^-]+");
                        InfoUrl = "http://www.sdcin.com.cn/viewzbgg.php?" + regexLink.Match(aTag.Link).Value;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").GetJsString(); 
                        }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "98%")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            string ctx = string.Empty;
                            TableTag table = dtnode[0] as TableTag;
                            for (int k = 0; k < table.RowCount; k++)
                            {
                                for (int d = 0; d < table.Rows[k].ColumnCount; d++)
                                {
                                    if (d == 0)
                                        ctx += table.Rows[k].Columns[d].ToNodePlainString().Replace("：","").Replace(":","") + "：";
                                    else
                                        ctx += table.Rows[k].Columns[d].ToNodePlainString() + "\r\n";
                                }
                            }
                            bidCtx = ctx;
                            buildUnit = bidCtx.GetBuildRegex();
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = bidCtx.GetRegex("招标代理");
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            if (prjAddress.Contains("邮政编码"))
                            {
                                prjAddress = prjAddress.Remove(prjAddress.IndexOf("邮政编码"));
                            }
                            if (string.IsNullOrEmpty(beginDate))
                                beginDate = DateTime.Now.ToString("yyyy-MM-dd");
                            prjMgr = bidCtx.GetMgrRegex();
                            code = bidCtx.GetCodeRegex(); 
                            msgType = "佛山市顺德区建设工程交易中心";
                            specType = "建设工程";
                            prjName = ToolDb.GetPrjName(prjName);
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "佛山市区", "顺德区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
