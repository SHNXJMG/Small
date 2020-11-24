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
using System.Threading;


namespace Crawler.Instance
{
    public class BidWuHanJsgc : WebSiteCrawller
    {
        public BidWuHanJsgc()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "武汉市建设工程交易中心中标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取武汉市建设工程交易中心中标信息";
            this.SiteUrl = "http://www.jy.whzbtb.com/V2PRTS/WinningPublicityInfoListInit.do";
            this.MaxCount = 1500;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string newUrl = "http://www.jy.whzbtb.com/V2PRTS/WinningPublicityInfoList.do";
            IList list = new List<BidInfo>();
            int pageInt = 1, count = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = null;
            try
            {
                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "page", "rows" },
                   new string[] { "1", "200" });
                html = this.ToolWebSite.GetHtmlByUrl(newUrl, nvc);

                smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);

                int totalCount = Convert.ToInt32(smsTypeJson["total"]);

                pageInt = totalCount / 200 + 1;
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
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "page", "rows" },
                          new string[] { i.ToString(), "200" });

                        html = this.ToolWebSite.GetHtmlByUrl(newUrl, nvc);

                        serializer = new JavaScriptSerializer();
                        smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
                    }
                    catch { continue; }
                }
                foreach (KeyValuePair<string, object> obj in smsTypeJson)
                {
                    if (obj.Key == "total" || obj.Key.Equals("pageSize") || obj.Key.Equals("pageNumber")) continue;
                    object[] array = (object[])obj.Value;
                    foreach (object arrValue in array)
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
                                 prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;

                        string id = Convert.ToString(dic["id"]);
                        prjName = Convert.ToString(dic["prjName"]);
                        code = Convert.ToString(dic["constructionNo"]);
                        buildUnit = Convert.ToString(dic["tenderCorp"]);
                        bidUnit = Convert.ToString(dic["corpName"]);
                        beginDate = Convert.ToString(dic["publicityStartDate"]);
                        endDate = Convert.ToString(dic["publicityEndDate"]);
                        bidType = Convert.ToString(dic["tenderContent"]);
                        bidMoney = Convert.ToString(dic["winningPrice"]);

                        InfoUrl = "http://www.jy.whzbtb.com/V2PRTS/WinningPublicityInfoDetail.do?id=" + id;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        Parser parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zbggxx")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();
                            msgType = "武汉市建设工程交易中心";
                            specType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("湖北省", "湖北省及地市", "武汉市", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;

                            count++;
                            if (count >= 20)
                            {
                                count = 1;
                                Thread.Sleep(600 * 1000);
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
