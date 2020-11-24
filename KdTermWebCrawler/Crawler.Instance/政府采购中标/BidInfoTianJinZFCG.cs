using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class BidInfoTianJinZFCG : WebSiteCrawller
    {
        public BidInfoTianJinZFCG()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "天津市政府采购网中标公告";
            this.Description = "自动抓取天津市政府采购网中标公告";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 1000;
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.tjgp.gov.cn/portal/topicView.do?method=view&view=Infor&id=2014&ver=2&st=1&stmp=1484299824213";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "countPage")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    pageInt = int.Parse(pageNode[0].ToNodePlainString().GetRegexBegEnd("共", "页"));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                   "method",
                   "page",
                   "id",
                   "step",
                   "view",
                   "st",
                   "ldateQGE",
                   "ldateQLE"
                    }, new string[]{
                   "view",
                   i.ToString(),
                   "2014",
                   "1",
                   "Infor",
                   "1","",""
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "reflshPage")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string prjName = string.Empty,
                             buildUnit = string.Empty, bidUnit = string.Empty,
                             bidMoney = string.Empty, code = string.Empty,
                             bidDate = string.Empty,
                             beginDate = string.Empty,
                             endDate = string.Empty, bidType = string.Empty,
                             specType = string.Empty, InfoUrl = string.Empty,
                             msgType = string.Empty, bidCtx = string.Empty,
                             prjAddress = string.Empty, remark = string.Empty,
                             prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = listNode[j].GetATag();
                        if (aTag == null) continue;
                        prjName = aTag.GetAttribute("title");
                        string tempCode = prjName.GetReplace(" (项目编号:", "kdxx").GetReplace(")", "）").GetRegexBegEnd("kdxx", "）");
                        code = tempCode.GetReplace("目编号：,目编号:");
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.tjgp.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty; 
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = System.Web.HttpUtility.HtmlDecode(HtmlTxt.ToCtxString());
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            bidType = prjName.GetInviteBidType();

                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList bidNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "projectBundleList")));
                            if(bidNode!=null&&bidNode.Count>0)
                            {
                                string ctx = string.Empty;
                                TableTag table = bidNode[0] as TableTag;
                                if (table.RowCount >= 2)
                                {
                                    for (int c = 0; c < table.Rows[0].ColumnCount; c++)
                                    {
                                        try
                                        {
                                            string temp = table.Rows[0].Columns[c].ToNodePlainString();
                                            string tempValue = table.Rows[1].Columns[c].ToNodePlainString();
                                            ctx += System.Web.HttpUtility.HtmlDecode(temp) + "：" + System.Web.HttpUtility.HtmlDecode(tempValue) + "\r\n";
                                        }
                                        catch { }
                                    }
                                }
                                bidUnit = ctx.GetBidRegex().Replace("名称", "");
                                bidMoney = ctx.GetMoneyRegex(null, false, "万元");
                            }
                          
                            specType = "政府采购";
                            msgType = "天津政府采购办公室";
                            BidInfo info = ToolDb.GenBidInfo("天津市", "天津市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag aFile = aNode[a] as ATag;
                                    if (aFile.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (aFile.Link.Contains("http"))
                                            link = aFile.Link;
                                        else
                                            link = "http://www.tjgp.gov.cn/" + aFile.Link;
                                        string text = System.Web.HttpUtility.HtmlDecode(aFile.LinkText);
                                        base.AttachList.Add(ToolDb.GenBaseAttach(text, info.Id, link));
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
