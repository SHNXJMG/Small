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
    public class InviteNanShan : WebSiteCrawller
    {
        public InviteNanShan()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "广东省深圳市南山建设局建招标信息";
            this.Description = "自动抓取广东省深圳市南山建设局建招标信息";
            this.SiteUrl = "http://www.szns.gov.cn/publish/main/1/19/26/zbtbxx/5465/index.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8, ref cookiestr);
            }
            catch (Exception ex)
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hyxma03")));
            Regex regexPage = new Regex(@"共\d+页");
            try
            {
                page = int.Parse(regexPage.Match(nodeList.AsString()).Value.Trim(new char[] { '共', '页' }));
            }
            catch (Exception)
            { }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.szns.gov.cn/publish/main/1/19/26/zbtbxx/5465/index_" + i.ToString() + ".html"), Encoding.UTF8);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hyxdianbeijing")));
                if (tableNodeList.Count > 0)
                {
                    for (int j = 0; j < tableNodeList.Count; j++)
                    {
                        ATag aTag = tableNodeList.SearchFor(typeof(ATag), true)[j] as ATag;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                  prjAddress = string.Empty, inviteCtx = string.Empty, bidType = string.Empty,
                                  specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                  remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                  CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        prjName = aTag.LinkText;
                        Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                        beginDate = regDate.Match(tableNodeList[j].ToPlainTextString()).Value.Trim();
                        InfoUrl = "http://www.szns.gov.cn" + aTag.Link.Replace("amp;", "").Trim();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hyxzf2")));
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            Regex regeximg = new Regex(@"<img[^>]*>");//去掉图片
                            HtmlTxt = regeximg.Replace(HtmlTxt, "");
                            inviteCtx = dtnode.AsString().Replace("\n", "\r\n");
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            inviteCtx = regexHtml.Replace(inviteCtx, "");
                            Regex regBuidUnit = new Regex(@"(招标人|建设单位)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(inviteCtx).Value.Replace("招标人：", "").Replace("建设单位:", "").Trim();
                            Regex regPrjAddr = new Regex(@"(工程地点|工程地址)(：|:)[^\r\n]+\r\n");
                            prjAddress = regPrjAddr.Match(inviteCtx).Value.Replace("工程地点:", "").Replace("工程地址", "").Replace("：", "").Trim();
                            Regex regCode = new Regex(@"工程编号(：|:)[^\r\n]+\r\n");
                            code = regCode.Match(inviteCtx).Value.Replace("工程编号:", "").Replace("工程编号：", "").Trim();
                            if (Encoding.Default.GetByteCount(code) > 50)
                            {
                                code = "";
                            }
                            msgType = "深圳市南山区政府采购及招标中心";
                            specType = "建设工程";
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            if (prjAddress == "")
                            {
                                prjAddress = "见招标信息";
                            }
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳区及街道工程", "",
                             string.Empty, code, prjName, prjAddress, buildUnit,
                             beginDate, endDate, inviteCtx, remark, msgType, bidType, specType, otherType, InfoUrl, HtmlTxt);
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
