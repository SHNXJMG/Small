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
    public class BidSZJYZX : WebSiteCrawller
    {
        public BidSZJYZX()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省深圳市区";
            this.Description = "自动抓取广东省深圳市区中标信息";
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,10:20,10:50,11:30,14:05,14:25,14:50,15:25,16:00,16:50,19:00";
            this.MaxCount = 50;
            this.SiteUrl = "http://www.szjsjy.com.cn/BusinessInfo/ZBGSInfoList.aspx?id=100";
            this.ExistsHtlCtx = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {

            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));

            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("cellspacing", "2"), new TagNameFilter("table")));
            string pageString = sNode.AsString();
            Regex regexPage = new Regex(@"，共[^页]+页，");
            Match pageMatch = regexPage.Match(pageString);
            try { pageInt = int.Parse(pageMatch.Value.Replace("，共", "").Replace("页，", "").Trim()); }
            catch (Exception) { }

            string cookiestr = string.Empty;
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "__VIEWSTATEENCRYPTED", "__EVENTVALIDATION", "ctl00$hdnPageCount" }, new string[] { "ctl00$Content$GridView1", "Page$"+i.ToString(), viewState, "",eventValidation, pageInt.ToString() });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }

                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_Content_GridView1"), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
                       bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j] as TableRow;
                        code = tr.Columns[1].ToPlainTextString().Trim();
                        prjName = tr.Columns[2].ToPlainTextString().Trim();
                        buildUnit = tr.Columns[4].ToPlainTextString().Trim();
                        bidUnit = tr.Columns[5].ToPlainTextString().Trim();
                        bidMoney = tr.Columns[6].ToPlainTextString().Replace("万元", "").Trim();
                        beginDate = tr.Columns[3].ToPlainTextString().Split('至')[0].Replace("年", "-").Replace("月", "-").Replace("日", " ").Replace("时", "").Trim();
                        endDate = tr.Columns[3].ToPlainTextString().Split('至')[1].Replace("年", "-").Replace("月", "-").Replace("日", " ").Replace("时", "").Trim();
                        ATag aTag = tr.Columns[2].Children[0] as ATag;
                        InfoUrl = "http://www.szjsjy.com.cn/BusinessInfo/" + aTag.Link;

                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "lblXXNR"), new TagNameFilter("span")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                        }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "lblXXNR"), new TagNameFilter("span")));

                        bidCtx = dtnode.AsString().Replace(" ", "");
                        Regex regPrjAdd = new Regex(@"(工程地点|工程地址)：[^\r\n]+[\r\n]{1}");
                        prjAddress = regPrjAdd.Match(bidCtx).Value.Replace("工程地点：", "").Replace("工程地址：", "").Trim();
                        msgType = "深圳市建设工程交易中心";
                        specType = "建设工程";
                        Regex regprjMgr = new Regex(@"(项目经理|项目负责人|项目总监|建造师|监理师|项目经理姓名)(：|:)[^\s]+[\s]{1}");
                        prjMgr = regprjMgr.Match(bidCtx).Value.Replace("项目经理姓名", "").Replace("项目经理", "").Replace("项目总监", "").Replace("建造师", "").Replace("项目负责人", "").Replace("：", "").Replace(":", "").Replace("监理师", "").Trim();

                        string bidUnitInfo = bidCtx.GetBidRegex();

                        if (!string.IsNullOrEmpty(bidUnitInfo))
                            bidUnit = bidUnitInfo;

                        Regex regInvType = new Regex(@"[^\r\n]+[\r\n]{1}");
                        string InvType = regInvType.Match(bidCtx).Value;

                        prjName = ToolDb.GetPrjName(prjName);
                        if (!string.IsNullOrEmpty(bidUnit))
                        {
                            bidUnit = ToolDb.GetBidUnit(bidUnit);
                            if (bidUnit.Contains("报价"))
                            {
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("报价"));
                            }
                        }
                        bidType = ToolHtml.GetInviteTypes(InvType); 
                        BidInfo info = ToolDb.GenBidInfo("广东省", "深圳市工程", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, string.Empty, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        list.Add(info);

                        dtlparser.Reset();
                        NodeList dlNodes = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "trFujian"), new TagNameFilter("tr")));
                        if (dlNodes != null && dlNodes.Count > 0)
                        {
                            TableRow attr = dlNodes[0] as TableRow;
                            NodeList fileNodes = attr.SearchFor(typeof(ATag), true);
                            if (fileNodes != null && fileNodes.Count > 0)
                            {
                                for (int f = 0; f < fileNodes.Count; f++)
                                {
                                    ATag fileTag = fileNodes[f] as ATag;
                                    if (!string.IsNullOrEmpty(fileTag.Link))
                                    {
                                        BaseAttach attach = ToolDb.GenBaseAttach(fileTag.StringText, info.Id, fileTag.Link.Replace("..", "http://www.szjsjy.com.cn"));
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                        }
                        if (!crawlAll && list.Count >= this.MaxCount)
                            return list;
                    }
                } 
            } 
            return list;
        }
    }
}
