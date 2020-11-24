using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Lex;
using System.Collections;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class BidZhongShanZhenQu:WebSiteCrawller
    {
        public BidZhongShanZhenQu()
            : base()  
        {
            this.Group = "中标信息";
            this.Title = "广东省中山市镇区中标信息";
            this.Description = "自动抓取广东省中山市镇区中标信息";
            this.ExistCompareFields = "InfoUrl";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.SiteUrl = "http://www.zsjyzx.gov.cn/zsweb/index/showList/000000000004/000000000393";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt =15;
            //取得页码 
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList aNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "scott"))), new TagNameFilter("a")));
            if (aNodes != null && aNodes.Count > 0)
            {
                try
                {
                    string temp = aNodes.GetATagHref(aNodes.Count - 1);
                    pageInt = Convert.ToInt32(temp.GetRegexBegEnd("(", ")"));
                }
                catch 
                {
                    pageInt = 15;
                }
            }
            parser.Reset();

            //逐页读取数据
            for (int page = 1; page <= pageInt; page++)
            {
                try
                {
                    if (page > 1)
                    {
                        string typeId = html.GetInputValue("typeId");
                        string boardId = html.GetInputValue("boardId");
                        string totalRows = html.GetInputValue("totalRows");
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "typeId","boardId","newstitle","sTime","eTime","totalRows","pageNO"
                        }, new string[]{
                        typeId,boardId,string.Empty,string.Empty,string.Empty,totalRows,page.ToString()
                        });
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default);
                    }
                }
                catch  
                {
                    continue;
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "lefttable")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
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
                   prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[1].ToNodePlainString();
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = tr.GetATagHref();

                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList ifrm = parser.ExtractAllNodesThatMatch(new TagNameFilter("iframe"));
                            IFrameTag iframe = ifrm.SearchFor(typeof(IFrameTag), true)[0] as IFrameTag;
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(iframe.GetAttribute("src").Replace("/zsweb/..", ""), Encoding.Default);
                        }
                        catch { Logger.Error("BidZhongshan"); continue; }
                        parser = new Parser(new Lexer(htlDtl.Replace("th", "td").Replace("TH", "td")));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "newtalbe_c")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            HtmlTxt = dtlList.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            TableTag tab = dtlList[0] as TableTag;
                            string ctx = string.Empty;
                            for (int k = 0; k < tab.RowCount; k++)
                            {
                                for (int d = 0; d < tab.Rows[k].ColumnCount; d++)
                                {
                                    if ((d + 1) % 2 == 0)
                                    {
                                        ctx += tab.Rows[k].Columns[d].ToNodePlainString() + "\r\n";
                                    }
                                    else
                                    {
                                        ctx += tab.Rows[k].Columns[d].ToNodePlainString() + "：";
                                    }
                                }
                            }
                            code = htlDtl.ToCtxString().GetCodeRegex().Replace("[", "").Replace("]", "");
                            buildUnit = ctx.GetBuildRegex();
                            prjAddress = ctx.GetAddressRegex();
                            bidUnit = ctx.GetBidRegex();
                            bidMoney = ctx.GetMoneyRegex();
                            bidType = prjName.GetInviteBidType();
                            msgType = "中山市住房和城乡建设局";
                            specType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "中山市区", string.Empty, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                   bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList aList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aList != null && aList.Count > 0)
                            {
                                for (int c = 0; c < aList.Count; c++)
                                {
                                    ATag a = aList[c] as ATag;
                                    if (a.LinkText.IsAtagAttach())
                                    {
                                        string alink = a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, alink);
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
