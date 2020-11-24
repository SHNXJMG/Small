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
    public class BidHeYuan : WebSiteCrawller
    {
        public BidHeYuan()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省河源市工程建设中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.Description = "自动抓取广东省河源市工程建设中标信息";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.hyggzy.com/ggzy/jsgczbgs/";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new TagNameFilter("select"));
            if (tableNodeList != null && tableNodeList.Count > 0)
            {
                try
                {
                    SelectTag select = tableNodeList[0] as SelectTag;
                    page = int.Parse(select.OptionTags[select.OptionTags.Length-1].Value);
                }
                catch
                { }
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "/index_" + i + ".html", Encoding.UTF8);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "list2")), true), new TagNameFilter("li")));
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

                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.GetAttribute("title");

                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();

                        InfoUrl = "http://www.hyggzy.com" + aTag.Link.Replace("amp;", "").Trim();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content-cnt")));
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml(); 
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {  
                                TableTag dtlTable = tableNode[0] as TableTag;
                                for (int r = 0; r < dtlTable.RowCount; r++)
                                { 
                                    for (int c = 0; c < dtlTable.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = dtlTable.Rows[r].Columns[c].ToNodePlainString(); 
                                        if (c % 2 == 0)
                                            bidCtx += temp + "：";
                                        else
                                            bidCtx += temp + "\r\n"; 
                                    }
                                }
                            }
                            if (string.IsNullOrWhiteSpace(bidCtx))
                                bidCtx = HtmlTxt.GetReplace("</p>", "\r\n").ToCtxString();

                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetRegex("第一中标候选单位,第一中标候选人为");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrWhiteSpace(prjMgr))
                                prjMgr = bidCtx.GetRegex("项目负责人名称,项目负责人", true, 50);
                            if (prjMgr.Contains("投标报价"))
                                prjMgr = string.Empty;
                            if (bidUnit.Contains("单位名称"))
                                bidUnit = string.Empty;
                            prjName = ToolDb.GetPrjName(prjName);
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            specType = "建设工程";
                            msgType = "河源市公共资源交易中心";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "河源市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return null;
        }
    }
}
