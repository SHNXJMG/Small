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
using System.Threading;

namespace Crawler.Instance
{
    public class ProjectLicSzJs : WebSiteCrawller
    {
        public ProjectLicSzJs()
            : base()
        {
            this.Group = "工程信息";
            this.PlanTime = "12:04,03:30";
            this.Title = "深圳市住房和建设局施工许可信息（2014新版）";
            this.Description = "自动抓取深圳市住房和建设局施工许可信息（2014新版）";
            this.ExistCompareFields = "PrjCode";
            this.MaxCount = 100000;
            this.SiteUrl = "http://portal.szjs.gov.cn:8888/gongshi/sgxkList.html";
            this.ExistsUpdate = true;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            
            IList list = new List<ProjectLic>();
            int pageInt = 1,count=0;
            string htl = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty; 
            try
            { 
                htl = ToolHtml.GetHtmlByUrlEncode(SiteUrl, Encoding.UTF8); 
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "pageLinkTd")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                try
                {
                    string temp = tdNodes.AsString().ToNodeString();
                    string s = temp.GetRegexBegEnd("总页数", "页").Replace(":","");
                    pageInt = int.Parse(s);
                }
                catch (Exception ex) { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]
                    {
                        "page",
                        "qymc",
                        "ann_serial",
                        "pro_name"

                    }, new string[] {
                        i.ToString(),
                        "",
                        "",
                        ""
                    });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "tblPrjConstBid")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = (TableTag)listNode[0];
                    for (int j = 1; j < table.RowCount-1; j++)
                    {

                        string pPrjName = string.Empty, pBuildUnit = string.Empty,
                          pBuildAddress = string.Empty, pBuildManager = string.Empty,
                          pBuildScale = string.Empty, pPrjPrice = string.Empty,
                          pPrjStartDate = string.Empty, PrjEndDate = string.Empty,
                          pConstUnit = string.Empty, pConstUnitManager = string.Empty,
                          pSuperUnit = string.Empty, pSuperUnitManager = string.Empty,
                          pProspUnit = string.Empty, pProspUnitManager = string.Empty,
                          pDesignUnit = string.Empty, pDesignUnitManager = string.Empty,
                          pPrjManager = string.Empty, pSpecialPerson = string.Empty,
                          pLicUnit = string.Empty, pPrjLicCode = string.Empty,
                          PrjLicDate = string.Empty, pPrjDesc = string.Empty,
                          pProvince = string.Empty, pCity = string.Empty,
                          pInfoSource = string.Empty, pUrl = string.Empty,
                          pCreatetime = string.Empty, pPrjCode = string.Empty;
                        TableRow tr = table.Rows[j];
                        pPrjLicCode = tr.Columns[0].ToNodePlainString();
                        pPrjCode = tr.Columns[1].ToNodePlainString();
                        pPrjName = tr.Columns[2].ToNodePlainString();
                        pBuildUnit = tr.Columns[3].ToNodePlainString();
                        PrjLicDate = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        pUrl = "http://portal.szjs.gov.cn:8888/gongshi/sgxkz.html";
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "instanceGuid", "yxtywlsh" }, new string[] { pPrjCode, pPrjLicCode });

                        string htmldetl = string.Empty;
                        try
                        {
                            htmldetl = this.ToolWebSite.GetHtmlByUrl(pUrl,nvc, Encoding.UTF8);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetl));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "tblPrjConstBid")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            TableTag dtlTag = dtnode[0] as TableTag;
                            pInfoSource = "";
                            for (int rowIndex = 1; rowIndex < dtlTag.RowCount; rowIndex++)
                            {
                                for (int colIndex = 0; colIndex < dtlTag.Rows[rowIndex].ColumnCount; colIndex++)
                                {
                                    if (colIndex % 2 == 0)
                                    {
                                        pInfoSource += dtlTag.Rows[rowIndex].Columns[colIndex].ToNodePlainString() + "：";
                                    }
                                    else
                                    {
                                        pInfoSource += dtlTag.Rows[rowIndex].Columns[colIndex].ToNodePlainString() + "\r\n";
                                    }
                                }
                            }

                            pPrjStartDate = pInfoSource.GetRegex("合同开工日期");
                            PrjEndDate = pInfoSource.GetRegex("合同竣工日期");
                            pDesignUnit = pInfoSource.GetRegex("设计单位,建设单位 ");
                            pBuildAddress = pInfoSource.GetRegex("工程地址,建设地址");
                            pBuildScale = pInfoSource.GetRegex("建筑面积,建设规模");
                            pSuperUnit = pInfoSource.GetRegex("监理单位");
                            pConstUnit = pInfoSource.GetRegex("施工单位");
                            pLicUnit = pInfoSource.GetRegex("发证机关");
                            pProspUnit = pInfoSource.GetRegex("勘察单位");
                            pPrjPrice = pInfoSource.GetRegex("合同价格");
                            pPrjManager = pInfoSource.GetRegex("项目经理,项目负责人");
                            if (string.IsNullOrEmpty(pLicUnit))
                            {
                                pLicUnit = "深圳市住房和建设局";
                            }
                            ProjectLic info = ToolDb.GenProjectLic(pPrjName, pBuildUnit, pBuildAddress, pBuildManager, pBuildScale, pPrjPrice, pPrjStartDate, PrjEndDate, pConstUnit, pConstUnitManager, pSuperUnit, pSuperUnitManager, pProspUnit, pProspUnitManager, pDesignUnit, pDesignUnitManager, pPrjManager, pSpecialPerson, pLicUnit, pPrjLicCode, PrjLicDate, pPrjDesc, "广东省", "深圳市区", pInfoSource, pUrl, pCreatetime, pPrjCode, "深圳市住房和建设局");
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount)
                                return list;

                            count++;
                            if (count >= 200)
                            {
                                count = 1;
                                Thread.Sleep(600 * 1000);
                            }
                        }
                    }
                } 
            }
            return list;
        }
    }
}