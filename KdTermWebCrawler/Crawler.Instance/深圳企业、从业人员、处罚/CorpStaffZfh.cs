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
    public class CorpStaffZfh : WebSiteCrawller
    {
        public CorpStaffZfh()
            : base()
        {
            this.PlanTime = "2 0:30";//"1-28 2:00,4-28 2:00,7-28 2:00,10-28 2:00";
            this.Group = "从业人员信息";
            this.Title = "广东省三库一平台从业人员信息";
            this.Description = "自动抓取广东省三库一平台从业人员信息";
            this.ExistCompareFields = "Url";
            this.ExistsUpdate = true;
            this.MaxCount = 100000;
            this.SiteUrl = "http://113.108.219.40/PlatForm/SearchCenter/PersonalRegSearch.aspx?sqe=0";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            string pageHtl = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_AspNetPager1")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList[0].ToPlainTextString().GetRegexBegEnd("共", "条");
                    int page = int.Parse(temp);
                    int result = page / 15;
                    if (page % 15 != 0)
                        pageInt = result + 1;
                    else
                        pageInt = result;
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                i = 500;
                if (i > 1)
                {
                    try
                    {
                        viewState = ToolWeb.GetAspNetViewState(html);
                        NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[]{
                          "__EVENTTARGET",
                          "__EVENTARGUMENT",
                          "__VIEWSTATE",
                          "ctl00$ContentPlaceHolder1$txtName",
                          "ctl00$ContentPlaceHolder1$txtIdNum",
                          "ctl00$ContentPlaceHolder1$txtEmpName",
                          "ctl00$ContentPlaceHolder1$txtEMP_ORG_CODE",
                          "ctl00$ContentPlaceHolder1$txtCertNum",
                          "ctl00$ContentPlaceHolder1$rdoIsDock"
                        },new string[]{
                        "ctl00$ContentPlaceHolder1$AspNetPager1",
                        i.ToString(),
                        viewState,
                        "","","","","","0"
                        });
                        html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc,Encoding.UTF8);
                    }
                    catch { }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "dataTable")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string Name = string.Empty, Sex = string.Empty, CredType = string.Empty, IdNum = string.Empty, CorpName = string.Empty, CorpCode = string.Empty, CertCode = string.Empty, CertGrade = string.Empty, RegLevel = string.Empty, RegCode = string.Empty, AuthorUnit = string.Empty, PersonType = string.Empty, Province = string.Empty, City = string.Empty, CreateTime = string.Empty, InfoSource = string.Empty, Url = string.Empty, Profession = string.Empty, staffNum = string.Empty, IssuanceTime = string.Empty, Organ = string.Empty, CertState=string.Empty;

                        TableRow tr = table.Rows[j];
                        Name = tr.Columns[1].ToNodePlainString();
                        RegCode = tr.Columns[2].ToNodePlainString(); 
                        CertCode = tr.Columns[3].ToNodePlainString();
                        CorpName = tr.Columns[5].ToNodePlainString();
                        PersonType = tr.Columns[4].ToNodePlainString();
                        CertGrade = tr.Columns[6].ToNodePlainString();
                        string htldtl = string.Empty;
                        Url = "http://113.108.219.40/PlatForm/SearchCenter/" + tr.Columns[2].GetATagHref();
                        string sexUrl = "http://113.108.219.40/PlatForm/SearchCenter/" + tr.Columns[1].GetATagHref();
                        try
                        {
                            string htl = ToolWeb.GetHtmlByUrl(sexUrl, Encoding.UTF8);
                            parser = new Parser(new Lexer(htl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                TableTag tab = dtlList[0] as TableTag;
                                string ctx = string.Empty;
                                for (int k = 0; k < tab.RowCount; k++)
                                {
                                    for (int d = 0; d < tab.Rows[k].ColumnCount; d++)
                                    {
                                        if ((d + 1) % 2 == 0)
                                            ctx += tab.Rows[k].Columns[d].ToNodePlainString() + "\r\n";
                                        else
                                            ctx += tab.Rows[k].Columns[d].ToNodePlainString().Replace("：", "").Replace(":", "") + "：";
                                    }
                                }
                                Sex = ctx.GetRegex(new string[] { "性别" });
                            }
                        }
                        catch { }
                        try
                        {
                            htldtl = ToolWeb.GetHtmlByUrl(Url, Encoding.UTF8);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                        if (dtList != null && dtList.Count > 0)
                        {
                            TableTag tab = dtList[0] as TableTag;
                            string ctx = string.Empty;
                            for (int k = 0; k < tab.RowCount; k++)
                            {
                                for (int d = 0; d < tab.Rows[k].ColumnCount; d++)
                                {
                                    if ((d + 1) % 2 == 0)
                                        ctx += tab.Rows[k].Columns[d].ToNodePlainString() + "\r\n";
                                    else
                                        ctx += tab.Rows[k].Columns[d].ToNodePlainString().Replace("：", "").Replace(":", "") + "：";
                                }
                            }
                            IssuanceTime = ctx.GetRegex(new string[] { "签发日期", "日期" });
                            CertState = ctx.GetRegex(new string[] { "证书状态" });
                            Organ = ctx.GetRegex(new string[] { "发证机关" });

                            staffNum = CertGrade.GetLevel();
                              
                            CorpStaff corpStaff = ToolDb.GenCorpStaff(Name, Sex, CredType, IdNum, CorpName, CorpCode, CertCode, RegLevel, RegCode, AuthorUnit, PersonType, CertGrade, "广东省", "广东地区", "广东省住房和城乡建设厅", Url, Profession, staffNum, IssuanceTime, Organ, CertState);
                            ToolDb.SaveEntity(corpStaff, this.ExistCompareFields, this.ExistsUpdate);
                        }
                    }
                }
            }
            return null;
        }
    }
}
