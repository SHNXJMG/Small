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
    public class CorpCreditJDSZ : WebSiteCrawller
    {
        public CorpCreditJDSZ()
            : base()
        {
            this.IsCrawlAll = true;
            this.Group = "企业评价信息";
            this.Title = "深圳市建设局企业阶段得分";
            this.MaxCount = 5000;
            this.Description = "自动抓取深圳市建设局企业阶段得分";
            this.ExistCompareFields = "CorpName,CorpRank,CorpCategory,Ranking,CategoryRank,RealScore,CalcuEndDate";
            this.PlanTime = "05:35";
            this.SiteUrl = "http://61.144.226.2:8008/JDList.aspx";
        }
        int count = 0,sqlcount=0;
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string bidhtml = string.Empty;
            string html = string.Empty;
            int pageInt = 1;
            try
            {
                html = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "GridView1"), new TagNameFilter("table")));
            if (nodeList != null && nodeList.Count > 0)
            {
                TableTag table = nodeList[0] as TableTag;
                for (int i = 1; i < table.RowCount; i++)
                {
                    Winista.Text.HtmlParser.Tags.TableRow tr = table.Rows[i];
                    ATag alink = tr.Columns[8].SearchFor(typeof(ATag), true)[0] as ATag;
                    string view = string.Empty;
                    string even = string.Empty;
                    view = ToolWeb.GetAspNetViewState(html);
                    even = ToolWeb.GetAspNetEventValidation(html);
                    string alin = alink.Link.Replace("__doPostBack('", "").Replace("','')", "");
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "GridViewPaging1$txtGridViewPagingForwardTo", "__VIEWSTATEENCRYPTED", "__EVENTVALIDATION" },
                        new string[] { alin, "", view, "1", "", even });
                    string cookies = string.Empty;
                    try
                    {
                        bidhtml = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookies);
                    }
                    catch (Exception ex) { Logger.Error(ex.ToString()); }


                    for (int l = 1; l <= 14; l++)
                    {
                        if (l == 7) continue;
                        Save(l, bidhtml, list, crawlAll); 
                    }
                }
            }
            if (sqlcount > 100)
            {
                string sql = string.Format("update CorpCreditjd set IsNew='0' where CreateTime<'{0}'",DateTime.Now.ToString("yyyy-MM-dd"));
                ToolDb.ExecuteSql(sql);
            }
            return list;
        }

        private void Save(int l, string bidhtml, IList list, bool crawlAll)
        {
            string Url = "http://61.144.226.2:8008/JDScore.aspx?clearPaging=true&guid=450845";
            string htl = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiedtstr = string.Empty;
            try
            {
                htl = ToolWeb.GetHtmlByUrl(Url, Encoding.UTF8, ref cookiedtstr);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }

            string[] classLen = new string[] { "A", "A-", "B", "B-", "C", "C-" };
            string ddlindex = l.ToString();
            if (l > 13)
            {
                ddlindex = "999999999";
            }
            for (int n = 0; n < classLen.Length; n++)
            {

                int pageInt = 1;
                viewState = ToolWeb.GetAspNetViewState(htl);
                eventValidation = ToolWeb.GetAspNetEventValidation(htl);
                string strcookie = string.Empty;
                NameValueCollection nvc3 = ToolWeb.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT",
                                "__LASTFOCUS", "__VIEWSTATE", "txtCorpName", "DropDownList1", "DropDownList2", "hiddenIsFirst", "GridViewPaging1$txtGridViewPagingForwardTo", "GridViewPaging1$btnForwardToPage", "__VIEWSTATEENCRYPTED", "__EVENTVALIDATION" },
                    new string[] { "", "", "", viewState, "", classLen[n], ddlindex, "false", "1", "Go", "", eventValidation });
                try
                {
                    htl = ToolWeb.GetHtmlByUrl(Url, nvc3, Encoding.UTF8, ref strcookie);
                }
                catch (Exception ex) { }
                Parser parser = new Parser(new Lexer(htl));
                NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "GridViewPaging1_lblGridViewPagingDesc")));

                if (dtList != null && dtList.Count > 0)
                {
                    Regex reg = new Regex(@"共\d+页");
                    try
                    {
                        pageInt = int.Parse(reg.Match(dtList.AsString()).Value.Trim(new char[] { '共', '页' }));
                    }
                    catch
                    { }
                }
                for (int k = 1; k <= pageInt; k++)
                {
                    if (k > 1)
                    {
                        string viewState1 = ToolWeb.GetAspNetViewState(htl);
                        string eventValidation1 = ToolWeb.GetAspNetEventValidation(htl);
                        NameValueCollection nvc4 = ToolWeb.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT",
                                    "__LASTFOCUS", "__VIEWSTATE", "txtCorpName", "DropDownList1", "DropDownList2", "hiddenIsFirst", "GridViewPaging1$txtGridViewPagingForwardTo", "GridViewPaging1$btnForwardToPage", "__VIEWSTATEENCRYPTED", "__EVENTVALIDATION" },
                        new string[] { "", "", "", viewState1, "", classLen[n], ddlindex, "false", k.ToString(), "Go", "", eventValidation1 });
                        try
                        {
                            htl = ToolWeb.GetHtmlByUrl(Url, nvc4, Encoding.UTF8, ref strcookie);
                        }

                        catch (Exception ex) { }
                    }
                    string beg = string.Empty, end = string.Empty, avg = string.Empty, type = string.Empty, thtype = string.Empty, classlv = string.Empty;
                    Parser parserCtx = new Parser(new Lexer(htl));
                    NodeList ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("id", "DropDownList1")));
                    classlv = ctxNode.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("option"), new HasAttributeFilter("value", classLen[n])), true).AsString().Replace("&nbsp;", "");

                    Parser parserCtx2 = new Parser(new Lexer(htl));
                    NodeList ctxNode2 = parserCtx2.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("id", "DropDownList2")));
                    thtype = ctxNode2.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("option"), new HasAttributeFilter("value", ddlindex)), true).AsString().Replace("&nbsp;", "");


                    Parser dtparser = new Parser(new Lexer(htl));
                    NodeList delList = dtparser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView2")));
                    if (delList != null && delList.Count > 0)
                    {
                        TableTag tab = delList[0] as TableTag;
                        for (int e = 1; e < tab.RowCount; e++)
                        {
                            Winista.Text.HtmlParser.Tags.TableRow trdate = tab.Rows[e];
                            type = trdate.Columns[0].ToPlainTextString().Trim();
                            beg = trdate.Columns[1].ToPlainTextString().Trim();
                            end = trdate.Columns[2].ToPlainTextString().Trim();
                            Regex regInt = new Regex(@"\d{1,}[\.]?\d{0,}");
                            string temp = trdate.Columns[3].ToPlainTextString();
                            avg = regInt.Match(temp).Value;
                        }
                    }
                    Parser par = new Parser(new Lexer(htl));
                    NodeList conList = par.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
                    if (conList != null && conList.Count > 0)
                    {
                        TableTag tabContent = conList[0] as TableTag;
                        for (int f = 1; f < tabContent.RowCount; f++)
                        {
                            Winista.Text.HtmlParser.Tags.TableRow dr = tabContent.Rows[f];
                            string corpName = string.Empty, corpType = string.Empty, corpRank = string.Empty, corpCategory = string.Empty,
                                ranking = string.Empty, categoryRank = string.Empty, realScore = string.Empty, province = string.Empty,
                                city = string.Empty, infoSource = string.Empty, infourl = string.Empty, beginDate = string.Empty,
                                endDate = string.Empty, bidhtl = string.Empty, bad = string.Empty, good = string.Empty;
                            if (dr.ColumnCount > 7)
                            {
                                corpName = dr.Columns[1].ToPlainTextString().Trim();
                                categoryRank = dr.Columns[6].ToPlainTextString().Trim();
                                ranking = dr.Columns[5].ToPlainTextString().Trim();
                                string rea = dr.Columns[7].ToPlainTextString().Trim();
                                string goodStr = dr.Columns[3].ToPlainTextString().Trim();
                                string badStr = dr.Columns[4].ToPlainTextString().Trim();
                                Regex regInt = new Regex(@"\d{1,}[\.]?\d{0,}");
                                realScore = regInt.Match(rea).Value;
                                good = regInt.Match(goodStr).Value;
                                bad = regInt.Match(badStr).Value;
                                beginDate = beg;
                                endDate = end;
                                corpCategory = thtype;
                                corpRank = classlv;
                                infourl = Url;
                                corpType = type;
                                infoSource = "深圳市住房和建设局";
                                province = "广东省";
                                city = "深圳市";
                                bidhtl = bidhtml;
                            }
                            else
                            {
                                corpName = dr.Columns[1].ToPlainTextString().Trim();
                                categoryRank = dr.Columns[5].ToPlainTextString().Trim();
                                ranking = dr.Columns[4].ToPlainTextString().Trim();
                                string rea = dr.Columns[6].ToPlainTextString().Trim();
                                string goodStr = dr.Columns[2].ToPlainTextString().Trim();
                                string badStr = dr.Columns[3].ToPlainTextString().Trim();
                                Regex regInt = new Regex(@"\d{1,}[\.]?\d{0,}");
                                realScore = regInt.Match(rea).Value;
                                good = regInt.Match(goodStr).Value;
                                bad = regInt.Match(badStr).Value;
                                beginDate = beg;
                                endDate = end;
                                corpCategory = thtype;
                                corpRank = classlv;
                                infourl = Url;
                                corpType = type;
                                infoSource = "深圳市住房和建设局";
                                province = "广东省";
                                city = "深圳市";
                                bidhtl = bidhtml;
                            }
                            CorpCreditjd info = ToolDb.GenCorpCreditJD(corpName, corpType, corpRank, corpCategory, ranking, categoryRank, beginDate, endDate, realScore, province, city, infoSource, infourl, bidhtl, avg, good, bad); 
                            ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate);
                            count++;
                            sqlcount++;
                            //if (!crawlAll && list.Count >= this.MaxCount) break;
                            if (count > 200)
                            {
                                count = 0;
                                Thread.Sleep(120000);
                            }
                        }
                    }
                    
                }
            }
        }
    }
}
