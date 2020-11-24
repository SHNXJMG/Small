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
    public class BidZhXiangZhou : WebSiteCrawller
    {
        public BidZhXiangZhou()
            : base()
        {
            this.Group = "街道办中标信息";
            this.Title = "广东省珠海市香洲区教育局信息中标信息";
            this.Description = "自动抓取珠海市香洲区教育局信息中标信息";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://xzedu.zhuhai.gov.cn/StyleList1.aspx?parentID=1080000&ID=1080200";
            this.MaxCount = 150;
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "cNavBar_cTotalPages")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToNodePlainString();
                    pageInt = int.Parse(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                    "__VIEWSTATE",
                    "__VIEWSTATEGENERATOR",
                    "__EVENTVALIDATION",
                    "cSortField",
                    "cSortDirection",
                    "cID",
                    "cParentID",
                    "cLeft:cParentID",
                    "cLeft:cID",
                    "cNavBar:cPageIndex"
                    }, new string[] { 
                    viewState,
                    "8A9C3F4D",
                    eventValidation,
                    "",
                    "",
                    "1080200",
                    "1080000",
                    "1080000",
                    "1080200",
                    i.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list")), true), new TagNameFilter("li")));

                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
                    {
                        string buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, beginDate = string.Empty, prjName = string.Empty, InfoUrl = string.Empty;


                        ATag aTag = viewList[j].GetATag();
                        beginDate = viewList[j].ToPlainTextString().GetDateRegex();
                        prjName = aTag.GetAttribute("title");
                        InfoUrl = "http://xzedu.zhuhai.gov.cn/" + aTag.Link.GetReplace("./");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "news_view_main")), true), new TagNameFilter("li")));
                        if (dtl != null && dtl.Count > 1)
                        {
                            HtmlTxt = dtl[1].ToHtml().ToLower();
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                            string src = string.Empty;
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag tag = tableNode[0] as TableTag;
                                if (tag.RowCount > 1)
                                {
                                    string ctx = string.Empty;
                                    try
                                    {
                                        for (int r = 0; r < tag.Rows[0].ColumnCount; r++)
                                        {
                                            ctx += tag.Rows[0].Columns[r].ToNodePlainString() + "：";
                                            ctx += tag.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                        }
                                    }
                                    catch { }
                                    bidUnit = ctx.GetBidRegex().GetReplace("中标（成交）");
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("投标单位");
                                    bidMoney = ctx.GetMoneyRegex();
                                    prjMgr = ctx.GetMgrRegex();
                                }
                            }
                            else
                            {
                                Parser imgParser = new Parser(new Lexer(HtmlTxt.ToLower()));
                                NodeList imgNode = imgParser.ExtractAllNodesThatMatch(new TagNameFilter("img"));

                                if (imgNode != null && imgNode.Count > 0)
                                {
                                    string imgUrl = (imgNode[0] as ImageTag).GetAttribute("src");
                                    src = "http://xzedu.zhuhai.gov.cn/" + imgUrl;
                                    HtmlTxt = HtmlTxt.ToLower().GetReplace(imgUrl, src);
                                }
                                bidUnit = bidCtx.GetBidRegex().GetReplace("中标（成交）");
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = bidCtx.GetRegex("中标（成交）供应商名称");
                                bidMoney = bidCtx.GetMoneyRegex(new string[] { "中标（成交）候选人投标报价" });
                                if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                    bidMoney = bidCtx.GetMoneyRegex();
                                prjMgr = bidCtx.GetMgrRegex();
                            }
                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));

                            code = bidCtx.GetCodeRegex().GetCodeDel();

                            try
                            {
                                if (decimal.Parse(bidMoney) < 1)
                                    bidMoney = "0";
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }

                            if (prjMgr.Contains("资格"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("资格"));

                            specType = "政府采购";
                            bidType = prjName.GetInviteBidType();
                            msgType = "珠海市香洲区教育局";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "珠海市区", "香洲区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            if (!string.IsNullOrEmpty(src))
                            {
                                string sql = string.Format("select Id from BidInfo where InfoUrl='{0}'", info.InfoUrl);
                                object obj = ToolDb.ExecuteScalar(sql);
                                if (obj == null || obj.ToString() == "")
                                {
                                    try
                                    {
                                        BaseAttach attach = ToolHtml.GetBaseAttach(src, prjName, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                        if (attach != null)
                                            ToolDb.SaveEntity(attach, "");
                                    }
                                    catch { }
                                }
                            }
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k].GetATag();
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://xzedu.zhuhai.gov.cn/" + a.Link;
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
