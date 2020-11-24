using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Web.UI.WebControls;

namespace Crawler.Instance
{
    public class CreditAppraiseSZ : WebSiteCrawller
    {
        public CreditAppraiseSZ()
            : base()
        { 
            this.Group = "企业评价信息";
            this.Title = "深圳市建设局企业诚信评价信用记录";
            this.MaxCount = 16000;
            this.Description = "自动抓取深圳市建设局企业诚信评价信用记录";
            this.ExistCompareFields = "CorpName,ProjectName,TargetCode,TargetDesc,TargetClass,ActionType,ActionDateTime";
            this.PlanTime = "05:35";
            this.SiteUrl = "http://61.144.226.2:8008/Credit/CreditListCount.aspx";
        }

        private delegate void NewMethod(bool crawlAll);
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            GetBuliang(crawlAll, list);
            GetLianghao(crawlAll, list);
         
            return list;
        }

        private IList GetLianghao(bool crawlAll, IList list)
        {
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            int pageInt = 1;
            int pageCount = 1;
            int count = 0;
            //IList list = new ArrayList();
            try
            {
                html = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            Parser parser = new Parser(new Lexer(html));
            viewState = ToolWeb.GetAspNetViewState(parser);
            eventValidation = ToolWeb.GetAspNetEventValidation(parser);
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "__VIEWSTATEENCRYPTED",
            "__EVENTVALIDATION","queryWhereAction","queryWhereType","queryWhere","txtquery","GridViewPaging1$txtGridViewPagingForwardTo"},
            new string[] { "GoodAction", "", viewState, "", eventValidation, "GoodAction", "", "", "", "1" });
            try
            {
                html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
            }
            catch { }
            parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "GridViewPaging1_lblGridViewPagingDesc")));
            if (nodeList != null && nodeList.Count > 0)
            {
                Regex reg = new Regex(@"共\d+页");
                try
                {
                    pageInt = int.Parse(reg.Match(nodeList.AsString()).Value.Trim(new char[] { '共', '页' }));
                }
                catch
                { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                pageCount++;
                if (pageCount > 10)
                {
                    pageCount = 1;
                    Thread.Sleep(300*1000);
                }
                if (i > 1)
                {
                    viewState = ToolWeb.GetAspNetViewState(html);
                    eventValidation = ToolWeb.GetAspNetEventValidation(html);
                    NameValueCollection nvc1 = ToolWeb.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "__VIEWSTATEENCRYPTED",
            "__EVENTVALIDATION","queryWhereAction","queryWhereType","queryWhere","txtquery","GridViewPaging1$txtGridViewPagingForwardTo","GridViewPaging1$btnForwardToPage"},
            new string[] { "", "", viewState, "", eventValidation, "GoodAction", "", "", "", i.ToString(), "GO" });
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc1, Encoding.UTF8);
                    }
                    catch { }
                }
                parser = new Parser(new Lexer(html));
                NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
                if (dtList != null && dtList.Count > 0)
                {
                    TableTag table = dtList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        Winista.Text.HtmlParser.Tags.TableRow tr = table.Rows[j];
                        string corpName = string.Empty, projectName = string.Empty, targetCode = string.Empty, targetDesc = string.Empty, targetClass = string.Empty,
                            actionDateTime = string.Empty, actionType = string.Empty, province = string.Empty, city = string.Empty, infoSource = string.Empty,
                            url = string.Empty;
                        corpName = tr.Columns[0].ToPlainTextString().Trim();
                        projectName = tr.Columns[1].ToPlainTextString().Trim();
                        targetCode = tr.Columns[2].ToPlainTextString().Trim();
                        targetDesc = tr.Columns[3].ToPlainTextString().Trim();
                        targetClass = tr.Columns[4].ToPlainTextString().Trim();
                        actionDateTime = tr.Columns[5].ToPlainTextString().Trim();
                        actionType = "良好行为";
                        province = "广东省";
                        city = "深圳市";
                        infoSource = "深圳市住房和建设局";
                        url = SiteUrl;
                        if (Encoding.Default.GetByteCount(targetDesc) > 200)
                            targetDesc = string.Empty;
                        CreditAppraise info = ToolDb.GenCreditAppraise(corpName, projectName, targetCode, targetDesc, targetClass, actionDateTime, actionType, province, city, infoSource, url);
                        //ToolDb.SaveEntity(info, this.ExistCompareFields);
                        list.Add(info);
                        count++;
                        if (!crawlAll && count >= this.MaxCount)
                            return list;
                    }
                }
            }
            return list;
        }

        private IList GetBuliang(bool crawlAll, IList list)
        {
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            int pageInt = 1;
            int pageCount = 1;
            int buCount = 0;
            //IList list = new ArrayList();
            try
            {
                html = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            Parser parser = new Parser(new Lexer(html));
            viewState = ToolWeb.GetAspNetViewState(parser);
            eventValidation = ToolWeb.GetAspNetEventValidation(parser);
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "__VIEWSTATEENCRYPTED",
            "__EVENTVALIDATION","queryWhereAction","queryWhereType","queryWhere","txtquery","GridViewPaging1$txtGridViewPagingForwardTo"},
            new string[] { "BtnQuery", "", viewState, "", eventValidation, "BadAction", "","","","1" });
            try
            {
                html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
            }
            catch { }
            parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "GridViewPaging1_lblGridViewPagingDesc")));
            if (nodeList != null && nodeList.Count > 0)
            {
                Regex reg = new Regex(@"共\d+页");
                try
                {
                    pageInt = int.Parse(reg.Match(nodeList.AsString()).Value.Trim(new char[] { '共', '页' }));
                }
                catch
                { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                pageCount++;
                if (pageCount > 10)
                {
                    pageCount = 1;
                    Thread.Sleep(300 * 1000);
                } 
                if (i > 1)
                {
                    viewState = ToolWeb.GetAspNetViewState(html);
                    eventValidation = ToolWeb.GetAspNetEventValidation(html);
                    NameValueCollection nvc1 = ToolWeb.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "__VIEWSTATEENCRYPTED",
            "__EVENTVALIDATION","queryWhereAction","queryWhereType","queryWhere","txtquery","GridViewPaging1$txtGridViewPagingForwardTo","GridViewPaging1$btnForwardToPage"},
            new string[] { "", "", viewState, "", eventValidation, "BadAction", "", "", "", i.ToString(),"GO" });
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc1, Encoding.UTF8);
                    }
                    catch { }
                }
                parser = new Parser(new Lexer(html));
                NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
                if (dtList != null && dtList.Count > 0)
                {
                    TableTag table = dtList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        Winista.Text.HtmlParser.Tags.TableRow tr = table.Rows[j];
                        string corpName = string.Empty, projectName = string.Empty, targetCode = string.Empty, targetDesc = string.Empty, targetClass = string.Empty,
                            actionDateTime = string.Empty, actionType = string.Empty, province = string.Empty, city = string.Empty, infoSource = string.Empty,
                            url = string.Empty;
                        corpName = tr.Columns[0].ToPlainTextString().Trim();
                        projectName = tr.Columns[1].ToPlainTextString().Trim();
                        targetCode = tr.Columns[2].ToPlainTextString().Trim();
                        targetDesc = tr.Columns[3].ToPlainTextString().Trim();
                        targetClass = tr.Columns[4].ToPlainTextString().Trim();
                        actionDateTime = tr.Columns[5].ToPlainTextString().Trim();
                        actionType = "不良行为";
                        province = "广东省";
                        city = "深圳市";
                        infoSource = "深圳市住房和建设局";
                        url = SiteUrl;
                        CreditAppraise info = ToolDb.GenCreditAppraise(corpName, projectName, targetCode, targetDesc, targetClass, actionDateTime, actionType, province, city, infoSource, url);
                        list.Add(info);
                        buCount++;
                        //ToolDb.SaveEntity(info, this.ExistCompareFields);
                        if (!crawlAll && buCount >= this.MaxCount)
                            return list;
                        
                    }
                }
            }
            return list;
        }
    }
}
