using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using System.Text.RegularExpressions;
using System.Collections.Specialized;


namespace Crawler.Instance
{    /// <summary>
    /// 内蒙古 自治区建设工程信息
    /// </summary>
    public class BidNeimeng : WebSiteCrawller
    {
        public BidNeimeng()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "内蒙古自治区中标信息";
            this.PlanTime = "9:40,11:40,14:40,15:30,16:40,18:40";
            this.Description = "自动抓取内蒙古自治区中标信息";
            this.SiteUrl = "http://www.nmgztb.com/Html/gongchengxinxi/zhongbiaogongshi/index.htm";
            this.MaxCount = 20;
            this.ExistCompareFields = "InfoUrl";
        }


        protected override IList ExecuteCrawl(bool crawlAll)
        { 
            IList list = new List<BidInfo>();
            //取得页码
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return list;
            }
            int pageInt = 1;
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "totalpage")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    pageInt = Convert.ToInt32(pageNode[0].ToNodePlainString());
                }
                catch { }
            }
            for (int i = pageInt; i >= 1; i--)
            {
                if (i < pageInt)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.nmgztb.com/Html/gongchengxinxi/zhongbiaogongshi/index_" + i + ".htm", Encoding.Default);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList sNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                if (sNodes != null && sNodes.Count > 0)
                {
                    TableTag table = sNodes[0] as TableTag;
                    for (int t = 0; t < table.RowCount; t++)
                    {
                        if (table.Rows[t].ColumnCount < 2)
                            continue;
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
                         bidDate = string.Empty, beginDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, HtmlTxt = string.Empty, strHtml = string.Empty;
                        StringBuilder ctx = new StringBuilder();
                        TableRow tr = table.Rows[t] as TableRow;
                        NodeList nodeList = tr.SearchFor(typeof(ATag), true);
                        if (nodeList.Count > 0)
                        {
                            ATag aTag = nodeList[0] as ATag;
                            InfoUrl = "http://www.nmgztb.com" + aTag.Link;

                            prjName = aTag.GetAttribute("title");
                            string htmldtl = string.Empty, dtlStr = string.Empty;
                            try
                            {
                                dtlStr = ToolHtml.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                htmldtl = dtlStr.ToLower();
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            htmldtl = regexHtml.Replace(htmldtl, "");
                            Parser parserdtl = new Parser(new Lexer(htmldtl));
                            NodeList nodesDtl = parserdtl.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "link_con_con")));
                            if (nodesDtl != null && nodesDtl.Count > 0)
                            {
                                Regex regex = new Regex(@"更新时间：\d{4}年\d{1,2}月\d{1,2}日");
                                Match math = regex.Match(nodesDtl.AsString());
                                if (math != null)
                                {
                                    beginDate = math.Value.Replace("更新时间：", "").Replace("年", "-").Replace("月", "-").Replace("日", "").Trim();
                                }
                            }
                            parserdtl.Reset();
                            nodesDtl = parserdtl.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "oo")));
                            HtmlTxt = nodesDtl.AsHtml();
                            Parser par = new Parser(new Lexer(dtlStr));
                            NodeList htmlList = par.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "oo")));
                            strHtml = htmlList.AsHtml();
                            string str = nodesDtl.AsString().Replace("&nbsp;", "").Replace(" ", "");
                            Regex regexCTX = new Regex(@"作者：[^更新时间]+更新时间：\d{4}年\d{1,2}月\d{1,2}日");
                            str = str.Replace(regexCTX.Match(str).Value, "");
                            if (str.IndexOf("上一篇：") > -1)
                            {
                                ctx.Append(str.Substring(0, str.IndexOf("上一篇：")));
                            }
                            else
                            {
                                ctx.Append(str);
                            }
                            if (ctx.ToString().Contains("招标人：") || ctx.ToString().Contains("招标单位：") || ctx.ToString().Contains("招标采购单位："))
                            {
                                Regex regex = new Regex("(招标人|招标单位|招标采购单位)：[^\r\n]+[\r\n]{1}");
                                Match match = regex.Match(ctx.ToString());
                                buildUnit = match.Value.Substring(match.Value.IndexOf("：") + 1).Trim();
                                buildUnit = buildUnit.Replace("&ldquo;", "").Replace("&rdquo;", "");
                            }
                            if (ctx.ToString().Contains("预中标人："))
                            {
                                try
                                {
                                    Regex regex = new Regex("(预中标人)：[^\r\n]+[\r\n]{1}");
                                    MatchCollection match = regex.Matches(ctx.ToString());
                                    bidUnit = match[0].Value.Substring(match[0].Value.IndexOf("：") + 1).Trim();
                                }
                                catch { }
                            }
                            if (ctx.ToString().Contains("第一中标候选人："))
                            {
                                try
                                {
                                    Regex regex = new Regex("(第一中标候选人)：[^\r\n]+[\r\n]{1}");
                                    MatchCollection match = regex.Matches(ctx.ToString());
                                    bidUnit = match[0].Value.Substring(match[0].Value.IndexOf("：") + 1).Trim();
                                }
                                catch { }
                            }
                            if (ctx.ToString().Contains("中标候选人公示"))
                            {
                                try
                                {
                                    Regex regex = new Regex("(第一名)：[^\r\n]+[\r\n]{1}");
                                    MatchCollection match = regex.Matches(ctx.ToString());
                                    bidUnit = match[0].Value.Substring(match[0].Value.IndexOf("：") + 1).Trim();
                                }
                                catch { }
                            }
                            Regex regMon = new Regex(@"(中标价|价格|金额)(：|:)[^\r\n]+[\r\n]{1}");
                            string monerystr = regMon.Match(ctx.ToString()).Value.Replace("中标价", "").Replace("价格", "").Replace("金额", "").Replace("：", "").Replace(":", "").Trim();
                            Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");

                            if (!string.IsNullOrEmpty(regBidMoney.Match(monerystr).Value))
                            {

                                if (monerystr.Contains("万元") || monerystr.Contains("万美元"))
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
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                string nodeCon = string.Empty;
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList nodeCtx = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "msonormaltable")));
                                if (nodeCtx != null && nodeCtx.Count > 0)
                                {
                                    TableTag tabCtx = nodeCtx[0] as TableTag;
                                    if (tabCtx.RowCount > 1)
                                    {
                                        for (int k = 0; k < tabCtx.Rows[0].ColumnCount; k++)
                                        {
                                            nodeCon += tabCtx.Rows[0].Columns[k].ToNodePlainString();
                                            nodeCon += "：" + tabCtx.Rows[1].Columns[k].ToNodePlainString() + "\r\n";
                                        }
                                    }
                                }
                                bidUnit = nodeCon.GetBidRegex().Replace("第一名", "");
                                bidMoney = nodeCon.GetMoneyRegex();
                                if (bidMoney == "0")
                                    bidMoney = nodeCon.GetRegex("投标报价（元）").GetMoney();
                            }
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "";
                            }
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                bidUnit = "";
                            }
                            if (Encoding.Default.GetByteCount(buildUnit) > 150)
                                buildUnit = string.Empty;
                            if (Encoding.Default.GetByteCount(bidUnit) > 150)
                                bidUnit = string.Empty;
                            if (Encoding.Default.GetByteCount(code) > 50)
                                code = string.Empty;

                            prjName = ToolDb.GetPrjName(prjName);
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            try
                            {
                                BidInfo info = ToolDb.GenBidInfo("内蒙古自治区", "内蒙古自治区及盟市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, string.Empty, ctx.ToString(), string.Empty, "内蒙古自治区建设工程招标投标服务中心", bidType, "建设工程", string.Empty, bidMoney, InfoUrl, string.Empty, HtmlTxt);
                                //ToolDb.SaveEntity(info, this.ExistCompareFields);
                                list.Add(info);
                            }
                            catch { Logger.Error(prjName); }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
