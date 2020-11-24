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
    public class InviteZJJingJi : WebSiteCrawller
    {
        public InviteZJJingJi()
            : base()
        {
            this.Group = "区县招标信息";
            this.Title = "广东省湛江经济技术开发区";
            this.Description = "自动抓取广东省广东省湛江经济技术开发区";
            this.PlanTime = "9:31,10:32,14:04,16:05";
            this.SiteUrl = "http://www.zetdz.gov.cn/tzgg/list.asp?class=3";
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
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("style", "float:left;line-height:30px;height:30px;")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToNodePlainString().ToCtxString().Replace("[", "页");
                    //temp = temp.Remove(temp.IndexOf("["));
                    Regex reg = new Regex(@"/[^页]+页");
                    string page = reg.Match(temp).Value.Replace("/", "").Replace("页", "");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl) + "&page=" + i.ToString(), Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeListn = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "95%"))); 
                if (nodeListn != null && nodeListn.Count > 1)
                {
                    TableTag table = nodeListn[1] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                 prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                 specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                 remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                 CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        beginDate = tr.Columns[0].ToNodePlainString().GetDateRegex();
                        prjName = tr.Columns[0].ToNodePlainString().Replace(beginDate, "").Replace("[]","");
                        inviteType = prjName.GetInviteBidType();
                        InfoUrl = "http://www.zetdz.gov.cn/tzgg/" + tr.Columns[0].GetATagHref();

                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                            htlDtl = htlDtl.GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "95%")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            HtmlTxt = dtlList.ToHtml();
                            inviteCtx = HtmlTxt.ToCtxString();

                            buildUnit = inviteCtx.GetBuildRegex();
                            code = inviteCtx.GetCodeRegex();
                            prjAddress = inviteCtx.GetAddressRegex();

                            msgType = "湛江经济技术开发区政府采购中心";
                            specType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "湛江市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
