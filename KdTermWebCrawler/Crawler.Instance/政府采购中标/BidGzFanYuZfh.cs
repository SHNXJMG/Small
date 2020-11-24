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
using System.Threading;

namespace Crawler.Instance
{
    public class BidGzFanYuZfh : WebSiteCrawller
    {
        public BidGzFanYuZfh()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "广东省广州市番禺区住房和建设局中标公告";
            this.Description = "自动抓取广东省广州市番禺区住房和建设局中标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://js.panyu.gov.cn/cd_dzxxm.aspx?c=114";
            this.MaxCount = 80;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder2_lblSumPage")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString();
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{ 
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__VIEWSTATE",
                        "__VIEWSTATEGENERATOR",
                        "__EVENTVALIDATION"
                    }, new string[]{
                    "ctl00$ContentPlaceHolder2$lnkBtnNext",
                    "",
                    viewState,
                    "96852609",
                    eventValidation
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "695")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount - 1; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.GetATag();
                        prjName = aTag.LinkText.Trim();
                        beginDate = tr.Columns[1].ToPlainTextString().GetDateRegex();

                        InfoUrl = "http://js.panyu.gov.cn/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_txtContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();//.Replace("<br", "\r\n<br");
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = bidCtx.GetBuildRegex();
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "gridtable")));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                string ctx = string.Empty;
                                TableTag tab = tableNode[0] as TableTag;
                                for (int r = 1; r < tab.RowCount; r++)
                                {
                                    try
                                    {
                                        ctx += tab.Rows[r].Columns[0].ToNodePlainString() + "：";
                                        ctx += tab.Rows[r].Columns[1].ToNodePlainString() + "\r\n";
                                    }
                                    catch { }
                                }
                                bidUnit = ctx.GetRegex("单位名称");
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = ctx.GetBidRegex();
                                bidMoney = ctx.GetRegex("投标价（万元）");
                                if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                    bidMoney = ctx.GetMoneyRegex();
                                prjMgr = ctx.GetRegex("项目经理姓名及资质证书编号");
                                if (prjMgr.Contains("/"))
                                    prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                            }

                            bidType = prjName.GetInviteBidType();
                            msgType = "广州市番禺区住房和建设局";
                            specType = "政府采购"; 
                            BidInfo info = ToolDb.GenBidInfo("广东省", "广州政府采购", "番禺区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList fileNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (fileNode != null && fileNode.Count > 0)
                            {
                                for (int k = 0; k < fileNode.Count; k++)
                                {
                                    ATag fileAtag = fileNode[k].GetATag();
                                    if (fileAtag.IsAtagAttach())
                                    {
                                        string fileName = fileAtag.LinkText.ToNodeString().Replace(" ", "");
                                        string fileLink = fileAtag.Link;
                                        if (!fileLink.ToLower().Contains("http"))
                                            fileLink = "http://js.panyu.gov.cn/" + fileAtag.Link;
                                        base.AttachList.Add(ToolDb.GenBaseAttach(fileName, info.Id, fileLink));
                                    }
                                }
                            }

                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
