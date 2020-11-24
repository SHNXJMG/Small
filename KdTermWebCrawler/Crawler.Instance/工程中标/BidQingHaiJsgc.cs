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
    public class BidQingHaiJsgc : WebSiteCrawller
    {
        public BidQingHaiJsgc()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "青海省建设工程招标交易网中标信息";
            this.Description = "自动抓取青海省建设工程招标交易网中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 300;
            this.SiteUrl = "http://www.qhbid.com.cn/www/zbgs.asp";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "693")));
            if (pageNode != null && pageNode.Count > 0)
            {
                TableTag table = pageNode[pageNode.Count - 1] as TableTag;
                try
                {
                    ATag node = table.Rows[table.RowCount - 1].Columns[1].GetATag(1);
                    string temp = node.GetAttribute("href").GetReplace("/www/zbgs.asp?qyfl=%&qydz=%&qymc=%&native_place=&post_title=&polity_identity=&bz=&act=&typeid=&curyear=&query_like=&query_like_logic=&query_like_input=&query_logic_1=&query_logic_2=&query_ct_type=&query_date=&query_date_logic=''&queryyear=&querymonth=&queryday=&curpagenum=");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?curpagenum=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "693")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[listNode.Count - 1] as TableTag;
                    for (int j = 0; j < table.RowCount - 1; j++)
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
                            prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.LinkText;
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex("yyyy年MM月dd日");
                        InfoUrl = "http://www.qhbid.com.cn/www/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "con")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml().GetReplace("</p>,<br/>", "\r\n");
                            bidCtx = HtmlTxt.ToCtxString();
                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("（1）", false);
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetReplace("中标候选人：\r\n", "中标候选人：").GetRegex("中标候选人");
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetRegex("中标金（人民币）", false).GetMoney();
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrEmpty(prjMgr))
                                prjMgr = bidCtx.GetRegex("项目负责人（建造师）,建造师,项目经理");
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            bidType = specType = "建设工程";
                            msgType = "青海省建设工程招标投标管理办公室";
                            BidInfo info = ToolDb.GenBidInfo("青海省", "青海省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.qhbid.com.cn/" + a.Link.GetReplace("../,./");
                                        if (Encoding.Default.GetByteCount(link) > 500)
                                            continue;
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
