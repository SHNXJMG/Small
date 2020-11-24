using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;
namespace Crawler.Instance
{
    public class BidJiangXiTieL : WebSiteCrawller
    {
        public BidJiangXiTieL()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "江西省公共资源交易网中标信息(铁路工程)";
            this.Description = "自动抓取江西省公共资源交易网中标信息(铁路工程)";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.jxsggzy.cn/web/jyxx/002004/002004004/jyxx.html";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "wb-page-li")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("/", "\r");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                SiteUrl = "http://www.jxsggzy.cn/web/jyxx/002004/002004004/" + i + ".html";
                try
                    {

                    html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
                }
                    catch { continue; }
                
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "ewb-list-node clearfix")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
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
                        ATag aTag = listNode[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        if (string.IsNullOrWhiteSpace(prjName))
                            prjName = aTag.LinkText;
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        if (prjName[2].Equals('县') || prjName[2].Equals('区') || prjName[2].Equals('市'))
                            area = prjName.Substring(0, 3);
                        InfoUrl = "http://www.jxsggzy.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "article-info")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                                string ctx = string.Empty;

                            HtmlTxt = dtlNode.AsHtml();//.Replace("<br", "\r\n<br");
                            ctx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                            prjAddress = ctx.GetAddressRegex();
                                buildUnit = ctx.GetBuildRegex();

                                bidUnit = ctx.GetBidRegex(new string[] { "第一中标候选单位" });
                                if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = ctx.GetRegex("排序第一的中标候选人");

                            bidMoney = ctx.GetMoneyRegex();
                                prjMgr = ctx.GetMgrRegex(new string[] { "建造师姓名" });
                               // code = ctx.GetCodeRegex();
                                bidCtx = ctx;
                            
                            buildUnit = buildUnit.Replace(" ","");
                            bidUnit = bidUnit.Replace(" ","");
                           // code = code.Replace(" ", "");
                            prjMgr = prjMgr.Replace(" ", "");
                            prjAddress = prjAddress.Replace(" ","");
                            bidType = "铁路工程";
                            specType = "政府采购";
                            msgType = "江西省公共资源交易中心";
                            BidInfo info = ToolDb.GenBidInfo("江西省", "江西省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
