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
    public class BidCZChaoAn : WebSiteCrawller
    {
        public BidCZChaoAn()
            : base()
        {
            this.Group = "区县中标信息";
            this.Title = "广东省潮州市潮安县住房和建设局";
            this.Description = "自动抓取广东省潮州市潮安县住房和建设局";
            this.PlanTime = "9:23,10:24,13:56,16:57";
            this.SiteUrl = "http://www.cajsw.gov.cn/bigclassdeta.asp?typeid=32&bigclassid=132";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("vAlign", "middle")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString();
                    Regex reg = new Regex(@"/[^页]+页");
                    string page = reg.Match(temp).Value.Replace("/", "").Replace("页", "");
                    pageInt = Convert.ToInt32(page);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl) + "&page=" + i.ToString(), Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("width", "100%"))), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
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

                        TableTag table = nodeList[j] as TableTag;
                        TableRow tr = table.Rows[0];

                        prjName = tr.Columns[0].ToNodePlainString();
                        bidType = prjName.GetInviteBidType();
                        beginDate = tr.Columns[1].ToPlainTextString();

                        InfoUrl = "http://www.cajsw.gov.cn/" + tr.Columns[0].GetATagHref(2);

                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                            htlDtl = htlDtl.GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "fontzoom")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            HtmlTxt = dtlList.ToHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            string ctx = bidCtx.ToNodeString();

                            bidUnit = ctx.GetRegexBegEnd("中标候选人为", "，");
                            bidUnit = ToolHtml.GetSubString(bidUnit, 150);
                            string money = ctx.GetRegexBegEnd("投标报价", "元").GetMoney();

                            bidMoney = money.GetMoney();

                            prjAddress = bidCtx.GetAddressRegex();
                            code = bidCtx.GetCodeRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            msgType = "潮州市潮安县住房和城乡建设局";
                            specType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "潮州市区", "潮安县", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
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
