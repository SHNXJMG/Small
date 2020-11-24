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
using System.Threading;
namespace Crawler.Instance
{
    public class BidHzHuiYang : WebSiteCrawller
    {
        public BidHzHuiYang()
            : base()
        {
            this.Group = "街道办中标信息";
            this.Title = "广东省惠州市惠阳区信息中标公告";
            this.Description = "自动抓取广东省惠州市惠阳区信息中标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://hyjyzx.huiyang.gov.cn/gcjy/zbgs/index.shtml";
            this.MaxCount = 100;
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "noBorder")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToNodePlainString();
                    pageInt = Convert.ToInt32(temp.GetRegexBegEnd("/", "页"));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://hyjyzx.huiyang.gov.cn/gcjy/zbgs/index_" + i + ".shtml", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("UL"), new HasAttributeFilter("class", "list lh24 f14")), true), new TagNameFilter("li")));
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

                        ATag aTag = viewList[j].GetATag();
                        prjName = aTag.LinkText.Trim();
                        InfoUrl = "http://hyjyzx.huiyang.gov.cn/" + aTag.Link.GetReplace("../");
                        beginDate = viewList[j].ToPlainTextString().GetDateRegex();
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml();
                            if (prjName.Contains(".."))
                            {
                                parser = new Parser(new Lexer(htlDtl));
                                NodeList nameNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("h1"));
                                if (nameNode != null && nameNode.Count > 0)
                                    prjName = nameNode[0].ToNodePlainString();
                            }
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                            bidType = prjName.GetInviteBidType();

                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = bidCtx.GetBuildRegex();
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";

                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();

                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag table = tableNode[0] as TableTag;
                                    if (table.ToPlainTextString().Contains("投标人"))
                                    {
                                        if (table.RowCount > 1)
                                        {
                                            for (int r = 0; r < table.Rows[0].ColumnCount; r++)
                                            {
                                                try
                                                {
                                                    ctx += table.Rows[0].Columns[r].ToNodePlainString() + "：";
                                                    ctx += table.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                                }
                                                catch { continue; }
                                            }

                                            bidUnit = ctx.GetBidRegex();
                                            if (string.IsNullOrEmpty(bidUnit))
                                                bidUnit = ctx.GetRegex("投标人名称,投标人");
                                            if (bidUnit.Contains("单位名称"))
                                                bidUnit = ctx.GetRegex("第一中标候选人");
                                            bidMoney = ctx.GetMoneyRegex();
                                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                                bidMoney = ctx.GetMoneyRegex(new string[] { "报价" });
                                        }
                                    }
                                    else
                                    {
                                        for (int r = 0; r < table.RowCount; r++)
                                        {
                                            try
                                            {
                                                ctx += table.Rows[r].Columns[0].ToNodePlainString() + "：";
                                                ctx += table.Rows[r].Columns[1].ToNodePlainString() + "\r\n";
                                            }
                                            catch { continue; }
                                        }
                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrEmpty(bidUnit))
                                            bidUnit = ctx.GetRegex("单位名称");
                                        bidMoney = ctx.GetMoneyRegex();

                                        prjMgr = ctx.GetMgrRegex();
                                        if (string.IsNullOrEmpty(prjMgr))
                                            prjMgr = ctx.GetRegex("项目负责人姓名及资质证书编号");
                                        if (prjMgr.Contains("，"))
                                            prjMgr = prjMgr.Remove(prjMgr.IndexOf("，"));
                                        if (prjMgr.Contains(","))
                                            prjMgr = prjMgr.Remove(prjMgr.IndexOf(","));
                                    }
                                }
                                else
                                {
                                    bidUnit = bidCtx.GetRegexBegEnd("中标人", "公司").GetReplace(":,：") + "公司";
                                    if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                        bidMoney = bidCtx.GetRegexBegEnd("中标价", "元").GetReplace(":,：").GetMoney();
                                }
                            }
                            try
                            {
                                if (decimal.Parse(bidMoney) < 1)
                                    bidMoney = "0";
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }

                            if (bidUnit == "公司")
                                bidUnit = "";
                            msgType = "惠州市惠阳区公共资源交易中心";
                            specType = "政府采购";

                            BidInfo info = ToolDb.GenBidInfo("广东省", "惠州市区", "惠阳区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                  bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k].GetATag();
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://hyjyzx.huiyang.gov.cn/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                            if (list.Count % 40 == 0)
                            {
                                Thread.Sleep(1000 * 60 * 5);
                            }
                        }
                    }
                }

            }
            return list;
        }
    }
}
