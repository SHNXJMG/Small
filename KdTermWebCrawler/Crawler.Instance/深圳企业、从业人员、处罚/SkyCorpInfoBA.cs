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

namespace Crawler.Instance
{
    public class SkyCorpInfoBA : WebSiteCrawller
    {
        public SkyCorpInfoBA()
            : base()
        {
            this.PlanTime = "1 21:30,8 21:30,16 21:30,24 21:30";//"1-28 2:00,4-28 2:00,7-28 2:00,10-28 2:00";
            this.Group = "企业信息";
            this.Title = "三库一平台进粤备案企业信息";
            this.Description = "自动抓取三库一平台进粤备案企业信息";
            this.ExistCompareFields = "CorpType,CorpName";
            this.MaxCount = 50000;
            this.SiteUrl = "http://113.108.219.40/intogd/";
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
                html = ToolWeb.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ContentPlaceHolder1_AspNetPager1")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].GetATagHref().Replace("&#39;", "").Replace(")", "kdxx").Replace(",", "xxdk");
                    pageInt = int.Parse(temp.GetRegexBegEnd("xxdk", "kdxx"));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        if (i == 2)
                        {
                            viewState = ToolWeb.GetAspNetViewState(html);
                            eventValidation = ToolWeb.GetAspNetEventValidation(html);
                        }
                        NameValueCollection nvc = ToolWeb.GetNameValueCollection(
                            new string[]{
                            "ctl00$ContentPlaceHolder1$ScriptManager1",
                            "ctl00$ContentPlaceHolder1$txtORGNAME",
                            "ctl00$ContentPlaceHolder1$txtORGCODE",
                            "ctl00$ContentPlaceHolder1$txtPNAME",
                            "ctl00$ContentPlaceHolder1$txtIDNUM",
                            "ctl00$ContentPlaceHolder1$txtHIREERORGNAME",
                            "ctl00$ContentPlaceHolder1$txtHIREERORGCODE",
                            "ctl00$ContentPlaceHolder1$ddlRegType",
                            "ctl00$ContentPlaceHolder1$ddlTitle",
                            "ctl00$ContentPlaceHolder1$ddlABC",
                            "ctl00$ContentPlaceHolder1$ddlCert",
                            "__VIEWSTATE",
                            "__EVENTTARGET",
                            "__EVENTARGUMENT",
                            "__EVENTVALIDATION",
                            "__ASYNCPOST"
                            },
                            new string[]{
                            "ctl00$ContentPlaceHolder1$UpdatePanel1|ctl00$ContentPlaceHolder1$AspNetPager1",
                            "","","","","","","","","","",
                            viewState,
                            "ctl00$ContentPlaceHolder1$AspNetPager1",
                            i.ToString(),
                            eventValidation,
                            "true"
                            }
                            );

                        html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "data-grid")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string CorpName = string.Empty, CorpCode = string.Empty, CorpAddress = string.Empty,
                                            RegDate = string.Empty, RegFund = string.Empty, BusinessCode = string.Empty,
                                            BusinessType = string.Empty, LinkMan = string.Empty, LinkPhone = string.Empty,
                                            Fax = string.Empty, Email = string.Empty, CorpSite = string.Empty, cUrl = string.Empty, CorpType,
                                            ISOQualNum = string.Empty, ISOEnvironNum = string.Empty, OffAdr = string.Empty, Cert = string.Empty;

                        TableRow tr = table.Rows[j];
                        CorpName = tr.Columns[0].ToNodePlainString();
                        LinkMan = tr.Columns[1].ToNodePlainString();
                        cUrl = tr.Columns[0].GetATagValue("onclick").Replace("OpenWin('", "");
                        if (cUrl.IndexOf("'") > 0)
                        {
                            cUrl = "http://113.108.219.40/intogd/" + cUrl.Remove(cUrl.IndexOf("'"));
                        }
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = ToolWeb.GetHtmlByUrl(cUrl, Encoding.UTF8);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "data-table")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            string ctx = string.Empty;
                            TableTag dtlTable = dtlNode[0] as TableTag;
                            for (int k = 0; k < dtlTable.RowCount; k++)
                            {
                                for (int d = 0; d < dtlTable.Rows[k].ColumnCount; d++)
                                {
                                    TableColumn col = dtlTable.Rows[k].Columns[d];
                                    if (col.GetAttribute("class") == "td-left")
                                        ctx += col.ToNodePlainString() + "：";
                                    else
                                        ctx += col.ToNodePlainString() + "\r\n";
                                }
                            }


                            RegDate = ctx.GetRegex("成立时间,注册时间").GetDateRegex();
                            RegFund = ctx.GetRegex("注册资本");
                            BusinessCode = ctx.GetRegex("营业执照注册号");
                            CorpType = "外地进粤企业";
                            CorpAddress = ctx.GetRegex("注册详细地址");
                            if (!string.IsNullOrEmpty(RegFund) && !RegFund.Contains("万"))
                                RegFund += "万";

                            CorpInfo corp = ToolDb.GenCorpInfo(CorpName, CorpCode, CorpAddress, RegDate, RegFund, BusinessCode, BusinessType, LinkMan, LinkPhone, Fax, Email, CorpSite, CorpType, "广东省", "广东地区", "广东省住房和城乡建设厅", cUrl, ISOQualNum, ISOEnvironNum, OffAdr);
                            string strSql = string.Format("select Id from CorpInfo where CorpName='{0}' and CorpType='{1}'", corp.CorpName, corp.CorpType);
                            DataTable dt = ToolCoreDb.GetDbData(strSql);
                             if (dt != null && dt.Rows.Count > 0)
                            {
                                string id = dt.Rows[0]["Id"].ToString();
                                StringBuilder delCorpQual = new System.Text.StringBuilder();
                                StringBuilder delCorpLeader = new System.Text.StringBuilder();
                                StringBuilder delCorpSecLicStaff = new System.Text.StringBuilder();
                                StringBuilder delCorpInstitution = new StringBuilder();
                                delCorpInstitution.AppendFormat("delete from CorpInstitution where CorpId='{0}'", id);
                                delCorpQual.AppendFormat("delete from CorpQual where CorpId='{0}'", id);
                                delCorpLeader.AppendFormat("delete from CorpLeader where CorpId='{0}'", id);
                                delCorpSecLicStaff.AppendFormat("delete from CorpTecStaff where CorpId='{0}'", id);
                                ToolCoreDb.ExecuteSql(delCorpInstitution.ToString());
                                ToolCoreDb.ExecuteSql(delCorpQual.ToString());
                                ToolCoreDb.ExecuteSql(delCorpLeader.ToString());
                                ToolCoreDb.ExecuteSql(delCorpSecLicStaff.ToString());
                                string corpSql = string.Format("delete from CorpInfo where Id='{0}'", id);
                                ToolCoreDb.ExecuteSql(corpSql);
                            }
                            if (ToolDb.SaveEntity(corp, this.ExistCompareFields))
                            {
                                if (!string.IsNullOrEmpty(LinkMan))
                                {
                                    CorpLeader leader = ToolDb.GenCorpLeader(corp.Id, LinkMan, "", "企业法定代表人", cUrl);
                                    ToolDb.SaveEntity(leader, "");
                                }
                                if (!string.IsNullOrEmpty(tr.Columns[2].ToNodePlainString()))
                                {
                                    CorpLeader leader = ToolDb.GenCorpLeader(corp.Id, tr.Columns[2].ToNodePlainString(), "", "技术负责人", cUrl);
                                    ToolDb.SaveEntity(leader, "");
                                }
                                if (!string.IsNullOrEmpty(tr.Columns[3].ToNodePlainString()))
                                {
                                    CorpLeader leader = ToolDb.GenCorpLeader(corp.Id, tr.Columns[3].ToNodePlainString(), "", "驻粤负责人", cUrl);
                                    ToolDb.SaveEntity(leader, "");
                                }
                                AddCorpQual(corp, htmldtl);
                                AddCorpTecStaff(corp, htmldtl);
                                GetOffAddress(htmldtl, cUrl, corp);
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 保存资质
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        private void AddCorpQual(CorpInfo info, string html)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "data-grid")));
            if (nodeList != null && nodeList.Count > 0)
            {
                TableTag table = nodeList[0] as TableTag;
                for (int i = 1; i < table.RowCount; i++)
                {
                    TableRow tr = table.Rows[i];
                    string CorpId = string.Empty, QualName = string.Empty, QualCode = string.Empty, QualSeq = string.Empty, qualNum = string.Empty, QualType = string.Empty, QualLevel = string.Empty, ValidDate = string.Empty, LicDate = string.Empty, LicUnit = string.Empty;
                    CorpId = info.Id;
                    QualName = tr.Columns[0].ToNodePlainString();
                    QualCode = tr.Columns[2].ToNodePlainString();
                    QualLevel = tr.Columns[1].ToNodePlainString();
                    QualType = info.CorpType;
                    LicDate = tr.Columns[4].ToPlainTextString().GetDateRegex();
                    ValidDate = tr.Columns[5].ToPlainTextString().GetDateRegex();
                    LicUnit = tr.Columns[3].ToNodePlainString();
                    qualNum = QualLevel.GetLevel();
                    
                    CorpQual qual = ToolDb.GenCorpQual(info.Id, QualName, QualCode, QualSeq, QualType, QualLevel, ValidDate, LicDate, LicUnit, info.Url, qualNum, "广东省", "广东地区");
                    ToolDb.SaveEntity(qual, string.Empty);
                }
            }
        }

        /// <summary>
        /// 保存企业技术管理人员情况
        /// </summary>
        /// <param name="info"></param>
        /// <param name="html"></param>
        private void AddCorpTecStaff(CorpInfo info, string html)
        {
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            viewState = ToolWeb.GetAspNetViewState(html);
            eventValidation = ToolWeb.GetAspNetEventValidation(html);
            int pageInt = 1;
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[]{
                    "ctl00$MainContent$ScriptManager1",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "__EVENTVALIDATION",
                    "__ASYNCPOST"
                    }, new string[]{
                    "ctl00$MainContent$UpdatePanel1|ctl00$MainContent$step3",
                    "ctl00$MainContent$step3",
                    "", 
                    viewState,
                    eventValidation,
                    "true"
                    });
            string htmldtl = string.Empty;
            try
            {
                htmldtl = ToolWeb.GetHtmlByUrl(info.Url, nvc, Encoding.UTF8, ref cookiestr);
            }
            catch { }

            //Parser parser = new Parser(new Lexer(htmldtl));
            //NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "MainContent_AspNetPager1")), true), new TagNameFilter("a")));
            //if (pageNode != null && pageNode.Count > 0)
            //{
            //    try
            //    {
            //        string temp = pageNode[pageNode.Count - 1].GetATagHref().Replace("&#39;", "").Replace(")", "kdxx").Replace(",", "xxdk");
            //        pageInt = int.Parse(temp.GetRegexBegEnd("xxdk", "kdxx"));
            //    }
            //    catch { }
            //}
            //for (int i = 1; i <= pageInt; i++)
            //{ 
            //    if (i > 1)
            //    {
            //        NameValueCollection nvc1 = ToolWeb.GetNameValueCollection(new string[]{
            //        "ctl00$MainContent$ScriptManager1",
            //        "__EVENTTARGET",
            //        "__EVENTARGUMENT",
            //        "__VIEWSTATE",
            //        "__EVENTVALIDATION",
            //        "__ASYNCPOST"
            //        }, new string[]{
            //        "ctl00$MainContent$UpdatePanel1|ctl00$MainContent$AspNetPager1",
            //        "ctl00$MainContent$AspNetPager1",
            //        i.ToString(), 
            //        viewState,
            //        eventValidation,
            //        "true"
            //        });
            //        try
            //        {
            //            htmldtl = ToolWeb.GetHtmlByUrl("http://113.108.219.40/intogd/Open/EnterpriseInfo.aspx?ID=1aNTSgxf1zvCznU8XPW9UQ==", nvc1, Encoding.UTF8, ref cookiestr);
            //        }
            //        catch { continue; }
            //    }
            Parser parser = new Parser(new Lexer(htmldtl));
            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "data-grid")));
            if (dtlNode != null && dtlNode.Count > 0)
            {
                TableTag table = dtlNode[0] as TableTag;
                for (int j = 1; j < table.RowCount; j++)
                {
                    TableRow tr = table.Rows[j];
                    string StaffName = string.Empty, IdCard = string.Empty, CertLevel = string.Empty, CertNo = string.Empty, stffType = string.Empty;
                    StaffName = tr.Columns[0].ToNodePlainString();
                    stffType = tr.Columns[1].ToNodePlainString();
                    string aHref = "http://113.108.219.40/intogd/Open/" + tr.Columns[0].GetATagHref();
                    string staffDtl = string.Empty;
                    try
                    {
                        staffDtl = ToolWeb.GetHtmlByUrl(aHref, Encoding.UTF8);
                    }
                    catch { }
                    parser = new Parser(new Lexer(staffDtl));
                    NodeList staffNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                    if (staffNode != null && staffNode.Count > 0)
                    {
                        string ctx = string.Empty;
                        TableTag dtlTable = staffNode[1] as TableTag;
                        for (int k = 0; k < dtlTable.RowCount; k++)
                        {
                            for (int d = 0; d < dtlTable.Rows[k].ColumnCount; d++)
                            {
                                TableColumn col = dtlTable.Rows[k].Columns[d];
                                if (col.GetAttribute("class") == "td-left")
                                    ctx += col.ToNodePlainString() + "：";
                                else
                                    ctx += col.ToNodePlainString() + "\r\n";
                            }
                        }
                        CertNo = ctx.GetRegex("职称证号");
                    }

                    CorpTecStaff staff = ToolDb.GenCorpTecStaff(info.Id, StaffName, IdCard, CertLevel, CertNo, info.Url, stffType);
                    ToolDb.SaveEntity(staff, string.Empty);
                }
            }
            //  }
        }

        /// <summary>
        /// 获取办公地址并保存分支机构信息
        /// </summary>
        /// <param name="html"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetOffAddress(string html, string url, CorpInfo info)
        {
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            viewState = ToolWeb.GetAspNetViewState(html);
            eventValidation = ToolWeb.GetAspNetEventValidation(html);
            string returnValue = string.Empty;
            int pageInt = 1;
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[]{
                    "ctl00$MainContent$ScriptManager1",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "__EVENTVALIDATION",
                    "__ASYNCPOST"
                    }, new string[]{
                    "ctl00$MainContent$UpdatePanel1|ctl00$MainContent$step2",
                    "ctl00$MainContent$step2",
                    "", 
                    viewState,
                    eventValidation,
                    "true"
                    });
            string htmldtl = string.Empty;
            try
            {
                htmldtl = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8, ref cookiestr);
            }
            catch { }
            Parser parser = new Parser(new Lexer(htmldtl));
            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "data-grid")));
            if (dtlNode != null && dtlNode.Count > 0)
            {
                TableTag table = dtlNode[0] as TableTag;
                for (int i = 1; i < table.RowCount; i++)
                {
                    string CorpName = string.Empty, CorpCode = string.Empty, Location = string.Empty, DtlAddress = string.Empty, PostalCode = string.Empty, ResInstitution = string.Empty, LinkMan = string.Empty, LinPhone = string.Empty, Fax = string.Empty, BusinessCode = string.Empty, RegDate = string.Empty, Email = string.Empty, SafetyCode = string.Empty, TotalReMan = string.Empty, TechReMan = string.Empty, SafeReMan = string.Empty, QualityReMan = string.Empty, Url = string.Empty, TotalSafetyCode = string.Empty, TechSafetyCode = string.Empty, QualitySafetyCode=string.Empty;
                    TableRow tr = table.Rows[i];
                    Url = "http://113.108.219.40/intogd/Open/" + tr.Columns[0].GetATagHref();
                    CorpName = tr.Columns[0].ToNodePlainString();
                    TotalReMan = tr.Columns[2].ToNodePlainString();
                    TechReMan = tr.Columns[3].ToNodePlainString();
                    QualityReMan = tr.Columns[4].ToNodePlainString();
                    SafeReMan = tr.Columns[5].ToNodePlainString();
                    string dtlHtml = string.Empty;
                    try
                    {
                        dtlHtml = ToolWeb.GetHtmlByUrl(Url, Encoding.UTF8);
                    }
                    catch { continue; }
                    parser = new Parser(new Lexer(dtlHtml));
                    NodeList staffNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                    if (staffNode != null && staffNode.Count > 1)
                    {
                        string ctx = string.Empty;
                        TableTag dtlTable = staffNode[1] as TableTag;
                        for (int k = 0; k < dtlTable.RowCount; k++)
                        {
                            for (int d = 0; d < dtlTable.Rows[k].ColumnCount; d++)
                            {
                                TableColumn col = dtlTable.Rows[k].Columns[d];
                                if (col.GetAttribute("class") == "td-left")
                                    ctx += col.ToNodePlainString() + "：";
                                else
                                    ctx += col.ToNodePlainString() + "\r\n";
                            }
                        }
                        if (string.IsNullOrEmpty(returnValue))
                            returnValue = ctx.GetRegex("详细地址");

                        CorpCode = ctx.GetRegex("组织机构代码");
                        Location = ctx.GetRegex("所在地");
                        DtlAddress = ctx.GetRegex("详细地址");
                        PostalCode = ctx.GetRegex("邮政编码");
                        ResInstitution = ctx.GetRegex("驻粤负责机构");
                        LinkMan = ctx.Replace(" ", "").GetRegex("联系人");
                        Fax = ctx.GetRegex("传真号码");
                        LinPhone = ctx.GetRegex("联系电话");
                        BusinessCode = ctx.GetRegex("营业执照注册号").Replace("分", "");
                        RegDate = ctx.GetRegex("设立时间").GetDateRegex();
                        Email = ctx.GetRegex("邮箱");
                        parser.Reset();
                        NodeList safeNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "data-table")));
                        if (safeNode != null && safeNode.Count > 2)
                        {
                            TableTag safeTable = safeNode[2] as TableTag;
                            string TotalSafetyCodeCtx = string.Empty;
                            string TechSafetyCodeCtx = string.Empty;
                            for (int k = 0; k < safeTable.RowCount; k++)
                            { 
                                for (int d = 0; d < safeTable.Rows[k].ColumnCount; d++)
                                {
                                    TableColumn col = safeTable.Rows[k].Columns[d];
                                    if (d >= 2)
                                    {
                                        if (col.GetAttribute("class") == "td-left")
                                            TechSafetyCodeCtx += col.ToNodePlainString() + "：";
                                        else
                                            TechSafetyCodeCtx += col.ToNodePlainString() + "\r\n";
                                    }
                                    else
                                    {
                                        if (col.GetAttribute("class") == "td-left")
                                            TotalSafetyCodeCtx += col.ToNodePlainString() + "：";
                                        else
                                            TotalSafetyCodeCtx += col.ToNodePlainString() + "\r\n";
                                    }
                                }
                            }
                            TotalSafetyCode = ToolHtml.GetRegexStringNot(TotalSafetyCodeCtx, new string[] { "安全生产考核合格证号（A证）" });
                            TechSafetyCode = ToolHtml.GetRegexStringNot(TechSafetyCodeCtx, new string[] { "安全生产考核合格证号（A证）" });  
                        }

                        if (safeNode != null && safeNode.Count > 4)
                        {
                            TableTag safeTable = safeNode[4] as TableTag;
                            string SafetyCodeCtx = string.Empty;
                            string QualitySafetyCodeCtx = string.Empty;
                            for (int k = 0; k < safeTable.RowCount; k++)
                            {
                                for (int d = 0; d < safeTable.Rows[k].ColumnCount; d++)
                                {
                                    TableColumn col = safeTable.Rows[k].Columns[d];
                                    if (d >= 2)
                                    {
                                        if (col.GetAttribute("class") == "td-left")
                                            QualitySafetyCodeCtx += col.ToNodePlainString() + "：";
                                        else
                                            QualitySafetyCodeCtx += col.ToNodePlainString() + "\r\n";
                                    }
                                    else
                                    {
                                        if (col.GetAttribute("class") == "td-left")
                                            SafetyCodeCtx += col.ToNodePlainString() + "：";
                                        else
                                            SafetyCodeCtx += col.ToNodePlainString() + "\r\n";
                                    }
                                }
                            }
                            SafetyCode = ToolHtml.GetRegexStringNot(SafetyCodeCtx, new string[] { "安全生产考核合格证号（A或B证）" });
                            QualitySafetyCode = ToolHtml.GetRegexStringNot(QualitySafetyCodeCtx, new string[] { "安全生产考核合格证号" });// QualitySafetyCodeCtx.GetRegex("安全生产考核合格证号");
                        }
                         
                        CorpInstitution entity = ToolDb.GenCorpInstitution("广东省", "广东地区", info.Id, CorpName, CorpCode, Location, DtlAddress, PostalCode, ResInstitution, LinkMan, LinPhone, Fax, BusinessCode, RegDate, Email, SafetyCode, TotalReMan, TechReMan, SafeReMan, QualityReMan, Url,TotalSafetyCode,TechSafetyCode,QualitySafetyCode);

                        ToolDb.SaveEntity(entity, string.Empty);

                    }
                }
            }
            return returnValue;
        }
    }
}
