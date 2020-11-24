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
using System.Web.UI.WebControls;

namespace Crawler.Instance
{
    public class SzProjectInfoLG : WebSiteCrawller
    {
        public SzProjectInfoLG()
            : base(true)
        {
            this.Group = "工程信息";
            this.PlanTime = "10:30,22:30";
            this.Title = "深圳市龙岗区建设局工程基本信息";
            this.Description = "自动抓取深圳市龙岗区建设局工程基本信息";
            this.ExistCompareFields = "Url";
            this.SiteUrl = "http://www.cb.gov.cn/sgxk/browse.aspx";
            this.MaxCount = 50; 
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
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "PageNumControl1_lbltotal")));
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
                    viewState = this.ToolWebSite.GetAspNetViewState(htl);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__LASTFOCUS",
                        "__VIEWSTATE",
                        "txtPrj_ID",
                        "txtPrj_Name","Chk_Query","Radiobuttonlist1","PageNumControl1$gotopage","PageNumControl1$NEXTpage","__EVENTVALIDATION",
                    }, new string[]{
                        string .Empty,
                        string.Empty,
                        string.Empty,
                        viewState,
                        string.Empty,
                        string .Empty,
                        "0",  "0","","下一页",eventValidation
                    });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "dgConstBid")));
                if (tableNodeList!=null&&tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string pUrl = string.Empty, pInfoSource = string.Empty, pBeginDate = string.Empty, pBuilTime = string.Empty, pEndDate = string.Empty, pConstUnit = string.Empty, pSuperUnit = string.Empty, pDesignUnit = string.Empty, pProspUnit = string.Empty, pInviteArea = string.Empty, pBuildArea = string.Empty, pPrjClass = string.Empty, pProClassLevel = string.Empty, pChargeDept = string.Empty, pPrjAddress = string.Empty, pBuildUnit = string.Empty, pPrjCode = string.Empty, PrjName = string.Empty, pCreatetime = string.Empty;
                        Winista.Text.HtmlParser.Tags.TableRow tr = table.Rows[j]; 
                        PrjName = tr.Columns[3].ToPlainTextString().Trim();
                        pBuildUnit = tr.Columns[4].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                        string link = aTag.Link.Replace("GoDetail('", "").Replace("')", "").Replace(";", "");
                        pUrl = "http://www.cb.gov.cn/sgxk/Details.aspx?NID=" + link;// +"&xxlxbh=&PRJ_TYPE=0";
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(pUrl), Encoding.UTF8).Replace("<br>", "\r\n");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table8")));
                        if (dtnode!=null&&dtnode.Count > 0)
                        {
                            string ctx = string.Empty;
                            string ctx1 = string.Empty;
                            string dateStr = string.Empty;
                            TableTag tableTwo = (TableTag)dtnode[0];
                            for (int k = 0; k < tableTwo.RowCount; k++)
                            {
                                Winista.Text.HtmlParser.Tags.TableRow trTwo = tableTwo.Rows[k];
                                for (int z = 0; z < trTwo.ColumnCount; z++)
                                {
                                    dateStr = trTwo.Columns[z].ToPlainTextString().Replace("\t", "").Replace("<br>", "\r\n").Replace("&nbsp;", "").Replace("<br/>", "\r\n").Trim();
                                    ctx += trTwo.Columns[z].ToPlainTextString().Replace("\t", "").Replace("<br>", "\r\n").Replace("&nbsp;", "").Replace("<br/>", "\r\n").Replace(" ", "").Trim();
                                }
                                ctx += "\r\n";
                            }

                            ctx1 = dtnode.AsString().Replace("&nbsp;", "").Replace("\t", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Trim();
                            pInfoSource = ctx1;

                            pPrjCode = ctx.GetRegex("工程序号");

                            Regex regPrjAddr = new Regex(@"(工程地点|工程地址)(：|:)[^\r\n]+\r\n");
                            pPrjAddress = regPrjAddr.Match(ctx).Value.Replace("工程地址", "").Replace("工程地点", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regChargeDept = new Regex(@"主管部门(：|:)[^\r\n]+\r\n");
                            pChargeDept = regChargeDept.Match(ctx).Value.Replace("主管部门", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regProClassLevel = new Regex(@"工程类别等级(：|:)[^\r\n]+\r\n");
                            pProClassLevel = regProClassLevel.Match(ctx).Value.Replace("工程类别等级", "").Replace(":", "").Replace("：", "").Trim(); 

                            Regex regPrjClass = new Regex(@"(工程类型|工程类别)(：|:)[^\r\n]+\r\n");
                            pPrjClass = regPrjClass.Match(ctx).Value.Replace("工程类别", "").Replace("工程类型", "").Replace("：", "").Replace(":", "").Trim();

                            Regex regBuildUnit = new Regex(@"(招标面积|本次招标面积)(：|:)[^\r\n]+\r\n");
                            pInviteArea = regBuildUnit.Match(ctx).Value.Replace("本次招标面积", "").Replace("招标面积", "").Replace("：", "").Replace(":", "").Trim();

                            Regex regpInviteArea = new Regex(@"建筑总面积(：|:)[^\r\n]+\r\n");
                            pBuildArea = regpInviteArea.Match(ctx).Value.Replace("建筑总面积", "").Replace(":", "").Replace("：", "").Trim();

                            pConstUnit = ctx.GetRegex("施工单位");
                            if (pConstUnit == "/")
                                pConstUnit = string.Empty;
                            pSuperUnit = ctx.GetRegex("监理单位");
                            if (pSuperUnit == "/")
                                pSuperUnit = string.Empty;
                            pDesignUnit = ctx.GetRegex("设计单位");
                            if (pDesignUnit == "/")
                                pDesignUnit = string.Empty;
                            pProspUnit = ctx.GetRegex("勘察单位");
                            if (pProspUnit == "/")
                                pProspUnit = string.Empty;

                            pBeginDate = dateStr.GetRegex("计划开工日期").GetDateRegex();
                            pEndDate = dateStr.GetRegex("计划竣工日期").GetDateRegex(); 

                            BaseProject info = ToolDb.GenBaseProject("广东省", pUrl, "深圳市龙岗区", pInfoSource, pBuilTime, pBeginDate, pEndDate, pConstUnit, pSuperUnit, pDesignUnit, pProspUnit, pInviteArea,
                                pBuildArea, pPrjClass, pProClassLevel, pChargeDept, pPrjAddress, pBuildUnit, pPrjCode, PrjName, pCreatetime, "深圳市龙岗区住房和建设局");

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
