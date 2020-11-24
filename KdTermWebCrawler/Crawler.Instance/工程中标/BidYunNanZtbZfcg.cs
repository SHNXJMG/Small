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
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public  class BidYunNanZtbZfcg:WebSiteCrawller
    {
        public BidYunNanZtbZfcg()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "云南省公共资源政府采购中标信息";
            this.Description = "自动抓取云南省公共资源政府采购中标信息";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.ynggzy.net/bulletininfo.do?method=bulletinMore&hySort=1&bulletinclass=09";
            this.MaxCount = 400;
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch
            { 
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "bulletininfotable_toolbarTable")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("/", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    string bulletininfotable_totalpages = ToolHtml.GetHtmlInputValue(html, "bulletininfotable_totalpages");
                    string bulletininfotable_totalrows = ToolHtml.GetHtmlInputValue(html, "bulletininfotable_totalrows");
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "ec_i",
                    "bulletininfotable_efn",
                    "bulletininfotable_crd",
                    "bulletininfotable_p",
                    "bulletininfotable_s_bulletintitle",
                    "bulletininfotable_s_finishday",
                    "hySort",
                    "findAjaxZoneAtClient",
                    "method",
                    "bulletinclass",
                    "bulletininfotable_totalpages",
                    "bulletininfotable_totalrows",
                    "bulletininfotable_pg",
                    "bulletininfotable_rd"
                    },
                        new string[]{
                            "bulletininfotable",
                            "",
                            "20",
                            i.ToString(),
                            "",
                            "",
                            "1",
                            "false",
                            "bulletinMore",
                            "01",
                            bulletininfotable_totalpages,
                            bulletininfotable_totalrows,
                            (i-1).ToString(),
                            "5"
                        });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch {   continue; }
                }
                parser = new Parser(new Lexer(html.Replace("tbody", "table")));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "bulletininfotable_table_body")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;
                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[0].ToNodePlainString();
                        beginDate = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.ynggzy.net/bulletin.do?method=showbulletin&bulletin_id=" + tr.GetAttribute("id");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = ToolHtml.GetHtmlByUrl(this.SiteUrl, InfoUrl, Encoding.Default);
                        }
                        catch {   continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml(); 
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,<br />,<br/>,<br>", "\r\n").ToCtxString();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            code = bidCtx.GetCodeRegex();
                            bidType = prjName.GetInviteBidType();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("成交人,成交供应商");
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                bidUnit = bidCtx.GetRegexBegEnd("确定中标供应商为", "，");
                                if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                    bidMoney = bidCtx.GetRegexBegEnd("投标报价为", "万元");
                                if (string.IsNullOrWhiteSpace(bidUnit))
                                {
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList bidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                    if (bidNode != null && bidNode.Count > 0)
                                    {
                                        string ctx = string.Empty;
                                        TableTag tag = bidNode[0] as TableTag;
                                        if (tag.RowCount > 1)
                                        {
                                            for (int c = 0; c < tag.Rows[0].ColumnCount; c++)
                                            {
                                                try
                                                {
                                                    ctx += tag.Rows[0].Columns[c].ToNodePlainString() + "：";
                                                    ctx += tag.Rows[1].Columns[c].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                        }
                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrWhiteSpace(bidUnit))
                                            bidUnit = ctx.GetRegex("入围供应商,成交人,单位名称");
                                        if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                            bidMoney = ctx.GetMoneyRegex();
                                        if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                            bidMoney = ctx.GetMoneyString().GetMoney();
                                    }
                                }
                            }
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (bidUnit.Contains("地址"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("地址"));
                            try
                            {
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            specType = "政府采购";
                            msgType = "云南省公共资源交易中心";
                            BidInfo info = ToolDb.GenBidInfo("云南省", "云南省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                        string obj = a.Link.GetReplace("(", "（").GetRegexBegEnd("（", ",").GetReplace("（").GetReplace("'").Replace(",", "");
                                        string name = a.Link.GetReplace(")", "）").GetRegexBegEnd(",", "）").GetReplace("）").GetReplace("'").Replace(",", "");
                                        string link = "http://www.ynggzy.net/resource/bulletin.do?method=mdownloadFile&file_id=" + obj + "&file_name=" + name;
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
