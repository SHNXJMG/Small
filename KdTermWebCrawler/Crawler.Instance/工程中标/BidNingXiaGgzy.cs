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
    public class BidNingXiaGgzy : WebSiteCrawller
    {
        public BidNingXiaGgzy()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "宁夏回族自治区公告资源交易网中标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取宁夏回族自治区公告资源交易网中标信息";
            this.SiteUrl = "http://www.nxggzyjy.org/ningxiaweb/002/002001/002001003/about.html";
            this.MaxCount = 400;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string urlFormt = "http://www.nxzfcg.gov.cn/ningxia/services/BulletinWebServer/getBulletinInfoList?response=application/json&pageIndex=1&pageSize={0}&siteguid={1}&categorynum=002001003&cityname=";
            IList list = new List<BidInfo>();
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch { return list; }
            string siteId = ToolHtml.GetHtmlInputValueById(html, "siteguid");
            string url = string.Format(urlFormt, this.MaxCount, siteId);

            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(url);
            }
            catch { return null; }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
            smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(smsTypeJson["return"].ToString());
            object[] listDatas = (object[])smsTypeJson["Table"];

            foreach (object data in listDatas)
            {
                string prjName = string.Empty,
                          buildUnit = string.Empty, bidUnit = string.Empty,
                          bidMoney = string.Empty, code = string.Empty,
                          bidDate = string.Empty, beginDate = string.Empty,
                          endDate = string.Empty, bidType = string.Empty,
                          specType = string.Empty, InfoUrl = string.Empty,
                          msgType = string.Empty, bidCtx = string.Empty,
                          prjAddress = string.Empty, remark = string.Empty,
                          prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                Dictionary<string, object> dic = data as Dictionary<string, object>;
                prjName = Convert.ToString(dic["title"]);
                string infoid = Convert.ToString(dic["infoid"]);
                beginDate = Convert.ToString(dic["infodate"]);
                string area = prjName.GetReplace("[,]", "kdxx").GetRegexBegEnd("kdxx", "kdxx");
                prjName = prjName.GetReplace(string.Format("[{0}],[{1}],[{2}]", area, "正在报名", "报名结束"));
                if (prjName.Contains("[自治区]"))
                    prjName = prjName.GetReplace("[自治区]");
                InfoUrl = string.Format("http://www.nxggzyjy.org/ningxia/WebbuilderMIS/RedirectPage/RedirectPage.jspx?infoid={0}&categorynum=002001003&locationurl=http://www.nxggzyjy.org/ningxiaweb", infoid);
                string htmldtl = string.Empty;
                try
                {
                    htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                }
                catch { continue; }
                Parser parser = new Parser(new Lexer(htmldtl));
                NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zbgsId")));
                //页面招标信息改为图片
                //待修改
                if (dtlNode != null && dtlNode.Count > 0)
                {
                    HtmlTxt = dtlNode.AsHtml();
                    bidCtx = HtmlTxt.ToLower().GetReplace("<br/>,<br>,</p>", "\r\n").ToCtxString();
                    buildUnit = bidCtx.GetReplace(" ").GetBuildRegex();
                    if (buildUnit.Contains("招标代理"))
                        buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                    if (buildUnit.Contains("代理"))
                        buildUnit = buildUnit.Remove(buildUnit.IndexOf("代理"));
                    if (buildUnit.Contains("联系"))
                        buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                    if (buildUnit.Contains("改革局"))
                        buildUnit = buildUnit.Remove(buildUnit.IndexOf("改革局")) + "改革局";
                    code = bidCtx.GetReplace(" ").GetCodeRegex().GetCodeDel();
                    bidMoney = bidCtx.GetMoneyRegex(null, false, "万元");
                    bidUnit = bidCtx.GetBidRegex();
                    if (string.IsNullOrEmpty(bidUnit))
                        bidUnit = bidCtx.GetRegex("第一名,通过单位");
                    if (string.IsNullOrEmpty(bidUnit))
                    {
                        parser = new Parser(new Lexer(HtmlTxt));
                        NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("border", "1")));
                        if (tableNode != null && tableNode.Count > 1)
                        {
                            TableTag table = tableNode[0] as TableTag;
                            string ctx = string.Empty;
                            if (table.RowCount >= 2)
                            {
                                for (int c = 0; c < table.Rows[0].ColumnCount; c++)
                                {
                                    try
                                    {
                                        string temp = table.Rows[0].Columns[c].ToNodePlainString();
                                        string tempValue = table.Rows[1].Columns[c].ToNodePlainString();
                                        ctx += temp + "：" + tempValue + "\r\n";
                                    }
                                    catch { }
                                }
                            }
                            bidUnit = ctx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = ctx.GetRegex("单位名称");

                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                ctx = string.Empty;
                                for (int r = 0; r < table.RowCount; r++)
                                {
                                    for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = table.Rows[r].Columns[c].ToNodePlainString();
                                        if (c % 2 == 0)
                                            ctx += temp + "：";
                                        else
                                            ctx += temp + "\r\n";
                                    }
                                }
                                bidUnit = ctx.GetBidRegex();
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = ctx.GetRegex("单位名称");
                                bidUnit = bidUnit.Replace("名称", "");
                            }
                            if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                bidMoney = ctx.GetMoneyRegex(null, false, "万元");
                            prjMgr = ctx.GetMgrRegex();
                        }
                    }
                    try
                    {
                        if (decimal.Parse(bidMoney) >= 1000000)
                            bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                    }
                    catch { }
                    if (string.IsNullOrEmpty(bidUnit))
                    {
                        bidUnit = bidCtx.GetRegexBegEnd("1、", "得分").GetReplace("，");
                    }
                    if (bidUnit.Contains("公司"))
                        bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";

                    msgType = "宁夏公共资源交易管理局";
                    specType = "建设工程";
                    bidType = prjName.GetInviteBidType();
                    BidInfo info = ToolDb.GenBidInfo("宁夏回族自治区", "宁夏回族自治区及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                    list.Add(info);
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
                                    link = "http://www.nxzfcg.gov.cn/" + a.Link;
                                BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                base.AttachList.Add(attach);
                            }
                        }
                    }
                    if (!crawlAll && list.Count >= this.MaxCount)
                        return list;

                }
            }
            return list;
        }
    }
}
