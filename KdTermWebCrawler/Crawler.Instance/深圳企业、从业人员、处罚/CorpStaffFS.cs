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
    public class CorpStaffFS : WebSiteCrawller
    {
        public CorpStaffFS()
            : base()
        {
            this.PlanTime = "2 1:30";//"1-28 2:00,4-28 2:00,7-28 2:00,10-28 2:00";
            this.Group = "从业人员信息";
            this.Title = "广东省佛山市住房和城乡建设管理局";
            this.Description = "自动抓取广东省佛山市住房和城乡建设管理局从业人员信息";
            this.ExistCompareFields = "Name,Sex,CredType,CorpName,CorpCode,CertCode,InfoSource,PersonType";
            this.ExistsUpdate = true;
            this.MaxCount = 100000;
            this.SiteUrl = "http://119.145.135.38/fscx/web/tab4List.do?tab=4&kind=zyxx";
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
            catch { return null; }
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
                        new string[] { string.Empty, i.ToString(), "15", "4", "zyxx" }
                        );
                    html = ToolWeb.GetHtmlByUrl("http://119.145.135.38/fscx/web/tab4List.do", nvc, Encoding.Default);
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
                        string Name = string.Empty, Sex = string.Empty, CredType = string.Empty, IdNum = string.Empty, CorpName = string.Empty, CorpCode = string.Empty, CertCode = string.Empty, CertGrade = string.Empty, RegLevel = string.Empty, RegCode = string.Empty, AuthorUnit = string.Empty, PersonType = string.Empty, Province = string.Empty, City = string.Empty, CreateTime = string.Empty, InfoSource = string.Empty, Url = string.Empty, Profession = string.Empty, staffNum = string.Empty, IssuanceTime = string.Empty, Organ = string.Empty;

                        TableRow tr = table.Rows[j];
                        Name = tr.Columns[0].ToNodePlainString();
                        CorpName = tr.Columns[1].ToNodePlainString();
                        CertCode = tr.Columns[2].ToNodePlainString().Replace(".", "");
                        IssuanceTime = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        Regex regexLink = new Regex(@"\?id=[^&]+");
                        string temp = tr.GetAttribute("onclick").GetRegexBegEnd("'", "'");
                        string ids = regexLink.Match(temp).Value;
                        Url = "http://119.145.135.38/fscx/web/tab4Detail.do" + ids;
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = ToolWeb.GetHtmlByUrl(Url, Encoding.Default);
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
                            Sex = ctx.GetRegex("性别"); 
                            CorpCode = ctx.GetRegex("所在单位机构代码");
                            PersonType = ctx.GetRegex("专业");
                        }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList cDtList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "tabs1-1")), true), new TagNameFilter("table")));
                        if (cDtList != null && cDtList.Count > 0)
                        {
                            TableTag tab = cDtList[0] as TableTag;
                            for (int k = 1; k < tab.RowCount; k++)
                            { 
                                TableRow dr = tab.Rows[k];
                                string code = dr.Columns[0].ToNodePlainString();
                                if (code.Contains(CertCode))
                                {
                                    CertCode = code;
                                    CredType = dr.Columns[2].ToNodePlainString();
                                    CertGrade = dr.Columns[3].ToNodePlainString();
                                    string type = dr.Columns[4].ToNodePlainString();
                                    if (!string.IsNullOrEmpty(type))
                                        PersonType = type;
                                    Organ = dr.Columns[5].ToNodePlainString();
                                    staffNum = CertGrade.GetLevel();
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                        if (PersonType == "-" || PersonType == "/")
                        {
                            PersonType = string.Empty;
                        }
                        CorpStaff corpStaff = ToolDb.GenCorpStaff(Name, Sex, CredType, IdNum, CorpName, CorpCode, CertCode, RegLevel, RegCode, AuthorUnit, PersonType, CertGrade, "广东省", "佛山市", "佛山市住房和城乡建设管理局", Url, Profession, staffNum, IssuanceTime, Organ,"");
                        ToolDb.SaveEntity(corpStaff, this.ExistCompareFields, this.ExistsUpdate);
                    }
                }
            }
            return null;
        }
    }
}
