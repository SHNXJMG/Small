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
    public class ZgGongXinBuCorp : WebSiteCrawller
    {
        public ZgGongXinBuCorp()
            : base()
        {
            this.PlanTime = "10 10 06:00,4 10 06:00";//"1-28 2:00,4-28 2:00,7-28 2:00,10-28 2:00";
            this.Group = "企业信息";
            this.Title = "中华人民共和国工业和信息化部";
            this.Description = "自动抓取中华人民共和国工业和信息化部";
            this.ExistCompareFields = "CorpType,CorpName";
            this.MaxCount = 50000;
            this.SiteUrl = "http://gzly.miit.gov.cn:8080/datainfo/miit/miit_jczzqymd.jsp";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<CorpInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1, count = 0;
            string eventValidation = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("align", "center")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string countTemp = pageNode.AsString().GetRegexBegEnd("\r", "条").Replace("&nbsp;", "").Replace("\r", "").Replace("\n", "");
                    string temp = pageNode.AsString().GetRegexBegEnd("/", "页").Replace("&nbsp;", "");
                    pageInt = int.Parse(temp);
                    count = int.Parse(countTemp);

                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(
                        new string[] { "datainfo_id", "datainfo_action", "count", "pages", "page", "dwmc", "zzdj", "zsbh", "szss" },
                        new string[] { string.Empty, string.Empty, count.ToString(), pageInt.ToString(), i.ToString(), string.Empty, string.Empty, string.Empty, string.Empty }
                        );
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "list-table")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    { 
                        TableRow tr = table.Rows[j];
                        string CorpId = string.Empty, QualName = string.Empty, QualCode = string.Empty, QualSeq = string.Empty, qualNum = string.Empty, QualType = string.Empty, QualLevel = string.Empty, ValidDate = string.Empty, LicDate = string.Empty, LicUnit = string.Empty;

                        QualName = "计算机信息系统集成";
                        QualCode = tr.Columns[3].ToNodePlainString();
                        QualLevel = tr.Columns[2].ToNodePlainString();
                        LicDate = tr.Columns[5].ToPlainTextString().GetDateRegex();
                        qualNum = QualLevel.GetLevel();
                        string corpName = tr.Columns[1].ToNodePlainString();
                        string city = tr.Columns[6].ToNodePlainString();
                        object isCorp = ToolDb.ExecuteScalar("select Id from CorpInfo where CorpName='"+corpName+"'");
                        if (isCorp == null || isCorp.ToString() == "")
                        {
                            string CorpName = string.Empty, CorpCode = string.Empty, CorpAddress = string.Empty,
                                          RegDate = string.Empty, RegFund = string.Empty, BusinessCode = string.Empty,
                                          BusinessType = string.Empty, LinkMan = string.Empty, LinkPhone = string.Empty,
                                          Fax = string.Empty, Email = string.Empty, CorpSite = string.Empty, cUrl = string.Empty,
                                          ISOQualNum = string.Empty, ISOEnvironNum = string.Empty, OffAdr = string.Empty, Cert = string.Empty, ctxKc = string.Empty,corpRz = string.Empty;
                            CorpInfo info = ToolDb.GenCorpInfo(corpName, CorpCode, CorpAddress, RegDate, RegFund, BusinessCode, BusinessType, LinkMan, LinkPhone, Fax, Email, CorpSite, string.Empty, city, city, "中华人民共和国工业和信息化部", this.SiteUrl, ISOQualNum, ISOEnvironNum, OffAdr);
                            if (ToolDb.SaveEntity(info, null))
                            {
                                CorpQual qual = ToolDb.GenCorpQual(info.Id, QualName, QualCode, QualSeq, QualType, QualLevel, ValidDate, LicDate, LicUnit, info.Url, qualNum, city, city);
                                ToolDb.SaveEntity(qual, "");
                            }
                        }
                        else
                        {
                            CorpQual qual = ToolDb.GenCorpQual(isCorp.ToString(), QualName, QualCode, QualSeq, QualType, QualLevel, ValidDate, LicDate, LicUnit, this.SiteUrl, qualNum, city, city);
                            ToolDb.SaveEntity(qual, "QualCode,CorpId,QualName", true);
                        }
                    }
                }
            }
            return list;
        }
    }
}
