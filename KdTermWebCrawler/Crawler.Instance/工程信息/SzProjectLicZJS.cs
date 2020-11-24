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
    public class SzProjectLicZJS : WebSiteCrawller
    {
        public SzProjectLicZJS()
            : base(true)
        {
            this.Group = "工程信息";
            this.PlanTime = "12:04,03:30";
            this.Title = "深圳市住房和建设局施工许可信息（新版）";
            this.Description = "自动抓取深圳市住房和建设局施工许可信息（新版）";
            this.ExistCompareFields = "PrjCode";
            this.MaxCount = 100;
            this.SiteUrl = "http://www.szjs.gov.cn/build/build.ashx?_=1352593922281&menu=%E9%A1%B9%E7%9B%AE%E4%BF%A1%E6%81%AF&type=%E6%96%BD%E5%B7%A5%E8%AE%B8%E5%8F%AF&pageSize=20&pageIndex=1"; 
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
                        htl = this.ToolWebSite.GetHtmlByUrl("http://www.szjs.gov.cn/build/build.ashx?_=1352593922281&menu=%E9%A1%B9%E7%9B%AE%E4%BF%A1%E6%81%AF&type=%E6%96%BD%E5%B7%A5%E8%AE%B8%E5%8F%AF&pageSize=20&pageIndex=" + i.ToString(), Encoding.UTF8);
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
                        string pPrjName = string.Empty, pBuildUnit = string.Empty,
                            pBuildAddress = string.Empty, pBuildManager = string.Empty,
                            pBuildScale = string.Empty, pPrjPrice = string.Empty,
                            pPrjStartDate = string.Empty, PrjEndDate = string.Empty,
                            pConstUnit = string.Empty, pConstUnitManager = string.Empty,
                            pSuperUnit = string.Empty, pSuperUnitManager = string.Empty,
                            pProspUnit = string.Empty, pProspUnitManager = string.Empty,
                            pDesignUnit = string.Empty, pDesignUnitManager = string.Empty,
                            pPrjManager = string.Empty, pSpecialPerson = string.Empty,
                            pLicUnit = string.Empty, pPrjLicCode = string.Empty,
                            PrjLicDate = string.Empty, pPrjDesc = string.Empty,
                            pProvince = string.Empty, pCity = string.Empty,
                            pInfoSource = string.Empty, pUrl = string.Empty,
                            pCreatetime = string.Empty, pPrjCode = string.Empty;
                        try
                        {
                            pPrjCode = Convert.ToString(dicSmsType["AnnSerial"]);
                            pPrjName = Convert.ToString(dicSmsType["PrjName"]);
                            pBuildUnit = Convert.ToString(dicSmsType["ConstOrg"]);
                            PrjLicDate = Convert.ToString(dicSmsType["IssueDate"]);
                            pUrl = "http://www.szjs.gov.cn/build/sgxk_detail.aspx?id=" + pPrjCode;
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
                            NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "js-table mar-l-4")));
                            if (dtList != null && dtList.Count > 0)
                            {
                                TableTag table = dtList[0] as TableTag;
                                for (int j = 0; j < table.RowCount; j++)
                                {
                                    TableRow dr = table.Rows[j];
                                    string ctx = string.Empty;
                                    for (int k = 0; k < dr.ColumnCount; k++)
                                    {
                                        ctx += dr.Columns[k].ToPlainTextString().Trim().Replace("\r", "").Replace("\n", "");
                                    }
                                    pInfoSource += ctx + "\r\n";
                                }
                                
                                Regex regpDesignUnit = new Regex(@"设计单位(：|:)[^\r\n]+\r\n");
                                pDesignUnit = regpDesignUnit.Match(pInfoSource).Value.Replace("设计单位", "").Replace("/", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regAddree = new Regex(@"(工程地址|工程地点)(：|:)[^\r\n]+\r\n");
                                pBuildAddress = regAddree.Match(pInfoSource).Value.Replace("工程地址", "").Replace("工程地点", "").Replace("/", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regSca = new Regex(@"(建筑面积|建设规模)(：|:)[^\r\n]+\r\n");
                                pBuildScale = regSca.Match(pInfoSource).Value.Replace("建设规模", "").Replace("建筑面积", "").Replace("/", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regpSuperUnit = new Regex(@"监理单位(：|:)[^\r\n]+\r\n");
                                pSuperUnit = regpSuperUnit.Match(pInfoSource).Value.Replace("监理单位", "").Replace("/", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regpConstUnit = new Regex(@"施工单位(：|:)[^\r\n]+\r\n");
                                pConstUnit = regpConstUnit.Match(pInfoSource).Value.Replace("施工单位", "").Replace("/", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regpLicUnit = new Regex(@"发证机关(：|:)[^\r\n]+\r\n");
                                pLicUnit = regpLicUnit.Match(pInfoSource).Value.Replace("发证机关", "").Replace("/", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regpPosoUnit = new Regex(@"勘察单位(：|:)[^\r\n]+\r\n");
                                pProspUnit = regpPosoUnit.Match(pInfoSource).Value.Replace("勘察单位", "").Replace("/", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regMan = new Regex(@"(项目经理|项目负责人)(：|:)[^\r\n]+\r\n");
                                pPrjManager = regMan.Match(pInfoSource).Value.Replace("项目负责人", "").Replace("项目经理", "").Replace("/", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regBeg = new Regex(@"计划开工日期(：|:)[^\r\n]+\r\n");
                                pPrjStartDate = regBeg.Match(pInfoSource).Value.Replace("计划开工日期", "").Replace("/", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regEnd = new Regex(@"计划竣工日期(：|:)[^\r\n]+\r\n");
                                PrjEndDate = regEnd.Match(pInfoSource).Value.Replace("计划竣工日期", "").Replace("/", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regpPrice = new Regex(@"工程造价(：|:)[^\r\n]+\r\n");
                                pPrjPrice = regpPrice.Match(pInfoSource).Value.Replace("工程造价", "").Replace("/", "").Replace("：", "").Replace(":", "").Trim();
                                Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                                if (pPrjPrice.Contains("万"))
                                {
                                    pPrjPrice = pPrjPrice.Remove(pPrjPrice.IndexOf("万")).Trim();
                                    pPrjPrice = regBidMoney.Match(pPrjPrice).Value;
                                }
                                else
                                {
                                    try
                                    {
                                        pPrjPrice = (decimal.Parse(regpPrice.Match(pPrjPrice).Value) / 10000).ToString();
                                        if (decimal.Parse(pPrjPrice) < decimal.Parse("0.1"))
                                        {
                                            pPrjPrice = "0";
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        pPrjPrice = "0";
                                    }
                                }
                                if (string.IsNullOrEmpty(pLicUnit))
                                {
                                    pLicUnit = "深圳市住房和建设局";
                                }
                                ProjectLic info = ToolDb.GenProjectLic(pPrjName, pBuildUnit, pBuildAddress, pBuildManager, pBuildScale, pPrjPrice, pPrjStartDate, PrjEndDate, pConstUnit, pConstUnitManager, pSuperUnit, pSuperUnitManager, pProspUnit, pProspUnitManager, pDesignUnit, pDesignUnitManager, pPrjManager, pSpecialPerson, pLicUnit, pPrjLicCode, PrjLicDate, pPrjDesc, "广东省", "深圳市区", pInfoSource, pUrl, pCreatetime, pPrjCode, "深圳市住房和建设局");
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
