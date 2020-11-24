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
    public class InviteYueHai : WebSiteCrawller
    {
        public InviteYueHai()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省深圳市南山区粤海街道办事处";
            this.Description = "自动抓取广东省深圳市南山区粤海街道办事处招标信息";
            this.PlanTime = "9:17,13:51";
            this.SiteUrl = "http://www.szns.gov.cn/publish/yhj/3131/5168/index.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
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
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "jwpage")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString();
                    Regex reg = new Regex(@"/共[^页]+页");
                    pageInt = Convert.ToInt32(reg.Match(temp).Value.Replace("/共", "").Replace("页", ""));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {

                }
                parser = new Parser(new Lexer(html));
                NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "jwRercon"))), new TagNameFilter("ul"))), new TagNameFilter("li")));
                if (dtlList != null && dtlList.Count > 0)
                {
                    for (int j = 0; j < dtlList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                            prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                            specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                            remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                            CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        prjName = dtlList[j].ToPlainTextString().Trim().Remove(dtlList[j].ToPlainTextString().Trim().IndexOf("["));
                        Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                        beginDate = regDate.Match(dtlList[j].ToPlainTextString().Trim()).Value;
                        ATag aTag = dtlList.SearchFor(typeof(ATag), true)[j] as ATag;
                        InfoUrl = "http://www.szns.gov.cn" + aTag.Link;
                        string htmDtl = string.Empty;
                        try
                        {
                            htmDtl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8);
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
                            htmDtl = regexHtml.Replace(htmDtl, "");
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hyxzf2")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml();
                            inviteCtx = dtl.AsString().Replace("&nbsp;", "").Replace("\n", "\r\n");
                            string InvType = prjName;
                            if (InvType.Contains("施工"))
                            {
                                inviteType = "施工";
                            }
                            if (InvType.Contains("监理"))
                            {
                                inviteType = "监理";
                            }
                            if (InvType.Contains("设计"))
                            {
                                inviteType = "设计";
                            }
                            if (InvType.Contains("勘察"))
                            {
                                inviteType = "勘察";
                            }
                            if (InvType.Contains("服务"))
                            {
                                inviteType = "服务";
                            }
                            if (InvType.Contains("劳务分包"))
                            {
                                inviteType = "劳务分包";
                            }
                            if (InvType.Contains("专业分包"))
                            {
                                inviteType = "专业分包";
                            }
                            if (InvType.Contains("小型施工"))
                            {
                                inviteType = "小型工程";
                            }
                            if (InvType.Contains("设备材料"))
                            {
                                inviteType = "设备材料";
                            }
                            Regex regPrjAddr = new Regex(@"(工程位置|工程地点|工程地址)(:|：)[^\r\n]+\r\n");
                            prjAddress = regPrjAddr.Match(inviteCtx.Replace(" ", "")).Value.Replace("工程位置", "").Replace("工程地点", "").Replace("工程地址", "").Replace(":", "").Replace("：", "").Trim();
                            Regex regBuildUnit = new Regex(@"(招标单位|招标人|招标单位（盖章）)(:|：)[^\r\n]+\r\n");
                            buildUnit = regBuildUnit.Match(inviteCtx.Replace(" ", "")).Value.Replace("招标单位", "").Replace("招标人", "").Replace("（盖章）", "").Replace(":", "").Replace("：", "").Trim();
                            Regex regPrjCode = new Regex(@"(工程编号|项目编号|编号)(:|：)[^\r\n]+\r\n");
                            code = regPrjCode.Match(inviteCtx.Replace(" ", "")).Value.Replace("工程编号", "").Replace("项目编号", "").Replace("编号", "").Replace(":", "").Replace("：", "").Trim();
                            msgType = "深圳市南山区粤海街道办事处";
                            if (string.IsNullOrEmpty(prjAddress))
                            { prjAddress = "见招标信息"; }
                            code = ToolHtml.GetSubString(code, 50);
                            buildUnit = ToolHtml.GetSubString(buildUnit, 150);
                            specType = "建设工程";
                            inviteType = "小型工程";
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "深圳市南山区粤海街道办事处";
                            }
                            inviteType = ToolHtml.GetInviteType(inviteType);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳区及街道工程", "南山区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
