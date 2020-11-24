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
    public class BidQuanGuoGGZYJYPT : WebSiteCrawller
    {
        public BidQuanGuoGGZYJYPT()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "全国公共资源交易平台";
            this.Description = "自动抓取全国公共资源交易平台中标信息";
            this.PlanTime = "16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://deal.ggzy.gov.cn/ds/deal/dealList.jsp";
            this.MaxCount = 50;
        }
        protected string[] BidUnitFamt
        {
            get
            {
                return new string[] { "第一中标（选）候选人", "中标候选人为", "中标候选人名称", "第1名候选人", "拟确定中标人", "第一名", "第1名", "中标单位", "中标企业", "成交单位", "中标人名称", "中标人", "中标方", "中标商", "成交供应商名称", "中标单位名称", "成交供应商", "成交商", "服务商名称", "投标报名人", "供应商名称", "投标人名称", "中标单位为", "第一中标后选人单位", "第一候选人", "第一中标候选人", "中标供应商为", "中标供应商名称", "中标（成交）供应商名称", "中标候选单位", "中标供应商名称（包一）", "中标（成交）候选人", "成交单位名称", "成交供应商为", "中标（成交）人名称", "中标候选人", "投标供应商", "拟定中标人候选人", "第一中签人", "第一中标侯选人", "中标供应商", "中标人名称", "中标（成交）单位名称", "单位名称", "成交候选人" };
            }
        }

        protected string[] BidMoenyFamt
        {
            get
            {
                return new string[] { "报价", "中标金额", "中标（成交）金额人民币", "中标金额为", "预中标金额", "成交金额", "中标总金额", "中标（成交）金额", "中标价", "中标金额", "中标金额", "成交总金额", "中标金额（包一）", "投标价", "其中标价为", "投标报价", "预中标人", "总投资", "发包价", "投标报价", "中标标价", "承包标价", "价格", "金额", "总价", "承包价", "中标造价", "采购控制价" };
            }
        }

        protected string[] PrjMgrFamt
        {
            get
            {
                return new string[] { "项目经理姓名", "项目经理及注册编号", "项目经理（或建造师）", "项目经理", "项目负责人", "项目总监", "建造师", "总工程师", "监理师", "建造师（总监）", "项目总监理工程师", "总监理工程师", "项目经理（总监）名称", "注册建造师（项目经理）姓名及等级", "项目经理或总监或者首席设计师或技术负责人", "项目总监理工程师", "项目负责人(项目经理)", "负责人", "项目经理姓名称" };
            }
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            DateTime startDate = DateTime.Today;
            DateTime endDates = startDate.AddDays(-90);
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                        "TIMEBEGIN_SHOW",
                        "TIMEEND_SHOW",
                        "TIMEBEGIN",
                        "TIMEEND",
                        "DEAL_TIME",
                        "DEAL_CLASSIFY",
                        "DEAL_STAGE",
                        "DEAL_PROVINCE",
                        "DEAL_CITY",
                        "DEAL_PLATFORM",
                        "DEAL_TRADE",
                        "isShowAll",
                        "PAGENUMBER",
                        "FINDTXT"

                    }, new string[] {
                        endDates.ToString(),
                        startDate.ToString(),
                        endDates.ToString(),
                        startDate.ToString(),
                        "05",
                        "01",
                        "0104",
                        "0",
                        "0",
                        "0",
                        "0",
                        "1",
                        "1",
                        ""
                    });
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                }
                catch { }

            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "paging")), true), new TagNameFilter("span")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("共", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {

                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                        "TIMEBEGIN_SHOW",
                        "TIMEEND_SHOW",
                        "TIMEBEGIN",
                        "TIMEEND",
                        "DEAL_TIME",
                        "DEAL_CLASSIFY",
                        "DEAL_STAGE",
                        "DEAL_PROVINCE",
                        "DEAL_CITY",
                        "DEAL_PLATFORM",
                        "DEAL_TRADE",
                        "isShowAll",
                        "PAGENUMBER",
                        "FINDTXT"

                    }, new string[] {
                        endDates.ToString(),
                        startDate.ToString(),
                        endDates.ToString(),
                        startDate.ToString(),
                        "05",
                        "01",
                        "0104",
                        "0",
                        "0",
                        "0",
                        "0",
                        "1",
                        i.ToString(),
                        ""
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }

                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "publicont")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {

                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty, prov = string.Empty, ctxBid = string.Empty;
                        string span_ly = string.Empty;
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;
                        parser = new Parser(new Lexer(node.ToHtml()));
                        NodeList txtNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "span_on")));
                        if (txtNode != null && txtNode.Count >= 4)
                        {
                            prov = txtNode[0].ToNodePlainString();
                            span_ly = txtNode[1].ToNodePlainString();
                        }

                        prjName = aTag.GetAttribute("title");
                        if (prov.Contains("广东"))
                            continue;
                        //if (!prjName.Contains("临夏现代职业学院信息化服务平台建设项目（一期）中标公告"))
                        //    continue;
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link.GetReplace("information/html/a/", "information/html/b/");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList ctxNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "mycontent")));
                        if (ctxNode != null && ctxNode.Count > 0)
                        {
                            ctxBid = ctxNode.AsHtml().Replace("</span>", "\r\n").ToCtxString();
                            if (span_ly.Contains("阿坝州公共资源交易中心"))
                            {

                                parser = new Parser(new Lexer(htmldtl));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    ATag fileTag = aNode[0].GetATag();
                                    if (fileTag.Link.Contains("http"))
                                        InfoUrl = fileTag.Link;
                                }
                                try
                                {
                                    htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                                    parser = new Parser(new Lexer(htmldtl));
                                }
                                catch { continue; }
                            }
                            if (prov.Contains("江苏"))
                            {
                                ctxBid = ctxNode.AsHtml().Replace("</span>", "\r\n").Replace("</tr>", "\r\n").ToCtxString();

                                try
                                {
                                    parser = new Parser(new Lexer(htmldtl));
                                    NodeList mrjNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("SPAN"), new HasAttributeFilter("lang", "EN-US")));
                                    if (mrjNode != null && mrjNode.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(prjMgr))
                                            prjMgr = mrjNode[5].ToNodePlainString();
                                        if (string.IsNullOrEmpty(prjMgr) || prjMgr.Contains("."))
                                            prjMgr = mrjNode[2].ToNodePlainString();
                                    }
                                }
                                catch { }
                                try
                                {
                                    parser = new Parser(new Lexer(htmldtl));
                                    NodeList mrjsNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "lblPName")));
                                    if (mrjsNode != null && mrjsNode.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(prjMgr))
                                            prjMgr = mrjsNode[0].ToNodePlainString();
                                    }
                                }
                                catch { }
                                try
                                {
                                    parser = new Parser(new Lexer(htmldtl));
                                    NodeList mrjxsNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("td"));
                                    if (mrjxsNode != null && mrjxsNode.Count > 0)
                                    {
                                        if (mrjxsNode[25].ToNodePlainString().Contains("项目负责人"))
                                        {
                                            if (string.IsNullOrEmpty(prjMgr))
                                                prjMgr = mrjxsNode[26].ToNodePlainString();
                                        }
                                        if (mrjxsNode[26].ToNodePlainString().Contains("项目负责人"))
                                        {
                                            if (string.IsNullOrEmpty(prjMgr))
                                                prjMgr = mrjxsNode[27].ToNodePlainString();
                                        }
                                        if (mrjxsNode[54].ToNodePlainString().Contains("项目负责人"))
                                        {
                                            if (string.IsNullOrEmpty(prjMgr))
                                                prjMgr = mrjxsNode[55].ToNodePlainString();
                                        }
                                        if (mrjxsNode[36].ToNodePlainString().Contains("项目负责人"))
                                        {
                                            if (string.IsNullOrEmpty(prjMgr))
                                                prjMgr = mrjxsNode[37].ToNodePlainString();
                                        }
                                        if (mrjxsNode[37].ToNodePlainString().Contains("项目经理"))
                                        {
                                            if (string.IsNullOrEmpty(prjMgr))
                                                prjMgr = mrjxsNode[43].ToNodePlainString();
                                        }
                                    }
                                }
                                catch { }
                                try
                                {
                                    parser = new Parser(new Lexer(htmldtl));
                                    NodeList mrjxsNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("tb"), new HasAttributeFilter("class", "s58613d53")));
                                    if (mrjxsNode != null && mrjxsNode.Count > 0)
                                    {
                                        prjMgr = mrjxsNode[13].ToNodePlainString();
                                    }
                                }
                                catch { }
                            }
                            if (prov.Contains("甘肃"))
                            {
                                parser.Reset();
                                NodeList msgSourceNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("label"), new HasAttributeFilter("id", "platformName")));
                                if (msgSourceNode != null && msgSourceNode.Count > 0)
                                {
                                    string temp = msgSourceNode[0].ToNodePlainString();
                                    if (temp.Contains("白银市"))
                                    {
                                        continue;
                                    }
                                }
                                if (!ctxBid.Contains("项目"))
                                {
                                    string urls = string.Empty, url = string.Empty, htmUrl = string.Empty;
                                    parser = new Parser(new Lexer(htmldtl));
                                    NodeList mrjxNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "detail_content")));
                                    if (mrjxNode != null && mrjxNode.Count > 0)
                                    {
                                        urls = mrjxNode.AsHtml();
                                        url = urls.GetRegexBegEnd("src=", "></iframe>");
                                        htmUrl = url.GetRegexBegEnd("\"", "\"");
                                    }
                                    try
                                    {
                                        htmldtl = this.ToolWebSite.GetHtmlByUrl(htmUrl).GetJsString();
                                    }
                                    catch { continue; }
                                    parser = new Parser(new Lexer(htmldtl));
                                    NodeList ctxxNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "Html_Div")));
                                    if (ctxxNode != null && ctxxNode.Count > 0)
                                    {
                                        ctxBid = ctxxNode.AsHtml().Replace("</span>", "\r\n").ToCtxString();
                                    }
                                }
                                if (prjMgr.Contains("试验检测") || prjMgr.Contains("安全生产") || prjMgr.Contains("建设单位"))
                                {
                                    prjMgr = "";
                                }
                            }
                        }
                        if (prov.Contains("浙江"))
                        {
                            parser.Reset();
                            NodeList msgSourceNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("label"), new HasAttributeFilter("id", "platformName")));
                            if (msgSourceNode != null && msgSourceNode.Count > 0)
                            {
                                string temp = msgSourceNode[0].ToNodePlainString();
                                if (temp.Contains("嘉兴市"))
                                {
                                    parser.Reset();
                                    NodeList aNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new TagNameFilter("body"), true), new TagNameFilter("a")));
                                    if (aNodes != null && aNodes.Count > 0)
                                    {
                                        string urls = aNodes[0].GetATagHref();
                                        try
                                        {
                                            htmldtl = this.ToolWebSite.GetHtmlByUrl(urls).GetJsString();
                                            NodeList dtlNodes = new Parser(new Lexer(htmldtl)).ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "container mt10")));
                                            parser = new Parser(new Lexer("<body>" + dtlNodes.AsHtml() + "</body>"));
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                        parser.Reset();
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode[0].ToHtml();//System.Web.HttpUtility.HtmlDecode(dtlNode.AsHtml()).Replace(" ", "");
                            string bidCtx2 = HtmlTxt.ToLower().Replace("div", "span").Replace("font", "span").Replace("td", "span").GetReplace("<br>,<br/>,<BR>,</p>,</tr>,</span>", "\r\n");
                            bidCtx2 = bidCtx2.ToCtxString().Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\t\r\n\t", "\r\n");

                            bidCtx = HtmlTxt.ToLower().Replace("div", "span").Replace("font", "span").Replace("td", "span").GetReplace("<br>,<br/>,<BR>,</p>,</tr>", "\r\n");
                            bidCtx = bidCtx.ToCtxString().Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\t\r\n\t", "\r\n");


                            bidCtx = this.GetBidUnitCtx(bidCtx, BidUnitFamt);
                            bidCtx = this.GetBidMoneyCtx(bidCtx, BidMoenyFamt);
                            bidCtx = this.GetPrjMgrCtx(bidCtx, PrjMgrFamt);

                            bidCtx2 = this.GetBidUnitCtx(bidCtx2, BidUnitFamt);
                            bidCtx2 = this.GetBidMoneyCtx(bidCtx2, BidMoenyFamt);
                            bidCtx2 = this.GetPrjMgrCtx(bidCtx2, PrjMgrFamt);
                            if (string.IsNullOrWhiteSpace(code))
                                code = bidCtx.GetCodeRegex();
                            if (string.IsNullOrWhiteSpace(buildUnit))
                                buildUnit = bidCtx.GetBuildRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetBidRegex(BidUnitFamt, true);
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetBidRegex(BidUnitFamt, false);
                           
                            if (bidUnit.Contains("公示"))
                                bidUnit = bidCtx.GetRegexBegEnd("中标单位", "\r\n");
                            if (bidUnit.Contains("招标代理"))
                                bidUnit = null;
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                bidUnit = bidCtx2.GetBidRegex(BidUnitFamt, true);
                                if (string.IsNullOrWhiteSpace(bidUnit))
                                    bidUnit = bidCtx2.GetBidRegex(BidUnitFamt, false);
                                if (bidUnit.Contains("公示"))
                                    bidUnit = bidCtx2.GetRegexBegEnd("中标单位", "\r\n");
                            }
                            bidUnit = GetBidUnitName(bidUnit);

                            string ctx = this.GetTableCtx(HtmlTxt);
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                bidUnit = ctx.GetBidRegex(BidUnitFamt, true);
                                if (string.IsNullOrWhiteSpace(bidUnit))
                                    bidUnit = ctx.GetBidRegex(BidUnitFamt, false);
                                if (bidUnit.Contains("公示"))
                                    bidUnit = ctx.GetRegexBegEnd("中标单位", "\r\n");
                            }
                            bidUnit = GetBidUnitName(bidUnit);
                            bidMoney = bidCtx.GetMoneyRegex(BidMoenyFamt, false, "万元");

                            if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetMoneyString(BidMoenyFamt).GetMoney("万元");
                            if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetRegexBegEnd("中标金额", "\r\n").GetMoney("万元");
                            if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                            {
                                bidMoney = bidCtx2.GetMoneyRegex(BidMoenyFamt, false, "万元");
                                if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                    bidMoney = bidCtx2.GetMoneyString(BidMoenyFamt).GetMoney("万元");
                                if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                    bidMoney = bidCtx2.GetRegexBegEnd("中标金额", "\r\n").GetMoney("万元");
                            }


                            if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                            {
                                bidMoney = ctx.GetMoneyRegex(BidMoenyFamt, false, "万元");
                                if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                    bidMoney = ctx.GetMoneyString(BidMoenyFamt).GetMoney("万元");
                                if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                    bidMoney = ctx.GetRegexBegEnd("中标金额", "\r\n").GetMoney("万元");
                            }
                            if (string.IsNullOrEmpty(prjMgr))
                                prjMgr = bidCtx.GetMgrRegex(PrjMgrFamt);
                            if (string.IsNullOrEmpty(prjMgr))
                                prjMgr = bidCtx.GetMgrRegex(PrjMgrFamt, false);

                            if (string.IsNullOrWhiteSpace(prjMgr))
                            {
                                prjMgr = bidCtx2.GetMgrRegex(PrjMgrFamt);
                                if (string.IsNullOrEmpty(prjMgr))
                                    prjMgr = bidCtx2.GetMgrRegex(PrjMgrFamt, false);
                            }

                            prjMgr = GetPrjMgrName(prjMgr);
                            if (string.IsNullOrWhiteSpace(prjMgr))
                            {
                                prjMgr = ctx.GetMgrRegex(PrjMgrFamt);
                                if (string.IsNullOrEmpty(prjMgr))
                                    prjMgr = ctx.GetMgrRegex(PrjMgrFamt, false);
                            }

                            prjMgr = GetPrjMgrName(prjMgr);
                            if (bidUnit.Contains("公司名称"))
                                bidUnit = bidCtx.GetRegex("公司名称");
                            if (bidUnit.Contains("公示变更") || bidUnit.IsNumber())
                                bidUnit = "";
                            bidType = prjName.GetInviteBidType();

                            bidUnit = bidUnit.Replace("名称", "");
                            if (bidUnit.Contains("投标"))
                                bidUnit = "";
                            specType = "建设工程";
                            msgType = "国家信息中心";

                            if (buildUnit.Contains("运输局"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("运输局")) + "运输局";
                            if (buildUnit.Contains("管理局"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("管理局")) + "管理局";
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                            if (prjName.Length >= 300)
                            {
                                Logger.Error(prjName);
                                continue;
                            }
                            if (bidUnit.Contains("招标代理") || bidUnit.Contains("无效") || bidUnit.Contains("负责人"))
                                bidUnit = "";
                            string saveCtx = ctxBid ?? HtmlTxt.ToCtxString();
                            string[] provs = this.GetPrivoce(prov);
                            BidInfo info = ToolDb.GenBidInfo(provs[0], provs[1], area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, saveCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag fileTag = aNode[a].GetATag();
                                    if (fileTag.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (fileTag.Link.Contains("http"))
                                            link = fileTag.Link;
                                        else
                                            link = "http://deal.ggzy.gov.cn/" + fileTag.Link;
                                        base.AttachList.Add(ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, link));
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

        protected string GetPrjMgrName(string prjMgr)
        {

            if (prjMgr.Contains("级别"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("级别"));
            if (prjMgr.Contains("抽取"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("抽取"));
            if (prjMgr.Contains("证号"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("证号"));
            if (prjMgr.Contains("专业"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("专业"));
            if (prjMgr.Contains("联系"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("联系"));
            if (prjMgr.Contains("地址"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("地址"));
            if (prjMgr.Contains("二级"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("二级"));
            if (prjMgr.Contains("商务"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("商务"));
            if (prjMgr.Contains("职称"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("职称"));
            if (prjMgr.Contains("投标"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("投标"));
            if (prjMgr.Contains("等级"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("等级"));
            if (prjMgr.Contains("技术"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("技术"));
            if (prjMgr.Contains("证书"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("证书"));
            if (prjMgr.Contains("中标"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("中标"));
            if (prjMgr.Contains("报价"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("报价"));
            if (prjMgr.Contains("()"))
                prjMgr = prjMgr.Replace("()", "");
            if (prjMgr.Contains("（）"))
                prjMgr = prjMgr.Replace("（）", "");
            if (prjMgr.Contains("（总监、）"))
                prjMgr = prjMgr.Replace("（总监、）", "");
            if (prjMgr.Contains("（姓名）"))
                prjMgr = prjMgr.Replace("（姓名）", "");
            if (prjMgr.Contains("姓名"))
                prjMgr = prjMgr.Replace("姓名", "");
            if (prjMgr.Contains("得分"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("得分"));
            if (prjMgr.Contains("投诉"))
                prjMgr = "";
            if (prjMgr.Contains("资格"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("资格"));
            if (prjMgr.Contains("有效"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("有效"));
            if (prjMgr.Contains("资质"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("资质"));
            if (prjMgr.Contains("注册"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("注册"));
            if (prjMgr.Contains("工程"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("工程"));
            if (prjMgr.Contains("工期"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("工期"));
            if (prjMgr.Contains("）"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("）"));
            if (prjMgr.Contains("（"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("（"));
            if (prjMgr.Contains(")"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf(")"));
            if (prjMgr.Contains("("))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("("));
            if (prjMgr.Contains("["))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("["));
            if (prjMgr.Contains("　"))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf("　"));
            if (prjMgr.Contains(" "))
                prjMgr = prjMgr.Remove(prjMgr.IndexOf(" "));
            if (prjMgr.Length == 1 || prjMgr.Contains("资质") || prjMgr.Contains("综合") || prjMgr.Contains("(设总)") || prjMgr.Contains("(设总)") || prjMgr.Contains("执业") || prjMgr.Contains("项目"))
                prjMgr = "";
            if (prjMgr.StartsWith("第"))
                prjMgr = "";
            if (prjMgr.StartsWith("企业"))
                prjMgr = "";
            if (prjMgr.StartsWith("建筑"))
                prjMgr = "";
            if (prjMgr.StartsWith("业绩"))
                prjMgr = "";
            if (prjMgr.StartsWith("招标"))
                prjMgr = "";
            if (prjMgr.StartsWith("1"))
                prjMgr = "";
            if (prjMgr.StartsWith("2"))
                prjMgr = "";
            if (prjMgr.StartsWith("/"))
                prjMgr = "";
            if (prjMgr.StartsWith("证号"))
                prjMgr = "";
            if (prjMgr.StartsWith("设计"))
                prjMgr = "";
            if (prjMgr.StartsWith("名称"))
                prjMgr = "";
            if (prjMgr.StartsWith("暂估价"))
                prjMgr = "";
            if (prjMgr.StartsWith("货物"))
                prjMgr = "";
            return prjMgr;
        }

        protected string GetBidUnitName(string bidUnit)
        {
            if (bidUnit.Contains("施工单位"))
                bidUnit = bidUnit.Replace("施工单位", "");
            if (bidUnit.StartsWith("情况"))
                bidUnit = "";
            if (bidUnit.Contains("中标"))
                bidUnit = "";
            if (bidUnit.Contains("投标"))
                bidUnit = "";
            if (bidUnit.Contains("排序"))
                bidUnit = "";
            if (bidUnit.Contains("候选"))
                bidUnit = "";
            if (bidUnit.Contains("公示"))
                bidUnit = "";
            if (bidUnit.StartsWith("项目"))
                bidUnit = "";
            if (bidUnit.StartsWith("总监"))
                bidUnit = "";
            if (bidUnit.StartsWith("第"))
                bidUnit = "";
            if (bidUnit.StartsWith("企业"))
                bidUnit = "";
            if (bidUnit.StartsWith("中标"))
                bidUnit = "";
            if (bidUnit.StartsWith("推荐"))
                bidUnit = "";
            if (bidUnit.StartsWith("公告"))
                bidUnit = "";
            if (bidUnit.StartsWith("评标"))
                bidUnit = "";
            if (bidUnit.StartsWith("公告"))
                bidUnit = "";
            if (bidUnit.StartsWith("名单"))
                bidUnit = "";
            if (bidUnit.StartsWith("报价"))
                bidUnit = "";
            if (bidUnit.StartsWith("地址"))
                bidUnit = "";
            if (bidUnit.StartsWith("排名"))
                bidUnit = "";
            if (bidUnit.StartsWith("("))
                bidUnit = "";
            if (bidUnit.StartsWith("（"))
                bidUnit = "";
            if (bidUnit.StartsWith("联合体"))
                bidUnit = "";
            if (bidUnit.StartsWith("名次"))
                bidUnit = "";
            if (bidUnit.StartsWith("拟定"))
                bidUnit = "";
            if (bidUnit.Contains("："))
                bidUnit = "";

            return bidUnit;
        }

        protected string GetPrjMgrCtx(string htmlCtx, params string[] param)
        {

            StringBuilder sb = new StringBuilder();
            string[] fomats = new string[] { "：", ":", "" };
            foreach (string str in param)
            {
                foreach (string fomat in fomats)
                {
                    sb.AppendFormat("{0}{1}\r\n,", str, fomat);
                }
            }
            sb.Remove(sb.Length - 1, 1);
            return htmlCtx.GetReplace(sb.ToString(), "项目经理：");
            //sb = new StringBuilder();
            //foreach (string str in param)
            //{
            //    foreach (string fomat in fomats)
            //    {
            //        sb.AppendFormat("{0}{1},", str, fomat);
            //    }
            //}
            //sb.Remove(sb.Length - 1, 1);
            //return htmlCtx.GetReplace(sb.ToString(), "项目经理：");
        }

        /// <summary>
        /// 处理中标金额
        /// </summary>
        /// <param name="htmlCtx"></param>
        /// <returns></returns>
        protected string GetBidMoneyCtx(string htmlCtx, params string[] param)
        {
            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            string[] moneys1 = new string[] { "（万元）", "(万元)", "（人民币：万元）", "(人民币：万元)" };
            string[] moneys2 = new string[] { "（元）", "(元)", "（人民币：元）", "(人民币：元)", "（人民币）", "(人民币)", "" };
            string[] fomats = new string[] { "：", ":", "" };
            foreach (string str in param)
            {
                foreach (string money in moneys1)
                {
                    foreach (string fomat in fomats)
                    {
                        sb1.AppendFormat("{0}{1}{2}\r\n,", str, money, fomat);
                    }
                }
            }
            sb1.Remove(sb1.Length - 1, 1);
            htmlCtx = htmlCtx.GetReplace(sb1.ToString(), "中标金额（万元）");
            foreach (string str in param)
            {
                foreach (string money in moneys2)
                {
                    foreach (string fomat in fomats)
                    {
                        sb2.AppendFormat("{0}{1}{2}\r\n,", str, money, fomat);
                    }
                }
            }
            sb2.Remove(sb2.Length - 1, 1);
            return htmlCtx.GetReplace(sb2.ToString(), "中标金额：");

            //sb1 = new StringBuilder();
            //foreach (string str in param)
            //{
            //    foreach (string money in moneys1)
            //    {
            //        foreach (string fomat in fomats)
            //        {
            //            sb1.AppendFormat("{0}{1}{2},", str, money, fomat);
            //        }
            //    }
            //}
            //sb1.Remove(sb1.Length - 1, 1);
            //htmlCtx = htmlCtx.GetReplace(sb1.ToString(), "中标金额（万元）");

            //sb2 = new StringBuilder();
            //foreach (string str in param)
            //{
            //    foreach (string money in moneys2)
            //    {
            //        foreach (string fomat in fomats)
            //        {
            //            sb2.AppendFormat("{0}{1}{2},", str, money, fomat);
            //        }
            //    }
            //}
            //sb2.Remove(sb2.Length - 1, 1);

            //return htmlCtx.GetReplace(sb2.ToString(), "中标金额：");
        }
        /// <summary>
        /// 处理中标单位
        /// </summary>
        /// <param name="htmlCtx"></param>
        /// <returns></returns>
        protected string GetBidUnitCtx(string htmlCtx, params string[] param)
        {
            StringBuilder sb = new StringBuilder();
            string[] fomats = new string[] { "：", ":", "" };
            foreach (string str in param)
            {
                foreach (string fomat in fomats)
                {
                    sb.AppendFormat("{0}{1}\r\n,", str, fomat);
                }
            }
            sb.Remove(sb.Length - 1, 1);
            return htmlCtx.GetReplace(sb.ToString(), "中标单位：");
            //sb = new StringBuilder();
            //foreach (string str in param)
            //{
            //    foreach (string fomat in fomats)
            //    {
            //        sb.AppendFormat("{0}{1},", str, fomat);
            //    }
            //}
            //sb.Remove(sb.Length - 1, 1);
            //return htmlCtx.GetReplace(sb.ToString(), "中标单位：");
        }

        protected string GetTableCtx(string htmlCtx)
        {
            StringBuilder sb = new StringBuilder();
            Parser parser = new Parser(new Lexer(htmlCtx));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
            if (nodeList != null && nodeList.Count > 0)
            {
                for (int i = 0; i < nodeList.Count; i++)
                {
                    TableTag table = nodeList[i] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        if ((j + 2) <= table.RowCount)
                        {
                            TableRow tr = table.Rows[j];
                            TableRow tr2 = table.Rows[j + 1];

                            if (tr.ColumnCount == tr2.ColumnCount)
                            {
                                for (int c = 0; c < tr.ColumnCount; c++)
                                {
                                    sb.AppendFormat("{0}:{1}\r\n", tr.Columns[c].ToNodePlainString(), tr2.Columns[c].ToNodePlainString());
                                }
                            }
                        }

                    }
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// 处理省份和城市
        /// </summary>
        /// <param name="prov"></param>
        /// <returns></returns>
        protected string[] GetPrivoce(string prov)
        {
            string[] strs = null;
            switch (prov)
            {
                case "内蒙古":
                    strs = new string[] { "内蒙古自治区", "内蒙古自治区及盟市" };
                    break;
                case "广西":
                    strs = new string[] { "广西壮族自治区", "广西壮族自治区及地市" };
                    break;
                case "宁夏":
                    strs = new string[] { "宁夏回族自治区", "宁夏回族自治区及地市" };
                    break;
                case "新疆":
                case "兵团":
                    strs = new string[] { "新疆维吾尔自治区", "新疆维吾尔自治区及地市" };
                    break;
                case "西藏":
                    strs = new string[] { "西藏自治区", "西藏自治区及地市" };
                    break;
                case "广东":
                    strs = new string[] { "广东省", "广东省内工程" };
                    break;
                case "重庆":
                    strs = new string[] { "重庆市", "重庆市及区县" };
                    break;
                case "上海":
                    strs = new string[] { "上海市", "上海市区" };
                    break;
                case "天津":
                    strs = new string[] { "天津市", "天津市区" };
                    break;
                case "北京":
                    strs = new string[] { "北京市", "北京市区" };
                    break;
                default:
                    strs = new string[] { prov + "省", prov + "省及地市" };
                    break;
            }
            return strs;
        }

        protected NameValueCollection GetNvc()
        {
            //            "TIMEBEGIN_SHOW=2017-06-04
            //TIMEEND_SHOW = 2017 - 09 - 04
            //TIMEBEGIN = 2017 - 06 - 04
            //TIMEEND = 2017 - 09 - 04
            //DEAL_TIME = 04
            //DEAL_CLASSIFY = 01
            //DEAL_STAGE = 0100
            //DEAL_PROVINCE = 330000
            //DEAL_CITY = 330400
            //DEAL_PLATFORM = 0
            //DEAL_TRADE = 0
            //isShowAll = 0
            //PAGENUMBER = 1
            //FINDTXT"
            NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                        "TIMEBEGIN_SHOW",
                        "TIMEEND_SHOW",
                        "TIMEBEGIN",
                        "TIMEEND",
                        "DEAL_TIME",
                        "DEAL_CLASSIFY",
                        "DEAL_STAGE",
                        "DEAL_PROVINCE",
                        "DEAL_CITY",
                        "DEAL_PLATFORM",
                        "DEAL_TRADE",
                        "isShowAll",
                        "PAGENUMBER",
                        "FINDTXT"

                    }, new string[] {
                        "2017-06-04",
                        "2017-09-04",
                        "2017-06-04",
                        "2017-09-04",
                        "04",
                        "01",
                        "0104",
                        "620000",
                        "620400",
                        "0",
                        "0",
                        "1",
                        "1",
                        ""
                    });
            return nvc;
        }
    }
}
