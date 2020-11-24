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

namespace Crawler.Instance
{
    public class SzProjectConpactZJS : WebSiteCrawller
    {
        public SzProjectConpactZJS()
            : base(true)
        {
            this.Group = "工程信息";
            this.PlanTime = "12:00,03:20";
            this.Title = "深圳市住房与建设局合同备案基本信息（新版）";
            this.MaxCount = 100;
            this.Description = "自动抓取深圳市住房与建设局合同备案基本信息（新版）";
            this.ExistCompareFields = "PrjName,ContUnit,CompactType";
            this.SiteUrl = "http://www.szjs.gov.cn/build/build.ashx?_=1352582850568&menu=%E9%A1%B9%E7%9B%AE%E4%BF%A1%E6%81%AF&type=%E6%96%BD%E5%B7%A5%E7%9B%91%E7%90%86%E5%90%88%E5%90%8C%E5%A4%87%E6%A1%88&pageSize=20&pageIndex=1";
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
                        htl = this.ToolWebSite.GetHtmlByUrl("http://www.szjs.gov.cn/build/build.ashx?_=1352582850568&menu=%E9%A1%B9%E7%9B%AE%E4%BF%A1%E6%81%AF&type=%E6%96%BD%E5%B7%A5%E7%9B%91%E7%90%86%E5%90%88%E5%90%8C%E5%A4%87%E6%A1%88&pageSize=20&pageIndex=" + i.ToString(), Encoding.UTF8);
                    }
                    catch (Exception ex)
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
                        string pProvince = string.Empty, pUrl = string.Empty,
                            pCity = string.Empty, pSubcontractCode = string.Empty,
                            pSubcontractName = string.Empty, pSubcontractCompany = string.Empty,
                            pInfoSource = string.Empty, pRecordDate = string.Empty, pCompactPrice = string.Empty,
                            pCompactType = string.Empty, pBuildUnit = string.Empty, pPrjCode = string.Empty,
                            PrjName = string.Empty, pPrjMgrQual = string.Empty, pPrjMgrName = string.Empty,
                            pContUnit = string.Empty, pCreatetime = string.Empty;
                        try
                        {
                            string noid = Convert.ToString(dicSmsType["Nid"]);
                            PrjName = Convert.ToString(dicSmsType["PrjName"]);
                            pBuildUnit = Convert.ToString(dicSmsType["ConstOrg"]);
                            pContUnit = Convert.ToString(dicSmsType["CorpName"]);
                            pCompactType = Convert.ToString(dicSmsType["PactType"]);
                            pRecordDate = Convert.ToString(dicSmsType["IssueDate"]);
                            pUrl = "http://www.szjs.gov.cn/build/htba_detail.aspx?id=" + noid;
                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(pUrl), Encoding.UTF8).Trim();
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
                                        ctx += dr.Columns[k].ToPlainTextString().Trim().Replace(" ", "").Replace("\r", "").Replace("\n", "");
                                    }
                                    pInfoSource += ctx + "\r\n";
                                }
                                Regex regexsubcode = new Regex(@"分包工程编号(：|:)[^\r\n]+\r\n");
                                pSubcontractCode = regexsubcode.Match(pInfoSource).Value.Replace("分包工程编号：", "").Trim();
                                Regex regexsubname = new Regex(@"分包工程名称(：|:)[^\r\n]+\r\n");
                                pSubcontractName = regexsubname.Match(pInfoSource).Value.Replace("分包工程名称：", "").Trim();
                                Regex regexsubcom = new Regex(@"分包工程发包单位(：|:)[^\r\n]+\r\n");
                                pSubcontractCompany = regexsubcom.Match(pInfoSource).Value.Replace("分包工程发包单位：", "").Trim();
                                Regex regpCompactPrice = new Regex(@"合同价(：|:)[^\r\n]+\r\n");
                                pCompactPrice = regpCompactPrice.Match(pInfoSource).Value.Replace("合同价：", "").Trim();
                                Regex regpPrjMgrQual = new Regex(@"项目经理资格(：|:)[^\r\n]+\r\n");
                                pPrjMgrQual = regpPrjMgrQual.Match(pInfoSource).Value.Replace("项目经理资格：", "").Trim();
                                Regex regpPrjMgrName = new Regex(@"项目经理名称(：|:)[^\r\n]+\r\n");
                                pPrjMgrName = regpPrjMgrName.Match(pInfoSource).Value.Replace("项目经理名称：", "").Trim();
                                Regex regpPrjCode = new Regex(@"(工程编号|总包工程编号)(：|:)[^\r\n]+\r\n");
                                pPrjCode = regpPrjCode.Match(pInfoSource).Value.Replace("总包工程编号", "").Replace("工程编号", "").Replace(":", "").Replace("：", "").Replace("总包", "").Trim();

                                Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                                if (pCompactPrice.Contains("万"))
                                {
                                    pCompactPrice = pCompactPrice.Remove(pCompactPrice.IndexOf("万")).Trim();
                                    pCompactPrice = regBidMoney.Match(pCompactPrice).Value;
                                }
                                else
                                {
                                    try
                                    {
                                        pCompactPrice = (decimal.Parse(regBidMoney.Match(pCompactPrice).Value) / 10000).ToString();
                                        if (decimal.Parse(pCompactPrice) < decimal.Parse("0.1"))
                                        {
                                            pCompactPrice = "0";
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        pCompactPrice = "0";
                                    }
                                }
                            }
                            ProjectConpact info = ToolDb.GenProjectConpact("广东省", pUrl, "深圳市区", pSubcontractCode, pSubcontractName, pSubcontractCompany, pInfoSource, pRecordDate, pCompactPrice, pCompactType, pBuildUnit, pPrjCode, PrjName, pPrjMgrQual, pPrjMgrName, pContUnit, pCreatetime, "深圳市住房和建设局");

                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount)
                                return list;
                        }
                        catch
                        { continue; }
                    }
                }
            }
            return list;
        }
    }
}
