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
using System.Windows.Forms;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteDaLang : WebSiteCrawller
    {
        public InviteDaLang()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省深圳市大浪街道办事处";
            this.Description = "自动抓取广东省深圳市大浪街道办事处招标信息";
            this.PlanTime = "9:18,13:49";
            this.SiteUrl = "http://dalang.szlhxq.gov.cn/dlbsc/zwgk73/cgzb10/zbgz/index.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
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
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "dlbsc-feiyeR")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().ToRegString().GetRegexBegEnd("/", "跳");
                    
                    pageInt = int.Parse(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://dalang.szlhxq.gov.cn/dlbsc/zwgk73/cgzb10/zbgz/13891-" + i + ".html", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "dlbsc_contUl")), true), new TagNameFilter("li")));

                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                        beginDate = regDate.Match(viewList[j].ToNodePlainString()).Value;
                        prjName = viewList[j].GetATag().LinkText;

                        InfoUrl = "http://dalang.szlhxq.gov.cn" + viewList[j].GetATagHref(0).Replace("./", "/");
                        string htmDtl = string.Empty;
                        try
                        {
                            htmDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
                            htmDtl = regexHtml.Replace(htmDtl, "");
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "dlbsc-content")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = System.Text.RegularExpressions.Regex.Replace(dtl.ToHtml(), "(<script)[\\s\\S]*?(</script>)", "");
                            inviteCtx = HtmlTxt.ToCtxString();
                            inviteType = prjName.GetInviteBidType();
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            msgType = "深圳市龙华新区大浪街道办事处";
                            if (string.IsNullOrEmpty(prjAddress)) prjAddress = "见招标信息";

                            specType = "建设工程";
                            inviteType = "小型工程";
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "深圳市龙华新区大浪街道办事处";
                            }
                            inviteType = ToolHtml.GetInviteType(inviteType);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳区及街道工程", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
