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
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class BidHbRmzf : WebSiteCrawller
    {
        public BidHbRmzf()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "湖北省人民政府办公厅信息中标信息";
            this.Description = "自动抓取湖北省人民政府办公厅信息中标信息";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://gcjs.cnhubei.com/dict/420000/20/";
            this.MaxCount = 120;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 409;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + i);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "msgTable")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
                            bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty,
                            InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty,
                            prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty,
                        area = string.Empty;

                        TableRow tr = table.Rows[j];

                        ATag aTag = tr.GetATag();
                        area = tr.Columns[2].ToNodePlainString();
                        if (area.Contains("市"))
                        {
                            area = area.Remove(area.IndexOf("市")) + "市";
                        }
                        else if (area.Contains("区"))
                        {
                            area = area.Remove(area.IndexOf("区")) + "区";
                        }
                        else if (area.Contains("县"))
                        {
                            area = area.Remove(area.IndexOf("县")) + "县";
                        }
                        else if (area.Contains("镇"))
                        {
                            area = area.Remove(area.IndexOf("镇")) + "镇";
                        }
                        else if (area.Contains("州"))
                        {
                            area = area.Remove(area.IndexOf("州")) + "州";
                        }
                        beginDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://gcjs.cnhubei.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "3")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            TableTag dtlTable = dtlNode[0] as TableTag;
                            for (int r = 0; r < dtlTable.RowCount; r++)
                            {
                                for (int c = 0; c < dtlTable.Rows[r].ColumnCount; c++)
                                {
                                    string temp = string.Empty;
                                    if (r + 1 == dtlTable.RowCount && c % 2 != 0)
                                        temp = dtlTable.Rows[r].Columns[c].ToHtml().GetReplace("<br>,</p>,</br>", "\r\n").ToCtxString();
                                    else
                                        temp = dtlTable.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                    if (c % 2 == 0)
                                        bidCtx += temp + "：";
                                    else
                                        bidCtx += temp + "\r\n";
                                }
                            }
                            prjName = bidCtx.GetRegex("项目名称,工程名称");
                            code = bidCtx.GetRegex("项目编码");
                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                bidUnit = bidCtx.GetReplace("中标候选人名称\r\n\r\n,中标候选人名称\r\n", "中标候选人名称：").GetBidRegex(new string[] { "中标候选人名称" });
                            }
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("中标结果");
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                            {
                                bidMoney = bidCtx.GetReplace("投标报价（万元）\r\n\r\n,投标报价（万元）\r\n", "投标报价（万元）：").GetMoneyRegex(new string[] { "投标报价" });
                            }
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                            {
                                bidMoney = bidCtx.GetReplace("投标报价(万元)\r\n\r\n,投标报价(万元)\r\n", "投标报价(万元)：").GetMoneyRegex(new string[] { "投标报价" });
                            }
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                            {
                                bidMoney = bidCtx.GetReplace("投标报价（元）\r\n\r\n,投标报价（元）\r\n", "投标报价（元）：").GetMoneyRegex(new string[] { "投标报价" });
                            }
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                            {
                                bidMoney = bidCtx.GetReplace("投标报价(元)\r\n\r\n,投标报价(元)\r\n", "投标报价(元)：").GetMoneyRegex(new string[] { "投标报价" });
                            }
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                            {
                                bidMoney = bidCtx.GetReplace("投标报价\r\n\r\n,投标报价\r\n", "投标报价：").GetMoneyRegex(new string[] { "投标报价" });
                            }
                            try
                            {
                                if (decimal.Parse(bidMoney) < 1)
                                    bidMoney = "0";
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                            if (bidUnit.Contains("编号") || bidUnit.Contains("、") || bidUnit.Contains("恩施州公共资源交易中心"))
                                bidUnit = "";
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            bidUnit = bidUnit.GetNotChina();
                            msgType = "湖北省人民政府办公厅";
                            specType = "建设工程";
                            bidType = prjName.GetInviteBidType();
                            BidInfo info = ToolDb.GenBidInfo("湖北省", "湖北省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                        else
                                            link = "http://gcjs.cnhubei.com/" + a.Link;
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
