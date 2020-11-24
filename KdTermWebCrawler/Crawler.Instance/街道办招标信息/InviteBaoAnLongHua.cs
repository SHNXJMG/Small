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

namespace Crawler.Instance
{
    public class InviteBaoAnLongHua : WebSiteCrawller
    {
        public InviteBaoAnLongHua()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省深圳市龙华街道办事处";
            this.Description = "自动抓取广东省深圳市龙华街道办事处招标信息";
            this.PlanTime = "9:18,13:49";
            this.SiteUrl = "http://lhbsc.szlhxq.gov.cn/lhbsc/bsdt43/qyfw78/zbcg2/zbxxgs49/index.html";
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
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "Normal")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().Replace("createPageHTML(", "").Replace("index", "").Replace("html", "").Replace(", 0,", "").Replace(");", "").Replace(",", "").Replace(";", "").Replace(")", "").Replace("\"", "").Replace(" ", "").GetRegexBegEnd("/", "跳");
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
                        string url = "http://lhbsc.szlhxq.gov.cn/lhbsc/bsdt43/qyfw78/zbcg2/zbxxgs49/0e647d73-" + i.ToString() + ".html";
                        html = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                //NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("class", ""))), new TagNameFilter("tr")));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("class", "")));
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
                        INode node = viewList[j];
                        ATag aTag = node.GetATag();
                        beginDate = regDate.Match(viewList[j].ToPlainTextString().Trim()).Value;
                        InfoUrl = "http://lhbsc.szlhxq.gov.cn" + aTag.Link.Replace("../", "").Replace("./", "");
                        prjName = aTag.GetAttribute("title");
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
                            htlDtl = regexHtml.Replace(htlDtl, "");
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "contentbox")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = Regex.Replace(dtl.AsHtml(), "(<script)[\\s\\S]*?(</script>)", "");
                            inviteCtx = Regex.Replace(HtmlTxt, "(<script)[\\s\\S]*?(</script>)", "");
                            inviteCtx = Regex.Replace(inviteCtx, "<[^>]*>", "").Replace("&nbsp;", "").Replace(" ", "").Replace("\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            string InvType = prjName;
                            inviteType = prjName.GetInviteBidType();
                            Regex regPrjAddr = new Regex(@"(工程位置|工程地点|工程地址|详细地址|地址)(:|：)[^\r\n]+\r\n");
                            prjAddress = regPrjAddr.Match(inviteCtx).Value.Replace("工程位置", "").Replace("工程地点", "").Replace("工程地址", "").Replace("详细地址", "").Replace("地址", "").Replace(":", "").Replace("：", "").Trim();
                            Regex regBuildUnit = new Regex(@"(招标代理机构|采购代理机构|采购人名称|招标单位|招标人|招标单位（盖章）)(:|：)[^\r\n]+\r\n");
                            buildUnit = regBuildUnit.Match(inviteCtx).Value.Replace("采购人名称", "").Replace("采购代理机构", "").Replace("招标代理机构", "").Replace("招标单位", "").Replace("招标人", "").Replace("（盖章）", "").Replace(":", "").Replace("：", "").Trim();
                            Regex regPrjCode = new Regex(@"(工程编号|项目编号|编号)(:|：)[^\r\n]+\r\n");
                            code = regPrjCode.Match(inviteCtx).Value.Replace("工程编号", "").Replace("项目编号", "").Replace("编号", "").Replace(":", "").Replace("：", "").Replace("（", "").Replace("）", "").Trim();
                            msgType = "深圳市龙华新区龙华街道办事处";
                            if (string.IsNullOrEmpty(prjAddress) || Encoding.Default.GetByteCount(prjAddress) > 150)
                            { prjAddress = "见招标信息"; }
                            code = ToolHtml.GetSubString(code, 50);
                            buildUnit = ToolHtml.GetSubString(buildUnit, 150);
                            specType = "建设工程";
                            inviteType = "小型工程";
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "深圳市龙华新区龙华街道办事处";
                            }
                            inviteType = ToolHtml.GetInviteType(inviteType);
                            buildUnit = ToolHtml.GetSubString(buildUnit, 150);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳区及街道工程", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k] as ATag;
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
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
            return list;
        }
    }
}
