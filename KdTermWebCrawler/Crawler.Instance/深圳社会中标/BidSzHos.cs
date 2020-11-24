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
    public class BidSzHos : WebSiteCrawller
    {
        public BidSzHos()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Title = "深圳人民医院中标信息";
            this.Description = "自动抓取深圳人民医院中标信息";
            this.SiteUrl = "http://www.szhospital.com/02news/index02.asp?ListId=1&KeyWord=";
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
            NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "cpx12_ff6600")));
            page = Convert.ToInt32(tableNodeList[1].ToPlainTextString());
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.szhospital.com/02news/index02.asp?ListId=36&currentpage=" + i.ToString() + "&keyWord="), Encoding.Default);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList NodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "tablebg")));
                if (NodeList != null && NodeList.Count > 0)
                {
                    TableTag table = new TableTag();
                    table = (TableTag)NodeList[0];
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty,
                           code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                           bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty,
                           bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty,
                           otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[2].ToPlainTextString().Trim();
                        if (prjName.Contains("结果"))
                        {
                            ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                            InfoUrl = "http://www.szhospital.com/" + aTag.Link.Trim();

                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Trim();
                                Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                                NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "cpx12_000000")));
                                HtmlTxt = dtnodeHTML.AsHtml();
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "").Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace("<BR>", "\r\n");
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            Parser parserdetail = new Parser(new Lexer(htmldetail));
                            NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "cpx12_000000")));
                            bidCtx = dtnode.AsString();
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            bidCtx = regexHtml.Replace(bidCtx, "");
                            Regex regCode = new Regex(@"(招标编号|采购编号|采购序号)：[^\r\n]+\r\n");
                            code = regCode.Match(bidCtx).Value.Replace("招标编号：", "").Replace("采购序号：", "").Replace("采购编号：", "").Trim();
                            msgType = "深圳人民医院";
                            if (bidType == "设备材料" || bidType == "小型施工" || bidType == "专业分包" || bidType == "劳务分包" || bidType == "服务" || bidType == "勘察" || bidType == "设计" || bidType == "监理" || bidType == "施工")
                            {
                                specType = "建设工程";
                            }
                            else
                            {
                                specType = "其他";
                            }
                            beginDate = tr.Columns[3].ToPlainTextString().Replace("(", "").Replace(")", "").Trim();
                            if (endDate == "")
                            {
                                endDate = string.Empty;
                            }
                            if (code == "" && prjName.Contains("（") && prjName.Contains("）"))
                            {
                                code = prjName.Substring(prjName.IndexOf("（")).ToString().Replace("（", "").Trim();
                                code = code.Remove(code.IndexOf("）")).Trim();
                            }
                            if (Encoding.Default.GetByteCount(code) > 50)
                                code = string.Empty;
                            bidUnit = "";
                            buildUnit = "深圳人民医院";
                            prjName = ToolDb.GetPrjName(prjName);
                            bidType = ToolHtml.GetInviteTypes(bidType);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                              bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
