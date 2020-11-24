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
using System.Data;
using System.Net;

namespace Crawler.Instance
{
    public class NoticeSzJyzxZsjyj : WebSiteCrawller
    {
        public NoticeSzJyzxZsjyj()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市交易中心资审及业绩(2015版)";
            this.PlanTime = "9:27,11:27,14:17,17:27";
            this.Description = "自动抓取广东省深圳市交易中心资审及业绩(2015版)";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw/queryYeJiList.do?page=1&rows=";
            this.MaxCount = 500;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + (MaxCount + 20));
            }
            catch { return null; }
            int startIndex = html.IndexOf("{");
            int endIndex = html.LastIndexOf("}");
            html = html.Substring(startIndex, (endIndex + 1) - startIndex);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
            foreach (KeyValuePair<string, object> obj in smsTypeJson)
            {
                if (obj.Key == "total") continue;
                object[] array = (object[])obj.Value;
                foreach (object arrValue in array)
                {
                    string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty, prjType = string.Empty, bgType = string.Empty;
                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    prjCode = Convert.ToString(dic["bdBH"]);
                    InfoTitle = Convert.ToString(dic["bdName"]);
                    prjType = Convert.ToString(dic["gcLeiXing2"]);
                    PublistTime = Convert.ToString(dic["faBuTime"]);
                    InfoType = "资审及业绩公示";
                    InfoUrl = Convert.ToString(dic["detailUrl"]);
                    try
                    {
                        htmlTxt = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString().GetReplace("\\t,\\r,\\n,\",{html:,}");
                    }
                    catch { }

                    string dtlUrl = "https://www.szjsjy.com.cn:8001/jyw/queryYeJiById.do", fileUrl = "https://www.szjsjy.com.cn:8001/jyw/queryYeJiFuJianByGGBDGuid.do";

                    string dtlJson = string.Empty, fileJson = string.Empty;
                    try
                    {
                        NameValueCollection dtlNvc = this.ToolWebSite.GetNameValueCollection(new string[] { "guid" }, new string[] { Convert.ToString(dic["pbYeJiGongShiGuid"]) });

                        dtlJson = this.ToolWebSite.GetHtmlByUrl(dtlUrl, dtlNvc); 
                    }
                    catch { }
                    List<BaseAttach> listAttach = new List<BaseAttach>();
                    StringBuilder dtlSb = new StringBuilder();

                    try
                    {
                        Dictionary<string, object> fileSmsJson = null;
                        JavaScriptSerializer dtlSerializer = new JavaScriptSerializer();
                        Dictionary<string, object> dtlSmsJson = (Dictionary<string, object>)dtlSerializer.DeserializeObject(dtlJson);
                        Dictionary<string, object> yeJiGongShi = null; //dtlSmsJson["yeJiGongShi"] as Dictionary<string, object>;
                        // Dictionary<string, object> zhaoBiaoFangShi = yeJiGongShi["zhaoBiaoFangShi"] as Dictionary<string, object>;
                        Dictionary<string, object> ggBd = null; //yeJiGongShi["ggBd"] as Dictionary<string, object>;
                        Dictionary<string, object> bd = null;// ggBd["bd"] as Dictionary<string, object>;
                        Dictionary<string, object> gc = null; //bd["gc"] as Dictionary<string, object>;
                        Dictionary<string, object> xm = null; //bd["xm"] as Dictionary<string, object>;

                        try
                        {
                            yeJiGongShi =  dtlSmsJson["yeJiGongShi"] as Dictionary<string, object>; 
                            ggBd = yeJiGongShi["ggBd"] as Dictionary<string, object>;
                            bd =  ggBd["bd"] as Dictionary<string, object>;
                            gc =  bd["gc"] as Dictionary<string, object>;
                            xm =  bd["xm"] as Dictionary<string, object>;
                        }
                        catch { }

                        if (yeJiGongShi != null)
                        {
                            dtlSb.Append("<table>");
                            try
                            {
                                string xm_BH = Convert.ToString(xm["xm_BH"]);
                                string xm_Name = Convert.ToString(xm["xm_Name"]);
                                dtlSb.Append("<tr>");
                                dtlSb.Append("<td>项目编号：</td>");
                                dtlSb.Append("<td>" + xm_BH + "</td>");
                                dtlSb.Append("<td>项目名称：</td>");
                                dtlSb.Append("<td>" + xm_Name + "</td>");
                                dtlSb.Append("</tr>");
                            }
                            catch { }

                            dtlSb.Append("<tr>");
                            dtlSb.Append("<td>招标项目编号：</td>");
                            dtlSb.Append("<td>" + Convert.ToString(gc["gcBH"]) + "</td>");
                            dtlSb.Append("<td>招标项目名称：</td>");
                            dtlSb.Append("<td>" + Convert.ToString(gc["gcName"]) + "</td>");
                            dtlSb.Append("</tr>");

                            dtlSb.Append("<tr>");
                            dtlSb.Append("<td>标段编号：</td>");
                            dtlSb.Append("<td>" + Convert.ToString(bd["bdBH"]) + "</td>");
                            dtlSb.Append("<td>标段名称：</td>");
                            dtlSb.Append("<td>" + Convert.ToString(bd["bdName"]) + "</td>");
                            dtlSb.Append("</tr>");

                            dtlSb.Append("<tr>");
                            dtlSb.Append("<td>招标方式：</td>");
                            dtlSb.Append("<td>" + Convert.ToString(yeJiGongShi["zhaoBiaoFangShi"]) + "</td>");
                            dtlSb.Append("<td>工程类型：</td>");
                            dtlSb.Append("<td>" + Convert.ToString(yeJiGongShi["gcLeiXing2"]) + "</td>");
                            dtlSb.Append("</tr>");

                            dtlSb.Append("<tr>");
                            dtlSb.Append("<td>建设单位：</td>");
                            dtlSb.Append("<td>" + Convert.ToString(gc["zbRName"]) + "</td>");
                            dtlSb.Append("<td></td>");
                            dtlSb.Append("<td></td>");
                            dtlSb.Append("</tr>");

                            string startDate = Convert.ToString(yeJiGongShi["gongShiStartDate"]);
                            if (!string.IsNullOrWhiteSpace(startDate))
                            {
                                startDate = ToolHtml.GetDateTimeByLong(long.Parse(startDate)).ToString();
                            }
                            string endDate = Convert.ToString(yeJiGongShi["gongShiEndDate"]);
                            if (!string.IsNullOrWhiteSpace(endDate))
                            {
                                endDate = ToolHtml.GetDateTimeByLong(long.Parse(endDate)).ToString();
                            }
                            dtlSb.Append("<tr>");
                            dtlSb.Append("<td>发布开始时间：</td>");
                            dtlSb.Append("<td>" + startDate + "</td>");
                            dtlSb.Append("<td>发布截止时间：</td>");
                            dtlSb.Append("<td>" + endDate + "</td>");
                            dtlSb.Append("</tr>");

                            string gongShiLaiYuan = Convert.ToString(yeJiGongShi["gongShiLaiYuan"]);
                            string gongShiLaiYuanResult = gongShiLaiYuan == "1" ? "报名环节" : gongShiLaiYuan == "2" ? "资格审查环节" : gongShiLaiYuan == "3" ? "评标环节" : "其他";
                            dtlSb.Append("<tr>");
                            dtlSb.Append("<td>公示环节：</td>");
                            dtlSb.Append("<td>" + gongShiLaiYuanResult + "</td>");
                            dtlSb.Append("<td></td>");
                            dtlSb.Append("<td></td>");
                            dtlSb.Append("</tr>");

                            dtlSb.Append("</table>");
                            string ggBdGuid = Convert.ToString(yeJiGongShi["ggBdGuid"]), pbYeJiGongShiGuid = Convert.ToString(yeJiGongShi["pbYeJiGongShiGuid"]);

                            try
                            {
                                NameValueCollection fileNvc = this.ToolWebSite.GetNameValueCollection(new string[] { "ggBDGuid", "pbYeJiGongShiGuid" }, new string[] { ggBdGuid, pbYeJiGongShiGuid });
                                fileJson = this.ToolWebSite.GetHtmlByUrl(fileUrl, fileNvc);
                            }
                            catch { }
                            JavaScriptSerializer fileSerializer = new JavaScriptSerializer();
                            fileSmsJson = (Dictionary<string, object>)fileSerializer.DeserializeObject(fileJson);

                        }
                        else
                        {
                            try
                            {
                                Uri uri = new Uri(InfoUrl);
                                NameValueCollection param = ToolHtml.GetQueryString(uri.Query);
                                string id = param["id"];
                                string type = param["type"];
                                NameValueCollection fileNvc = this.ToolWebSite.GetNameValueCollection(new string[] { "id", "type" }, new string[] { id, type });
                                fileJson = this.ToolWebSite.GetHtmlByUrl("https://www.szjsjy.com.cn:8001/jyw/queryOldDataDetail.do", fileNvc);
                            }
                            catch { }
                            JavaScriptSerializer fileSerializer = new JavaScriptSerializer();
                            fileSmsJson = (Dictionary<string, object>)fileSerializer.DeserializeObject(fileJson);
                        }


                        foreach (KeyValuePair<string, object> fileObj in fileSmsJson)
                        {
                            if (fileObj.Key == "list")
                            {
                                object[] fileArray = (object[])fileObj.Value;
                                int rowIndex = 1;
                                foreach (object fileValue in fileArray)
                                {
                                    Dictionary<string, object> fileDic = (Dictionary<string, object>)fileValue;
                                    dtlSb.Append("<table>");
                                    dtlSb.Append("<tr>");
                                    dtlSb.Append("<th>序号</th>");
                                    dtlSb.Append("<th>单位名称</th>");
                                    dtlSb.Append("<th>资格文件</th>");
                                    dtlSb.Append("<th>业绩文件</th>");
                                    dtlSb.Append("<th>审查结果</th>");
                                    dtlSb.Append("<th>不合格原因</th>");
                                    dtlSb.Append("</tr>");

                                    dtlSb.Append("<tr>");
                                    dtlSb.Append("<td>" + rowIndex + "</td>");
                                    dtlSb.Append("<td>" + fileDic["DWMC"] + "</td>");
                                    dtlSb.Append("<td>" + fileDic["ZGWJ"] + "</td>");
                                    dtlSb.Append("<td>" + fileDic["YJWJ"] + "</td>");
                                    dtlSb.Append("<td>" + fileDic["IsHeGe"] + "</td>");
                                    dtlSb.Append("<td>" + fileDic["BuHeGeYuanYin"] + "</td>");
                                    dtlSb.Append("</tr>");

                                    BaseAttach item = ToolDb.GenBaseAttach(Convert.ToString(fileDic["ZGWJ"]), "", Convert.ToString(fileDic["ZGWJURL"]));
                                    listAttach.Add(item);
                                    rowIndex++;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        Logger.Error(InfoTitle);
                    }

                    NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳市工程", string.Empty, string.Empty, InfoTitle, InfoType, dtlSb.ToString().GetReplace("</tr>","\r\n").ToCtxString(), PublistTime, string.Empty, MsgTypeCosnt.ShenZhenMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, prjType, bgType, dtlSb.ToString());
                    list.Add(info);
                    foreach (BaseAttach item in listAttach)
                    {
                        item.SourceID = info.Id;
                        base.AttachList.Add(item);
                    }
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }

            return list;
        }
    }
}
