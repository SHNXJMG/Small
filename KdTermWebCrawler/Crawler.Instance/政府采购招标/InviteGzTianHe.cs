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
using System.Text;

namespace Crawler.Instance
{
    public class InviteGzTianHe:WebSiteCrawller
    {
        public InviteGzTianHe()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "广州市天河区住房和建设水务局招标、中标公告";
            this.Description = "自动抓取天河区住房和建设水务局招标、中标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://zhujian.thnet.gov.cn/jianshe/zbglxx/list_ss.shtml";
            this.MaxCount = 120;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 24;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://zhujian.thnet.gov.cn/jianshe/zbglxx/list_ss_" + i + ".shtml", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "art-list")), true), new TagNameFilter("li")));
                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
                    {
                        string prjName = string.Empty, InfoUrl = string.Empty, beginDate = string.Empty, HtmlTxt = string.Empty;
                        ATag aTag = viewList[j].GetATag();
                        if (aTag == null) continue;

                        prjName = aTag.GetAttribute("title").Trim();
                        InfoUrl = "http://zhujian.thnet.gov.cn/" + aTag.Link.GetReplace("../");
                        beginDate = viewList[j].ToPlainTextString().GetDateRegex();
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "mbd")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml(); 

                            if (prjName.Contains("中标") || prjName.Contains("成交") || prjName.Contains("结果") || prjName.Contains("单位公示"))
                            {
                                string buildUnit = string.Empty, bidUnit = string.Empty,
                          bidMoney = string.Empty, code = string.Empty,
                          bidDate = string.Empty,
                          endDate = string.Empty, bidType = string.Empty,
                          specType = string.Empty,
                          msgType = string.Empty, bidCtx = string.Empty,
                          prjAddress = string.Empty, remark = string.Empty,
                          prjMgr = string.Empty, otherType = string.Empty;
                                bidCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                                 
                                code = bidCtx.GetCodeRegex().GetCodeDel();
                                buildUnit = bidCtx.GetBuildRegex();
                                if (buildUnit.Contains("地址"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                                if (buildUnit.Contains("招标代理"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";

                                bidUnit = bidCtx.GetBidRegex();

                                bidMoney = bidCtx.GetMoneyRegex();

                                if (string.IsNullOrEmpty(bidUnit))
                                {
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                    if (tableNode != null && tableNode.Count > 0)
                                    {
                                        TableTag table = tableNode[0] as TableTag;
                                        if (table.RowCount >= 3 && table.Rows[0].ColumnCount >= 2)
                                        {
                                            string ctx = string.Empty;
                                            for (int r = 1; r < table.RowCount; r++)
                                            {
                                                try
                                                {
                                                    ctx += table.Rows[r].Columns[0].ToNodePlainString() + "：";
                                                    ctx += table.Rows[r].Columns[1].ToNodePlainString() + "\r\n";
                                                }
                                                catch { }
                                            }
                                            bidUnit = ctx.GetBidRegex(new string[] { "承包意向人名称" });
                                            if (string.IsNullOrEmpty(bidUnit))
                                                bidUnit = ctx.GetBidRegex();
                                            prjMgr = ctx.GetMgrRegex(new string[]{"项目负责人姓名及证书编号"});
                                            if (prjMgr.Contains("/"))
                                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                                        }
                                    }
                                }
                                if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                    bidMoney = bidCtx.GetRegexBegEnd("暂定价为", "元").GetMoney();
                                try
                                {
                                    if (decimal.Parse(bidMoney) > 100000)
                                        bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                                }
                                catch { }
                                if (string.IsNullOrEmpty(buildUnit))
                                    buildUnit = "天河区住房和建设水务局";
                                msgType = "天河区住房和建设水务局";
                                specType = "政府采购";
                                bidType = prjName.GetInviteBidType();
                                BidInfo info = ToolDb.GenBidInfo("广东省", "广州政府采购", "天河区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            {
                                                link = "http://zhujian.thnet.gov.cn/jianshe/zbglxx/" + a.Link.GetReplace("./");
                                            }
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                            else
                            {
                                string code = string.Empty, buildUnit = string.Empty,
                       prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                       specType = string.Empty, endDate = string.Empty,
                       remark = string.Empty, inviteCon = string.Empty,
                       CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty;

                                inviteCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                                inviteType = prjName.GetInviteBidType();
                                 
                                code = inviteCtx.GetCodeRegex().GetCodeDel();
                                buildUnit = inviteCtx.GetBuildRegex();
                                prjAddress = inviteCtx.GetAddressRegex();
                                if (buildUnit.Contains("招标代理"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                                if (buildUnit.Contains("地址"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                                if (string.IsNullOrEmpty(buildUnit))
                                    buildUnit = "天河区住房和建设水务局";

                                msgType = "天河区住房和建设水务局";
                                specType = "政府采购";

                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "广州政府采购", "天河区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            {
                                                link = "http://zhujian.thnet.gov.cn/jianshe/zbglxx/" + a.Link.GetReplace("./");
                                            }
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
