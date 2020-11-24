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
using System.Threading;

namespace Crawler.Instance
{
    public class InviteWuHanJsgc : WebSiteCrawller
    {
        public InviteWuHanJsgc()
            : base()
        {
            this.Group = "招标信息";
            this.PlanTime = "12:06,03:25";
            this.Title = "武汉市建设工程交易中心招标信息";
            this.Description = "自动抓取武汉市建设工程交易中心招标信息";
            this.SiteUrl = "http://www.jy.whzbtb.com/V2PRTS/TendererNoticeInfoListInit.do";
            this.MaxCount = 500;//
            this.ExistsUpdate = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string newUrl = "http://www.jy.whzbtb.com/V2PRTS/TendererNoticeInfoList.do";
            IList list = new List<InviteInfo>();
            int pageInt = 1, count=1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = null;
            try
            {
                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "page", "rows" },
                   new string[] { "1", this.MaxCount.ToString() });
                html = this.ToolWebSite.GetHtmlByUrl(newUrl, nvc);

                smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);

            }
            catch
            {
                return list;
            }
            foreach (KeyValuePair<string, object> obj in smsTypeJson)
            {
                if (obj.Key == "total" || obj.Key.Equals("pageSize") || obj.Key.Equals("pageNumber")) continue;
                object[] array = (object[])obj.Value;
                foreach (object arrValue in array)
                {
                    string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                     prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                     specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                     remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                     CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;

                    string id = Convert.ToString(dic["id"]);
                    prjName = Convert.ToString(dic["tenderPrjName"]);
                    code = Convert.ToString(dic["registrationId"]);
                    buildUnit = Convert.ToString(dic["prjbuildCorpName"]);
                    beginDate = Convert.ToString(dic["noticeStartDate"]);
                    endDate = Convert.ToString(dic["noticeEndDate"]);
                    prjAddress = Convert.ToString(dic["prjAddress"]);

                    InfoUrl = "http://www.jy.whzbtb.com/V2PRTS/TendererNoticeInfoDetail.do?id=" + id;
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

                        string strHtml1 = string.Empty, strHtml2 = string.Empty, strHtml3 = string.Empty;
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "registrationId" },
                            new string[] { code });

                        string url3 = "http://www.jy.whzbtb.com/V2PRTS/TenderPrjDetailList.do";

                        try
                        {
                            strHtml1 = this.ToolWebSite.GetHtmlByUrl(url3, nvc);

                            serializer = new JavaScriptSerializer();
                            Dictionary<string, object> dic1 = (Dictionary<string, object>)serializer.DeserializeObject(strHtml1);
                            object[] dtlObj = (object[])dic1["rows"];
                            StringBuilder sb = new StringBuilder();
                            sb.Append("<div class='panel-title panel-with-icon'>项目标段详情</div>");
                            sb.Append(@"<table id='reserve_data' cellspacing='0' cellpadding='0' >");
                            sb.Append("<tr>");
                            sb.Append("<td style='width:5%'></td>");
                            sb.Append("<td style='width:35%'>标段名称</td>");
                            sb.Append("<td style='width:20%'>标段号</td>");
                            sb.Append("<td style='width:40%'>标段描述</td>");
                            sb.Append("</tr>");
                            for (int j = 0; j < dtlObj.Length; j++)
                            {
                                Dictionary<string, object> dtlDic = dtlObj[j] as Dictionary<string, object>;
                                sb.Append("<tr>");
                                sb.AppendFormat("<td>{0}</td>", (j + 1).ToString());
                                sb.AppendFormat("<td>{0}</td>", dtlDic["prjName"]);
                                sb.AppendFormat("<td>{0}</td>", dtlDic["section"]);
                                sb.AppendFormat("<td>{0}</td>", dtlDic["description"]);
                                sb.Append("</tr>");
                            }

                            sb.Append("</table>");
                            HtmlTxt += sb.ToString();
                        }
                        catch { }

                        string url2 = "http://www.jy.whzbtb.com/V2PRTS/TendererProjectManagerList.do";
                        try
                        {
                            strHtml1 = this.ToolWebSite.GetHtmlByUrl(url2, nvc);

                            serializer = new JavaScriptSerializer();
                            Dictionary<string, object> dic1 = (Dictionary<string, object>)serializer.DeserializeObject(strHtml1);
                            object[] dtlObj = (object[])dic1["rows"];
                            StringBuilder sb = new StringBuilder();
                            sb.Append("<div class='panel-title panel-with-icon'>招标对项目经理要求</div>");
                            sb.Append(@"<table id='reserve_data' cellspacing='0' cellpadding='0' >");
                            sb.Append("<tr>");
                            sb.Append("<td style='width:5%'></td>");
                            sb.Append("<td style='width:20%'>注册类型</td>");
                            sb.Append("<td style='width:20%'>注册等级</td>");
                            sb.Append("<td style='width:25%'>注册专业</td>");
                            sb.Append("<td style='width:30%'>要求说明</td>");
                            sb.Append("</tr>");
                            for (int j = 0; j < dtlObj.Length; j++)
                            {
                                Dictionary<string, object> dtlDic = dtlObj[j] as Dictionary<string, object>;
                                sb.Append("<tr>");
                                sb.AppendFormat("<td>{0}</td>", (j + 1).ToString());
                                sb.AppendFormat("<td>{0}</td>", dtlDic["tradeRequirementsName"]);
                                sb.AppendFormat("<td>{0}</td>", dtlDic["tradeLevelName"]);
                                sb.AppendFormat("<td>{0}</td>", dtlDic["regTradeTypeName"]);
                                sb.AppendFormat("<td>{0}</td>", dtlDic["requirements"]);
                                sb.Append("</tr>");
                            }

                            sb.Append("</table>");
                            HtmlTxt += sb.ToString();
                        }
                        catch { }

                        string url1 = "http://www.jy.whzbtb.com/V2PRTS/TendererRequirementList.do";
                        try
                        {
                            strHtml1 = this.ToolWebSite.GetHtmlByUrl(url1, nvc);

                            serializer = new JavaScriptSerializer();
                            Dictionary<string, object> dic1 = (Dictionary<string, object>)serializer.DeserializeObject(strHtml1);
                            object[] dtlObj = (object[])dic1["rows"];
                            StringBuilder sb = new StringBuilder();
                            sb.Append("<div class='panel-title panel-with-icon'>招标对投标人要求</div>");
                            sb.Append(@"<table id='reserve_data' cellspacing='0' cellpadding='0' >");
                            sb.Append("<tr>");
                            sb.Append("<td style='width:5%'></td>");
                            sb.Append("<td style='width:15%'>资质类型</td>");
                            sb.Append("<td style='width:20%'>资质分类</td>");
                            sb.Append("<td style='width:25%'>资质专业</td>");
                            sb.Append("<td style='width:15%'>资质等级（含及以上）</td>");
                            sb.Append("<td style='width:20%'>备注</td>");
                            sb.Append("</tr>");
                            for (int j = 0; j < dtlObj.Length; j++)
                            {
                                Dictionary<string, object> dtlDic = dtlObj[j] as Dictionary<string, object>;
                                sb.Append("<tr>");
                                sb.AppendFormat("<td>{0}</td>", (j + 1).ToString());
                                sb.AppendFormat("<td>{0}</td>", dtlDic["certTypeNumName"]);
                                sb.AppendFormat("<td>{0}</td>", dtlDic["tradeLargeClassName"]);
                                sb.AppendFormat("<td>{0}</td>", dtlDic["tradeCategoryCodeName"]);
                                sb.AppendFormat("<td>{0}</td>", dtlDic["certTypeLevelName"]);
                                sb.AppendFormat("<td>{0}</td>", dtlDic["description"]);
                                sb.Append("</tr>");
                            } 

                            sb.Append("</table>");
                            HtmlTxt += sb.ToString();
                        }
                        catch { }

                      
                        
                        inviteCtx = HtmlTxt.ToCtxString();
                        inviteType = prjName.GetInviteBidType();
                        msgType = "武汉市建设工程交易中心";
                        specType = "建设工程";
                        InviteInfo info = ToolDb.GenInviteInfo("湖北省", "湖北省及地市", "武汉市", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
            return list;
        }
    }
}
