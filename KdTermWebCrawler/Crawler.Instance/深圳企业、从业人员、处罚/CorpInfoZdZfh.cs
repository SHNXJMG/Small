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
    public class CorpInfoZdZfh : WebSiteCrawller
    {
        public CorpInfoZdZfh()
            : base()
        {
            this.PlanTime = "1 23:40,8 23:40,16 23:40,24 23:40";
            this.Group = "企业信息";
            this.Title = "广东省三库一平台企业信息";
            this.Description = "自动抓取广东省三库一平台企业信息";
            this.ExistCompareFields = "Url";
            this.ExistsUpdate = true;
            this.MaxCount = 50000;
            this.SiteUrl = "http://113.108.219.40/Dop/Open/EntCreditList.aspx";
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
                html = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch
            {
                return null;
            } 

            string opValue = string.Empty, leveVlaue = string.Empty;
            string[] levelNode = new string[] { 
            "特级","特级(旧标准)","一级","一级(旧标准)","二级","二级(旧标准)","三级","三级(旧标准)","暂定三级(旧标准)","不分等级"
            };
            Parser parser = new Parser(new Lexer(html));
            NodeList typeNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_ddlENT_SORT_ID")), true), new TagNameFilter("option")));

            if (typeNode != null && typeNode.Count > 0)
            {
                for (int t = 1; t < typeNode.Count; t++)
                {
                    for (int l = 1; l < levelNode.Length; l++)
                    {
                        leveVlaue = levelNode[l];

                        OptionTag opTag = typeNode[t] as OptionTag;
                        opValue = opTag.GetAttribute("value");
                        parser = new Parser(new Lexer(html));
                        NodeList inputNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("input"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_ValidateCode1_txtRanNum")));
                        string valiCode = string.Empty;
                        if (inputNode != null && inputNode.Count > 0)
                            valiCode = (inputNode[0] as InputTag).GetAttribute("value");
                        viewState = ToolWeb.GetAspNetViewState(html);
                        NameValueCollection typeNvc = ToolWeb.GetNameValueCollection(
                            new string[] { 
                            "ctl00_ContentPlaceHolder1_toolkitScriptManager1_HiddenField", 
                            "__EVENTTARGET",
                            "__EVENTARGUMENT", 
                            "__LASTFOCUS",
                        "__VIEWSTATE",
                        "ctl00$ContentPlaceHolder1$ddlENT_SORT_ID",
                        "ctl00$ContentPlaceHolder1$ddlRank",
                        "ctl00$ContentPlaceHolder1$txtEnt_name",
                            "ctl00$ContentPlaceHolder1$ValidateCode1$txtValidateCode",
                            "ctl00$ContentPlaceHolder1$ValidateCode1$txtRanNum",
                            "ctl00$ContentPlaceHolder1$btnsearch"},
                            new string[]{
                                "","","","",
                                viewState,opValue,leveVlaue,"",valiCode,valiCode,
                      "搜  索"
                        });
                        try
                        {
                            html = ToolWeb.GetHtmlByUrl(SiteUrl, typeNvc, Encoding.UTF8, ref cookiestr);
                        }
                        catch { continue; }

                        parser = new Parser(new Lexer(html));
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
                            if (i > 20)
                                break;
                            if (i > 1)
                            {
                                try
                                {
                                    parser = new Parser(new Lexer(html));
                                    NodeList pageInputNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("input"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_ValidateCode1_txtRanNum")));
                                    string pageValiCode = string.Empty;
                                    if (pageInputNode != null && pageInputNode.Count > 0) pageValiCode = (pageInputNode[0] as InputTag).GetAttribute("value");
                                    viewState = ToolWeb.GetAspNetViewState(html);
                                    NameValueCollection nvc = ToolWeb.GetNameValueCollection(
                                        new string[]{
                           "ctl00$ContentPlaceHolder1$ddlENT_SORT_ID",
                           "ctl00$ContentPlaceHolder1$ddlRank",
                           "ctl00$ContentPlaceHolder1$txtEnt_name",
                           "ctl00$ContentPlaceHolder1$ValidateCode1$txtRanNum",
                           "ctl00$ContentPlaceHolder1$ValidateCode1$txtValidateCode",
                           "ctl00_ContentPlaceHolder1_toolkitScriptManager1_HiddenField",
                           "__EVENTARGUMENT",
                           "__EVENTTARGET",
                           "__LASTFOCUS",
                           "__VIEWSTATE"
                            },
                                        new string[]{
                           opValue,
                           leveVlaue,"",
                           pageValiCode,
                           "","",
                          i.ToString(),
                           "ctl00$ContentPlaceHolder1$AspNetPager1","",
                           viewState
                            }
                                        );
                                    html = ToolWeb.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                                }
                                catch { continue; }
                            }
                            parser = new Parser(new Lexer(html));
                            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "tab_ent")));
                            if (nodeList != null && nodeList.Count > 0)
                            {
                                #region 循环列表
                                TableTag table = nodeList[0] as TableTag;
                                for (int j = 1; j < table.RowCount; j++)
                                {
                                    string CorpName = string.Empty, CorpCode = string.Empty, CorpAddress = string.Empty,
                                                      RegDate = string.Empty, RegFund = string.Empty, BusinessCode = string.Empty,
                                                      BusinessType = string.Empty, LinkMan = string.Empty, LinkPhone = string.Empty,
                                                      Fax = string.Empty, Email = string.Empty, CorpSite = string.Empty, cUrl = string.Empty,
                                                      ISOQualNum = string.Empty, ISOEnvironNum = string.Empty, corpType = string.Empty,
                                                      qualCode = string.Empty, corpMgr = string.Empty, businessMgr = string.Empty, tecMgr = string.Empty;
                                    string htlCtx = string.Empty, QualType = string.Empty, CorpLevey = string.Empty;
                                    TableRow tr = table.Rows[j];
                                    string qualStr = tr.Columns[2].ToHtml();
                                    CorpName = tr.Columns[1].ToNodePlainString();
                                    QualType = tr.Columns[2].ToPlainTextString();
                                    CorpLevey = tr.Columns[3].ToNodePlainString();
                                    qualCode = tr.Columns[4].ToNodePlainString();
                                    if (QualType == "--") QualType = "";
                                    cUrl = "http://113.108.219.40/PlatForm/SearchCenter/" + tr.Columns[1].GetATagHref();

                                    List<string> quaList = new List<string>();
                                    parser = new Parser(new Lexer(tr.Columns[4].ToHtml()));
                                    NodeList quaNodeList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                    if (quaNodeList != null && quaNodeList.Count > 0)
                                    {
                                        for (int q = 0; q < quaNodeList.Count; q++)
                                        {
                                            quaList.Add("http://113.108.219.40/PlatForm/SearchCenter/" + quaNodeList[q].GetATagHref());
                                        }
                                    }
                                    string quaUrl = "http://113.108.219.40/PlatForm/SearchCenter/" + tr.Columns[4].GetATagHref();
                                    string htldtl = string.Empty;
                                    try
                                    {
                                        htldtl = ToolWeb.GetHtmlByUrl(cUrl, Encoding.UTF8);
                                    }
                                    catch { continue; }

                                    parser = new Parser(new Lexer(htldtl));
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


                                        corpType = ctx.GetRegex(new string[] { "企业类型", "类型" });
                                        CorpAddress = ctx.GetRegex(new string[] { "企业注册地址", "地址" });
                                        BusinessCode = ctx.GetRegex(new string[] { "营业执照注册号", "注册号" });
                                        RegDate = ctx.GetRegex(new string[] { "成立时间", "成立日期", "时间", "日期" }).GetDateRegex();
                                        LinkMan = ctx.GetRegex(new string[] { "企业法定代表人", "法定代表人" });
                                        RegFund = ctx.GetRegex(new string[] { "注册资金", "资金" });
                                        if (!RegFund.Contains("万"))
                                            RegFund += "万";
                                        corpMgr = ctx.GetRegex(new string[] { "企业经理" });
                                        if (corpMgr.Contains("暂无"))
                                            corpMgr = string.Empty;
                                        businessMgr = ctx.GetRegex(new string[] { "经营负责人" });
                                        if (businessMgr.Contains("暂无"))
                                            businessMgr = string.Empty;
                                        tecMgr = ctx.GetRegex(new string[] { "技术负责人" });
                                        if (tecMgr.Contains("暂无"))
                                            tecMgr = string.Empty;

                                        CorpInfo info = ToolDb.GenCorpInfo(CorpName, CorpCode, CorpAddress, RegDate, RegFund, BusinessCode, BusinessType, LinkMan, LinkPhone, Fax, Email, CorpSite, corpType, "广东省", "广东地区", "广东省住房和城乡建设厅", cUrl, ISOQualNum, ISOEnvironNum, string.Empty);

                                        string strSql = string.Format("select Id from CorpInfo where CorpName='{0}' and Url='{1}'", info.CorpName, info.Url);
                                        object obj = ToolDb.ExecuteScalar(strSql);
                                        if (obj != null && obj.ToString() != "")
                                        {
                                            StringBuilder delCorpQual = new System.Text.StringBuilder();
                                            StringBuilder delCorpLeader = new System.Text.StringBuilder();
                                            delCorpQual.AppendFormat("delete from CorpQual where CorpId='{0}'", obj);
                                            delCorpLeader.AppendFormat("delete from CorpLeader where CorpId='{0}'", obj);
                                            ToolDb.ExecuteSql(delCorpQual.ToString());
                                            ToolDb.ExecuteSql(delCorpLeader.ToString());
                                            string corpSql = string.Format("delete from CorpInfo where Id='{0}'", obj);
                                            ToolCoreDb.ExecuteSql(corpSql);
                                        }

                                        if (ToolDb.SaveEntity(info, string.Empty))
                                        {
                                            if (!string.IsNullOrEmpty(LinkMan))
                                            {
                                                CorpLeader leader = ToolDb.GenCorpLeader(info.Id, LinkMan, "", "企业法定代表人", cUrl);
                                                ToolDb.SaveEntity(leader, string.Empty);
                                            }
                                            if (!string.IsNullOrEmpty(corpMgr))
                                            {
                                                CorpLeader leader = ToolDb.GenCorpLeader(info.Id, corpMgr, "", "企业经理", cUrl);
                                                ToolDb.SaveEntity(leader, string.Empty);
                                            }
                                            if (!string.IsNullOrEmpty(businessMgr))
                                            {
                                                CorpLeader leader = ToolDb.GenCorpLeader(info.Id, businessMgr, "", "经营负责人", cUrl);
                                                ToolDb.SaveEntity(leader, string.Empty);
                                            }
                                            if (!string.IsNullOrEmpty(tecMgr))
                                            {
                                                CorpLeader leader = ToolDb.GenCorpLeader(info.Id, tecMgr, "", "技术负责人", cUrl);
                                                ToolDb.SaveEntity(leader, string.Empty);
                                            }
                                            if (!string.IsNullOrEmpty(qualStr))
                                            {
                                                List<CorpQual> corpQuals = new List<CorpQual>();
                                                string quaCtx = string.Empty;
                                                for (int c = 0; c < quaList.Count; c++)
                                                {
                                                    string quaHtl = string.Empty;
                                                    try
                                                    {
                                                        quaHtl = ToolWeb.GetHtmlByUrl(quaList[c], Encoding.UTF8);
                                                    }
                                                    catch { }

                                                    parser = new Parser(new Lexer(quaHtl));
                                                    NodeList quaNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                                    if (quaNode != null && quaNode.Count > 0)
                                                    {
                                                        TableTag quaTable = quaNode[0] as TableTag;

                                                        for (int k = 0; k < quaTable.RowCount; k++)
                                                        {
                                                            for (int d = 0; d < quaTable.Rows[k].ColumnCount; d++)
                                                            {
                                                                string temp = quaTable.Rows[k].Columns[d].ToNodePlainString();
                                                                //string quatemp = quaTable.Rows[k].ToNodePlainString();
                                                                if ((d + 1) % 2 == 0)
                                                                {
                                                                    quaCtx += temp + "\r\n";
                                                                }
                                                                else
                                                                {
                                                                    quaCtx += temp.Replace("：", "").Replace(":", "") + "：";
                                                                }
                                                            }
                                                        }
                                                    }
                                                    string qualctx = string.Empty;
                                                    parser.Reset();
                                                    NodeList spanNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "lblQuaInfo")));
                                                    if (spanNode != null && spanNode.Count > 0)
                                                    {
                                                        qualctx = spanNode.ToHtml().GetReplace("<br/>,<br />,<br>,</br>", "∈").ToCtxString();
                                                    }
                                                    string str = qualctx.ToLower().Replace("<br/>", "∈").Replace("</br>", "∈").Replace("<br>", "∈");
                                                    str = Regex.Replace(str, "<[^>]*>", "");
                                                    string[] qual = str.Split('∈');
                                                    for (int q = 0; q < qual.Length; q++)
                                                    {
                                                        if (string.IsNullOrEmpty(qual[q]) || qual[q] == "--")
                                                            continue;
                                                        string CorpId = string.Empty, QualName = string.Empty, quaCode = string.Empty, QualSeq = string.Empty, qualNum = string.Empty, QualLevel = string.Empty, ValidDate = string.Empty, LicDate = string.Empty, LicUnit = string.Empty, quaType = string.Empty;
                                                        LicDate = quaCtx.GetRegex("发证日期,发证时间").GetDateRegex();
                                                        LicUnit = quaCtx.GetRegex("发证机关,发证机构");
                                                        ValidDate = quaCtx.GetRegex("证书有效期").GetDateRegex();
                                                        quaType = quaCtx.GetRegex("证书类型");
                                                        string value = qual[q];
                                                        int len = value.IndexOf("/");
                                                        if (len != -1)
                                                        {
                                                            QualLevel = value.Substring(len, value.Length - len).Replace("/", "");
                                                            value = value.Remove(len);
                                                        }
                                                        else
                                                        {
                                                            QualLevel = CorpLevey;
                                                        }
                                                        string[] dtl = value.Split(' ');
                                                        CorpId = info.Id;
                                                        QualName = dtl[0].Trim();
                                                        if (string.IsNullOrEmpty(QualName))
                                                            QualName = dtl[dtl.Length - 1];
                                                        quaCode = quaCtx.GetRegex("证书编号");//qualCode;
                                                        for (int ty = 1; ty < dtl.Length; ty++)
                                                            quaType += dtl[ty].Trim() + ",";
                                                        if (!string.IsNullOrEmpty(quaType) && quaType.Contains(","))
                                                        {
                                                            quaType = quaType.Substring(0, quaType.Length - 1);
                                                            if (quaType[0] == ',' || quaType[0] == '，')
                                                                quaType = quaType.Substring(1, quaType.Length - 1);
                                                        }
                                                        qualNum = QualLevel.GetLevel();

                                                        CorpQual corpQual = null;
                                                        corpQual = ToolDb.GenCorpQual(info.Id, QualName, quaCode, QualSeq, quaType, QualLevel, ValidDate, LicDate, LicUnit, quaUrl, qualNum, "广东省", "广东地区");
                                                        ToolDb.SaveEntity(corpQual, string.Empty);
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                }
            }
            ToolCoreDb.ExecuteProcedure();
            return null;
        }
    }
}
