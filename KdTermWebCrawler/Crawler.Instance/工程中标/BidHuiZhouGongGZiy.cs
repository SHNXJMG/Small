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
    public class BidHuiZhouGongGZiy : WebSiteCrawller
    {
        public BidHuiZhouGongGZiy()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省惠州市公共资源交易中心";
            this.Description = "自动抓取广东省惠州市公共资源交易中心中标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.SiteUrl = "http://zyjy.huizhou.gov.cn/pages/cms/hzggzyjyzx/html/artList.html?cataId=a000dc84e53b4dc88e1e05d15d7c90f7";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagination")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().Replace(" ", "");
                    Regex reg = new Regex(@"/[^页]+页");
                    pageInt = Convert.ToInt32(reg.Match(temp).Value.Replace("/", "").Replace("页", ""));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://zyjy.huizhou.gov.cn/pages/cms/hzggzyjyzx/html/artList.html?cataId=a000dc84e53b4dc88e1e05d15d7c90f7&pageNo=" + i.ToString(), Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "div_list"))), new TagNameFilter("ul")));
                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
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
                        Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                        beginDate = regDate.Match(viewList[j].ToPlainTextString()).Value;
                        //prjName = viewList[j].ToPlainTextString().Replace("\r", "").Replace("\n", "").Replace(beginDate, "");
                        ATag aTag = viewList.SearchFor(typeof(ATag), true)[j] as ATag;
                        prjName = aTag.GetAttribute("title");
                        InfoUrl = "http://zyjy.huizhou.gov.cn" + aTag.Link;
                        string htmDtl = string.Empty;
                        try
                        {
                            htmDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
                            htmDtl = regexHtml.Replace(htmDtl, "");
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "divZoom")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            NodeList ifrm = new Parser(new Lexer(htmDtl)).ExtractAllNodesThatMatch(new TagNameFilter("iframe"));
                            if (ifrm != null && ifrm.Count > 0)
                            {
                                IFrameTag frame = ifrm[0] as IFrameTag;
                                string url = frame.GetAttribute("src");
                                try
                                {
                                    string htm = this.ToolWebSite.GetHtmlByUrl(url, Encoding.Default);
                                    NodeList tabNode = new Parser(new Lexer(htm)).ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                    string ctx = tabNode.AsHtml().ToCtxString().Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t");
                                    bidCtx = ctx + bidCtx;
                                }
                                catch { }
                            }
                            //bidCtx = System.Text.RegularExpressions.Regex.Replace(HtmlTxt, "(<script)[\\s\\S]*?(</script>)", "");
                            //bidCtx = System.Text.RegularExpressions.Regex.Replace(bidCtx, "<[^>]*>", "").Replace("&nbsp;", "").Replace(" ", "").Replace("\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\r\n", "\r\n").Replace("\r\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\t", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                             
                            Regex regPrjCode = new Regex(@"(工程编号|项目编号|招标编号|中标编号|编号)(:|：)[^\r\n]+\r\n");
                            code = regPrjCode.Match(bidCtx.Replace(" ", "")).Value.Replace("工程编号", "").Replace("项目编号", "").Replace("招标编号", "").Replace("中标编号", "").Replace("编号", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regBuidUnit = new Regex(@"(建设单位|招标人|承包人|招标单位|招标方|招标代理机构)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(bidCtx.Replace(" ", "")).Value.Replace("招标代理机构", "").Replace("建设单位", "").Replace("招标人", "").Replace("承包人", "").Replace("招标单位", "").Replace("招标方", "").Replace("：", "").Replace(":", "").Trim();

                            Regex regMoney = new Regex(@"(中标价|投标价|总投资|发包价|投标报价|价格|金额|总价)(：|:|)[^\r\n]+\r\n");
                            bidMoney = regMoney.Match(bidCtx.Replace(" ", "")).Value.Replace("中标价", "").Replace("总投资", "").Replace("发包价", "").Replace("总价", "").Replace("投标报价", "").Replace("投标价", "").Replace("价格", "").Replace("金额", "").Replace("：", "").Replace(":", "").Replace("，", "").Replace(",", "").Trim();

                            Regex regBidUnit = new Regex(@"(成交供应商|中标供应商|第一候选人|中标候选人|中标单位|中标人|中标方)(：|:)[^\r\n]+\r\n");
                            bidUnit = regBidUnit.Match(bidCtx.Replace(" ", "")).Value.Replace("成交供应商", "").Replace("中标供应商", "").Replace("中标候选人", "").Replace("第一候选人", "").Replace("中标单位", "").Replace("中标人", "").Replace("中标方", "").Replace("：", "").Replace(":", "").Trim();

                            Regex regprjMgr = new Regex(@"(项目经理姓名|项目经理（或建造师）|项目经理|项目负责人|项目总监|建造师|总工程师|监理师)(：|:)[^\r\n]+\r\n");
                            prjMgr = regprjMgr.Match(bidCtx.Replace(" ", "")).Value.Replace("项目经理（或建造师）", "").Replace("项目经理姓名", "").Replace("总工程师", "").Replace("项目经理", "").Replace("项目总监", "").Replace("建造师", "").Replace("监理师", "").Replace("项目负责人", "").Replace("：", "").Replace(":", "").Trim();

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
                            if (prjMgr.Contains("资格"))
                            {
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("资格"));
                            }
                            bidUnit = ToolHtml.GetStringTemp(bidUnit).Replace("；", "");
                            buildUnit = ToolHtml.GetSubString(buildUnit, 150);
                            bidUnit = ToolHtml.GetSubString(bidUnit, 150);
                            code = ToolHtml.GetSubString(code, 50);
                            prjMgr = ToolHtml.GetSubString(prjMgr, 50);
                            msgType = "惠州市公共资源交易中心";
                            specType = "建设工程";
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "惠州市公共资源交易中心";
                            }
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "惠州市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
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
