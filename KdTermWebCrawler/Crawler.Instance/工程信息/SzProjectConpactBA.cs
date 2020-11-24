using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using Crawler;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Util;

namespace Crawler.Instance
{
    public class SzProjectConpactBA : WebSiteCrawller
    {
        public SzProjectConpactBA()
            : base()
        {
            this.Group = "工程信息";
            this.PlanTime = "03:55";
            this.Title = "深圳市宝安区建设局合同备案基本信息";
            this.MaxCount = 1000;
            this.Description = "自动抓取深圳市宝安区建设局合同备案基本信息";
            this.ExistCompareFields = "PrjName,PrjCode,BuildUnit,MsgType";
            this.SiteUrl = "http://www.szbajs.gov.cn/SiteManage/PictBackUpList.aspx?rp=true&ModleNo=GCXX&tit=HTBA";
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
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(this.SiteUrl), Encoding.UTF8, ref cookiestr);
                viewState = this.ToolWebSite.GetAspNetViewState(htl);
                eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_Main_paging_LblPageCount")));
            if (tdNodes.Count > 0 && tdNodes != null)
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
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]
                        {
                           "ctl00$ScriptManager1",
                           "__EVENTTARGET",
                           "__EVENTARGUMENT",
                           "ctl00$Main$ddl_type",
                           "ctl00$Main$txt_Title",
                           "ctl00$Main$paging$txtPageIndex",
                           "__VIEWSTATE",
                           "__VIEWSTATEENCRYPTED",
                           "__EVENTVALIDATION",
                           "__ASYNCPOST",
                           "ctl00$Main$paging$btnForward.x",
                           "ctl00$Main$paging$btnForward.y"
                        }, new string[]
                        {
                            "ctl00$UpdatePanel1|ctl00$Main$paging$btnNext","","","1","",i.ToString(),viewState,"",eventValidation,"true","6","6"
                        });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_Main_gv_ZbResult")));
                if (tableList != null && tableList.Count > 0)
                {
                    TableTag table = tableList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string pProvince = string.Empty, pUrl = string.Empty,
                            pCity = string.Empty, pSubcontractCode = string.Empty,
                            pSubcontractName = string.Empty, pSubcontractCompany = string.Empty,
                            pInfoSource = string.Empty, pRecordDate = string.Empty, pCompactPrice = string.Empty,
                            pCompactType = string.Empty, pBuildUnit = string.Empty, pPrjCode = string.Empty,
                            PrjName = string.Empty, pPrjMgrQual = string.Empty, pPrjMgrName = string.Empty,
                            pContUnit = string.Empty, pCreatetime = string.Empty;
                        TableRow tr = table.Rows[j];
                        pPrjCode = tr.Columns[2].ToPlainTextString().Trim();
                        PrjName = tr.Columns[3].ToNodePlainString();
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
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            string ctx = dtnode.AsString().Replace(" ", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("工程名称：\r\n", "工程名称：").Replace("工程名称:\r\n", "工程名称:");
                            pInfoSource = ctx;
                            string c = dtnode.AsString();
                            if (string.IsNullOrEmpty(PrjName))
                                PrjName = ctx.GetRegex("工程名称");
                            pBuildUnit = ctx.GetRegex("建设单位");
                            pContUnit = ctx.GetRegex("分包施工单位");
                            pPrjMgrName = ctx.GetRegex("施工单位联系人");
                            pCompactPrice = ctx.GetRegex("工程造价");
                            pCompactType = "专业分包合同";
                            pSubcontractCompany = ctx.GetRegex("总包施工单位");
                            pRecordDate = c.GetRegex("合同开工日期").GetDateRegex();
                            pSubcontractCode = pPrjCode;
                            pSubcontractName = PrjName;

                            ProjectConpact info = ToolDb.GenProjectConpact("广东省", pUrl, "深圳市宝安区", pSubcontractCode, pSubcontractName, pSubcontractCompany, pInfoSource, pRecordDate, pCompactPrice, pCompactType, pBuildUnit, pPrjCode, PrjName, pPrjMgrQual, pPrjMgrName, pContUnit, pCreatetime, "深圳市宝安区建设局");
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
