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
   public class BidNanShan : WebSiteCrawller
    {
       public BidNanShan()
           : base()
       {
           this.Group = "政府采购中标信息";
           this.Title = "广东省深圳市南山建设局建中标信息";
           this.Description = "自动抓取广东省深圳市南山建设局建中标信息";
           this.ExistCompareFields = "Code,ProjectName,InfoUrl";
           this.SiteUrl = "http://www.szns.gov.cn/publish/main/1/19/26/zbtbxx/5466/index.html";
           this.MaxCount = 50;
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
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8, ref cookiestr);
            }
            catch (Exception ex)
            { 
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hyxma03")));
            Regex regexPage = new Regex(@"共\d+页");
            try
            {
                page = int.Parse(regexPage.Match(nodeList.AsString()).Value.Trim(new char[] { '共', '页' }));
            }
            catch (Exception)
            { }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.szns.gov.cn/publish/main/1/19/26/zbtbxx/5466/index_" + i.ToString() + ".html"), Encoding.UTF8);
                    }
                    catch (Exception ex) {  }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hyxdianbeijing")));
                if (tableNodeList.Count > 0)
                {
                    for (int j = 0; j < tableNodeList.Count; j++)
                    {
                        ATag aTag = tableNodeList.SearchFor(typeof(ATag), true)[j] as ATag;
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
                        prjName = aTag.LinkText;
                        Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                        beginDate = regDate.Match(tableNodeList[j].ToPlainTextString()).Value.Trim();
                      
                        InfoUrl = "http://www.szns.gov.cn" + aTag.Link.Replace("amp;", "").Trim();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hyxzf2")));
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            Regex regeximg = new Regex(@"<img[^>]*>");//去掉图片
                            HtmlTxt = regeximg.Replace(HtmlTxt, "");
                            bidCtx = dtnode.AsString().Replace("\n", "\r\n").Replace(" ","").Trim();
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            bidCtx = regexHtml.Replace(bidCtx, "");
                            Regex regBuidUnit = new Regex(@"(招标人|建设单位)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(bidCtx).Value.Replace("招标人：", "").Replace("建设单位：", "").Trim();
                            Regex regCode = new Regex(@"工程编号(：|:)[^\r\n]+\r\n");
                            code = regCode.Match(bidCtx).Value.Replace("工程编号：", "").Trim();
                            Regex regBidUnit = new Regex(@"中标人(：|:)[^\r\n]+\r\n");
                            bidUnit = regBidUnit.Match(bidCtx).Value.Replace("中标人：", "").Trim();
                            Regex regMoney = new Regex(@"(中标价|中标价格)(：|:)[^\r\n]+\r\n");
                            bidMoney = regMoney.Match(bidCtx).Value.Replace("中标价：", "").Replace("中标价格：", "").Replace(",", "").Trim();
                            Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
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
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            if (bidUnit == "")
                            {
                                bidUnit = "";
                            }
                            Regex regprjMgr = new Regex(@"(总监|建造师|建造师（总监）)(：|:)[^\r\n]+\r\n");
                            prjMgr = regprjMgr.Match(bidCtx).Value.Replace("建造师：", "").Replace("总监：", "").Replace("建造师（总监）：", "").Trim();
                            msgType = "深圳市南山区政府采购及招标中心";
                            specType = "建设工程";
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            prjName = ToolDb.GetPrjName(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳区及街道工程", "", string.Empty, code, prjName, buildUnit, beginDate,
                                       bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                       bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
