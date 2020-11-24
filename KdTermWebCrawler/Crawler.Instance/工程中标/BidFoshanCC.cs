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
    public class BidFoshanCC : WebSiteCrawller
    {
        public BidFoshanCC()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省佛山市禅城区中标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.Description = "自动抓取广东省佛山市禅城区中标信息";
            this.SiteUrl = "http://www.fsggzy.cn/gcjy/gc_zbxx1/gczbgq/gc_1zbcc/";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8).Replace("&nbsp;", "");
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().GetRegexBegEnd("HTML", ",").GetReplace("(");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            string cookiestr = string.Empty;

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "index_" + (i - 1).ToString() + ".html");
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "secondrightlistbox")), true), new TagNameFilter("li")));
                if (sNode != null && sNode.Count > 0)
                {
                    for (int t = 0; t < sNode.Count; t++)
                    {

                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        INode node = sNode[t];
                        ATag aTag = node.GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = this.SiteUrl + aTag.Link.GetReplace("./");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content2")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList bidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (bidNode != null && bidNode.Count > 0)
                            {
                                TableTag table = bidNode[0] as TableTag;
                                for (int r = 0; r < table.RowCount; r++)
                                {
                                    for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = table.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                        if ((c + 1) % 2 == 0)
                                            bidCtx += temp + "\r\n";
                                        else
                                            bidCtx += temp + "：";
                                    }
                                }
                            }
                            else
                                bidCtx = HtmlTxt.ToCtxString();
                            bidUnit = bidCtx.GetBidRegex();
                            if (!string.IsNullOrEmpty(bidUnit) && bidUnit.Length <= 3)
                                bidUnit = "";
                             
                            buildUnit = bidCtx.GetBuildRegex();

                           
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrEmpty(prjMgr))
                                prjMgr = bidCtx.GetRegex("项目负责人及资质证书编号");
                            if (string.IsNullOrEmpty(prjMgr))
                            {
                                prjMgr = bidCtx.GetRegex("项目负责人/证书号");
                            }
                            if (prjMgr.Contains("/"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                            msgType = "佛山市禅城区建设工程交易中心";
                            specType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "佛山市区", "禅城区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
