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
    public class ItemPlanHeBeiHz:WebSiteCrawller
    {
        public ItemPlanHeBeiHz() :
            base()
        {
            this.Group = "项目立项";
            this.Title = "河北省投资项目管理系统项目立项（核准项目）";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取河北省投资项目管理系统项目立项（核准项目）";
            this.SiteUrl = "http://hebpi.net:80/portal/ShowMoreProjectAction.do?method=HzProListPage&page=1&rp=20";
            this.MaxCount = 500;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ItemPlan>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 5;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch
            {
                return list;
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://hebpi.net:80/portal/ShowMoreProjectAction.do?method=HzProListPage&page=" + i + "&rp=20");
                    }
                    catch { continue; }
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
                foreach (KeyValuePair<string, object> obj in smsTypeJson)
                {
                    if (obj.Key == "total" || obj.Key == "ROWNUM_") continue;
                    object[] array = (object[])obj.Value;
                    foreach (object arrValue in array)
                    {
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;
                        Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;

                        ItemName = Convert.ToString(dic["CUTNAME"]);
                        PlanDate = Convert.ToString(dic["DD"]);
                        InfoUrl = "http://hebpi.net:80/portal/ShowMoreProjectAction.do?method=detail&id=" + Convert.ToString(dic["ID"]);
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        Parser parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "neirongleft")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            CtxHtml = dtlNode.AsHtml();
                            ItemCtx = CtxHtml.ToCtxString();
                            TotalInvest = ItemCtx.GetRegexBegEnd("总投资", "万元");
                            PlanType = "项目核准信息";
                            MsgType = "河北省发展和改革委员会";

                            ItemPlan info = ToolDb.GenItemPlan("河北省", "河北省及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl);

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
