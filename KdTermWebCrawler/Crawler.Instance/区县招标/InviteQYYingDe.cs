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
    public class InviteQYYingDe : WebSiteCrawller
    {
        public InviteQYYingDe()
            : base()
        {
            this.Group = "区县招标信息";
            this.Title = "广东省清远英德市住房和城乡建设局";
            this.Description = "自动抓取广东省清远英德市住房和城乡建设局";
            this.PlanTime = "9:13,10:14,14:14,16:15";
            this.SiteUrl = "http://www.ydszj.gov.cn/channel/138006/content";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("vAlign", "bottom")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString();
                    Regex reg = new Regex(@",共[^页]+页");
                    string page = reg.Match(temp).Value.Replace(",共", "").Replace("页", "");
                    pageInt = Convert.ToInt32(page);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.ydszj.gov.cn/channel/138006/content?pageNo=" + i.ToString(), Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("style", "height:350px;"))), new HasAttributeFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string prjType = nodeList[j].ToPlainTextString();
                        if (prjType.Contains("中标") || prjType.Contains("结果"))
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

                            prjName = nodeList[j].ToPlainTextString().Replace(nodeList[j].ToPlainTextString().GetDateRegex(), "");
                            beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                            bidType = prjName.GetInviteBidType();
                            InfoUrl = "http://www.ydszj.gov.cn" + nodeList[j].GetATagHref();

                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "news_content")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                bidCtx = HtmlTxt.ToCtxString();
                                bidUnit = bidCtx.GetBidRegex();
                                buildUnit = bidCtx.GetBuildRegex();
                                prjAddress = bidCtx.GetAddressRegex();
                                code = bidCtx.GetCodeRegex();
                                bidMoney = bidCtx.GetMoneyRegex();

                                msgType = "英德市住房和城乡建设局";
                                specType = "建设工程";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "清远市区", "英德市", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
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

                            prjName = nodeList[j].ToPlainTextString().Replace(nodeList[j].ToPlainTextString().GetDateRegex(), "");
                            beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                            inviteType = prjName.GetInviteBidType();
                            InfoUrl = "http://www.ydszj.gov.cn" + nodeList[j].GetATagHref();
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "news_content")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                inviteCtx = HtmlTxt.ToCtxString();

                                buildUnit = inviteCtx.GetBuildRegex();
                                code = inviteCtx.GetCodeRegex();
                                prjAddress = inviteCtx.GetAddressRegex();

                                msgType = "英德市住房和城乡建设局";
                                specType = "建设工程"; 
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "清远市区", "英德市", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
