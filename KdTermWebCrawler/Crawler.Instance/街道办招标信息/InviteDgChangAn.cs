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
    public class InviteDgChangAn : WebSiteCrawller
    {
        public InviteDgChangAn()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省东莞长安镇政府信息招标中标公告";
            this.Description = "自动抓取东莞长安镇政府信息招标中标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://xxgk.dgca.gov.cn/dgca/1100/caxxgklist.shtml";
            this.MaxCount = 150;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 400;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch { return list; }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://xxgk.dgca.gov.cn/dgca/1100/caxxgklist_" + i + ".shtml");
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "rightbobj01 list_bg")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                         
                        TableRow tr = table.Rows[j];
                        string code = string.Empty, prjName = string.Empty, beginDate = string.Empty, InfoUrl = string.Empty;

                        ATag atag = tr.Columns[1].GetATag();

                        prjName = atag.GetAttribute("title").GetReplace(" ");
                        if (!prjName.Contains("中标") && !prjName.Contains("招标"))
                            continue;

                        code = tr.Columns[0].ToNodePlainString();
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://xxgk.dgca.gov.cn/" + atag.Link.GetReplace("../");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "zoom")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {

                            if (prjName.Contains("中标"))
                            {
                                string buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, bidDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;


                                HtmlTxt = dtlNode.AsHtml().ToLower();
                                bidCtx = HtmlTxt.GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();

                                buildUnit = bidCtx.GetBuildRegex();
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex(new string[] { "中标值" });//.GetMoney();
                                if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                    bidMoney = bidCtx.GetMoneyRegex();
                                prjMgr = bidCtx.GetMgrRegex();

                                specType = "政府采购";
                                bidType = prjName.GetInviteBidType();
                                msgType = "东莞市长安镇政府";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "东莞市区", "长安镇", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                                link = "http://xxgk.dgca.gov.cn/" + a.Link;
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

                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";

                                specType = "政府采购";
                                inviteType = prjName.GetInviteBidType();
                                msgType = "东莞市长安镇政府";

                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "东莞市区", "长安镇", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                                link = "http://xxgk.dgca.gov.cn/" + a.Link;
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }

                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }

                            if (list.Count % 20 == 0)
                            {
                                Thread.Sleep(1000 * 600);
                            }
                        }

                    }
                }

            }
            return list;
        }
    }
}
