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

namespace Crawler.Instance
{
    public class ProjectInfoSzJsZhiJie:WebSiteCrawller
    {
        public ProjectInfoSzJsZhiJie()
        :base()
        {
            this.Group = "工程信息";
            this.PlanTime = "12:06,03:25";
            this.Title = "深圳市住房与建设局工程基本信息（2014新版直接发包）";
            this.Description = "自动抓取深圳市住房与建设局工程基本信息（2014新版直接发包）";
            this.ExistCompareFields = "PrjCode";
            this.SiteUrl = "http://61.144.226.2:8001/web/zjfbAction.do?method=getZjfbList";
            this.MaxCount = 100000;
            this.ExistsUpdate = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BaseProject>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("a"), new HasAttributeFilter("id", "lx")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.GetATagHref().GetRegexBegEnd("page=", "&");
                    pageInt = int.Parse(temp);
                }
                catch
                {
                }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i.ToString(), Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "bean")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string pUrl = string.Empty, pInfoSource = string.Empty,
                           pBeginDate = string.Empty, pBuilTime = string.Empty,
                           pEndDate = string.Empty, pConstUnit = string.Empty,
                           pSuperUnit = string.Empty, pDesignUnit = string.Empty,
                           pProspUnit = string.Empty, pInviteArea = string.Empty,
                           pBuildArea = string.Empty, pPrjClass = string.Empty,
                           pProClassLevel = string.Empty, pChargeDept = string.Empty,
                           pPrjAddress = string.Empty, pBuildUnit = string.Empty,
                           pPrjCode = string.Empty, PrjName = string.Empty, pCreatetime = string.Empty;

                        TableRow tr = table.Rows[j];
                        pPrjCode = tr.Columns[1].GetATag().LinkText;
                        PrjName = tr.Columns[2].ToNodePlainString();
                        pBuildUnit = tr.Columns[3].ToNodePlainString();
                        pBuilTime = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        pUrl = "http://61.144.226.2:8001/web/zjfbAction.do?method=view&id=" + pPrjCode;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(pUrl, Encoding.Default);
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htmldtl.Replace("th", "td")));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "infoTableL")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            TableTag dtlTable = dtlNode[0] as TableTag;
                            for (int k = 0; k < dtlTable.RowCount; k++)
                            {
                                for (int d = 0; d < dtlTable.Rows[k].ColumnCount; d++)
                                {
                                    string temp = dtlTable.Rows[k].Columns[d].ToNodePlainString().Replace("：", "").Replace(":", "");
                                    if (d == 0)
                                        pInfoSource += temp += "：";
                                    else
                                        pInfoSource += temp += "\r\n";
                                }
                            }

                            pPrjAddress = pInfoSource.GetRegex("工程地址,工程地点");
                            pChargeDept = pInfoSource.GetRegex("主管部门");
                            pProClassLevel = pInfoSource.GetRegex("工程类别等级");
                            pPrjClass = pInfoSource.GetRegex("工程类别");
                            pInviteArea = pInfoSource.GetRegex("招标面积,本次招标面积");
                            pBuildArea = pInfoSource.GetRegex("建筑总面积");
                            pProspUnit = pInfoSource.GetRegex("勘察单位");
                            pDesignUnit = pInfoSource.GetRegex("设计单位");
                            pSuperUnit = pInfoSource.GetRegex("监理单位");
                            pBeginDate = pInfoSource.GetRegex("计划开工日期");
                            pEndDate = pInfoSource.GetRegex("计划竣工日期");

                            BaseProject info = ToolDb.GenBaseProject("广东省", pUrl, "深圳市区", pInfoSource, pBuilTime, pBeginDate, pEndDate, pConstUnit, pSuperUnit, pDesignUnit, pProspUnit, pInviteArea,
                                pBuildArea, pPrjClass, pProClassLevel, pChargeDept, pPrjAddress, pBuildUnit, pPrjCode, PrjName, pCreatetime, "深圳市住房和建设局");

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
