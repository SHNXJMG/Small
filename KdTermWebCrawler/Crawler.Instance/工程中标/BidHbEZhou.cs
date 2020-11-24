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
    public class BidHbEZhou : WebSiteCrawller
    {
        public BidHbEZhou()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "鄂州市公共资源交易中心中标信息";
            this.Description = "自动抓取鄂州市公共资源交易中心中标信息";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://www.ezztb.gov.cn/jiaoyixingxi/jyxx.html?type=10&index=1";
            this.MaxCount = 80;
        }


        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int count = 0;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch { return null; }
            int startIndex = html.IndexOf("{");
            int endIndex = html.LastIndexOf("}");
            html = html.Substring(startIndex, (endIndex + 1) - startIndex);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
            object[] objvalues = smsTypeJson["rows"] as object[];
            foreach (object objValue in objvalues)
            {

                string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
 bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                Dictionary<string, object> dic = (Dictionary<string, object>)objValue;
                prjName = Convert.ToString(dic["title"]);
                //if (!prjName.Contains("湖北能源集团鄂州电厂三期2×1050MW超超临界燃煤机组扩建工程金属及压力容器质量检查（安全性能检查）-(二次)"))
                //{
                //    continue;
                //}
                beginDate = Convert.ToString(dic["faBuStartTimeText"]).GetDateRegex();
                bidType = Convert.ToString(dic["gongChengTypeText"]);
                string foUrl = "http://www.ezztb.gov.cn//jyw/jyw/queryZbgs.do?guid=" + dic["yuanXiTongId"];
                string htmldtl = string.Empty;
                string prName = string.Empty, bdName = string.Empty, xmBh = string.Empty, xmMc = string.Empty,
                        startTime = string.Empty, endTime = string.Empty, zbrAndLht = string.Empty, zbdlJG = string.Empty,
                        zbFangShi = string.Empty, zhongBiaoGQ = string.Empty, ziGeDengJi = string.Empty, ziGeZhengShu = string.Empty;

                try
                {
                    HtmlTxt = this.ToolWebSite.GetHtmlByUrl(foUrl, Encoding.UTF8);
                }
                catch
                {
                    continue;
                }
                JavaScriptSerializer Newserializer = new JavaScriptSerializer();
                Dictionary<string, object> newTypeJson = null;
                try
                {
                    newTypeJson = (Dictionary<string, object>)Newserializer.DeserializeObject(HtmlTxt);
                }
                catch
                { 
                    continue;
                }
                InfoUrl = "http://www.ezztb.gov.cn/jiaoyixingxi/zbgs_view.html?guid=" + dic["yuanXiTongId"];
                try
                {
                    htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl);
                }
                catch { continue; }
                Dictionary<string, object> dics = (Dictionary<string, object>)newTypeJson;
                Dictionary<string, object> bd = dics["bd"] as Dictionary<string, object>;
                Dictionary<string, object> gcx = bd["gc"] as Dictionary<string, object>;
                Dictionary<string, object> xm = bd["xm"] as Dictionary<string, object>;

                code = Convert.ToString(gcx["gcBH"]);
                buildUnit = Convert.ToString(gcx["zbRName"]);
                bidUnit = Convert.ToString(dics["tbrName"]);
                prjMgr = Convert.ToString(dics["xiangMuJiLi"]);
                bidMoney = Convert.ToString(dics["zhongBiaoJE"]);
                prName = Convert.ToString(gcx["gcName"]);
                bdName = Convert.ToString(bd["bdName"]);
                xmBh = Convert.ToString(xm["xm_BH"]);
                xmMc = Convert.ToString(xm["xm_Name"]);
                startTime = Convert.ToString(dics["zbgsStartTime"]);
                startTime = ToolHtml.GetDateTimeByLong(Convert.ToInt64(startTime)).ToString("yyyy-MM-dd HH:mm");
                endTime = Convert.ToString(dics["zbgsEndTime"]);
                endTime = ToolHtml.GetDateTimeByLong(Convert.ToInt64(endTime)).ToString("yyyy-MM-dd HH:mm");
                zbrAndLht = Convert.ToString(dics["zbrAndLht"]);
                zbdlJG = Convert.ToString(dics["zbdlJG"]);
                zbFangShi = Convert.ToString(dics["zbFangShi"]);
                zhongBiaoGQ = Convert.ToString(dics["zhongBiaoGQ"]);
                ziGeDengJi = Convert.ToString(dics["ziGeDengJi"]);
                ziGeZhengShu = Convert.ToString(dics["ziGeZhengShu"]);
                HtmlTxt = ("<table>") + ("<tr><th>招标项目编号：</th><td>" + code + "</td></tr>") +
                ("<tr><th>招标项目名称：</th><td>" + prName + "</td></tr>") +
                ("<tr><th>标段名称：</th><td>" + bdName + "</td></tr>") +
                ("<tr><th>项目编号：</th><td>" + xmBh + "</td></tr>") +
                ("<tr><th>项目名称：</th><td>" + xmMc + "</td></tr>") +
                ("<tr><th>公示时间：</th><td>" + startTime + "至" + endTime + "</td></tr>") +
                ("<tr><th>招标人：</th><td>" + zbrAndLht + "</td></tr>") +
                ("<tr><th>招标代理机构：</th><td>" + zbdlJG + "</td></tr>") +
                ("<tr><th>招标方式：</th><td>" + zbFangShi + "</td></tr>") +
                ("<tr><th>中标人：</th><td>" + bidUnit + "</td></tr>") +
                ("<tr><th>中标价：</th><td>" + bidMoney + "</td></tr>") +
                ("<tr><th>中标工期：</th><td>" + zhongBiaoGQ + "</td></tr>") +
                ("<tr><th>项目经理：</th><td>" + prjMgr + "</td></tr>") +
                ("<tr><th>资格等级：</th><td>" + ziGeDengJi + "</td></tr>") +
                ("<tr><th>资格证书：</th><td>" + ziGeZhengShu + "</td></tr>") +
                ("</table>");
                bidCtx = HtmlTxt.Replace("</td>", "\r\n").ToCtxString();
                try
                {
                    if (Convert.ToDecimal(bidMoney) > 100000)
                        bidMoney = (decimal.Parse(bidMoney) / 1000000).ToString();
                }
                catch { }

                msgType = "鄂州市公共资源交易中心";
                specType = "政府采购";
                bidType = prjName.GetInviteBidType();
                buildUnit = buildUnit.Replace(" ", "");
                BidInfo info = ToolDb.GenBidInfo("湖北省", "湖北省及地市", "鄂州市", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                list.Add(info);
                count++;
                Parser parser = new Parser(new Lexer(HtmlTxt));
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
                                link = "http://www.ezztb.gov.cn/" + a.Link;
                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                            base.AttachList.Add(attach);
                        }
                    }
                }
                if (count >= 50)
                {
                    Thread.Sleep(1000 * 60 * 5);
                    count = 0;
                }
                if (!crawlAll && list.Count >= this.MaxCount) return list;

            }
            return list;
        }
    }
}
