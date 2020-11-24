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
    public class BidMeiZhou:WebSiteCrawller
    {
        public BidMeiZhou()
            : base() {
                this.Group = "中标信息";
                this.Title = "广东省梅州市建设工程中标信息（市区）";
                this.Description = "自动抓取广东省梅州市建设工程中标信息";
                this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
                this.SiteUrl = "http://market.meizhou.gov.cn/deptWebsiteAction.do?action=secondIndex&deptId=1925&issueTypeCode=009009001&issueTypeName=中标公示&showSubNodeflag=1";
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
                Regex regexHtml = new Regex(@"<script[^<]*</script>");
                htl = regexHtml.Replace(htl, "");
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("align", "right")));
            Regex regexPage = new Regex(@"共\d+页");
            try
            {
                page = int.Parse(regexPage.Match(nodeList.AsString()).Value.Trim(new char[] { '共', '页' }));
            }
            catch (Exception)
            { }
            for (int i = 1; i < page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&otype=&pageNum=" + i.ToString()), Encoding.Default);
                        Regex regexHtml = new Regex(@"<script[^<]*</script>");
                        htl = regexHtml.Replace(htl, "");
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "1")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty,
                           buildUnit = string.Empty, bidUnit = string.Empty,
                           bidMoney = string.Empty, code = string.Empty,
                           bidDate = string.Empty,
                           beginDate = string.Empty,
                           endDate = string.Empty, bidType = string.Empty,
                           specType = string.Empty, InfoUrl = string.Empty,
                           msgType = string.Empty, bidCtx = string.Empty,
                           prjAddress = string.Empty, remark = string.Empty,
                           prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[0].ToPlainTextString().Trim();
                        beginDate = tr.Columns[1].ToPlainTextString().Replace("&nbsp; ", "").Trim().Substring(0, 10);
                        ATag aTag = tr.Columns[0].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = aTag.GetAttribute("onclick").Replace("showDeptContent('1925','", "");
                        if (InfoUrl.IndexOf("'") != -1)
                        {
                            InfoUrl = InfoUrl.Remove(InfoUrl.IndexOf("'"));
                        }
                        if (InfoUrl.Contains("/website/html"))
                        {
                            InfoUrl = "http://market.meizhou.gov.cn" + InfoUrl;
                        }
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).GetJsString();
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "size13")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString().Replace("\t\r\n\t\r\n", "\t\r\n").Replace("\t\r\n\t\r\n", "\t\r\n").Replace("\t\r\n\t\r\n", "\t\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\t\r\n\t\r\n", "\t\r\n");
                            bidMoney = bidCtx.GetMoneyRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = bidCtx.GetRegexBegEnd("招标单位委托", "，");
                            }
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoNormalTable")));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag tab = tableNode[0] as TableTag;
                                    for (int r = 1; r < tab.RowCount; r++)
                                    {
                                        for (int c = 0; c < tab.Rows[r].ColumnCount; c++)
                                        {
                                            if (c > 1) continue;
                                            string temp = tab.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                            if (c == 0)
                                                ctx += temp + "：";
                                            else
                                                ctx += temp + "\r\n";
                                        }
                                    }
                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("单位名称");
                                    bidMoney = ctx.GetMoneyRegex();
                                    prjMgr = ctx.GetMgrRegex();
                                    if (prjMgr.Contains("/"))
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                                    if (prjMgr.Contains("粤"))
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("粤"));
                                    if (prjMgr.Contains("证书"))
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("证书"));
                                }
                            }
                            if (prjMgr.Contains("证书"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("证书"));
                            if (prjMgr.Contains("/"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                            if (prjMgr.Contains("（"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("（"));
                            bidType = prjName.GetInviteBidType();
                            msgType = "梅州市建设工程交易中心";
                            specType = "建设工程";
                            bidCtx = bidCtx.Replace("<?xml:namespaceprefix=o/>：", "").Trim();
                            prjName = ToolDb.GetPrjName(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "梅州市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return null;
        }
    }
}
