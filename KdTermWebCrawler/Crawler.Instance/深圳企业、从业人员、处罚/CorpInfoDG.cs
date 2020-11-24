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
    public class CorpInfoDG : WebSiteCrawller
    {
        public CorpInfoDG()
            : base()
        {
            this.PlanTime = "1 22:30,8 22:30,16 22:30,24 22:30";//"1-28 2:00,4-28 2:00,7-28 2:00,10-28 2:00";
            this.Group = "企业信息";
            this.Title = "东莞建设网企业信息";
            this.Description = "自动抓取东莞建设网企业信息";
            this.ExistCompareFields = "InfoSource,CorpName";
            this.ExistsUpdate = true;
            this.MaxCount = 50000;
            this.SiteUrl = "http://www.dgjs.gov.cn/dgweb/search.do?method=searchHandBook&selected=1";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            string pageHtl = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList enttypeNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("name", "selected2")), true), new TagNameFilter("option")));
            parser.Reset();
            NodeList typeNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("name", "selected")), true), new TagNameFilter("option")));
            if (enttypeNode != null && enttypeNode.Count > 0 && typeNode != null && typeNode.Count > 0)
            {
                for (int t = 0; t < enttypeNode.Count; t++)
                {
                    string entTag = (enttypeNode[t] as OptionTag).GetAttribute("value");
                    string entText = enttypeNode[t].ToNodePlainString();
                    for (int d = 0; d < typeNode.Count; d++)
                    {
                        string typeTag = (typeNode[d] as OptionTag).GetAttribute("value");
                        string corpType = typeNode[d].ToNodePlainString();
                        if (t == 1 && d == 0)
                        { typeTag = "16"; corpType = "房地产开发企业"; }
                        if (t == 1 && d == 1)
                        { typeTag = "17"; corpType = "预拌商品混凝土企业"; }
                        if (t == 1 && d == 2)
                        { typeTag = "19"; corpType = "建筑业施工企业"; }

                        try
                        {
                            NameValueCollection nvc = ToolWeb.GetNameValueCollection(
                                new string[]{
                                "pageMethod",
                                "method",
                                "selected2",
                                "selected",
                                "_state",
                                "keyword",
                                "currentPage",
                                "currentPage_temp" 
                                },
                                new string[]{
                                "",
                                "searchHandBook",
                                entTag,
                               typeTag,
                                "1",
                                "","1","1"
                                });
                            html = ToolWeb.GetHtmlByUrl("http://www.dgjs.gov.cn/dgweb/search.do", nvc, Encoding.UTF8, ref cookiestr);
                        }
                        catch { continue; }

                        parser = new Parser(new Lexer(html));
                        NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "mainNextPage")));
                        if (pageNode != null && pageNode.Count > 0)
                        {
                            try
                            {
                                string temp = pageNode.AsString().GetRegexBegEnd("/", "页").Replace("\r", "").Replace("\t", "").Replace("\n", "");
                                pageInt = int.Parse(temp);
                            }
                            catch { }
                        }
                        for (int i = 1; i <= pageInt; i++)
                        {
                            if (i > 1)
                            {
                                try
                                {
                                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(
                               new string[]{
                                "pageMethod",
                                "method",
                                "selected2",
                                "selected",
                                "_state",
                                "keyword",
                                "currentPage",
                                "currentPage_temp", 
                                },
                               new string[]{
                                "next",
                                "searchHandBook",
                                entTag,
                                typeTag,
                                "1",
                                "",(i-1).ToString(),i.ToString()
                                });
                                    html = ToolWeb.GetHtmlByUrl("http://www.dgjs.gov.cn/dgweb/search.do", nvc, Encoding.UTF8, ref cookiestr);
                                }
                                catch { continue; }
                            }
                            parser = new Parser(new Lexer(html));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("align", "center")), true), new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "jsxmtb"))));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag table = tableNode[0] as TableTag;
                                for (int j = 2; t == 1 ? j <= table.RowCount : j < table.RowCount; j++)
                                {
                                    string CorpName = string.Empty, CorpCode = string.Empty, CorpAddress = string.Empty,
                                  RegDate = string.Empty, RegFund = string.Empty, BusinessCode = string.Empty,
                                  BusinessType = string.Empty, LinkMan = string.Empty, LinkPhone = string.Empty,
                                  Fax = string.Empty, Email = string.Empty, CorpSite = string.Empty, cUrl = string.Empty,
                                  ISOQualNum = string.Empty, ISOEnvironNum = string.Empty, CorpLevey = string.Empty;

                                    TableRow tr = null;

                                    #region 信用手册
                                    if (entText.Contains("手册"))
                                    {
                                        tr = table.Rows[j];
                                        CorpName = tr.Columns[1].ToNodePlainString();
                                        LinkMan = tr.Columns[3].ToNodePlainString();
                                        CorpAddress = tr.Columns[5].ToNodePlainString();
                                        CorpLevey = tr.Columns[2].ToNodePlainString();
                                        if (corpType.Contains("担保企业"))
                                            cUrl = "http://www.dgjs.gov.cn/dgweb/" + tr.Columns[10].GetATagHref();
                                        else
                                            cUrl = "http://www.dgjs.gov.cn/dgweb/" + tr.Columns[9].GetATagHref();
                                        string htlDtl = string.Empty;
                                        try
                                        {
                                            htlDtl = ToolWeb.GetHtmlByUrl(cUrl, Encoding.UTF8).GetJsString();
                                        }
                                        catch { continue; }

                                        parser = new Parser(new Lexer(htlDtl.Replace("th", "td").Replace("TH", "TD")));
                                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "dgjsj")), true), new TagNameFilter("table")));
                                        if (dtlNode != null && dtlNode.Count > 1)
                                        {
                                            string ctx = string.Empty;
                                            TableTag dtlTable = dtlNode[0] as TableTag;
                                            for (int c = 1; c < dtlTable.RowCount; c++)
                                            {
                                                for (int v = 0; v < dtlTable.Rows[c].ColumnCount; v++)
                                                {
                                                    if (string.IsNullOrEmpty(dtlTable.Rows[c].Columns[v].ToNodePlainString())) continue;
                                                    if ((v + 1) % 2 == 0)
                                                        ctx += dtlTable.Rows[c].Columns[v].ToNodePlainString() + "\r\n";
                                                    else
                                                        ctx += dtlTable.Rows[c].Columns[v].ToNodePlainString() + "：";
                                                }
                                            }

                                            RegDate = ctx.GetRegex("设立时间,设立日期");
                                            LinkPhone = ctx.GetRegex("联系电话");
                                            Fax = ctx.GetRegex("传真");
                                            Email = ctx.GetRegex("电子邮箱");
                                            BusinessType = ctx.GetRegex("经济性质");
                                            BusinessCode = ctx.GetRegex("营业执照注册号");
                                        }
                                        CorpInfo info = ToolDb.GenCorpInfo(CorpName, CorpCode, CorpAddress, RegDate, RegFund, BusinessCode, BusinessType, LinkMan, LinkPhone, Fax, Email, CorpSite, corpType, "广东省", "东莞市", "东莞市住房和城乡建设局", cUrl, ISOQualNum, ISOEnvironNum, string.Empty);
                                        if (!string.IsNullOrEmpty(CorpName.GetNotChina()))
                                        {
                                            string strSql = string.Format("select Id from CorpInfo where CorpName='{0}' and InfoSource='{1}' and CorpType='{2}'", info.CorpName, info.InfoSource, info.CorpType);
                                            object obj = ToolDb.ExecuteScalar(strSql);
                                            if (obj != null && obj.ToString() != "")
                                            {
                                                StringBuilder delCorpQual = new System.Text.StringBuilder();
                                                StringBuilder delCorpLeader = new System.Text.StringBuilder();
                                                StringBuilder delCorpTecStaff = new System.Text.StringBuilder();
                                                delCorpQual.AppendFormat("delete from CorpQual where CorpId='{0}'", obj);
                                                delCorpLeader.AppendFormat("delete from CorpLeader where CorpId='{0}'", obj);
                                                delCorpTecStaff.AppendFormat("delete from CorpTecStaff where CorpId='{0}'", obj);
                                                ToolDb.ExecuteSql(delCorpQual.ToString());
                                                ToolDb.ExecuteSql(delCorpLeader.ToString());
                                                ToolDb.ExecuteSql(delCorpTecStaff.ToString());
                                                string corpSql = string.Format("delete from CorpInfo where Id='{0}'", obj);
                                                ToolCoreDb.ExecuteSql(corpSql);
                                            }
                                            if (ToolDb.SaveEntity(info, string.Empty))
                                            {
                                                object corpId = ToolDb.ExecuteScalar("select Id from CorpInfo where Url='" + info.Url + "' and InfoSource='东莞市住房和城乡建设局' ");

                                                ToolDb.ExecuteSql("delete from CorpQual where CorpId='" + corpId + "'");

                                                #region 企业资质
                                                TableTag quaTable = dtlNode[1] as TableTag;
                                                for (int q = 2; q < quaTable.RowCount; q++)
                                                {
                                                    TableRow quaTr = quaTable.Rows[q];
                                                    string CorpId = string.Empty, QualName = string.Empty, QualCode = string.Empty, QualSeq = string.Empty, qualNum = string.Empty, QualType = string.Empty, QualLevel = string.Empty, ValidDate = string.Empty, LicDate = string.Empty, LicUnit = string.Empty;
                                                    CorpId = info.Id;
                                                    QualName = quaTr.Columns[0].ToNodePlainString();
                                                    QualLevel = quaTr.Columns[1].ToNodePlainString();
                                                    QualCode = quaTr.Columns[5].ToNodePlainString();
                                                    LicUnit = quaTr.Columns[6].ToNodePlainString();
                                                    QualType = quaTr.Columns[0].ToNodePlainString();
                                                    ValidDate = quaTr.Columns[3].ToPlainTextString().GetDateRegex();
                                                    qualNum = QualLevel.GetLevel();

                                                    CorpQual qual = ToolDb.GenCorpQual(CorpId, QualName, QualCode, QualSeq, QualType, QualLevel, ValidDate, LicDate, LicUnit, cUrl, qualNum, "广东省", "东莞市");

                                                    ToolDb.SaveEntity(qual, "");
                                                }
                                                #endregion

                                                #region 企业负责人
                                                parser = new Parser(new Lexer(htlDtl));
                                                NodeList leaderNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "head2")));
                                                if (leaderNode != null && leaderNode.Count > 0)
                                                {
                                                    ToolDb.ExecuteSql("delete from CorpLeader where CorpId='" + corpId + "'");
                                                    ATag leaderTag = leaderNode.GetATag(1);
                                                    if (!leaderTag.LinkText.Contains("负责人"))
                                                    {
                                                        leaderTag = leaderNode.GetATag(2);
                                                    }
                                                    if (!leaderTag.LinkText.Contains("负责人"))
                                                    {
                                                        leaderTag = leaderNode.GetATag(3);
                                                    }
                                                    if (!leaderTag.LinkText.Contains("负责人"))
                                                    {
                                                        leaderTag = leaderNode.GetATag(4);
                                                    }
                                                    if (leaderTag.LinkText.Contains("负责人"))
                                                    {
                                                        string leaderUrl = "http://www.dgjs.gov.cn/dgweb/" + leaderTag.Link;
                                                        string leaderDtl = string.Empty;
                                                        try
                                                        {
                                                            leaderDtl = ToolWeb.GetHtmlByUrl(leaderUrl, Encoding.UTF8).GetJsString();
                                                        }
                                                        catch { }

                                                        parser = new Parser(new Lexer(leaderDtl));
                                                        NodeList leaderDtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "dgjsj")), true), new TagNameFilter("table")));

                                                        if (leaderDtlNode != null && leaderDtlNode.Count > 0)
                                                        {
                                                            TableTag leaderTable = leaderDtlNode[0] as TableTag;
                                                            for (int l = 3; l < leaderTable.RowCount; l++)
                                                            {
                                                                TableRow leaderTr = leaderTable.Rows[l];

                                                                if (leaderTr.ToHtml().ToLower().Contains("none")) continue;
                                                                string LeaderName = string.Empty, LeaderDuty = string.Empty, LeaderType = string.Empty, htlCtx = string.Empty;
                                                                try
                                                                {
                                                                    LeaderName = leaderTr.Columns[0].ToNodePlainString();
                                                                    LeaderDuty = leaderTr.Columns[4].ToNodePlainString();

                                                                    LeaderType = leaderTr.Columns[1].ToNodePlainString();

                                                                }
                                                                catch
                                                                { }
                                                                if (!string.IsNullOrEmpty(LeaderName))
                                                                {
                                                                    CorpLeader corpLeader = ToolDb.GenCorpLeader(info.Id, LeaderName, LeaderDuty, LeaderType, leaderUrl);
                                                                    ToolDb.SaveEntity(corpLeader, string.Empty);
                                                                }
                                                            }
                                                        }
                                                    }

                                                }
                                                #endregion

                                                #region 企业技术力量
                                                parser = new Parser(new Lexer(htlDtl));
                                                NodeList tecNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "head2")));
                                                if (tecNode != null && tecNode.Count > 0)
                                                {
                                                    ToolDb.ExecuteSql("delete from CorpTecStaff where CorpId='" + corpId + "'");
                                                    ATag leaderTag = tecNode.GetATag(1);
                                                    if (!leaderTag.LinkText.Contains("技术"))
                                                    {
                                                        leaderTag = tecNode.GetATag(2);
                                                    }
                                                    if (!leaderTag.LinkText.Contains("技术"))
                                                    {
                                                        leaderTag = tecNode.GetATag(3);
                                                    }
                                                    if (!leaderTag.LinkText.Contains("技术"))
                                                    {
                                                        leaderTag = tecNode.GetATag(4);
                                                    }
                                                    if (!leaderTag.LinkText.Contains("技术"))
                                                    {
                                                        leaderTag = tecNode.GetATag(5);
                                                    }
                                                    if (leaderTag.LinkText.Contains("技术"))
                                                    {
                                                        string leaderUrl = "http://www.dgjs.gov.cn/dgweb/" + leaderTag.Link;
                                                        string leaderDtl = string.Empty;
                                                        try
                                                        {
                                                            leaderDtl = ToolWeb.GetHtmlByUrl(leaderUrl, Encoding.UTF8).GetJsString();
                                                        }
                                                        catch { }

                                                        parser = new Parser(new Lexer(leaderDtl));
                                                        NodeList leaderDtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "maintable")));

                                                        if (leaderDtlNode != null && leaderDtlNode.Count > 0)
                                                        {
                                                            TableTag leaderTable = leaderDtlNode[0] as TableTag;
                                                            for (int l = 2; l < leaderTable.RowCount - 1; l++)
                                                            {
                                                                TableRow leaderTr = leaderTable.Rows[l];

                                                                string StaffName = string.Empty, IdCard = string.Empty, CertLevel = string.Empty, CertNo = string.Empty, stffType = string.Empty;
                                                                try
                                                                {
                                                                    StaffName = leaderTr.Columns[1].ToNodePlainString();
                                                                    stffType = leaderTr.Columns[6].ToNodePlainString();
                                                                    if (stffType == "/") stffType = null;
                                                                    CertNo = leaderTr.Columns[8].ToNodePlainString();
                                                                }
                                                                catch { }
                                                                if (!string.IsNullOrEmpty(StaffName))
                                                                {
                                                                    CorpTecStaff staff = ToolDb.GenCorpTecStaff(info.Id, StaffName, IdCard, CertLevel, CertNo, leaderUrl, stffType);
                                                                    ToolDb.SaveEntity(staff, string.Empty);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                #endregion
                                            }
                                        }
                                    }
                                    #endregion

                                    #region 资质证书企业
                                    else
                                    {
                                        tr = table.Rows[j - 1];
                                        try
                                        {
                                            CorpName = tr.Columns[0].ToNodePlainString();
                                            CorpAddress = tr.Columns[1].ToNodePlainString();
                                            LinkMan = tr.Columns[2].ToNodePlainString();
                                            CorpInfo info1 = ToolDb.GenCorpInfo(CorpName, CorpCode, CorpAddress, RegDate, RegFund, BusinessCode, BusinessType, LinkMan, LinkPhone, Fax, Email, CorpSite, corpType, "广东省", "东莞市", "东莞市住房和城乡建设局", cUrl, ISOQualNum, ISOEnvironNum, string.Empty);
                                            if (!string.IsNullOrEmpty(CorpName.GetNotChina()))
                                            {
                                                string strSql = string.Format("select Id from CorpInfo where CorpName='{0}' and InfoSource='{1}' and CorpType='{2}'", info1.CorpName, info1.InfoSource, info1.CorpType);
                                                object obj = ToolDb.ExecuteScalar(strSql);
                                                if (obj != null && obj.ToString() != "")
                                                {
                                                    string corpSql = string.Format("delete from CorpInfo where Id='{0}'", obj);
                                                    ToolCoreDb.ExecuteSql(corpSql);
                                                }
                                                ToolDb.SaveEntity(info1, string.Empty);
                                            }
                                        }
                                        catch (Exception ex) { }
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
