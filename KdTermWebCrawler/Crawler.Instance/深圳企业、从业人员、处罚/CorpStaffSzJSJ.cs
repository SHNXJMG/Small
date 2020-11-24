using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Windows.Forms;
using System.Web;
using System.Text.RegularExpressions;
using System.Collections.Specialized;


namespace Crawler.Instance
{
    /// <summary>
    /// 深圳建设局从业人员信息
    /// </summary>
    public class CorpStaffSzJSJ : WebSiteCrawller
    {
        public CorpStaffSzJSJ()
            : base(true)
        {
            this.IsCrawlAll = true;
            this.PlanTime = "8-29 18:00,3-04 0:00,6-04 0:00,9-04 0:00,12-04 0:00";
            this.Group = "从业人员信息";
            this.Title = "广东省深圳市";
            this.Description = "自动抓取广东省深圳市从业人员信息";
            this.MaxCount = 50000;
            this.ExistCompareFields = "Name,Sex,CredType,IdNum,CorpName,CorpCode,CertCode,RegCode,PersonType,Province,City,Url";
            //TODO: 检查路径是否可用
            this.SiteUrl = null;
        }

        public string GetStartUrl(int path)
        {
            return "http://61.144.226.2/ryxx/browse.aspx?category=" + path;
        }

        private delegate void NewMethod(int path, bool crawlAll, IList list);
        private delegate void NewType(bool crawlAll, IList list);
        protected override IList ExecuteCrawl(bool crawlAll)
        {

            IList list = new ArrayList();
            //GetCorpStaffDui(crawlAll, list);
            //GetCorpStaffShuili(crawlAll, list);
            //GetCorpStaffXiao(crawlAll, list);
            NewType newType1 = new NewType(GetCorpStaffDui);
            IAsyncResult irr1 = newType1.BeginInvoke(crawlAll, list, null, null);
            NewType newTypw2 = new NewType(GetCorpStaffShuili);
            IAsyncResult irr2 = newTypw2.BeginInvoke(crawlAll, list, null, null);
            NewType newType3 = new NewType(GetCorpStaffXiao);
            IAsyncResult irr3 = newType3.BeginInvoke(crawlAll, list, null, null);
            NewMethod newMeth1 = new NewMethod(GetCorpStaffJzao);
            IAsyncResult ir1 = newMeth1.BeginInvoke(2, crawlAll, list, null, null);
            NewMethod newMeth2 = new NewMethod(GetCorpStaffJLi);
            IAsyncResult ir2 = newMeth2.BeginInvoke(3, crawlAll, list, null, null);
            NewMethod newMeth3 = new NewMethod(GetCorpStaffZJia);
            IAsyncResult ir3 = newMeth3.BeginInvoke(4, crawlAll, list, null, null);
            NewMethod newMeth4 = new NewMethod(GetCorpStaffZLZR);
            IAsyncResult ir4 = newMeth4.BeginInvoke(7, crawlAll, list, null, null);
            NewMethod newMeth5 = new NewMethod(GetCorpStaffAQZR);
            IAsyncResult ir5 = newMeth5.BeginInvoke(8, crawlAll, list, null, null);
            NewMethod newMeth6 = new NewMethod(GetCorpStaffXXXM);
            IAsyncResult ir6 = newMeth6.BeginInvoke(1, crawlAll, list, null, null);
            NewMethod newMeth7 = new NewMethod(GetCorpStaffJZGCS);
            IAsyncResult ir7 = newMeth7.BeginInvoke(5, crawlAll, list, null, null);
            NewMethod newMeth8 = new NewMethod(GetCorpStaffJGGCS);
            IAsyncResult ir8 = newMeth8.BeginInvoke(6, crawlAll, list, null, null);



            //GetCorpStaffJzao(2, crawlAll, list);//采集建造工程师
            //GetCorpStaffJLi(3, crawlAll, list);//采集监理工程师
            //GetCorpStaffZJia(4, crawlAll, list); //采集造价工程师
            //GetCorpStaffZLZR(7, crawlAll, list);//采集质量主任
            //GetCorpStaffAQZR(8, crawlAll, list);//采集安全主任
            //GetCorpStaffXXXM(1, crawlAll, list);// 采集小型工程项目负责人
            //GetCorpStaffJZGCS(5, crawlAll, list);// 采集建筑工程师
            //GetCorpStaffJGGCS(6, crawlAll, list);//采集结构工程师
            return list;
        }

        /// <summary>
        /// 采集水利监理工程师  http://61.144.226.2/ryxx/browse_sljl.aspx
        /// </summary>
        /// <param name="path"></param>
        /// <param name="crawlAll"></param>
        /// <param name="list"></param>
        public void GetCorpStaffShuili(bool crawlAll, IList list)
        {
            GetListShui("http://61.144.226.2/ryxx/browse_sljl.aspx", crawlAll, list);
        }

        /// <summary>
        /// 小型工程项目负责人  
        /// </summary>
        /// <param name="crawlAll"></param>
        /// <param name="list"></param>
        public void GetCorpStaffXiao(bool crawlAll, IList list)
        {
            GetListXiao("http://61.144.226.2/ryxx/browse_New.aspx", crawlAll, list);
        }

        /// <summary>
        /// 劳务队长  http://61.144.226.2/ryxx/browse_lwdz.aspx
        /// </summary>
        /// <param name="crawlAll"></param>
        /// <param name="list"></param>
        public void GetCorpStaffDui(bool crawlAll, IList list)
        {
            GetListDui("http://61.144.226.2/ryxx/browse_lwdz.aspx", crawlAll, list);
        }

        /// <summary>
        /// 采集建造工程师（xzcf）http://61.144.226.2/ryxx/browse.aspx?category=2
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public void GetCorpStaffJzao(int path, bool crawlAll, IList list)
        {
            GetList(path, crawlAll, list);
        }
        /// <summary>
        /// 采集监理工程师（sgxk）http://61.144.226.2/ryxx/browse.aspx?category=3
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public void GetCorpStaffJLi(int path, bool crawlAll, IList list)
        {
            GetList(path, crawlAll, list);
        }
        /// <summary>
        /// 采集造价工程师（zdxm）http://61.144.226.2/ryxx/browse.aspx?category=4
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public void GetCorpStaffZJia(int path, bool crawlAll, IList list)
        {
            GetList(path, crawlAll, list);
        }
        /// <summary>
        /// 采集小型工程项目负责人（htba）http://61.144.226.2/ryxx/browse.aspx?category=1
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public void GetCorpStaffXXXM(int path, bool crawlAll, IList list)
        {
            GetList(path, crawlAll, list);
        }
        /// <summary>
        /// 采集质量主任（jgys）http://61.144.226.2/ryxx/browse.aspx?category=7
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public void GetCorpStaffZLZR(int path, bool crawlAll, IList list)
        {
            GetList(path, crawlAll, list);
        }
        /// <summary>
        /// 采集安全主任（jzjn）http://61.144.226.2/ryxx/browse.aspx?category=8
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public void GetCorpStaffAQZR(int path, bool crawlAll, IList list)
        {
            GetList(path, crawlAll, list);
        }
        /// <summary>
        /// 采集建筑工程师（jzgcs）http://61.144.226.2/ryxx/browse.aspx?category=5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public void GetCorpStaffJZGCS(int path, bool crawlAll, IList list)
        {
            GetList(path, crawlAll, list);
        }
        /// <summary>
        /// 采集结构工程师（jggcs）http://61.144.226.2/ryxx/browse.aspx?category=6
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public void GetCorpStaffJGGCS(int path, bool crawlAll, IList list)
        {
            GetList(path, crawlAll, list);
        }

        private IList GetListDui(string url, bool crawlAll, IList list)
        {
            string html = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(url, Encoding.Default);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return list;
            }
            //第一页
            GetCorpStaffSzjsjMethod(url, list, html, crawlAll);
            if (!crawlAll && list.Count >= this.MaxCount) return list;

            string viewState = "";
            int pageInt = 1;
            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table1")));
            if (tdNodes != null)
            {
                try
                {
                    string pageTemp = tdNodes.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ucPageNumControl_lbltotal")), true).AsString().Replace("&nbsp;", "").Trim();
                    pageInt = int.Parse(pageTemp);
                }
                catch (Exception ex) { Logger.Error(ex); }
            }
            parser.Reset();
            if (pageInt > 1)
            {
                for (int i = 2; i <= pageInt; i++)
                {
                    string cookiestr = string.Empty;
                    viewState = ToolWeb.GetAspNetViewState(html);
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE" ,
                        "ORG_NAME", "PERS_NAME", "ucPageNumControl:gotopage",
                        "ucPageNumControl:NEXTpage" },
                        new string[] { string.Empty, string.Empty, viewState, string.Empty, string.Empty, string.Empty, "下一页" });

                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.Default, ref cookiestr);
                        //处理后续页
                        GetCorpStaffSzjsjMethod(url, list, html, crawlAll);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        continue;
                    }
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }
            return null;
        }

        private IList GetListXiao(string url, bool crawlAll, IList list)
        {
            string html = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(url, Encoding.Default);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return list;
            }
            //第一页
            GetCorpStaXiao(url, list, html, crawlAll);
            if (!crawlAll && list.Count >= this.MaxCount) return list;

            string viewState = "";
            int pageInt = 1;
            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table1")));
            if (tdNodes != null)
            {
                try
                {
                    string pageTemp = tdNodes.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ucPageNumControl_lbltotal")), true).AsString().Replace("&nbsp;", "").Trim();
                    pageInt = int.Parse(pageTemp);
                }
                catch (Exception ex) { Logger.Error(ex); }
            }
            parser.Reset();
            if (pageInt > 1)
            {
                for (int i = 2; i <= pageInt; i++)
                {
                    string cookiestr = string.Empty;
                    viewState = ToolWeb.GetAspNetViewState(html);
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE" ,
                        "ORG_NAME", "PERS_NAME", "ucPageNumControl:gotopage",
                        "ucPageNumControl:NEXTpage" },
                        new string[] { string.Empty, string.Empty, viewState, string.Empty, string.Empty, string.Empty, "下一页" });

                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.Default, ref cookiestr);
                        //处理后续页
                        GetCorpStaXiao(url, list, html, crawlAll);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        continue;
                    }
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }
            return null;
        }

        private IList GetListShui(string url, bool crawlAll, IList list)
        {
            string html = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(url, Encoding.Default);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return list;
            }
            //第一页
            //GetCorpStaffSzjsjMethod(url, list, html, crawlAll);
            GetCorpStaShui(url, list, html, crawlAll);
            if (!crawlAll && list.Count >= this.MaxCount) return list;

            string viewState = "";
            int pageInt = 1;
            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table1")));
            if (tdNodes != null)
            {
                try
                {
                    string pageTemp = tdNodes.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ucPageNumControl_lbltotal")), true).AsString().Replace("&nbsp;", "").Trim();
                    pageInt = int.Parse(pageTemp);
                }
                catch (Exception ex) { Logger.Error(ex); }
            }
            parser.Reset();
            if (pageInt > 1)
            {
                for (int i = 2; i <= pageInt; i++)
                {
                    string cookiestr = string.Empty;
                    viewState = ToolWeb.GetAspNetViewState(html);
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE" ,
                        "PERSONNAME", "ucPageNumControl:gotopage", 
                        "ucPageNumControl:NEXTpage" },
                        new string[] { string.Empty, string.Empty, viewState, string.Empty, string.Empty, "下一页" });

                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.Default, ref cookiestr);
                        //处理后续页
                        GetCorpStaShui(url, list, html, crawlAll);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        continue;
                    }
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }
            return null;
        }

        private IList GetList(int path, bool crawlAll, IList list)
        {
            string html = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(GetStartUrl(path), Encoding.Default);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return list;
            }
            //第一页
            GetCorpStaffSzjsjMethod(path, list, html, crawlAll);
            if (!crawlAll && list.Count >= this.MaxCount) return list;

            string viewState = "";
            int pageInt = 1;
            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table1")));
            if (tdNodes != null)
            {
                try
                {
                    string pageTemp = tdNodes.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ucPageNumControl_lbltotal")), true).AsString().Replace("&nbsp;", "").Trim();
                    pageInt = int.Parse(pageTemp);
                }
                catch (Exception ex) { Logger.Error(ex); }
            }
            parser.Reset();
            if (pageInt > 1)
            {
                for (int i = 2; i <= pageInt; i++)
                {
                    string cookiestr = string.Empty;
                    viewState = ToolWeb.GetAspNetViewState(html);
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "CORP_NAME", "NAME", "ucPageNumControl:gotopage", "ucPageNumControl:NEXTpage" }, new string[] { string.Empty, string.Empty, viewState, string.Empty, string.Empty, string.Empty, "下一页" });

                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(GetStartUrl(path), nvc, Encoding.Default, ref cookiestr);
                        //处理后续页
                        GetCorpStaffSzjsjMethod(path, list, html, crawlAll);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        continue;
                    }
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }
            return null;
        }


        private void GetCorpStaShui(string url, IList list, string html, bool crawlAll)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList aNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "dgConstBid")));
            if (aNodes != null && aNodes.Count == 1 && aNodes[0] is TableTag)
            {
                TableTag table = (TableTag)aNodes[0];
                for (int i = 1; i < table.Rows.Length; i++)
                {
                    Type typs = typeof(ATag);
                    string Name = string.Empty, Sex = string.Empty, CredType = string.Empty, IdNum = string.Empty, CorpName = string.Empty, CorpCode = string.Empty, CertCode = string.Empty, CertGrade = string.Empty, RegLevel = string.Empty, RegCode = string.Empty, AuthorUnit = string.Empty, PersonType = string.Empty, Province = string.Empty, City = string.Empty, CreateTime = string.Empty, InfoSource = string.Empty, Url = string.Empty, Profession = string.Empty;
                    Name = table.Rows[i].Columns[1].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    //Sex = table.Rows[i].Columns[1].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    string urlSpilt = (table.Rows[i].Columns[1].Children.SearchFor(typs, true)[0] as ATag).Link;
                    string idnum = urlSpilt.Replace("GoDetail('", "").Replace("');", "");//urlSpilt.Substring(urlSpilt.IndexOf("('") + 2, (urlSpilt.Length - urlSpilt.IndexOf("&amp;") - 2));
                    IdNum = idnum.Replace("&am", "").Replace("&a", "").Replace("p;c", "").Replace("cate", "").Replace("cat", "").Replace("ate", "");//
                    CorpName = "";//table.Rows[i].Columns[2].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    CertCode = CorpName;//table.Rows[i].Columns[3].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    CertGrade = table.Rows[i].Columns[3].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    PersonType = table.Rows[i].Columns[2].ToPlainTextString().Trim().Replace("&nbsp;", ""); //table.Rows[i].Columns[3].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    Url = "http://61.144.226.2/ryxx/Detail_SLJL.aspx?ID_NUMBER=" + idnum;
                    Profession = table.Rows[i].Columns[4].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    string ctxhtml = string.Empty;
                    try
                    {
                        ctxhtml = ToolWeb.GetHtmlByUrl(Url, Encoding.Default);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("人员姓名：" + CorpName + "，证件号：" + IdNum + "所在单位：" + CorpName + "，" + Url + "；" + ex);
                        continue;
                    }

                    Parser parserCtx = new Parser(new Lexer(ctxhtml));
                    NodeList ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("borderColor", "#cccccc")));
                    TableTag tabTag = ctxNode[0] as TableTag;
                    string text = ctxNode.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("TD"), new HasAttributeFilter("width", "76%")), true).AsString().Replace("&nbsp;", "");
                    string strSpilt = "任职企业编号:.*?\r\n";
                    MatchCollection mc = Regex.Matches(text, strSpilt);
                    foreach (Match m in mc)
                    {
                        CorpCode = m.ToString().Replace("任职企业编号:", "").Replace("\r\n", "");
                    }
                    CorpStaff corpStaff = ToolDb.GenCorpStaff(Name, Sex, CredType, string.Empty, CorpName, CorpCode, CertCode, RegLevel, RegCode, AuthorUnit, PersonType, CertGrade, "广东省", "深圳市区", "深圳市住房和建设局", Url, Profession, "", "","","");
                    // list.Add(corpStaff);
                    ToolDb.SaveEntity(corpStaff, this.ExistCompareFields);

                    // if (!crawlAll && list.Count >= this.MaxCount) return; 
                }
                parser.Reset();
            }
        }

        private void GetCorpStaXiao(string url, IList list, string html, bool crawlAll)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList aNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "dgConstBid")));
            if (aNodes != null && aNodes.Count == 1 && aNodes[0] is TableTag)
            {
                TableTag table = (TableTag)aNodes[0];
                for (int i = 1; i < table.Rows.Length; i++)
                {
                    Type typs = typeof(ATag);
                    string Name = string.Empty, Sex = string.Empty, CredType = string.Empty, IdNum = string.Empty, CorpName = string.Empty, CorpCode = string.Empty, CertCode = string.Empty, CertGrade = string.Empty, RegLevel = string.Empty, RegCode = string.Empty, AuthorUnit = string.Empty, PersonType = string.Empty, Province = string.Empty, City = string.Empty, CreateTime = string.Empty, InfoSource = string.Empty, Url = string.Empty, Profession = string.Empty;
                    Name = table.Rows[i].Columns[1].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    //Sex = table.Rows[i].Columns[1].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    string urlSpilt = (table.Rows[i].Columns[1].Children.SearchFor(typs, true)[0] as ATag).Link;
                    string idnum = urlSpilt.Replace("GoDetail('", "").Replace("');", "");//urlSpilt.Substring(urlSpilt.IndexOf("('") + 2, (urlSpilt.Length - urlSpilt.IndexOf("&amp;") - 2));
                    IdNum = idnum.Replace("&am", "").Replace("&a", "").Replace("p;c", "").Replace("cate", "").Replace("cat", "").Replace("ate", "");//
                    CorpName = table.Rows[i].Columns[2].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    CorpCode = CorpName;
                    CertCode = table.Rows[i].Columns[4].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    //CertGrade = table.Rows[i].Columns[5].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    PersonType = table.Rows[i].Columns[3].ToPlainTextString().Trim().Replace("&nbsp;", ""); //table.Rows[i].Columns[3].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    Url = "http://61.144.226.2/ryxx/Detail_SLJL.aspx?ID_NUMBER=" + idnum;
                    Profession = table.Rows[i].Columns[5].ToPlainTextString().Trim().Replace("&nbsp;", "");
                    string ctxhtml = string.Empty;
                    try
                    {
                        ctxhtml = ToolWeb.GetHtmlByUrl(Url, Encoding.Default);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("人员姓名：" + CorpName + "，证件号：" + IdNum + "所在单位：" + CorpName + "，" + Url + "；" + ex);
                        continue;
                    }

                    Parser parserCtx = new Parser(new Lexer(ctxhtml));
                    NodeList ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("borderColor", "#cccccc")));
                    TableTag tabTag = ctxNode[0] as TableTag;
                    string text = ctxNode.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("TD"), new HasAttributeFilter("width", "76%")), true).AsString().Replace("&nbsp;", "");
                    string strSpilt = "任职企业编号:.*?\r\n";
                    MatchCollection mc = Regex.Matches(text, strSpilt);
                    foreach (Match m in mc)
                    {
                        CorpCode = m.ToString().Replace("任职企业编号:", "").Replace("\r\n", "");
                    }
                    CorpStaff corpStaff = ToolDb.GenCorpStaff(Name, Sex, CredType, string.Empty, CorpName, CorpCode, CertCode, RegLevel, RegCode, AuthorUnit, PersonType, CertGrade, "广东省", "深圳市区", "深圳市住房和建设局", Url, Profession, "", "","","");
                    // list.Add(corpStaff);
                    ToolDb.SaveEntity(corpStaff, this.ExistCompareFields);

                    // if (!crawlAll && list.Count >= this.MaxCount) return; 
                }
                parser.Reset();
            }
        }


        private void GetCorpStaffSzjsjMethod(string url, IList list, string html, bool crawlAll)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList aNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "dgConstBid")));
            if (aNodes != null && aNodes.Count == 1 && aNodes[0] is TableTag)
            {
                TableTag table = (TableTag)aNodes[0];
                for (int i = 1; i < table.Rows.Length; i++)
                {
                    if (table.Rows[i].Columns.Length == 6)
                    {
                        Type typs = typeof(ATag);
                        string Name = string.Empty, Sex = string.Empty, CredType = string.Empty, IdNum = string.Empty, CorpName = string.Empty, CorpCode = string.Empty, CertCode = string.Empty, CertGrade = string.Empty, RegLevel = string.Empty, RegCode = string.Empty, AuthorUnit = string.Empty, PersonType = string.Empty, Province = string.Empty, City = string.Empty, CreateTime = string.Empty, InfoSource = string.Empty, Url = string.Empty, Profession = string.Empty;
                        Name = table.Rows[i].Columns[1].ToPlainTextString().Trim().Replace("&nbsp;", "");
                        //Sex = table.Rows[i].Columns[1].ToPlainTextString().Trim().Replace("&nbsp;", "");
                        string urlSpilt = (table.Rows[i].Columns[1].Children.SearchFor(typs, true)[0] as ATag).Link;
                        string idnum = urlSpilt.Replace("GoDetail('", "").Replace("');", "");//urlSpilt.Substring(urlSpilt.IndexOf("('"), (urlSpilt.Length  - 2));
                        IdNum = idnum.Replace("&am", "").Replace("&a", "").Replace("p;c", "").Replace("cate", "").Replace("cat", "").Replace("ate", "");//
                        CorpName = table.Rows[i].Columns[2].ToPlainTextString().Trim().Replace("&nbsp;", "");
                        CorpCode = CorpName;
                        CertCode = table.Rows[i].Columns[4].ToPlainTextString().Trim().Replace("&nbsp;", "");
                        Profession = table.Rows[i].Columns[5].ToPlainTextString().Trim().Replace("&nbsp;", "");
                        PersonType = table.Rows[i].Columns[3].ToPlainTextString().Trim().Replace("&nbsp;", "");
                        Url = "http://61.144.226.2/ryxx/Detail_LWDZ.aspx?ID_NUMBER=" + idnum;
                        string ctxhtml = string.Empty;
                        try
                        {
                            ctxhtml = ToolWeb.GetHtmlByUrl(Url, Encoding.Default);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("人员姓名：" + CorpName + "，证件号：" + IdNum + "所在单位：" + CorpName + "，" + Url + "；" + ex);
                            continue;
                        }

                        Parser parserCtx = new Parser(new Lexer(ctxhtml));
                        NodeList ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("borderColor", "#cccccc")));
                        TableTag tabTag = ctxNode[0] as TableTag;
                        string text = ctxNode.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("TD"), new HasAttributeFilter("width", "76%")), true).AsString().Replace("&nbsp;", "");
                        string strSpilt = "任职企业编号:.*?\r\n";
                        MatchCollection mc = Regex.Matches(text, strSpilt);
                        foreach (Match m in mc)
                        {
                            CorpCode = m.ToString().Replace("任职企业编号:", "").Replace("\r\n", "");
                        }
                        CorpStaff corpStaff = ToolDb.GenCorpStaff(Name, Sex, CredType, string.Empty, CorpName, CorpCode, CertCode, RegLevel, RegCode, AuthorUnit, PersonType, CertGrade, "广东省", "深圳市区", "深圳市住房和建设局", Url, Profession, "", "","","");
                        // list.Add(corpStaff);
                        ToolDb.SaveEntity(corpStaff, this.ExistCompareFields);

                        // if (!crawlAll && list.Count >= this.MaxCount) return;
                    }
                }
                parser.Reset();
            }
        }

        private void GetCorpStaffSzjsjMethod(int path, IList list, string html, bool crawlAll)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList aNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "dgConstBid")));
            if (aNodes != null && aNodes.Count == 1 && aNodes[0] is TableTag)
            {
                TableTag table = (TableTag)aNodes[0];
                for (int i = 1; i < table.Rows.Length; i++)
                {
                    if (table.Rows[i].Columns.Length == 6)
                    {
                        Type typs = typeof(ATag);
                        string Name = string.Empty, Sex = string.Empty, CredType = string.Empty, IdNum = string.Empty, CorpName = string.Empty, CorpCode = string.Empty, CertCode = string.Empty, CertGrade = string.Empty, RegLevel = string.Empty, RegCode = string.Empty, AuthorUnit = string.Empty, PersonType = string.Empty, Province = string.Empty, City = string.Empty, CreateTime = string.Empty, InfoSource = string.Empty, Url = string.Empty, Profession = string.Empty;
                        Name = table.Rows[i].Columns[1].ToPlainTextString().Trim().Replace("&nbsp;", "");
                        //Sex = table.Rows[i].Columns[1].ToPlainTextString().Trim().Replace("&nbsp;", "");
                        string urlSpilt = (table.Rows[i].Columns[1].Children.SearchFor(typs, true)[0] as ATag).Link;
                        string idnum = urlSpilt.Substring(urlSpilt.IndexOf("('") + 2, (urlSpilt.Length - urlSpilt.IndexOf("&amp;") - 2));
                        IdNum = idnum.Replace("&am", "").Replace("&a", "").Replace("p;c", "").Replace("cate", "").Replace("cat", "").Replace("ate", "");//
                        CorpName = table.Rows[i].Columns[2].ToPlainTextString().Trim().Replace("&nbsp;", "");
                        CertCode = table.Rows[i].Columns[4].ToPlainTextString().Trim().Replace("&nbsp;", "");
                        CertGrade = table.Rows[i].Columns[5].ToPlainTextString().Trim().Replace("&nbsp;", "");
                        PersonType = table.Rows[i].Columns[3].ToPlainTextString().Trim().Replace("&nbsp;", "");
                        Url = "http://61.144.226.2/ryxx/Detail.aspx?ID_NUMBER=" + idnum + "&categoryid=" + path;
                        string ctxhtml = string.Empty;
                        try
                        {
                            ctxhtml = ToolWeb.GetHtmlByUrl(Url, Encoding.Default);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("人员姓名：" + CorpName + "，证件号：" + IdNum + "所在单位：" + CorpName + "，" + Url + "；" + ex);
                            continue;
                        }

                        Parser parserCtx = new Parser(new Lexer(ctxhtml));
                        NodeList ctxNode = parserCtx.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("borderColor", "#cccccc")));
                        TableTag tabTag = ctxNode[0] as TableTag;
                        string text = ctxNode.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("TD"), new HasAttributeFilter("width", "76%")), true).AsString().Replace("&nbsp;", "");
                        string strSpilt = "任职企业编号:.*?\r\n";
                        MatchCollection mc = Regex.Matches(text, strSpilt);
                        foreach (Match m in mc)
                        {
                            CorpCode = m.ToString().Replace("任职企业编号:", "").Replace("\r\n", "");
                        }
                        CorpStaff corpStaff = ToolDb.GenCorpStaff(Name, Sex, CredType, IdNum, CorpName, CorpCode, CertCode, RegLevel, RegCode, AuthorUnit, PersonType, CertGrade, "广东省", "深圳市区", "深圳市住房和建设局", Url, Profession, "","","","");
                        // list.Add(corpStaff);
                        ToolDb.SaveEntity(corpStaff, this.ExistCompareFields);

                        // if (!crawlAll && list.Count >= this.MaxCount) return;
                    }
                }
                parser.Reset();
            }
        }
    }
}
