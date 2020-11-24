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
    public class ProjectInfoSzJs : WebSiteCrawller
    {
        public ProjectInfoSzJs()
            : base()
        {
            this.Group = "工程信息";
            this.PlanTime = "12:06,03:25";
            this.Title = "深圳市住房与建设局工程基本信息（2014新版）";
            this.Description = "自动抓取深圳市住房与建设局工程基本信息（2014新版）";
            this.ExistCompareFields = "PrjCode";
            this.SiteUrl = "http://www.szjs.gov.cn/ztfw/gcjs/xmxx/project/"; 
            this.MaxCount = 500;
            this.ExistsUpdate = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string newUrl = "http://projreg.szjs.gov.cn/web/webService/getEngList.json";
            IList list = new List<BaseProject>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int count = 1, pageInt = 1;
            string eventValidation = string.Empty;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = null;
            try
            {
                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "page", "rows" },
                   new string[] { "1", "500" });

                htl = this.ToolWebSite.GetHtmlByUrl(newUrl, nvc);

                smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(htl);

                int totalCount = Convert.ToInt32(smsTypeJson["total"]);

                pageInt = totalCount / 500 + 1;
            }
            catch (Exception ex)
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
                          new string[] { i.ToString(), "500" });

                        htl = this.ToolWebSite.GetHtmlByUrl(newUrl, nvc);

                        serializer = new JavaScriptSerializer();
                        smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(htl);
                        
                    }
                    catch { continue; }
                }
                foreach (KeyValuePair<string, object> obj in smsTypeJson)
                {
                    if (obj.Key == "total") continue;
                    object[] array = (object[])obj.Value;
                    foreach (object arrValue in array)
                    {
                        string pUrl = string.Empty, pInfoSource = string.Empty,
                           pBeginDate = string.Empty, pBuilTime = string.Empty,
                           pEndDate = string.Empty, pConstUnit = string.Empty,
                           pSuperUnit = string.Empty, pDesignUnit = string.Empty,
                           pProspUnit = string.Empty, pInviteArea = string.Empty,
                           pBuildArea = string.Empty, pPrjClass = string.Empty,
                           pProClassLevel = string.Empty, pChargeDept = string.Empty,
                           pPrjAddress = string.Empty, pBuildUnit = string.Empty,
                           pPrjCode = string.Empty, PrjName = string.Empty, pCreatetime = string.Empty;

                        Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;

                        string id = Convert.ToString(dic["id"]);
                        pPrjCode = Convert.ToString(dic["engnumber"]);
                        PrjName = Convert.ToString(dic["engname"]);
                        pBuildUnit = Convert.ToString(dic["constructionunit"]);
                        pBuilTime = Convert.ToString(dic["registertime"]);



                        pUrl = "http://projreg.szjs.gov.cn/web/eng.jsp?id=" + pPrjCode;
                        string dtlUrl = "http://projreg.szjs.gov.cn/web/webService/getEngInfo.json";
                        string htmldtl = string.Empty;
                        try
                        {
                            NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "id" },
                               new string[] { id });
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(dtlUrl, nvc);
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }

                        JavaScriptSerializer serializerNew = new JavaScriptSerializer();
                        Dictionary<string, object> dtlDic = (Dictionary<string, object>)serializer.DeserializeObject(htmldtl);

                        pInfoSource = string.Format("工程编号：{0}\r\n工程名称：{1}\r\n工程地址：{2}\r\n主管部门：{3}\r\n工程类别等级：{4}\r\n工程类别：{5}\r\n建筑总面积：{6}\r\n红线面积：{7}\r\n勘察单位：{8}\r\n设计单位：{9}\r\n监理单位：{10}\r\n合同开工日期：{11}\r\n合同竣工日期：{12}",
                                                               pPrjCode,
                                                               PrjName,
                                                               Convert.ToString(dtlDic["engaddress"]),
                                                               Convert.ToString(dtlDic["prodepartment"]),
                                                               Convert.ToString(dtlDic["englevel"]),
                                                               Convert.ToString(dtlDic["professiontype"]),
                                                               Convert.ToString(dtlDic["grossarea"]),
                                                               Convert.ToString(dtlDic["redlinearea"]),
                                                               Convert.ToString(dtlDic["survey"]),
                                                               Convert.ToString(dtlDic["design"]),
                                                               Convert.ToString(dtlDic["supervision"]),
                                                               Convert.ToString(dtlDic["starttime"]),
                                                               Convert.ToString(dtlDic["finishtime"]));

                        pPrjAddress = pInfoSource.GetRegex("工程地址");
                        pChargeDept = pInfoSource.GetRegex("主管部门");
                        pProClassLevel = pInfoSource.GetRegex("工程类别等级");
                        pPrjClass = pInfoSource.GetRegex("工程类别");
                        pInviteArea = pInfoSource.GetRegex("红线面积");
                        pBuildArea = pInfoSource.GetRegex("建筑总面积");
                        pProspUnit = pInfoSource.GetRegex("勘察单位");
                        pDesignUnit = pInfoSource.GetRegex("设计单位");
                        pSuperUnit = pInfoSource.GetRegex("监理单位");
                        pBeginDate = pInfoSource.GetRegex("合同开工日期");
                        pEndDate = pInfoSource.GetRegex("合同竣工日期");

                        BaseProject info = ToolDb.GenBaseProject("广东省", pUrl, "深圳市区", pInfoSource, pBuilTime, pBeginDate, pEndDate, pConstUnit, pSuperUnit, pDesignUnit, pProspUnit, pInviteArea,
                            pBuildArea, pPrjClass, pProClassLevel, pChargeDept, pPrjAddress, pBuildUnit, pPrjCode, PrjName, pCreatetime, "深圳市住房和建设局");

                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount)
                            return list;

                        count++;
                        if (count >= 100)
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
