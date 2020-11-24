using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class InviteJiangXiFazw : WebSiteCrawller
    {
        public InviteJiangXiFazw()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "江西省发展和改革委员会招、中标信息";
            this.Description = "自动抓取江西省发展和改革委员会招、中标信息";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.jxdpc.gov.cn/zdxm/zdgcztb/ztbpb/";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tdfont")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().Replace("function createPageHTML", "").GetRegexBegEnd("createPageHTML", ",").Replace("(", "");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "index_" + (i - 1) + ".htm", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellspacing", "3")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        if (table.Rows[j].ColumnCount < 2) continue;
                        string prjName = table.Rows[j].Columns[1].GetATag().GetAttribute("title");
                        if (prjName.Contains("中标"))
                            BidInfoAdd(list, table.Rows[j], crawlAll);
                        else
                            InviteInfoAdd(list, table.Rows[j], crawlAll);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }

        protected void InviteInfoAdd(IList list, TableRow tr, bool crawlAll)
        {
            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                             prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                             specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                             remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                             CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

            ATag aTag = tr.Columns[1].GetATag();
            prjName = aTag.GetAttribute("title");
            beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
            if (aTag.Link.ToLower().Contains("departmentsite"))
            {
                InfoUrl = "http://www.jxdpc.gov.cn/" + aTag.Link.Replace("../", "");
            }
            else
            {
                InfoUrl = "http://www.jxdpc.gov.cn/zdxm/zdgcztb/ztbpb/" + aTag.Link.Replace("./", "");
            }
            string htmldtl = string.Empty;
            try
            {
                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
            }
            catch { return; }
            Parser parser = new Parser(new Lexer(htmldtl));
            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "artibody")));
            if (dtlNode != null && dtlNode.Count > 0)
            {
                HtmlTxt = dtlNode.AsHtml();
                inviteCtx = HtmlTxt.ToCtxString();
                prjAddress = inviteCtx.GetAddressRegex();
                buildUnit = inviteCtx.GetBuildRegex();
                code = inviteCtx.GetCodeRegex().GetCodeDel();
                inviteType = prjName.GetInviteBidType();
                specType = "建设工程";
                msgType = "江西省发展和改革委员会";
                InviteInfo info = ToolDb.GenInviteInfo("江西省", "江西省及地市", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                list.Add(info);
                if (!crawlAll && list.Count >= this.MaxCount) return;
            }
        }

        protected void BidInfoAdd(IList list, TableRow tr, bool crawlAll)
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
            ATag aTag = tr.Columns[1].GetATag();
            prjName = aTag.GetAttribute("title");
            beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
            if (aTag.Link.ToLower().Contains("departmentsite"))
            {
                InfoUrl = "http://www.jxdpc.gov.cn/" + aTag.Link.Replace("../", "");
            }
            else
            {
                InfoUrl = "http://www.jxdpc.gov.cn/zdxm/zdgcztb/ztbpb/" + aTag.Link.Replace("./", "");
            }
            string htmldtl = string.Empty;
            try
            {
                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
            }
            catch { return; }
            Parser parser = new Parser(new Lexer(htmldtl));
            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "artibody")));
            if (dtlNode != null && dtlNode.Count > 0)
            {
                HtmlTxt = dtlNode.AsHtml();
                bidCtx = HtmlTxt.ToCtxString();
                bidUnit = bidCtx.GetBidRegex();
                bidMoney = bidCtx.GetMoneyRegex();
                prjMgr = bidCtx.GetMgrRegex();
                if (string.IsNullOrWhiteSpace(bidUnit))
                    bidUnit = bidCtx.GetRegex("排序第一的单位名称,第一中标排序人,第一名");
                if (string.IsNullOrWhiteSpace(bidUnit))
                {
                    bidUnit = bidCtx.GetRegex("1、", false);
                    if (!string.IsNullOrEmpty(bidUnit))
                        if (!bidUnit.Contains("公司"))
                            bidUnit = string.Empty;
                }

                if (string.IsNullOrWhiteSpace(bidUnit))
                    bidUnit = bidCtx.GetRegex("排序第1名");
                if (string.IsNullOrEmpty(bidUnit))
                {
                    parser = new Parser(new Lexer(HtmlTxt));
                    NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("border", "1")));
                    if (tableNode != null && tableNode.Count > 0)
                    {
                        TableTag table = tableNode[0] as TableTag;
                        string ctx = string.Empty;
                        if (table.RowCount > 1)
                        {
                            for (int k = 0; k < table.RowCount; k++)
                            {
                                for (int c = 0; c < table.Rows[k].ColumnCount; c++)
                                {
                                    if (c % 2 == 0)
                                        ctx += table.Rows[k].Columns[c].ToNodePlainString().Replace("　", "") + "：";
                                    else
                                        ctx += table.Rows[k].Columns[c].ToNodePlainString().Replace("　", "") + "\r\n";
                                }
                            }
                        }
                        bidUnit = ctx.GetBidRegex();
                        if (string.IsNullOrEmpty(bidUnit))
                            bidUnit = ctx.GetRegex("中标候选人名单");
                        if (bidMoney == "0")
                            bidMoney = ctx.GetMoneyRegex();
                        if (string.IsNullOrEmpty(prjMgr))
                            prjMgr = ctx.GetMgrRegex();
                    }
                }
                buildUnit = bidCtx.GetBuildRegex();
                prjAddress = bidCtx.GetAddressRegex();
                code = bidCtx.GetCodeRegex().GetCodeDel();
                if (bidUnit.Contains("公司"))
                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                specType = "建设工程";
                msgType = "江西省发展和改革委员会";
                BidInfo info = ToolDb.GenBidInfo("江西省", "江西省及地市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                       bidMoney, InfoUrl, prjMgr, HtmlTxt);
                list.Add(info);
                if (!crawlAll && list.Count >= this.MaxCount) return;
            }
        }
    }
}
