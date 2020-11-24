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
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Data;

namespace Crawler.Instance
{
    public class BidDgXzzfcg : WebSiteCrawller
    {
        public BidDgXzzfcg()
            : base() 
        {
            this.Group = "政府采购中标信息";
            this.Title = "东莞市政府采购中标信息(乡镇)";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取东莞市政府采购中标信息(乡镇)";
            this.SiteUrl = "https://www.dgzb.com.cn/ggzy/website/WebPagesManagement/TradeInfo/GovProcurement/govlist?fcInfotype=7&openbidbelong=ZJ&belongIndex=1&govTypeIndex=2";
            this.MaxCount = 800;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("script"), new HasAttributeFilter("type", "text/javascript")));
            string b = pageNode.AsString().GetCtxBr();
            string s = b.Replace("('", "心情").Replace("')", "你猜");
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = s.GetRegexBegEnd("心情", "你猜");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i >= 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                        new string[] { "fcInfotitle",
                            "currentPage"},
                        new string[]{
                        "",
                        i.ToString()
                        }
                        );
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("https://www.dgzb.com.cn/ggzy/website/WebPagesManagement/TradeInfo/GovProcurement/findListByPage?fcInfotype=7&openbidbelong=ZJ", nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);

                foreach (KeyValuePair<string, object> obj in smsTypeJson)
                {
                    object[] array = (object[])obj.Value;

                    foreach (object arrValue in array)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                        code = Convert.ToString(dic["fcTendersn"]);
                        prjName = Convert.ToString(dic["fcInfotitle"]);
                        beginDate = Convert.ToString(dic["fcInfostartdate"]).GetDateRegex("yyyy-MM-dd");
                        string xu = Convert.ToString(dic["publishinfoid"]);
                        InfoUrl = "https://www.dgzb.com.cn/ggzy/website/WebPagesManagement/TradeInfo/GovProcurement/govdetail?publishinfoid=" + xu + "&fcInfotype=7";
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        bool isTable = true;
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("align", "center")));
                        if (dtlNode == null || dtlNode.Count < 1)
                        {
                            isTable = false;
                            parser.Reset();
                            dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content")));
                        }
                        if (dtlNode == null || dtlNode.Count < 1)
                        {
                            isTable = false;
                            parser = new Parser(new Lexer(htmldtl));
                            dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        }
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.ToHtml();
                            if (isTable)
                            {
                                TableTag dtlTable = dtlNode[0] as TableTag;
                                for (int d = 0; d < dtlTable.RowCount; d++)
                                {
                                    try
                                    {
                                        bidCtx += dtlTable.Rows[d].Columns[0].ToPlainTextString().Replace("：", "").Replace(":", "") + "：";
                                        bidCtx += dtlTable.Rows[d].Columns[1].ToPlainTextString() + "\r\n";
                                    }
                                    catch { }
                                }
                            }
                            if (string.IsNullOrEmpty(bidCtx)) bidCtx = HtmlTxt.ToCtxString();

                            bidCtx = bidCtx.GetCtxBr();
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            bidType = prjName.GetInviteBidType();
                            if (buildUnit.Contains("&#"))
                                buildUnit = string.Empty;
                            string ctx = string.Empty;
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList bidUnitNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "caigou_table")));
                            if (bidUnitNode != null && bidUnitNode.Count > 0)
                            {
                                TableTag unitTable = bidUnitNode[0] as TableTag;
                                try
                                {
                                    for (int c = 0; c < unitTable.Rows[0].ColumnCount; c++)
                                    {
                                        ctx += unitTable.Rows[0].Columns[c].ToNodePlainString() + "：";
                                        ctx += unitTable.Rows[1].Columns[c].ToNodePlainString() + "\r\n";
                                    }
                                }
                                catch { }
                                bidUnit = ctx.GetBidRegex();

                                bidMoney = ctx.GetMoneyRegex();
                            }
                            if (bidUnit.Contains("&#"))
                                bidUnit = string.Empty;

                            msgType = "东莞市政府采购";
                            specType = "政府采购";


                            BidInfo info = ToolDb.GenBidInfo("广东省", "东莞市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aTagNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aTagNode != null && aTagNode.Count > 0)
                            {
                                for (int k = 0; k < aTagNode.Count; k++)
                                {
                                    ATag aTag = aTagNode[k].GetATag();
                                    if (aTag.IsAtagAttach())
                                    {
                                        BaseAttach attach = ToolDb.GenBaseAttach(aTag.LinkText, info.Id, aTag.Link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }




                        //parser = new Parser(new Lexer(html));
                        //NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table02")));
                        //if (listNode != null && listNode.Count > 0)
                        //{
                        //    TableTag table = listNode[0] as TableTag;
                        //    for (int j = 1; j < table.RowCount; j++)
                        //    {
                        //        TableRow tr = table.Rows[j];
                        //        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;


                        //        prjName = tr.Columns[2].ToNodePlainString();
                        //        code = tr.Columns[1].ToNodePlainString();
                        //        beginDate = tr.Columns[3].ToPlainTextString().GetDateRegex("yyyy-MM-dd");
                        //        string v=tr.Columns[1].GetATagHref().Replace("/viewer.do?id=", "");
                        //        InfoUrl = "http://dggp.dg.gov.cn/portal/documentView.do?method=view&id=" + tr.Columns[1].GetATagHref().Replace("/viewer.do?id=", "");
                        //        string htmldtl = string.Empty;
                        //        try
                        //        {
                        //            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        //        }
                        //        catch
                        //        {
                        //            continue;
                        //        }
                        //        bool isTable = true;
                        //        parser = new Parser(new Lexer(htmldtl));
                        //        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "bulletinContent")));
                        //        if (dtlNode == null || dtlNode.Count < 1)
                        //        {
                        //            isTable = false;
                        //            parser.Reset();
                        //            dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "container")));
                        //        }
                        //        if (dtlNode == null || dtlNode.Count < 1)
                        //        {
                        //            isTable = false;
                        //            parser = new Parser(new Lexer(htmldtl));
                        //            dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        //        }
                        //        if (dtlNode != null && dtlNode.Count > 0)
                        //        {
                        //            HtmlTxt = dtlNode.ToHtml();
                        //            if (isTable)
                        //            {
                        //                TableTag dtlTable = dtlNode[0] as TableTag;
                        //                for (int d = 0; d < dtlTable.RowCount; d++)
                        //                {
                        //                    try
                        //                    {
                        //                        bidCtx += dtlTable.Rows[d].Columns[0].ToPlainTextString().Replace("：", "").Replace(":", "") + "：";
                        //                        bidCtx += dtlTable.Rows[d].Columns[1].ToPlainTextString() + "\r\n";
                        //                    }
                        //                    catch { }
                        //                }
                        //            }
                        //            if (string.IsNullOrEmpty(bidCtx)) bidCtx = HtmlTxt.ToCtxString();

                        //            bidCtx = bidCtx.GetCtxBr();
                        //            buildUnit = bidCtx.GetBuildRegex();
                        //            prjAddress = bidCtx.GetAddressRegex();
                        //            bidType = prjName.GetInviteBidType();
                        //            if (buildUnit.Contains("&#"))
                        //                buildUnit = string.Empty;
                        //            string ctx = string.Empty;
                        //            parser = new Parser(new Lexer(HtmlTxt));
                        //            NodeList bidUnitNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "projectBundleList")));
                        //            if (bidUnitNode != null && bidUnitNode.Count > 0)
                        //            {
                        //                TableTag unitTable = bidUnitNode[0] as TableTag;
                        //                try
                        //                {
                        //                    for (int c = 0; c < unitTable.Rows[0].ColumnCount; c++)
                        //                    {
                        //                        ctx += unitTable.Rows[0].Columns[c].ToNodePlainString() + "：";
                        //                        ctx += unitTable.Rows[1].Columns[c].ToNodePlainString() + "\r\n";
                        //                    }
                        //                }
                        //                catch { }
                        //                bidUnit = ctx.GetBidRegex();
                        //                bidMoney = ctx.GetMoneyRegex();
                        //            }
                        //            if (bidUnit.Contains("&#"))
                        //                bidUnit = string.Empty;

                        //            msgType = "东莞市政府采购";
                        //            specType = "政府采购";


                        //            BidInfo info = ToolDb.GenBidInfo("广东省", "东莞市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        //            list.Add(info);
                        //            parser = new Parser(new Lexer(HtmlTxt));
                        //            NodeList aTagNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        //            if (aTagNode != null && aTagNode.Count > 0)
                        //            {
                        //                for (int k = 0; k < aTagNode.Count; k++)
                        //                {
                        //                    ATag aTag = aTagNode[k].GetATag();
                        //                    if (aTag.IsAtagAttach())
                        //                    {
                        //                        BaseAttach attach = ToolDb.GenBaseAttach(aTag.LinkText, info.Id, "http://dggp.dg.gov.cn" + aTag.Link);
                        //                        base.AttachList.Add(attach);
                        //                    }
                        //                }
                        //            }
                        //            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        //        }

                        //    }
                    }
            return list;
        }
    }
}
