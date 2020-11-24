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
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteQuanGuoGGZYJYPT : WebSiteCrawller
    {
        public InviteQuanGuoGGZYJYPT()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "全国公共资源交易平台";
            this.Description = "自动抓取全国公共资源交易平台招标中标信息";
            this.PlanTime = "16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://deal.ggzy.gov.cn/ds/deal/dealList.jsp";
            this.MaxCount = 50;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            DateTime startDate = DateTime.Today;
            DateTime endDates = startDate.AddDays(-90);
            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                        "TIMEBEGIN_SHOW",
                        "TIMEEND_SHOW",
                        "TIMEBEGIN",
                        "TIMEEND",
                        "DEAL_TIME",
                        "DEAL_CLASSIFY",
                        "DEAL_STAGE",
                        "DEAL_PROVINCE",
                        "DEAL_CITY",
                        "DEAL_PLATFORM",
                        "DEAL_TRADE",
                        "isShowAll",
                        "PAGENUMBER",
                        "FINDTXT"

                    }, new string[] {
                        endDates.ToString(),
                        startDate.ToString(),
                        endDates.ToString(),
                        startDate.ToString(),
                        "02",
                        "01",
                        "0101",
                        "0",
                        "0",
                        "0",
                        "0",
                        "1",
                        "1",
                        ""
                    });
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                }
                catch { }
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "paging")), true), new TagNameFilter("span")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("共", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {

                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                        "TIMEBEGIN_SHOW",
                        "TIMEEND_SHOW",
                        "TIMEBEGIN",
                        "TIMEEND",
                        "DEAL_TIME",
                        "DEAL_CLASSIFY",
                        "DEAL_STAGE",
                        "DEAL_PROVINCE",
                        "DEAL_CITY",
                        "DEAL_PLATFORM",
                        "DEAL_TRADE",
                        "isShowAll",
                        "PAGENUMBER",
                        "FINDTXT"

                    }, new string[] {
                        endDates.ToString(),
                        startDate.ToString(),
                        endDates.ToString(),
                        startDate.ToString(),
                        "02",
                        "01",
                        "0101",
                        "0",
                        "0",
                        "0",
                        "0",
                        "1",
                        i.ToString(),
                        ""
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "publicont")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string nlse = string.Empty;
                        string ywlx = string.Empty;
                        string sehu = string.Empty;
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;
                        string nod = node.ToHtml();
                        parser = new Parser(new Lexer(nod));
                        NodeList txtNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "span_on")));
                        if (txtNode != null && txtNode.Count > 0)
                        {
                            sehu = txtNode[0].ToNodePlainString();
                            nlse = txtNode[3].ToNodePlainString();
                            ywlx = txtNode[2].ToNodePlainString();
                        }
                        if (nlse.Contains("招标/资审公告"))
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                  prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                  specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                  remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                  CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                            prjName = aTag.GetAttribute("title");
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            beginDate = node.ToPlainTextString().GetDateRegex();
                            InfoUrl = aTag.Link.GetReplace("amp;");
                            string htmlDtl = string.Empty;
                            try
                            {
                                htmlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                                htmlDtl = ToolHtml.GetRegexHtlTxt(htmlDtl);
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmlDtl));
                            NodeList zsList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "div_0101")));
                            if (zsList != null && zsList.Count > 0)
                            {
                                try
                                {
                                    INode nodezs = zsList[0];
                                    ATag aTagzs = nodezs.GetATag();
                                    string urlzs = aTagzs.GetAttribute("onclick");
                                    string urls = urlzs.GetReplace("showdetail(this, '0101','", "").GetReplace("')", "").Replace(",", "").Replace(")", "");
                                    urls = "http://www.ggzy.gov.cn/information" + urls;
                                    htmlDtl = this.ToolWebSite.GetHtmlByUrl(urls, Encoding.UTF8);
                                    htmlDtl = ToolHtml.GetRegexHtlTxt(htmlDtl);
                                }
                                catch (Exception) { throw; }
                            }

                            parser = new Parser(new Lexer(htmlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "detail")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                string ctxUrl = string.Empty;
                                HtmlTxt = dtlList.AsHtml();

                                inviteCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString();
                                try
                                {
                                    Parser parurl = new Parser(new Lexer(HtmlTxt));
                                    NodeList zsUrl = parurl.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("p"), new HasAttributeFilter("class", "p_o")));
                                    if (zsUrl != null && zsUrl.Count > 0)
                                    {
                                        INode urlzs = zsUrl[0];
                                        ATag aTagurl = urlzs.GetATag();
                                        ctxUrl = "原文链接地址 : " + aTagurl.Link;
                                    }
                                }
                                catch (Exception ex)
                                { }
                                inviteCtx = inviteCtx + ctxUrl;
                                prjAddress = inviteCtx.GetAddressRegex();
                                buildUnit = inviteCtx.GetBuildRegex();
                                code = inviteCtx.GetCodeRegex();
                                if (string.IsNullOrEmpty(buildUnit))
                                    buildUnit = inviteCtx.GetRegex("招标人");
                                buildUnit = ToolHtml.GetSubString(buildUnit, 150);
                                if (string.IsNullOrWhiteSpace(code))
                                {
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList bidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                    if (bidNode != null && bidNode.Count > 0)
                                    {
                                        string ctx = string.Empty;
                                        TableTag bidTable = bidNode[0] as TableTag;
                                        try
                                        {
                                            for (int r = 0; r < bidTable.RowCount; r++)
                                            {
                                                ctx += bidTable.Rows[r].Columns[0].ToNodePlainString() + "：";
                                                ctx += bidTable.Rows[r].Columns[1].ToNodePlainString() + "\r\n";
                                            }
                                        }
                                        catch { }

                                        if (string.IsNullOrWhiteSpace(buildUnit))
                                            buildUnit = ctx.GetBuildRegex();

                                        if (string.IsNullOrWhiteSpace(prjAddress))
                                            prjAddress = ctx.GetAddressRegex();

                                        if (string.IsNullOrWhiteSpace(code))
                                            code = ctx.GetCodeRegex();
                                    }
                                }

                                msgType = "国家信息中心";
                                specType = "建设工程";
                                inviteType = "建设工程";
                                string[] provs = GetPrivoce(sehu);

                                InviteInfo info = ToolDb.GenInviteInfo(provs[0], provs[1], "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                try
                                {
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList nodeFm = parser.ExtractAllNodesThatMatch((new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "detail_content")))));
                                    if (dtlList != null && dtlList.Count > 0)
                                    {
                                        INode nodFm = nodeFm[0];
                                        ATag aTagzs = nodFm.GetATag();
                                        string dfe = aTagzs.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach("内容(点击下载)", info.Id, dfe);
                                        base.AttachList.Add(attach);
                                    }
                                }
                                catch { }
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                        else { continue; }

                    }
                }
            }
            return list;
        }

        protected string[] GetPrivoce(string prov)
        {
            string[] strs = null;
            switch (prov)
            {
                case "内蒙古":
                    strs = new string[] { "内蒙古自治区", "内蒙古自治区及盟市" };
                    break;
                case "广西":
                    strs = new string[] { "广西壮族自治区", "广西壮族自治区及地市" };
                    break;
                case "宁夏":
                    strs = new string[] { "宁夏回族自治区", "宁夏回族自治区及地市" };
                    break;
                case "新疆":
                case "兵团":
                    strs = new string[] { "新疆维吾尔自治区", "新疆维吾尔自治区及地市" };
                    break;
                case "西藏":
                    strs = new string[] { "西藏自治区", "西藏自治区及地市" };
                    break;
                case "广东":
                    strs = new string[] { "广东省", "广东省内工程" };
                    break;
                case "重庆":
                    strs = new string[] { "重庆市", "重庆市及区县" };
                    break;
                case "上海":
                    strs = new string[] { "上海市", "上海市区" };
                    break;
                case "天津":
                    strs = new string[] { "天津市", "天津市区" };
                    break;
                case "北京":
                    strs = new string[] { "北京市", "北京市区" };
                    break;
                default:
                    strs = new string[] { prov + "省", prov + "省及地市" };
                    break;
            }
            return strs;
        }
    }
}
