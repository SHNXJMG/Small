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
    public class BidFoshanSS : WebSiteCrawller
    {
        public BidFoshanSS()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省佛山市三水区";
            this.Description = "自动抓取广东省佛山市三水区中标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.SiteUrl = "http://ssggzy.ss.gov.cn/CgtExpandFront/tender/list.do?cid=3&type=0";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        { 
            IList list = new List<BidInfo>();
            //取得页码
            int pageInt = 1; 
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch  
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "page"), new TagNameFilter("div")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToPlainTextString().GetRegexBegEnd("/", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&currentPage=" + i, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "730")));
                if (sNode != null && sNode.Count > 0)
                {
                    TableTag table = sNode[0] as TableTag;
                    for (int t = 1; t < table.RowCount; t++)
                    {
                        TableRow tr = table.Rows[t];
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt=string.Empty;
                        code = tr.Columns[1].ToNodePlainString();
                        prjName = tr.Columns[2].ToNodePlainString();
                        bidType = tr.Columns[3].ToNodePlainString();
                        beginDate = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://ssggzy.ss.gov.cn" + tr.Columns[2].GetATagHref();
                        if (InfoUrl.ToLower().Contains("url="))
                        {
                            InfoUrl = "http://ssggzy.ss.gov.cn" + InfoUrl.Substring(InfoUrl.ToLower().IndexOf("url=")).Replace("url=", "");
                        } 
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch  { continue; }
                          parser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                TableTag dtlTable = dtlNode[dtlNode.Count-1] as TableTag;
                                for (int d= 0; d < dtlTable.RowCount;d++)
                                {
                                    for (int k = 0; k < dtlTable.Rows[d].ColumnCount; k++)
                                    {
                                        if ((k + 1) % 2 == 0)
                                            bidCtx += dtlTable.Rows[d].Columns[k].ToNodePlainString() + "\r\n";
                                        else
                                            bidCtx += dtlTable.Rows[d].Columns[k].ToNodePlainString() + "：";
                                    }
                                }
                            } 
                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            prjMgr = bidCtx.GetRegex("项目负责人/证书号");
                            if (prjMgr.Contains("/"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                            msgType = "佛山市三水区建设工程交易中心";
                            specType = "建设工程";
                            prjName = ToolDb.GetPrjName(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "佛山市区", "三水区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag aTag = aNode[k] as ATag;
                                    if (aTag.IsAtagAttach())
                                    {
                                        BaseAttach attach = ToolDb.GenBaseAttach(aTag.LinkText, info.Id, aTag.Link);
                                        base.AttachList.Add(attach);
                                    }
                                }
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