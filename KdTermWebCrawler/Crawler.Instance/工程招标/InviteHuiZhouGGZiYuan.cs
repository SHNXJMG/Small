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
    public class InviteHuiZhouGGZiYuan : WebSiteCrawller
    {
        public InviteHuiZhouGGZiYuan()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省惠州市公共资源交易中心";
            this.Description = "自动抓取广东省惠州市公共资源交易中心招标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.SiteUrl = "http://zyjy.huizhou.gov.cn/pages/cms/hzggzyjyzx/html/artList.html?cataId=54f6d9f3580843d59b9dd64918e7ae4f";
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
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagination")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().Replace(" ", "");
                    Regex reg = new Regex(@"/[^页]+页");
                    pageInt = Convert.ToInt32(reg.Match(temp).Value.Replace("/", "").Replace("页", ""));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://zyjy.huizhou.gov.cn/pages/cms/hzggzyjyzx/html/artList.html?cataId=54f6d9f3580843d59b9dd64918e7ae4f&pageNo=" + i.ToString(), Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "div_list"))), new TagNameFilter("ul")));
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
                        beginDate = regDate.Match(viewList[j].ToPlainTextString()).Value;
                        prjName = viewList[j].ToPlainTextString().Replace("\r", "").Replace("\n", "").Replace(beginDate, "");
                        ATag aTag = viewList.SearchFor(typeof(ATag), true)[j] as ATag;
                        InfoUrl = "http://zyjy.huizhou.gov.cn" + aTag.Link;
                        string htmDtl = string.Empty;
                        try
                        {
                            System.Data.DataTable dt = new System.Data.DataTable();
                            htmDtl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8);
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
                            htmDtl = regexHtml.Replace(htmDtl, "");
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "divZoom")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = System.Text.RegularExpressions.Regex.Replace(dtl.ToHtml(), "(<script)[\\s\\S]*?(</script>)", "");
                            inviteCtx = System.Text.RegularExpressions.Regex.Replace(HtmlTxt, "(<script)[\\s\\S]*?(</script>)", "");
                            inviteCtx = System.Text.RegularExpressions.Regex.Replace(inviteCtx, "<[^>]*>", "").Replace("&nbsp;", "").Replace(" ", "").Replace("\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\r\n", "\r\n").Replace("\r\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\t", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");

                            Regex regPrjAddr = new Regex(@"(工程位置|工程地点|工程地址|详细地址|地点|地址)(:|：)[^\r\n]+\r\n");
                            prjAddress = regPrjAddr.Match(inviteCtx).Value.Replace("工程位置", "").Replace("工程地点", "").Replace("工程地址", "").Replace("详细地址", "").Replace("地点", "").Replace("地址", "").Replace(":", "").Replace("：", "").Trim();
                            Regex regBuildUnit = new Regex(@"(招标代理机构|招标单位|招标人|招标单位（盖章）)(:|：)[^\r\n]+\r\n");
                            buildUnit = regBuildUnit.Match(inviteCtx).Value.Replace("招标代理机构", "").Replace("招标单位", "").Replace("招标人", "").Replace("（盖章）", "").Replace(":", "").Replace("：", "").Trim();
                            if (buildUnit.Contains("资质"))
                            {
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("资质"));
                            }
                            prjAddress = ToolHtml.GetSubString(prjAddress, 150);
                            buildUnit = ToolHtml.GetSubString(buildUnit, 150);
                            Regex regPrjCode = new Regex(@"(工程编号|项目编号|编号)(:|：)[^\r\n]+\r\n");
                            code = regPrjCode.Match(inviteCtx).Value.Replace("工程编号", "").Replace("项目编号", "").Replace("编号", "").Replace(":", "").Replace("：", "").Trim();
                            msgType = "惠州市公共资源交易中心";
                            specType = "建设工程";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            if (string.IsNullOrEmpty(prjAddress) || Encoding.Default.GetByteCount(prjAddress) > 150)
                            { prjAddress = "见招标信息"; }
                            if (Encoding.Default.GetByteCount(code) > 50)
                            {
                                code = "";
                            }
                            inviteType = ToolHtml.GetInviteType(inviteType);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "惠州市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
