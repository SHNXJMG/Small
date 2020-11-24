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
using System.Web.Script.Serialization;

namespace Crawler.Instance
{
    public class ItemPlanZheJiangFgw : WebSiteCrawller
    {
        public ItemPlanZheJiangFgw() :
            base()
        {
            this.Group = "项目立项";
            this.Title = "浙江省发展和改革委员会项目立项";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取浙江省发展和改革委员会项目立项";
            this.SiteUrl = "http://www.zjdpc.gov.cn/module/jslib/jquery/jpage/dataproxy.jsp?startrecord=1&endrecord=45&perpage=15";
            this.MaxCount = 500;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ItemPlan>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            { 
                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{ 
                "appid",
                "webid",
                "path",
                "columnid",
                "sourceContentType",
                "unitid",
                "webname",
                "permissiontype"
                },
                    new string[]{ 
                "1",
                "1",
                "/",
                "808",
                "1",
                "620",
                "浙江省发展和改革委员会",
                "0"
                    });
                string post="appid=1&webid=1&path=%2F&columnid=808&sourceContentType=1&unitid=620&webname=浙江省发展和改革委员会&permissiontype=0";
                html = ToolHtml.GetHtmlGJByUrlPost(this.SiteUrl, post, Encoding.UTF8, "");//this.ToolWebSite.GetHtmlByUrl("http://www.zjdpc.gov.cn/col/col808/index.html", Encoding.UTF8, ref cookiestr);
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
            }
            catch {  }

            try
            {
                string temp = html.GetRegexBegEnd("totalPage", ";").GetReplace("=");
                pageInt = int.Parse(temp);
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                "col",
                "appid",
                "webid",
                "path",
                "columnid",
                "sourceContentType",
                "unitid",
                "webname",
                "permissiontype"
                },
        new string[]{
                      "1",
                "1",
                "1",
                "/",
                "808",
                "1",
                "620",
                "浙江省发展和改革委员会",
                "0"
                    });
                    try
                    {
                        int endrecord = i * 45;
                        int startrecord = 45 * i - 44;
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.zjdpc.gov.cn/module/jslib/jquery/jpage/dataproxy.jsp?perpage=15&endrecord=" + endrecord + "&startrecord=" + startrecord, nvc);
                    }
                    catch { continue; }
                }

                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;

                        TableRow tr = (listNode[j] as TableTag).Rows[0];
                        ATag aTag = tr.Columns[1].GetATag();
                        ItemName = aTag.GetAttribute("title").GetReplace("省发改委,\\,'");
                        PlanDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.zjdpc.gov.cn" + aTag.Link.GetReplace("\\,'");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zoom")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            ItemCtx = CtxHtml.ToCtxString().GetReplace("begin-->,&ldquo;,&rdquo;,end-->");
                            TotalInvest = ItemCtx.GetRegexBegEnd("总投资", "万元");
                            MsgType = "浙江省公共资源交易中心";
                            PlanType = "项目审批信息";
                            ItemPlan info = ToolDb.GenItemPlan("浙江省", "浙江省及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);
                            list.Add(info);
                            parser = new Parser(new Lexer(CtxHtml));
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
                                            link = "http://www.zjdpc.gov.cn/" + a.Link;
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
