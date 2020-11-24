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
    public class ItemInfoSzJs:WebSiteCrawller
    {
        public ItemInfoSzJs()
            : base(true)
        {
            this.Group = "项目信息";
            this.PlanTime = "12:10,03:15";
            this.Title = "深圳市住房和建设局项目信息";
            this.MaxCount = 100;
            this.Description = "自动抓取深圳市住房和建设局项目信息";
            this.ExistCompareFields = "URL,ItemName";
            this.SiteUrl = "http://www.szjs.gov.cn/build/build.ashx?_=1352754365882&menu=%E9%A1%B9%E7%9B%AE%E4%BF%A1%E6%81%AF&type=%E9%A1%B9%E7%9B%AE%E4%BF%A1%E6%81%AF&pageSize=20&pageIndex=1";
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
            for (int i = 1; i<=page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl("http://www.szjs.gov.cn/build/build.ashx?_=1352754365882&menu=%E9%A1%B9%E7%9B%AE%E4%BF%A1%E6%81%AF&type=%E9%A1%B9%E7%9B%AE%E4%BF%A1%E6%81%AF&pageSize=20&pageIndex=" + i.ToString(), Encoding.UTF8);
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
                        string itemCode=string.Empty,itemName = string.Empty,buildUnit=string.Empty,address  =string.Empty,
                            investMent=string.Empty,buildKind=string.Empty,investKink=string.Empty,linkMan=string.Empty,
                            linkmanTel=string.Empty,itemDesc=string.Empty,apprNo=string.Empty,apprDate=string.Empty,
                            apprUnit=string.Empty, apprResult=string.Empty,
                            landapprNo=string.Empty,landplanNo=string.Empty,buildDate=string.Empty,infoSource=string.Empty,url=string.Empty,
                            textCode=string.Empty,licCode=string.Empty,msgType=string.Empty;
                        try
                        {
                            itemCode = Convert.ToString(dicSmsType["ItemId"]);
                            itemName = Convert.ToString(dicSmsType["ItemName"]);
                            buildUnit = Convert.ToString(dicSmsType["ConstOrg"]);
                            address = Convert.ToString(dicSmsType["ItemLocation"]);
                            url = "http://www.szjs.gov.cn/build/xmxx_detail.aspx?id=" + itemCode;
                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8).Trim();
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex.ToString() + "==>" + i.ToString());
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
                                    infoSource += ctx + "\r\n";
                                }
                                Regex regText = new Regex(@"文号(：|:)[^\r\n]+\r\n");
                                textCode = regText.Match(infoSource).Value.Replace("文号", "").Trim().Replace(" ", "").Replace(":", "").Replace("：", "");

                                Regex regLic = new Regex(@"施工许可号(：|:)[^\r\n]+\r\n");
                                licCode = regLic.Match(infoSource).Value.Replace("施工许可号", "").Trim().Replace(" ", "").Replace(":", "").Replace("：", "");

                                Regex regDate = new Regex(@"(报建时间|报建日期)(：|:)[^\r\n]+\r\n");
                                buildDate = regDate.Match(infoSource).Value.Replace("报建日期", "").Replace("报建时间", "").Trim().Replace(" ", "").Replace(":", "").Replace("：", "");

                                Regex regPri = new Regex(@"计划总投资(：|:)[^\r\n]+\r\n");
                                investMent = regPri.Match(infoSource).Value.Replace("计划总投资", "").Trim().Replace(" ", "").Replace(":", "").Replace("：", "");

                                Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                                if (investMent.Contains("万"))
                                {
                                    investMent = investMent.Remove(investMent.IndexOf("万")).Trim();
                                    investMent = regBidMoney.Match(investMent).Value;
                                }
                                else
                                {
                                    try
                                    {
                                        investMent = (decimal.Parse(regBidMoney.Match(investMent).Value) / 10000).ToString();
                                        if (decimal.Parse(investMent) < decimal.Parse("0.1"))
                                        {
                                            investMent = "0";
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        investMent = "0";
                                    }
                                }
                                if (string.IsNullOrEmpty(investMent))
                                {
                                    investMent = "0";
                                }
                                msgType = "深圳市住房和建设局";
                                ItemInfo info = ToolDb.GenItemInfo(itemCode, itemName, buildUnit, address, investMent, buildKind, investKink, linkMan, linkmanTel, itemDesc, apprNo, apprDate, apprUnit, apprResult, landapprNo, landplanNo, buildDate, "广东省", "深圳市区", infoSource, url, textCode, licCode, msgType, "");
                                //ToolDb.SaveEntity(info, this.ExistCompareFields); 
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount)
                                    return list;
                            }
                        }
                        catch(Exception ex)
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
