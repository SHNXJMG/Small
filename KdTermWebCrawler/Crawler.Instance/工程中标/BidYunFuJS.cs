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
    public class BidYunFuJS : WebSiteCrawller
    {
        public BidYunFuJS()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省云浮市建设工程中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.Description = "自动抓取广东省云浮市建设工程中标信息";
            this.SiteUrl = "http://ggzy.yunfu.gov.cn/yfggzy/jsgc/002003/";
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
             NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "wb-page-items clearfix")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToPlainTextString().GetRegexBegEnd("/", "转到");
                    page = int.Parse(temp);
                }
                catch { }
            } 
            for (int i = 1; i < page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?pageing=" + i.ToString(), Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList node = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "r-items")), true), new TagNameFilter("li")));
                if (node != null && node.Count > 0)
                {
                    for (int j = 0; j < node.Count; j++)
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

                        ATag aTag = node[j].GetATag();
                        InfoUrl = "http://ggzy.yunfu.gov.cn" + aTag.Link;
                        prjName = aTag.LinkText;
                        beginDate = node[j].ToPlainTextString().GetDateRegex();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "mainContent")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();

                            bidCtx = HtmlTxt.ToCtxString().Replace("（盖章）", "").Replace("（元）", "").Replace("(元)", "").Replace("(盖章)", "").Replace("&ldquo;", "").Replace("&rdquo;", "").Trim();
                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                bidUnit = bidCtx.GetBidRegex(new string[] {"第一中标候选人","第一候选人" });
                            }
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            if (prjMgr.Contains("（"))
                            {
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("（"));
                            }
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                bidUnit = bidCtx.GetRegexBegEnd("第一中标候选人", "，").Replace("：","").Replace(":","").Replace("\r","").Replace("\n","");
                            }
                            if (bidUnit.Contains("投标"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("投标"));
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.LastIndexOf ("公司"))+ "公司";

                            parserdetail.Reset();
                            NodeList nameNode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("h2"), new HasAttributeFilter("class", "post-title")));
                            if (nameNode != null && nameNode.Count > 0)
                            {
                                string tempName = nameNode[0].ToNodePlainString();
                                if (!string.IsNullOrWhiteSpace(tempName))
                                    prjName = tempName;
                            }
                            msgType = "云浮市建设工程交易中心";
                            specType = "建设工程";
                            bidCtx = bidCtx.Replace("<?", "").Replace("xml:namespace prefix = st1 ns = ", "").Replace("urn:schemas-microsoft-com:office:smarttags", "").Replace("/>", "").Replace("：", "").Trim();
                            
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "云浮市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
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
