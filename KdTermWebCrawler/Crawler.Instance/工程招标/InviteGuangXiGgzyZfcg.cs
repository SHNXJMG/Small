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
    public class InviteGuangXiGgzyZfcg : WebSiteCrawller
    {
        public InviteGuangXiGgzyZfcg()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广西省公共资源交易中心招标信息(政府采购)";
            this.Description = "自动抓取广西省公共资源交易中心招标信息(政府采购)";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.SiteUrl = "http://www.gxzbtb.cn/gxzbw/showinfo/jyxx.aspx?QuYu=450001&categoryNum=001004001";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();

            Dictionary<string, string> citys = this.GetCitys();
            foreach (string area in citys.Keys)
            {
                int count = 0;
                int pageInt = 1;
                string html = string.Empty;
                string viewState = string.Empty;
                string eventValidation = string.Empty;
                string cookiestr = string.Empty;
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(citys[area], Encoding.UTF8, ref cookiestr);
                }
                catch { return list; }
                Parser parser = new Parser(new Lexer(html));
                NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("nowrap", "true")));
                if (pageNode != null && pageNode.Count > 0)
                {
                    try
                    {
                        string temp = pageNode.AsString().GetRegexBegEnd("总页数", "当前页").Replace("：", "");
                        pageInt = int.Parse(temp);
                    }
                    catch { }
                }
                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        string viewSTATEGENERATOR = ToolHtml.GetHtmlInputValue(html, "__VIEWSTATEGENERATOR");
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                            "__VIEWSTATE",
                            "__VIEWSTATEGENERATOR",
                            "__EVENTTARGET",
                            "__EVENTARGUMENT",
                            "__EVENTVALIDATION",
                            "MoreInfoList1$txtTitle"
                        },
                            new string[] {
                                viewState,
                                viewSTATEGENERATOR,
                                "MoreInfoList1$Pager",
                                i.ToString(),
                                eventValidation,
                                ""
                            });
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(citys[area], nvc, Encoding.UTF8, ref cookiestr);
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "MoreInfoList1_DataGrid1")));
                    if (listNode != null && listNode.Count > 0)
                    {
                        TableTag table = listNode[0] as TableTag;
                        for (int j = 1; j < table.RowCount; j++)
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                        prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                        specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                        remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                        CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            TableRow tr = table.Rows[j];
                            ATag aTag = tr.Columns[1].GetATag();
                            prjName = aTag.GetAttribute("title").GetReplace("【正在报名】,【报名结束】");
                            beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                            InfoUrl = "http://www.gxzbtb.cn" + aTag.Link;
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.AsHtml();
                                inviteCtx = HtmlTxt.ToCtxString();
                                prjAddress = inviteCtx.GetAddressRegex().GetReplace(" ");
                                buildUnit = inviteCtx.GetBuildRegex().GetReplace(" ");
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                                if (buildUnit.Contains("地址"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址")) + "地址";
                                code = inviteCtx.GetCodeRegex().GetCodeDel().GetReplace(" ");
                                msgType = "广西壮族自治区公共资源交易中心";
                                specType = "政府采购";
                                inviteType = prjName.GetInviteBidType();
                                buildUnit = buildUnit.Replace(" ", "");
                                InviteInfo info = ToolDb.GenInviteInfo("广西壮族自治区", "广西壮族自治区及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                count++;
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
                                            else
                                                link = "http://www.gxzbtb.cn/" + a.Link.GetReplace("../,./");
                                            if (Encoding.Default.GetByteCount(link) > 500)
                                                continue;
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }
                                if (!crawlAll && count >= this.MaxCount) goto Funcs;
                            }
                        }
                    }
                }
                Funcs:;
            }
            return list;
        }


        protected Dictionary<string, string> GetCitys()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { }
            Parser parser = new Parser(new Lexer(html));
            NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hubs")), true), new TagNameFilter("li")));

            if (listNode != null && listNode.Count > 0)
            {
                for (int i = 0; i < listNode.Count; i++)
                {
                    Bullet node = listNode[i] as Bullet;
                    string id = node.GetAttribute("id");
                    string city = node.ToNodePlainString();
                    string url = string.Format("http://www.gxzbtb.cn/gxzbw/showinfo/MoreInfo.aspx?QuYu={0}&categoryNum=001004001", id);
                    dic.Add(city, url);
                }
            }
            return dic;
        }
    }
}
