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
using System.Collections.Specialized;

namespace Crawler.Instance
{
    /// <summary>
    /// 广东省惠州市大亚湾区
    /// </summary>
    public class BidHuiZhouDYWQ : WebSiteCrawller
    {
        public BidHuiZhouDYWQ()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省惠州市大亚湾区";
            this.Description = "自动抓取广东省惠州市大亚湾区中标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.ExistCompareFields = "Prov,City,Area,Road,Code,ProjectName,InfoUrl";
            this.SiteUrl = "http://www.ebc.huizhou.gov.cn/index/showList/000000000004/000000000395";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;    
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                  
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "scott"))), new HasChildFilter(new TagNameFilter("a")))).SearchFor(typeof(ATag), true);
            for (int i = 0; i < sNodes.Count; i++)
            {
                ATag aTag = sNodes[i] as ATag;
                if (aTag.ToPlainTextString().Contains(">>"))
                {
                    pageInt = int.Parse(aTag.Link.ToLower().Replace("gopage(", "").Replace(")", ""));
                }
            }
            parser.Reset();
            //处理后续页
            if (pageInt > 1)
            {
                string cookiestr = string.Empty;
                for (int i = 1; i <= pageInt; i++)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "boardId", "eTime", "newstitle", "pageNO", "sTime", "totalRows", "typeId" }, new string[] { "000000000201", string.Empty, string.Empty, i.ToString(), string.Empty, "0", "000000000002" });

                  
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default, ref cookiestr);

                        DealHtml(list, html, crawlAll);
                    }
                    catch (Exception ex)
                    {
                          
                        continue;
                    }
                 

                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }

            return list;
        }

        public void DealHtml(IList list, string html, bool crawlAll)
        {

            Parser parserDtl = new Parser(new Lexer(html));
            NodeList aNodes = parserDtl.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "lefttable")));
            if (aNodes != null && aNodes.Count > 0)
            {
                Type typs = typeof(ATag);
                TableTag table = aNodes[0] as TableTag;
                for (int t = 1; t < table.RowCount - 1; t++)
                {
                    string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty,bidType=string.Empty,
                         inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, ctx = string.Empty, CreateTime = string.Empty, FbTime = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, HtmlTxt=string.Empty;

                    TableRow tr = table.Rows[t] as TableRow;
                    ATag aTag = tr.SearchFor(typeof(ATag), true)[0] as ATag;

                    InfoUrl = aTag.Link;
                    prjName = table.Rows[t].Columns[1].ToPlainTextString().Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", " ").Trim();
                    endDate = table.Rows[t].Columns[2].ToPlainTextString().Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", " ").Trim();
                    string htmlDtl = string.Empty;
                    try
                    {

                        htmlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                    }
                    catch (Exception ex)
                    {
                          
                        continue;
                    }
                    Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                    htmlDtl = regexHtml.Replace(htmlDtl, "");
                    Parser parserCtx = new Parser(new Lexer(htmlDtl));

                    NodeList ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "printTb lefttable")));
                    if (ctxNode != null && ctxNode.Count > 0)
                    {
                        Parser parserdiv = new Parser(new Lexer(htmlDtl));
                        NodeList aNodesdiv = parserdiv.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "biuuu_button")));
                        HtmlTxt = ctxNode.AsHtml().Replace(aNodesdiv.AsHtml(), "").Trim();
                        Type tp = typeof(ATag);
                        TableTag tabTag = ctxNode[0] as TableTag;

                        string startTime = tabTag.Rows[1].Columns[0].ToPlainTextString().Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", " ").Trim();
                        Regex regex = new Regex(@"时间：\d{4}-\d{1,2}-\d{1,2} \d{1,2}:\d{1,2}:\d{1,2}");
                        Match math = regex.Match(startTime);
                        beginDate = math.Value.Replace("时间：", "").Trim();

                        Regex regexcode = new Regex("(工程编号|项目编号)：[^\r\n]+[\r\n]{1}");
                        Match match = regexcode.Match(tabTag.ToPlainTextString());
                        if (match.Value.Length > 0)
                        {
                            code = match.Value.Substring(match.Value.IndexOf("：") + 1).Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", " ").Trim();
                        }
                        Regex regexBuildUnit = new Regex("(中标人|中标单位)：[^\r\n]+[\r\n]{1}");
                        Match matchBuildUnit = regexBuildUnit.Match(tabTag.ToPlainTextString());
                        if (matchBuildUnit.Value.Length > 0)
                        {
                            buildUnit = matchBuildUnit.Value.Substring(matchBuildUnit.Value.IndexOf("：") + 1).Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", " ").Trim();
                        }
                        Regex regexbidUnit = new Regex("(招标人|建设单位)：[^\r\n]+[\r\n]{1}");
                        Match matchbidUnit = regexbidUnit.Match(tabTag.ToPlainTextString());
                        if (matchbidUnit.Value.Length > 0)
                        {
                            bidUnit = matchbidUnit.Value.Replace("招标人：", "").Replace("建设单位：", "").Replace("\r\n", "").Replace("\t", "").Replace("&nbsp;", " ").Trim();
                        }

                        Regex regexMoney = new Regex("(中标价|其中标价为|中标价格)：[^\r\n]+[\r\n]{1}");
                        Match matchMoney = regexMoney.Match(tabTag.ToPlainTextString());
                        if (matchMoney.Value.Length > 0)
                        {
                            bidMoney = matchMoney.Value.Replace("中标价：", "").Replace("其中标价为：", "").Replace("中标价格：", "").Replace("\r", "");
                        }
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
                        if (Encoding.Default.GetByteCount(code) > 50)
                        {
                            code = "";
                        }
                        if (buildUnit == "" || buildUnit == null)
                        {
                            buildUnit = "";
                        }
                        if (Encoding.Default.GetByteCount(buildUnit)> 150)
                        {
                            buildUnit = buildUnit.Substring(0, 150);
                        }
                        if (bidUnit == "" || bidUnit == null)
                        {
                            bidUnit = "";
                        }
                        if (bidUnit.Length > 75)
                        {
                            bidUnit = bidUnit.Substring(0, 150);
                        }
                        ctx = tabTag.Rows[2].Columns[0].ToPlainTextString().Replace("&nbsp;", " ").Replace("\r\n\r\n\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                        if (ctx.Length > 0)
                        {
                            Regex regexCtx = new Regex("<!--[^<]+-->");
                            ctx = regexCtx.Replace(ctx, "");
                        }
                    }

                    parserCtx.Reset();

                    ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "toptd_bai")));
                    Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                    beginDate = regDate.Match(ctxNode.AsString()).Value.Trim();
                    if (ctx.Contains("公示开始时间"))
                    {
                        beginDate = ctx.Substring(ctx.IndexOf("公示开始时间")).ToString();
                        Regex regBeDate = new Regex(@"\d{4}年\d{1,2}月\d{1,2}日");
                        beginDate = regBeDate.Match(beginDate).Value.Trim();
                    }
                    if (beginDate == "")
                    {
                        beginDate = regDate.Match(ctxNode.AsString()).Value.Trim();
                    }
                    if (beginDate == "")
                    {
                        beginDate = string.Empty;
                    }
                    prjName = ToolDb.GetPrjName(prjName);
                    bidType = ToolHtml.GetInviteTypes(prjName);
                    BidInfo info = ToolDb.GenBidInfo("广东省", "惠州市区", "大亚湾区", string.Empty, code, prjName, bidUnit, beginDate, buildUnit, beginDate, endDate, ctx, string.Empty, "惠州市建设工程交易中心",bidType, "建设工程", string.Empty, bidMoney, InfoUrl, string.Empty, HtmlTxt);

                    list.Add(info);
                    ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("a"), new HasAttributeFilter("target", "_blank")));
                    NodeList aTagNodes = ctxNode.SearchFor(typeof(ATag), true);
                    for (int a = 0; a < aTagNodes.Count; a++)
                    {

                        ATag fileTage = aTagNodes[a] as ATag;
                        if (fileTage.Link.Contains("http://www.ebc.huizhou.gov.cn/index/loadNewsFile"))
                        {
                            string downloadURL = fileTage.Link;
                            BaseAttach attach = ToolDb.GenBaseAttach(fileTage.ToPlainTextString(), info.Id, downloadURL);
                            base.AttachList.Add(attach);
                        }
                    }
                    if (!crawlAll && list.Count >= this.MaxCount) return;
                }
            }
        }


    }

}
