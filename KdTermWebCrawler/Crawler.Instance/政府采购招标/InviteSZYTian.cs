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
    public class InviteSZYTian : WebSiteCrawller
    {
        public InviteSZYTian()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "广东省深圳市盐田区招标信息";
            this.Description = "自动抓取广东省深圳市盐田区招标信息";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.yantian.gov.cn/cn/zwgk/zfcg/zbgg/index.shtml";
            this.MaxCount = 50;
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
            NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("align", "right")));
            if (tableNodeList != null && tableNodeList.Count > 0)
            {
                Regex regexPage = new Regex(@"共\d+页");
                page = int.Parse(regexPage.Match(tableNodeList.AsString()).Value.Trim(new char[] { '共', '页' }));
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl("http://www.yantian.gov.cn/cn/zwgk/zfcg/zbgg/index_" + (i - 1).ToString() + ".shtml", Encoding.UTF8);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "565")), true), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    string url = string.Empty;
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string beg = nodeList[j].ToPlainTextString().GetDateRegex();
                        if (string.IsNullOrEmpty(beg))
                        {
                            continue;
                        }
                        else if (j > 0 && nodeList[j].GetATagHref() == url)
                        {
                            continue;
                        }
                        url = nodeList[j].GetATagHref();
                        TableTag table = nodeList[j] as TableTag;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                       prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                       specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                       remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                       CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        prjName = table.GetATagValue("title").Replace("&#41;", ")").Replace("&#40;", "(");
                        InfoUrl = "http://www.yantian.gov.cn" + table.GetATagValue();
                        beginDate = beg;
                        string htmldetail = string.Empty;
                        if (prjName.Contains("["))
                        {
                            prjName = prjName.Remove(prjName.IndexOf("[")).ToString();
                        }
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Replace("<br />", "\r\n").Trim();
                        }
                        catch
                        {
                            continue;
                        }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            inviteCtx = dtnode.AsString().Replace(" ", "").Trim();
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            inviteCtx = regexHtml.Replace(inviteCtx, "");
                            Regex regCode = new Regex(@"(项目序号|招标编号)(：|:)[^\r\n]+\r\n");
                            code = regCode.Match(inviteCtx).Value.Replace("招标编号：", "").Replace("项目序号：", "").Trim();
                            if (Encoding.Default.GetByteCount(code) > 50)
                            {
                                code = "";
                            }
                            msgType = "深圳市盐田区政府采购中心";
                            specType = "建设工程";
                            Regex regBuidUnit = new Regex(@"(招标人|建设单位)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(inviteCtx).Value.Replace("招标人：", "").Replace("建设单位:", "").Trim();
                            if (Encoding.Default.GetByteCount(buildUnit) > 150)
                            {
                                buildUnit = "";
                            }
                            if (prjAddress == "")
                            {
                                prjAddress = "见招标信息";
                            }
                            if (Encoding.Default.GetByteCount(prjAddress) > 200)
                            {
                                prjAddress = "";
                            }
                            inviteCtx = inviteCtx.Replace("<ahref=", "").Replace("/service/", "").Replace("</a>", "").Replace("您是第", "").Replace("位访问者粤ICP备06000803号", "").Replace(">", "").Trim();
                            prjName = prjName.Replace("&ldquo;", "").Replace("&rdquo;", "").Trim();
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳区及街道工程", "",
                            string.Empty, code, prjName, prjAddress, buildUnit,
                            beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount)
                            {
                                return list;
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
