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
    public class BidSzXinXiXueYuan : WebSiteCrawller
    {
        public BidSzXinXiXueYuan()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "深圳信息职业技术学院中标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳信息职业技术学院中标信息";
            this.SiteUrl = "http://zbcg.sziit.edu.cn/zbxx1.htm";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "fanye66953")));
            if (nodeList != null && nodeList.Count > 0)
            {
                try
                {
                    string temp = nodeList[0].ToPlainTextString().GetRegexBegEnd("/", "&").ToLower().Replace("&nbsp;", "");
                    page = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        string  a = (page + 1 - i).ToString();
                       string url = "http://zbcg.sziit.edu.cn/zbxx1/"+a+".htm";
                        htl = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table-l")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = tableNodeList[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        if (tr.ColumnCount < 2) continue;

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
                        prjName = tr.Columns[1].ToNodePlainString();
                        if (prjName.Contains("暂停公告"))
                        {
                            continue;
                        }
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex("yyyy/MM/dd");
                        ATag aTag = tr.GetATag();
                        InfoUrl = "http://zbcg.sziit.edu.cn/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "vsb_newscontent")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            Regex regeximg = new Regex(@"<IMG[^>]*>");//去掉图片
                            HtmlTxt = regeximg.Replace(HtmlTxt, "");
                            bidCtx = dtnode.AsString().Replace("&nbsp;", "").Replace("EndFragment", "").Trim();
                            if (bidCtx.Contains("招标编号"))
                            {
                                code = bidCtx.Substring(bidCtx.IndexOf("招标编号")).ToString();
                                Regex regcode = new Regex(@"\w{4}-\w{7}");
                                code = regcode.Match(code).Value;
                            }
                            bidUnit = bidCtx.Replace("\n", "\r\n").GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.Replace("\n", "\r\n").GetBidRegex(null, false);
                            if (bidType == "设备材料" || bidType == "小型施工" || bidType == "专业分包" || bidType == "劳务分包" || bidType == "服务" || bidType == "勘察" || bidType == "设计" || bidType == "监理" || bidType == "施工")
                            {
                                specType = "建设工程";
                            }
                            else
                            {
                                specType = "其他";
                            }
                            bidType = ToolHtml.GetInviteTypes(bidType);
                            buildUnit = "";
                            msgType = "深圳信息职业技术学院";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate,
                                      bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
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
