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
    public class InviteHeBeiGgzy : WebSiteCrawller
    {
        public InviteHeBeiGgzy()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "河北省公共资源交易信息网招标信息";
            this.Description = "自动抓取河北省公告资源交易信息网招标信息";
            this.PlanTime = "9:10,11:10,14:10,16:10,18:10";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.SiteUrl = "http://www.hebpr.cn/002/002009/002009002/moreinfo.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagemargin")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString();
                    temp = temp.GetRegexBegEnd("/", "转到");
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.hebpr.cn/002/002009/002009002/" + i + ".html", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "frame-con-items-commons")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string prjName = string.Empty, beginDate = string.Empty, InfoUrl = string.Empty,
                    area = string.Empty;
                        INode node = listNode[j];

                        beginDate = node.ToPlainTextString().GetDateRegex();
                        if (string.IsNullOrWhiteSpace(beginDate)) continue;

                        ATag aTag = node.GetATag();
                        string temp = node.ToNodePlainString();

                        area = temp.GetRegexBegEnd("【", "】");
                        if (area.Contains("省"))
                            area = "";
                        InfoUrl = "http://www.hebpr.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList nameNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "show-title"), new TagNameFilter("h1")));
                        if (nameNode != null && nameNode.Count > 0)
                            prjName = nameNode[0].ToNodePlainString().Replace(" ", "");
                        else
                            continue;

                        if (prjName.Contains("中标") || prjName.Contains("结果"))
                        {
                            string buildUnit = string.Empty, bidUnit = string.Empty,
                               bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty,
                               endDate = string.Empty, bidType = string.Empty, specType = string.Empty,
                               msgType = string.Empty, bidCtx = string.Empty,
                               prjAddress = string.Empty, remark = string.Empty,
                               prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                            parser.Reset();
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "show-con infoContent")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.AsHtml();
                                bidCtx = HtmlTxt.ToCtxString();
                                bidCtx = HtmlTxt.GetReplace(new string[] { "<br/>", "<br />", "</p>", "<br>" }, "\r\n").GetReplace("\r\n\r\n,\r\n\r\n,\r\n\r\n,\r\n\r\n,\r\n\r\n,\r\n\r\n,\r\n\r\n", "\r\n").ToCtxString();
                                prjAddress = bidCtx.GetAddressRegex();
                                buildUnit = bidCtx.GetBuildRegex();
                                bidUnit = bidCtx.GetBidRegex().GetReplace("A");
                                if (string.IsNullOrWhiteSpace(bidUnit))
                                    bidUnit = bidCtx.GetRegex("招标候选人");
                                bidMoney = bidCtx.GetMoneyRegex();
                                prjMgr = bidCtx.GetMgrRegex();

                                if (string.IsNullOrWhiteSpace(bidUnit))
                                {
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                    if (tableNode != null && tableNode.Count > 0)
                                    {
                                        string ctx = string.Empty;
                                        TableTag table = tableNode[0] as TableTag;
                                        for (int r = 0; r < table.RowCount; r++)
                                        {
                                            for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                                            {
                                                string tempStr = table.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                                if (c % 2 == 0)
                                                    ctx += tempStr += "：";
                                                else
                                                    ctx += tempStr += "\r\n";
                                            }
                                        }
                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                            bidMoney = ctx.GetMoneyRegex(null,false,"万元");
                                        if (string.IsNullOrEmpty(buildUnit))
                                            buildUnit = ctx.GetBuildRegex();
                                        
                                    }
                                    
                                }
                                if (string.IsNullOrWhiteSpace(code))
                                    code = bidCtx.GetCodeRegex().GetCodeDel();

                                if (bidUnit.Contains("公司"))
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                                if (buildUnit.Contains("地址"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                                if (buildUnit.Contains("招标"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标"));
                                try
                                {
                                    if (decimal.Parse(bidMoney) >= 100000)
                                        bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();

                                }
                                catch { }
                                msgType = "河北省公共资源交易中心";
                                specType = "建设工程";
                                bidType = prjName.GetInviteBidType();
                                BidInfo info = ToolDb.GenBidInfo("河北省", "河北省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag a = aNode[k] as ATag;
                                        if (a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = a.Link;
                                            else
                                                link = "http://www.hebpr.cn/" + a.Link.GetReplace("../,./");
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            string code = string.Empty, buildUnit = string.Empty,
                   prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                   specType = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty,
                   CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                            parser.Reset();
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "show-con infoContent")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.AsHtml();
                                inviteCtx = HtmlTxt.ToCtxString();
                                prjAddress = inviteCtx.GetAddressRegex();
                                buildUnit = inviteCtx.GetBuildRegex();
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                                if (buildUnit.Contains("地址"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                                if (buildUnit.Contains("联系"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                                if (buildUnit.Contains("招标"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标"));
                                if (string.IsNullOrWhiteSpace(code))
                                    code = inviteCtx.GetCodeRegex().GetCodeDel();
                                msgType = "河北省公共资源交易中心";
                                specType = "建设工程";
                                inviteType = prjName.GetInviteBidType();
                                InviteInfo info = ToolDb.GenInviteInfo("河北省", "河北省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag a = aNode[k] as ATag;
                                        if (a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = a.Link;
                                            else
                                                link = "http://www.hebpr.cn/" + a.Link.GetReplace("../,./");
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
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
