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
    public class CorpStaffSky : WebSiteCrawller
    {
        public CorpStaffSky()
            : base(true)
        {
            this.PlanTime = "1 23:30,8 23:30,16 23:30,24 23:30";
            this.Group = "企业信息";
            this.Title = "全国建筑市场监管公共服务平台人员信息（四库一平台）";
            this.Description = "自动抓取全国建筑市场监管公共服务平台人员信息（四库一平台）";
            this.ExistCompareFields = "Name,Sex,CredType,CorpName,CorpCode,CertCode,InfoSource,PersonType";
            this.MaxCount = 50000;
            this.SiteUrl = "http://jzsc.mohurd.gov.cn/dataservice/query/staff/list";
            
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            int count = 0;
            IList list = new List<CorpStaff>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
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
            int totalPage = 0;
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "clearfix")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().Replace(",", "，");
                    string page = temp.GetRegexBegEnd("total", "，").GetReplace("\":");
                    totalPage = int.Parse(page);
                    pageInt = totalPage / 15 + 1;
                }
                catch { }
            }
            for (int p = 1; p <= pageInt; p++)
            {
                if (p > 1)
                {
                    Logger.Error(p);
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] {
                            "$total",
                            "$reload",
                            "$pg",
                            "$pgsz"
                        },
                            new string[] {
                                totalPage.ToString(),
                                "0",
                                p.ToString(),
                                "15"
                            });
                    try
                    {  
                        html = ToolWeb.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        try
                        {
                            Thread.Sleep(60 * 1000 * 6);
                            html = ToolWeb.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                        }
                        catch
                        {
                            try
                            {
                                Thread.Sleep(60 * 1000 * 6);
                                html = ToolWeb.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table_box responsive personal")));

                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int i = 1; i < table.RowCount - 1; i++)
                    {
                        TableRow tr = table.Rows[i];

                        string Name = string.Empty, Sex = string.Empty, CredType = string.Empty, IdNum = string.Empty, CorpName = string.Empty, CorpCode = string.Empty, CertCode = string.Empty, CertGrade = string.Empty, RegLevel = string.Empty, RegCode = string.Empty, AuthorUnit = string.Empty, PersonType = string.Empty, Province = string.Empty, City = string.Empty, CreateTime = string.Empty, InfoSource = string.Empty, Url = string.Empty, Profession = string.Empty, staffNum = string.Empty, IssuanceTime = string.Empty, Organ = string.Empty;

                        Name = tr.Columns[1].ToNodePlainString();
                        IdNum = tr.Columns[2].ToNodePlainString();
                        CertGrade = tr.Columns[3].ToNodePlainString();
                        RegCode = tr.Columns[4].ToNodePlainString();
                        PersonType = tr.Columns[5].ToNodePlainString();
                        ATag aTag = tr.Columns[1].GetATag();
                        Url = "http://jzsc.mohurd.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = ToolWeb.GetHtmlByUrl(Url, Encoding.UTF8).GetJsString();
                        }
                        catch
                        {
                            try
                            {
                                Thread.Sleep(60 * 1000 * 6);
                                htmldtl = ToolWeb.GetHtmlByUrl(Url, Encoding.UTF8).GetJsString();
                            }
                            catch
                            {
                                try
                                {
                                    Thread.Sleep(60 * 1000 * 6);
                                    htmldtl = ToolWeb.GetHtmlByUrl(Url, Encoding.UTF8).GetJsString();
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        }

                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "activeTinyTabContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            string ctx = dtlNode.AsHtml().GetReplace("</dd>", "\r\n").ToCtxString();
                            Sex = ctx.GetRegex("性别");
                        }
                        parser.Reset();
                        dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "regcert_tab")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            string ctx = dtlNode.AsHtml().GetReplace("</dd>", "\r\n").ToCtxString();
                            CertCode = ctx.GetRegex("证书编号");
                            ATag nameTag = dtlNode.GetATag(1);
                            if (nameTag != null)
                                CorpName = nameTag.LinkText.ToNodeString();
                        }

                        CorpStaff corpStaff = ToolDb.GenCorpStaff(Name, Sex, CredType, IdNum, CorpName, CorpCode, CertCode, RegLevel, RegCode, AuthorUnit, PersonType, CertGrade, "全国", "", "中华人民共和国住房和城乡建设部建筑市场监管司", Url, Profession, staffNum, IssuanceTime, Organ, "");
                        ToolDb.SaveEntity(corpStaff, this.ExistCompareFields, this.ExistsUpdate);

                        count++;

                        if (count >= 28)
                        {
                            count = 0;
                            Thread.Sleep(60 * 1000 * 6);
                        }
                    }
                }
            }
            return null;
        }
    }
}
