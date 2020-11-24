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
    public class CorpInfoGzJsgc : WebSiteCrawller
    {
        public CorpInfoGzJsgc()
            : base()
        {
            this.PlanTime = "1 23:40,8 23:40,16 23:40,24 23:40";
            this.Group = "企业信息";
            this.Title = "广州公共资源交易网企业信息";
            this.Description = "自动抓取广州公共资源交易网企业信息";
            this.ExistCompareFields = "Url";
            this.ExistsUpdate = true;
            this.MaxCount = 50000;
            this.SiteUrl = "http://202.104.65.182:8081/G2/gfmweb/web-enterprise!list.do";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string newUrl = "http://202.104.65.182:8081/G2/gfmweb/web-enterprise!list.do?data&filter_params_=enterpriseId,rowNum,enterpriseBaseId,enterpriseName,organizationCode&defined_operations_=&nocheck_operations_=&";

            string gridSearch = "true";
            string nd = ToolHtml.GetDateTimeLong(DateTime.Now).ToString();
            string PAGESIZE = "100";
            string PAGE = "1";
            string sortField = "";
            string sortDirection = "asc";
            string searchVal = "1";
            string _enterpriseName_like = "公司";
            string entTypeCodes = "";

            NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] {
            "gridSearch","nd","PAGESIZE","PAGE","sortField","sortDirection","searchVal","_enterpriseName_like","entTypeCodes"
            }, new string[] { 
            gridSearch,nd,PAGESIZE,PAGE,sortField,sortDirection,searchVal,_enterpriseName_like,entTypeCodes
            });

            string html = string.Empty;
            int pageInt = 1;

            try
            {
                html = ToolWeb.GetHtmlByUrl(newUrl, nvc, Encoding.UTF8);
            }
            catch { return null; }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);

            string page = smsTypeJson["total"].ToString();
            pageInt = int.Parse(page);

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    PAGE = i.ToString();
                    nvc = ToolWeb.GetNameValueCollection(new string[] {
                     "gridSearch","nd","PAGESIZE","PAGE","sortField","sortDirection","searchVal","_enterpriseName_like","entTypeCodes"
                    }, new string[] {
                     gridSearch,nd,PAGESIZE,PAGE,sortField,sortDirection,searchVal,_enterpriseName_like,entTypeCodes
                    });
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(newUrl, nvc, Encoding.UTF8);
                        smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
                    }
                    catch { continue; }
                    
                }

                object[] objList = (object[])smsTypeJson["data"];

                foreach (object obj in objList)
                {
                    Dictionary<string, object> dic = obj as Dictionary<string, object>;

                    string CorpName = string.Empty, CorpCode = string.Empty, CorpAddress = string.Empty,
                                                     RegDate = string.Empty, RegFund = string.Empty, BusinessCode = string.Empty,
                                                     BusinessType = string.Empty, LinkMan = string.Empty, LinkPhone = string.Empty,
                                                     Fax = string.Empty, Email = string.Empty, CorpSite = string.Empty, cUrl = string.Empty, ISOQualNum = string.Empty, ISOEnvironNum = string.Empty, corpType = string.Empty,
                                                     qualCode = string.Empty, corpMgr = string.Empty, businessMgr = string.Empty, tecMgr = string.Empty;

                    CorpName = Convert.ToString(dic["enterpriseName"]);

                    CorpCode = Convert.ToString(dic["organizationCode"]);
                    string idCode = Convert.ToString(dic["enterpriseBaseId"]);
                    string enterpriseId = Convert.ToString(dic["enterpriseId"]);
                    cUrl = "http://202.104.65.182:8081/G2/webdrive/web-enterprise!view.do?enterpriseId=" + enterpriseId;

                    //string infoUrl = "http://202.104.65.182:8081/G2/webdrive/web-enterprise-pub!getEnterpriseInfoById.do";
                    //string infoUrl2 = "http://202.104.65.182:8081/G2/webdrive/web-enterprise-pub!menuTree.do";
                    //Dictionary<string, object> dtlInfo = null, dtlInfo2 = null;
                    //string infoJson = string.Empty, infoJson2 = string.Empty;
                    string htmldtl = string.Empty;
                    try
                    {
                        htmldtl = ToolWeb.GetHtmlByUrl(cUrl).GetJsString();
                        //NameValueCollection dtlNvc = ToolWeb.GetNameValueCollection(new string[] { 
                        //"enterpriseId","menutype"
                        //}, new string[] { enterpriseId, "" });

                        //infoJson = ToolWeb.GetHtmlByUrl(infoUrl, dtlNvc, Encoding.UTF8);
                        //dtlInfo = (Dictionary<string, object>)serializer.DeserializeObject(infoJson);

                        //dtlNvc = ToolWeb.GetNameValueCollection(new string[] { 
                        //"enterpriseId",
                        //"menutype",
                        //"actionFlag"
                        //}, new string[] { 
                        //enterpriseId,"",""
                        //});

                        //infoJson2 = ToolWeb.GetHtmlByUrl(infoUrl2, dtlNvc, Encoding.UTF8);
                        //dtlInfo2 = (Dictionary<string, object>)serializer.DeserializeObject(infoJson2);
                    }
                    catch { continue; }

                    CorpAddress = ToolHtml.GetHtmlInputValue(htmldtl, "_M.registerAddress");
                    RegDate = ToolHtml.GetHtmlInputValue(htmldtl, "_M.registerTime");
                    RegFund = ToolHtml.GetHtmlInputValue(htmldtl, "_M.licenseCapital");
                    if (!string.IsNullOrEmpty(RegFund))
                        RegFund += "万元";
                    BusinessCode = ToolHtml.GetHtmlInputValue(htmldtl, "_M.licenseRegistrationCode");
                    CorpSite = ToolHtml.GetHtmlInputValue(htmldtl, "_M.firmWebsite");

                    LinkMan = ToolHtml.GetHtmlInputValue(htmldtl, "_M.name");
                    Email = ToolHtml.GetHtmlInputValue(htmldtl, "_M.email");
                    LinkPhone = ToolHtml.GetHtmlInputValue(htmldtl, "_M.tel");
                    Fax = ToolHtml.GetHtmlInputValue(htmldtl, "_M.fax");
                    corpMgr = ToolHtml.GetHtmlInputValue(htmldtl, "_M.legalPersonName");

                    Parser parser = new Parser(new Lexer(htmldtl));
                    NodeList typeNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "g2-cell col-sm-6")));
                    if (typeNode != null && typeNode.Count > 0)
                    {
                        string str = string.Empty;
                        for (int j = 2; j < typeNode.Count; j++)
                        {
                            string semp = typeNode[j].ToNodePlainString();
                            if (!string.IsNullOrEmpty(semp))
                            {
                                try
                                {
                                    DateTime time = DateTime.Parse(semp);
                                    continue;
                                }
                                catch { }
                                str += semp + ",";
                            }
                        }
                        if (!string.IsNullOrEmpty(str))
                            corpType = str.Remove(str.Length - 1);
                    }


                    CorpInfo info = ToolDb.GenCorpInfo(CorpName, CorpCode, CorpAddress, RegDate, RegFund, BusinessCode, BusinessType, LinkMan, LinkPhone, Fax, Email, CorpSite, corpType, "广东省", "广东地区", "广东省住房和城乡建设厅", cUrl, ISOQualNum, ISOEnvironNum, string.Empty);

                    string exisSql = string.Format("select Id from CorpInfo where CorpName='{0}' and CorpType='{1}' and InfoSource='{2}'", info.CorpName, info.CorpType, info.InfoSource);

                    string corpId = Convert.ToString(ToolDb.ExecuteScalar(exisSql));

                    if (!string.IsNullOrEmpty(corpId))
                    {
                        string delCorpQual = string.Format("delete from CorpQual where CorpId='{0}'", corpId);
                        string delCorpLeader = string.Format("delete from CorpLeader where CorpId='{0}'", corpId);
                        string delCorpSecLicStaff = string.Format("delete from CorpSecLicStaff where CorpId='{0}'", corpId);
                        int qualCount = 0, leaderCount = 0, tecstaffCount = 0, infoCount = 0;
                        string corpSql = string.Format("delete from CorpInfo where Id='{0}'", corpId);
                        infoCount = ToolDb.ExecuteSql(corpSql);
                        qualCount = ToolDb.ExecuteSql(delCorpQual);
                        leaderCount = ToolDb.ExecuteSql(delCorpLeader);
                        tecstaffCount = ToolDb.ExecuteSql(delCorpSecLicStaff);

                        if (infoCount > 0)
                            ToolDb.SaveEntity(info, "");
                        if (qualCount >= 0)
                            try
                            {
                                AddCorpQual(info, enterpriseId);
                            }
                            catch (Exception ex) { Logger.Error(ex); }
                        if (leaderCount >= 0)
                            try
                            {
                                AddCorpLeader(info, enterpriseId);
                            }
                            catch (Exception ex) { Logger.Error(ex); }
                        if (tecstaffCount >= 0)
                            try
                            {
                                AddCorpStaff(info, enterpriseId);
                            }
                            catch (Exception ex) { Logger.Error(ex); }
                    }
                    else
                    {
                        if (ToolDb.SaveEntity(info, ""))
                        {
                            try
                            {
                                AddCorpLeader(info, enterpriseId);
                            }
                            catch (Exception ex) { Logger.Error(ex); }
                            try
                            {
                                AddCorpQual(info, enterpriseId);
                            }
                            catch (Exception ex) { Logger.Error(ex); }
                            try
                            {
                                AddCorpStaff(info, enterpriseId);
                            }
                            catch (Exception ex) { Logger.Error(ex); }
                        }
                    }

                }

            }

            ToolCoreDb.ExecuteProcedure();
            return null;
        }

        protected void AddCorpQual(CorpInfo info, string enterpriseId)
        {
            string gridSearch = "false";
            string nd = ToolHtml.GetDateTimeLong(DateTime.Now).ToString();
            string PAGESIZE = "100";
            string PAGE = "1";
            string sortField = "";
            string sortDirection = "asc";
            string url = "http://202.104.65.182:8081/G2/webdrive/web-enterprise-qualification.do?enterpriseId=" + enterpriseId + "&data&filter_params_=rowNum,qualificationId,enterpriseBaseId,enterpriseId,qualificationBeforeId,qualificationCode,validEnd&defined_operations_=&nocheck_operations_=&";

            NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] {
            "gridSearch","nd","PAGESIZE","PAGE","sortField","sortDirection"
            }, new string[] {
            gridSearch,nd,PAGESIZE,PAGE,sortField,sortDirection
            });
            string strJson = string.Empty;
            try
            {
                strJson = ToolWeb.GetHtmlByUrl(url, nvc);
            }
            catch { return; }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(strJson);

            object[] objList = smsTypeJson["data"] as object[];
            if (objList != null)
            {
                foreach (object obj in objList)
                {
                    Dictionary<string, object> dic = obj as Dictionary<string, object>;
                    string enterpriseBaseId = Convert.ToString(dic["enterpriseBaseId"]);
                    string validEnd = Convert.ToString(dic["validEnd"]);
                    string qualificationCode = Convert.ToString(dic["qualificationCode"]);
                    string enterpriseIds = Convert.ToString(dic["enterpriseId"]);
                    string qualificationBeforeId = Convert.ToString(dic["qualificationBeforeId"]);
                    string qualificationId = Convert.ToString(dic["qualificationId"]);


                    string dtlUrl = "http://202.104.65.182:8081/G2/webdrive/none/web-enterprise-qualification-item.do?qualificationId=" + qualificationId + "&actionFlag=&data&filter_params_=rowNum,qualificationItemId,qualificationId,contentCodeValue,qualificationCode,contentCodeName,qualificationLevelName,mainItem,certificateIssuer,certificateDate,validDate&defined_operations_=&nocheck_operations_=&";

                    string dtlJson = string.Empty;
                    try
                    {
                        dtlJson = ToolWeb.GetHtmlByUrl(dtlUrl, nvc);
                    }
                    catch { continue; }

                    Dictionary<string, object> dtlDicJson = (Dictionary<string, object>)serializer.DeserializeObject(dtlJson);

                    object[] objDtlList = dtlDicJson["data"] as object[];
                    if (objDtlList != null)
                    {
                        foreach (object objDtl in objDtlList)
                        {
                            Dictionary<string, object> dicDtl = objDtl as Dictionary<string, object>;
                            string CorpId = string.Empty, QualName = string.Empty, QualCode = string.Empty, QualSeq = string.Empty, qualNum = string.Empty, QualType = string.Empty, QualLevel = string.Empty, ValidDate = string.Empty, LicDate = string.Empty, LicUnit = string.Empty;

                            CorpId = info.Id;
                            QualCode = qualificationCode;
                            QualName = QualType = Convert.ToString(dicDtl["contentCodeName"]);
                            QualLevel = Convert.ToString(dicDtl["qualificationLevelName"]);
                            LicUnit = Convert.ToString(dicDtl["certificateIssuer"]);
                            LicDate = Convert.ToString(dicDtl["certificateDate"]);
                            ValidDate = Convert.ToString(dicDtl["validDate"]);
                            qualNum = QualLevel.GetLevel();
                            Dictionary<string, object> mainItem = dicDtl["mainItem"] as Dictionary<string, object>;
                            if (mainItem != null)
                            {
                                QualSeq = Convert.ToString(mainItem["desc"]);
                            }

                            CorpQual qual = ToolDb.GenCorpQual(CorpId, QualName, QualCode, QualSeq, QualType, QualLevel, ValidDate, LicDate, LicUnit, info.Url, qualNum, "广东省", "广东地区");
                            ToolDb.SaveEntity(qual, string.Empty);


                        }
                    }
                }
            }

        }

        protected void AddCorpLeader(CorpInfo info, string enterpriseId)
        {
            string url = "http://202.104.65.182:8081/G2/webdrive/web-enterprise-leader.do?enterpriseId=" + enterpriseId + "&data&filter_params_=rowNum,leaderId,name,title,safetyLicenseCode,safetyLicenseIssuer,safetyLicenseValidEnd&defined_operations_=&nocheck_operations_=&";

            string gridSearch = "false";
            string nd = ToolHtml.GetDateTimeLong(DateTime.Now).ToString();
            string PAGESIZE = "1000";
            string PAGE = "1";
            string sortField = "";
            string sortDirection = "asc";
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] {
            "gridSearch","nd","PAGESIZE","PAGE","sortField","sortDirection"
            }, new string[] {
            gridSearch,nd,PAGESIZE,PAGE,sortField,sortDirection
            });
            string strJson = string.Empty;
            try
            {
                strJson = ToolWeb.GetHtmlByUrl(url, nvc);
            }
            catch { return; }


            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(strJson);

            object[] objList = smsTypeJson["data"] as object[];
            if (objList != null)
            {
                foreach (object obj in objList)
                {
                    Dictionary<string, object> dic = obj as Dictionary<string, object>;
                    string LeaderName = string.Empty, LeaderDuty = string.Empty, LeaderType = string.Empty, htlCtx = string.Empty, phone = string.Empty;

                    LeaderName = Convert.ToString(dic["name"]);
                    LeaderType = Convert.ToString(dic["title"]);

                    CorpLeader corpLeader = ToolDb.GenCorpLeader(info.Id, LeaderName, LeaderDuty, LeaderType, info.Url, phone);
                    ToolDb.SaveEntity(corpLeader, string.Empty);
                }
            }

        }

        protected void AddCorpStaff(CorpInfo info, string enterpriseId)
        {
            string url = "http://202.104.65.182:8081/G2/webdrive/web-person-info.do?enterpriseId=" + enterpriseId + "&enterpriseBaseId=&data&filter_params_=rowNum,personId,personBaseId,name,isPause,isDel&defined_operations_=&nocheck_operations_=&";

            string gridSearch = "false";
            string nd = ToolHtml.GetDateTimeLong(DateTime.Now).ToString();
            string PAGESIZE = "1000";
            string PAGE = "1";
            string sortField = "";
            string sortDirection = "asc";
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] {
            "gridSearch","nd","PAGESIZE","PAGE","sortField","sortDirection"
            }, new string[] {
            gridSearch,nd,PAGESIZE,PAGE,sortField,sortDirection
            });
            string strJson = string.Empty;
            try
            {
                strJson = ToolWeb.GetHtmlByUrl(url, nvc);
            }
            catch { return; }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(strJson);

            object[] objList = smsTypeJson["data"] as object[];
            if (objList != null)
            {
                foreach (object obj in objList)
                {
                    string StaffName = string.Empty, IdCard = string.Empty, CertLevel = string.Empty, CertNo = string.Empty, stffType = string.Empty;

                    Dictionary<string, object> dic = obj as Dictionary<string, object>;
                    StaffName = Convert.ToString(dic["name"]);

                    string dtlUrl = "http://202.104.65.182:8081/G2/webdrive/web-person-certificate.do?personId=" + dic["personId"] + "&actionFlag=view&data&filter_params_=rowNum,personBaseId,personId,certificateId,certificateType,registerLevel,certificateCode,certificatePhotoetch,gardenMajor,issuer,major,pmTitle,issueDate,registerValidEnd&defined_operations_=&nocheck_operations_=view&";//"http://202.104.65.182:8081/G2/webdrive/web-person-certificate.do?personId=" + enterpriseId + "&actionFlag=view&data&filter_params_=rowNum,personBaseId,personId,certificateId,certificateType,registerLevel,certificateCode,certificatePhotoetch,gardenMajor,issuer,major,pmTitle,issueDate,registerValidEnd&defined_operations_=&nocheck_operations_=view&";

                    string dtlJson = string.Empty;
                    try
                    {
                        dtlJson = ToolWeb.GetHtmlByUrl(dtlUrl, nvc);
                    }
                    catch { continue; }
                    Dictionary<string, object> dtlDic = (Dictionary<string, object>)serializer.DeserializeObject(dtlJson);
                    object[] dtlObjList = dtlDic["data"] as object[];
                    if (dtlObjList != null && dtlObjList.Length > 0)
                    {
                        Dictionary<string, object> dicDtl = dtlObjList[0] as Dictionary<string, object>;
                        CertNo = Convert.ToString(dicDtl["certificateCode"]);
                        stffType = Convert.ToString(dicDtl["major"]);
                    }
                    CorpTecStaff staff = ToolDb.GenCorpTecStaff(info.Id, StaffName, IdCard, CertLevel, CertNo, info.Url, stffType);
                    ToolDb.SaveEntity(staff, string.Empty);
                }
            }

        }
    }
}
