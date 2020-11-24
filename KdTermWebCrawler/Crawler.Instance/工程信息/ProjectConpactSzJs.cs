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
using System.IO;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Threading;

namespace Crawler.Instance
{
    public class ProjectConpactSzJs : WebSiteCrawller
    {
        public ProjectConpactSzJs()
            : base()
        {
            this.Group = "工程信息";
            this.PlanTime = "12:00,03:20";
            this.Title = "深圳市住房与建设局合同备案基本信息（2014新版）";
            this.MaxCount = 100000;
            this.Description = "自动抓取深圳市住房与建设局合同备案基本信息（2014新版）";
            this.ExistCompareFields = "PrjName,ContUnit,CompactType";
            this.SiteUrl = "http://htjg.szjs.gov.cn/web/contractlist.jsp";
            this.ExistsUpdate = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string newUrl = "http://htjg.szjs.gov.cn/web/webService/getContractList.json";
            IList list = new List<ProjectConpact>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1, count = 1;
            string eventValidation = string.Empty;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = null;


            try
            {
                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "pageNumber", "limit" },
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
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "paageNumber", "limit" },
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
                        string pProvince = string.Empty, pUrl = string.Empty,
                         pCity = string.Empty, pSubcontractCode = string.Empty,
                         pSubcontractName = string.Empty, pSubcontractCompany = string.Empty,
                         pInfoSource = string.Empty, pRecordDate = string.Empty, pCompactPrice = string.Empty,
                         pCompactType = string.Empty, pBuildUnit = string.Empty, pPrjCode = string.Empty,
                         PrjName = string.Empty, pPrjMgrQual = string.Empty, pPrjMgrName = string.Empty,
                         pContUnit = string.Empty, pCreatetime = string.Empty;


                        Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;

                        string id = Convert.ToString(dic["id"]);
                        PrjName = Convert.ToString(dic["itemname"]);
                        pBuildUnit = Convert.ToString(dic["const_org"]);
                        pContUnit = Convert.ToString(dic["corp_name"]);
                        pCompactType = Convert.ToString(dic["pact_type"]);
                        pRecordDate = Convert.ToString(dic["status_time"]);

                        pUrl = "http://htjg.szjs.gov.cn/web/contractdetail.jsp?id=" + id;
                        string dtlUrl = "http://htjg.szjs.gov.cn/web/webService/getContractById.json";
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

                        pInfoSource = string.Format("合同类型：{0}\r\n发包方式：{1}\r\n工程项目编码：{2}\r\n工程项目名称：{3}\r\n 工程标段名称：{4}\r\n建设单位：{5}\r\n分包工程编号：{6}\r\n分包工程名称：{7}\r\n发包单位：{8}\r\n承包单位：{9}\r\n合同价：{10}\r\n备案日期：{11}\r\n",
                                                           Convert.ToString(dtlDic["pact_type"]),
                                                           Convert.ToString(dtlDic["appl_method"]),
                                                           Convert.ToString(dtlDic["itemcode"]),
                                                           PrjName,
                                                           Convert.ToString(dtlDic["prj_name"]),
                                                           pBuildUnit,
                                                           Convert.ToString(dtlDic["fb_prj_id"]),
                                                           Convert.ToString(dtlDic["fb_prj_name"]),
                                                           Convert.ToString(dtlDic["fbr_org"]),
                                                           pContUnit,
                                                           Convert.ToString(dtlDic["contract_price"]),
                                                           pRecordDate);
                        pCompactType = pInfoSource.GetRegex("合同类型");
                        pSubcontractCompany = pInfoSource.GetRegex("发包方式");
                        pPrjCode = pInfoSource.GetRegex("工程项目编码");
                        pCreatetime = pInfoSource.GetRegex("工程标段名称");
                        pSubcontractCode = pInfoSource.GetRegex("分包工程编号");
                        pSubcontractName = pInfoSource.GetRegex("分包工程名称");
                        pPrjMgrQual = pInfoSource.GetRegex("发包单位");
                        pCompactPrice = pInfoSource.GetRegex("合同价");

                        ProjectConpact info = ToolDb.GenProjectConpact("广东省", pUrl, "深圳市区", pSubcontractCode, pSubcontractName, pSubcontractCompany,
                            pInfoSource, pRecordDate, pCompactPrice, pCompactType, pBuildUnit, pPrjCode, PrjName, pPrjMgrQual,
                            pPrjMgrName, pContUnit, pCreatetime, "深圳市住房和建设局");
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount)
                            return list;

                        count++;
                        if (count >= 200)
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
