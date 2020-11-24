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
    public class BidSztc : WebSiteCrawller
    {
        public BidSztc()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "深圳市国际招标有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳市国际招标有限公司中标信息";
            this.SiteUrl = "http://new.sztc.com/bidNotice/index.jhtml";
            this.MaxCount = 40;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl);
            }
            catch
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagination")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                string pageTemp = tdNodes.AsString().Replace("&nbsp;", "");
                try
                {
                    pageInt = int.Parse(pageTemp.GetRegexBegEnd("/", "页"));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://new.sztc.com/bidNotice/index_" + i + ".jhtml");
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "lb-link")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.LinkText.ToNodeString().Replace(" ", "");

                        beginDate = prjName.GetDateRegex();
                        if (!string.IsNullOrEmpty(prjName))
                            prjName = prjName.Replace(beginDate, "");
                        InfoUrl = aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }

                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "ninfo-con"), new TagNameFilter("div")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</p>,<br/>", "\r\n").ToCtxString().GetReplace("\t","").GetReplace("\r\n\r\n,\r\n\r\n,\r\n\r\n,\r\n\r\n,\r\n\r\n,\r\n\r\n,\r\n\r\n,\r\n\r\n,\r\n\r\n,\r\n\r\n,\r\n\r\n", "\r\n");
                            bidCtx = bidCtx.GetReplace("单位:\r\n,单位：\r\n", "单位：").GetReplace("中标人:\r\n,中标人：\r\n", "中标人：").GetReplace("编号:\r\n,编号：\r\n", "编号：");

                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();

                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.Replace("和中标金额", "").GetMoneyRegex(new string[] { "中标金额" });
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                bidMoney = bidCtx.GetMoneyRegex(); 
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (dtlNode != null && dtlNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag table = dtlNode[0] as TableTag;
                                    for (int r = 0; r < table.Rows[0].ColumnCount; r++)
                                    {
                                        try
                                        {
                                            ctx += table.Rows[0].Columns[r].ToNodePlainString() + "：";
                                            ctx += table.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                        }
                                        catch { }
                                    }
                                    bidUnit = ctx.GetBidRegex();
                                    bidMoney = ctx.GetMoneyRegex(new string[] { "中标金额" });
                                    if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                        bidMoney = ctx.GetMoneyRegex();
                                }
                            }
                            if (bidUnit.Contains("名称"))
                                bidUnit = bidUnit.Replace("名称","");
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("包号"))
                                bidUnit = "";

                            specType = "政府采购";
                            msgType = "深圳市国际招标有限公司";
                            bidType = prjName.GetInviteBidType();
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            dtlparser = new Parser(new Lexer(HtmlTxt));
                            NodeList FileTag = dtlparser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (FileTag != null && FileTag.Count > 0)
                            {
                                for (int f = 0; f < FileTag.Count; f++)
                                {
                                    ATag file = FileTag[f] as ATag;
                                    if (file.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (file.Link.ToLower().Contains("http"))
                                            link = file.Link;
                                        else
                                            link = "http://new.sztc.com/" + file.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(file.ToPlainTextString(), info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount)
                            {
                                return list;
                            }
                        }
                    }
                }
            }
            return list;
        }

    }

}
