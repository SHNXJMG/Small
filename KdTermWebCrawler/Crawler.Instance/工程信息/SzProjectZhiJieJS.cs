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

namespace Crawler.Instance
{
    public class SzProjectZhiJieJS:WebSiteCrawller
    {
        public SzProjectZhiJieJS()
            : base()
        {
            this.Group = "工程信息";
            this.PlanTime = "12:15,03:35";
            this.Title = "深圳市住房与建设局工程基本信息（新版直接发包）";
            this.Description = "自动抓取深圳市住房与建设局工程基本信息（新版）";
            this.ExistCompareFields = "PrjCode,Url";
            this.SiteUrl = "http://www.szjs.gov.cn/build/build.ashx?_=1356597320333&menu=%E9%A1%B9%E7%9B%AE%E4%BF%A1%E6%81%AF&type=%E7%9B%B4%E6%8E%A5%E5%8F%91%E5%8C%85&pageSize=20&pageIndex=1&prjName=&timp=333";
            this.MaxCount = 100;
            this.ExistsUpdate = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            if (htl.Contains("RowCount"))
            {
                try
                {
                    int index = htl.IndexOf("RowCount");
                    string pageStr = htl.Substring(index, htl.Length - index).Replace("RowCount", "").Replace("}", "").Replace(":", "").Replace("\"", "");
                    decimal b = decimal.Parse(pageStr) / 20;
                    if (b.ToString().Contains("."))
                    {
                        page = Convert.ToInt32(b) + 1;
                    }
                    else { page = Convert.ToInt32(b); }
                }
                catch { }
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl("http://www.szjs.gov.cn/build/build.ashx?_=1356597320333&menu=%E9%A1%B9%E7%9B%AE%E4%BF%A1%E6%81%AF&type=%E7%9B%B4%E6%8E%A5%E5%8F%91%E5%8C%85&pageSize=20&pageIndex=" + i.ToString() + "&prjName=&timp=333", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(htl);
                foreach (KeyValuePair<string, object> obj in smsTypeJson)
                {
                    if (obj.Key != "DataList")
                    {
                        continue;
                    }
                    object[] array = (object[])obj.Value;
                    foreach (object obj2 in array)
                    { 
                          Dictionary<string, object> dicSmsType = (Dictionary<string, object>)obj2;
                        string pUrl = string.Empty, pInfoSource = string.Empty, 
                            pBeginDate = string.Empty, pBuilTime = string.Empty,
                            pEndDate = string.Empty, pConstUnit = string.Empty,
                            pSuperUnit = string.Empty, pDesignUnit = string.Empty,
                            pProspUnit = string.Empty, pInviteArea = string.Empty, 
                            pBuildArea = string.Empty, pPrjClass = string.Empty,
                            pProClassLevel = string.Empty, pChargeDept = string.Empty, 
                            pPrjAddress = string.Empty, pBuildUnit = string.Empty, 
                            pPrjCode = string.Empty, PrjName = string.Empty, pCreatetime = string.Empty;
                        try
                        {
                        
                            pPrjCode = Convert.ToString(dicSmsType["PrjId"]);
                            PrjName = Convert.ToString(dicSmsType["PrjName"]);
                            pBuildUnit = Convert.ToString(dicSmsType["ConstOrg"]);
                            pBuilTime = Convert.ToString(dicSmsType["Bjrq"]);
                            pUrl = "http://www.szjs.gov.cn/build/gcxm_detail.aspx?id=" + pPrjCode;
                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(pUrl, Encoding.UTF8, ref cookiestr).Trim();
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            Parser parser = new Parser(new Lexer(htmldetail));
                            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "js-table mar-l-4")));
                            if (nodeList != null && nodeList.Count > 0)
                            {
                                TableTag table = nodeList[0] as TableTag;
                                for (int j = 0; j < table.RowCount; j++)
                                {
                                    TableRow dr = table.Rows[j];
                                    string ctx = string.Empty;
                                    for (int k = 0; k < dr.ColumnCount; k++)
                                    {
                                        ctx += dr.Columns[k].ToPlainTextString().Trim().Replace(" ", "").Replace("\r", "").Replace("\n", "");
                                    }
                                    pInfoSource += ctx + "\r\n";
                                }
                                Regex regPrjAddr = new Regex(@"(工程地点|工程地址)(：|:)[^\r\n]+\r\n");
                                pPrjAddress = regPrjAddr.Match(pInfoSource).Value.Replace("工程地址", "").Replace("工程地点", "").Replace(":", "").Replace("：", "").Trim();

                                Regex regChargeDept = new Regex(@"主管部门(：|:)[^\r\n]+\r\n");
                                pChargeDept = regChargeDept.Match(pInfoSource).Value.Replace("主管部门", "").Replace(":", "").Replace("：", "").Trim();

                                Regex regProClassLevel = new Regex(@"工程类别等级(：|:)[^\r\n]+\r\n");
                                pProClassLevel = regProClassLevel.Match(pInfoSource).Value.Replace("工程类别等级", "").Replace(":", "").Replace("：", "").Trim();

                                Regex regConst = new Regex(@"施工单位(：|:)[^\r\n]+\r\n");
                                pConstUnit = regConst.Match(pInfoSource).Value.Replace("施工单位", "").Replace(":", "").Replace("：", "").Trim();

                                Regex regPrjClass = new Regex(@"(工程类型|工程类别)(：|:)[^\r\n]+\r\n");
                                pPrjClass = regPrjClass.Match(pInfoSource).Value.Replace("工程类别", "").Replace("工程类型", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regBuildUnit = new Regex(@"(招标面积|本次招标面积)(：|:)[^\r\n]+\r\n");
                                pInviteArea = regBuildUnit.Match(pInfoSource).Value.Replace("本次招标面积", "").Replace("招标面积", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regpInviteArea = new Regex(@"建筑总面积(：|:)[^\r\n]+\r\n");
                                pBuildArea = regpInviteArea.Match(pInfoSource).Value.Replace("建筑总面积", "").Replace(":", "").Replace("：", "").Trim();

                                Regex regpProspUnit = new Regex(@"勘查单位(：|:)[^\r\n]+\r\n");
                                pProspUnit = regpProspUnit.Match(pInfoSource).Value.Replace("勘查单位", "").Replace(":", "").Replace("：", "").Trim();

                                Regex regpDesignUnit = new Regex(@"设计单位(：|:)[^\r\n]+\r\n");
                                pDesignUnit = regpDesignUnit.Match(pInfoSource).Value.Replace("设计单位", "").Replace(":", "").Replace("：", "").Trim();

                                Regex regpSuperUnit = new Regex(@"监理单位(：|:)[^\r\n]+\r\n");
                                pSuperUnit = regpSuperUnit.Match(pInfoSource).Value.Replace("监理单位", "").Replace(":", "").Replace("：", "").Trim();

                                Regex regpBeginDate = new Regex(@"(计划开工日期|预计开工日期)(：|:)[^\r\n]+\r\n");
                                pBeginDate = regpBeginDate.Match(pInfoSource).Value.Replace("计划开工日期", "").Replace("预计开工日期", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regpEndDate = new Regex(@"(计划竣工日期|预计竣工日期)(：|:)[^\r\n]+\r\n");
                                pEndDate = regpEndDate.Match(pInfoSource).Value.Replace("计划竣工日期", "").Replace("预计竣工日期", "").Replace("：", "").Replace(":", "").Trim();

                                if (!string.IsNullOrEmpty(pBeginDate))
                                {
                                    try
                                    {
                                        int date = pBeginDate.IndexOf("-");
                                        string time = pBeginDate.Substring(0, pBeginDate.Length - date - 5);
                                        if (time.Contains("0") || time.Contains("9"))
                                        {
                                            pBeginDate = "";
                                        }
                                    }
                                    catch { }
                                }
                                if (!string.IsNullOrEmpty(pEndDate))
                                {
                                    try
                                    {
                                        int date = pEndDate.IndexOf("-");
                                        string time = pEndDate.Substring(0, pEndDate.Length - date - 5);
                                        if (time.Contains("0") || time.Contains("9"))
                                        {
                                            pEndDate = "";
                                        }
                                    }
                                    catch { }
                                }

                                BaseProject info = ToolDb.GenBaseProject("广东省", pUrl, "深圳市区", pInfoSource, pBuilTime, pBeginDate, pEndDate, pConstUnit, pSuperUnit, pDesignUnit, pProspUnit, pInviteArea,
                                pBuildArea, pPrjClass, pProClassLevel, pChargeDept, pPrjAddress, pBuildUnit, pPrjCode, PrjName, pCreatetime, "深圳市住房和建设局");
                         
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount)
                                    return list;
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            return list;
        }
    }
}
