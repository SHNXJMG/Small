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
using System.Threading;

namespace Crawler.Instance
{
    public class SzCorpInfoJZSJSG : WebSiteCrawller
    {
        public SzCorpInfoJZSJSG()
            : base()
        {
            this.PlanTime = "4 23:30,11 23:30,19 23:30,27 23:30";//"1-28 2:00,4-28 2:00,7-28 2:00,10-28 2:00";
            this.Group = "企业信息";
            this.Title = "深圳市建设局企业信息（设计与施工一体化企业）";
            this.Description = "自动抓取深圳市建设局企业信息（设计与施工一体化企业）";
            this.ExistCompareFields = "CorpType,CorpName";
            this.MaxCount = 50000;
            this.SiteUrl = "http://61.144.226.2:8001/web/enterprs/unitInfoAction.do?method=toList&certType=24";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            int count = 1, totalCount = 1;
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            string pageHtl = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("a"), new HasAttributeFilter("id", "lx")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.GetATagHref().GetRegexBegEnd("page=", "&");
                    pageInt = int.Parse(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(this.SiteUrl + "&page=" + i.ToString(), Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "bean")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string CorpName = string.Empty, CorpCode = string.Empty, CorpAddress = string.Empty,
                                  RegDate = string.Empty, RegFund = string.Empty, BusinessCode = string.Empty,
                                  BusinessType = string.Empty, LinkMan = string.Empty, LinkPhone = string.Empty,
                                  Fax = string.Empty, Email = string.Empty, CorpSite = string.Empty, cUrl = string.Empty,
                                  ISOQualNum = string.Empty, ISOEnvironNum = string.Empty, OffAdr = string.Empty, Cert = string.Empty, ctxKc = string.Empty,
                                  corpRz = string.Empty;

                        TableRow tr = table.Rows[j];
                        CorpName = tr.Columns[1].ToNodePlainString();
                        CorpCode = tr.Columns[2].ToNodePlainString();
                        LinkMan = tr.Columns[3].ToNodePlainString();
                        string href = tr.Columns[1].GetATagValue("onclick");
                        string htmldtl = string.Empty;
                        string[] url = null;
                        try
                        {
                            string temp = href.Replace("doView", "").Replace("(", "").Replace(")", "").Replace("'", "");
                            url = temp.Split(',');
                            cUrl = "http://61.144.226.2:8001/web/enterprs/unitInfoAction.do?method=toView&qybh=" + url[0] + "&certType=1&orgcode=" + url[1];
                            htmldtl = ToolWeb.GetHtmlByUrl(cUrl, Encoding.Default);
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htmldtl.Replace("th", "td")));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "infoTableL")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            TableTag tabledtl = dtlNode[0] as TableTag;
                            string ctx = string.Empty;
                            for (int d = 0; d < tabledtl.RowCount; d++)
                            {
                                for (int k = 0; k < tabledtl.Rows[d].ColumnCount; k++)
                                {
                                    string temp = tabledtl.Rows[d].Columns[k].ToNodePlainString();
                                    if (k == 0)
                                        ctx += temp + "：";
                                    else
                                        ctx += temp + "\r\n";
                                }
                            }
                            LinkPhone = ctx.GetRegex("联系电话");
                            Fax = ctx.GetRegex("传真");
                            Email = ctx.GetRegex("电子邮箱");
                            CorpAddress = ctx.GetRegex("注册地址");
                            RegFund = ctx.GetRegex("注册资金");
                            RegDate = ctx.GetRegex("设立时间");
                        }

                        CorpInfo info = ToolDb.GenCorpInfo(CorpName, CorpCode, CorpAddress, RegDate, RegFund, BusinessCode, BusinessType, LinkMan, LinkPhone, Fax, Email, CorpSite, "设计与施工一体化企业", "广东省", "深圳市", "深圳市住房和建设局", cUrl, ISOQualNum, ISOEnvironNum, OffAdr);

                        object obj = ToolDb.ExecuteScalar(string.Format("select Id from CorpInfo where CorpName='{0}' and CorpType='{1}' and InfoSource='{2}'", info.CorpName, info.CorpType, info.InfoSource));
                        int qualCount = 0, leaderCount = 0, awardCount = 0, certCount = 0, punishCount = 0, seclicCount = 0, seclicstaffCount = 0, tecstaffCount = 0, deviceCount = 0, resultCount = 0, infoCount = 0;
                        bool isDel = false;
                        if (obj != null && obj.ToString() != "")
                        {
                            isDel = true;
                            string id = obj.ToString();
                            StringBuilder delCorpQual = new System.Text.StringBuilder();
                            StringBuilder delCorpLeader = new System.Text.StringBuilder();
                            StringBuilder delCorpAward = new System.Text.StringBuilder();
                            StringBuilder delCorpCert = new System.Text.StringBuilder();
                            StringBuilder delCorpPunish = new System.Text.StringBuilder();
                            StringBuilder delCorpSecLic = new System.Text.StringBuilder();
                            StringBuilder delCorpSecLicStaff = new System.Text.StringBuilder();
                            StringBuilder delCorpDevice = new System.Text.StringBuilder();
                            StringBuilder delCorpResults = new System.Text.StringBuilder();
                            StringBuilder delCorpTecStaff = new System.Text.StringBuilder();
                            delCorpQual.AppendFormat("delete from CorpQual where CorpId='{0}'", id);
                            delCorpLeader.AppendFormat("delete from CorpLeader where CorpId='{0}'", id);
                            delCorpAward.AppendFormat("delete from CorpAward where CorpId='{0}'", id);
                            delCorpCert.AppendFormat("delete from CorpCert where CorpId='{0}'", id);
                            delCorpPunish.AppendFormat("delete from CorpPunish where CorpId='{0}'", id);
                            delCorpSecLic.AppendFormat("delete from CorpSecLic where CorpId='{0}'", id);
                            delCorpSecLicStaff.AppendFormat("delete from CorpSecLicStaff where CorpId='{0}'", id);
                            delCorpTecStaff.AppendFormat("delete from CorpTecStaff where CorpId='{0}'", id);
                            delCorpDevice.AppendFormat("delete from CorpDevice where CorpId='{0}'", id);
                            delCorpResults.AppendFormat("delete from CorpResults where CorpId='{0}'", id);
                            qualCount = ToolCoreDb.ExecuteSql(delCorpQual.ToString());
                            leaderCount = ToolCoreDb.ExecuteSql(delCorpLeader.ToString());
                            awardCount = ToolCoreDb.ExecuteSql(delCorpAward.ToString());
                            certCount = ToolCoreDb.ExecuteSql(delCorpCert.ToString());
                            punishCount = ToolCoreDb.ExecuteSql(delCorpPunish.ToString());
                            seclicCount = ToolCoreDb.ExecuteSql(delCorpSecLic.ToString());
                            seclicstaffCount = ToolCoreDb.ExecuteSql(delCorpSecLicStaff.ToString());
                            tecstaffCount = ToolCoreDb.ExecuteSql(delCorpTecStaff.ToString());
                            deviceCount = ToolCoreDb.ExecuteSql(delCorpDevice.ToString());
                            resultCount = ToolCoreDb.ExecuteSql(delCorpResults.ToString());
                            string corpSql = string.Format("delete from CorpInfo where Id='{0}'", id);
                            infoCount = ToolCoreDb.ExecuteSql(corpSql);
                        }
                        if (infoCount != -1 || !isDel)
                        {
                            if (ToolDb.SaveEntity(info, string.Empty))
                            {
                                if (isDel)
                                {
                                    if (qualCount != -1)
                                        AddCorpQual(info, htmldtl);
                                    if (awardCount != -1)
                                        AddCorpAward(info, htmldtl);
                                    if (certCount != -1)
                                        AddCorpCert(info, htmldtl);
                                    if (deviceCount != -1)
                                        AddCorpDevice(info, htmldtl);
                                    if (punishCount != -1)
                                        AddCorpPunish(info, htmldtl);
                                    if (resultCount != -1)
                                        AddCorpResults(info, htmldtl);
                                    if (seclicCount != -1)
                                        AddCorpSecLic(info, htmldtl);
                                    if (seclicstaffCount != -1)
                                        AddCorpSecLicStaff(info, htmldtl);
                                    if (tecstaffCount != -1)
                                        AddCorpTecStaff(info, htmldtl);
                                    if (leaderCount != -1)
                                        AddCorpLeader(info, htmldtl);
                                }
                                else
                                {
                                    AddCorpQual(info, htmldtl);
                                    AddCorpAward(info, htmldtl);
                                    AddCorpCert(info, htmldtl);
                                    AddCorpDevice(info, htmldtl);
                                    AddCorpPunish(info, htmldtl);
                                    AddCorpResults(info, htmldtl);
                                    AddCorpSecLic(info, htmldtl);
                                    AddCorpSecLicStaff(info, htmldtl);
                                    AddCorpTecStaff(info, htmldtl);
                                    AddCorpLeader(info, htmldtl);
                                }

                            }
                        }
                        count++;
                        totalCount++;
                        if (count >= 90)
                        {
                            count = 1;
                            Thread.Sleep(700000);
                        } 
                    }

                }
            }
            ToolCoreDb.ExecuteProcedure();
            string sql = "update a set a.FkId= c.Id FROM AttenCorp  a left join  CorpInfo c on c.CorpName=A.CorpName";
            ToolDb.ExecuteSql(sql);
            return null;
        }

        /// <summary>
        /// 企业负责人信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        public void AddCorpLeader(CorpInfo info, string html)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "fzrxx")));
            if (nodeList != null && nodeList.Count > 0)
            {
                parser = new Parser(new Lexer(nodeList.ToHtml().Replace("th", "td")));
                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                if (tableNode != null && tableNode.Count > 0)
                {
                    TableTag table = tableNode[0] as TableTag;
                    for (int i = 1; i < table.RowCount; i++)
                    {
                        TableRow tr = table.Rows[i];
                        if (tr.Columns[0].ToPlainTextString().Contains("没有显示结果"))
                            break;

                        string LeaderName = string.Empty, LeaderDuty = string.Empty, LeaderType = string.Empty, htlCtx = string.Empty, phone = string.Empty;
                        LeaderName = tr.Columns[1].ToNodePlainString();
                        LeaderDuty = tr.Columns[2].ToNodePlainString();
                        phone = tr.Columns[3].ToNodePlainString();
                        LeaderType = tr.Columns[4].ToNodePlainString();
                        CorpLeader corpLeader = ToolDb.GenCorpLeader(info.Id, LeaderName, LeaderDuty, LeaderType, info.Url, phone);
                        ToolDb.SaveEntity(corpLeader, string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 企业认证信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpCert(CorpInfo info, string html)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "rzxx")));
            if (nodeList != null && nodeList.Count > 0)
            {
                parser = new Parser(new Lexer(nodeList.ToHtml().Replace("th", "td")));
                NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("tbody"), new HasAttributeFilter("id", "rzInfo")));
                if (dtlNode != null && dtlNode.Count > 0)
                {
                    string temp = dtlNode[0].ToNodePlainString();
                    if (temp.Contains("没有显示结果") || string.IsNullOrEmpty(temp))
                        return;
                    CorpCert cert = ToolDb.GenCorpCert(info.Id, temp, info.Url);
                    ToolDb.SaveEntity(cert, string.Empty);
                }
            }
        }

        /// <summary>
        /// 企业技术力量
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpTecStaff(CorpInfo info, string html)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "jsll")));
            if (nodeList != null && nodeList.Count > 0)
            {
                parser = new Parser(new Lexer(nodeList.ToHtml().Replace("th", "td")));
                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                if (tableNode != null && tableNode.Count > 0)
                {
                    TableTag table = tableNode[0] as TableTag;
                    for (int i = 1; i < table.RowCount; i++)
                    {
                        TableRow tr = table.Rows[i];
                        if (tr.Columns[0].ToPlainTextString().Contains("没有显示结果"))
                            break;
                        string StaffName = string.Empty, IdCard = string.Empty, CertLevel = string.Empty, CertNo = string.Empty, stffType = string.Empty;
                        StaffName = tr.Columns[1].ToNodePlainString();
                        stffType = tr.Columns[2].ToNodePlainString();
                        CertNo = tr.Columns[3].ToNodePlainString();

                        CorpTecStaff staff = ToolDb.GenCorpTecStaff(info.Id, StaffName, IdCard, CertLevel, CertNo, info.Url, stffType);
                        ToolDb.SaveEntity(staff, string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 企业机械设备
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpDevice(CorpInfo info, string html)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "jxsb")));
            if (nodeList != null && nodeList.Count > 0)
            {
                parser = new Parser(new Lexer(nodeList.ToHtml().Replace("th", "td")));
                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                if (tableNode != null && tableNode.Count > 0)
                {
                    TableTag table = tableNode[0] as TableTag;
                    for (int i = 1; i < table.RowCount; i++)
                    {
                        TableRow tr = table.Rows[i];
                        if (tr.Columns[0].ToPlainTextString().Contains("没有显示结果"))
                            break;
                        string DeviceName = string.Empty, DeviceSpec = string.Empty, DeviceCount = string.Empty;
                        DeviceName = tr.Columns[1].ToNodePlainString();
                        DeviceSpec = tr.Columns[2].ToNodePlainString();
                        DeviceCount = tr.Columns[3].ToNodePlainString();
                        CorpDevice device = ToolDb.GenCorpDevice(info.Id, DeviceName, DeviceSpec, DeviceCount, info.Url);
                        ToolDb.SaveEntity(device, string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 企业业绩
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpResults(CorpInfo info, string html)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "yjxx")));
            if (nodeList != null && nodeList.Count > 0)
            {
                parser = new Parser(new Lexer(nodeList.ToHtml().Replace("th", "td")));
                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                if (tableNode != null && tableNode.Count > 0)
                {
                    TableTag table = tableNode[0] as TableTag;
                    for (int i = 1; i < table.RowCount; i++)
                    {
                        TableRow tr = table.Rows[i];
                        if (tr.Columns[0].ToPlainTextString().Contains("没有显示结果"))
                            break;

                        string PrjName = string.Empty, PrjCode = string.Empty, BuildUnit = string.Empty, GrantDate = string.Empty, PrjAddress = string.Empty, ChargeDept = string.Empty, PrjClassLevel = string.Empty, PrjClass = string.Empty, BuildArea = string.Empty, InviteArea = string.Empty, ProspUnit = string.Empty, DesignUnit = string.Empty, SuperUnit = string.Empty, ConstUnit = string.Empty, PrjStartDate = string.Empty, PrjEndDate = string.Empty;

                        PrjName = tr.Columns[2].ToNodePlainString();
                        PrjCode = tr.Columns[1].ToNodePlainString();
                        BuildUnit = tr.Columns[3].ToNodePlainString();
                        GrantDate = tr.Columns[4].ToPlainTextString().GetDateRegex();

                        CorpResults result = ToolDb.GenCorpResults(info.Id, PrjName, PrjCode, BuildUnit, GrantDate, PrjAddress, ChargeDept, PrjClassLevel, PrjClass, BuildArea, InviteArea, ProspUnit, DesignUnit, SuperUnit, ConstUnit, PrjStartDate, PrjEndDate, info.Url);

                        ToolDb.SaveEntity(result, string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 企业获奖信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpAward(CorpInfo info, string html)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "hjxx")));
            if (nodeList != null && nodeList.Count > 0)
            {
                parser = new Parser(new Lexer(nodeList.ToHtml().Replace("th", "td")));
                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                if (tableNode != null && tableNode.Count > 0)
                {
                    TableTag table = tableNode[0] as TableTag;
                    for (int i = 1; i < table.RowCount; i++)
                    {
                        TableRow tr = table.Rows[i];
                        if (tr.Columns[0].ToPlainTextString().Contains("没有显示结果"))
                            break;
                        string AwardName = string.Empty, AwardDate = string.Empty, AwardLevel = string.Empty, GrantUnit = string.Empty, ProjectName = string.Empty;

                        AwardName = tr.Columns[1].ToNodePlainString();
                        AwardDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        AwardLevel = tr.Columns[3].ToNodePlainString();
                        GrantUnit = tr.Columns[4].ToNodePlainString();
                        ProjectName = tr.Columns[5].ToNodePlainString();

                        CorpAward award = ToolDb.GenCorpAward(info.Id, AwardName, AwardDate, AwardLevel, GrantUnit, ProjectName, info.Url);
                        ToolDb.SaveEntity(award, string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 企业处罚信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpPunish(CorpInfo info, string html)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "xzcf")));
            if (nodeList != null && nodeList.Count > 0)
            {
                parser = new Parser(new Lexer(nodeList.ToHtml().Replace("th", "td")));
                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                if (tableNode != null && tableNode.Count > 0)
                {
                    TableTag table = tableNode[0] as TableTag;
                    for (int i = 1; i < table.RowCount; i++)
                    {
                        TableRow tr = table.Rows[i];
                        if (tr.Columns[0].ToPlainTextString().Contains("没有显示结果"))
                            break;
                        string DocNo = string.Empty, PunishType = string.Empty, GrantUnit = string.Empty, DocDate = string.Empty, PunishCtx = string.Empty, IsShow = string.Empty;
                        DocNo = tr.Columns[1].ToNodePlainString();
                        PunishType = tr.Columns[2].ToNodePlainString();
                        GrantUnit = tr.Columns[3].ToNodePlainString();
                        DocDate = tr.Columns[4].ToNodePlainString();

                        CorpPunish punish = ToolDb.GenCorpPunish(info.Id, DocNo, PunishType, GrantUnit, DocDate, PunishCtx, info.Url, "0");
                        ToolDb.SaveEntity(punish, string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 企业安全许可
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpSecLic(CorpInfo info, string html)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "aqsc")));
            if (nodeList != null && nodeList.Count > 0)
            {
                parser = new Parser(new Lexer(nodeList.ToHtml().Replace("th", "td")));
                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                if (tableNode != null && tableNode.Count > 0)
                {
                    TableTag table = tableNode[0] as TableTag;
                    for (int i = 1; i < table.RowCount; i++)
                    {
                        TableRow tr = table.Rows[i];
                        if (tr.Columns[0].ToPlainTextString().Contains("没有显示结果"))
                            break;

                        string SecLicCode = string.Empty, SecLicDesc = string.Empty, ValidStartDate = string.Empty, ValidStartEnd = string.Empty, SecLicUnit = string.Empty;

                        SecLicCode = tr.Columns[1].ToNodePlainString();
                        SecLicDesc = tr.Columns[2].ToNodePlainString();
                        ValidStartDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        ValidStartEnd = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        SecLicUnit = tr.Columns[5].ToNodePlainString();
                        if (Encoding.Default.GetByteCount(SecLicDesc) > 1000)
                            SecLicDesc = string.Empty;
                        CorpSecLic seclic = ToolDb.GenCorpSecLic(info.Id, SecLicCode, SecLicDesc, ValidStartDate, ValidStartEnd, SecLicUnit, info.Url);
                        ToolDb.SaveEntity(seclic, string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 企业安全人员证书
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpSecLicStaff(CorpInfo info, string html)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ryaq")));
            if (nodeList != null && nodeList.Count > 0)
            {
                parser = new Parser(new Lexer(nodeList.ToHtml().Replace("th", "td")));
                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                if (tableNode != null && tableNode.Count > 0)
                {
                    TableTag table = tableNode[0] as TableTag;
                    for (int i = 1; i < table.RowCount; i++)
                    {
                        TableRow tr = table.Rows[i];
                        if (tr.Columns[0].ToPlainTextString().Contains("没有显示结果"))
                            break;

                        string PersonName = string.Empty, PersonCertNo = string.Empty, GrantUnit = string.Empty, GrantDate = string.Empty;

                        PersonName = tr.Columns[1].ToNodePlainString();
                        PersonCertNo = tr.Columns[2].ToNodePlainString();
                        GrantUnit = tr.Columns[3].ToNodePlainString();
                        GrantDate = tr.Columns[4].ToPlainTextString().GetDateRegex();

                        CorpSecLicStaff SecLicStaff = ToolDb.GenCorpSecLicStaff(info.Id, PersonName, PersonCertNo, GrantUnit, GrantDate, info.Url);
                        ToolDb.SaveEntity(SecLicStaff, string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 企业资质信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpQual(CorpInfo info, string html)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zzzsxx")));
            if (nodeList != null && nodeList.Count > 0)
            {
                parser = new Parser(new Lexer(nodeList.ToHtml().Replace("th", "td")));
                NodeList dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                if (dtlNode != null && dtlNode.Count > 0)
                {
                    TableTag table = dtlNode[0] as TableTag;
                    for (int i = 1; i < table.RowCount; i++)
                    {
                        string CorpId = string.Empty, QualName = string.Empty, QualCode = string.Empty, QualSeq = string.Empty, qualNum = string.Empty, QualType = string.Empty, QualLevel = string.Empty, ValidDate = string.Empty, LicDate = string.Empty, LicUnit = string.Empty;

                        TableRow tr = table.Rows[i];
                        if (tr.Columns[0].ToPlainTextString().Contains("没有显示结果"))
                        {
                            break;
                        }
                        QualType = tr.Columns[1].ToNodePlainString();
                        QualCode = tr.Columns[2].ToNodePlainString();
                        parser = new Parser(new Lexer(html));
                        NodeList listDtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "qyzz")));
                        if (listDtlNode != null && listDtlNode.Count > 0)
                        {
                            parser = new Parser(new Lexer(listDtlNode.ToHtml().Replace("th", "td")));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag dtlTable = tableNode[0] as TableTag;
                                bool isAdd = false;
                                for (int k = 1; k < dtlTable.RowCount; k++)
                                {

                                    TableRow dr = dtlTable.Rows[k];
                                    if (dr.Columns[0].ToPlainTextString().Contains("没有显示结果"))
                                    {
                                        isAdd = true;
                                        break;
                                    }
                                    QualName = dr.Columns[1].ToNodePlainString();
                                    QualLevel = dr.Columns[2].ToNodePlainString();
                                    LicUnit = dr.Columns[3].ToNodePlainString();
                                    LicDate = dr.Columns[4].ToNodePlainString();
                                    if (info.CorpType.Contains("监理"))
                                    {
                                        QualName = QualName + "监理";
                                    }
                                    qualNum = QualLevel.GetLevel();
                                    CorpQual qual = ToolDb.GenCorpQual(info.Id, QualName, QualCode, QualSeq, QualType, QualLevel, ValidDate, LicDate, LicUnit, info.Url, qualNum, "广东省", "深圳市");
                                    ToolDb.SaveEntity(qual, string.Empty);
                                }
                                if (isAdd)
                                {
                                    if (info.CorpType.Contains("监理"))
                                    {
                                        QualName = QualName + "监理";
                                    }
                                    qualNum = QualLevel.GetLevel();
                                    LicUnit = tr.Columns[3].ToNodePlainString();
                                    LicDate = tr.Columns[4].ToPlainTextString().GetDateRegex();
                                    QualName = QualType;
                                    CorpQual qual = ToolDb.GenCorpQual(info.Id, QualName, QualCode, QualSeq, QualType, QualLevel, ValidDate, LicDate, LicUnit, info.Url, qualNum, "广东省", "深圳市");
                                    ToolDb.SaveEntity(qual, string.Empty);
                                }
                            }
                        }
                    }
                }
            }
        }



    }
}
