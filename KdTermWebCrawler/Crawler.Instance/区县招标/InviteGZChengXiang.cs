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

namespace Crawler.Instance
{
    public class InviteGZChengXiang : WebSiteCrawller
    {
        public InviteGZChengXiang()
            : base()
        {
            this.Group = "区县招标信息";
            this.Title = "广东省广州市城乡小型项目专区";
            this.Description = "自动抓取广东省广州市城乡小型项目专区招标信息";
            this.PlanTime = "9:36,10:38,14:36,16:38";
            this.SiteUrl = "http://www.gzzb.gd.cn/cms/wz/view/zq/country_infolist.jsp?siteId=1&channelId=239";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tzgg_right_page"))), new TagNameFilter("table")));
            parser = new Parser(new Lexer(sNode.ToHtml()));
            sNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a")); 
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    ATag aTag = sNode.SearchFor(typeof(ATag), true)[sNode.Count - 3] as ATag;
                    string temp = aTag.Link.Substring(aTag.Link.ToLower().IndexOf("page"), aTag.Link.Length - aTag.Link.ToLower().IndexOf("page"));
                    temp = temp.Remove(temp.IndexOf("&")).ToLower().Replace("page=", "");
                    pageInt = Convert.ToInt32(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.gzzb.gd.cn/cms/wz/view/zq/country_infolist.jsp?page=" + i.ToString() + "&siteId=1&channelId=239", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "bszn_right_table"))), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string prjType = tr.Columns[1].ToPlainTextString();
                        if (prjType.Contains("中标") || prjType.Contains("结果") || prjType.Contains("发包"))
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

                            prjName = tr.Columns[1].ToPlainTextString();
                            beginDate = tr.Columns[2].ToPlainTextString();
                            bidType = prjName.GetInviteBidType();
                            InfoUrl = "http://www.gzzb.gd.cn" + (tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag).Link;
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "contentDiv")));
                            if (dtlList != null && dtlList.Count > 0)
                            { 
                                HtmlTxt = dtlList.ToHtml();
                                bidCtx = HtmlTxt.ToCtxString();
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex();
                                if (string.IsNullOrEmpty(bidUnit))
                                {
                                    string bidunits = string.Empty;
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList bidList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoNormalTable")));
                                    if (bidList != null && bidList.Count > 0)
                                    {
                                        try
                                        {
                                            TableTag tab = bidList[0] as TableTag;
                                            for (int k = 1; k < tab.RowCount; k++)
                                            {
                                                bidunits += tab.Rows[k].Columns[0].ToPlainTextString().ToNodeString().Replace("单位名称", "中标单位");
                                                bidunits += "：" + tab.Rows[k].Columns[1].ToPlainTextString().ToNodeString() + "\r\n";
                                            }
                                        }
                                        catch { }
                                    }
                                    bidUnit = bidunits.GetBidRegex();
                                    if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                        bidMoney = bidunits.GetMoneyRegex();
                                }
                                buildUnit = bidCtx.GetBuildRegex();


                                code = bidCtx.GetCodeRegex();
                                prjAddress = bidCtx.GetAddressRegex();
                                prjMgr = bidCtx.GetMgrRegex();
                                msgType = "广州建设工程交易中心";
                                specType = "建设工程";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "广州市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                   bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                        else
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                       prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                       specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                       remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                       CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                            prjName = tr.Columns[1].ToPlainTextString();
                            beginDate = tr.Columns[2].ToPlainTextString();
                            inviteType = prjName.GetInviteBidType();
                            InfoUrl = "http://www.gzzb.gd.cn" + (tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag).Link;
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "contentDiv")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                inviteCtx = HtmlTxt.ToCtxString();
                                buildUnit = inviteCtx.GetBuildRegex();
                                code = inviteCtx.GetCodeRegex();
                                prjAddress = inviteCtx.GetAddressRegex();
                                msgType = "广州建设工程交易中心";
                                specType = "建设工程";
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "广州市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
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
