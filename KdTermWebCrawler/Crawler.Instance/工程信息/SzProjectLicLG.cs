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

namespace Crawler.Instance
{
    public class SzProjectLicLG : WebSiteCrawller
    {
        public SzProjectLicLG()
            : base()
        {
            this.Group = "工程信息";
            this.PlanTime = "03:25";
            this.Title = "深圳市龙岗区建设局施工许可信息";
            this.Description = "自动抓取深圳市龙岗区建设局施工许可信息";
            this.ExistCompareFields = "Url";
            this.MaxCount = 50;
            this.SiteUrl = "http://www.cb.gov.cn/order/list/sgxkOrderList.jsp?itemId=226571&curId=226572&listId=226577"; 
            this.ExistsUpdate = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ProjectLic>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch 
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagefooter")));
            if (tdNodes.Count > 0 && tdNodes != null)
            {
                try
                {
                    string temp = tdNodes.AsString().GetRegexBegEnd(",共有", "页");
                    page = int.Parse(temp); 
                }
                catch { return list; }
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                { 
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "&web_cur_page=" + i, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table1")));
                if (tableNodeList.Count > 0 && tableNodeList != null)
                {
                    TableTag table = tableNodeList[tableNodeList.Count-1] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string pPrjName = string.Empty, pBuildUnit = string.Empty, pBuildAddress = string.Empty, pBuildManager = string.Empty, pBuildScale = string.Empty, pPrjPrice = string.Empty, pPrjStartDate = string.Empty, PrjEndDate = string.Empty, pConstUnit = string.Empty, pConstUnitManager = string.Empty, pSuperUnit = string.Empty, pSuperUnitManager = string.Empty, pProspUnit = string.Empty, pProspUnitManager = string.Empty, pDesignUnit = string.Empty, pDesignUnitManager = string.Empty, pPrjManager = string.Empty, pSpecialPerson = string.Empty, pLicUnit = string.Empty, pPrjLicCode = string.Empty, PrjLicDate = string.Empty, pPrjDesc = string.Empty, pProvince = string.Empty, pCity = string.Empty, pInfoSource = string.Empty, pUrl = string.Empty, pCreatetime = string.Empty, pPrjCode = string.Empty;
                        TableRow tr = table.Rows[j];
                        pPrjName = tr.Columns[3].ToPlainTextString().Trim();
                        pPrjCode = tr.Columns[2].ToPlainTextString().Trim();
                        PrjLicDate = tr.Columns[1].ToPlainTextString().Trim();
                        pBuildUnit = tr.Columns[4].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                        pUrl = "http://www.cb.gov.cn" + aTag.Link.Replace("GoDetail('", "").Replace("');", "");
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(pUrl, Encoding.UTF8).Replace("<br/>", "\r\n").Trim();
                        }
                        catch 
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htmldetail));
                        NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table1")));
                        if (dtList != null && dtList.Count > 0)
                        {
                            pInfoSource = dtList.AsHtml().GetReplace("</p>,</br>","\r\n").ToCtxString(); 
                            pDesignUnit = pInfoSource.GetRegex("设计单位"); 
                            pBuildAddress = pInfoSource.GetRegex("工程地址,工程地点");
                             
                            pBuildScale = pInfoSource.GetRegex("建筑面积,建设规模"); 
                            pSuperUnit = pInfoSource.GetRegex("监理单位"); 
                            pConstUnit = pInfoSource.GetRegex("施工单位"); 
                            pLicUnit = pInfoSource.GetRegex("发证机关"); 
                            pProspUnit = pInfoSource.GetRegex("勘察单位"); 
                            pPrjManager = pInfoSource.GetRegex("项目经理,项目负责人");  
                            pPrjStartDate = pInfoSource.GetRegex("计划开工日期");  
                            PrjEndDate = pInfoSource.GetRegex("计划竣工日期");  
                            pPrjPrice = pInfoSource.GetRegex("工程造价"); 
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
                                    pPrjPrice = (decimal.Parse(pInfoSource.GetRegex("工程造价")) / 10000).ToString();
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
                                pLicUnit = "深圳市龙岗区住房和建设局";
                            }
                            ProjectLic info = ToolDb.GenProjectLic(pPrjName, pBuildUnit, pBuildAddress, pBuildManager, pBuildScale, pPrjPrice, pPrjStartDate, PrjEndDate, pConstUnit, pConstUnitManager, pSuperUnit, pSuperUnitManager, pProspUnit, pProspUnitManager, pDesignUnit, pDesignUnitManager, pPrjManager, pSpecialPerson, pLicUnit, pPrjLicCode, PrjLicDate, pPrjDesc, "广东省", "深圳市龙岗区", pInfoSource, pUrl, pCreatetime, pPrjCode, "深圳市龙岗区住房和建设局");
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount)
                                return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
