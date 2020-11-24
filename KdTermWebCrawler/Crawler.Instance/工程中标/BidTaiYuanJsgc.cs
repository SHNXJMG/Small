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
    public class BidTaiYuanJsgc : WebSiteCrawller
    {
        public BidTaiYuanJsgc()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "太原建设工程信息网";
            this.Description = "自动抓取太原建设工程信息网中标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.SiteUrl = "http://www.tyjzsc.com.cn/qyxx.do?method=getZhongbgglist";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default).Replace("&nbsp;", "");
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "Body_div")), true), new TagNameFilter("li")));
            if (sNode != null && sNode.Count > 0)
            {
                for (int t = 0; t < sNode.Count; t++)
                {

                    string prjName = string.Empty,
                           buildUnit = string.Empty, bidUnit = string.Empty,
                           bidMoney = string.Empty, code = string.Empty,
                           bidDate = string.Empty, beginDate = string.Empty,
                           endDate = string.Empty, bidType = string.Empty,
                           specType = string.Empty, InfoUrl = string.Empty,
                           msgType = string.Empty, bidCtx = string.Empty,
                           prjAddress = string.Empty, remark = string.Empty,
                           prjMgr = string.Empty, otherType = string.Empty,
                           HtmlTxt = string.Empty, area = string.Empty;

                    INode node = sNode[t];
                    ATag aTag = node.GetATag();
                    prjName = aTag.GetAttribute("title");
                    beginDate = node.ToPlainTextString().GetDateRegex();
                    InfoUrl = "http://www.tyjzsc.com.cn/" + aTag.Link.GetReplace("./");
                    string htmldtl = string.Empty;
                    try
                    {
                        htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                    }
                    catch { continue; }
                    parser = new Parser(new Lexer(htmldtl.GetReplace("th,TH", "td")));
                    NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "mytable")));
                    if (dtlNode != null && dtlNode.Count > 0)
                    {
                        HtmlTxt = dtlNode.AsHtml();
                        bidCtx = "";
                        TableTag table = dtlNode[0] as TableTag;
                        for (int r = 0; r < table.RowCount; r++)
                        {
                            for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                            {
                                if (c % 2 == 0)
                                    bidCtx += table.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：") + "：";
                                else
                                    bidCtx += table.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：") + "\r\n";
                            }
                        } 

                        buildUnit = bidCtx.GetBuildRegex();
                        prjAddress = bidCtx.GetAddressRegex();
                        code = bidCtx.GetCodeRegex();
                        if (string.IsNullOrEmpty(code))
                            code = bidCtx.GetRegex("工程编码",true,50);
                        bidUnit = bidCtx.GetBidRegex();
                        bidMoney = bidCtx.GetMoneyRegex();
                        if(string.IsNullOrEmpty(bidMoney)||bidMoney=="0")
                            bidMoney = bidCtx.GetMoneyRegex(new string[]{"投资总额"});
                        if (Encoding.Default.GetByteCount(prjName)>200)
                            prjName = prjName.Substring(0, 100);
                        msgType = "太原市建设工程交易中心";
                        specType = "建设工程";
                        bidType = prjName.GetInviteBidType();
                        BidInfo info = ToolDb.GenBidInfo("山西省", "山西省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        list.Add(info);

                    }
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }
            return list;
        }
    }
}
