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
    public class BidZhaoqing : WebSiteCrawller
    {
        public BidZhaoqing()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省肇庆市";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.Description = "自动抓取广东省肇庆市区中标信息";
            this.SiteUrl = "http://ggzy.zhaoqing.gov.cn/zqfront/showinfo/moreinfolist.aspx?categorynum=003001003&Paging=";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "1", Encoding.UTF8);
            }
            catch
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "paging"), new TagNameFilter("div")));
            if (sNode != null && sNode.Count > 0)
            {
                string temp = sNode[0].ToNodePlainString();
                try
                {
                    temp = temp.GetRegexBegEnd("/", "转到");
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
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + i, Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }

                parser = new Parser(new Lexer(html));
                sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasAttributeFilter("class", "column-info-list"), new TagNameFilter("div")), true), new TagNameFilter("li")));
                if (sNode != null && sNode.Count > 0)
                {
                    for (int t = 0; t < sNode.Count; t++)
                    {

                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty,
                            code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                            bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty,
                            bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty,
                            otherType = string.Empty, HtmlTxt = string.Empty;                       
                        ATag aTag = sNode[t].GetATag();
                        prjName = aTag.LinkText.ToNodeString();
                        InfoUrl = "http://ggzy.zhaoqing.gov.cn" + aTag.Link;
                        beginDate = sNode[t].ToPlainTextString().GetDateRegex();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();

                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();

                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                dtlparser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("border", "1")));
                                if (tableNode == null || tableNode.Count < 1)
                                {
                                    dtlparser.Reset();
                                    tableNode = dtlparser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                }
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag table = tableNode[0] as TableTag;
                                    if (table.Rows[0].ColumnCount >= 2)
                                    {
                                        for (int j = 1; j < table.RowCount; j++)
                                        {
                                            ctx += table.Rows[j].Columns[0].ToNodePlainString() + "：";
                                            ctx += table.Rows[j].Columns[1].ToNodePlainString() + "\r\n";
                                        }
                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrWhiteSpace(bidUnit))
                                            bidUnit = ctx.GetRegex("单位名称,第一中标候选人");
                                        bidMoney = ctx.GetMoneyRegex();
                                        prjMgr = ctx.GetMgrRegex();
                                    }

                                }
                            }
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            code = bidCtx.GetCodeRegex();

                            msgType = "肇庆市公共资源交易中心";
                            specType = "建设工程";

                            prjName = ToolDb.GetPrjName(prjName);
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "肇庆市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            dtlparser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = dtlparser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag fileTag = aNode[a] as ATag;
                                    if (fileTag.IsAtagAttach())
                                    {
                                        string url = string.Empty;
                                        if (fileTag.Link.Contains("http"))
                                            url = fileTag.Link;
                                        else
                                        {
                                            url = this.SiteUrl + beginDate.GetReplace("-").Substring(0, 6) + fileTag.Link.GetReplace("./", "/");
                                        }
                                        BaseAttach item = ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, url);

                                        base.AttachList.Add(item);
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
