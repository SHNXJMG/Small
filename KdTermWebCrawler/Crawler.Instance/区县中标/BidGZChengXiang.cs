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

namespace Crawler.Instance
{
    public class BidGZChengXiang : WebSiteCrawller
    {
        public BidGZChengXiang()
            : base()
        {
            this.Group = "区县中标信息";
            this.Title = "广东省广州市城乡小型项目专区";
            this.Description = "自动抓取广东省广州市城乡小型项目专区";
            this.PlanTime = "9:18,10:19,14:13,16:14";
            this.SiteUrl = "http://www.gzzb.gd.cn/cms/wz/view/zq/CountryServlet?method=getZbhxrgsInfoList&channelId=00093";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tzgg_right_page"))), new TagNameFilter("table")));
            parser = new Parser(new Lexer(sNode.ToHtml()));
            sNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
            //NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tzgg_right_page"))), new TagNameFilter("table"))), new TagNameFilter("a")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    ATag aTag = sNode.SearchFor(typeof(ATag), true)[sNode.Count - 3] as ATag;
                    string temp = aTag.Link.Substring(aTag.Link.ToLower().IndexOf("page"), aTag.Link.Length - aTag.Link.ToLower().IndexOf("page"));
                    temp = temp.Remove(temp.IndexOf("&")).ToLower().Replace("page=", "");
                    pageInt = Convert.ToInt32(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.gzzb.gd.cn/cms/wz/view/zq/CountryServlet?method=getZbhxrgsInfoList&channelId=00093&page=" + i.ToString(), Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "zbgg_right_table"))), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
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
                        code = tr.Columns[1].ToPlainTextString();
                        prjName = tr.Columns[2].ToPlainTextString().ToNodeString().Replace(" ", "");
                        beginDate = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        bidType = prjName.GetInviteBidType();
                        InfoUrl = "http://www.gzzb.gd.cn" + (tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag).Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                            htmldetail = htmldetail.GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldetail));
                        string htlDtl = string.Empty;
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "gridtable"), new TagNameFilter("table")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            TableTag tab = dtl[0] as TableTag;
                            for (int t = 1; t < tab.RowCount; t++)
                            {
                                for (int c = 0; c < tab.Rows[t].ColumnCount; c++)
                                {
                                    if (c == 0)
                                        htlDtl += tab.Rows[t].Columns[c].ToPlainTextString().Trim() + "：";
                                    else if (c == 1)
                                        htlDtl += tab.Rows[t].Columns[c].ToPlainTextString().Trim() + "\r\n";
                                    else
                                        break;
                                }
                            }
                        }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "contentDiv"), new TagNameFilter("div")));
                        HtmlTxt = dtnode.AsHtml();
                        Regex regexCtx = new Regex(@"<[^>]*>");
                        bidCtx = regexCtx.Replace(System.Web.HttpUtility.HtmlDecode(dtnode.AsString().Replace("&#1;", "")), "");
                        string ctxTemp = regexCtx.Match(bidCtx.Replace("[if", "<if").Replace("if]", "if>")).Value;
                        if (!string.IsNullOrEmpty(ctxTemp))
                        {
                            bidCtx = bidCtx.Replace(ctxTemp, "");
                        }

                        Regex regPrjMgr = new Regex(@"(项目经理姓名及资质证书编号|项目负责人姓名及证书编号|项目经理|姓名)(:|：)[^\r\n]+\r\n");
                        prjMgr = regPrjMgr.Match(htlDtl).Value.Replace("项目负责人姓名及证书编号", "").Replace("项目经理姓名及资质证书编号", "").Replace("项目经理", "").Replace("姓名", "").Replace(":", "").Replace("：", "").Trim();
                        if (!string.IsNullOrEmpty(prjMgr))
                        {
                            string mgr = Regex.Replace(prjMgr, @"[\u4E00-\u9FA5]", "");
                            if (!string.IsNullOrEmpty(mgr))
                                prjMgr = prjMgr.Replace(mgr, "");
                        }
                        if (prjMgr.Contains("/"))
                        {
                            prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                        }

                        prjMgr = prjMgr.Replace("见附件", "");
                        Regex regBuildUnit = new Regex(@"(异议受理部门(招标人)|招标单位|(招标人)|招标单位（盖章）|招标人名称)(:|：)[^\r\n]+\r\n");
                        buildUnit = regBuildUnit.Match(bidCtx).Value.Replace("异议受理部门(招标人)", "").Replace("招标单位", "").Replace("(招标人)", "").Replace("招标人名称", "").Replace("（盖章）", "").Replace(":", "").Replace("：", "").Trim();

                        Regex regBidUnit = new Regex(@"(单位名称|中标单位|单位)(:|：)[^\r\n]+\r\n");
                        bidUnit = regBidUnit.Match(htlDtl).Value.Replace("单位名称", "").Replace("中标单位", "").Replace("单位", "").Replace(":", "").Replace("：", "");

                        Regex regBidMoneystr = new Regex(@"(投标价（万元）|投标价|金额|价格|报价)(：|:)[^\r\n]+\r\n");
                        string monerystr = regBidMoneystr.Match(htlDtl).Value.Replace("投标价（万元）", "").Replace("投标价", "").Replace("金额", "").Replace("价格", "").Replace("报价", "").Replace("：", "").Replace(":", "").Replace(",", "").Replace("，", "").Trim();
                        Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");

                        if (!string.IsNullOrEmpty(regBidMoney.Match(monerystr).Value))
                        {
                            if (htlDtl.Contains("万元") || htlDtl.Contains("万美元"))
                            {
                                bidMoney = regBidMoney.Match(monerystr).Value;
                            }
                            else
                            {
                                try
                                {
                                    bidMoney = (decimal.Parse(regBidMoney.Match(monerystr).Value) / 10000).ToString();
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
                        if (!string.IsNullOrEmpty(buildUnit) && buildUnit.Contains("日期"))
                        {
                            buildUnit = buildUnit.Remove(buildUnit.IndexOf("日期"));
                        }
                        if (string.IsNullOrEmpty(bidUnit))
                        {
                            bidUnit = htlDtl.GetBidRegex();
                        }
                        if (string.IsNullOrEmpty(bidUnit))
                        {
                            bidUnit = htlDtl.GetRegex("承包意向人名称");
                        }
                        if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                        {
                            bidMoney = bidCtx.GetRegexBegEnd("交易价为", "元").GetMoney();
                        }
                        if (string.IsNullOrEmpty(buildUnit))
                        {
                            buildUnit = bidCtx.GetBuildRegex();
                        }
                        if (string.IsNullOrEmpty(prjMgr))
                        {
                            prjMgr = htlDtl.GetRegex("项目负责人姓名及证书编号");
                            if (!string.IsNullOrEmpty(prjMgr))
                            {
                                string mgr = Regex.Replace(prjMgr, @"[\u4E00-\u9FA5]", "");
                                if (!string.IsNullOrEmpty(mgr))
                                    prjMgr = prjMgr.Replace(mgr, "");
                            }
                            if (prjMgr.Contains("/"))
                            {
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                            }
                        }
                        msgType = "广州建设工程交易中心";
                        specType = "建设工程";
                        BidInfo info = ToolDb.GenBidInfo("广东省", "广州市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                               bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        list.Add(info);
                        parser = new Parser(new Lexer(HtmlTxt));
                        NodeList aList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        if (aList != null && aList.Count > 0)
                        {
                            for (int c = 0; c < aList.Count; c++)
                            {
                                ATag a = aList[c] as ATag;
                                if (a.Link.IsAtagAttach())
                                {
                                    string alink = "http://www.gzzb.gd.cn" + a.Link;
                                    BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, alink);
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
