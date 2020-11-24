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
    public class CreditActionSZ : WebSiteCrawller
    { 
        public CreditActionSZ()
            : base()
        {
            this.IsCrawlAll = true;
            this.Group = "企业评价信息";
            this.Title = "深圳市建设局企业诚信行为记录";
            this.MaxCount = 5000;
            this.Description = "自动抓取深圳市建设局企业诚信行为记录";
            this.ExistCompareFields = "CorpCode,CorpName,TargetCode,TargetDesc,TargetClass,TargetLevel,TargetUnit,ActionType,DocNo";
            this.PlanTime = "05:35";
            this.SiteUrl = "http://61.144.226.2:8008/GeneralGs.aspx";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            GetBuliang(crawlAll); 
            GetLianghao(crawlAll); 
            return list;
        }

        private void GetLianghao(bool crawlAll)
        {
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            int pageInt = 1;
            IList list = new ArrayList();
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
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] { "ScriptManager1", "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE",
            "txtName","ddlType","GridViewPaging1$txtGridViewPagingForwardTo","__VIEWSTATEENCRYPTED","__EVENTVALIDATION","btnOK"},
            new string[] { "UpdatePanel1|btnOK", "", "", viewState, "", "GoodAction", "1", "", eventValidation, "查询" });
            try
            {
                html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
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
                if (i > 1)
                {
                    viewState = ToolWeb.GetAspNetViewState(html);
                    eventValidation = ToolWeb.GetAspNetEventValidation(html);
                    ToolWeb.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "__VIEWSTATEENCRYPTED",
                    "__EVENTVALIDATION","txtName","ddlType","GridViewPaging1$txtGridViewPagingForwardTo","GridViewPaging1$btnForwardToPage"},
                      new string[] { "", "", viewState, "", eventValidation, "", "GoodAction", i.ToString(), "GO" });
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
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
                        string corpCode = string.Empty, corpName = string.Empty, targetCode = string.Empty, targetDesc = string.Empty,
                            targetClass = string.Empty, targetLevel = string.Empty, targetUnit = string.Empty, docNo = string.Empty, beginDateTime = string.Empty,
                            actionDateTime = string.Empty, actionType = string.Empty, province = string.Empty, city = string.Empty, infoSource = string.Empty, url = string.Empty, prjName = string.Empty ;
                        Winista.Text.HtmlParser.Tags.TableRow tr = table.Rows[j];
                        corpCode = tr.Columns[1].ToPlainTextString().Trim();
                        corpName = tr.Columns[2].ToPlainTextString().Trim();
                        targetCode = tr.Columns[4].ToPlainTextString().Trim();
                        targetDesc = tr.Columns[5].ToPlainTextString().Trim();
                        targetClass = tr.Columns[6].ToPlainTextString().Trim();
                        targetLevel = tr.Columns[7].ToPlainTextString().Trim();
                        targetUnit = tr.Columns[8].ToPlainTextString().Trim();
                        docNo = tr.Columns[9].ToPlainTextString().Trim();
                        beginDateTime = tr.Columns[10].ToPlainTextString().Trim();
                        actionDateTime = tr.Columns[11].ToPlainTextString().Trim();
                        actionType = "良好行为";
                        CreditAction info = ToolDb.GenCreditAction(corpCode, corpName, targetCode, targetDesc, targetClass, targetLevel, targetUnit,
                            docNo, beginDateTime, actionDateTime, actionType, "广东省", "深圳市", "深圳市住房和建设局", SiteUrl,prjName);
                        ToolDb.SaveEntity(info, this.ExistCompareFields);
                    }
                }
            } 
        }


        private void GetBuliang(bool crawlAll)
        {
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            int pageInt = 1;
            IList list = new ArrayList();
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
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] { "ScriptManager1", "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE",
            "txtName","ddlType","GridViewPaging1$txtGridViewPagingForwardTo","__VIEWSTATEENCRYPTED","__EVENTVALIDATION","btnOK"},
            new string[] { "UpdatePanel1|btnOK", "", "", viewState, "", "BadAction", "", "", eventValidation, "查询" });
            try
            {
                html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
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
                if (i > 1)
                {
                    viewState = ToolWeb.GetAspNetViewState(html);
                    eventValidation = ToolWeb.GetAspNetEventValidation(html);
                    NameValueCollection nvc1 = ToolWeb.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "__VIEWSTATEENCRYPTED",
                    "__EVENTVALIDATION","txtName","ddlType","GridViewPaging1$txtGridViewPagingForwardTo","GridViewPaging1$btnForwardToPage"},
                    new string[] { "", "", viewState, "", eventValidation, "", "BadAction", i.ToString(), "GO" });
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc1, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
                if (dtList != null && dtList.Count > 0)
                {
                    TableTag table = dtList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string corpCode = string.Empty, corpName = string.Empty, targetCode = string.Empty, targetDesc = string.Empty,
                            targetClass = string.Empty, targetLevel = string.Empty, targetUnit = string.Empty, docNo = string.Empty, beginDateTime = string.Empty,
                            actionDateTime = string.Empty, actionType = string.Empty, province = string.Empty, city = string.Empty, infoSource = string.Empty, url = string.Empty,prjName=string.Empty;
                        Winista.Text.HtmlParser.Tags.TableRow tr = table.Rows[j];
                        corpCode = tr.Columns[1].ToPlainTextString().Trim();
                        corpName = tr.Columns[2].ToPlainTextString().Trim();
                        prjName = tr.Columns[3].ToPlainTextString().Trim();
                        targetCode = tr.Columns[4].ToPlainTextString().Trim();
                        targetDesc = tr.Columns[5].ToPlainTextString().Trim();
                        targetClass = tr.Columns[6].ToPlainTextString().Trim();
                        targetLevel = tr.Columns[7].ToPlainTextString().Trim();
                        targetUnit = tr.Columns[8].ToPlainTextString().Trim();
                        docNo = tr.Columns[9].ToPlainTextString().Trim();
                        beginDateTime = tr.Columns[10].ToPlainTextString().Trim();
                        actionDateTime = tr.Columns[11].ToPlainTextString().Trim();
                        actionType = "不良行为";
                        CreditAction info = ToolDb.GenCreditAction(corpCode, corpName, targetCode, targetDesc, targetClass, targetLevel, targetUnit,
                            docNo, beginDateTime, actionDateTime, actionType, "广东省", "深圳市", "深圳市住房和建设局", SiteUrl, prjName);
                        ToolDb.SaveEntity(info, this.ExistCompareFields);
                    }
                }
            } 
        }
    }
}
