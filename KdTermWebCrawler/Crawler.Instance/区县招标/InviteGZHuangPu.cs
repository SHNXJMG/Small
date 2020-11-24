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
    public class InviteGZHuangPu : WebSiteCrawller
    {
        public InviteGZHuangPu()
            : base()
        {
            this.Group = "区县招标信息";
            this.Title = "广东省广州市黄浦区";
            this.Description = "自动抓取广东省广州市黄浦区";
            this.PlanTime = "9:25,10:13,14:25,16:26";
            this.SiteUrl = "http://www.hp.gov.cn/hp/zfcg/list.shtml";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new TagNameFilter("script"));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString().GetRegexBegEnd("page_div", "list").Replace("1,","").Replace(",","").Replace("'","");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int page = 1; page <= pageInt; page++)
            { 
                if (page > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.hp.gov.cn/hp/zfcg/list_"+page.ToString()+".shtml", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_list")),true),new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        string prjType = nodeList[i].GetATag().LinkText;
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

                            beginDate = nodeList[i].ToPlainTextString().GetDateRegex();
                            prjName = prjType.ToNodeString();

                            InfoUrl = "http://www.hp.gov.cn/" + nodeList[i].GetATagHref().Replace("../","");
                            bidType = prjName.GetInviteBidType();

                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zoomcon")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                bidCtx = HtmlTxt.ToCtxString();
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex();

                                prjMgr = bidCtx.GetMgrRegex();
                                msgType = "黄埔信息网";
                                specType = "建设工程";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "广州市区", "黄浦区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
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

                            beginDate = nodeList[i].ToPlainTextString().GetDateRegex();
                            prjName = prjType.ToNodeString();
                            inviteType = prjName.GetInviteBidType();
                            InfoUrl = "http://www.hp.gov.cn/" +nodeList[i].GetATagHref().Replace("../","");
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zoomcon")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                inviteCtx = HtmlTxt.Replace("<p>", "\r\n").Replace("</p>", "\r\n").ToCtxString();

                                prjAddress = inviteCtx.GetAddressRegex();
                                buildUnit = inviteCtx.GetBuildRegex();
                                code = inviteCtx.GetCodeRegex();
                                msgType = "黄埔信息网";
                                specType = "建设工程";
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "广州市区", "黄浦区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
