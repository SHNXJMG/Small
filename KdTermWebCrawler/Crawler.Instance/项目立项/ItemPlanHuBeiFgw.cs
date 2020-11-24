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
    public class ItemPlanHuBeiFgw:WebSiteCrawller
    {
        public ItemPlanHuBeiFgw()
            : base()
        {
            this.Group = "项目立项";
            this.Title = "湖北省发展和改革委员会项目核准信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取湖北省发展和改革委员会项目核准信息";
            this.SiteUrl = "http://www.hbfgw.gov.cn/hqfw/xmgg/xmkzgg/index.shtml";
            this.MaxCount = 800;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ItemPlan>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 27;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return null; } 
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.hbfgw.gov.cn/hqfw/xmgg/xmkzgg/index_"+(i-1).ToString()+".shtml");
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "mytable")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string ItemCode = string.Empty, ItemName = string.Empty, ItemAddress = string.Empty, BuildUnit = string.Empty, BuildNature = string.Empty, TotalInvest = string.Empty, PlanInvest = string.Empty, IssuedPlan = string.Empty, InvestSource = string.Empty, ApprovalUnit = string.Empty, ApprovalDate = string.Empty, ApprovalCode = string.Empty, MsgUnit = string.Empty, PlanDate = string.Empty, PlanType = string.Empty, PlanBeginDate = string.Empty, PlanEndDate = string.Empty, CtxHtml = string.Empty, ItemCtx = string.Empty, ItemContent = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty;
                        TableRow tr = table.Rows[j];
                        ItemCode = tr.Columns[0].ToNodePlainString().GetReplace("('无')").GetReplace("('", "kdxx").GetReplace("')","xxdk").GetRegexBegEnd("kdxx","xxdk");
                        ATag aTag = tr.Columns[1].GetATag();
                        ItemName = aTag.LinkText;
                        ApprovalUnit = tr.Columns[2].ToNodePlainString();
                        PlanDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.hbfgw.gov.cn/hqfw/xmgg/xmkzgg/" + aTag.Link.GetReplace("../,./");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "appendixDiv")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList hNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("h1"));
                            if (hNode != null && hNode.Count > 0)
                            {
                                string temp = hNode[0].ToNodePlainString();
                                ItemName = string.IsNullOrEmpty(temp) ? ItemName : temp;
                            }
                            ItemName = ItemName.GetReplace("省发改委批复,省发改委核准");
                            CtxHtml = dtlNode.AsHtml().Replace("none","block");
                            ItemCtx = CtxHtml.ToCtxString();

                            string imgUrl = InfoUrl.Substring(0,InfoUrl.LastIndexOf("/"));
                            List<string> attach = new List<string>();
                            parser = new Parser(new Lexer(CtxHtml));
                            NodeList imgNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                            if (imgNode != null && imgNode.Count > 0)
                            {
                                for (int p = 0; p < imgNode.Count; p++)
                                {
                                    ImageTag img = imgNode[p] as ImageTag;
                                    string link = imgUrl +"/"+ img.ImageURL.GetReplace("../,./");
                                    CtxHtml = CtxHtml.GetReplace(img.ImageURL,link);
                                    attach.Add(link);
                                }
                            }
                            PlanType = "项目核准信息";
                            MsgType = "湖北省发展和改革委员会";

                            ItemPlan info = ToolDb.GenItemPlan("湖北省", "湖北省及地市", "", ItemCode, ItemName, ItemAddress, BuildUnit, BuildNature, TotalInvest, PlanInvest, IssuedPlan, InvestSource, ApprovalUnit, ApprovalDate, ApprovalCode, MsgUnit, PlanDate, PlanType, PlanBeginDate, PlanEndDate, CtxHtml, ItemCtx, ItemContent, MsgType, InfoUrl); 
                            list.Add(info);
                            if (attach.Count > 0)
                            {
                                for (int a = 0; a < attach.Count; a++)
                                {
                                    BaseAttach entity = ToolDb.GenBaseAttach(ItemName, info.Id, attach[a]);
                                    base.AttachList.Add(entity);
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
