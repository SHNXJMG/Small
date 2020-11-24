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
    public class BidXinAn : WebSiteCrawller
    {
        public BidXinAn()
            : base()
        {
            this.Group = "街道办中标信息";
            this.Title = "广东省深圳市新安街道办事处";
            this.Description = "自动抓取广东省深圳市新安街道办事处中标信息";
            this.PlanTime = "9:23,13:55";
            this.SiteUrl = "http://xajdb.baoan.gov.cn/xxgk_11984/ywxx/zbcg/zbxxgg/index.html";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_page")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().Replace(",","kdcc");
                    string te = temp.GetRegexBegEnd("HTML", "kdcc").Replace("(","");
                    pageInt = Convert.ToInt32(te);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://xajdb.baoan.gov.cn/xxgk_11984/ywxx/zbcg/zbxxgg/index_" + (i - 1).ToString() + ".html", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "right_list"))), new TagNameFilter("ul"))), new TagNameFilter("li")));
                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
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
                        Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                        beginDate = regDate.Match(viewList[j].ToPlainTextString().Trim()).Value;
                        string temp = viewList[j].ToPlainTextString().Trim().Replace(beginDate, "");
                        try
                        {

                            string ahtml = viewList[j].ToHtml();
                            string aStr = ahtml.Replace(";", "kdxx").GetRegexBegEnd("write", "kdxx").GetReplace("(,)");
                            ATag atag = ahtml.GetATag();
                            if (string.IsNullOrWhiteSpace(atag.Link))
                            {
                                ahtml = ahtml.Replace(aStr, "");
                                ahtml = ahtml.Replace("", "kdxx").GetRegexBegEnd("write","kdxx").GetReplace("(,)");
                            }
                            int beg = temp.IndexOf("else"), end = temp.Length;
                            temp = temp.Substring(beg, end - beg);
                            beg = temp.IndexOf("<a");
                            end = temp.IndexOf("/a>");
                            temp = temp.Substring(beg, (end - beg) + 3);
                            beg = temp.IndexOf(">");
                            end = temp.IndexOf("</");
                            prjName = temp.Substring(beg + 1, end - beg - 1);
                            Parser p = new Parser(new Lexer(temp));
                            NodeList l = p.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            ATag aTag = l.SearchFor(typeof(ATag), true)[0] as ATag;
                            InfoUrl = "http://xajdb.baoan.gov.cn/xxgk_11984/ywxx/zbcg/zbxxgg/" + aTag.Link.Replace("../", "").Replace("./", "");
                        }
                        catch { continue; }
                        string htmDtl = string.Empty;
                        try
                        {
                            htmDtl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8);
                            htmDtl = Regex.Replace(htmDtl, "(<script)[\\s\\S]*?(</script>)", "");
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
                            htmDtl = regexHtml.Replace(htmDtl, "");
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "DivContent")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = Regex.Replace(dtl.AsHtml(), "(<script)[\\s\\S]*?(</script>)", "");
                            parser = new Parser(new Lexer(HtmlTxt.ToLower().Replace("th", "td")));
                            NodeList dtlTab = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (dtlTab != null && dtlTab.Count > 0)
                            { 
                                TableTag table = dtlTab[0] as TableTag;
                                for (int k = 0; k < table.RowCount; k++)
                                {
                                    for (int c = 0; c < table.Rows[k].ColumnCount; c++)
                                    {
                                        string strCtx = table.Rows[k].Columns[c].ToPlainTextString().Replace("&nbsp;", "").Replace(" ", "");
                                        if (strCtx == "工程类型")
                                            break;
                                        if (c % 2 == 0)
                                            bidCtx += strCtx + "：";
                                        else
                                            bidCtx += strCtx + "\r\n";
                                    }
                                }
                                bidCtx = bidCtx.Replace("\n", "").Replace("\r\n\r\n", "\r\n").Replace("\r", "\r\n") + "\r\n";
                            }
                            else
                            {
                                bidCtx = System.Text.RegularExpressions.Regex.Replace(HtmlTxt, "(<script)[\\s\\S]*?(</script>)", "");
                                bidCtx = Regex.Replace(bidCtx.Replace("<BR/>", "\r\n").Replace("<br/>", "\r\n").Replace("<BR>", "\r\n").Replace("<br>", "\r\n"), "<[^>]*>", "").Replace("&nbsp;", "").Replace(" ", "").Replace("\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\r\n", "\r\n") + "\r\n";
                            }
                            bidType = "工程";
                            if (prjName.Contains("施工"))
                            {
                                bidType = "施工";
                            }
                            if (prjName.Contains("监理"))
                            {
                                bidType = "监理";
                            }
                            if (prjName.Contains("设计"))
                            {
                                bidType = "设计";
                            }
                            if (prjName.Contains("勘察"))
                            {
                                bidType = "勘察";
                            }
                            if (prjName.Contains("服务"))
                            {
                                bidType = "服务";
                            }
                            if (prjName.Contains("劳务分包"))
                            {
                                bidType = "劳务分包";
                            }
                            if (prjName.Contains("专业分包"))
                            {
                                bidType = "专业分包";
                            }
                            if (prjName.Contains("小型施工"))
                            {
                                bidType = "小型工程";
                            }
                            if (prjName.Contains("设备材料"))
                            {
                                bidType = "设备材料";
                            }
                            Regex regPrjCode = new Regex(@"(工程编号|项目编号|招标编号|中标编号|编号)(:|：)[^\r\n]+\r\n");
                            code = regPrjCode.Match(bidCtx.Replace(" ", "")).Value.Replace("工程编号", "").Replace("项目编号", "").Replace("招标编号", "").Replace("中标编号", "").Replace("编号", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regBuidUnit = new Regex(@"(建设单位|招标人|承包人|招标单位|招标方|招标代理机构)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(bidCtx.Replace(" ", "")).Value.Replace("招标代理机构", "").Replace("建设单位", "").Replace("招标人", "").Replace("承包人", "").Replace("招标单位", "").Replace("招标方", "").Replace("：", "").Replace(":", "").Trim();

                            Regex regMoney = new Regex(@"(中标价|投标价|总投资|发包价|投标报价|价格|金额)(：|:|)[^\r\n]+\r\n");
                            bidMoney = regMoney.Match(bidCtx.Replace(" ", "")).Value.Replace("中标价", "").Replace("总投资", "").Replace("发包价", "").Replace("投标报价", "").Replace("投标价", "").Replace("价格", "").Replace("金额", "").Replace("：", "").Replace(":", "").Trim();

                            Regex regBidUnit = new Regex(@"(第一候选人|中标候选人|中标单位|中标人|中标方)(：|:)[^\r\n]+\r\n");
                            bidUnit = regBidUnit.Match(bidCtx.Replace(" ", "")).Value.Replace("中标候选人", "").Replace("第一候选人", "").Replace("中标单位", "").Replace("中标人", "").Replace("中标方", "").Replace("：", "").Replace(":", "").Trim();

                            Regex regprjMgr = new Regex(@"(项目经理姓名|项目经理|项目负责人|项目总监|建造师|总工程师|监理师)(：|:)[^\r\n]+\r\n");
                            prjMgr = regprjMgr.Match(bidCtx.Replace(" ", "")).Value.Replace("项目经理姓名", "").Replace("总工程师", "").Replace("项目经理", "").Replace("项目总监", "").Replace("建造师", "").Replace("监理师", "").Replace("项目负责人", "").Replace("：", "").Replace(":", "").Trim();

                            Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                            if (bidMoney.Contains("万"))
                            {
                                bidMoney = bidMoney.Remove(bidMoney.IndexOf("万")).Trim();
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
                            if (prjMgr.Contains("资格"))
                            {
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("资格"));
                            }
                            

                            if (string.IsNullOrEmpty(bidUnit) && string.IsNullOrEmpty(buildUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt.ToLower().Replace("th", "td").Replace("</p>", "\r\n").Replace("<br/>", "\r\n").Replace("<br>", "\r\n")));
                                dtlTab = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (dtlTab != null && dtlTab.Count > 0)
                                {
                                    bidCtx = "";
                                    TableTag table = dtlTab[0] as TableTag;
                                    for (int k = 0; k < table.RowCount; k++)
                                    {
                                        for (int c = 0; c < table.Rows[k].ColumnCount; c++)
                                        {
                                            string strCtx = table.Rows[k].Columns[c].ToPlainTextString().Replace("&nbsp;", "").Replace(" ", "").Replace("\n", "\r\n");
                                            if (strCtx == "工程类型")
                                                break;
                                            if (c % 2 == 0)
                                                bidCtx += strCtx + "：";
                                            else
                                                bidCtx += strCtx + "\r\n";
                                        }
                                    }
                                    bidCtx = bidCtx.Replace("\n", "").Replace("\r\n\r\n", "\r\n").Replace("\r", "\r\n") + "\r\n";
                                }
                                regBuidUnit = new Regex(@"(建设单位|招标人|承包人|招标单位|招标方|招标代理机构)(：|:)[^\r\n]+\r\n");
                                buildUnit = regBuidUnit.Match(bidCtx.Replace(" ", "")).Value.Replace("招标代理机构", "").Replace("建设单位", "").Replace("招标人", "").Replace("承包人", "").Replace("招标单位", "").Replace("招标方", "").Replace("：", "").Replace(":", "").Trim();

                                regBidUnit = new Regex(@"(第一候选人|中标候选人|中标单位|中标人|中标方)(：|:)[^\r\n]+\r\n");
                                bidUnit = regBidUnit.Match(bidCtx.Replace(" ", "")).Value.Replace("中标候选人", "").Replace("第一候选人", "").Replace("中标单位", "").Replace("中标人", "").Replace("中标方", "").Replace("：", "").Replace(":", "").Trim();

                            }
                            buildUnit = ToolHtml.GetSubString(buildUnit, 150);
                            bidUnit = ToolHtml.GetSubString(bidUnit, 150);
                            code = ToolHtml.GetSubString(code, 50);
                            prjMgr = ToolHtml.GetSubString(prjMgr, 50);
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "深圳市宝安区新安街道办事处";
                            }

                            msgType = "深圳市宝安区新安街道办事处";
                            specType = "建设工程";
                            bidType = "小型工程"; 
                            prjName = ToolDb.GetPrjName(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳区及街道工程", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                   bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
