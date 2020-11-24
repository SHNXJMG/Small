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
    public class InviteSTQuXian : WebSiteCrawller
    {
        public InviteSTQuXian()
            : base()
        {
            this.Group = "区县招标信息";
            this.Title = "广东省汕头市区县交易中心";
            this.Description = "自动抓取广东省汕头市区县交易中心";
            this.PlanTime = "9:02,10:25,14:07,16:08";
            this.SiteUrl = "http://www.stjs.gov.cn/zbtb/qxzhaobiaonews.asp?page=1";
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
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("width", "700")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().ToNodeString();
                    Regex reg = new Regex(@"/[^下一页]+下一页");
                    string page = reg.Match(temp).Value.Replace("/", "").Replace("下一页", "");
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.stjs.gov.cn/zbtb/qxzhaobiaonews.asp?page=" + i.ToString(), Encoding.Default);
                    }
                    catch
                    { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "705")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[nodeList.Count - 1] as TableTag;
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

                            prjName = prjType.Replace("[中标公示]", "");
                            bidType = prjName.GetInviteBidType();
                            beginDate = tr.Columns[2].ToNodePlainString().Replace(".", "-");
                            InfoUrl = "http://www.stjs.gov.cn/" + tr.Columns[1].GetATagHref().Replace("../", "");
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "680")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                bidCtx = HtmlTxt.ToCtxString();
                                string ctx = string.Empty;
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tabList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                                if (tabList != null && tabList.Count > 0)
                                {
                                    TableTag tab = tabList[0] as TableTag;
                                    for (int k = 0; k < tab.RowCount; k++)
                                    {
                                        for (int d = 0; d < tab.Rows[k].ColumnCount; d++)
                                        {
                                            if (d % 2 == 0)
                                                ctx += tab.Rows[k].Columns[d].ToPlainTextString().Replace(" ", "").Replace("\n", "").Replace("\r", "") + "：";
                                            else
                                                ctx += tab.Rows[k].Columns[d].ToPlainTextString().Replace(" ", "").Replace("\n", "").Replace("\r", "") + "\r\n";
                                        }
                                    }
                                    bidUnit = ctx.GetBidRegex();
                                    bidMoney = ctx.GetMoneyRegex();
                                    buildUnit = ctx.GetBuildRegex();
                                    prjAddress = ctx.GetAddressRegex();
                                    code = ctx.GetCodeRegex();
                                }
                                else
                                {
                                    bidUnit = bidCtx.GetBidRegex();
                                    bidMoney = bidCtx.GetMoneyRegex();
                                    buildUnit = bidCtx.GetBuildRegex();
                                    prjAddress = bidCtx.GetAddressRegex();
                                    code = bidCtx.GetCodeRegex();
                                }
                               


                                msgType = "汕头建设网";
                                specType = "建设工程";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "汕头市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
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
                            prjName = prjType.Replace("[招标公告]", "");
                            inviteType = prjName.GetInviteBidType();
                            beginDate = tr.Columns[2].ToNodePlainString().Replace(".", "-");
                            InfoUrl = "http://www.stjs.gov.cn/" + tr.Columns[1].GetATagHref().Replace("../", "");
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "680")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                inviteCtx = HtmlTxt.ToCtxString().Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");

                                string ctx = string.Empty;
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tabList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                                if (tabList != null && tabList.Count > 0)
                                {
                                    TableTag tab = tabList[0] as TableTag;
                                    for (int k = 0; k < tab.RowCount; k++)
                                    {
                                        for (int d = 0; d < tab.Rows[k].ColumnCount; d++)
                                        {
                                            if (d % 2 == 0)
                                                ctx += tab.Rows[k].Columns[d].ToPlainTextString().Replace(" ", "").Replace("\r\n", "") + "：";
                                            else
                                                ctx += tab.Rows[k].Columns[d].ToPlainTextString().Replace(" ", "").Replace("\r\n", "") + "\r\n";
                                        }
                                    }
                                    buildUnit = ctx.GetBuildRegex();
                                    code = ctx.GetCodeRegex().Replace("/", "");
                                    prjAddress = ctx.GetAddressRegex();
                                }
                                else
                                {
                                    buildUnit = inviteCtx.GetBuildRegex();
                                    code = inviteCtx.GetCodeRegex().Replace("/", "");
                                    prjAddress = inviteCtx.GetAddressRegex();
                                }

                                msgType = "汕头建设网";
                                specType = "建设工程";
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "汕头市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
