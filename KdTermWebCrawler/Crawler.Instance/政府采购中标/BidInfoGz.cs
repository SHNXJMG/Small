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
using System.IO;
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class BidInfoGz : WebSiteCrawller
    {
        public BidInfoGz()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "广州市政府采购网";
            this.Description = "自动抓取广州是政府采购网中标信息";
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,10:20,10:50,11:30,14:05,14:25,14:50,15:25,16:00,16:50,19:00";
            this.SiteUrl = "http://gzg2b.gzfinance.gov.cn/Sites/_Layouts/ApplicationPages/News/News.aspx?ColumnID=F795A469-6447-4A61-91E0-ACF97FB82D6C";
            this.MaxCount = 50000;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagerbtn")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].GetATagValue("href").Replace("javascript", "").Replace("__doPostBack", "").Replace(":", "").Replace("(", "").Replace("ctl00$main$pager", "").Replace("ctl00$main$pagerHeader", "").Replace(")", "").Replace("'", "").Replace(",", "");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 2; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    try
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                            new string[]{
                            "__WPPS",
                            "__EVENTTARGET",
                            "__EVENTARGUMENT",
                            "__VIEWSTATE",
                            "ctl00$hfGlobalToolbar",
                            "ctl00$main$pagerHeader_input",
                            "ctl00$main$pager_input"
                            },
                            new string[]{
                            "u",
                            "ctl00$main$pagerHeader",
                            i.ToString(),
                            viewState,
                            "","1","1"
                            }
                            );
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "list3")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j =1; j < nodeList.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        prjName = nodeList[j].GetATagValue("title");
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://gzg2b.gzfinance.gov.cn" + nodeList[j].GetATagHref();
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = ToolWeb.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "note_container")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.ToHtml();



                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList dtlNode1 = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoTableGrid")));
                            if (dtlNode1 != null && dtlNode1.Count > 0)
                            {
                                TableTag table = dtlNode1[0] as TableTag;
                                for (int n = 1; n < table.RowCount; n++)
                                {
                                    TableRow tr = table.Rows[n];
                                    ATag aTag = tr.GetATag();
                                    bidUnit = tr.Columns[0].ToPlainTextString().GetReplace("\r\n","");
                                    prjAddress= tr.Columns[2].ToPlainTextString().GetReplace("\r\n", "");
                                }
                             }


                                bidCtx = HtmlTxt.ToCtxString().Replace("&rdquo;", "").Replace("&ldquo;", "");
                            buildUnit = bidCtx.GetRegexBegEnd("采购人：", "地址");
                            if (string.IsNullOrEmpty(buildUnit))
                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex().Replace("中标公告", "").Replace("）","").Replace(")","").Replace("(","").Replace("（","");

                            if (string.IsNullOrEmpty(prjAddress))
                                prjAddress = bidCtx.GetRegex("地址");
                            if (string.IsNullOrEmpty(prjAddress))
                            prjAddress = bidCtx.GetAddressRegex();

                            if (string.IsNullOrEmpty(prjMgr))
                                prjMgr = bidCtx.GetMgrRegex();

                            bidType = prjName.GetInviteBidType();

                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegexBegEnd("中标、成交供应商名称：", "；");
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegexBegEnd("中标供应商名称", "法人");

                            bidMoney = bidCtx.GetRegexBegEnd("中标、成交金额（万元）：", "；");
                            if (string.IsNullOrEmpty(bidMoney))
                            bidMoney = bidCtx.Replace("，","").Replace(",","").GetRegexBegEnd("中标、成交金额（万元）：", "；").GetMoneyRegex(null,true);
                            if (bidMoney != "0" && float.Parse(bidMoney) > 10000)
                            {
                                bidMoney = bidCtx.Replace("，", "").Replace(",", "").GetMoneyRegex(null, true, "万元");
                            }
                            if (bidMoney != "0" && float.Parse(bidMoney) > 10000)
                            {
                                bidMoney = (decimal.Parse(bidMoney) / 10000).ToString(); ;
                            }
                            specType = "政府采购";
                            msgType = "广州市政府采购网";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "广州政府采购", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
