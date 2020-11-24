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

namespace Crawler.Instance
{
    public class SzProjectLicBA : WebSiteCrawller
    { 
        public SzProjectLicBA()
            : base()
        {
            this.Group = "工程信息";
            this.PlanTime = "03:55";
            this.Title = "深圳市宝安区建设局施工许可信息";
            this.Description = "自动抓取深圳市宝安区建设局施工许可信息";
            this.ExistCompareFields = "PrjName,BuildUnit,MsgType,PrjCode";
            this.MaxCount = 12000;
            this.SiteUrl = "http://www.szbajs.gov.cn/SiteManage/ConstructFiatList.aspx?rp=true&ModleNo=GCXX&tit=SGXK";
            this.Disabled = false;
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
                htl = ToolHtml.GetHtmlByUrlEncode(SiteUrl, Encoding.UTF8);
                viewState = this.ToolWebSite.GetAspNetViewState(htl);
                eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_Main_paging_LblPageCount")));
            if (tdNodes!=null&& tdNodes.Count > 0)
            {
                try
                {
                    page = int.Parse(tdNodes[0].ToPlainTextString().Trim());
                }
                catch { return list; }
            }
            for (int i = 1; i <= page; i++)
            { 
                if (i > 1)
                {
                    //if (i < 3)
                    //{
                    //    viewState = this.ToolWebSite.GetAspNetViewState(htl);
                    //    eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                    //}
                    //NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    //    "ctl00$ScriptManager1",
                    //    "__EVENTTARGET",
                    //    "__EVENTARGUMENT",
                    //    "__VIEWSTATE",
                    //    "__VIEWSTATEENCRYPTED",
                    //    "__EVENTVALIDATION",
                    //    "ctl00$Main$ddl_type",
                    //    "ctl00$Main$txt_Title",
                    //    "ctl00$Main$paging$txtPageIndex",
                    //    "__ASYNCPOST",
                    //    "ctl00$Main$paging$btnNext.x","ctl00$Main$paging$btnNext.y"
                    //}, new string[]{
                    //    "ctl00$Main$paging$btnForward",
                    //    string.Empty,
                    //    string.Empty,
                    //    viewState,
                    //    string.Empty,
                    //    eventValidation,
                    //    "","",i.ToString(),"true","5","9"
                    //});
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "ctl00$ScriptManager1",
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "ctl00$Main$ddl_type",
                        "ctl00$Main$txt_Title",
                        "ctl00$Main$paging$txtPageIndex",
                        "__VIEWSTATE",
                        "__VIEWSTATEGENERATOR",
                        "__VIEWSTATEENCRYPTED",
                        "__EVENTVALIDATION",
                        "__ASYNCPOST",
                        "ctl00$Main$paging$btnForward.x",
                        "ctl00$Main$paging$btnForward.y"
                    }, new string[]{
                        "ctl00$UpdatePanel1|ctl00$Main$paging$btnForward",
                        string.Empty,
                        string.Empty,
                        "1",
                        string.Empty,
                        i.ToString(), 
                        viewState,
                        "19AE96F3",
                        "",
                        eventValidation,
                        "true",
                        "7","9"
                    });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                        //viewState = htl.GetRegexBegEnd("VIEWSTATE", "hiddenField", 100000).Replace("|8|", "").Replace("|", "");
                        //eventValidation = htl.Replace("|", "kdxxAdmin").GetRegexBegEnd("EVENTVALIDATIONkdxxAdmin", "kdxxAdmin", 10000);
                        
                        //continue;
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_Main_GV_New")));
                if (tableList != null && tableList.Count > 0)
                {
                    TableTag table = (TableTag)tableList[0];
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string pPrjName = string.Empty, pBuildUnit = string.Empty, pBuildAddress = string.Empty, pBuildManager = string.Empty, pBuildScale = string.Empty, pPrjPrice = string.Empty, pPrjStartDate = string.Empty, PrjEndDate = string.Empty, pConstUnit = string.Empty, pConstUnitManager = string.Empty, pSuperUnit = string.Empty, pSuperUnitManager = string.Empty, pProspUnit = string.Empty, pProspUnitManager = string.Empty, pDesignUnit = string.Empty, pDesignUnitManager = string.Empty, pPrjManager = string.Empty, pSpecialPerson = string.Empty, pLicUnit = string.Empty, pPrjLicCode = string.Empty, PrjLicDate = string.Empty, pPrjDesc = string.Empty, pProvince = string.Empty, pCity = string.Empty, pInfoSource = string.Empty, pUrl = string.Empty, pCreatetime = string.Empty, pPrjCode = string.Empty;
                        TableRow tr = table.Rows[j];
                        pPrjName = tr.Columns[2].ToPlainTextString().Trim();
                        pPrjCode = tr.Columns[1].ToPlainTextString().Trim();
                        pBuildUnit = tr.Columns[3].ToPlainTextString().Trim();
                        PrjLicDate = tr.Columns[4].ToPlainTextString().Trim();
                        pUrl = "http://www.szbajs.gov.cn/SiteManage/" + tr.GetAttribute("ondblclick").Replace("&amp;", "&").Replace(")", "kdxx").GetRegexBegEnd("&#39;", "kdxx").Replace("&#39;", "");
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(pUrl), Encoding.UTF8);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "data_con")));
                        if (dtnode != null &&dtnode.Count > 0 )
                        {
                            string ctx = dtnode.AsString().Replace(" ", "");
                            pInfoSource = ctx;
                             
                            pDesignUnit = ctx.GetRegex(new string[]{"设计单位"});
                            pBuildAddress = ctx.GetRegex(new string[]{"工程地址","工程地点"});
                            pBuildScale = ctx.GetRegex(new string[] { "建设规模", "建筑面积" });
                            pSuperUnit = ctx.GetRegex(new string[]{"监理单位"});
                            pConstUnit = ctx.GetRegex(new string[] { "施工单位" });
                            pLicUnit = ctx.GetRegex(new string[] { "发证机关" });
                            pProspUnit = ctx.GetRegex(new string[] { "勘察单位" });
                            pPrjManager = ctx.GetRegex(new string[] { "项目经理", "项目负责人" });
                            pPrjStartDate = ctx.GetRegex(new string[] { "计划开工日期" });
                            PrjEndDate = ctx.GetRegex(new string[] { "计划竣工日期" });
                            pPrjPrice = ctx.GetMoneyRegex(new string[] { "工程造价" });
                            if (string.IsNullOrEmpty(PrjLicDate))
                                ctx.GetRegex("发证日期");
                             
                            if (string.IsNullOrEmpty(pLicUnit))
                            {
                                pLicUnit = "深圳市宝安区建设局";
                            }
                            ProjectLic info = ToolDb.GenProjectLic(pPrjName, pBuildUnit, pBuildAddress, pBuildManager, pBuildScale, pPrjPrice, pPrjStartDate, PrjEndDate, pConstUnit, pConstUnitManager, pSuperUnit, pSuperUnitManager, pProspUnit, pProspUnitManager, pDesignUnit, pDesignUnitManager, pPrjManager, pSpecialPerson, pLicUnit, pPrjLicCode, PrjLicDate, pPrjDesc, "广东省", "深圳市宝安区", pInfoSource, pUrl, pCreatetime, pPrjCode, "深圳市宝安区建设局");
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
