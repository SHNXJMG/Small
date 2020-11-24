using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Windows.Forms;
using System.Text.RegularExpressions;
namespace Crawler.Instance
{
    public class BidNeiMengMS : WebSiteCrawller
    {
        public BidNeiMengMS()
        {
            this.Group = "政府采购中标信息";
            this.Title = "内蒙古政府采购盟市信息";
            this.Description = "自动抓取内蒙古政府采购盟市信息";
            this.SiteUrl = "http://www.nmgp.gov.cn/procurement/pages/tender.jsp?notarea=150000&type=2";
            this.PlanTime = "9:40,11:40,14:40,16:40,18:40";
            this.MaxCount = 60;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();

            //取得页码
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            Parser parser = new Parser(new Lexer(html));
            int pageInt = 1;
            NodeList sNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagenumber")), true), new TagNameFilter("a")));
            if (sNodes != null && sNodes.Count > 1)
            {
                string page = sNodes[sNodes.Count - 2].ToPlainTextString();
                try
                {
                    pageInt = int.Parse(page);
                }
                catch { }
            }
            parser.Reset();
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&pos=" + i.ToString(), Encoding.Default);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.ToString());
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "recordlist")));
                if (nodes != null && nodes.Count > 0)
                {
                    TableTag table = nodes[0] as TableTag;
                    for (int t = 0; t < table.RowCount; t++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty,
                            code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                            bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty,
                            bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty,
                            otherType = string.Empty, HtmlTxt = string.Empty, strHtml = string.Empty;
                        TableRow tr = table.Rows[t];
                        endDate = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        ATag alink = tr.Columns[0].GetATag();
                        prjName = tr.Columns[0].GetATagValue("title");
                        InfoUrl = "http://www.nmgp.gov.cn" + alink.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Trim();
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }

                        Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                        htmldtl = regexHtml.Replace(htmldtl, "");
                        Parser parserdtl = new Parser(new Lexer(htmldtl));
                        Parser dtlparserHTML = new Parser(new Lexer(htmldtl));
                        NodeList nodesDtl = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "hlcms_9")));
                        if (nodesDtl != null && nodesDtl.Count > 0)
                        {
                            Parser begDate = new Parser(new Lexer(nodesDtl.ToHtml()));
                            NodeList begNode = begDate.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "yzhang")));
                            if (begNode != null && begNode.Count > 0)
                            {
                                beginDate = begNode.AsString().GetDateRegex("yyyy年MM月dd日");
                            }
                            begDate.Reset();
                            NodeList dtlTable = begDate.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "5")));
                            if (dtlTable != null && dtlTable.Count > 0)
                            {
                                TableTag tableDtl = dtlTable[0] as TableTag;
                                if (tableDtl.RowCount > 2)
                                {
                                    string ctx = tableDtl.Rows[2].ToPlainTextString();
                                    bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                                    bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                                }
                                if (bidMoney == "0" && tableDtl.RowCount > 4)
                                {
                                    string ctx = tableDtl.Rows[4].ToPlainTextString();
                                    bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                                    bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                                }
                                if (bidMoney == "0" && tableDtl.RowCount > 6)
                                {
                                    string ctx = tableDtl.Rows[6].ToPlainTextString();
                                    bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                                    bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                                }
                                if (bidMoney == "0" && tableDtl.RowCount > 8)
                                {
                                    string ctx = tableDtl.Rows[8].ToPlainTextString();
                                    bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                                    bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                                }
                                if (bidMoney == "0" && tableDtl.RowCount > 10)
                                {
                                    string ctx = tableDtl.Rows[10].ToPlainTextString();
                                    bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                                    bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                                }
                                if (bidMoney == "0" && tableDtl.RowCount > 12)
                                {
                                    string ctx = tableDtl.Rows[12].ToPlainTextString();
                                    bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                                    bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                                }
                            }
                            HtmlTxt = nodesDtl.ToHtml();
                            bidCtx = HtmlTxt.ToCtxString();

                            code = bidCtx.GetRegex("批准文件编号,工程编号,项目编号").Replace("无", "");
                            buildUnit = bidCtx.GetBuildRegex();
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = bidCtx.GetRegex("采购代理机构名称,采购单位名称");
                            prjAddress = bidCtx.GetAddressRegex();
                            if (string.IsNullOrEmpty(prjAddress))
                                prjAddress = bidCtx.GetRegex("投标地点,开标地点,地址");
                            if (bidUnit.Contains("废标"))
                                bidUnit = "没有中标商";
                            msgType = "内蒙古政府采购盟市";
                            specType = "政府采购";
                            if (Encoding.Default.GetByteCount(code) > 50)
                                code = code.GetChina();
                            if (Encoding.Default.GetByteCount(code) > 50)
                                code = "";

                            bidType = ToolHtml.GetInviteTypes(prjName);
                            prjName = ToolDb.GetPrjName(prjName);
                            BidInfo info = ToolDb.GenBidInfo("内蒙古自治区", "内蒙古自治区及盟市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount)
                                return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
