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
    public class BidZJJingJi : WebSiteCrawller
    {
        public BidZJJingJi()
            : base()
        {
            this.Group = "区县中标信息";
            this.Title = "广东省湛江市经济开发技术区";
            this.Description = "自动抓取广东省湛江市经济开发技术区";
            this.PlanTime = "9:34,10:34,14:26,16:26";
            this.SiteUrl = "http://www.zetdz.gov.cn/tzgg/list.asp?class=5";
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
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("style", "float:left;line-height:30px;height:30px;")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToNodePlainString().ToCtxString().Replace("[", "页");
                    //temp = temp.Remove(temp.IndexOf("["));
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
                NodeList nodeListn = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "95%")));//parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "25"))), new TagNameFilter("table")));//parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"),new HasAttributeFilter("width","100%")));
                if (nodeListn != null && nodeListn.Count > 1)
                { 
                     TableTag table = nodeListn[1] as TableTag;
                     for (int j = 0; j < table.RowCount; j++)
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

                         TableRow tr = table.Rows[j];
                        beginDate = tr.Columns[0].ToNodePlainString().GetDateRegex();
                        prjName = tr.Columns[0].ToNodePlainString().Replace(beginDate, "").Replace("[]","");
                        bidType = prjName.GetInviteBidType();
                        InfoUrl = "http://www.zetdz.gov.cn/tzgg/" + tr.Columns[0].GetATagHref();

                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                            htlDtl = htlDtl.GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "95%")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            HtmlTxt = dtlList.ToHtml();
                            bidCtx = HtmlTxt.ToCtxString();

                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                bidUnit = bidCtx.ToCtxString().GetRegexBegEnd("确定", "为湛江");
                            }
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                            {
                                bidMoney = bidCtx.ToCtxString().GetRegexBegEnd("金额为", "。");
                            }
                            prjAddress = bidCtx.GetAddressRegex();
                            code = bidCtx.GetCodeRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "湛江经济技术开发区政府采购中心";
                            }
                            msgType = "湛江经济技术开发区政府采购中心";
                            specType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "湛江市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
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
