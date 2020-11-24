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
    public class BidSzEnergy : WebSiteCrawller
    {
        public BidSzEnergy()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "广东省深圳能源集团公司中标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取广东省深圳能源集团公司中标信息";
            this.SiteUrl = "http://www.sec.com.cn/Bidding_list.aspx?TypeId=70";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 2;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch (Exception ex)
            { 
                return list;
            }
            for (int i = 1; i <= page; i++)
            {
                Parser parser = new Parser(new Lexer(htl));
                NodeList ulNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "f-l")));
                if (ulNode == null || ulNode.Count < 1) return null;
                parser = new Parser(new Lexer(ulNode[0].ToHtml()));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new TagNameFilter("ul"), true), new TagNameFilter("li")));
                if (tableNodeList.Count > 0)
                { 
                    for (int j = 0; j < tableNodeList.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty,
                              code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                              bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty,
                              bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty,
                              otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = tableNodeList[j].GetATag();
                        prjName = aTag.LinkText.ToNodeString().Replace(" ", "");
                        beginDate = prjName.GetDateRegex();
                        prjName = prjName.Replace(beginDate, "");
                        aTag = tableNodeList.SearchFor(typeof(ATag), true)[j] as ATag;
                        InfoUrl = "http://www.sec.com.cn/" + aTag.Link.Trim();

                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "g-n-con")));
                        HtmlTxt = dtnode.AsHtml();
                        Regex regCode = new Regex(@"(招 标 编 号|项目编号)：[^\r\n]+\r\n");
                        code = regCode.Match(bidCtx).Value.Replace("招 标 编 号：", "").Replace("项目编号：", "").Trim();
                        bidCtx = dtnode.AsString().Replace("\t", "").Trim()+"\r\n";
                        Regex regBuidUnit = new Regex(@"(招标人|招标单位|招 标 人)(：|:)[^\r\n]+\r\n");
                        buildUnit = regBuidUnit.Match(bidCtx).Value.Replace("招标单位：", "").Replace("招标人：", "").Replace("招 标 人：", "").Trim();
                        Regex regBidUnit = new Regex(@"(中 标 人|中标人|预中标人名称|中标人名称)(：|:)[^\r\n]+\r\n");
                        bidUnit = regBidUnit.Match(bidCtx).Value.Replace("中 标 人：", "").Replace("中标人：", "").Replace("预中标人名称：", "").Replace("中标人名称：", "").Replace("&amp;", "与").Trim();
                        Regex regMoney = new Regex(@"(中标价格|预 中 标 价|中标金额)(：|:)[^\r\n]+\r\n");
                        bidMoney = regMoney.Match(bidCtx).Value.Replace("中标价格：", "").Replace("预 中 标 价：", "").Replace("中标金额：", "").Replace(",", "").Trim();
                        if (bidMoney.Contains("￥"))
                        {
                            bidMoney = bidMoney.Substring(bidMoney.IndexOf("￥")).ToString();
                        }
                        if (bidMoney.Contains("元"))
                        {
                            bidMoney = bidMoney.Remove(bidMoney.IndexOf("元")).ToString();
                        }
                        Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                        if (bidMoney.Contains(","))
                        {
                            bidMoney = bidMoney.Replace(",", "").Trim();
                        }
                        if (bidMoney.Contains("万"))
                        {
                            bidMoney = bidMoney.Remove(bidMoney.IndexOf("万")).Trim();
                            bidMoney = regBidMoney.Match(bidMoney).Value;
                        }
                        else
                        {
                            try
                            {
                                bidMoney = (decimal.Parse(regBidMoney.Match(bidMoney).Value) / 10000).ToString();
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
                        msgType = "深圳能源集团公司";
                        specType = "建设工程";
                        prjAddress = "见中标信息";
                        if (buildUnit == "")
                        {
                            buildUnit = "";
                        }
                        if (bidUnit == "")
                        {
                            if (bidCtx.Contains("中标人"))
                            {
                                bidUnit = bidCtx.Substring(bidCtx.IndexOf("中标人")).ToString().Replace("中标人", "").Replace("：", "").Trim();
                            }
                            if (bidUnit == "")
                            {
                                if (bidCtx.Contains("中 标 人"))
                                {
                                    bidUnit = bidCtx.Substring(bidCtx.IndexOf("中 标 人")).ToString().Replace("中 标 人", "").Replace("：", "").Trim();
                                }
                            }
                        }
                        if (bidUnit == "")
                        {
                            bidUnit = "";
                        }
                        if (Encoding.Default.GetByteCount(bidUnit) > 150)
                            bidUnit = "";
                        prjName = ToolDb.GetPrjName(prjName);
                        bidType = ToolHtml.GetInviteTypes(prjName);
                        BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit,
                            beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
