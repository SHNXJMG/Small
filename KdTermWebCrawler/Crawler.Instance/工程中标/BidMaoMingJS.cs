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
    public class BidMaoMingJS : WebSiteCrawller
    {
        public BidMaoMingJS()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省茂名市建设工程中标信息";
            this.Description = "自动抓取广东省茂名市建设工程中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://jyzx.maoming.gov.cn/mmzbtb/jyxx/";
        }

        private Dictionary<string, string> _AllSiteUrl;
        protected Dictionary<string, string> AllSiteUrl
        {
            get
            {
                if (_AllSiteUrl == null)
                {
                    _AllSiteUrl = new Dictionary<string, string>();
                    _AllSiteUrl.Add("施工", "033001/033001001/033001001003/033001001003001");
                    _AllSiteUrl.Add("勘察设计", "033001/033001001/033001001003/033001001003002");
                    _AllSiteUrl.Add("监理", "033001/033001001/033001001003/033001001003003");
                    _AllSiteUrl.Add("其他", "033001/033001001/033001001003/033001001003004");
                }
                return _AllSiteUrl;
            }
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            foreach (string siteUrl in AllSiteUrl.Keys)
            {
                int result = 0;
                string webUrl = this.SiteUrl + AllSiteUrl[siteUrl];
                string html = string.Empty;
                string cookiestr = string.Empty;
                string viewState = string.Empty;
                int pageInt = 1;
                string eventValidation = string.Empty;
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(webUrl, Encoding.UTF8, ref cookiestr);
                }
                catch
                {
                    return list;
                }

                Parser parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "Paging")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    string temp = nodeList.AsString().GetRegexBegEnd("总页数：", "当前");
                    try
                    {
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
                            html = this.ToolWebSite.GetHtmlByUrl(webUrl + "?Paging=" + i.ToString(), Encoding.UTF8);
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("valign", "top")));
                    if (tableNodeList != null && tableNodeList.Count > 0)
                    {
                        TableTag table = (TableTag)tableNodeList[0];
                        for (int j = 0; j < table.RowCount - 2; j++)
                        {
                            TableRow tr = table.Rows[j];
                            ATag aTag = tr.GetATag();
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
                            InfoUrl = "http://jyzx.maoming.gov.cn" + aTag.Link;
                            beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                            }
                            catch
                            {
                                continue;
                            }
                            parser = new Parser(new Lexer(htmldetail));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                           
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.AsHtml();
                                bidCtx = HtmlTxt.GetReplace("</p>", "\r\n").ToCtxString();
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoNormalTable")));
                                if (tableNode == null || tableNode.Count < 1)
                                {
                                    parser.Reset();
                                    tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                }
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    
                                    string ctx = string.Empty;
                                    TableTag dtlTable = tableNode[0] as TableTag;
                                    for (int r = 0; r < dtlTable.RowCount; r++)
                                    {
                                        bool isBid = false;
                                        for (int c = 0; c < dtlTable.Rows[r].ColumnCount; c++)
                                        {
                                            string temp = dtlTable.Rows[r].Columns[c].ToNodePlainString();
                                            if (temp.Contains("中标人") || temp.Contains("中标候选人"))
                                            {
                                                isBid = true;
                                                continue;
                                            }
                                            else
                                            {
                                                if (isBid)
                                                {
                                                    if (c % 2 == 0)
                                                        ctx += temp + "\r\n";
                                                    else
                                                        ctx += temp + "：";
                                                }
                                                else
                                                {
                                                    if (c % 2 == 0)
                                                        ctx += temp + "：";
                                                    else
                                                        ctx += temp + "\r\n";
                                                }
                                            }

                                        }
                                    }
                                    buildUnit = ctx.GetRegex("招标单位");
                                    if (string.IsNullOrWhiteSpace(buildUnit))
                                        buildUnit = ctx.GetBuildRegex();
                                    prjAddress = ctx.GetAddressRegex();
                                    bidUnit = ctx.GetBidRegex();
                                    if (string.IsNullOrWhiteSpace(bidUnit))
                                        bidUnit = ctx.GetRegex("单位名称");
                                    bidMoney = ctx.GetMoneyRegex();
                                    if (siteUrl.Equals("施工"))
                                        prjMgr = ctx.GetRegex("项目（经理）负责人");
                                    else if (siteUrl.Contains("勘察"))
                                        prjMgr = ctx.GetRegex("勘察负责人");
                                    else if (siteUrl.Equals("监理"))
                                        prjMgr = ctx.GetRegex("总监理工程师,项目总监");
                                    else
                                        prjMgr = ctx.GetRegex("项目（经理）负责人");
                                }
                                else
                                {
                                    buildUnit = bidCtx.GetBuildRegex();
                                    prjAddress = bidCtx.GetAddressRegex();
                                    bidUnit = bidCtx.GetBidRegex();
                                    bidMoney = bidCtx.GetMoneyRegex();
                                    if (siteUrl.Equals("施工"))
                                        prjMgr = bidCtx.GetRegex("项目（经理）负责人");
                                    else if (siteUrl.Contains("勘察"))
                                        prjMgr = bidCtx.GetRegex("勘察负责人");
                                    else if (siteUrl.Equals("监理"))
                                        prjMgr = bidCtx.GetRegex("总监理工程师,项目总监");
                                    else
                                        prjMgr = bidCtx.GetRegex("项目（经理）负责人");
                                }

                                msgType = "茂名市公共资源交易中心";
                                specType = "建设工程";
                                if (bidUnit.Contains("候选人"))
                                    bidUnit = string.Empty;
                                if (Encoding.Default.GetByteCount(prjMgr) > 50)
                                    prjMgr = string.Empty;
                                prjName = ToolDb.GetPrjName(prjName);
                                bidType = siteUrl;
                                BidInfo info = ToolDb.GenBidInfo("广东省", "茂名市区", "", string.Empty, code, prjName, buildUnit, beginDate,
                                  bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                  bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                result++;
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int a = 0; a < aNode.Count; a++)
                                    {
                                        ATag fileTag = aNode[a] as ATag;
                                        if (fileTag.IsAtagAttach())
                                        {
                                            string fileUrl = string.Empty;
                                            if (fileTag.Link.Contains("http"))
                                                fileUrl = fileTag.Link;
                                            else
                                                fileUrl = "http://jyzx.maoming.gov.cn/" + fileTag.Link;

                                            base.AttachList.Add(ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, fileUrl));
                                        }
                                    }
                                }
                                if (result >= this.MaxCount && !crawlAll)
                                    goto Finish;
                            }
                        }
                    }
                }
            Finish: continue;
            }
            return list;
        }
    }
}
