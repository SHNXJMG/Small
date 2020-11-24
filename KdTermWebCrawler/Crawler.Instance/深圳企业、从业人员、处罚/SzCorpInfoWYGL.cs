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
    public class SzCorpInfoWYGL : WebSiteCrawller
    {
        public SzCorpInfoWYGL()
            : base()
        {
            this.PlanTime = "1 23:30,8 23:30,16 23:30,24 23:30";//"1-28 2:00,4-28 2:00,7-28 2:00,10-28 2:00";
            this.Group = "企业信息";
            this.Title = "深圳市建设局企业信息（物业管理企业）";
            this.Description = "自动抓取深圳市建设局企业信息（物业管理企业）";
            this.ExistCompareFields = "CorpType,CorpName";
            this.MaxCount = 50000;
            this.SiteUrl = "http://61.144.226.3:92/WyjgInsideWeb/e-business/prg/signup/Creditlist.jsp?EventID=InterpriseInit&EventType=null";
        }

        protected override System.Collections.IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<CorpInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1, sqlCount = 0;
            string eventValidation = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "TABLEPANE")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("共", "页");
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
                        html = ToolWeb.GetHtmlByUrl(this.SiteUrl + "&pages=" + i,Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "resultset")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount-1; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string CorpName = string.Empty, CorpCode = string.Empty, CorpAddress = string.Empty,
                                 RegDate = string.Empty, RegFund = string.Empty, BusinessCode = string.Empty,
                                 BusinessType = string.Empty, LinkMan = string.Empty, LinkPhone = string.Empty,
                                 Fax = string.Empty, Email = string.Empty, CorpSite = string.Empty, cUrl = string.Empty,
                                 ISOQualNum = string.Empty, ISOEnvironNum = string.Empty, OffAdr = string.Empty, Cert = string.Empty, ctxKc = string.Empty,
                                 corpRz = string.Empty, corpType = string.Empty;

                        CorpName = tr.Columns[2].ToNodePlainString();
                        if (string.IsNullOrWhiteSpace(CorpName)) continue;
                        CorpCode = tr.Columns[3].ToNodePlainString();
                        corpType = "物业管理企业";
                        LinkMan = tr.Columns[5].ToNodePlainString();
                        LinkPhone = tr.Columns[6].ToNodePlainString();
                        cUrl = "http://61.144.226.3:92/WyjgInsideWeb/e-business/prg/signup/CreditInfoBase.jsp?EventID=CreditInfo&QYFRDM=" + CorpCode;
                        string htmldtl = string.Empty; 
                        try
                        {
                            htmldtl = ToolWeb.GetHtmlByUrl(cUrl,Encoding.Default);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("SCRIPT"), new HasAttributeFilter("language", "javascript")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            string ctx = dtlNode.AsString();
                            CorpAddress = ctx.GetRegexBegEnd("qyxxdz.value", ";").GetReplace(new string[] { "=", "'" });
                            CorpSite = ctx.GetRegexBegEnd("qywz.value", ";").GetReplace(new string[] { "=", "'" });
                            Email = ctx.GetRegexBegEnd("dzxx.value", ";").GetReplace(new string[] { "=", "'" });
                            BusinessCode = ctx.GetRegexBegEnd("yyzzzch.value", ";").GetReplace(new string[] { "=", "'" });
                            RegFund = ctx.GetRegexBegEnd("zczb.value", ";").GetReplace(new string[] { "=", "'", "人民币" });
                            RegDate = ctx.GetRegexBegEnd("qyclsj.value", ";").GetReplace(new string[] { "=", "'" });
                            BusinessType = ctx.GetRegexBegEnd("qydjzclx.value", ";").GetReplace(new string[] { "=", "'" });

                            CorpInfo info = ToolDb.GenCorpInfo(CorpName, CorpCode, CorpAddress, RegDate, RegFund, BusinessCode, BusinessType, LinkMan, LinkPhone, Fax, Email, CorpSite, corpType, "广东省", "深圳市", "深圳市住房和建设局", cUrl, ISOQualNum, ISOEnvironNum, OffAdr);

                            string result = Convert.ToString(ToolDb.ExecuteScalar(string.Format("select Id from CorpInfo where CorpName='{0}' and CorpType='{1}' and CorpCode='{2}' and InfoSource='{3}'", info.CorpName, info.CorpType, info.CorpCode, info.InfoSource)));

                            if (string.IsNullOrEmpty(result))
                            {
                                AddCorpInfo(info, ctx);
                            }
                            else
                            {
                                string delQual = string.Format("delete from CorpQual where CorpId='{0}'", result);
                                string delCorp = string.Format("delete from CorpInfo where Id='{0}'", result);
                                int delResult = 0;
                                if (ToolCoreDb.ExecuteSql(delQual) > 0)
                                    delResult = ToolCoreDb.ExecuteSql(delCorp);
                                if (delResult > 0)
                                    AddCorpInfo(info, ctx);
                            }
                            sqlCount++;
                            if (sqlCount >= 90)
                            {
                                sqlCount = 0;
                                Thread.Sleep(11 * 60 * 1000);
                            }
                        }
                    }
                }
            }
            return list;
        }

        private void AddCorpInfo(CorpInfo info, string ctx)
        {
            if (ToolDb.SaveEntity(info, string.Empty))
            {
                string QualName = string.Empty, QualCode = string.Empty, QualSeq = string.Empty, qualNum = string.Empty, QualType = string.Empty, QualLevel = string.Empty, ValidDate = string.Empty, LicDate = string.Empty, LicUnit = string.Empty;

                QualName = QualType = "物业管理";
                QualCode = ctx.GetRegexBegEnd("zzzshm.value", ";").GetReplace(new string[] { "=", "'" });
                LicDate = ctx.GetRegexBegEnd("zzzsfzrq.value", ";").GetReplace(new string[] { "=", "'" });
                QualLevel = ctx.GetRegexBegEnd("qyzzmc.value", ";").GetReplace(new string[] { "=", "'" });
                qualNum = QualLevel.GetLevel();
                CorpQual qual = ToolDb.GenCorpQual(info.Id, QualName, QualCode, QualSeq, QualType, QualLevel, ValidDate, LicDate, LicUnit, info.Url, qualNum, "广东省", "深圳市");
                ToolDb.SaveEntity(qual, string.Empty);
            }
        }
    }
}
