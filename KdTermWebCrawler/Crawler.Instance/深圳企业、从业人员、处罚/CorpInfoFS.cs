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
using System.Data;
using System.Data.SqlClient;

namespace Crawler.Instance
{
    public class CorpInfoFS : WebSiteCrawller
    {
        public CorpInfoFS()
            : base()
        {
            this.Group = "企业信息";
            this.Title = "广东省佛山市住房和城乡建设管理局";
            this.Description = "自动抓取广东省佛山市住房和城乡建设管理局企业信息";
            this.PlanTime = "2 23:30,9 23:30,17 23:30,25 23:30";
            this.MaxCount = 5000;
            this.ExistCompareFields = "Url";
            this.SiteUrl = "http://119.145.135.38/fscx/web/tab3List.do?tab=2&kind=zzxx";
            this.ExistsUpdate = true;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ContentPlaceHolder1_aspnetPager1")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList[0].ToPlainTextString().GetRegexBegEnd("/", "页"); ;
                    pageInt = int.Parse(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(
                        new string[] { "searchStr", "currentPage", "pageSize", "tab", "kind" },
                        new string[] { string.Empty, i.ToString(), "15", "2", "zzxx" }
                        );
                    html = ToolWeb.GetHtmlByUrl("http://119.145.135.38/fscx/web/tab3List.do", nvc, Encoding.Default);
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "data-table2")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = null;
                    if (nodeList.Count > 1) table = nodeList[1] as TableTag;
                    else table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string CorpName = string.Empty, CorpCode = string.Empty, CorpAddress = string.Empty,
                                            RegDate = string.Empty, RegFund = string.Empty, BusinessCode = string.Empty,
                                            BusinessType = string.Empty, LinkMan = string.Empty, LinkPhone = string.Empty,
                                            Fax = string.Empty, Email = string.Empty, CorpSite = string.Empty, cUrl = string.Empty,
                                            ISOQualNum = string.Empty, ISOEnvironNum = string.Empty, CorpType = string.Empty;
                        TableRow tr = table.Rows[j];
                        CorpName = tr.Columns[0].ToNodePlainString();
                        CorpType = tr.Columns[1].ToNodePlainString();
                        Regex regexLink = new Regex(@"\?id=[^&]+");
                        string temp = tr.GetAttribute("onclick").GetRegexBegEnd("'", "'");
                        string ids = regexLink.Match(temp).Value;
                        cUrl = "http://119.145.135.38/fscx/web/tab3Detail.do" + ids;
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = ToolWeb.GetHtmlByUrl(cUrl, Encoding.Default);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "tabs-1")), true), new TagNameFilter("table")));
                        if (dtList != null && dtList.Count > 0)
                        {
                            string ctx = string.Empty;
                            TableTag tab = dtList[0] as TableTag;
                            for (int d = 0; d < tab.RowCount; d++)
                            {
                                for (int k = 0; k < tab.Rows[d].ColumnCount; k++)
                                {
                                    if ((k + 1) % 2 == 0)
                                        ctx += tab.Rows[d].Columns[k].ToNodePlainString() + "\r\n";
                                    else
                                        ctx += tab.Rows[d].Columns[k].ToNodePlainString() + "：";
                                }
                            }

                            CorpCode = ctx.GetRegex("组织机构代码,机构代码");
                            BusinessCode = ctx.GetRegex("营业执照注册号");
                            BusinessType = ctx.GetRegex("注册经济类别");
                            RegFund = ctx.GetRegex("注册资本(万元),注册资本,注册资金", false).Replace("(万元)", "").Replace("(万)", "").Replace("万元", "").Replace("万", "");
                            RegDate = ctx.GetRegex("成立日期,成立时间,设立日期,设立时间");
                            CorpAddress = ctx.GetRegex("注册地址");
                            LinkMan = ctx.GetRegex("法定代表人,联系人");
                            LinkPhone = ctx.GetRegex("联系电话");
                            Fax = ctx.GetRegex("传真");
                            Email = ctx.GetRegex("电子邮箱");
                            CorpSite = ctx.GetRegex("企业网址");
                            if (RegDate.Contains("000"))
                            {
                                RegDate = "";
                            }
                            if (!RegFund.Contains("万"))
                            {
                                RegFund += "万";
                            }
                            CorpInfo info = ToolDb.GenCorpInfo(CorpName, CorpCode, CorpAddress, RegDate, RegFund, BusinessCode, BusinessType, LinkMan, LinkPhone, Fax, Email, CorpSite, CorpType, "广东省", "佛山市", "佛山市住房和城乡建设管理局", cUrl, ISOQualNum, ISOEnvironNum, string.Empty);

                            string strSql = string.Format("select Id from CorpInfo where CorpName='{0}' and InfoSource='{1}'", info.CorpName, info.InfoSource);
                            object obj = ToolDb.ExecuteScalar(strSql);
                            if (obj != null && obj.ToString() != "")
                            {
                                StringBuilder delCorpQual = new System.Text.StringBuilder();
                                StringBuilder delCorpResults = new System.Text.StringBuilder();
                                StringBuilder delCorpSecLic = new System.Text.StringBuilder();
                                StringBuilder delCorpPunish = new StringBuilder();
                                delCorpQual.AppendFormat("delete from CorpQual where CorpId='{0}'", obj);
                                delCorpResults.AppendFormat("delete from CorpResults where CorpId='{0}'", obj);
                                delCorpSecLic.AppendFormat("delete from CorpSecLic where CorpId='{0}'", obj);
                                delCorpPunish.AppendFormat("delete from CorpPunish where CorpId='{0}'", obj);
                                ToolDb.ExecuteSql(delCorpQual.ToString());
                                ToolDb.ExecuteSql(delCorpResults.ToString());
                                ToolDb.ExecuteSql(delCorpSecLic.ToString());
                                ToolDb.ExecuteSql(delCorpPunish.ToString());
                                string corpSql = string.Format("delete from CorpInfo where Id='{0}'", obj);
                                ToolCoreDb.ExecuteSql(corpSql);
                            }

                            if (ToolDb.SaveEntity(info, string.Empty))
                            {
                                parser = new Parser(new Lexer(htldtl));
                                NodeList qualList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "tabs1-1")), true), new TagNameFilter("table")));
                                if (qualList != null && qualList.Count > 0)
                                {
                                    AddQual(qualList[0] as TableTag, info.Id, info.Url);
                                }

                                parser = new Parser(new Lexer(htldtl));
                                NodeList secLicList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "tabs5-1")), true), new TagNameFilter("table")));
                                if (secLicList != null && secLicList.Count > 0)
                                {
                                    AddCorpSecLic(secLicList[0] as TableTag, info.Id, info.Url);
                                }

                                parser = new Parser(new Lexer(htldtl));
                                NodeList resultsList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "tabs2-1")), true), new TagNameFilter("table")));
                                if (resultsList != null && resultsList.Count > 0)
                                {
                                    AddCorpResults(resultsList[0] as TableTag, info.Id, info.Url);
                                }

                                parser = new Parser(new Lexer(htldtl));
                                NodeList PunishList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "tabs7-1")), true), new TagNameFilter("table")));
                                if (PunishList != null && PunishList.Count > 0)
                                {
                                    AddCorpPunish(PunishList[0] as TableTag, info.Id, info.Url);
                                }

                            }
                        }
                    }
                }
            }
            ToolCoreDb.ExecuteProcedure();
            return null;
        }

        /// <summary>
        /// 保存资质
        /// </summary>
        /// <param name="noList"></param>
        private void AddQual(TableTag table, string id, string url)
        {
            for (int i = 1; i < table.RowCount; i++)
            {
                TableRow tr = table.Rows[i];
                string CorpId = string.Empty, QualName = string.Empty, QualCode = string.Empty, QualSeq = string.Empty, qualNum = string.Empty, QualType = string.Empty, QualLevel = string.Empty, ValidDate = string.Empty, LicDate = string.Empty, LicUnit = string.Empty;

                QualCode = tr.Columns[0].ToNodePlainString();
                QualName = QualType = tr.Columns[1].ToNodePlainString();
                QualLevel = tr.Columns[2].ToNodePlainString();
                ValidDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
                LicDate = tr.Columns[4].ToPlainTextString().GetDateRegex();
                LicUnit = tr.Columns[5].ToNodePlainString();
                qualNum = QualLevel.GetLevel();

                CorpQual qual = ToolDb.GenCorpQual(id, QualName, QualCode, QualSeq, QualType, QualLevel, ValidDate, LicDate, LicUnit, url, qualNum, "广东省", "佛山市");
                ToolDb.SaveEntity(qual, string.Empty);
            }
        }

        /// <summary>
        /// 保存安全生产许可
        /// </summary>
        /// <param name="table"></param>
        /// <param name="id"></param>
        /// <param name="url"></param>
        private void AddCorpSecLic(TableTag table, string id, string url)
        {
            for (int i = 1; i < table.RowCount; i++)
            {
                TableRow tr = table.Rows[i];
                string SecLicCode = string.Empty, SecLicDesc = string.Empty, ValidStartDate = string.Empty, ValidStartEnd = string.Empty, SecLicUnit = string.Empty;
                SecLicCode = tr.Columns[0].ToNodePlainString();
                ValidStartEnd = tr.Columns[1].ToPlainTextString().GetDateRegex();
                SecLicUnit = tr.Columns[2].ToNodePlainString();
                ValidStartDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
                if (string.IsNullOrEmpty(SecLicCode) && string.IsNullOrEmpty(ValidStartEnd) && string.IsNullOrEmpty(SecLicUnit) && string.IsNullOrEmpty(ValidStartDate))
                {
                    continue;
                }
                CorpSecLic seclic = ToolDb.GenCorpSecLic(id, SecLicCode, SecLicDesc, ValidStartDate, ValidStartEnd, SecLicUnit, url);
                ToolDb.SaveEntity(seclic, string.Empty);
            }
        }

        /// <summary>
        /// 保存企业业绩
        /// </summary>
        /// <param name="table"></param>
        /// <param name="id"></param>
        /// <param name="url"></param>
        private void AddCorpResults(TableTag table, string id, string url)
        {
            for (int i = 1; i < table.RowCount; i++)
            {
                string PrjName = string.Empty, PrjCode = string.Empty, BuildUnit = string.Empty, GrantDate = string.Empty, PrjAddress = string.Empty, ChargeDept = string.Empty, PrjClassLevel = string.Empty, PrjClass = string.Empty, BuildArea = string.Empty, InviteArea = string.Empty, ProspUnit = string.Empty, DesignUnit = string.Empty, SuperUnit = string.Empty, ConstUnit = string.Empty, PrjStartDate = string.Empty, PrjEndDate = string.Empty;

                TableRow tr = table.Rows[i];
                PrjName = tr.Columns[1].ToNodePlainString();
                GrantDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                PrjStartDate = tr.Columns[5].ToPlainTextString().Replace("&nbsp;", "");
                if (string.IsNullOrEmpty(PrjName) && string.IsNullOrEmpty(GrantDate) && string.IsNullOrEmpty(PrjStartDate))
                {
                    continue;
                }
                CorpResults result = ToolDb.GenCorpResults(id, PrjName, PrjCode, BuildUnit, GrantDate, PrjAddress, ChargeDept, PrjClassLevel, PrjClass, BuildArea, InviteArea, ProspUnit, DesignUnit, SuperUnit, ConstUnit, PrjStartDate, PrjEndDate, url);

                ToolDb.SaveEntity(result, string.Empty);
            }
        }

        /// <summary>
        /// 保存行政处罚
        /// </summary>
        /// <param name="table"></param>
        /// <param name="id"></param>
        /// <param name="url"></param>
        private void AddCorpPunish(TableTag table, string id, string url)
        {
            for (int i = 1; i < table.RowCount; i++)
            {
                string DocNo = string.Empty, PunishType = string.Empty, GrantUnit = string.Empty, DocDate = string.Empty, PunishCtx = string.Empty, IsShow = string.Empty;
                TableRow tr = table.Rows[i];
                PunishType = tr.Columns[0].ToNodePlainString();
                PunishCtx = tr.Columns[4].ToPlainTextString().Replace("&nbsp;", "");
                DocDate = tr.Columns[5].ToPlainTextString().GetDateRegex();
                GrantUnit = tr.Columns[7].ToNodePlainString();
                if (string.IsNullOrEmpty(PunishType) && string.IsNullOrEmpty(PunishCtx) && string.IsNullOrEmpty(DocDate) && string.IsNullOrEmpty(GrantUnit))
                {
                    continue;
                }
                CorpPunish punish = ToolDb.GenCorpPunish(id, DocNo, PunishType, GrantUnit, DocDate, PunishCtx, url, "0");
                ToolDb.SaveEntity(punish, string.Empty);
            }
        }
    }
}
