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
    public class BidJinCaiW : WebSiteCrawller
    {
        public BidJinCaiW()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "中国金融集中采购网中标信息";
            this.Description = "自动抓取中国金融集中采购网中标信息";
            this.PlanTime = "9:40,11:40,14:40,16:40,18:40";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 100;
            this.SiteUrl = "http://www.cfcpn.com/plist/jieguo?pageNo=1&kflag=0&keyword=&keywordType=&province=&city=&typeOne=&ptpTwo=,";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string html = string.Empty;
            int pageInt = 1;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "pagination")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 2].ToNodePlainString();
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.cfcpn.com/plist/jieguo?pageNo="+i+"&kflag=0&keyword=&keywordType=&province=&city=&typeOne=", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "cfcpn_list_content text-left")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.cfcpn.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList telNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("p"), new HasAttributeFilter("class", "cfcpn_news_title")));
                        if (telNode != null && telNode.Count > 0)
                        {
                            prjName = telNode.AsHtml();
                            prjName = prjName.ToCtxString();
                        }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "news_content")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("<br/>,</p>,<br>,<br />,</div>", "\r\n").ToCtxString().GetReplace("一包：\r\n", "一包：").GetReplace("一包：\r\n", "一包：").GetReplace("一包：\r\n", "一包：").GetReplace("一包：\r\n", "一包：").GetReplace("一包：\r\n", "一包：").GetReplace("一包：\r\n", "一包：").GetReplace("一包：\r\n", "一包：").GetReplace("一包：\r\n", "一包：").GetReplace("一包：\r\n", "一包：");
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();

                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("成交候选人,第一中标候选人名称,一包").GetReplace("名称");
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();

                            if (bidUnit.Contains("废标") || bidCtx.Contains("废除原因") || bidCtx.Contains("废止原因") || bidCtx.Contains("废标"))
                            {
                                bidUnit = "废标";
                                prjMgr = string.Empty;
                                bidMoney = "0";
                            }

                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"),new HasAttributeFilter("border","1")));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    TableTag dtlTable = tableNode[0] as TableTag;
                                    string ctx = string.Empty;
                                    if (dtlTable.RowCount > 1)
                                    {
                                        try
                                        {
                                            for (int r = 0; r < dtlTable.Rows[0].ColumnCount; r++)
                                            {
                                                ctx += dtlTable.Rows[0].Columns[r].ToNodePlainString()+"：";
                                                ctx += dtlTable.Rows[1].Columns[r].ToNodePlainString()+"\r\n";
                                            }
                                        }
                                        catch { }
                                        bidUnit = ctx.GetBidRegex();
                                        if (bidMoney == "0")
                                            bidMoney = ctx.GetMoneyRegex();
                                        if (string.IsNullOrEmpty(prjMgr))
                                            prjMgr = ctx.GetMgrRegex();
                                    }
                                }
                            }
                            bidUnit = bidUnit.GetReplace("名称,&#160", "");
                            buildUnit = buildUnit.GetReplace("&#160");
                            prjAddress = prjAddress.GetReplace("&#160");
                            prjName = prjName.GetReplace("&#160");
                            code = code.GetReplace("&#160");
                            prjMgr = prjMgr.GetReplace("&#160");
                            specType = "政府采购";
                            bidType = prjName.GetInviteBidType();
                            msgType = "中国金融集中采购网";

                            BidInfo info = ToolDb.GenBidInfo("全国", "金融专项采购", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag file = aNode[a].GetATag();
                                    if (file.IsAtagAttach())
                                    {
                                        string link = file.Link;
                                        if (!link.ToLower().Contains("http"))
                                            link = "http://www.cfcpn.com/" + file.Link;
                                        base.AttachList.Add(ToolDb.GenBaseAttach(file.LinkText, info.Id, link));
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
