using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Util;

namespace Crawler.Instance
{
    public class BidSzbaoanD : WebSiteCrawller
    {
        public BidSzbaoanD()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省深圳市宝安区街道办";
            this.Description = "自动抓取广东省深圳市宝安区街道办中标信息";
            this.SiteUrl = "http://www.bajsjy.com/JDZHB"; 
            this.MaxCount = 20;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8); 
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "input-group-addon")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                try
                {
                    string reTemp = tdNodes.AsString().GetRegexBegEnd("共", "项");
                    string pageTemp = tdNodes.AsString().GetRegexBegEnd("项", "页").GetReplace("共,项,页," + reTemp + ",，");
                    pageInt = int.Parse(pageTemp);
                }
                catch (Exception) { }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "?pi=" + (i-1), Encoding.UTF8);
                    }
                    catch { continue; } 
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "inside_table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag tableRow = (TableTag)nodeList[0];
                    for (int j = 1; j < tableRow.RowCount; j++)
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
                        TableRow tr = tableRow.Rows[j];
                        beginDate = tr.Columns[3].ToPlainTextString().Trim();
                        prjName = tr.Columns[1].ToPlainTextString().Trim().GetReplace("&quot;");
                        buildUnit = tr.Columns[2].ToPlainTextString().Trim();
                        InfoUrl = "http://www.bajsjy.com/" + tr.Columns[1].GetATagHref();

                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8).Replace("<th", "<td").Replace("</th>", "</td>").Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "inside_table")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            TableTag tabledetail = (TableTag)dtnode[0];
                            for (int r = 0; r < tabledetail.RowCount; r++)
                            {
                                TableRow trdetail = tabledetail.Rows[r];
                                try
                                {
                                    for (int c = 0; c < trdetail.ColumnCount; c++)
                                    {

                                        string tr1 = string.Empty;
                                        string tr2 = string.Empty;
                                        tr1 = trdetail.Columns[c].ToPlainTextString().Trim();
                                        tr2 = trdetail.Columns[c + 1].ToPlainTextString().Trim();
                                        bidCtx += tr1 + "：" + tr2 + "\r\n";
                                        if (trdetail.ColumnCount > (c + 1))
                                        {
                                            c = c + 1;
                                        }
                                    }
                                }
                                catch
                                {
                                    bidCtx = HtmlTxt.ToCtxString();
                                }
                            }
                            Regex regPrjAdd = new Regex(@"(工程地点|工程地址)：[^\r\n]+\r\n");
                            prjAddress = regPrjAdd.Match(bidCtx).Value.Replace("工程地点：", "").Replace("工程地址：", "").Trim();
                            if (string.IsNullOrEmpty(prjAddress))
                            {
                                prjAddress = string.Empty;
                            }
                            prjAddress = ToolHtml.GetSubString(prjAddress, 50);
                            msgType = "深圳市建设工程交易中心宝安分中心";
                            specType = "建设工程";
                            Regex regMoney = new Regex(@"(中标价)：[^\r\n]+\r\n");
                            bidMoney = regMoney.Match(bidCtx).Value.Replace("金额", "").Replace("中标价", "").Replace("：", "").Replace(":", "").Replace("/", "").Replace("，", "").Trim();
                            Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");

                            if (!string.IsNullOrEmpty(regBidMoney.Match(bidMoney).Value))
                            {

                                if (bidMoney.Contains("万元") || bidMoney.Contains("万美元") || bidMoney.Contains("万"))
                                {
                                    bidMoney = regBidMoney.Match(bidMoney).Value;
                                }
                                else
                                {
                                    try
                                    {
                                        bidMoney = (decimal.Parse(regBidMoney.Match(bidMoney).Value) / 10000).ToString();
                                        if (decimal.Parse(bidMoney) < decimal.Parse("0.1"))
                                        {
                                            bidMoney = "0";
                                        }
                                    }
                                    catch (Exception)
                                    {

                                        bidMoney = "0";
                                    }

                                }
                            }
                            Regex regBidUnit = new Regex(@"(中标人|中标单位)：[^\r\n]+\r\n");
                            bidUnit = regBidUnit.Match(bidCtx).Value.Replace("中标人：", "").Replace("中标单位", "").Trim();

                            if (bidUnit == "" || bidUnit == null)
                            {
                                bidUnit = "";
                            }
                            if (Encoding.Default.GetByteCount(bidUnit) > 150)
                            {
                                bidUnit = bidUnit.Substring(0, 150);
                            }
                            Regex regprjMgr = new Regex(@"(项目经理)：[^\r\n]+\r\n");
                            prjMgr = regprjMgr.Match(bidCtx).Value.Replace("项目经理：", "").Trim();
                            if (string.IsNullOrEmpty(prjMgr))
                            {
                                prjMgr = string.Empty;
                            }
                            prjMgr = ToolHtml.GetSubString(prjMgr, 30);
                            Regex regOtherType = new Regex(@"(工程类型)：[^\r\n]+\r\n");
                            string oType = regOtherType.Match(bidCtx).Value.Replace("工程类型：", "").Trim();
                            if (oType.Contains("房建"))
                            {
                                otherType = "房建及工业民用建筑";
                            }
                            if (oType.Contains("市政"))
                            {
                                otherType = "市政工程";
                            }
                            if (oType.Contains("园林绿化"))
                            {
                                otherType = "园林绿化工程";
                            }
                            if (oType.Contains("装饰装修"))
                            {
                                otherType = "装饰装修工程";
                            }
                            if (oType.Contains("电力"))
                            {
                                otherType = "电力工程";
                            }
                            if (oType.Contains("水利"))
                            {
                                otherType = "水利工程";
                            }
                            if (oType.Contains("环保"))
                            {
                                otherType = "环保工程";
                            }
                            otherType = ToolHtml.GetSubString(otherType, 50);
                            oType = ToolHtml.GetSubString(oType, 50);
                            //prjName = ToolDb.GetPrjName(prjName);
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            buildUnit = ToolHtml.GetSubString(buildUnit, 150);
                            BidInfo info = null;
                            try
                            {
                                info = ToolDb.GenBidInfo("广东省", "深圳宝安区工程", "宝安区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, beginDate, beginDate, HtmlTxt);
                            }
                            catch
                            {
                                Logger.Error("出错啦");
                            }
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
