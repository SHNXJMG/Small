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
    public class SzCorpInfoJZYI : WebSiteCrawller
    {
        public SzCorpInfoJZYI()
            : base()
        {
            this.PlanTime = "1 23:30,8 23:30,16 23:30,24 23:30";//"1-28 2:00,4-28 2:00,7-28 2:00,10-28 2:00";
            this.Group = "企业信息";
            this.Title = "深圳市建设局企业信息（建筑业企业第一部分）";
            this.Description = "自动抓取深圳市建设局企业信息（建筑业企业第一部分）";
            this.ExistCompareFields = "CorpType,CorpName";
            this.MaxCount = 50000;
            this.SiteUrl = "http://portal.szjs.gov.cn:8888/publicShow/corpList.html";//"http://61.144.226.2:8001/web/enterprs/unitInfoAction.do?method=toList&certType=1";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            int count = 1, totalCount = 1;
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 80;
            string eventValidation = string.Empty;
            string pageHtl = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            for (int i = 1; i <= pageInt; i++)
            {

                if (i > 1)
                {
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] { "param", "corpType", "corp_name", "page" }, new string[] { "", "1", "", i.ToString() });
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        Thread.Sleep(12 * 60 * 1000);
                        try
                        {
                            html = ToolWeb.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                        }
                        catch
                        {
                            Thread.Sleep(8 * 60 * 1000);
                            try
                            {
                                html = ToolWeb.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                            }
                            catch
                            {
                                Thread.Sleep(8 * 60 * 1000);
                                continue;
                            }
                        }
                    }
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
                        string href = tr.Columns[1].GetATagHref();
                        string htmldtl = string.Empty;
                        string[] postParams = null;
                        NameValueCollection dtlNvc = null;
                        string infoUrl = "http://portal.szjs.gov.cn:8888/publicShow/corpDetail.html";

                        try
                        {
                            string temp = href.Replace("corpDetail", "").Replace("(", "").Replace(")", "").Replace("'", "");
                            postParams = temp.Split(',');
                            dtlNvc = ToolWeb.GetNameValueCollection(new string[] {"param","corpType","orgCode"
                            }, new string[] { postParams[0], "1", postParams[1] });
                            cUrl = infoUrl + string.Format("?param={0}&corpType=1&orgCode={1}", postParams[0], CorpCode);
                        }
                        catch { continue; }
                        try
                        {
                            htmldtl = ToolWeb.GetHtmlByUrl(infoUrl, dtlNvc, Encoding.UTF8);
                        }
                        catch
                        {
                            Thread.Sleep(12 * 60 * 1000);
                            try
                            {
                                ToolWeb.GetHtmlByUrl(infoUrl, dtlNvc, Encoding.UTF8);
                            }
                            catch
                            {
                                Thread.Sleep(8 * 60 * 1000);
                                try
                                {
                                    htmldtl = ToolWeb.GetHtmlByUrl(infoUrl, dtlNvc, Encoding.UTF8);
                                }
                                catch
                                {
                                    Thread.Sleep(8 * 60 * 1000);
                                    continue;
                                }
                            }
                        }
                        parser = new Parser(new Lexer(htmldtl.Replace("th", "td")));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("align", "center")));
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

                        CorpInfo info = ToolDb.GenCorpInfo(CorpName, CorpCode, CorpAddress, RegDate, RegFund, BusinessCode, BusinessType, LinkMan, LinkPhone, Fax, Email, CorpSite, "建筑业企业", "广东省", "深圳市", "深圳市住房和建设局", cUrl, ISOQualNum, ISOEnvironNum, OffAdr);

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
                            //delCorpCert.AppendFormat("delete from CorpCert where CorpId='{0}'", id);
                            delCorpPunish.AppendFormat("delete from CorpPunish where CorpId='{0}'", id);
                            delCorpSecLic.AppendFormat("delete from CorpSecLic where CorpId='{0}'", id);
                            delCorpSecLicStaff.AppendFormat("delete from CorpSecLicStaff where CorpId='{0}'", id);
                            delCorpTecStaff.AppendFormat("delete from CorpTecStaff where CorpId='{0}'", id);
                            //delCorpDevice.AppendFormat("delete from CorpDevice where CorpId='{0}'", id);
                            delCorpResults.AppendFormat("delete from CorpResults where CorpId='{0}'", id);
                            qualCount = ToolCoreDb.ExecuteSql(delCorpQual.ToString());
                            leaderCount = ToolCoreDb.ExecuteSql(delCorpLeader.ToString());
                            awardCount = ToolCoreDb.ExecuteSql(delCorpAward.ToString());
                            //certCount = ToolCoreDb.ExecuteSql(delCorpCert.ToString());
                            punishCount = ToolCoreDb.ExecuteSql(delCorpPunish.ToString());
                            seclicCount = ToolCoreDb.ExecuteSql(delCorpSecLic.ToString());
                            seclicstaffCount = ToolCoreDb.ExecuteSql(delCorpSecLicStaff.ToString());
                            tecstaffCount = ToolCoreDb.ExecuteSql(delCorpTecStaff.ToString());
                            //deviceCount = ToolCoreDb.ExecuteSql(delCorpDevice.ToString());
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
                                        AddCorpQual(info, postParams[0], "1");
                                    if (awardCount != -1)
                                        AddCorpAward(info, postParams[0], "1");
                                    //if (certCount != -1)
                                    //    AddCorpCert(info, htmldtl);
                                    //if (deviceCount != -1)
                                    //    AddCorpDevice(info, htmldtl);
                                    if (punishCount != -1)
                                        AddCorpPunish(info, postParams[0], "1");
                                    if (resultCount != -1)
                                        AddCorpResults(info, postParams[0], "1");
                                    if (seclicCount != -1)
                                        AddCorpSecLic(info, postParams[0], "1");
                                    if (seclicstaffCount != -1)
                                        AddCorpSecLicStaff(info, postParams[0], "1");
                                    if (tecstaffCount != -1)
                                        AddCorpTecStaff(info, postParams[0], "1");
                                    if (leaderCount != -1)
                                        AddCorpLeader(info, postParams[0], "1");
                                }
                                else
                                {
                                    AddCorpQual(info, postParams[0], "1");
                                    AddCorpAward(info, postParams[0], "1");
                                    //AddCorpCert(info, htmldtl);
                                    //AddCorpDevice(info, htmldtl);
                                    AddCorpPunish(info, postParams[0], "1");
                                    AddCorpResults(info, postParams[0], "1");
                                    AddCorpSecLic(info, postParams[0], "1");
                                    AddCorpSecLicStaff(info, postParams[0], "1");
                                    AddCorpTecStaff(info, postParams[0], "1");
                                    AddCorpLeader(info, postParams[0], "1");
                                }
                            }
                        }
                        count++;
                        totalCount++;
                        if (count >= 90)
                        {
                            count = 1;
                            Thread.Sleep(10 * 60 * 1000);
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
        public void AddCorpLeader(CorpInfo info, string param, string corpType)
        {
            string url = "http://portal.szjs.gov.cn:8888/publicShow/queryPrincipal.html";
            string[] postParams = new string[] { "param", "corpType", "orgCode", "page" };
            string[] postValues = new string[] { param, corpType, info.CorpCode, "1" };
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
            string html = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
            }
            catch
            {
                Thread.Sleep(12 * 60 * 1000);
                try
                {
                    html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                }
                catch
                {
                    Thread.Sleep(8 * 60 * 1000);
                    return;
                }
            }
            JavaScriptSerializer java = new JavaScriptSerializer();
            Dictionary<string, object> jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
            int pageInt = 1;
            try
            {
                pageInt = (int)jsonResults["totalPage"];
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    postValues = new string[] { param, corpType, info.CorpCode, i.ToString() };
                    nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        Thread.Sleep(12 * 60 * 1000);
                        try
                        {
                            html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                        }
                        catch
                        {
                            Thread.Sleep(8 * 60 * 1000);
                            continue;
                        }
                    }
                    jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
                }
                object[] dicRecords = (object[])jsonResults["records"];
                foreach (object dicRecord in dicRecords)
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)dicRecord;
                    string LeaderName = string.Empty, LeaderDuty = string.Empty, LeaderType = string.Empty, htlCtx = string.Empty, phone = string.Empty;

                    LeaderName = Convert.ToString(dic["name"]);
                    LeaderDuty = Convert.ToString(dic["duty"]);
                    phone = Convert.ToString(dic["tel"]);
                    LeaderType = Convert.ToString(dic["emptype"]);

                    CorpLeader corpLeader = ToolDb.GenCorpLeader(info.Id, LeaderName, LeaderDuty, LeaderType, info.Url, phone);
                    ToolDb.SaveEntity(corpLeader, string.Empty);
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
        protected void AddCorpTecStaff(CorpInfo info, string param, string corpType)
        {
            string url = "http://portal.szjs.gov.cn:8888/publicShow/queryTechnology.html";
            string[] postParams = new string[] { "param", "corpType", "orgCode", "page" };
            string[] postValues = new string[] { param, corpType, info.CorpCode, "1" };
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
            string html = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
            }
            catch
            {
                Thread.Sleep(12 * 60 * 1000);
                try
                {
                    html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                }
                catch
                {
                    Thread.Sleep(8 * 60 * 1000);
                    return;
                }
            }
            JavaScriptSerializer java = new JavaScriptSerializer();
            Dictionary<string, object> jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
            int pageInt = 1;
            try
            {
                pageInt = (int)jsonResults["totalPage"];
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    postValues = new string[] { param, corpType, info.CorpCode, i.ToString() };
                    nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        Thread.Sleep(12 * 60 * 1000);
                        try
                        {
                            Thread.Sleep(8 * 60 * 1000);
                            html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
                }
                object[] dicRecords = (object[])jsonResults["records"];
                foreach (object dicRecord in dicRecords)
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)dicRecord;
                    string StaffName = string.Empty, IdCard = string.Empty, CertLevel = string.Empty, CertNo = string.Empty, stffType = string.Empty;

                    StaffName = Convert.ToString(dic["name"]);
                    stffType = Convert.ToString(dic["typename"]);
                    CertNo = Convert.ToString(dic["alt_cert_id"]);
                    CertLevel = Convert.ToString(dic["alt_qual_lv"]);
                    IdCard = Convert.ToString(dic["id_number"]);
                    CorpTecStaff staff = ToolDb.GenCorpTecStaff(info.Id, StaffName, IdCard, CertLevel, CertNo, info.Url, stffType);
                    ToolDb.SaveEntity(staff, string.Empty);
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
        protected void AddCorpResults(CorpInfo info, string param, string corpType)
        {
            string url = "http://portal.szjs.gov.cn:8888/publicShow/queryPerformance.html";
            string[] postParams = new string[] { "param", "corpType", "orgCode", "page" };
            string[] postValues = new string[] { param, corpType, info.CorpCode, "1" };
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
            string html = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
            }
            catch
            {
                Thread.Sleep(12 * 60 * 1000);
                try
                {
                    html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                }
                catch
                {
                    Thread.Sleep(8 * 60 * 1000);
                    return;
                }
            }
            JavaScriptSerializer java = new JavaScriptSerializer();
            Dictionary<string, object> jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
            int pageInt = 1;
            try
            {
                pageInt = (int)jsonResults["totalPage"];
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    postValues = new string[] { param, corpType, info.CorpCode, i.ToString() };
                    nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        Thread.Sleep(12 * 60 * 1000);
                        try
                        {
                            html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                        }
                        catch
                        {
                            Thread.Sleep(8 * 60 * 1000);
                            continue;
                        }
                    }
                    jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
                }
                object[] dicRecords = (object[])jsonResults["records"];
                foreach (object dicRecord in dicRecords)
                {
                    string PrjName = string.Empty, PrjCode = string.Empty, BuildUnit = string.Empty, GrantDate = string.Empty, PrjAddress = string.Empty, ChargeDept = string.Empty, PrjClassLevel = string.Empty, PrjClass = string.Empty, BuildArea = string.Empty, InviteArea = string.Empty, ProspUnit = string.Empty, DesignUnit = string.Empty, SuperUnit = string.Empty, ConstUnit = string.Empty, PrjStartDate = string.Empty, PrjEndDate = string.Empty;

                    PrjName = "业绩";
                    PrjCode = "业绩";
                    BuildUnit = "业绩";
                    GrantDate = DateTime.Today.ToString();

                    CorpResults result = ToolDb.GenCorpResults(info.Id, PrjName, PrjCode, BuildUnit, GrantDate, PrjAddress, ChargeDept, PrjClassLevel, PrjClass, BuildArea, InviteArea, ProspUnit, DesignUnit, SuperUnit, ConstUnit, PrjStartDate, PrjEndDate, info.Url);

                    ToolDb.SaveEntity(result, string.Empty);
                }
            }
        }

        /// <summary>
        /// 企业获奖信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpAward(CorpInfo info, string param, string corpType)
        {
            string url = "http://portal.szjs.gov.cn:8888/publicShow/queryPrizes.html";
            string[] postParams = new string[] { "param", "corpType", "orgCode", "page" };
            string[] postValues = new string[] { param, corpType, info.CorpCode, "1" };
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
            string html = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
            }
            catch
            {
                Thread.Sleep(12 * 60 * 1000);
                try
                {
                    html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                }
                catch
                {
                    Thread.Sleep(8 * 60 * 1000);
                    return;
                }
            }
            JavaScriptSerializer java = new JavaScriptSerializer();
            Dictionary<string, object> jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
            int pageInt = 1;
            try
            {
                pageInt = (int)jsonResults["totalPage"];
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    postValues = new string[] { param, corpType, info.CorpCode, i.ToString() };
                    nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        Thread.Sleep(12 * 60 * 1000);
                        try
                        {
                            html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                        }
                        catch
                        {
                            Thread.Sleep(8 * 60 * 1000);
                            continue;
                        }
                    }
                    jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
                }
                object[] dicRecords = (object[])jsonResults["records"];
                foreach (object dicRecord in dicRecords)
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)dicRecord;
                    string AwardName = string.Empty, AwardDate = string.Empty, AwardLevel = string.Empty, GrantUnit = string.Empty, ProjectName = string.Empty;
                    AwardName = Convert.ToString(dic["award_name"]);
                    AwardDate = Convert.ToString(dic["award_date"]);
                    AwardLevel = Convert.ToString(dic["award_lvl"]);
                    GrantUnit = Convert.ToString(dic["award_org"]);
                    ProjectName = Convert.ToString(dic["rel_prj"]);
                    CorpAward award = ToolDb.GenCorpAward(info.Id, AwardName, AwardDate, AwardLevel, GrantUnit, ProjectName, info.Url);
                    ToolDb.SaveEntity(award, string.Empty);
                }
            }

        }

        /// <summary>
        /// 企业处罚信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpPunish(CorpInfo info, string param, string corpType)
        {
            string url = "http://portal.szjs.gov.cn:8888/publicShow/queryPunish.html";
            string[] postParams = new string[] { "param", "corpType", "orgCode", "page" };
            string[] postValues = new string[] { param, corpType, info.CorpCode, "1" };
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
            string html = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
            }
            catch
            {
                Thread.Sleep(12 * 60 * 1000);
                try
                {
                    html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                }
                catch
                {
                    Thread.Sleep(8 * 60 * 1000);
                    return;
                }
            }
            JavaScriptSerializer java = new JavaScriptSerializer();
            Dictionary<string, object> jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
            int pageInt = 1;
            try
            {
                pageInt = (int)jsonResults["totalPage"];
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    postValues = new string[] { param, corpType, info.CorpCode, i.ToString() };
                    nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        Thread.Sleep(12 * 60 * 1000);
                        try
                        {
                            html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                        }
                        catch
                        {
                            Thread.Sleep(8 * 60 * 1000);
                            continue;
                        }
                    }
                    jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
                }
                object[] dicRecords = (object[])jsonResults["records"];
                foreach (object dicRecord in dicRecords)
                {
                    string DocNo = string.Empty, PunishType = string.Empty, GrantUnit = string.Empty, DocDate = string.Empty, PunishCtx = string.Empty, IsShow = string.Empty;

                    Dictionary<string, object> dic = (Dictionary<string, object>)dicRecord;
                    DocNo = Convert.ToString(dic["file_id"]);
                    PunishType = Convert.ToString(dic["pun_type_text"]);
                    GrantUnit = Convert.ToString(dic["file_org"]);
                    DocDate = Convert.ToString(dic["file_date"]);

                    CorpPunish punish = ToolDb.GenCorpPunish(info.Id, DocNo, PunishType, GrantUnit, DocDate, PunishCtx, info.Url, "0");

                    ToolDb.SaveEntity(punish, string.Empty);
                }
            }

        }

        /// <summary>
        /// 企业安全许可
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpSecLic(CorpInfo info, string param, string corpType)
        {
            string url = "http://portal.szjs.gov.cn:8888/publicShow/querySafeProduction.html";
            string[] postParams = new string[] { "param", "corpType", "orgCode", "page" };
            string[] postValues = new string[] { param, corpType, info.CorpCode, "1" };
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
            string html = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
            }
            catch
            {
                Thread.Sleep(12 * 60 * 1000);
                try
                {
                    html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                }
                catch
                {
                    Thread.Sleep(8 * 60 * 1000);
                    return;
                }
            }
            JavaScriptSerializer java = new JavaScriptSerializer();
            Dictionary<string, object> jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
            int pageInt = 1;
            try
            {
                pageInt = (int)jsonResults["totalPage"];
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    postValues = new string[] { param, corpType, info.CorpCode, i.ToString() };
                    nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        Thread.Sleep(12 * 60 * 1000);
                        try
                        {
                            html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                        }
                        catch
                        {
                            Thread.Sleep(8 * 60 * 1000);
                            continue;
                        }
                    }
                    jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
                }
                object[] dicRecords = (object[])jsonResults["records"];
                foreach (object dicRecord in dicRecords)
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)dicRecord;
                    string SecLicCode = string.Empty, SecLicDesc = string.Empty, ValidStartDate = string.Empty, ValidStartEnd = string.Empty, SecLicUnit = string.Empty;

                    SecLicCode = Convert.ToString(dic["lics_id"]);
                    SecLicDesc = Convert.ToString(dic["lics_range"]);
                    ValidStartDate = Convert.ToString(dic["valid_start_date"]);
                    ValidStartEnd = Convert.ToString(dic["valid_end_date"]);
                    SecLicUnit = Convert.ToString(dic["issue_dept"]);
                    if (Encoding.Default.GetByteCount(SecLicDesc) > 1000)
                        SecLicDesc = string.Empty;
                    CorpSecLic seclic = ToolDb.GenCorpSecLic(info.Id, SecLicCode, SecLicDesc, ValidStartDate, ValidStartEnd, SecLicUnit, info.Url);
                    ToolDb.SaveEntity(seclic, string.Empty);
                }
            }
        }

        /// <summary>
        /// 企业安全人员证书
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpSecLicStaff(CorpInfo info, string param, string corpType)
        {
            string url = "http://portal.szjs.gov.cn:8888/publicShow/queryPersonSafe.html";
            string[] postParams = new string[] { "param", "corpType", "orgCode", "page" };
            string[] postValues = new string[] { param, corpType, info.CorpCode, "1" };
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
            string html = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
            }
            catch
            {
                Thread.Sleep(12 * 60 * 1000);
                try
                {
                    html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                }
                catch
                {
                    Thread.Sleep(8 * 60 * 1000);
                    return;
                }
            }
            JavaScriptSerializer java = new JavaScriptSerializer();
            Dictionary<string, object> jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
            int pageInt = 1;
            try
            {
                pageInt = (int)jsonResults["totalPage"];
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    postValues = new string[] { param, corpType, info.CorpCode, i.ToString() };
                    nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        Thread.Sleep(12 * 60 * 1000);
                        try
                        {
                            html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                        }
                        catch
                        {
                            Thread.Sleep(8 * 60 * 1000);
                            continue;
                        }
                    }
                    jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
                }
                object[] dicRecords = (object[])jsonResults["records"];
                foreach (object dicRecord in dicRecords)
                {
                    string PersonName = string.Empty, PersonCertNo = string.Empty, GrantUnit = string.Empty, GrantDate = string.Empty;

                    Dictionary<string, object> dic = (Dictionary<string, object>)dicRecord;
                    PersonName = Convert.ToString(dic["name"]);
                    PersonCertNo = Convert.ToString(dic["lics_id"]);
                    GrantUnit = Convert.ToString(dic["issue_dept"]);
                    GrantDate = Convert.ToString(dic["issue_date"]);

                    CorpSecLicStaff SecLicStaff = ToolDb.GenCorpSecLicStaff(info.Id, PersonName, PersonCertNo, GrantUnit, GrantDate, info.Url);
                    ToolDb.SaveEntity(SecLicStaff, string.Empty);
                }
            }
        }

        /// <summary>
        /// 企业资质信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        protected void AddCorpQual(CorpInfo info, string param, string corpType)
        {
            string url = "http://portal.szjs.gov.cn:8888/publicShow/queryCertificateInfo.html";
            string[] postParams = new string[] { "param", "corpType", "orgCode", "page" };
            string[] postValues = new string[] { param, corpType, info.CorpCode, "1" };
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
            string html = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
            }
            catch
            {
                Thread.Sleep(12 * 60 * 1000);
                try
                {
                    html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                }
                catch
                {
                    Thread.Sleep(8 * 60 * 1000);
                    return;
                }
            }
            JavaScriptSerializer java = new JavaScriptSerializer();
            Dictionary<string, object> jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
            int pageInt = 1;
            try
            {
                pageInt = (int)jsonResults["totalPage"];
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    postValues = new string[] { param, corpType, info.CorpCode, i.ToString() };
                    nvc = ToolWeb.GetNameValueCollection(postParams, postValues);
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        Thread.Sleep(12 * 60 * 1000);
                        try
                        {
                            html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                        }
                        catch
                        {
                            Thread.Sleep(8 * 60 * 1000);
                            continue;
                        }
                    }
                    jsonResults = (Dictionary<string, object>)java.DeserializeObject(html);
                }
                object[] dicQuals = (object[])jsonResults["records"];
                foreach (object dicQual in dicQuals)
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)dicQual;
                    string CorpId = string.Empty, QualName = string.Empty, QualCode = string.Empty, QualSeq = string.Empty, qualNum = string.Empty, QualType = string.Empty, QualLevel = string.Empty, ValidDate = string.Empty, LicDate = string.Empty, LicUnit = string.Empty;
                    QualType = Convert.ToString(dic["name"]);
                    QualCode = Convert.ToString(dic["cert_no"]);
                    string certType = Convert.ToString(dic["cert_type"]);
                    string certId = Convert.ToString(dic["cert_id"]);
                    string htmldtl = string.Empty;
                    string urlDtl = "http://portal.szjs.gov.cn:8888/publicShow/queryCertificateDetail.html";
                    NameValueCollection dtlNvc = ToolWeb.GetNameValueCollection(new string[] { "param", "corpType", "cert_id" }, new string[] { param, certType, certId });
                    try
                    {
                        htmldtl = ToolWeb.GetHtmlByUrl(urlDtl, dtlNvc, Encoding.UTF8);
                    }
                    catch
                    {
                        Thread.Sleep(12 * 60 * 1000);
                        try
                        {
                            htmldtl = ToolWeb.GetHtmlByUrl(urlDtl, dtlNvc, Encoding.UTF8);
                        }
                        catch
                        {
                            Thread.Sleep(8 * 60 * 1000);
                            continue;
                        }
                    }
                    object[] dtlQuals = (object[])java.DeserializeObject(htmldtl);
                    foreach (object objQual in dtlQuals)
                    {
                        Dictionary<string, object> dicDtl = (Dictionary<string, object>)objQual;
                        QualName = Convert.ToString(dicDtl["name1"]);
                        QualLevel = Convert.ToString(dicDtl["name2"]);
                        LicUnit = Convert.ToString(dicDtl["appr_org"]);
                        LicDate = Convert.ToString(dicDtl["appr_date"]);
                        ValidDate = Convert.ToString(dicDtl["valid_period"]);
                        qualNum = QualLevel.GetLevel();
                        CorpQual qual = ToolDb.GenCorpQual(info.Id, QualName, QualCode, QualSeq, QualType, QualLevel, ValidDate, LicDate, LicUnit, info.Url, qualNum, "广东省", "深圳市");
                        ToolDb.SaveEntity(qual, string.Empty);
                    }
                }
            }
        }
    }
}
