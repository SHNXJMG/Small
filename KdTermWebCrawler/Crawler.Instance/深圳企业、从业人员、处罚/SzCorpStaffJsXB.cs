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
    public class SzCorpStaffJsXB : WebSiteCrawller
    {
        public SzCorpStaffJsXB()
            : base()
        {
            this.PlanTime = "28 2:00";//"1-28 2:00,4-28 2:00,7-28 2:00,10-28 2:00";
            this.Group = "从业人员信息";
            this.Title = "广东省深圳市从业人员信息（2014新版）";
            this.Description = "自动抓取广东省深圳市从业人员信息（2014新版）";
            this.MaxCount = 50000;
            this.ExistCompareFields = "Name,Sex,CredType,CorpName,CorpCode,CertCode,PersonType,Province,City,CertGrade,IdNum";
            this.SiteUrl = "http://www.szjs.gov.cn/ztfw/gcjs/cyryxx/zcjzgcs/";
            this.ExistsUpdate = true;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<CorpStaff>();
            int count = 1;
            Hashtable has = new Hashtable();
            has.Add("注册建造工程师", "http://61.144.226.2:8001/web/personAction.do?method=getPersonList&category=2");
            has.Add("注册建筑工程师", "http://61.144.226.2:8001/web/personAction.do?method=getPersonList&category=5");
            has.Add("注册结构工程师", "http://61.144.226.2:8001/web/personAction.do?method=getPersonList&category=6");
            has.Add("注册监理工程师", "http://61.144.226.2:8001/web/personAction.do?method=getPersonList&category=3");
            has.Add("水利监理工程师", "http://61.144.226.2:8001/web/sljlAction.do?method=getSljlList&pageSize=50");
            has.Add("注册造价工程师", "http://61.144.226.2:8001/web/personAction.do?method=getPersonList&category=4");
            has.Add("小型项目负责人", "http://61.144.226.2:8001/web/xxxmAction.do?method=getXxxmList");
            has.Add("质量主任", "http://61.144.226.2:8001/web/personAction.do?method=getPersonList&category=7");
            has.Add("安全主任", "http://61.144.226.2:8001/web/personAction.do?method=getPersonList&category=8");
            has.Add("劳务队长", "http://61.144.226.2:8001/web/lwdzAction.do?method=getLwdzList");
            foreach (string item in has.Keys)
            { 
                int sqlCount = 0;
                string htl = string.Empty;
                string cookiestr = string.Empty;
                string viewState = string.Empty;
                int pageInt = 1;
                string eventValidation = string.Empty;
                string pageHtl = string.Empty;
                try
                {
                    if (item == "小型项目负责人")
                    {
                        htl = ToolWeb.GetHtmlByUrl("http://61.144.226.2:8001/web/xxxmAction.do?pageSize=3000&page=1&backUrl=&page=136&method=getXxxmList&method=getXxxmList&personname=&personname=&orgName=&orgName=", Encoding.Default);
                    }
                    else
                    {
                        htl = ToolWeb.GetHtmlByUrl(has[item].ToString(), Encoding.Default);
                    }
                }
                catch
                {
                    continue;
                }
                Parser parser = new Parser(new Lexer(htl));
                NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("a"), new HasAttributeFilter("id", "lx")));
                if (pageNode != null && pageNode.Count > 0)
                {
                    try
                    {
                        string temp = pageNode.GetATagHref().GetRegexBegEnd("page=", "&");
                        pageInt = int.Parse(temp);
                    }
                    catch
                    { 
                    
                    }
                }
                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        try
                        {
                            if (item != "小型项目负责人")
                            {
                                htl = ToolWeb.GetHtmlByUrl(has[item] + "&page=" + i.ToString(), Encoding.Default);
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    parser = new Parser(new Lexer(htl));
                    NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "bean")));
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        TableTag table = nodeList[0] as TableTag;
                        for (int j = 1; j < table.RowCount; j++)
                        {
                            string Name = string.Empty, Sex = string.Empty, CredType = string.Empty, IdNum = string.Empty, CorpName = string.Empty, CorpCode = string.Empty, CertCode = string.Empty, CertGrade = string.Empty, RegLevel = string.Empty, RegCode = string.Empty, AuthorUnit = string.Empty, PersonType = string.Empty, Province = string.Empty, City = string.Empty, CreateTime = string.Empty, InfoSource = string.Empty, Url = string.Empty, Profession = string.Empty, staffNum = string.Empty;

                            TableRow tr = table.Rows[j];

                            if (item.Contains("注册建造工程师") || item.Contains("注册建筑工程师") || item.Contains("注册结构工程师"))
                            {
                                Name = tr.Columns[1].ToNodePlainString();
                                CorpName = tr.Columns[2].ToNodePlainString(); 
                                CertCode = tr.Columns[4].ToNodePlainString();
                                CertGrade = tr.Columns[5].ToNodePlainString();
                                
                            }

                            if (item.Contains("水利监理工程师"))
                            {
                                Name = tr.Columns[1].ToNodePlainString(); 
                                CertCode = tr.Columns[3].ToNodePlainString();
                                Profession = tr.Columns[4].ToNodePlainString();
                            }

                            if (item.Contains("注册监理工程师") || item.Contains("注册造价工程师"))
                            {
                                Name = tr.Columns[1].ToNodePlainString();
                                CorpName = tr.Columns[2].ToNodePlainString(); 
                                CertCode = tr.Columns[4].ToNodePlainString(); 
                            }

                            if (item.Contains("小型项目负责人"))
                            {
                                Name = tr.Columns[1].ToNodePlainString();
                                CorpName = tr.Columns[2].ToNodePlainString(); 
                                CertCode = tr.Columns[4].ToNodePlainString();
                                Profession = tr.Columns[5].ToNodePlainString();
                            }

                            if (item.Contains("质量主任") || item.Contains("安全主任"))
                            {
                                Name = tr.Columns[1].ToNodePlainString();
                                CorpName = tr.Columns[2].ToNodePlainString(); 
                            }
                            if (item.Contains("劳务队长"))
                            {
                                Name = tr.Columns[1].ToNodePlainString();
                                CorpName = tr.Columns[2].ToNodePlainString(); 
                                CertCode = tr.Columns[4].ToNodePlainString();
                            }



                            PersonType = item;
                            string tempUrl = "http://61.144.226.2:8001/web/" + tr.Columns[1].GetATagValue("onclick").Replace("doView", "").Replace("(", "").Replace(")", "").Replace("'", "");
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = ToolWeb.GetHtmlByUrl(tempUrl, Encoding.Default);
                            }
                            catch { }
                            string ctx = string.Empty;
                            parser = new Parser(new Lexer(htmldtl.Replace("th","td")));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "infoTableL")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                TableTag tableDtl = dtlNode[0] as TableTag;
                                for (int k = 0; k < tableDtl.RowCount; k++)
                                {
                                    for (int d = 0; d < tableDtl.Rows[k].ColumnCount; d++)
                                    {
                                        string temp = tableDtl.Rows[k].Columns[d].ToNodePlainString().Replace("：","").Replace(":","");
                                        if (d == 0)
                                            ctx += temp += "：";
                                        else
                                            ctx += temp += "\r\n";
                                    }
                                }
                            }
                            CorpCode = ctx.GetRegex("任职企业编号");
                            staffNum = CertGrade.GetLevel();
                             
                            CorpStaff corpStaff = ToolDb.GenCorpStaff(Name, Sex, CredType, IdNum, CorpName, CorpCode, CertCode, RegLevel, RegCode, AuthorUnit, PersonType, CertGrade, "广东省", "深圳市", "深圳市住房和建设局", tempUrl, Profession, staffNum, "", "", "");
                            sqlCount++;
                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return null;
                            }
                            ToolDb.SaveEntity(corpStaff, this.ExistCompareFields, this.ExistsUpdate);

                            count++;
                            if (count >= 100)
                            {
                                count = 1;
                                Thread.Sleep(480000);
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
