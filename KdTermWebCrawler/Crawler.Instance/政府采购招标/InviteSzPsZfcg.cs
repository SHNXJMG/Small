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
    public class InviteSzPsZfcg : WebSiteCrawller
    {
        public InviteSzPsZfcg()
            : base()
        {
            this.Enabled = false;
            this.Group = "政府采购招标信息";
            this.Title = "深圳市坪山新区政府采购招标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市坪山新区政府采购招标信息";
            this.SiteUrl = "http://ps.szzfcg.cn/portal/topicView.do?method=view&siteId=9&id=1660";
            this.MaxCount = 1500;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter( new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("name", "__ec_pages")),true),new TagNameFilter("option")));

            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    OptionTag selectTag = pageNode[pageNode.Count - 1] as OptionTag;
                    pageInt = int.Parse(selectTag.Value);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "ec_i", "topicChrList_20070702_crd", "topicChrList_20070702_f_a", "topicChrList_20070702_p", "topicChrList_20070702_s_name", "id", "method", "__ec_pages", "topicChrList_20070702_rd", "topicChrList_20070702_f_name", "topicChrList_20070702_f_ldate" }, new string[] { "topicChrList_20070702", "20", string.Empty, i.ToString(), string.Empty, "1660", "view", (i - 1).ToString(), "20", string.Empty, string.Empty });
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html.Replace("tbody", "table")));
                NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "tableBody")));
                if (tdNodes != null && tdNodes.Count > 0)
                {
                    TableTag table = tdNodes[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[1].ToPlainTextString().Trim();
                        beginDate = tr.Columns[3].ToPlainTextString().Trim();
                        InfoUrl = "http://ps.szzfcg.cn" + tr.Columns[1].GetATagHref();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldetail));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();

                            code = inviteCtx.GetCodeRegex().Replace("）","") ;
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            prjAddress = prjAddress.Replace("&rdquo", "").Replace("&ldquo", "");
                            msgType = "深圳市坪山新区政府采购中心";
                            specType = "政府采购";
                            code = code.Replace("（", "").Replace("(", "").Replace("）", "").Replace(")", "").GetChina();
                            
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳政府采购", "坪山新区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, string.Empty, InfoUrl, HtmlTxt);
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
