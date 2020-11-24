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
    public class BidGuangXiGgzyZfcg : WebSiteCrawller
    {
        public BidGuangXiGgzyZfcg()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广西省公共资源交易中心中标信息(政府采购)";
            this.Description = "自动抓取广西省公共资源交易中心中标信息(政府采购)";
            this.PlanTime = "9:05,11:15,14:15,16:15,18:15";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.SiteUrl = "http://www.gxzbtb.cn/gxzbw/showinfo/jyxx.aspx?QuYu=450001&categoryNum=001004004";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            Dictionary<string, string> citys = this.GetCitys();
            foreach (string area in citys.Keys)
            {
                int count = 0;
                int pageInt = 1;
                string html = string.Empty;
                string viewState = string.Empty;
                string eventValidation = string.Empty;
                string cookiestr = string.Empty;
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(citys[area], Encoding.UTF8, ref cookiestr);
                }
                catch { return list; }
                Parser parser = new Parser(new Lexer(html));
                NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("nowrap", "true")));
                if (pageNode != null && pageNode.Count > 0)
                {
                    try
                    {
                        string temp = pageNode.AsString().GetRegexBegEnd("总页数", "当前页").Replace("：", "");
                        pageInt = int.Parse(temp);
                    }
                    catch { }
                }
                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        string viewSTATEGENERATOR = ToolHtml.GetHtmlInputValue(html, "__VIEWSTATEGENERATOR");
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                            "__VIEWSTATE",
                            "__VIEWSTATEGENERATOR",
                            "__EVENTTARGET",
                            "__EVENTARGUMENT",
                            "__EVENTVALIDATION",
                            "MoreInfoList1$txtTitle"
                        },
                            new string[] {
                                viewState,
                                viewSTATEGENERATOR,
                                "MoreInfoList1$Pager",
                                i.ToString(),
                                eventValidation,
                                ""
                            });
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(citys[area], nvc, Encoding.UTF8, ref cookiestr);
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "MoreInfoList1_DataGrid1")));
                    if (listNode != null && listNode.Count > 0)
                    {
                        TableTag table = listNode[0] as TableTag;
                        for (int j = 1; j < table.RowCount; j++)
                        {
                            string prjName = string.Empty,
                                 buildUnit = string.Empty, bidUnit = string.Empty,
                                 bidMoney = string.Empty, code = string.Empty,
                                 bidDate = string.Empty, beginDate = string.Empty,
                                 endDate = string.Empty, bidType = string.Empty,
                                 specType = string.Empty, InfoUrl = string.Empty,
                                 msgType = string.Empty, bidCtx = string.Empty,
                                 prjAddress = string.Empty, remark = string.Empty,
                                 prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            TableRow tr = table.Rows[j];
                            ATag aTag = tr.Columns[1].GetATag();
                            prjName = aTag.GetAttribute("title").GetReplace("【正在报名】,【报名结束】");
                            beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                            InfoUrl = "http://www.gxzbtb.cn" + aTag.Link;
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.AsHtml();
                                bidCtx = HtmlTxt.GetReplace(new string[] { "<br/>", "<br />", "<br>" }, "\r\n").ToCtxString();
                                prjAddress = bidCtx.GetAddressRegex();
                                buildUnit = bidCtx.GetBuildRegex();
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex();
                                prjMgr = bidCtx.GetMgrRegex();
                                code = bidCtx.GetCodeRegex().GetCodeDel();

                                if (string.IsNullOrEmpty(bidUnit))
                                {
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList bidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                    if (bidNode != null && bidNode.Count > 0)
                                    {
                                        string ctx = string.Empty;
                                        TableTag bidTable = bidNode[0] as TableTag;
                                        for (int r = 0; r < bidTable.RowCount; r++)
                                        {
                                            for (int c = 0; c < bidTable.Rows[r].ColumnCount; c++)
                                            {
                                                if ((c + 1) % 2 == 0)
                                                    ctx += bidTable.Rows[r].Columns[c].ToNodePlainString() + "\r\n";
                                                else
                                                    ctx += bidTable.Rows[r].Columns[c].ToNodePlainString() + "：";
                                            }
                                        }

                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                            bidMoney = ctx.GetMoneyString().GetMoney("万元");
                                        if (string.IsNullOrEmpty(prjAddress))
                                            prjAddress = ctx.GetAddressRegex();
                                        if (string.IsNullOrEmpty(buildUnit))
                                            buildUnit = ctx.GetBuildRegex();
                                        if (string.IsNullOrEmpty(code))
                                            code = ctx.GetCodeRegex().GetCodeDel();
                                        if (bidUnit.Contains("推荐") || bidUnit.Contains("中标") || bidUnit.Contains("地址"))
                                            bidUnit = string.Empty;
                                        if (string.IsNullOrEmpty(bidUnit))
                                        {
                                            if (bidTable.RowCount > 1)
                                            {
                                                ctx = string.Empty;
                                                for (int d = 0; d < bidTable.Rows[0].ColumnCount; d++)
                                                {
                                                    ctx += bidTable.Rows[0].Columns[d].ToNodePlainString() + "：";
                                                    try
                                                    {
                                                        ctx += bidTable.Rows[1].Columns[d].ToNodePlainString() + "\r\n";
                                                    }
                                                    catch { }
                                                }
                                                bidUnit = ctx.GetBidRegex();
                                                if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                                    bidMoney = ctx.GetMoneyString().GetMoney();
                                                if (string.IsNullOrEmpty(prjAddress))
                                                    prjAddress = ctx.GetAddressRegex();
                                                if (string.IsNullOrEmpty(buildUnit))
                                                    buildUnit = ctx.GetBuildRegex();
                                                if (string.IsNullOrEmpty(code))
                                                    code = ctx.GetCodeRegex().GetCodeDel();
                                            }
                                        }
                                    }
                                }
                                try
                                {
                                    if (decimal.Parse(bidMoney) > 10000)
                                        bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                                }
                                catch { }
                                bidUnit = bidUnit.Replace("名称", "").Replace("单位", "").Replace("№", "").Replace("1", "").Replace("2", "").Replace("联合体", "").Replace("（", "");

                                if (bidUnit.Contains("公司"))
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                                if (bidUnit.Contains("研究院"))
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("研究院")) + "研究院";
                                if (bidUnit.Contains("研究所"))
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("研究所")) + "研究所";
                                bidType = "水利工程";
                                specType = "建设工程";
                                msgType = "广西壮族自治区公共资源交易中心";
                                BidInfo info = ToolDb.GenBidInfo("广西壮族自治区", "广西壮族自治区及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                count++;
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
                                                link = "http://www.gxzbtb.cn/" + a.Link.GetReplace("../,./");
                                            if (Encoding.Default.GetByteCount(link) > 500)
                                                continue;
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }
                                if (!crawlAll && count >= this.MaxCount) goto Funcs;
                            }
                        }
                    }
                }
                Funcs:;
            }
            return list;
        }

        protected Dictionary<string, string> GetCitys()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { }
            Parser parser = new Parser(new Lexer(html));
            NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hubs")), true), new TagNameFilter("li")));

            if (listNode != null && listNode.Count > 0)
            {
                for (int i = 0; i < listNode.Count; i++)
                {
                    Bullet node = listNode[i] as Bullet;
                    string id = node.GetAttribute("id");
                    string city = node.ToNodePlainString();
                    string url = string.Format("http://www.gxzbtb.cn/gxzbw/showinfo/MoreInfo.aspx?QuYu={0}&categoryNum=001004004", id);
                    dic.Add(city, url);
                }
            }
            return dic;
        }

    }
}
