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
    public class BidJiNanGgzy : WebSiteCrawller
    {
        public BidJiNanGgzy()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "济南市公共资源交易网水利工程中标信息";
            this.Description = "自动抓取济南市公共资源交易网水利工程中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://ggzy.jinan.gov.cn/col/col2111/index.html";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "513")));
            if (listNode != null && listNode.Count > 0)
            {
                for (int j = 0; j < listNode.Count; j++)
                {
                    TableTag tag = listNode[j] as TableTag;
                    string align = tag.GetAttribute("align");
                    string style = tag.GetAttribute("style");
                    if (!string.IsNullOrWhiteSpace(align) ||
                        !string.IsNullOrWhiteSpace(style))
                        continue;

                    string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                    TableRow tr = tag.Rows[0];
                    ATag aTag = tr.Columns[1].GetATag();
                    prjName = aTag.GetAttribute("title");
                    beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                    InfoUrl = "http://ggzy.jinan.gov.cn" + aTag.Link;
                    string htmldtl = string.Empty;
                    try
                    {
                        htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                    }
                    catch { continue; }
                    parser = new Parser(new Lexer(htmldtl));
                    NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zoom")));
                    if (dtlNode != null && dtlNode.Count > 0)
                    {
                        HtmlTxt = dtlNode.AsHtml().GetReplace("</p>,<br/>", "\r\n");
                        parser = new Parser(new Lexer(HtmlTxt));
                        NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                        if (tableNode != null && tableNode.Count > 0)
                        {
                            TableTag table = tableNode[0] as TableTag;
                            for (int r = 0; r < table.RowCount; r++)
                            {
                                for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                                {
                                    string temp = table.Rows[r].Columns[c].ToNodePlainString();
                                    if (string.IsNullOrWhiteSpace(temp)) continue;
                                    if ((c + 1) % 2 == 0)
                                        bidCtx += temp.GetReplace(":,：") + "\r\n";
                                    else
                                        bidCtx += temp.GetReplace(":,：") + "：";
                                }
                            }
                        }
                        else
                            bidCtx = HtmlTxt.ToCtxString().GetReplace("begin-->,end-->");
                        prjAddress = bidCtx.GetAddressRegex().GetCodeDel().GetReplace("　,&mdash");
                        buildUnit = bidCtx.GetBuildRegex().GetReplace("　");
                        if (buildUnit.Contains("公司"))
                            buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                        if (buildUnit.Contains("联系"))
                            buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                        if (buildUnit.Contains("地址"))
                            buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                        code = bidCtx.GetCodeRegex().GetCodeDel();
                        bidUnit = bidCtx.GetBidRegex();
                        bidMoney = bidCtx.GetMoneyRegex();
                        prjMgr = bidCtx.GetMgrRegex();
                        msgType = "济南市公共资源交易中心";
                        specType = "政府采购";
                        bidType = "建设工程";
                        BidInfo info = ToolDb.GenBidInfo("山东省", "山东省及地市", "济南市", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                        link = "http://ggzy.jinan.gov.cn" + a.Link.GetReplace("../,./");
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
            return list;
        }
    }
}
