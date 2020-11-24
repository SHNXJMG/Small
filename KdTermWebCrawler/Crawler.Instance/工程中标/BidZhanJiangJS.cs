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
    public class BidZhanJiangJS : WebSiteCrawller
    {
        public BidZhanJiangJS()
            : base()   
        {
            this.Group = "中标信息";
            this.Title = "广东省湛江市建设工程中标信息";
            this.Description = "自动抓取广东省湛江市建设工程中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.SiteUrl = "http://zb.zjcic.net/Default.aspx?tabid=89";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default, ref cookiestr);
            }
            catch (Exception ex)
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("id", "dnn_ctr476_ArticleList_cboPages")));
            if (nodeList != null && nodeList.Count > 0)
            {
                string oo = nodeList.AsString().Trim();
                page = Convert.ToInt32(oo.Substring(oo.LastIndexOf("第")).ToString().Replace("第", "").Replace("页", "").Trim());
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(htl);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__LASTFOCUS"  ,
                        "__VIEWSTATE",
                        "dnn$ctr476$ArticleList$cboPages",
                        "ScrollTop",
                        "__dnnVariable"
                    }, new string[]{
                        "dnn$ctr476$ArticleList$cmdNext",
                       string.Empty,
                        string.Empty,
                        viewState,
                        (i-2).ToString(),
                        "716",
                        eventValidation
                    });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "dnn_ctr476_ArticleList_PanelA")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = tableNodeList.SearchFor(typeof(TableTag), true)[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        if (tr.ColumnCount < 2)
                        {
                            continue;
                        }
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
                        prjName = tr.Columns[0].ToPlainTextString().Trim();
                        beginDate = tr.Columns[1].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[0].SearchFor(typeof(ATag), true)[0] as ATag;

                        InfoUrl = "http://zb.zjcic.net" + aTag.Link.Replace("amp;", "").Trim();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            Logger.Error("BidZhanJiangJS"); 
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "dnn_ctr377_ArticleShow_lblContent")));
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            bidCtx = dtnode.AsString().Trim().Replace("&#160;", "").Trim();
                            if (bidCtx.Contains("推荐"))
                            {
                                bidUnit = bidCtx.Substring(bidCtx.IndexOf("推荐")).Replace("推荐", "").Trim();
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("中标")).Trim();
                                if (bidUnit.Contains("公司"))
                                {
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司") + 2).Replace("：", "").Trim();
                                }
                                if (bidUnit.Contains("设计院"))
                                {
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("设计院") + 3).Replace(":", "").Replace("：", "").Trim();
                                }
                            }
                            if (bidCtx.Contains("中标原则") && bidUnit == "")
                            {
                                bidUnit = bidCtx.Substring(bidCtx.IndexOf("中标原则")).Replace("中标原则", "").Replace("，", "").Trim();
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("中标")).Trim();
                                if (bidUnit.Contains("公司"))
                                {
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司") + 2).Replace("：", "").Replace("，", "").Trim();
                                }
                                if (bidUnit.Contains("院"))
                                {
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("设计院") + 3).Replace("为", "").Replace("：", "").Replace(":", "").Trim();
                                }
                            }
                            if (bidCtx.Contains("定标原则") && bidUnit == "")
                            {
                                bidUnit = bidCtx.Substring(bidCtx.IndexOf("定标原则")).Replace("定标原则", "").Replace("，", "").Trim();
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("中标")).Trim();
                                if (bidUnit.Contains("公司"))
                                {
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司") + 2).Replace("：", "").Replace("，", "").Trim();
                                }
                                if (bidUnit.Contains("院"))
                                {
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("设计院") + 3).Replace("为", "").Replace("：", "").Replace(":", "").Trim();
                                }
                            }
                            if (bidCtx.Contains("评标办法") && bidUnit == "以")
                            {
                                bidUnit = bidCtx.Substring(bidCtx.IndexOf("评标办法")).Replace("评标办法", "").Replace("，", "").Trim();
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("中标")).Trim();
                                if (bidUnit.Contains("公司"))
                                {
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司") + 2).Replace("：", "").Replace("，", "").Trim();
                                }
                                if (bidUnit.Contains("院"))
                                {
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("设计院") + 3).Replace("为", "").Replace("：", "").Replace(":", "").Trim();
                                }
                            }
                            if (bidCtx.Contains("中标价："))
                            {
                                bidMoney = bidCtx.Substring(bidCtx.IndexOf("中标价：")).Replace("中标价：", "").Trim();
                                if (bidMoney.Contains("元"))
                                {
                                    bidMoney = bidMoney.Remove(bidMoney.IndexOf("元") + 1).Trim();
                                }
                            }
                            if (bidCtx.Contains("项目负责人："))
                            {
                                prjMgr = bidCtx.Substring(bidCtx.IndexOf("项目负责人：")).Replace("项目负责人：", "").Trim();
                                prjMgr = prjMgr.Substring(0, 4).Replace("）", "").Replace("。", "").Replace("，", "").Replace("；", "").Trim();
                            }
                            Regex regBuidUnit = new Regex(@"(招标人|招标单位)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(bidCtx).Value.Replace("招标人：", "").Replace("招标单位：", "").Trim();
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
                            msgType = "湛江市建设工程交易中心";
                            specType = "建设工程";
                            if (bidUnit == "的第一")
                            {
                                if (bidCtx.Contains("候选人"))
                                {
                                    bidUnit = bidCtx.Substring(bidCtx.IndexOf("候选人")).Replace("候选人", "").Trim();
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("中标")).Trim();
                                    if (bidUnit.Contains("公司"))
                                    {
                                        bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司") + 2).Replace("：", "").Trim();
                                    }
                                    if (bidUnit.Contains("设计院"))
                                    {
                                        bidUnit = bidUnit.Remove(bidUnit.IndexOf("设计院") + 3).Replace("为", "").Replace("：", "").Replace(":", "").Trim();
                                    }
                                }
                            }
                            if (bidUnit == "了")
                            {
                                parserdetail.Reset();
                                NodeList dtnodeF = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoNormalTable")));
                                if (dtnodeF.Count <= 0)
                                {
                                    parserdetail.Reset();
                                    dtnodeF = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoTableGrid")));
                                }
                                if (dtnodeF.Count > 0)
                                {
                                    string bitext = string.Empty;
                                    TableTag tableone = (TableTag)dtnodeF[0];
                                    for (int row = 0; row < tableone.RowCount; row++)
                                    {
                                        TableRow r = tableone.Rows[row];

                                        for (int k = 0; k < r.ColumnCount; k++)
                                        {
                                            string st = string.Empty;
                                            string st1 = string.Empty;
                                            st = r.Columns[k].ToPlainTextString().Trim();
                                            if (k + 1 < r.ColumnCount)
                                            {
                                                st1 = r.Columns[k + 1].ToPlainTextString().Trim();
                                            }
                                            bitext += st + "：" + st1 + "\r\n";
                                            if (k + 1 <= r.ColumnCount)
                                            {
                                                k++;
                                            }
                                        }
                                    }
                                    bitext = bitext.Replace("（", "").Replace("）", "").Trim();
                                    Regex regBidUnit = new Regex(@"单位名称(：|:)[^\r\n]+\r\n");
                                    bidUnit = regBidUnit.Match(bitext).Value.Replace("中标单位：", "").Trim();
                                    Regex regMoney = new Regex(@"(中标价|中标价格)(：|:)[^\r\n]+\r\n");
                                    bidMoney = regMoney.Match(bitext).Value.Replace("中标价：", "").Replace("中标价格：", "").Replace(",", "").Trim();
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
                                }

                            }
                            bidUnit = bidUnit.Replace("为", "").Replace("： ", "").Trim();
                            if (bidUnit == "了" || bidUnit == "以" || bidUnit == "的第一")
                            {
                                bidUnit = "";
                            }
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            prjName = ToolDb.GetPrjName(prjName);
                            prjName = prjName.Replace("·", "");
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "湛江市区", "", string.Empty, code, prjName, buildUnit, beginDate,
                                       bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                       bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return null;
        }
    }
}
