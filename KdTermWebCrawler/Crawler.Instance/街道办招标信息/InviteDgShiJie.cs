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
    public class InviteDgShiJie : WebSiteCrawller
    {
        public InviteDgShiJie()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省东莞石碣镇政府信息招标公告";
            this.Description = "自动抓取东莞石碣镇政府信息招标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://shijie.dg.gov.cn/desktop/publish/FileView.aspx?CategoryID=24&PageID=1";
            this.MaxCount = 150;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("align", "middle")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode.AsString();
                try
                {
                    pageInt = int.Parse(temp.GetRegexBegEnd("/", "页"));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__ControlState",
                    "__EventCaller",
                    "__EventParam",
                    "__VIEWSTATE",
                    "username",
                    "password",
                    "domain"
                    },new string[]{
                    "",
                    "0__Paging",
                    i.ToString(),
                    viewState,
                    "",
                    "",
                    "dg.gov.cn"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "96%")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {

                        TableRow tr = table.Rows[j];
                        string code = string.Empty, prjName = string.Empty, beginDate = string.Empty, InfoUrl = string.Empty;

                        ATag atag = tr.Columns[1].GetATag();

                        prjName = atag.LinkText.GetReplace(" ");
                       
                         
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://shijie.dg.gov.cn" + atag.Link.GetReplace("../");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "96%")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            parser = new Parser(new Lexer(dtlNode.AsHtml()));
                            NodeList nameNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "hei16c")));

                            prjName = nameNode[0].ToNodePlainString();
                            if (!prjName.Contains("中标") && !prjName.Contains("招标"))
                                continue;
                            if (prjName.Contains("中标"))
                            {
                                string buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, bidDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;


                                HtmlTxt = dtlNode.AsHtml().ToLower();
                                bidCtx = HtmlTxt.GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                                code = bidCtx.GetCodeRegex().GetCodeDel();
                                buildUnit = bidCtx.GetBuildRegex();
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex(new string[] { "中标值" });//.GetMoney();
                                if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                    bidMoney = bidCtx.GetMoneyRegex();
                                prjMgr = bidCtx.GetMgrRegex();

                                specType = "政府采购";
                                bidType = prjName.GetInviteBidType();
                                msgType = "东莞市石碣镇政府";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "东莞市区", "石碣镇", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag a = aNode[k].GetATag();
                                        if (a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = a.Link;
                                            else
                                                link = "http://shijie.dg.gov.cn/" + a.Link;
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }

                            }
                            else if (prjName.Contains("招标"))
                            {
                                string buildUnit = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                                HtmlTxt = dtlNode.AsHtml().ToLower();
                                inviteCtx = HtmlTxt.GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();

                                buildUnit = inviteCtx.GetBuildRegex();
                                prjAddress = inviteCtx.GetAddressRegex();
                                if (buildUnit.Contains("招标代理"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";

                                specType = "政府采购";
                                inviteType = prjName.GetInviteBidType();
                                msgType = "东莞市石碣镇政府";

                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "东莞市区", "石碣镇", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag a = aNode[k].GetATag();
                                        if (a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = a.Link;
                                            else
                                                link = "http://shijie.dg.gov.cn/" + a.Link;
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }

                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            } 
                        }

                    }
                }

            }
            return list;
        }
    }
}
