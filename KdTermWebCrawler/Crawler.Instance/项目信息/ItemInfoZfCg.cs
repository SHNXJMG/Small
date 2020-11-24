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
    public class ItemInfoZfCg : WebSiteCrawller
    {
        public ItemInfoZfCg()
            : base()
        {
            this.Group = "项目信息";
            this.PlanTime = "12:10,03:15";
            this.Title = "深圳市政府采购采购需求公示";
            this.MaxCount = 200;
            this.Description = "自动抓取深圳市政府采购采购需求公示";
            this.ExistCompareFields = "URL";
            this.SiteUrl = "http://www.szzfcg.cn/portal/topicView.do?method=view&id=2719966";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ItemInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1,sqlCount=1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "statusBar")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString().GetRegexBegEnd("找到", "条");
                    pageInt = (Convert.ToInt32(temp) + 20 - 1) / 20;
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    string id = ToolHtml.GetHtmlInputValue(html, "id");
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "ec_i",
                    "topicChrList_20070702_crd",
                    "topicChrList_20070702_f_a",
                    "topicChrList_20070702_p",
                    "topicChrList_20070702_s_name",
                    "topicChrList_20070702_s_topName",
                    "id",
                    "method",
                    "__ec_pages",
                    "topicChrList_20070702_rd",
                    "topicChrList_20070702_f_name",
                    "topicChrList_20070702_f_topName",
                    "topicChrList_20070702_f_ldate"
                    },
                        new string[]{
                        "topicChrList_20070702",
                        "20",
                        "",
                        i.ToString(),
                        "",
                        "",
                        id,
                        "view",
                        i.ToString(),
                        "20",
                        "","",""
                        });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "topicChrList_20070702_table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 3; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string itemCode = string.Empty, itemName = string.Empty, buildUnit = string.Empty, address = string.Empty,
                            investMent = string.Empty, buildKind = string.Empty, investKink = string.Empty, linkMan = string.Empty,
                            linkmanTel = string.Empty, itemDesc = string.Empty, apprNo = string.Empty, apprDate = string.Empty,
                            apprUnit = string.Empty, apprResult = string.Empty, landapprNo = string.Empty, landplanNo = string.Empty, buildDate = string.Empty, infoSource = string.Empty, url = string.Empty,
                            textCode = string.Empty, licCode = string.Empty, msgType = string.Empty, ctxHtml = string.Empty;

                        string listName = string.Empty;
                        listName = tr.Columns[1].ToNodePlainString();
                        buildDate = tr.Columns[3].ToNodePlainString().GetDateRegex();

                        url = "http://www.szzfcg.cn" + tr.Columns[1].GetATagHref();

                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "tab")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            ctxHtml = dtlNode.AsHtml();
                            infoSource = ctxHtml.ToCtxString();

                            string ctx = string.Empty;
                            TableTag dtlTable = dtlNode[0] as TableTag;
                            for (int k = 0; k < dtlTable.RowCount; k++)
                            {
                                for (int d = 0; d < dtlTable.Rows[k].ColumnCount; d++)
                                {
                                    if ((d + 1) % 2 == 0)
                                        ctx += dtlTable.Rows[k].Columns[d].ToNodePlainString() + "\r\n";
                                    else
                                        ctx += dtlTable.Rows[k].Columns[d].ToNodePlainString() + "：";
                                }
                            }
                            itemName = ctx.GetRegex("项目名称,工程名称,名称");
                            if (string.IsNullOrEmpty(itemName))
                                itemName = listName;

                            buildUnit = ctx.GetRegex("采购人名称");
                            investMent = ctx.GetRegex("财政预算限额（元）");
                            investMent = investMent.GetMoney();

                            msgType = "深圳政府采购";
                            ItemInfo info = ToolDb.GenItemInfo(itemCode, itemName, buildUnit, address, investMent, buildKind, investKink, linkMan, linkmanTel, itemDesc, apprNo, apprDate, apprUnit, apprResult, landapprNo, landplanNo, buildDate, "广东省", "深圳市区", infoSource, url, textCode, licCode, msgType, ctxHtml);

                            sqlCount++;
                            if (!crawlAll && sqlCount >= this.MaxCount) return list;

                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                            {
                                BaseProject prj = new BaseProject();
                                prj.Id = ToolDb.NewGuid;
                                prj.PrjCode = info.ItemCode;
                                prj.PrjName = info.ItemName;
                                prj.BuildUnit = info.BuildUnit;
                                prj.BuildTime = info.BuildDate;
                                prj.Createtime = info.CreateTime;
                                prj.PrjAddress = info.Address;
                                prj.InfoSource = info.InfoSource;
                                prj.MsgType = info.MsgType;
                                prj.Province = info.Province;
                                prj.City = info.City;
                                prj.Url = info.Url;

                                ToolDb.SaveEntity(prj, "Url", this.ExistsUpdate);
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
