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
    public class SzProjectFinishZJS : WebSiteCrawller
    {
        public SzProjectFinishZJS()
            : base()
        {
            this.Group = "工程信息";
            this.PlanTime = "12:02,03:35";
            this.Title = "深圳市住房和建设局竣工验收信息（新版）";
            this.Description = "自动抓取深圳市住房和建设局竣工验收信息（新版）";
            this.ExistCompareFields = "PrjEndCode";
            this.SiteUrl = "http://www.szjs.gov.cn/build/build.ashx?_=1352585430077&menu=%E9%A1%B9%E7%9B%AE%E4%BF%A1%E6%81%AF&type=%E7%AB%A3%E5%B7%A5%E9%AA%8C%E6%94%B6%E5%A4%87%E6%A1%88&pageSize=20&pageIndex=1";
            this.MaxCount = 100;
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
                        htl = this.ToolWebSite.GetHtmlByUrl("http://www.szjs.gov.cn/build/build.ashx?_=1352585430077&menu=%E9%A1%B9%E7%9B%AE%E4%BF%A1%E6%81%AF&type=%E7%AB%A3%E5%B7%A5%E9%AA%8C%E6%94%B6%E5%A4%87%E6%A1%88&pageSize=20&pageIndex=" + i.ToString(), Encoding.UTF8);
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
                        string pUrl = string.Empty, pInfoSource = string.Empty, pEndDate = string.Empty,
                            pConstUnit = string.Empty, pSuperUnit = string.Empty, pDesignUnit = string.Empty,
                            prjEndDesc = string.Empty, pPrjAddress = string.Empty, pBuildUnit = string.Empty,
                            pPrjCode = string.Empty, PrjName = string.Empty, pRecordUnit = string.Empty,
                            pCreatetime = string.Empty, pLicUnit = string.Empty;
                        try
                        {
                            pPrjCode = Convert.ToString(dicSmsType["LogSerial"]);
                            PrjName = Convert.ToString(dicSmsType["PrjLogName"]);
                            pBuildUnit = Convert.ToString(dicSmsType["ConstName"]);
                            pEndDate = Convert.ToString(dicSmsType["LogDate"]);
                            pUrl = "http://www.szjs.gov.cn/build/jgys_detail.aspx?id=" + pPrjCode;
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
                                Regex regPrjAddr = new Regex(@"(建设地点|工程地址)(：|:)[^\r\n]+\r\n");
                                pPrjAddress = regPrjAddr.Match(pInfoSource).Value.Replace("工程地址", "").Replace("建设地点", "").Replace(":", "").Replace("：", "").Trim();
                                Regex regpDesignUnit = new Regex(@"设计单位(：|:)[^\r\n]+\r\n");
                                pDesignUnit = regpDesignUnit.Match(pInfoSource).Value.Replace("设计单位", "").Replace("/", "").Replace(":", "").Replace("：", "").Trim();
                                Regex regpSuperUnit = new Regex(@"监理单位(：|:)[^\r\n]+\r\n");
                                pSuperUnit = regpSuperUnit.Match(pInfoSource).Value.Replace("监理单位", "").Replace("/", "").Replace(":", "").Replace("：", "").Trim();
                                Regex regpConstUnit = new Regex(@"施工单位(：|:)[^\r\n]+\r\n");
                                pConstUnit = regpConstUnit.Match(pInfoSource).Value.Replace("施工单位", "").Replace("/", "").Replace(":", "").Replace("：", "").Trim();
                                Regex regpRecordUnit = new Regex(@"备案机关(：|:)[^\r\n]+\r\n");
                                pRecordUnit = regpRecordUnit.Match(pInfoSource).Value.Replace("备案机关", "").Replace("/", "").Replace(":", "").Replace("：", "").Trim();
                                Regex regpLicUnit = new Regex(@"发证机关(：|:)[^\r\n]+\r\n");
                                pLicUnit = regpLicUnit.Match(pInfoSource).Value.Replace("发证机关", "").Replace("/", "").Replace(":", "").Replace("：", "").Trim(); ;
                                if (string.IsNullOrEmpty(pLicUnit))
                                {
                                    pLicUnit = "深圳市住房和建设局";
                                }
                                ProjectFinish info = ToolDb.GenProjectFinish("广东省", pUrl, "深圳市区", pInfoSource, pEndDate, pConstUnit, pSuperUnit, pDesignUnit, prjEndDesc, pPrjAddress, pBuildUnit, pPrjCode, PrjName, pRecordUnit, pCreatetime, "深圳市住房和建设局", pLicUnit);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount)
                                    return list;
                            }
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
