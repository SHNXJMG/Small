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
    public class SzProjectFinishBA : WebSiteCrawller
    {
        public SzProjectFinishBA()
            : base()
        {
            this.Group = "工程信息";
            this.PlanTime = "03:55";
            this.Title = "深圳市宝安区建设局竣工验收信息";
            this.Description = "自动抓取深圳市宝安区建设局竣工验收信息";
            this.ExistCompareFields = "PrjName,PrjEndCode,BuildUnit,MsgType";
            this.SiteUrl = "http://www.szbajs.gov.cn/SiteManage/FinishCheckList.aspx?rp=true&ModleNo=GCXX&tit=JGBA";
            this.MaxCount = 4000; 
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
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8, ref cookiestr);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_Main_paging_LblPageCount")));
            if (pageList != null && pageList.Count > 0)
            {
                try { page = int.Parse(pageList.AsString()); }
                catch { }
            }
            for (int i = 1; i <= page; i++)
            { 
                if (i > 1)
                {
                    if (i < 3)
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(htl);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                    }
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]
                        {
                           "ctl00$ScriptManager1",
                           "__EVENTTARGET",
                           "__EVENTARGUMENT",
                           "ctl00$Main$ddl_type",
                           "ctl00$Main$txt_Title",
                           "ctl00$Main$paging$txtPageIndex",
                           "__VIEWSTATE", 
                           "__EVENTVALIDATION",
                           "__VIEWSTATEENCRYPTED", 
                           "__ASYNCPOST",
                           "ctl00$Main$paging$btnForward.x",
                           "ctl00$Main$paging$btnForward.y"
                        },
                        new string[]
                        {
                            "ctl00$UpdatePanel1|ctl00$Main$paging$btnForward",
                            string.Empty,
                            string.Empty,
                            "1",
                            string.Empty, 
                            i.ToString(),
                            viewState,
                            eventValidation,
                            string.Empty, 
                            "true",
                            "10","11"
                        });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_Main_Gv_FinishCheck")));
                if (dtList != null && dtList.Count > 0)
                {
                    TableTag table = dtList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string pUrl = string.Empty, pInfoSource = string.Empty, pEndDate = string.Empty,
                                pConstUnit = string.Empty, pSuperUnit = string.Empty, pDesignUnit = string.Empty,
                                prjEndDesc = string.Empty, pPrjAddress = string.Empty, pBuildUnit = string.Empty,
                                pPrjCode = string.Empty, PrjName = string.Empty, pRecordUnit = string.Empty,
                                pCreatetime = string.Empty, pLicUnit = string.Empty;
                        TableRow tr = table.Rows[j];
                        PrjName = tr.Columns[3].ToPlainTextString().Trim();
                        pPrjCode = tr.Columns[1].ToPlainTextString().Trim();
                        pEndDate = tr.Columns[4].ToPlainTextString().Trim();
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
                        if (dtnode.Count > 0 && dtnode != null)
                        {
                            string ctx = dtnode.AsString().Replace(" ", "");
                            pInfoSource = ctx;
                            Regex regPrjAddr = new Regex(@"(建设地点|工程地址)(：|:)[^\r\n]+\r\n");
                            pPrjAddress = regPrjAddr.Match(ctx).Value.Replace("工程地址", "").Replace("建设地点", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regpDesignUnit = new Regex(@"设计单位(：|:)[^\r\n]+\r\n");
                            pDesignUnit = regpDesignUnit.Match(ctx).Value.Replace("设计单位", "").Replace("/", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regpSuperUnit = new Regex(@"监理单位(：|:)[^\r\n]+\r\n");
                            pSuperUnit = regpSuperUnit.Match(ctx).Value.Replace("监理单位", "").Replace("/", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regpConstUnit = new Regex(@"施工单位(：|:)[^\r\n]+\r\n");
                            pConstUnit = regpConstUnit.Match(ctx).Value.Replace("施工单位", "").Replace("/", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regpBuiUnit = new Regex(@"建设单位(：|:)[^\r\n]+\r\n");
                            pBuildUnit = regpBuiUnit.Match(ctx).Value.Replace("建设单位", "").Replace("/", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regpRecordUnit = new Regex(@"备案机关(：|:)[^\r\n]+\r\n");
                            pRecordUnit = regpRecordUnit.Match(ctx).Value.Replace("备案机关", "").Replace("/", "").Replace(":", "").Replace("：", "").Trim(); ;

                            Regex regpLicUnit = new Regex(@"发证机关(：|:)[^\r\n]+\r\n");
                            pLicUnit = regpLicUnit.Match(ctx).Value.Replace("发证机关", "").Replace("/", "").Replace(":", "").Replace("：", "").Trim(); ;

                            if (string.IsNullOrEmpty(pLicUnit))
                            {
                                pLicUnit = "深圳市宝安区建设局";
                            }

                            ProjectFinish info = ToolDb.GenProjectFinish("广东省", pUrl, "深圳市宝安区", pInfoSource, pEndDate, pConstUnit, pSuperUnit, pDesignUnit, prjEndDesc, pPrjAddress, pBuildUnit, pPrjCode, PrjName, pRecordUnit, pCreatetime, "深圳市宝安区建设局", pLicUnit);
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
