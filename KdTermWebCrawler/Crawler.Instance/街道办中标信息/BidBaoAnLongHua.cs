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
using System.Web.Script.Serialization;

namespace Crawler.Instance
{
    public class BidBaoAnLongHua : WebSiteCrawller
    {
        public BidBaoAnLongHua()
            : base()
        {
            this.Group = "街道办中标信息";
            this.Title = "广东省深圳市龙华街道办事处";
            this.Description = "自动抓取广东省深圳市龙华街道办事处中标信息";
            this.PlanTime = "9:20,13:51";
            this.SiteUrl = "http://lhbsc.szlhxq.gov.cn/lhbsc/bsdt43/qyfw78/zbcg2/zbxxgg/index.html";

        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "Normal")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().Replace("createPageHTML(", "").Replace("index", "").Replace("html", "").Replace(", 0,", "").Replace(");", "").Replace(",", "").Replace(";", "").Replace(")", "").Replace("\"", "").Replace(" ", "").GetRegexBegEnd("/", "跳");
                    pageInt = Convert.ToInt32(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    { 
                        string url = "http://lhbsc.szlhxq.gov.cn/lhbsc/bsdt43/qyfw78/zbcg2/zbxxgg/065b33d5-" + i.ToString() + ".html";
                        html = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("class", "")));
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
                        INode node = viewList[j];
                        ATag aTag = node.GetATag();
                        beginDate = regDate.Match(viewList[j].ToPlainTextString().Trim()).Value;
                        prjName = aTag.GetAttribute("title");
                        InfoUrl = "http://lhbsc.szlhxq.gov.cn" + aTag.Link.Replace("../", "").Replace("./", "");
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
                            htlDtl = regexHtml.Replace(htlDtl, "");
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "contentbox")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            Regex.Replace(dtl.AsHtml(), "(<script)[\\s\\S]*?(</script>)", "");
                            Regex.Replace(HtmlTxt, "(<script)[\\s\\S]*?(</script>)", "");
                            Regex.Replace(bidCtx, "<[^>]*>", "").Replace("&nbsp;", "").Replace(" ", "").Replace("\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("&yen;", "");
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            buildUnit = ToolHtml.GetRegexString(bidCtx, "按（建设单位）", "(提供)");

                            bidMoney = ToolHtml.GetRegexString(bidCtx, "(中标金额)", "(元)|(万元)|(；)").GetReplace("：","").GetMoney("万元");
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("中标供应商名称");

                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (prjMgr.Contains("资格"))
                            {
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("资格"));
                            }
                            if (string.IsNullOrWhiteSpace(bidMoney))
                                bidMoney = bidCtx.GetRegex("中标金额").GetReplace("：","");
                            bidUnit = ToolHtml.GetStringTemp(bidUnit);
                            if (string.IsNullOrWhiteSpace(buildUnit))
                                buildUnit = bidCtx.GetRegex("采购人名称");
                            bidUnit = ToolHtml.GetSubString(bidUnit, 150);
                            code = bidCtx.GetCodeRegex().GetReplace("）", "");
                            if (string.IsNullOrWhiteSpace(code))
                                code = bidCtx.GetRegexBegEnd("招标编号：", "）");
                            prjMgr = bidCtx.GetMgrRegex();
                            try
                            {
                                if (Convert.ToDecimal(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }

                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "深圳市龙华新区龙华街道办事处";
                            }
                            msgType = "深圳市龙华新区龙华街道办事处";
                            specType = "建设工程";
                            bidType = "小型工程";
                            prjName = ToolDb.GetPrjName(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳区及街道工程", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                   bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k] as ATag;
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
