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
    public class BidGdZbtb : WebSiteCrawller
    {
        public BidGdZbtb()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省招投标监督网中标信息";
            this.Description = "自动抓取广东省招投标监督网中标信息";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.gdzbtb.gov.cn/zhongbiaojieguo/index.htm";
            this.MaxCount = 20;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            List<BidInfo> list = new List<BidInfo>();

            List<BidInfo> zbhxrgss = this.GetBidInfo("http://www.gdzbtb.gov.cn/zbhxrgsbd/");
            if (zbhxrgss != null && zbhxrgss.Count > 0)
                list.AddRange(zbhxrgss);

            List<BidInfo> zbjgs = this.GetBidInfo("http://www.gdzbtb.gov.cn/zhongbiaojieguo/");
            if (zbjgs != null && zbjgs.Count > 0)
                list.AddRange(zbjgs);

            return list;
        }


        protected List<BidInfo> GetBidInfo(string url)
        {
            List<BidInfo> list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(url, Encoding.Default);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "cn6")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().Replace("(", "kdxx").GetRegexBegEnd("kdxx", ",");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(url + "index_" + (i - 1).ToString() + ".htm", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "position2")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
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
                             prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty,
                             city = string.Empty;

                        prjName = nodeList[j].GetATagValue("title");
                        if (string.IsNullOrEmpty(prjName))
                            continue;
                        if (prjName.Contains("项目所在地区"))
                        {
                            city = "广州市区";
                            prjName = prjName.Replace("[", "").Replace("]", "").Replace("my:项目所在地区notset区", "");
                        }
                        if (prjName.Contains("广东省"))
                        {
                            city = "广州市区";
                            prjName = prjName.Replace("[", "").Replace("]", "").Replace("广东省", "");
                        }
                        else
                        {
                            string temp = prjName.Replace("[", "kdxx").Replace("]", "xxdk").GetRegexBegEnd("kdxx", "xxdk");
                            if (!string.IsNullOrEmpty(temp))
                            {
                                prjName = prjName.Replace("[", "").Replace("]", "").Replace(temp, "");
                                city = temp + "区";
                            }
                            else
                            {
                                prjName = prjName.Replace("[", "").Replace("]", "");
                                city = "广州市区";
                            }
                        }
                        prjName = prjName.Replace("--中标结果", "");
                        if (!string.IsNullOrEmpty(prjName))
                        {
                            if (prjName[0] == '-')
                                prjName = prjName.Substring(1, prjName.Length - 1);
                        }

                        InfoUrl = url + nodeList[j].GetATagHref().Replace("../", "").Replace("./", "");
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch
                        {
                            Logger.Error(nodeList[j].ToNodePlainString());
                            continue;
                        }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellSpacing", "1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml().Replace("<br/>", "\r\n").Replace("<br>", "\r\n").Replace("<BR/>", "\r\n").Replace("<BR>", "\r\n").Replace("<p>", "\r\n").Replace("</p>", "\r\n").Replace("<P>", "\r\n").Replace("</P>", "\r\n");
                            bidCtx = HtmlTxt.ToCtxString();

                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.GetCodeRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            bidType = prjName.GetInviteBidType();

                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex(null, false, "万元");
                            if (bidMoney != "0")
                            {
                                try
                                {
                                    int money = int.Parse(bidMoney);
                                    if (money > 100000)
                                        bidMoney = bidCtx.GetMoneyRegex();
                                }
                                catch { }
                            }
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                bidUnit = bidCtx.GetRegexBegEnd("评标结果，", "为该项目");
                            }
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                bidUnit = bidCtx.GetRegexBegEnd("第一中标候选人，", "为本工程");
                            }
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                string ctx = string.Empty;
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList bidNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoNormalTable")));
                                if (bidNode != null && bidNode.Count > 0)
                                {

                                    TableTag table = bidNode[0] as TableTag;
                                    if (table.RowCount > 1)
                                    {
                                        try
                                        {
                                            for (int cell = 0; cell < table.Rows[0].ColumnCount; cell++)
                                            {
                                                ctx += table.Rows[0].Columns[cell].ToNodePlainString().Replace("　", "").Replace(" ", "") + "："; ctx += table.Rows[1].Columns[cell].ToNodePlainString().Replace("　", "").Replace(" ", "") + "\r\n";
                                            }
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        parser.Reset();
                                        bidNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoTableGrid")));
                                        if (bidNode != null && bidNode.Count > 0)
                                        {
                                            table = bidNode[0] as TableTag;
                                            if (table.RowCount > 1)
                                            {
                                                try
                                                {
                                                    for (int r = 0; r < table.RowCount; r++)
                                                    {
                                                        ctx += table.Rows[r].Columns[0].ToNodePlainString() + "：";
                                                        ctx += table.Rows[r].Columns[1].ToNodePlainString() + "\r\n";
                                                    }
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                    bidUnit = ctx.GetRegex("投标单位,第一候选人,单位名称");
                                    if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                    {
                                        bidMoney = ctx.GetMoneyRegex(null, false, "万元");
                                        if (bidMoney != "0")
                                        {
                                            try
                                            {
                                                int money = int.Parse(bidMoney);
                                                if (money > 100000)
                                                    bidMoney = ctx.GetMoneyRegex();
                                            }
                                            catch { }
                                        }
                                    }
                                }
                                else
                                {
                                    parser.Reset();
                                    bidNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoTableGrid")));
                                    if (bidNode == null || bidNode.Count < 1)
                                    {
                                        parser.Reset();
                                        bidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                    }
                                    if (bidNode != null && bidNode.Count > 0)
                                    {
                                        ctx = "";
                                        TableTag table = null;
                                        if (bidNode.Count > 1)
                                            table = bidNode[1] as TableTag;
                                        else
                                            table = bidNode[bidNode.Count - 1] as TableTag;
                                        if (table.RowCount > 1)
                                        {
                                            try
                                            {
                                                for (int r = 0; r < table.RowCount; r++)
                                                {
                                                    ctx += table.Rows[r].Columns[0].ToNodePlainString() + "：";
                                                    ctx += table.Rows[r].Columns[1].ToNodePlainString() + "\r\n";
                                                }
                                            }
                                            catch { }
                                        }
                                        bidUnit = ctx.GetRegex("投标单位,单位名称");
                                        if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                        {
                                            bidMoney = ctx.GetMoneyRegex(null, false, "万元");
                                            if (bidMoney != "0")
                                            {
                                                try
                                                {
                                                    int money = int.Parse(bidMoney);
                                                    if (money > 100000)
                                                        bidMoney = ctx.GetMoneyRegex();
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                            }
                            if (bidUnit.Contains("中标价"))
                            {
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("中标价"));
                            }
                            if (bidUnit.Contains("报价"))
                            {
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("报价"));
                            }
                            if (bidUnit.Contains("项目负责人"))
                            {
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("项目负责人"));
                            }

                            if (bidUnit.Contains("公司"))
                            {
                                bidUnit = bidUnit.Remove(bidUnit.LastIndexOf("公司")) + "公司";
                            }



                            bidUnit = bidUnit.Replace("（牵头人）", "");
                            msgType = "广东省招标投标监管网";
                            specType = "建设工程";
                            if (bidCtx.IndexOf("发布日期") != -1)
                            {
                                string ctx = bidCtx.Substring(bidCtx.IndexOf("发布日期"), bidCtx.Length - bidCtx.IndexOf("发布日期"));
                                beginDate = ctx.GetDateRegex();
                            }
                            else if (bidCtx.IndexOf("发布时间") != -1)
                            {
                                string ctx = bidCtx.Substring(bidCtx.IndexOf("发布时间"), bidCtx.Length - bidCtx.IndexOf("发布时间"));
                                beginDate = ctx.GetDateRegex();
                            }
                            if (string.IsNullOrEmpty(beginDate))
                            {
                                beginDate = DateTime.Now.ToString("yyyy-MM-dd");
                            }
                            BidInfo info = ToolDb.GenBidInfo("广东省", city, "", string.Empty, code, prjName, buildUnit, beginDate,
                                       bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                       bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList imgNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                            if (imgNode != null && imgNode.Count > 0)
                            {
                                for (int img = 0; img < imgNode.Count; img++)
                                {
                                    ImageTag imgTag = imgNode[img] as ImageTag;
                                    string tagUrl = imgTag.GetAttribute("src");
                                    if (!string.IsNullOrWhiteSpace(tagUrl))
                                    {
                                        string srcUrl = InfoUrl.Remove(InfoUrl.LastIndexOf("/"));
                                        string src = srcUrl + tagUrl.Replace("./", "/");
                                        string attachName = tagUrl.Replace("./", "");
                                        BaseAttach attach = null;
                                        if (Encoding.Default.GetByteCount(attachName) < 400)
                                            attach = ToolDb.GenBaseAttach(attachName, info.Id, src);
                                        base.AttachList.Add(attach);
                                        info.CtxHtml = info.CtxHtml.Replace(tagUrl, src);
                                    }
                                }
                            }
                            list.Add(info);
                            if (list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }


    }
}
