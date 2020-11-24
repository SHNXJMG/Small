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
    public class InviteZJSuiXi : WebSiteCrawller
    {
        public InviteZJSuiXi()
            : base()
        {
            this.Group = "区县招标信息";
            this.Title = "广东省湛江市遂溪住房和城乡规划建设局";
            this.Description = "自动抓取广东省湛江市遂溪住房和城乡规划建设局";
            this.PlanTime = "9:27,10:28,14:06,16:07";
            this.SiteUrl = "http://net.suixi.gov.cn/com/ghjsj/ns.php?nowmenuid=5248";
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
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "menu")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().ToNodeString().Replace("条共", "");
                    Regex reg = new Regex(@"条[^页]+页");
                    string page = reg.Match(temp).Value.Replace("条", "").Replace("页", "");
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
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "&page=" + i.ToString(), Encoding.Default);
                    }
                    catch
                    { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "90%")));
                if (nodeList != null && nodeList.Count > 1)
                {
                    TableTag table = nodeList[nodeList.Count-2] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string prjType = tr.Columns[1].ToNodePlainString();
                        if (prjType.Contains("中标") || prjType.Contains("结果"))
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

                            prjName = prjType;
                            beginDate = tr.Columns[3].ToNodePlainString();
                            bidType = prjName.GetInviteBidType();
                            InfoUrl = "http://net.suixi.gov.cn/com/ghjsj/" + tr.Columns[1].GetATagHref();
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }

                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "92%")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                bidCtx = HtmlTxt.ToCtxString();

                                bidUnit = bidCtx.GetBidRegex();
                                if (string.IsNullOrEmpty(bidUnit))
                                {
                                    bidUnit = bidCtx.ToNodeString().GetRegexBegEnd("评标委员会推荐", "为中标候选");
                                }
                                bidMoney = bidCtx.GetMoneyRegex();
                                if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                {
                                    bidMoney = bidCtx.ToNodeString().GetRegexBegEnd("中标价", "，");
                                }
                                prjAddress = bidCtx.GetAddressRegex();
                                code = bidCtx.GetCodeRegex();
                                buildUnit = bidCtx.GetBuildRegex();
                                msgType = "遂溪住房和城乡规划建设局";
                                specType = "建设工程";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "湛江市区", "遂溪县", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                       bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                        else
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
               prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
               specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
               remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
               CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                            prjName = prjType;
                            beginDate = tr.Columns[3].ToNodePlainString();
                            inviteType = prjName.GetInviteBidType();
                            InfoUrl = "http://net.suixi.gov.cn/com/ghjsj/" + tr.Columns[1].GetATagHref();
                             string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }

                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "92%")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                inviteCtx = HtmlTxt.ToCtxString();


                                buildUnit = inviteCtx.GetBuildRegex();
                                code = inviteCtx.GetCodeRegex();
                                prjAddress = inviteCtx.GetAddressRegex();

                                msgType = "遂溪住房和城乡规划建设局";
                                specType = "建设工程";
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "湛江市区", "遂溪县", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
