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
    public class ItemPlanLg : WebSiteCrawller
    {
        public ItemPlanLg()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "深圳市龙岗政府在线项目审批信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市龙岗政府在线项目审批信息";
            this.SiteUrl = "http://www.lg.gov.cn/col/col6801/index.html";
            this.MaxCount = 800;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ItemPlan>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                string postUrl = string.Empty;
                if(this.MaxCount>50)
                    postUrl = "http://www.lg.gov.cn/module/jslib/jquery/jpage/dataproxy.jsp?startrecord=1&endrecord=681&perpage=681";
                else
                    postUrl = "http://www.lg.gov.cn/module/jslib/jquery/jpage/dataproxy.jsp?startrecord=1&endrecord="+this.MaxCount+"&perpage="+this.MaxCount;
                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "appid",
                        "webid",
                        "path",
                        "col",
                        "columnid",
                        "sourceContentType",
                        "unitid",
                        "webname",
                        "permissiontype"
                        }, new string[]{
                        "1",
                        "1",
                        "/",
                        "1",
                        "6801",
                        "1",
                        "9393",
                        "龙岗政府在线",
                        "0"
                        });
                html = this.ToolWebSite.GetHtmlByUrl(postUrl, nvc);
                Regex reg = new Regex("(?<=(kdxx))[.\\s\\S]*?(?=(xxdk))", RegexOptions.Multiline | RegexOptions.Singleline);
                string c = reg.Match(html.Replace("['", "kdxx").Replace("']", "xxdk")).Value.Replace("kdxx", "").Replace("xxdk", "").Replace("','", "");
                html = "<table>" + c + "</table>";
            }
            catch { }
            Parser parser = new Parser(new Lexer(html));
            NodeList listNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
            if (listNode != null && listNode.Count > 0)
            {

                TableTag table = listNode[0] as TableTag;
                for (int j = 0; j < table.RowCount; j++)
                {
                    TableRow tr = table.Rows[j];
                    string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                    ATag aTag = tr.Columns[0].GetATag();
                    ItemName = aTag.GetAttribute("title");
                    PlanDate = tr.Columns[1].ToPlainTextString().GetDateRegex();
                    InfoUrl = aTag.Link;
                    string htmldtl = string.Empty;
                    try
                    {
                        htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                    parser = new Parser(new Lexer(htmldtl));
                    NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zoom")));
                    if (dtlNode != null && dtlNode.Count > 0)
                    {
                        CtxHtml = dtlNode.AsHtml();
                        ItemCtx = CtxHtml.ToCtxString();
                        PlanType = "项目审批信息";
                        MsgType = "深圳市龙岗区发改局";

                        ItemPlan info = ToolDb.GenItemPlan("广东省", "深圳市区", "龙岗区", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
