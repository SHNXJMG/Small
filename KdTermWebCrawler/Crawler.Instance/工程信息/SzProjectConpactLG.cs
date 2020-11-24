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
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class SzProjectConpactLG : WebSiteCrawller
    {
        public SzProjectConpactLG()
            : base()
        {
            this.Group = "工程信息";
            this.PlanTime = "03:20";
            this.Title = "深圳市龙岗区建设局合同备案基本信息";
            this.MaxCount = 50;
            this.Description = "自动抓取深圳市龙岗区建设局合同备案基本信息";
            this.ExistCompareFields = "PrjName,ContUnit,CompactType,RecordDate";
            this.SiteUrl = "http://www.cb.gov.cn/order/list/htbaOrderList.jsp?itemId=226571&curId=226572&listId=226579";
            this.ExistsUpdate = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ProjectConpact>();
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
                    catch { }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table1")));
                if (tableNodeList.Count > 0 || tableNodeList != null)
                {
                    TableTag table = tableNodeList[tableNodeList.Count-1] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string pProvince = string.Empty, pUrl = string.Empty, pCity = string.Empty, pSubcontractCode = string.Empty, pSubcontractName = string.Empty, pSubcontractCompany = string.Empty, pInfoSource = string.Empty, pRecordDate = string.Empty, pCompactPrice = string.Empty, pCompactType = string.Empty, pBuildUnit = string.Empty, pPrjCode = string.Empty, PrjName = string.Empty, pPrjMgrQual = string.Empty, pPrjMgrName = string.Empty, pContUnit = string.Empty, pCreatetime = string.Empty;
                        TableRow tr = table.Rows[j];
                        pBuildUnit = tr.Columns[1].ToPlainTextString().Trim();
                        pContUnit = tr.Columns[2].ToPlainTextString().Trim();
                        pCompactType = tr.Columns[3].ToPlainTextString().Trim();
                        pRecordDate = tr.Columns[4].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                        pUrl = "http://www.cb.gov.cn" + aTag.Link.Replace("GoDetail('", "").Replace("');", "");
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(pUrl), Encoding.UTF8).Replace("<br/>", "\r\n").Trim();
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htmldetail));
                        NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "sub_detailed")));
                        if (dtList != null && dtList.Count > 0)
                        { 
                            parser = new Parser(new Lexer(dtList.AsHtml()));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag tag = tableNode[0] as TableTag;
                                for (int r = 0; r < tag.RowCount; r++)
                                {
                                    for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = tag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                        if ((c + 1) % 2 == 0)
                                            pInfoSource += temp + "\r\n";
                                        else
                                            pInfoSource += temp + "：";
                                    }
                                }
                            }
                            PrjName = pInfoSource.GetRegex("工程名称"); 
                            pSubcontractCode = pInfoSource.GetRegex("分包工程编号"); 
                            pSubcontractName = pInfoSource.GetRegex("分包工程名称"); 
                            pSubcontractCompany = pInfoSource.GetRegex("分包工程发包单位");
                            pCompactPrice = pInfoSource.GetRegex("合同价款"); 
                            pPrjMgrQual = pInfoSource.GetRegex("项目经理资格"); 
                            pPrjMgrName = pInfoSource.GetRegex("项目经理名称"); 

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
                            ProjectConpact info = ToolDb.GenProjectConpact("广东省", pUrl, "深圳市龙岗区", pSubcontractCode, pSubcontractName, pSubcontractCompany, pInfoSource, pRecordDate, pCompactPrice, pCompactType, pBuildUnit, pPrjCode, PrjName, pPrjMgrQual, pPrjMgrName, pContUnit, pCreatetime, "深圳市龙岗区住房和建设局");
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
