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
    public class InviteSZJYZXJZ : WebSiteCrawller
    {
        public InviteSZJYZXJZ()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省深圳市交易中心建筑工务署";
            this.Description = "自动抓取广东省深圳市交易中心建筑工务署招、中标信息";
            this.PlanTime = "9:30,14:30";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.szjsjy.com.cn/SpecialColumn/Special.aspx?type=%e4%b8%93%e6%a0%8f%e4%b8%93%e5%8c%ba-%e5%bb%ba%e7%ad%91%e5%b7%a5%e5%8a%a1%e7%bd%b2";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            string eventget = string.Empty;
            int page = 1;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("valign", "top")), true), new TagNameFilter("a")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.GetATagHref(pageList.Count - 1);
                    temp = temp.Substring(temp.IndexOf("Page"), temp.Length - temp.IndexOf("Page")).Replace("Page$", "").Replace("')", "");
                    page = Convert.ToInt32(temp);
                }
                catch { page = 1; }
            }

            for (int i = 1; i <= page; i++)
            { 
                if (i > 1)
                {

                    try
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(htl);
                        eventget = this.ToolWebSite.GetAspNetEventValidation(htl);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "__EVENTTARGET","__EVENTARGUMENT","__VIEWSTATE","__EVENTVALIDATION","sel","beginDate","endDate","infotitle","hdnPwd",
                        "hdnOperate","hdnType"
                        }, new string[]{
                        "GridView1","Page$"+i.ToString(),viewState,eventget,"1","","","","","",""
                        });
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }

                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    { 
                        TableRow tr = table.Rows[j];
                        string prjType = tr.Columns[1].ToNodePlainString();
                        if (prjType.Contains("结果") || prjType.Contains("中标"))
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

                            prjName = prjType;
                            beginDate = tr.Columns[3].ToNodePlainString();
                            bidType = prjName.GetInviteBidType();
                            InfoUrl = "http://www.szjsjy.com.cn/SpecialColumn/"+tr.GetATagHref();
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.UTF8);
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table3")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.AsHtml();
                                bidCtx = HtmlTxt.ToCtxString();

                                bidUnit = bidCtx.GetRegexBegEnd("如下：1、","2、");
                                if (string.IsNullOrEmpty(bidUnit))
                                {
                                    bidUnit = bidCtx.GetRegexBegEnd("如下：", "公司");
                                    if (string.IsNullOrEmpty(bidUnit))
                                    {
                                        bidUnit = bidCtx.GetRegexBegEnd("如下:", "公司");
                                        if (!string.IsNullOrEmpty(bidUnit))
                                            bidUnit += "公司";
                                    }
                                }
                                buildUnit = bidCtx.GetBuildRegex();
                                if (string.IsNullOrEmpty(buildUnit))
                                {
                                    buildUnit = "深圳市建筑工务署";
                                }
                                bidMoney = bidCtx.GetMoneyRegex();
                                code = bidCtx.GetCodeRegex();
                                prjAddress = bidCtx.GetAddressRegex();

                                specType = "建设工程";
                                msgType = "深圳市建设工程交易中心";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "深圳市工程", string.Empty, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                   bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);

                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                        else
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty,
                           inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty,
                           endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                           CreateTime = string.Empty, msgType = string.Empty, HtmlTxt = string.Empty, otherType = string.Empty;

                            prjName = prjType;
                            inviteType = prjName.GetInviteBidType();
                            beginDate = tr.Columns[3].ToNodePlainString(); 
                            InfoUrl = "http://www.szjsjy.com.cn/SpecialColumn/" + tr.GetATagHref();
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                            }
                            catch { Logger.Error("InviteSZJYZXJZ"); continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table3")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.AsHtml();
                                inviteCtx = HtmlTxt.ToCtxString();

                                prjAddress = inviteCtx.GetAddressRegex();
                                code = inviteCtx.GetCodeRegex();
                                buildUnit = inviteCtx.GetBuildRegex();
                                if (string.IsNullOrEmpty(buildUnit))
                                {
                                    buildUnit = "深圳市建筑工务署";
                                }
                                specType = "建设工程";
                                msgType = "深圳市建设工程交易中心";

                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳市工程", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
