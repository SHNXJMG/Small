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
using System.Linq;
using System.IO;

namespace Crawler.Instance
{
    public class CorpInfoSky : WebSiteCrawller
    {
        public CorpInfoSky()
            : base(true)
        {
            this.PlanTime = "1 23:30,8 23:30,16 23:30,24 23:30";//"1-28 2:00,4-28 2:00,7-28 2:00,10-28 2:00";
            this.Group = "企业信息";
            this.Title = "全国建筑市场监管公共服务平台（四库一平台）";
            this.Description = "自动抓取全国建筑市场监管公共服务平台（四库一平台）";
            this.ExistCompareFields = "Province,City,InfoSource,CorpName";
            this.MaxCount = 50000;
            this.SiteUrl = "http://jzsc.mohurd.gov.cn/dataservice/query/comp/list";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            List<ProvInfo> citys = GetCity();
            foreach (ProvInfo info in citys)
            {
                if (info.RegionName == "广东")
                {
                    citys.Remove(info);
                    citys.Insert(0, info);
                    break;
                }
            }

            List<string> SqlQuals = this.SaveQuals();
            List<QualInfo> quals = GetQual().OrderByDescending(x => x.QualName).ToList();

            string path = Path.Combine(System.Environment.CurrentDirectory, "ProvQual.xml");
            List<ProvQual> provQual = ToolFile.Deserialize<ProvQual>(path);
            ProvQual tempQual = null;
            if (provQual != null && provQual.Count > 1)
                tempQual = provQual[0];
            else
                provQual = new List<ProvQual>();

            bool provFlat = true, qualFlot = true;
            int count = 1, totalCount = 1;
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            string pageHtl = string.Empty;

            foreach (ProvInfo city in citys)
            {
                if (city.RegionName != "广东")
                {
                    break;
                }
                if (tempQual != null && provFlat)
                {
                    if (tempQual.RegionId != city.RegionId &&
                        tempQual.RegionName != city.RegionName)
                        continue;
                    else
                        provFlat = false;
                }
                int qualIndex = 0;
                foreach (QualInfo qual in quals)
                {
                    //if (tempQual != null && qualFlot)
                    //{
                    //    if (tempQual.QualName != qual.QualName &&
                    //        tempQual.QualCode != qual.QualCode)
                    //        continue;
                    //    else
                    //        qualFlot = false;
                    //}
                    string name = qual.QualName;
                    if (name.Contains("不分"))
                        name = name.Remove(name.IndexOf("不分"));
                    else if (name.Contains("暂定级"))
                        name = name.Remove(name.IndexOf("暂定级"));
                    else if (name.Length > 2)
                        name = name.Remove(name.Length - 2, 2);
                    if (SqlQuals.Contains(name))
                    {
                        continue;
                    }
                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] {
                            "qy_type",
                            "apt_scope",
                            "apt_code",
                            "qy_name",
                            "qy_fr_name",
                            "apt_certno",
                            "qy_reg_addr",
                            "qy_region"
                        },
                                  new string[] {
                             "",
                             qual.QualName,
                             qual.QualCode,
                             "","","",
                             city.RegionName,
                             city.RegionId
                                  });
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        return null;
                    }

                    int totalPage = 0;
                    Parser parser = new Parser(new Lexer(html));
                    NodeList tempNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("class", "nodata")));
                    if (tempNode != null && tempNode.Count > 0)
                    {
                        if (tempNode[0].ToNodePlainString().Contains("暂未查询到已登记入库信息"))
                        {
                            continue;
                            qualIndex++;

                            if (qualIndex > 5)
                            {
                                Thread.Sleep(30 * 1000);
                                qualIndex = 0;
                            }
                        }
                    }
                    parser.Reset();
                    NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "clearfix")));
                    if (pageNode != null && pageNode.Count > 0)
                    {
                        try
                        {
                            string temp = pageNode.AsString().Replace(",", "，");
                            string page = temp.GetRegexBegEnd("total", "，").GetReplace("\":");
                            totalPage = int.Parse(page);
                            if (totalPage % 15 != 0 && totalPage > 15)
                                pageInt = totalPage / 15 + 1;
                            else if (totalPage % 15 == 0 && totalPage > 15)
                                pageInt = totalPage / 15;
                            else
                                pageInt = 1;
                        }
                        catch { }
                    }
                    for (int p = 1; p <= pageInt; p++)
                    {
                        if (p > 1)
                        {
                            Logger.Error(p);
                            Logger.Error(city.RegionName);
                            Logger.Error(qual.QualName);
                            nvc = ToolWeb.GetNameValueCollection(new string[] {
                            "apt_code",
                            "qy_region",
                            "qy_fr_name",
                            "$total",
                            "qy_reg_addr",
                            "$reload",
                            "qy_type",
                            "qy_name",
                            "$pg",
                            "$pgsz",
                            "apt_scope",
                            "apt_certno"
                        },
                                    new string[] {
                              qual.QualCode,
                                city.RegionId,
                                "",
                                totalPage.ToString(),
                                city.RegionName,
                                "0","","",
                                p.ToString(),
                                "15",
                                qual.QualName,
                                ""
                                    });
                            try
                            {
                                html = ToolWeb.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                            }
                            catch
                            {
                                try
                                {
                                    Thread.Sleep(60 * 1000 * 1);
                                    html = ToolWeb.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                                }
                                catch
                                {
                                    try
                                    {
                                        Thread.Sleep(60 * 1000 * 1);
                                        html = ToolWeb.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                                    }
                                    catch
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                        if (IsVailCode(html))
                        {
                            ProvQual pro = new ProvQual();
                            pro.RegionFullName = city.RegionFullName;
                            pro.QualName = qual.QualName;
                            pro.QualCode = qual.QualCode;
                            pro.RegionId = city.RegionId;
                            pro.RegionName = city.RegionName;
                            pro.PageIndex = p;
                            provQual.Add(pro);
                            ToolFile.Serialize<ProvQual>(provQual, path);
                            break;
                        }
                        parser = new Parser(new Lexer(html));
                        NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table_box responsive personal")));

                        if (listNode != null && listNode.Count > 0)
                        {
                            TableTag table = listNode[0] as TableTag;
                            for (int i = 1; i < table.RowCount - 1; i++)
                            {
                                TableRow tr = table.Rows[i];
                                if (table.Rows[i].ColumnCount <= 1)
                                    break;
                                string CorpName = string.Empty, CorpCode = string.Empty, CorpAddress = string.Empty,
                                        RegDate = string.Empty, RegFund = string.Empty, BusinessCode = string.Empty,
                                        BusinessType = string.Empty, LinkMan = string.Empty, LinkPhone = string.Empty,
                                        Fax = string.Empty, Email = string.Empty, CorpSite = string.Empty, cUrl = string.Empty,
                                        ISOQualNum = string.Empty, ISOEnvironNum = string.Empty, OffAdr = string.Empty, Cert = string.Empty, ctxKc = string.Empty, corpProv = string.Empty,
                                        corpRz = string.Empty;

                                CorpCode = tr.Columns[1].ToNodePlainString();
                                CorpName = tr.Columns[2].ToNodePlainString();
                                BusinessCode = CorpCode;
                                LinkMan = tr.Columns[3].ToNodePlainString();
                                corpProv = tr.Columns[4].ToNodePlainString();

                                ATag aTag = tr.Columns[2].GetATag();
                                if (aTag == null) continue;

                                cUrl = "http://jzsc.mohurd.gov.cn" + aTag.Link;
                                string htmldtl = string.Empty;
                                try
                                {
                                    htmldtl = ToolWeb.GetHtmlByUrl(cUrl).GetJsString();
                                }
                                catch
                                {
                                    try
                                    {
                                        Thread.Sleep(1000 * 60 * 1);
                                        htmldtl = ToolWeb.GetHtmlByUrl(cUrl).GetJsString();
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            Thread.Sleep(1000 * 60 * 1);
                                            htmldtl = ToolWeb.GetHtmlByUrl(cUrl).GetJsString();
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                    }
                                }
                                if (IsVailCode(htmldtl))
                                {
                                    count++;
                                    totalCount++;
                                    Logger.Error(p);
                                    continue;
                                }

                                parser = new Parser(new Lexer(htmldtl.ToLower().GetReplace("th", "td")));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "pro_table_box datas_table")));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag tableInfo = tableNode[0] as TableTag;
                                    for (int j = 0; j < tableInfo.RowCount; j++)
                                    {
                                        for (int c = 0; c < tableInfo.Rows[j].ColumnCount; c++)
                                        {
                                            string temp = tableInfo.Rows[j].Columns[c].ToNodePlainString();
                                            if (c % 2 == 0)
                                                ctx += temp + "：";
                                            else
                                                ctx += temp + "\r\n";
                                        }
                                    }
                                    BusinessType = ctx.GetRegex("企业登记注册类型");
                                    CorpAddress = ctx.GetRegex("企业经营地址");
                                }

                                CorpInfo info = ToolDb.GenCorpInfo(CorpName, CorpCode, CorpAddress, RegDate, RegFund, BusinessCode, BusinessType, LinkMan, LinkPhone, Fax, Email, CorpSite, "", corpProv, corpProv, "中华人民共和国住房和城乡建设部建筑市场监管司", cUrl, ISOQualNum, ISOEnvironNum, OffAdr);

                                string sql = string.Format("select Id from CorpInfo where CorpName='{0}' and Province='{1}' and City='{2}' and InfoSource='{3}'", info.CorpName, info.Province, info.City, info.InfoSource);
                                string resultId = Convert.ToString(ToolDb.ExecuteScalar(sql));
                                int delResult = 0;
                                if (!string.IsNullOrEmpty(resultId))
                                {
                                    string delCorpQual = string.Format("delete from CorpQual where CorpId='{0}'", resultId);
                                    string delCorpResult = string.Format("delete from CorpResults where CorpId='{0}'", resultId);
                                    string delCorpTecStaff = string.Format("delete from CorpTecStaff where CorpId='{0}'", resultId);
                                    string delCorpPrompt = string.Format("delete from CorpPrompt where CorpId='{0}'", resultId);
                                    string delCorpInfo = string.Format("delete from CorpInfo where Id='{0}'", resultId);
                                    ToolDb.ExecuteSql(delCorpQual);
                                    ToolDb.ExecuteSql(delCorpResult);
                                    ToolDb.ExecuteSql(delCorpTecStaff);
                                    ToolDb.ExecuteSql(delCorpPrompt);
                                    delResult = ToolDb.ExecuteSql(delCorpInfo);
                                }
                                bool isSave = false;
                                if (delResult >= 1)
                                    isSave = ToolDb.SaveEntity(info, "");
                                else
                                    isSave = ToolDb.SaveEntity(info, this.ExistCompareFields);

                                if (isSave)
                                {
                                    parser = new Parser(new Lexer(htmldtl));
                                    NodeList aNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "query_info_tab")), true), new TagNameFilter("a")));
                                    if (aNodes != null && aNodes.Count > 0)
                                    {
                                        for (int a = 0; a < aNodes.Count; a++)
                                        {
                                            ATag aInfo = aNodes[a] as ATag;
                                            string url = "http://jzsc.mohurd.gov.cn" + aInfo.GetAttribute("data-url");
                                            if (aInfo.LinkText.Contains("资质"))
                                            {
                                                AddCorpQual(info, url);
                                            }
                                            else if (aInfo.LinkText.Contains("注册人员"))
                                            {
                                                this.AddCorpTecStaff(info, url);
                                            }
                                            else if (aInfo.LinkText.Contains("工程项目"))
                                            {
                                                this.AddCorpResults(info, url);
                                            }
                                            else if (aInfo.LinkText.Contains("不良行为"))
                                            {
                                                this.AddCorpPromptGood(info, url);
                                            }
                                            else if (aInfo.LinkText.Contains("良好行为"))
                                            {
                                                this.AddCorpPrompt(info, url);
                                            }
                                            Thread.Sleep(1000 * 1);
                                        }
                                    }
                                }
                                Thread.Sleep(1000 * 2);
                                count++;
                                totalCount++;
                                if (count >= 20)
                                {
                                    count = 0;
                                    Thread.Sleep(1000 * 60 * 2);
                                }
                                if (totalCount >= 100)
                                {
                                    totalCount = 0;
                                    Thread.Sleep(1000 * 60 * 10);
                                }
                            }
                        }
                        Thread.Sleep(1000 * 60 * 3);
                    }

                    Thread.Sleep(1000 * 60 * 5);
                }
            }
            return null;

        }

        protected List<string> SaveQuals()
        {
            List<string> quals = new List<string>();
            string sql = "select QualName,count(*) from CorpQual where CorpId in (select Id from CorpInfo where Province='广东省' and InfoSource='中华人民共和国住房和城乡建设部建筑市场监管司') group by QualName";
            DataTable dt = ToolDb.GetDbData(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string name = Convert.ToString(row["QualName"]);
                    if (name.Contains("不分"))
                        name = name.Remove(name.IndexOf("不分"));
                    quals.Add(name);
                }
            }
            return quals;
        }
        protected List<ProvInfo> GetCity()
        {
            List<ProvInfo> citys = ToolFile.Deserialize<ProvInfo>(ToolFile.WebCityPath);
            if (citys == null || citys.Count < 1)
            {
                citys = new List<ProvInfo>();
                string url = "http://jzsc.mohurd.gov.cn/asite/region/index";
                string html = string.Empty;
                try
                {
                    html = ToolWeb.GetHtmlByUrl(url);
                }
                catch { }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
                object[] objs = (object[])(((Dictionary<string, object>)((Dictionary<string, object>)smsTypeJson["json"])["category"])["provinces"]);
                foreach (object obj in objs)
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)obj;
                    ProvInfo info = new ProvInfo();
                    info.RegionId = Convert.ToString(dic["region_id"]);
                    info.RegionName = Convert.ToString(dic["region_name"]);
                    info.RegionFullName = Convert.ToString(dic["region_fullname"]);
                    citys.Add(info);
                }
                citys = citys.OrderBy(x => x.RegionName).ToList();
                ToolFile.Serialize<ProvInfo>(citys, ToolFile.WebCityPath);
            }
            return citys;
        }

        protected List<QualInfo> GetQual()
        {
            List<QualInfo> quals = ToolFile.Deserialize<QualInfo>(ToolFile.WebQualPath);

            if (quals == null || quals.Count < 1)
            {
                quals = new List<QualInfo>();
                int pageInt = 1;
                int totalPage = 0;
                string url = "http://jzsc.mohurd.gov.cn/asite/qualapt/aptData?apt_type=";
                string html = string.Empty;
                try
                {
                    html = ToolWeb.GetHtmlByUrl(url);
                }
                catch { }
                Parser parser = new Parser(new Lexer(html));
                NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "clearfix")));
                if (pageNode != null && pageNode.Count > 0)
                {
                    try
                    {
                        string temp = pageNode.AsString().Replace(",", "，");
                        string page = temp.GetRegexBegEnd("total", "，").GetReplace("\":");
                        totalPage = int.Parse(page);
                        pageInt = totalPage / 10 + 1;
                    }
                    catch { }
                }

                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] {
                            "$total",
                            "$reload",
                            "$pg",
                            "$pgsz"
                        },
                          new string[] {
                                totalPage.ToString(),
                                "0",
                                i.ToString(),
                                "10"
                          });
                        try
                        {
                            html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8);
                        }
                        catch { }
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table_box")));
                    if (listNode != null && listNode.Count > 0)
                    {
                        TableTag table = listNode[0] as TableTag;
                        for (int j = 0; j < table.RowCount - 1; j++)
                        {
                            TableRow tr = table.Rows[j];
                            parser = new Parser(new Lexer(tr.ToHtml()));
                            try
                            {
                                NodeList input = parser.ExtractAllNodesThatMatch(new TagNameFilter("input"));
                                InputTag tag = input[0] as InputTag;
                                string json = tag.GetAttribute("value");
                                JavaScriptSerializer serializer = new JavaScriptSerializer();
                                Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(json);
                                QualInfo info = new QualInfo();
                                info.QualCode = Convert.ToString(smsTypeJson["apt_code"]);
                                info.QualName = Convert.ToString(smsTypeJson["apt_scope"]);
                                quals.Add(info);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(i);
                                Logger.Error(tr.ToHtml());
                            }
                        }
                    }
                    Thread.Sleep(1000 * 1);
                }
                quals = quals.OrderBy(x => x.QualCode).ToList();
                ToolFile.Serialize<QualInfo>(quals, ToolFile.WebQualPath);
            }


            return quals;
        }
        protected bool IsVailCode(string html)
        {
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("input"), new HasAttributeFilter("id", "code_input")));
            if (nodeList != null && nodeList.Count > 0)
            {
                InputTag input = nodeList[0] as InputTag;
                return input.GetAttribute("placeholder") == "请输入验证码";
            }
            return false;
        }

        protected void AddCorpQual(CorpInfo info, string infoUrl)
        {
            string htmldtl = string.Empty;
            try
            {
                htmldtl = ToolWeb.GetHtmlByUrl(infoUrl);
            }
            catch { }
            Parser parser = new Parser(new Lexer(htmldtl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
            if (nodeList != null && nodeList.Count > 0)
            {
                TableTag table = nodeList[0] as TableTag;
                for (int i = 1; i < table.RowCount; i++)
                {
                    if (table.Rows[i].ColumnCount <= 1)
                        break;
                    string CorpId = string.Empty, QualName = string.Empty, QualCode = string.Empty, QualSeq = string.Empty, qualNum = string.Empty, QualType = string.Empty, QualLevel = string.Empty, ValidDate = string.Empty, LicDate = string.Empty, LicUnit = string.Empty;

                    TableRow tr = table.Rows[i];
                    QualType = tr.Columns[1].ToNodePlainString();
                    QualCode = tr.Columns[2].ToNodePlainString();
                    string name = tr.Columns[3].ToNodePlainString();
                    LicDate = tr.Columns[4].ToPlainTextString().GetDateRegex();
                    ValidDate = tr.Columns[5].ToPlainTextString().GetDateRegex();
                    LicUnit = tr.Columns[6].ToNodePlainString();

                    if (name.Contains("不分"))
                    {
                        QualName = name.Remove(name.IndexOf("不分"));
                        QualLevel = "不分级";
                    }
                    else if(name.Contains("暂定级"))
                    {
                        QualName = name.Remove(name.IndexOf("暂定级"));
                        QualLevel = "不分级";
                    }
                    else if (!string.IsNullOrWhiteSpace(name) && name.Length > 2)
                    {
                        QualLevel = name.Substring(name.Length - 2, 2);
                        QualName = name.Remove(name.Length - 2, 2);
                    }

                    if (QualType.Contains("监理"))
                        QualName = QualName + "监理";
                    qualNum = QualLevel.GetLevel();

                    CorpQual qual = ToolDb.GenCorpQual(info.Id, QualName, QualCode, QualSeq, QualType, QualLevel, ValidDate, LicDate, LicUnit, info.Url, qualNum, info.Province, info.City);
                    ToolDb.SaveEntity(qual, string.Empty);

                }
            }
        }

        protected void AddCorpTecStaff(CorpInfo info, string infoUrl)
        {
            string htmldtl = string.Empty;
            try
            {
                htmldtl = ToolWeb.GetHtmlByUrl(infoUrl);
            }
            catch { }
            Parser parser = new Parser(new Lexer(htmldtl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
            if (nodeList != null && nodeList.Count > 0)
            {
                TableTag table = nodeList[0] as TableTag;
                for (int i = 2; i < table.RowCount; i++)
                {
                    if (table.Rows[i].ColumnCount <= 1)
                        break;
                    string StaffName = string.Empty, IdCard = string.Empty, CertLevel = string.Empty, CertNo = string.Empty, stffType = string.Empty;

                    TableRow tr = table.Rows[i]; 
                    StaffName = tr.Columns[1].ToNodePlainString();
                    IdCard = tr.Columns[2].ToNodePlainString();
                    CertLevel = tr.Columns[3].ToNodePlainString();
                    CertNo = tr.Columns[4].ToNodePlainString();
                    stffType = tr.Columns[5].ToNodePlainString();

                    CorpTecStaff staff = ToolDb.GenCorpTecStaff(info.Id, StaffName, IdCard, CertLevel, CertNo, info.Url, stffType);
                    ToolDb.SaveEntity(staff, string.Empty);
                }
            }
        }

        protected void AddCorpResults(CorpInfo info, string infoUrl)
        {
            string htmldtl = string.Empty;
            try
            {
                htmldtl = ToolWeb.GetHtmlByUrl(infoUrl);
            }
            catch { }
            Parser parser = new Parser(new Lexer(htmldtl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
            if (nodeList != null && nodeList.Count > 0)
            {
                TableTag table = nodeList[0] as TableTag;
                for (int i = 1; i < table.RowCount; i++)
                {
                    if (table.Rows[i].ColumnCount <= 1)
                        break;
                    string PrjName = string.Empty, PrjCode = string.Empty, BuildUnit = string.Empty, GrantDate = string.Empty, PrjAddress = string.Empty, ChargeDept = string.Empty, PrjClassLevel = string.Empty, PrjClass = string.Empty, BuildArea = string.Empty, InviteArea = string.Empty, ProspUnit = string.Empty, DesignUnit = string.Empty, SuperUnit = string.Empty, ConstUnit = string.Empty, PrjStartDate = string.Empty, PrjEndDate = string.Empty;

                    TableRow tr = table.Rows[i];
                    PrjCode = tr.Columns[1].ToNodePlainString();
                    PrjName = tr.Columns[2].ToNodePlainString();
                    PrjAddress = tr.Columns[3].ToNodePlainString();
                    PrjClass = tr.Columns[4].ToNodePlainString();
                    BuildUnit = tr.Columns[5].ToNodePlainString();

                    CorpResults result = ToolDb.GenCorpResults(info.Id, PrjName, PrjCode, BuildUnit, GrantDate, PrjAddress, ChargeDept, PrjClassLevel, PrjClass, BuildArea, InviteArea, ProspUnit, DesignUnit, SuperUnit, ConstUnit, PrjStartDate, PrjEndDate, info.Url);

                    ToolDb.SaveEntity(result, string.Empty);
                }
            }
        }

        protected void AddCorpPromptGood(CorpInfo info, string infoUrl)
        {
            string htmldtl = string.Empty;
            try
            {
                htmldtl = ToolWeb.GetHtmlByUrl(infoUrl);
            }
            catch { }
            Parser parser = new Parser(new Lexer(htmldtl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
            if (nodeList != null && nodeList.Count > 0)
            {
                TableTag table = nodeList[0] as TableTag;
                for (int i = 1; i < table.RowCount; i++)
                {
                    if (table.Rows[i].ColumnCount <= 1)
                        break;
                    string prov = string.Empty, city = string.Empty, area = string.Empty, corpId = string.Empty, RecordCode = string.Empty, RecordName = string.Empty, RecordInfo = string.Empty, ImplUnit = string.Empty, BeginDate = string.Empty, InfoUrl = string.Empty;
                    bool IsGood = true;

                    TableRow tr = table.Rows[i];
                    RecordCode = tr.Columns[0].ToNodePlainString();
                    RecordName = tr.Columns[1].ToNodePlainString();
                    RecordInfo = tr.Columns[2].ToNodePlainString();
                    ImplUnit = tr.Columns[3].ToNodePlainString();
                    BeginDate = tr.Columns[4].ToPlainTextString().GetDateRegex();

                    CorpPrompt corp = ToolDb.GetCorpPrompt(info.Province, info.City, "", info.Id, RecordCode, RecordName, RecordInfo, ImplUnit, BeginDate, IsGood, infoUrl);

                    ToolDb.SaveEntity(corp, string.Empty);
                }
            }
        }

        protected void AddCorpPrompt(CorpInfo info, string infoUrl)
        {
            string htmldtl = string.Empty;
            try
            {
                htmldtl = ToolWeb.GetHtmlByUrl(infoUrl);
            }
            catch { }
            Parser parser = new Parser(new Lexer(htmldtl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
            if (nodeList != null && nodeList.Count > 0)
            {
                TableTag table = nodeList[0] as TableTag;
                for (int i = 1; i < table.RowCount; i++)
                {
                    if (table.Rows[i].ColumnCount <= 1)
                        break;
                    string prov = string.Empty, city = string.Empty, area = string.Empty, corpId = string.Empty, RecordCode = string.Empty, RecordName = string.Empty, RecordInfo = string.Empty, ImplUnit = string.Empty, BeginDate = string.Empty, InfoUrl = string.Empty;
                    bool IsGood = false;

                    TableRow tr = table.Rows[i];
                    RecordCode = tr.Columns[0].ToNodePlainString();
                    RecordName = tr.Columns[1].ToNodePlainString();
                    RecordInfo = tr.Columns[2].ToNodePlainString();
                    ImplUnit = tr.Columns[3].ToNodePlainString();
                    BeginDate = tr.Columns[4].ToPlainTextString().GetDateRegex();

                    CorpPrompt corp = ToolDb.GetCorpPrompt(info.Province, info.City, "", info.Id, RecordCode, RecordName, RecordInfo, ImplUnit, BeginDate, IsGood, infoUrl);

                    ToolDb.SaveEntity(corp, string.Empty);
                }
            }
        }
    }
}
