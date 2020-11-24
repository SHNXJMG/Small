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
   public class BidYunFu : WebSiteCrawller
    {
       public BidYunFu()
           : base()   
       {
           this.Group = "中标信息";
           this.Title = "广东省云浮工程建设中标信息";
           this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
           this.Description = "自动抓取广东省云浮工程建设中标信息";
           this.SiteUrl = "http://gcjs.yunfu.gov.cn/gcjs/xmnews.jsp?columnid=014002002002";
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
            NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "fanyie")));
            if (tableNodeList != null && tableNodeList.Count > 0)
            {
                try
                {
                    Regex regexPage = new Regex(@"共\d+页");
                    page = int.Parse(regexPage.Match(tableNodeList.AsString()).Value.Trim(new char[] { '共', '页' }));
                }
                catch { }
            }
            for (int j = 1; j <= page; j++)
            {
                if (j > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&ipage=" + j.ToString()), Encoding.Default);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "list_headnews")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        ATag aTag = nodeList.SearchFor(typeof(ATag), true)[i] as ATag;
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
                        prjName = nodeList[i].ToPlainTextString().Replace(" ", "");
                        InfoUrl = "http://gcjs.yunfu.gov.cn" + aTag.Link;
                        Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                        beginDate = regDate.Match(prjName).Value.Trim();
                        if (!string.IsNullOrEmpty(beginDate))
                        { 
                            prjName = prjName.Replace(beginDate, "").Trim();
                        } 
                        if (prjName.Contains("招标公告") || prjName.Contains("补充公告"))
                        {
                            continue;
                        }
                         
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            Logger.Error("BidYunFu"); 
                            continue;
                        }
                        string htm = string.Empty;
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "STYLE10")));
                        if (dtnode != null &&dtnode.Count > 0  )
                        {
                            htm = dtnode[0].ToHtml();
                            bidCtx = dtnode.AsString().Replace("\n", "\r\n");
                            HtmlTxt = dtnode.AsHtml();
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            bidCtx = regexHtml.Replace(bidCtx, "");
                            if (bidCtx.Contains("第一中标候选人"))
                            {
                                try
                                {
                                    string ctx = string.Empty;
                                    ctx = bidCtx.Substring(bidCtx.IndexOf("第一中标候选人")).ToString().Replace("\r\n", "").Replace("，", "\r\n").Replace("；", "\r\n");
                                    Regex regBidUnit = new Regex(@"第一中标候选人(：|:)[^\r\n]+\r\n");
                                    bidUnit = regBidUnit.Match(ctx).Value.Replace("第一中标候选人：", "").Replace("第一中标候选人: ", "").Trim();
                                    Regex regMoney = new Regex(@"(中标价|投标价|投标报价)(：|:|)[^\r\n]+\r\n");
                                    bidMoney = regMoney.Match(ctx).Value.Replace("中标价：", "").Replace("投标报价", "").Replace("投标价", "").Replace(",", "").Trim();
                                    Regex regPrjMgr = new Regex(@"(项目总监|项目负责人|项目经理姓名及资质证书编号)(：|:)[^\r\n]+\r\n");
                                    prjMgr = regPrjMgr.Match(ctx).Value.Replace("项目总监：", "").Replace("项目负责人：", "").Replace("项目经理姓名及资质证书编号：", "").Trim();
                                    if (prjMgr.Contains("（"))
                                    {
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("（")).ToString();
                                    }
                                }
                                catch
                                {
                                    
                                }
                            }
                            if (bidCtx.Contains("第一候选人"))
                            {
                                try
                                {
                                    string ctx = string.Empty;
                                    ctx = bidCtx.Substring(bidCtx.IndexOf("第一候选人")).ToString().Replace("\r\n", "").Replace("，", "\r\n").Replace("；", "\r\n");
                                    Regex regBidUnit = new Regex(@"第一候选人(：|:)[^\r\n]+\r\n");
                                    bidUnit = regBidUnit.Match(ctx).Value.Replace("第一候选人：", "").Replace("第一候选人: ", "").Trim();
                                    Regex regMoney = new Regex(@"(中标价|投标价|投标报价)(：|:|)[^\r\n]+\r\n");
                                    bidMoney = regMoney.Match(ctx).Value.Replace("中标价：", "").Replace("投标报价", "").Replace("投标价", "").Replace(",", "").Trim();
                                    Regex regPrjMgr = new Regex(@"(项目总监|项目负责人|项目经理姓名及资质证书编号|项目经理)(：|:)[^\r\n]+\r\n");
                                    prjMgr = regPrjMgr.Match(ctx).Value.Replace("项目总监：", "").Replace("项目负责人：", "").Replace("项目经理姓名及资质证书编号：", "").Replace("项目经理：", "").Trim();
                                    if (prjMgr.Contains("（"))
                                    { 
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("（")).ToString();
                                    }
                                }
                                catch
                                {
                                     
                                }
                            }
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
                            Regex regBuidUnit = new Regex(@"(招 标 人|招标人|招 标人)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(bidCtx).Value.Replace("招 标 人：", "").Replace("招 标人：", "").Replace("招标人：", "").Trim();
                            if (buildUnit.Contains("招标代理机构"))
                            {
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理机构")).ToString().Trim();
                            }
                            msgType = "云浮市工程建设交易中心";
                            specType = "建设工程";
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            if (bidUnit == "")
                            {
                                bidUnit = "";
                            }
                            if (Encoding.Default.GetByteCount(buildUnit) > 150)
                            {
                                buildUnit = "";
                            }
                            if (Encoding.Default.GetByteCount(bidUnit) > 150)
                            {
                                bidUnit = "";
                            }
                            prjName = ToolDb.GetPrjName(prjName);
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "云浮市区", "", string.Empty, code, prjName, buildUnit, beginDate,
                                  bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                  bidMoney, InfoUrl, prjMgr, beginDate, beginDate, HtmlTxt);
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
