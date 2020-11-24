using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;

namespace Crawler.Instance
{
    /// <summary>
    /// 广州市番禺区
    /// </summary>
    public class BidGzPanyuSmall : WebSiteCrawller
    {
        public BidGzPanyuSmall()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省广州市番禺区小规模项目";
            this.Description = "自动抓取广东省广州市番禺区中标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.SiteUrl = "http://218.20.201.20/www/zbmsg/2008/xzb_list.asp?bc=小型项目招标&sc=中标公示";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellspacing", "5")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                NodeList aNodes = new NodeList();
                tdNodes[0].CollectInto(aNodes, new TagNameFilter("a"));
                if (aNodes != null && aNodes.Count > 0)
                {
                    for (int i = 0; i < aNodes.Count; i++)
                    {
                        ATag aTag = aNodes[i] as ATag;
                        if (aTag.ToPlainTextString().Contains("尾页"))
                        {
                            Regex re = new Regex(@"[^0-9]+");
                            pageInt = int.Parse(re.Replace(aTag.Link, ""));
                            break;
                        }
                    }
                }
            }
            parser.Reset();
            for (int i = 1; i <= pageInt; i++)
            {
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://218.20.201.20/www/zbmsg/2008/xzb_list.asp?page=" + i.ToString() + "&id=13828"), Encoding.Default);
                }
                catch (Exception ex)
                {
                    continue;
                }

                parser = new Parser(new Lexer(html));
                tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("a"), new HasParentFilter(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "news_list")), true)));
                if (tdNodes != null && tdNodes.Count > 0)
                {
                    for (int j = 0; j < tdNodes.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, buildScale = string.Empty, buildCycle = string.Empty,
                        levels = string.Empty, structType = string.Empty, bidMoney = string.Empty, buildType = string.Empty, buildQual = string.Empty, InfoUrl = string.Empty, beginDate = string.Empty, bidType = string.Empty, HtmlTxt = string.Empty;
                        decimal decMoney = 0;
                        StringBuilder ctx = new StringBuilder();
                        ATag aTag = tdNodes[j] as ATag;
                        if (aTag.Link.Contains("xzb_show.asp"))
                        {
                            InfoUrl = "http://218.20.201.20/www/zbmsg/2008/" + aTag.Link.Remove(aTag.Link.IndexOf("&"));
                            Regex regexHtml = new Regex(@"<div[^>]*>[\s]*</div>");
                            string dlHtml = string.Empty;
                            try
                            {
                                dlHtml = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).ToLower().Replace("&nbsp;", "");
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                            string filterHtml = dlHtml.Replace("\n", "").Replace("\r", "").Replace("<u>", "<a>").Replace("</u>", "</a>");
                            prjName = aTag.ToPlainTextString();
                             
                            //内容
                            Parser ctxParser = new Parser(new Lexer(dlHtml));
                            NodeList ctxNodes = ctxParser.ExtractAllNodesThatMatch(new HasParentFilter(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "news_show")), false));

                            ctx.Append(ctxNodes.AsString().Replace("&nbsp;", ""));
                            HtmlTxt = ctxNodes.AsHtml();
                            Parser dlParser = new Parser(new Lexer(regexHtml.Replace(filterHtml, "")));
                            NodeList dlNodes = dlParser.ExtractAllNodesThatMatch(new HasParentFilter(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "news_show")), false));


                            //搜索附件
                            NodeList findFiles = dlNodes.ExtractAllNodesThatMatch(new TagNameFilter("a"), true);
                            NodeList fileNode = new NodeList();
                            if (findFiles != null && findFiles.Count > 0)
                            {
                                for (int f = 0; f < findFiles.Count; f++)
                                {
                                    ATag fileA = findFiles[f] as ATag;
                                    if (fileA.Link.Contains("uploadfile"))
                                    {
                                        fileNode.Add(fileA);
                                    }
                                }
                            }
                            INode nods = dlNodes[0].Parent.Parent.Parent.Parent;
                            //发布日期
                            if (nods != null)
                            {
                                TableTag tb = nods as TableTag;
                                for (int t = 0; t < tb.RowCount; t++)
                                {
                                    TableRow tr = tb.Rows[t];
                                    if (tr.ToPlainTextString().Contains("发布日期"))
                                    {
                                        beginDate = tr.ToPlainTextString().Substring(tr.ToPlainTextString().IndexOf("[") + 1, tr.ToPlainTextString().IndexOf("]") - tr.ToPlainTextString().IndexOf("[") - 1);
                                        break;
                                    }
                                }
                            }
                            for (int k = 0; k < dlNodes.Count; k++)
                            {
                                if (dlNodes[k] is ITag)
                                {
                                    //对a标签进行过滤
                                    Regex strReplace = new Regex(@"<a[^>]*>|</a>");
                                    if (dlNodes[k].ToPlainTextString().Contains("中标候选人为：") || dlNodes[k].ToPlainTextString().Contains("中标人为："))
                                    {
                                        NodeList bidUnitNode = new NodeList();
                                        dlNodes[k].CollectInto(bidUnitNode, new TagNameFilter("a"));
                                        if (bidUnitNode.Count > 0)
                                        {
                                            //找出匹配的项
                                            Regex regexbidUnit = new Regex(@"<a[^>]*>[^<]*</a>");
                                            MatchCollection matchbidUnit = null;
                                            if (dlNodes[k].ToPlainTextString().Contains("中标候选人为："))
                                            {
                                                matchbidUnit = regexbidUnit.Matches(dlNodes[k].ToHtml().Substring(dlNodes[k].ToHtml().IndexOf("中标候选人为：")));
                                            }
                                            else if (dlNodes[k].ToPlainTextString().Contains("中标人为："))
                                            {
                                                matchbidUnit = regexbidUnit.Matches(dlNodes[k].ToHtml().Substring(dlNodes[k].ToHtml().IndexOf("中标人为：")));
                                            }
                                            if (matchbidUnit != null && matchbidUnit.Count > 0)
                                            {
                                                bidUnit = strReplace.Replace(matchbidUnit[0].ToString(), "");
                                            }
                                            if (string.IsNullOrEmpty(bidUnit))
                                            {
                                                bidUnit = dlNodes[k + 1].ToPlainTextString().Trim();
                                            }
                                        }
                                        else
                                        {
                                            bidUnit = dlNodes[k + 1].ToPlainTextString();
                                        }
                                    }
                                    if (dlNodes[k].ToPlainTextString().Contains("中标价：") || dlNodes[k].ToPlainTextString().Contains("投标报价：") || dlNodes[k].ToPlainTextString().Contains("中标价:") || dlNodes[k].ToPlainTextString().Contains("中标价为"))
                                    {
                                        Regex regdecimal = new Regex(@"\d{1,}[\.]?\d{0,}");
                                        NodeList moneyNode = new NodeList();
                                        dlNodes[k].CollectInto(moneyNode, new TagNameFilter("a"));
                                        if (moneyNode.Count > 0)
                                        {
                                            Regex regexmoney = new Regex(@"<a[^>]*>[^<]*</a>");
                                            MatchCollection matchmoney = null;
                                            if (dlNodes[k].ToPlainTextString().Contains("中标价："))
                                            {
                                                matchmoney = regexmoney.Matches(dlNodes[k].ToHtml().Substring(dlNodes[k].ToHtml().IndexOf("中标价：")));
                                            }
                                            if (dlNodes[k].ToPlainTextString().Contains("投标报价："))
                                            {
                                                matchmoney = regexmoney.Matches(dlNodes[k].ToHtml().Substring(dlNodes[k].ToHtml().IndexOf("投标报价：")));
                                            }
                                            if (matchmoney != null && matchmoney.Count > 0)
                                            {
                                                if (dlNodes[k].ToPlainTextString().Contains("万元"))
                                                {
                                                    try
                                                    {
                                                        decMoney = decimal.Parse(regdecimal.Matches(dlNodes[k].ToPlainTextString())[0].ToString());
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                    }
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        decMoney = decimal.Parse(regdecimal.Matches(dlNodes[k].ToPlainTextString())[0].ToString()) / 10000;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (dlNodes[k].ToPlainTextString().Contains("万元"))
                                            {
                                                decMoney = decimal.Parse(regdecimal.Matches(dlNodes[k].ToPlainTextString().ToString())[0].ToString());
                                            }
                                            else
                                            {
                                                decMoney = decimal.Parse(regdecimal.Matches(dlNodes[k].ToPlainTextString().ToString())[0].ToString()) / 10000;
                                            }
                                        }
                                    }
                                }
                            }
                            string regexstr = @"<[^>]*>";
                            string ctxStr = Regex.Replace(ctx.ToString(), regexstr, string.Empty, RegexOptions.IgnoreCase);
                            bidUnit = bidUnit.Replace(" ", "").Trim();
                            Regex reg = new Regex(@"[\u4e00-\u9fa5]");
                            if (!reg.IsMatch(bidUnit))
                            {
                                bidUnit = "";
                            }
                            else
                            {
                                Regex regBidMoneys = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                                string t = regBidMoneys.Match(bidUnit).Value;
                                if (!string.IsNullOrEmpty(t)) { bidUnit = ""; }
                            }
                            if (string.IsNullOrEmpty(bidUnit) || decMoney <= 0)
                            {
                                string txt = string.Empty;
                                parser = new Parser(new Lexer(dlHtml));
                                NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasParentFilter(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "news_show")), true)));
                                if (dtList != null && dtList.Count > 1)
                                {
                                    for (int k = 0; k < dtList.Count; k++)
                                    {
                                        if (dtList[k].ToPlainTextString().Trim().Contains("中标候选人") || dtList[k].ToPlainTextString().Trim().Contains("中标人"))
                                        {
                                            try
                                            {
                                                if (string.IsNullOrEmpty(dtList[k + 1].ToPlainTextString().Trim()))
                                                {
                                                    txt += dtList[k].ToPlainTextString().Trim();
                                                    string text = txt.Remove(txt.Length - txt.IndexOf("为：") - 2);
                                                    if (string.IsNullOrEmpty(text))
                                                    {
                                                        txt += dtList[k].ToPlainTextString().Trim();
                                                        txt += dtList[k + 2].ToPlainTextString().Trim() + "\r\n";
                                                    }
                                                    else
                                                    {
                                                        txt += dtList[k].ToPlainTextString().Trim() + "\r\n";
                                                    }
                                                }
                                                else
                                                {
                                                    txt += dtList[k].ToPlainTextString().Trim();
                                                    string text = txt.Remove(txt.Length - txt.IndexOf("为：") - 2);
                                                    if (string.IsNullOrEmpty(text))
                                                    {
                                                        txt += dtList[k].ToPlainTextString().Trim();
                                                        txt += dtList[k + 1].ToPlainTextString().Trim() + "\r\n";
                                                    }
                                                    else
                                                    {
                                                        txt += dtList[k].ToPlainTextString().Trim() + "\r\n";
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            txt += dtList[k].ToPlainTextString().Trim() + "\r\n";
                                        }
                                    }
                                    if (string.IsNullOrEmpty(bidUnit))
                                    {
                                        Regex regBidUnit = new Regex(@"(中标单位|中标候选单位|中标候选人为|中标人为)：[^\r\n]+\r\n");
                                        bidUnit = regBidUnit.Match(txt.Replace("\r\n\r\n", "")).Value.Replace("中标候选人为", "").Replace("中标人为", "").Replace("中标单位：", "").Replace("中标候选单位：", "").Replace("：", "").Trim();
                                    }
                                    if (decMoney <= 0)
                                    {
                                        Regex regBidMoneystr = new Regex(@"(金额|价格|报价|中标价)(：|:)[^\r\n]+\r\n");
                                        string monerystr = regBidMoneystr.Match(txt).Value.Replace("中标价", "").Replace("金额", "").Replace("价格", "").Replace("报价", "").Replace("：", "").Replace(":", "").Replace(",", "").Replace("，", "").Trim();
                                        Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                                        if (!string.IsNullOrEmpty(regBidMoney.Match(monerystr).Value))
                                        {
                                            if (monerystr.Contains("万元") || monerystr.Contains("万美元"))
                                            {
                                                decMoney = decimal.Parse(regBidMoney.Match(monerystr).Value);
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    decMoney = decimal.Parse(regBidMoney.Match(monerystr).Value) / 10000;
                                                    if (decMoney < decimal.Parse("0.1"))
                                                    {
                                                        decMoney = 0;
                                                    }
                                                }
                                                catch (Exception)
                                                {
                                                    decMoney = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(bidUnit) || decMoney <= 0)
                            {
                                string txt = string.Empty;
                                parser = new Parser(new Lexer(dlHtml));
                                NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("p"), new HasParentFilter(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "news_show")), true)));
                                if (dtList != null && dtList.Count > 1)
                                {
                                    for (int k = 0; k < dtList.Count; k++)
                                    {
                                        if (dtList[k].ToPlainTextString().Trim().Contains("中标候选人") || dtList[k].ToPlainTextString().Trim().Contains("中标人"))
                                        {
                                            if (string.IsNullOrEmpty(dtList[k + 1].ToPlainTextString().Trim()))
                                            {
                                                k++;
                                                txt += dtList[k].ToPlainTextString().Trim();
                                            }
                                            else
                                            {
                                                txt += dtList[k].ToPlainTextString().Trim();
                                                string text = txt.Remove(txt.Length - txt.IndexOf("为：") - 2);
                                                if (string.IsNullOrEmpty(text))
                                                {
                                                    txt = "";
                                                    txt += dtList[k].ToPlainTextString().Trim();
                                                }
                                                else
                                                {
                                                    txt = "";
                                                    txt += dtList[k].ToPlainTextString().Trim() + "\r\n";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            txt += dtList[k].ToPlainTextString().Trim() + "\r\n";
                                        }
                                        Regex regexsHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                                        txt = regexsHtml.Replace(txt, "");
                                    }
                                    if (string.IsNullOrEmpty(bidUnit))
                                    {
                                        Regex regBidUnit = new Regex(@"(中标单位|中标候选单位|中标候选人为|中标人为)：[^\r\n]+\r\n");
                                        bidUnit = regBidUnit.Match(txt.Replace("\r\n\r\n", "")).Value.Replace("中标候选人为", "").Replace("中标人为", "").Replace("中标单位：", "").Replace("中标候选单位：", "").Replace("：", "").Trim();
                                    }
                                    if (string.IsNullOrEmpty(bidMoney))
                                    {
                                        Regex regBidMoneystr = new Regex(@"(金额|价格|报价|中标价|中标价为)(：|:)[^\r\n]+\r\n");
                                        string monerystr = regBidMoneystr.Match(txt).Value.Replace("中标价为", "").Replace("中标价", "").Replace("金额", "").Replace("价格", "").Replace("报价", "").Replace("：", "").Replace(":", "").Replace(",", "").Replace("，", "").Trim();
                                        Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                                        if (!string.IsNullOrEmpty(regBidMoney.Match(monerystr).Value))
                                        {
                                            if (monerystr.Contains("万元") || monerystr.Contains("万美元"))
                                            {
                                                decMoney = decimal.Parse(regBidMoney.Match(monerystr).Value);
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    decMoney = decimal.Parse(regBidMoney.Match(monerystr).Value) / 10000;
                                                    if (decMoney < decimal.Parse("0.1"))
                                                    {
                                                        decMoney = 0;
                                                    }
                                                }
                                                catch (Exception)
                                                {
                                                    decMoney = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                             
                            prjName = ToolDb.GetPrjName(prjName.Replace(" ", ""));
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "广州市区", "番禺区", string.Empty, string.Empty, prjName, buildUnit, beginDate, bidUnit, beginDate, string.Empty, ctxStr, string.Empty, "广州市番禺区建设局", bidType, "建设工程", string.Empty, decMoney.ToString(), InfoUrl, string.Empty, HtmlTxt);

                            list.Add(info);
                            if (fileNode.Count > 0)
                            {
                                try
                                {
                                    for (int f = 0; f < fileNode.Count; f++)
                                    {
                                        BaseAttach attach = ToolDb.GenBaseAttach((fileNode[0] as ATag).StringText, info.Id, "http://218.20.201.20" + (fileNode[0] as ATag).Link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                                catch { }
                            }
                            dlParser.Reset();
                            if (!crawlAll && list.Count >= this.MaxCount)
                                return list;
                        }

                    }
                }

            }

            return list;
        }


    }
}
