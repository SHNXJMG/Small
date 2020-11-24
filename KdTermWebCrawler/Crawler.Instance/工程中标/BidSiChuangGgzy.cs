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
    public class BidSiChuangGgzy : WebSiteCrawller
    {
        public BidSiChuangGgzy()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "四川省公共资源交易中心中标信息";
            this.Description = "自动抓取四川省公共资源交易中心中标信息";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.spprec.com/sczw/jyfwpt/005001/005001003/MoreInfo.aspx?CategoryNum=005001003";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("vAlign", "bottom")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("总页数：", "当前");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    string __CSRFTOKEN = ToolHtml.GetHtmlInputValue(html, "__CSRFTOKEN");
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__CSRFTOKEN",
                    "__VIEWSTATE",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT"
                    },
                        new string[]{
                     __CSRFTOKEN,
                        viewState,
                              "MoreInfoList1$Pager",
                        i.ToString()
                        });
                    try
                    {
                        cookiestr = cookiestr.GetReplace(new string[] { "path=/;", "HttpOnly", "," });
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "MoreInfoList1_DataGrid1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.spprec.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ivs_content")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("<br />,<br/>,<br>,</p>", "\r\n").ToCtxString();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                bidCtx = string.Empty;
                                TableTag htmlTable = tableNode[0] as TableTag;
                                for (int r = 0; r < htmlTable.RowCount; r++)
                                {
                                    if (r == 8 || r == 9) continue;
                                    for (int c = 0; c < htmlTable.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = htmlTable.Rows[r].Columns[c].ToNodePlainString();
                                        if (r == 7)
                                        {
                                            try
                                            {
                                                bidCtx += temp + "：";
                                                string value = htmlTable.Rows[r + 2].Columns[c].ToNodePlainString();
                                                bidCtx += value + "\r\n";
                                            }
                                            catch {
                                                try
                                                {
                                                    bidCtx += temp + "：";
                                                    string value = htmlTable.Rows[r + 1].Columns[c].ToNodePlainString();
                                                    bidCtx += value + "\r\n";
                                                }
                                                catch { }
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(temp))
                                                continue;
                                            if ((c + 1) % 2 == 0)
                                                bidCtx += temp + "\r\n";
                                            else
                                                bidCtx += temp + "：";
                                        }
                                    }
                                }
                            }

                            buildUnit = bidCtx.GetBuildRegex().GetReplace("/");
                            prjAddress = bidCtx.GetAddressRegex();
                            code = bidCtx.GetCodeRegex().GetChina().GetCodeDel();
                            bidUnit = bidCtx.GetRegex("中标候选人名称").GetReplace("/");
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetBidRegex().GetReplace("/");
                            if (bidUnit.Contains("投标报价"))
                                bidUnit = "";
                            bidMoney = bidCtx.GetMoneyRegex();
                            msgType = "四川省公共资源交易中心";
                            specType = bidType = "建设工程";
                      
                            BidInfo info = ToolDb.GenBidInfo("四川省", "四川省及地市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag tag = aNode[k] as ATag;
                                    if (tag.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (tag.Link.ToLower().Contains("http"))
                                            link = tag.Link;
                                        else
                                            link = "http://www.spprec.com" + tag.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(tag.LinkText, info.Id, link);
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
