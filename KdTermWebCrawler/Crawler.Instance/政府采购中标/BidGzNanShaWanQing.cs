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
    public class BidGzNanShaWanQing : WebSiteCrawller
    {
        public BidGzNanShaWanQing()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "广东省广州市南沙区万顷沙镇人民政府中标公告";
            this.Description = "自动抓取广东省广州市南沙区万顷沙镇人民政府中标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://www.gzwqs.gov.cn/special_topic/wqsggzyjyxx/ggzyzbgs/";
            this.MaxCount = 80;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 18;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "index_" + (i - 1).ToString() + ".html", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "gllist")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = listNode[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();

                        InfoUrl = this.SiteUrl + aTag.Link.GetReplace("../,./");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "nr")));
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
                            NodeList bidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (bidNode != null && bidNode.Count > 0)
                            {
                                string ctx = string.Empty;
                                TableTag table = bidNode[0] as TableTag;
                                for (int r = 1; r < table.RowCount; r++)
                                {
                                    try
                                    {
                                        ctx += table.Rows[r].Columns[0].ToNodePlainString() + "：";
                                        ctx += table.Rows[r].Columns[1].ToNodePlainString() + "\r\n";
                                    }
                                    catch { }
                                }
                                bidUnit = ctx.GetRegex("单位名称");
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = ctx.GetBidRegex();
                                bidMoney = ctx.GetMoneyRegex(null, false, "万元");
                                try
                                {
                                    if (decimal.Parse(bidMoney) > 2000 || decimal.Parse(bidMoney) < 1)
                                        bidMoney = "0";
                                }
                                catch { }
                            }
                            bidType = prjName.GetInviteBidType();
                            msgType = "广州市南沙区万顷沙镇人民政府";
                            specType = "政府采购";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "广州政府采购", "南沙区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                        {
                                            int length = InfoUrl.LastIndexOf("/");
                                            if (length > 0)
                                            {
                                                fileLink = InfoUrl.Remove(length) + "/" + fileAtag.Link.GetReplace("../,./");
                                            }

                                        }
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
