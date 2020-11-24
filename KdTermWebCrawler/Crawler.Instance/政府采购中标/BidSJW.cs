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
using System.IO;
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class BidSJW : WebSiteCrawller
    {
        public BidSJW()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "深圳市交通运输委员会中标信息";
            this.Description = "自动抓取深圳市交通运输委员会中标信息";
            this.PlanTime = "21:08,11:12,14:02,16:33";
            this.SiteUrl = "http://61.144.227.212/was5/web/search?token=24.1504521526494.90&channelid=286471&templet=jwtender_list.jsp";
            this.MaxCount = 1000;
            this.ExistCompareFields = "InfoUrl";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList nodePage = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "r_con")), true), new TagNameFilter("a")));
            if (nodePage != null && nodePage.Count > 0)
            {

                try
                {
                    Regex reg = new Regex(@"[0-9]+");
                    string temp = reg.Match(nodePage[nodePage.Count - 1].GetATagHref().Replace("&#39;", "")).Value;
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {

                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://61.144.227.212/was5/web/search?page=" + i + "&channelid=286471&token=24.1504521526494.90&perpage=15&outlinepage=10&templet=jwtender_list.jsp", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "zx_ml_list zx_ml_list_right")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 1; j < nodeList.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        if (prjName.Contains("]"))
                        {
                            int len = prjName.LastIndexOf("]");
                            prjName = prjName.Substring(len + 1, prjName.Length - len - 1);
                        }
                        InfoUrl = "http://61.144.227.212/was5/web/" + aTag.Link.Replace("./", "");
                        string htmlDtl = string.Empty;
                        try
                        {
                            htmlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }

                        parser = new Parser(new Lexer(htmlDtl));
                        NodeList nodeDtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "zx_xxgk_cont")));
                        if (nodeDtl != null && nodeDtl.Count > 0)
                        {
                            HtmlTxt = nodeDtl.AsHtml();
                            bidCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString();
                            parser.Reset();
                            NodeList dateNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tit")));
                            if (dateNode != null && dateNode.Count > 0)
                            {
                                beginDate = dateNode.AsString().GetDateRegex();
                            }
                            parser.Reset();
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "xm_name")));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag table = tableNode[0] as TableTag;
                                if (table.RowCount > 1)
                                {
                                    string ctx = string.Empty;
                                    for (int t = 0; t < table.Rows[0].ColumnCount; t++)
                                    {
                                        ctx += table.Rows[0].Columns[t].ToNodePlainString() + "：";
                                        ctx += table.Rows[1].Columns[t].ToNodePlainString() + "\r\n";
                                    }
                                    code = ctx.GetCodeRegex();
                                    bidUnit = ctx.GetBidRegex();
                                    bidMoney = ctx.GetRegexBegEnd("折扣率：", "\r");

                                    Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                                    if (!string.IsNullOrEmpty(regBidMoney.Match(bidMoney).Value))
                                    {
                                        if (bidMoney.Contains("万元") || bidMoney.Contains("万美元") || bidMoney.Contains("万"))
                                        {
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
                                    }
                                }
                            }
                            else
                            {
                                code = bidCtx.GetCodeRegex();
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex();
                            }

                            prjAddress = bidCtx.GetAddressRegex();
                            bidType = prjName.GetInviteBidType();
                            specType = "政府采购";
                            msgType = "深圳市交通运输委员会";
                            if (string.IsNullOrEmpty(buildUnit)) buildUnit = msgType;

                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳政府采购", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);

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
