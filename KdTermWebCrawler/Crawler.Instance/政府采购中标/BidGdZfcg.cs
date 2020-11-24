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
using System.Threading;

namespace Crawler.Instance
{
    public class BidGdZfcg : WebSiteCrawller
    {
        public BidGdZfcg()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "广东省政府采购网中标信息";
            this.Description = "自动抓取广东省政府采购网中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 200;
            this.SiteUrl = "http://www.gdgpo.gov.cn/queryMoreInfoList/channelCode/0008.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string postUrl = "http://www.gdgpo.gov.cn/queryMoreInfoList.do";
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("form"), new HasAttributeFilter("name", "qPageForm")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString();
                    temp = temp.GetRegexBegEnd("共", "条");
                    int total = int.Parse(temp);
                    pageInt = total / 15 + 1;
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                    "channelCode",
                    "pointPageIndexId",
                    "pageIndex",
                    "pageSize"
                    }, new string[] {
                    "0008",
                    "1",
                    i.ToString(),
                    "15"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(postUrl, nvc);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "m_m_c_list")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;


                        ATag aTag = node.GetATag(1);
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        prjName = aTag.GetAttribute("title");
                        InfoUrl = "http://www.gdgpo.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "zw_c_c_cont")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</p>,<br/>", "\r\n").ToCtxString();


                            bidMoney = bidCtx.GetMoneyRegex(new string[] { "中标金额为" });
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                decimal money = 0;
                                TableTag table = tableNode[0] as TableTag;
                                for (int r = 1; r < table.RowCount; r++)
                                {
                                    try
                                    {
                                        string temp = table.Rows[r].Columns[table.Rows[r].ColumnCount - 1].ToNodePlainString().GetMoney();
                                        decimal tempMoney = decimal.Parse(temp);
                                        if (tempMoney > 0)
                                            money += tempMoney;
                                    }
                                    catch { }
                                }
                                bidMoney = money.ToString();
                            }

                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                bidMoney = bidCtx.GetMoneyRegex();

                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                bidUnit = bidCtx.GetRegex("中标供应商名称,成交供应商名称", false);
                            }
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("名称"))
                                bidUnit = bidUnit.Substring(2, bidUnit.Length - 2);

                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidMoney = "0";
                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            bidType = prjName.GetInviteBidType();
                            specType = "政府采购";
                            msgType = "广东省财政厅政府采购";

                            BidInfo info = ToolDb.GenBidInfo("广东省", "广州政府采购", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNodes = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNodes != null && aNodes.Count > 0)
                            {
                                for (int a = 0; a < aNodes.Count; a++)
                                {
                                    ATag aFile = aNodes[a] as ATag;
                                    if (aFile.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (aFile.Link.ToLower().Contains("http"))
                                            link = aFile.Link;
                                        else
                                            link = "http://www.gdgpo.gov.cn/" + aFile.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(aFile.LinkText, info.Id, link);
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
