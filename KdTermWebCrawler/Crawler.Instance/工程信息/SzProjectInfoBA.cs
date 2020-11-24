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
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class SzProjectInfoBA : WebSiteCrawller
    {
        public SzProjectInfoBA()
            : base()
        {
            this.Group = "工程信息";
            this.PlanTime = "10:30,22:30";
            this.Title = "深圳市宝安区建设局工程基本信息";
            this.Description = "自动抓取深圳市宝安区建设局工程基本信息";
            this.ExistCompareFields = "PrjName,BuildUnit,MsgType";
            this.SiteUrl = "http://www.szbajs.gov.cn/SiteManage/ConstructFiatList.aspx?rp=true&ModleNo=GCXX&tit=SGXK";
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
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8, ref cookiestr);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_Main_paging_LblPageCount")));
            if (tdNodes.Count > 0)
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
                    if (i < 3)
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(htl);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                    }
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
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
                    }, new string[]{
                        "ctl00$UpdatePanel1|ctl00$Main$paging$btnForward",
                        string.Empty,
                        string.Empty,
                        "1",
                        string.Empty,
                        i.ToString(),
                        viewState,  "",eventValidation,"true","8","9"
                    });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
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
                        string pUrl = string.Empty, pInfoSource = string.Empty, pBeginDate = string.Empty,
                            pBuilTime = string.Empty, pEndDate = string.Empty, pConstUnit = string.Empty,
                            pSuperUnit = string.Empty, pDesignUnit = string.Empty, pProspUnit = string.Empty,
                            pInviteArea = string.Empty, pBuildArea = string.Empty, pPrjClass = string.Empty,
                            pProClassLevel = string.Empty, pChargeDept = string.Empty, pPrjAddress = string.Empty,
                            pBuildUnit = string.Empty, pPrjCode = string.Empty, PrjName = string.Empty,
                            pCreatetime = string.Empty;
                        TableRow tr = table.Rows[j];
                        PrjName = tr.Columns[2].ToPlainTextString().Trim();
                        pBuildUnit = tr.Columns[3].ToPlainTextString().Trim();
                        string aLink = string.Empty;
                        ATag aTag = new ATag();
                        try
                        {
                            aLink = tr.ToHtml().Replace("ondblclick", "href").Replace("<tr", "<a");
                            aLink = aLink.Remove(aLink.IndexOf("<td")) + "</a>";
                            parser = new Parser(new Lexer(aLink));
                            NodeFilter a = new TagNameFilter("a");
                            NodeList aList = parser.ExtractAllNodesThatMatch(a);
                            if (aList != null && aList.Count > 0)
                            {
                                aTag = aList.SearchFor(typeof(ATag), true)[0] as ATag;
                            }
                            if (aTag.Link.Contains("PrjManager") || aTag.Link.Contains("View"))
                            {
                                pUrl = aTag.Link.Remove(aTag.Link.IndexOf("View")).Replace("&amp;", "&") + "View";
                                int index = pUrl.IndexOf("PrjManager");
                                pUrl = "http://www.szbajs.gov.cn/SiteManage/" + pUrl.Substring(index, pUrl.Length - index);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        catch (Exception ex) { continue; }
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
                            pInfoSource = dtnode.AsString().Replace(" ", "");
                            Regex regPrjAddr = new Regex(@"(工程地点|工程地址)(：|:)[^\r\n]+\r\n");
                            pPrjAddress = regPrjAddr.Match(pInfoSource).Value.Replace("工程地址", "").Replace("工程地点", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regpProspUnit = new Regex(@"勘查单位(：|:)[^\r\n]+\r\n");
                            pProspUnit = regpProspUnit.Match(pInfoSource).Value.Replace("勘查单位", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regpDesignUnit = new Regex(@"设计单位(：|:)[^\r\n]+\r\n");
                            pDesignUnit = regpDesignUnit.Match(pInfoSource).Value.Replace("设计单位", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regpSuperUnit = new Regex(@"监理单位(：|:)[^\r\n]+\r\n");
                            pSuperUnit = regpSuperUnit.Match(pInfoSource).Value.Replace("监理单位", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regConst = new Regex(@"施工单位(：|:)[^\r\n]+\r\n");
                            pConstUnit = regConst.Match(pInfoSource).Value.Replace("施工单位", "").Replace(":", "").Replace("：", "").Trim();
                            if (string.IsNullOrEmpty(pChargeDept))
                                pChargeDept = "宝安区建设局";
                            BaseProject info = ToolDb.GenBaseProject("广东省", pUrl, "深圳市宝安区", pInfoSource, pBuilTime, pBeginDate, pEndDate, pConstUnit, pSuperUnit, pDesignUnit, pProspUnit, pInviteArea,
                                pBuildArea, pPrjClass, pProClassLevel, pChargeDept, pPrjAddress, pBuildUnit, pPrjCode, PrjName, pCreatetime, "深圳市宝安区建设局");

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
