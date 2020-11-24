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
    public class BidGanSuJjxxw : WebSiteCrawller
    {
        public BidGanSuJjxxw()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "甘肃省经济信息网中标信息";
            this.Description = "自动抓取甘肃省经济信息网中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.SiteUrl = "http://www.gsei.com.cn/index.php/cms/item-list-category-1337.shtml";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "mtop pages")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("1/", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i >1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.gsei.com.cn/index.php/cms/item-list-category-1337-page-" + i + ".shtml", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "label_ul_b")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                { 
                    for (int j = 0; j < listNode.Count; j++)
                    { 
                        ATag aTag = listNode[j].GetATag();
                        if (aTag == null) continue;
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
                        prjName = aTag.GetAttribute("title");
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "p8_content_show")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,<br/>", "\r\n").ToCtxString();
                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("第一成交候选人,第一名,中标人为,中标单位名称");
                            bidMoney = bidCtx.GetMoneyRegex(null, false, "万元");
                            if (string.IsNullOrWhiteSpace(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetMoneyRegex(new string[] { "中标造价" }, false, "万元");
                            prjMgr = bidCtx.GetMgrRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    TableTag tag = tableNode[0] as TableTag;
                                    string ctx = string.Empty;
                                    for (int r = 0; r < tag.RowCount; r++)
                                    {
                                        string rowName = tag.Rows[r].ToNodePlainString();
                                        if (rowName.Contains("中标候选人名称") || rowName.Contains("中标价"))
                                        {
                                            for (int c = 0; c < 7; c++)
                                            {
                                                try
                                                {
                                                    if (c < 3)
                                                        ctx += tag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：") + "：";
                                                    else
                                                        ctx += tag.Rows[r + 1].Columns[c - 3].ToNodePlainString().GetReplace(":,：") + "：";

                                                    ctx += tag.Rows[r + 2].Columns[c].ToNodePlainString().GetReplace(":,：") + "\r\n";

                                                }
                                                catch { }
                                            }
                                        }
                                        else
                                        {
                                            for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                            {
                                                string temp = tag.Rows[r].Columns[c].ToNodePlainString();

                                                if ((c + 1) % 2 == 0)
                                                    ctx += temp.GetReplace(":,：") + "\r\n";
                                                else
                                                    ctx += temp.GetReplace(":,：") + "：";
                                            }
                                        }
                                        if (rowName.Contains("中标候选人名称") || rowName.Contains("中标价")) break;
                                    }
                                    bidUnit = ctx.GetBidRegex().GetReplace("第一名,第二名,第三名,名次");
                                    if (string.IsNullOrEmpty(bidUnit))
                                        bidUnit = ctx.GetRegex("中标候选人名称");
                                    if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                        bidMoney = ctx.GetMoneyRegex();
                                    if (string.IsNullOrEmpty(buildUnit))
                                        buildUnit = ctx.GetBuildRegex();
                                    if (string.IsNullOrEmpty(prjMgr) || prjMgr.IsNumber())
                                        prjMgr = ctx.GetMgrRegex();
                                    if (string.IsNullOrEmpty(code))
                                        code = ctx.GetCodeRegex().GetCodeDel();

                                    if (string.IsNullOrEmpty(bidUnit) || bidUnit.IsNumber())
                                    {
                                        ctx = string.Empty;
                                        for (int r = 0; r < tag.RowCount; r++)
                                        {
                                            for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                            {
                                                string temp = tag.Rows[r].Columns[c].ToNodePlainString();

                                                if ((c + 1) % 2 == 0)
                                                    ctx += temp.GetReplace(":,：") + "\r\n";
                                                else
                                                    ctx += temp.GetReplace(":,：") + "：";
                                            }
                                        }
                                        bidUnit = ctx.GetBidRegex().GetReplace("第一名,第二名,第三名,名次"); 
                                        if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                            bidMoney = ctx.GetMoneyRegex();
                                        if (string.IsNullOrEmpty(buildUnit))
                                            buildUnit = ctx.GetBuildRegex();
                                        if (string.IsNullOrEmpty(prjMgr) || prjMgr.IsNumber())
                                            prjMgr = ctx.GetMgrRegex();
                                        if (string.IsNullOrEmpty(code))
                                            code = ctx.GetCodeRegex().GetCodeDel();

                                        if (string.IsNullOrEmpty(bidUnit) || bidUnit.IsNumber())
                                        {
                                            ctx = string.Empty;
                                            for (int c = 0; c < tag.Rows[0].ColumnCount; c++)
                                            {
                                                try
                                                {
                                                    ctx += tag.Rows[0].Columns[c].ToNodePlainString().GetReplace(":,：") + "：";
                                                    ctx += tag.Rows[1].Columns[c].ToNodePlainString().GetReplace(":,：") + "\r\n";
                                                }
                                                catch { }
                                            }
                                            bidUnit = ctx.GetBidRegex().GetReplace("第一名,第二名,第三名,名次");
                                            if (string.IsNullOrEmpty(bidUnit))
                                                bidUnit = ctx.GetRegex("中标候选人名称");
                                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                                bidMoney = ctx.GetMoneyRegex();
                                            if (string.IsNullOrEmpty(buildUnit))
                                                buildUnit = ctx.GetBuildRegex();
                                            if (string.IsNullOrEmpty(prjMgr) || prjMgr.IsNumber())
                                                prjMgr = ctx.GetMgrRegex();
                                            if (string.IsNullOrEmpty(code))
                                                code = ctx.GetCodeRegex().GetCodeDel();
                                        }
                                        if (string.IsNullOrEmpty(bidUnit) || bidUnit.IsNumber())
                                        {
                                            for (int r = 0; r < tag.RowCount; r++)
                                            {
                                                string rowName = tag.Rows[r].ToNodePlainString();
                                                if (rowName.Contains("中标候选人名称") || rowName.Contains("中标价"))
                                                {
                                                    for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                                    {
                                                        try
                                                        {
                                                            ctx += tag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：") + "：";
                                                            ctx += tag.Rows[r + 1].Columns[c].ToNodePlainString().GetReplace(":,：") + "\r\n";
                                                        }
                                                        catch { }
                                                    }
                                                }
                                                else
                                                {
                                                    for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                                    {
                                                        string temp = tag.Rows[r].Columns[c].ToNodePlainString();

                                                        if ((c + 1) % 2 == 0)
                                                            ctx += temp.GetReplace(":,：") + "\r\n";
                                                        else
                                                            ctx += temp.GetReplace(":,：") + "：";
                                                    }
                                                }
                                                if (rowName.Contains("中标候选人名称") || rowName.Contains("中标价")) break;
                                            }
                                            bidUnit = ctx.GetBidRegex().GetReplace("第一名,第二名,第三名,名次");
                                            if (string.IsNullOrEmpty(bidUnit))
                                                bidUnit = ctx.GetRegex("中标候选人名称");
                                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                                bidMoney = ctx.GetMoneyRegex();
                                            if (string.IsNullOrEmpty(buildUnit))
                                                buildUnit = ctx.GetBuildRegex();
                                            if (string.IsNullOrEmpty(prjMgr)||prjMgr.IsNumber())
                                                prjMgr = ctx.GetMgrRegex();
                                            if (string.IsNullOrEmpty(code))
                                                code = ctx.GetCodeRegex().GetCodeDel();
                                        }
                                    }
                                }
                            }

                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("中标价"))
                                bidUnit = "";
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            List<string> imgList = new List<string>();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList imgNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                            if (imgNode != null && imgNode.Count > 0)
                            {
                                for (int m = 0; m < imgNode.Count; m++)
                                {
                                    ImageTag tag = imgNode[m] as ImageTag;
                                    string link = tag.GetAttribute("src");
                                    string webLink = "http://www.gsei.com.cn/" + link;
                                    HtmlTxt = HtmlTxt.GetReplace(link, webLink);
                                    imgList.Add(webLink);

                                }
                            }
                            if (!bidUnit.Contains("公司") && !bidUnit.Contains("研究院") && !bidUnit.Contains("管理局") && !bidUnit.Contains("院"))
                                bidUnit = "";
                            msgType = "甘肃省信息中心";
                            specType = "政府采购";
                            bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("甘肃省", "甘肃省及地市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            if (imgList.Count > 0)
                            {
                                foreach (string img in imgList)
                                {
                                    string linkName = string.Empty;
                                    if (img.Contains("/"))
                                        linkName = img.Substring(img.LastIndexOf("/"));
                                    else
                                        linkName = img;
                                    BaseAttach attach = ToolDb.GenBaseAttach(linkName, info.Id, img);
                                    base.AttachList.Add(attach);
                                }
                            }
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
                                            link = "http://www.gsei.com.cn/" + a.Link.GetReplace("../,./");
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
            }
            return list;
        }
    }
}
