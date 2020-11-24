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
    public class BidFuJianFgw : WebSiteCrawller
    {
        public BidFuJianFgw()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "福建省发展和改革委员会中标信息";
            this.Description = "自动抓取福建省发展和改革委员会中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.fjbid.gov.cn/was5/web/search?channelid=216460&random=0.07795790286021231&page=1&callback=jQuery111302905473880961891_1502868144660&_=1502868144661&prepage=";

        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();

            int sqlCount = 0;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + this.MaxCount);
            }
            catch { return null; }
            int startIndex = html.IndexOf("{");
            int endIndex = html.LastIndexOf("}");
            html = html.Substring(startIndex, (endIndex + 1) - startIndex);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
            object[] objvalues = smsTypeJson["docs"] as object[];
            foreach (object objValue in objvalues)
            {
                Dictionary<string, object> dic = (Dictionary<string, object>)objValue;
                string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
              bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                string ziGeDengJi = string.Empty, ziGeZhengShu = string.Empty, zbFangShi = string.Empty;
                beginDate = Convert.ToString(dic["CRTIME"]);
                if (beginDate.Contains("发布时间"))
                    continue;
                string ZR_ID = Convert.ToString(dic["ZR_ID"]);
                prjName = Convert.ToString(dic["ZR_TITLE"]);
                InfoUrl = "http://www.fjbid.gov.cn/was5/web/detail?channelid=216460&classsql=ZR_ID=" + ZR_ID;
                string htmldtl = string.Empty;
                try
                {
                    htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                }
                catch { continue; }
                Parser parser = new Parser(new Lexer(htmldtl));
                NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "xl_column")));
                if (dtlNode != null && dtlNode.Count > 0)
                {
                    HtmlTxt = dtlNode.AsHtml();
                    bidCtx = HtmlTxt.GetReplace("<br/>,<br />,<br>", "\r\n").ToCtxString().GetReplace("标价", "中标金额");
                    prjAddress = bidCtx.GetAddressRegex();
                    buildUnit = bidCtx.GetBuildRegex();
                    bidUnit = bidCtx.GetBidRegex();
                    bidMoney = bidCtx.GetMoneyRegex();
                    prjMgr = bidCtx.GetMgrRegex();

                    if (string.IsNullOrWhiteSpace(prjMgr))
                        prjMgr = bidCtx.GetRegexBegEnd("项目负责人","证号");
                    if (string.IsNullOrWhiteSpace(prjMgr))
                        prjMgr = bidCtx.GetRegexBegEnd("项目经理", "证号");
                    if (string.IsNullOrWhiteSpace(prjMgr))
                        prjMgr = bidCtx.GetRegexBegEnd("项目经理", "证书");
                    if (string.IsNullOrWhiteSpace(prjMgr))
                        prjMgr = bidCtx.GetRegexBegEnd("项目经理", "工期");
                    if (string.IsNullOrWhiteSpace(prjMgr))
                        prjMgr = bidCtx.GetRegexBegEnd("项目经理", "注册");
                    if (string.IsNullOrWhiteSpace(prjMgr))
                        prjMgr = bidCtx.GetRegexBegEnd("总监理工程师：", "证号");
                    if (string.IsNullOrWhiteSpace(prjMgr))
                        prjMgr = bidCtx.GetRegexBegEnd("总监理工程师", "证书");
                    if (string.IsNullOrWhiteSpace(prjMgr))
                        prjMgr = bidCtx.GetRegexBegEnd("总监：", "注册");

                    if (string.IsNullOrEmpty(code))
                        code = bidCtx.GetCodeRegex();
                    if (buildUnit.Contains("公司"))
                        buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                    if (buildUnit.Contains("联系"))
                        buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                    if (buildUnit.Contains("地址"))
                        buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                    code = bidCtx.GetCodeRegex();
                    bidUnit = bidCtx.GetBidRegex().GetReplace("A标,B标,C标,第一标段");
                    if (bidUnit.Contains("公司"))
                        bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                    if (bidUnit.Contains("详见")
                        || bidUnit.Contains("/"))
                        bidUnit = string.Empty;
                    bidMoney = bidCtx.GetMoneyRegex();

                   // prjMgr = bidCtx.GetMgrRegex();
                    if (prjMgr.Contains("证书"))
                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("证书"));
                    if (prjMgr.Contains("等级"))
                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("等级"));
                    if (prjMgr.Contains("注册"))
                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("注册"));
                    if (prjMgr.Contains("岗位"))
                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("岗位"));
                    if (prjMgr.Contains("（"))
                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("（"));
                    if (prjMgr.Contains("("))
                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("("));
                    if (prjMgr.Contains("证号"))
                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("证号"));
                    msgType = "福建省发展和改革委员会";
                    specType = bidType = "建设工程";
                    BidInfo info = ToolDb.GenBidInfo("福建省", "福建省及地市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                    list.Add(info);
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }
            return list;
        }
    }
}
