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
using System.Data;

namespace Crawler.Instance
{
    public class SzProjectFinishLG : WebSiteCrawller
    {
        public SzProjectFinishLG()
            : base()
        {
            this.Group = "工程信息";
            this.PlanTime = "03:15";
            this.Title = "深圳市龙岗区建设局竣工验收信息";
            this.Description = "自动抓取深圳市龙岗区建设局竣工验收信息";
            this.ExistCompareFields = "Url";
            this.SiteUrl = "http://www.cb.gov.cn/order/list/jgysOrderList.jsp?itemId=226571&curId=226572&listId=226578";
            this.MaxCount = 50;
            this.ExistsUpdate = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ProjectFinish>();
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
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagefooter")));
            if (pageList != null && pageList.Count > 0)
            {
                try 
                {
                    string temp = pageList.AsString().GetRegexBegEnd(",共有", "页");
                    page = int.Parse(temp); 
                }
                catch { }
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                { 
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "&web_cur_page="+i, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table1")));
                if (dtList != null && dtList.Count > 0)
                {
                    TableTag table = dtList[dtList.Count-1] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string pUrl = string.Empty, pInfoSource = string.Empty, pEndDate = string.Empty,
                            pConstUnit = string.Empty, pSuperUnit = string.Empty, pDesignUnit = string.Empty,
                            prjEndDesc = string.Empty, pPrjAddress = string.Empty, pBuildUnit = string.Empty,
                            pPrjCode = string.Empty, PrjName = string.Empty, pRecordUnit = string.Empty,
                            pCreatetime = string.Empty, pLicUnit = string.Empty;
                        TableRow tr = table.Rows[j];
                        PrjName = tr.Columns[3].ToPlainTextString().Trim();
                        pPrjCode = tr.Columns[2].ToPlainTextString().Trim();
                        pEndDate = tr.Columns[1].ToPlainTextString().Trim();
                        pBuildUnit = tr.Columns[4].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                        pUrl = "http://www.cb.gov.cn" + aTag.Link.Replace("GoDetail('", "").Replace("');", "");
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(pUrl, Encoding.UTF8).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table1")));
                        if (dtnode.Count > 0 && dtnode != null)
                        {
                            
                            pInfoSource = dtnode.AsHtml().ToCtxString(); 
                            pPrjAddress = pInfoSource.GetRegex("建设地点,工程地址"); 
                            pDesignUnit = pInfoSource.GetRegex("设计单位"); 
                            pSuperUnit = pInfoSource.GetRegex("监理单位"); 
                            pConstUnit = pInfoSource.GetRegex("施工单位"); 
                            pRecordUnit = pInfoSource.GetRegex("备案机关"); 
                            pLicUnit = pInfoSource.GetRegex("发证机关");
                            if (string.IsNullOrEmpty(pLicUnit))
                            {
                                pLicUnit = "深圳市龙岗区住房和建设局";
                            }
                            ProjectFinish info = ToolDb.GenProjectFinish("广东省", pUrl, "深圳市龙岗区", pInfoSource, pEndDate, pConstUnit, pSuperUnit, pDesignUnit, prjEndDesc, pPrjAddress, pBuildUnit, pPrjCode, PrjName, pRecordUnit, pCreatetime, "深圳市龙岗区住房和建设局", pLicUnit);
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
