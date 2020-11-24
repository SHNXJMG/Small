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
using System.Web.UI.HtmlControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class BidNanFangDW : WebSiteCrawller
    {
        public BidNanFangDW()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "中国南方电网";
            this.Description = "自动抓取中国南方电网";
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.bidding.csg.cn/zbhxrgs/index.jhtml";
            this.MaxCount = 40;
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList noList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "Top10 TxtCenter")));
            if (noList != null && noList.Count > 0)
            {
                string temp = noList.AsString().GetRegexBegEnd("/", "页");
                try
                {
                    pageInt = Convert.ToInt32(temp);
                }
                catch { pageInt = 10; }
            }
            else
                pageInt = 10;
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.bidding.csg.cn/zbhxrgs/index_" + i.ToString() + ".jhtml", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "W750 Right")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 1; j < nodeList.Count; j++)
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
                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.LinkText;
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.bidding.csg.cn" + aTag.Link;
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "Center W1000")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {

                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList nameNode = parser.ExtractAllNodesThatMatch(new AndFilter(new
                                 TagNameFilter("h1"), new HasAttributeFilter("class", "TxtCenter Padding10")));
                            if (nameNode != null && nameNode.Count > 0)
                            {
                                prjName = nameNode[0].ToNodePlainString();
                            }
                            bidType = prjName.GetInviteBidType();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegexBegEnd("公开询价确定", "成交单位");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("成交人,拟定采购单位,成交候选人,第一推荐成交候选人,第一");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegexBegEnd("签约单位为", "。");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegexBegEnd("第一入围候选人", "，");
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    TableTag tableDtl = tableNode[0] as TableTag;
                                    string ctx = string.Empty;
                                    for (int k = 1; k < tableDtl.RowCount; k++)
                                    {
                                        try
                                        {
                                            ctx += tableDtl.Rows[k].Columns[0].ToNodePlainString().Replace("单位名称", "中标单位").Replace("中标候选人", "中标单位") + "：";
                                            ctx += tableDtl.Rows[k].Columns[1].ToNodePlainString() + "\r\n";
                                        }
                                        catch { }
                                    }
                                    bidUnit = ctx.GetReplace("中标单位：第一").GetBidRegex();
                                    if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                        bidMoney = ctx.GetMoneyRegex();
                                    prjMgr = ctx.GetRegex("项目经理姓名及资质证书编号");
                                    if (prjMgr.IndexOf("/") > 0)
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                                    if (string.IsNullOrEmpty(bidUnit) || string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                    {
                                        ctx = string.Empty;
                                        for (int k = 0; k < tableDtl.RowCount; k++)
                                        {
                                            try
                                            {
                                                for (int d = 0; d < tableDtl.Rows[k].ColumnCount; d++)
                                                {
                                                    ctx += tableDtl.Rows[k].Columns[d].ToNodePlainString().Replace("单位名称", "中标单位").Replace("中标侯选人", "中标单位") + "：";
                                                    ctx += tableDtl.Rows[k + 1].Columns[d].ToNodePlainString() + "\r\n";
                                                }
                                            }
                                            catch { }
                                        }
                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                            bidMoney = ctx.GetMoneyRegex();
                                        prjMgr = ctx.GetRegex("项目经理姓名及资质证书编号");
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(prjAddress))
                            {
                                prjAddress = "见中标信息";
                            }
                            specType = "其他";
                            msgType = "中国南方电网有限责任公司招标服务中心";
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "中国南方电网有限责任公司招标服务中心";
                            }
                            bidUnit = bidUnit.GetReplace("：");
                            BidInfo info = ToolDb.GenBidInfo("广东省", "电网专项工程", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                    bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList nodeAtag = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (nodeAtag != null && nodeAtag.Count > 0)
                            {
                                for (int c = 0; c < nodeAtag.Count; c++)
                                {
                                    ATag a = nodeAtag[c] as ATag;
                                    if (a.Link.IsAtagAttach())
                                    {
                                        string alink = "http://www.bidding.csg.cn/" + a.Link;
                                        try
                                        {
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText.Replace("&nbsp", ""), info.Id, alink);
                                            base.AttachList.Add(attach);
                                        }
                                        catch { }
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
