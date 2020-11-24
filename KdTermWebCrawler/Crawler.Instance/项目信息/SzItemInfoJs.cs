using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace Crawler.Instance
{
    public class SzItemInfoJs : WebSiteCrawller
    {
        public SzItemInfoJs()
            : base()
        {
            this.Group = "项目信息";
            this.PlanTime = "12:10,03:15";
            this.Title = "深圳市住房和建设局项目信息(2014新版)";
            this.MaxCount = 10000;
            this.Description = "自动抓取深圳市住房和建设局项目信息(2014新版)";
            this.ExistCompareFields = "URL,ItemName";
            this.SiteUrl = "http://projreg.szjs.gov.cn/web/project_list.jsp";
            this.ExistsUpdate = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {

            string newUrl = "http://projreg.szjs.gov.cn/web/webService/getProjectList.json";
            IList list = new List<ItemInfo>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int sqlCount = 1, count = 1;
            string eventValidation = string.Empty;
            try
            {
                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "page", "rows" },
                    new string[] { "1", this.MaxCount.ToString() });
                htl = this.ToolWebSite.GetHtmlByUrl(newUrl, nvc);
            }
            catch (Exception ex)
            {
                return list;
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(htl);
            foreach (KeyValuePair<string, object> obj in smsTypeJson)
            {
                if (obj.Key == "total") continue;
                object[] array = (object[])obj.Value;
                foreach (object arrValue in array)
                {
                    string itemCode = string.Empty, itemName = string.Empty, buildUnit = string.Empty, address = string.Empty,
                         investMent = string.Empty, buildKind = string.Empty, investKink = string.Empty, linkMan = string.Empty,
                         linkmanTel = string.Empty, itemDesc = string.Empty, apprNo = string.Empty, apprDate = string.Empty,
                         apprUnit = string.Empty, apprResult = string.Empty, landapprNo = string.Empty, landplanNo = string.Empty, buildDate = string.Empty, infoSource = string.Empty, url = string.Empty, textCode = string.Empty, licCode = string.Empty, msgType = string.Empty;
                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;

                    itemCode = Convert.ToString(dic["pronumber"]);
                    itemName = Convert.ToString(dic["proname"]);
                    buildUnit = Convert.ToString(dic["constructionunit"]);
                    buildDate = Convert.ToString(dic["registertime"]);

                    string id = dic["id"].ToString();

                    url = "http://projreg.szjs.gov.cn/web/project.jsp?id=" + id;

                    string dtlUrl = "http://projreg.szjs.gov.cn/web/webService/getProjectInfo.json";
                    string htmldtl = string.Empty;
                    try
                    {
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "id" },
                            new string[] { id });
                        htmldtl = this.ToolWebSite.GetHtmlByUrl(dtlUrl, nvc);
                    }
                    catch { continue; }

                    JavaScriptSerializer serializerNew = new JavaScriptSerializer();
                    Dictionary<string, object> dtlDic = (Dictionary<string, object>)serializer.DeserializeObject(htmldtl);

                    textCode = Convert.ToString(dtlDic["plannumber"]);
                    address = Convert.ToString(dtlDic["projaddress"]);
                    investMent = Convert.ToString(dtlDic["totalinvestment"]);
                    licCode = Convert.ToString(dtlDic["landplanningpermission"]);

                    infoSource = string.Format("项目编号：{0}\r\n项目名称：{1}\r\n报建地址：{2}\r\n建设单位：{3}\r\n计划总投资：{4}\r\n计划立项文号：{5}\r\n规划许可号：{6}\r\n报建时间：{7}", itemCode, itemName, address, buildUnit, investMent, textCode, licCode, buildDate);

                    string ctxHtml = string.Format("项目编号：{0}<br/>项目名称：{1}<br/>报建地址：{2}<br/>建设单位：{3}<br/>计划总投资：{4}<br/>计划立项文号：{5}<br/>规划许可号：{6}<br/>报建时间：{7}", itemCode, itemName, address, buildUnit, investMent, textCode, licCode, buildDate);



                    msgType = "深圳市住房和建设局";
                    ItemInfo info = ToolDb.GenItemInfo(itemCode, itemName, buildUnit, address, investMent, buildKind, investKink, linkMan, linkmanTel, itemDesc, apprNo, apprDate, apprUnit, apprResult, landapprNo, landplanNo, buildDate, "广东省", "深圳市区", infoSource, url, textCode, licCode, msgType, ctxHtml);

                    sqlCount++;
                    if (!crawlAll && sqlCount >= this.MaxCount) return list;

                    count++;
                    if (count >= 100)
                    {
                        count = 1;
                        Thread.Sleep(600 * 1000);
                    }

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
            return list;
        }
    }
}
