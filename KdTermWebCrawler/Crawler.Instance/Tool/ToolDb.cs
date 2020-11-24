using System;
using Crawler;
using System.Web.UI.MobileControls;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Crawler.Base.KdService;
namespace Crawler.Instance
{
    public class ToolDb : ToolCoreDb
    {
        public static string GetPrjName(string prjName)
        {
            string[] prjStr = new string[]{ 
                "中标公告","成交公告","中标结果公示","中标结果公告","评标结果公示",
            "采购结果公告","招标结果公告","中标人公示","招标公告","结果公告","结果公示","中标公示",
            "中标候选单位公示","中标候选人的公示","中标候选人公示","采购成交公示","竞价采购失败公告","资格预审公告",
            "资格预审公示","废标公告","废标公示","采购公告","延期公告","变更公告","二次公告",
            "补疑公告","采购失败公告","重新公告","成交公示","竞价采购"
            };
            for (int i = 0; i < prjStr.Length; i++)
            {
                if (prjName.EndsWith(prjStr[i]))
                {
                    prjName = prjName.Substring(0, prjName.Length - prjStr[i].Length);
                    break;
                }
            }
            return prjName;
        }

        public static string GetBidUnit(string bidName)
        {
            string[] bidStr = new string[] {
                "第一包","第二包","第三包","第四包","第五包","第六包","第七包","第八包","第九包", 
            };
            string[] bidStr2 = new string[] { 
                "第一","第二","第三","第四","第五","第六","第七","第八","第九",
                "第1","第2","第3","第4","第5","第6","第7","第8","第9"
            };
            for (int i = 0; i < bidStr.Length; i++)
            {
                if (bidName.Contains(bidStr[i]))
                {
                    bidName = bidName.Replace(bidStr[i], "");
                    break;
                }
            }
            for (int i = 0; i < bidStr2.Length; i++)
            {
                if (bidName.EndsWith(bidStr2[i]))
                {
                    bidName = bidName.Substring(0, bidName.Length - bidStr2[i].Length);
                    break;
                }
            }
            return bidName;
        }

        public static CorpInstitution GenCorpInstitution(string prov, string city, string corpid, string corpname, string corpcode, string location, string dtladdress, string postalcode, string resinstitution, string linkman, string linphone, string fax, string businesscode, string regdate, string email, string safetycode, string totalreman, string techreman, string safereman, string qualityreman, string url, string TotalSafetyCode, string TechSafetyCode, string QualitySafetyCode)
        {
            CorpInstitution info = new CorpInstitution();
            info.Id = ToolDb.NewGuid;
            info.CreateTime = DateTime.Now;
            info.Province = prov;
            info.City = city;
            info.CorpId = corpid;
            info.CorpName = corpname;
            info.CorpCode = corpcode;
            info.Location = location;
            info.DtlAddress = dtladdress;
            info.PostalCode = postalcode;
            info.ResInstitution = resinstitution;
            info.LinkMan = linkman;
            info.LinPhone = linphone;
            info.Fax = fax;
            info.BusinessCode = businesscode;
            try
            {
                info.RegDate = DateTime.Parse(regdate);
            }
            catch { }
            info.Email = email;
            info.SafetyCode = safetycode;
            info.TotalReMan = totalreman;
            info.TechReMan = techreman;
            info.SafeReMan = safereman;
            info.QualityReMan = qualityreman;
            info.Url = url;
            info.TotalSafetyCode = TotalSafetyCode;
            info.TechSafetyCode = TechSafetyCode;
            info.QualitySafetyCode = QualitySafetyCode;
            return info;
        }

        public static BidUnitInfo GenBidUnitInfo(string prov, string city, string area, string projectName, string buildUnit, string code, string corpName, string registerMode, string prjMgr, string bidMember, string phone, string signUpDate, string msgType)
        {
            BidUnitInfo info = new BidUnitInfo();
            info.Id = ToolDb.NewGuid;
            info.Prov = prov;
            info.City = city;
            info.Area = area;
            info.Creator = info.LastModifier = ToolDb.EmptyGuid;
            info.CreateTime = info.LastModifierTime = DateTime.Now;
            info.ProjectName = projectName;
            info.BuildUnit = buildUnit;
            info.Code = code;
            info.CorpName = corpName;
            info.RegisterMode = registerMode;
            info.PrjMgr = prjMgr;
            info.BidMember = bidMember;
            info.Phone = phone;
            try
            {
                info.SignUpDate = Convert.ToDateTime(signUpDate);
            }
            catch { }
            info.MsgType = msgType;
            return info;
        }

        /// <summary>
        /// 通知公告
        /// </summary>
        /// <param name="headName"></param>
        /// <param name="releaseTime"></param>
        /// <param name="infoScorce"></param>
        /// <param name="msgType"></param>
        /// <param name="createTime"></param>
        /// <param name="infoUrl"></param>
        /// <param name="ctxHtml"></param>
        /// <param name="province"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        public static NotifyInfo GenNotifyInfo(string headName, string releaseTime, string infoScorce, string msgType,
            string infoUrl, string ctxHtml, string province, string city, string area, string infoCtx, string infoType)
        {
            NotifyInfo info = new NotifyInfo();
            info.Id = ToolDb.NewGuid;
            try
            {
                info.ReleaseTime = DateTime.Parse(releaseTime);
            }
            catch { info.ReleaseTime = null; }
            info.HeadName = headName.GetPrjNameByName();
            info.InfoScorce = infoScorce;
            info.Area = area;
            info.InfoUrl = infoUrl;
            info.MsgType = msgType;
            info.Province = province;
            info.CtxHtml = ctxHtml;
            info.City = city;
            info.InfoCtx = infoCtx;
            info.InfoType = infoType;
            if (string.IsNullOrEmpty(info.InfoType))
            {
                info.InfoType = "通知公告";
            }
            info.LastModifier = info.Creator = ToolDb.EmptyGuid;
            try
            {
                info.CreateTime = DateTime.Now;
                info.LastModifyTime = DateTime.Now;
            }
            catch
            {
                info.CreateTime = null;
                info.LastModifyTime = null;
            }
            return info;
        }

        /// <summary>
        /// 企业获奖信息
        /// </summary>
        /// <param name="prov"></param>
        /// <param name="city"></param>
        /// <param name="area"></param>
        /// <param name="corpCode"></param>
        /// <param name="corpName"></param>
        /// <param name="meritYear"></param>
        /// <param name="meritName"></param>
        /// <param name="meritDate"></param>
        /// <param name="meritLevel"></param>
        /// <param name="meritRegion"></param>
        /// <param name="meritSector"></param>
        /// <param name="meritPrjName"></param>
        /// <param name="prjSupporter"></param>
        /// <param name="source"></param>
        /// <param name="url"></param>
        /// <param name="remark"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        public static CorpMerit GenCorpMerit(string prov, string city, string area, string corpCode, string corpName, string meritYear, string meritName, string meritDate, string meritLevel, string meritRegion, string meritSector, string meritPrjName, string prjSupporter, string source, string url, string remark, string details)
        {
            CorpMerit info = new CorpMerit();
            info.Prov = prov;
            info.City = city;
            info.Area = area;
            info.CorpCode = corpCode;
            info.CorpName = corpName;
            info.MeritYear = meritYear;
            info.MeritName = meritName;
            info.MeritDate = meritDate;
            info.MeritLevel = meritLevel;
            info.MeritRegion = meritRegion;
            info.MeritSector = meritSector;
            info.MeritPrjName = meritPrjName;
            info.PrjSupporter = prjSupporter;
            info.Source = source;
            info.Url = url;
            info.Remark = remark;
            info.Details = details;

            info.Id = ToolDb.NewGuid;
            info.Creator = info.Modifier = ToolDb.EmptyGuid;
            info.CreateTime = info.ModifyTime = DateTime.Now;
            return info;
        }

        public static CorpMerit GenCorpMerit(string prov, string city, string area, string corpCode, string corpName, string meritYear, string meritName, string meritDate, string meritLevel, string meritRegion, string meritSector, string meritPrjName, string prjSupporter, string source, string url, string remark, string details, string MeritType, string PrjMgr, string SupMgr, string ManCost, string ProArea, string SupUnit, string PileConsUnit, string BuildingType)
        {
            CorpMerit info = GenCorpMerit(prov, city, area, corpCode, corpName, meritYear, meritName, meritDate, meritLevel, meritRegion, meritSector, meritPrjName, prjSupporter, source, url, remark, details);

            info.MeritType = MeritType;
            info.PrjMgr = PrjMgr;
            info.SupMgr = SupMgr;
            info.ManCost = ManCost;
            info.ProArea = ProArea;
            info.SupUnit = SupUnit;
            info.PileConsUnit = PileConsUnit;
            info.BuildingType = BuildingType;

            return info;
        }

        /// <summary>
        /// 项目信息
        /// </summary>
        /// <param name="titemcode"></param>
        /// <param name="citemname"></param>
        /// <param name="cbuildunit"></param>
        /// <param name="caddress"></param>
        /// <param name="cinvestment"></param>
        /// <param name="cbuildkind"></param>
        /// <param name="cinvestkind"></param>
        /// <param name="clinkman"></param>
        /// <param name="clinkmantel"></param>
        /// <param name="citemdesc"></param>
        /// <param name="capprno"></param>
        /// <param name="capprdate"></param>
        /// <param name="capprunit"></param>
        /// <param name="capprresult"></param>
        /// <param name="clandapprno"></param>
        /// <param name="clandplanno"></param>
        /// <param name="cbuilddate"></param>
        /// <param name="cprovince"></param>
        /// <param name="ccity"></param>
        /// <param name="cinfosource"></param>
        /// <param name="curl"></param>
        /// <param name="ctextcode"></param>
        /// <param name="cliccode"></param>
        /// <param name="smsgtype"></param>
        /// <returns></returns>
        public static ItemInfo GenItemInfo(string titemcode, string citemname, string cbuildunit, string caddress, string cinvestment, string cbuildkind, string cinvestkind,
            string clinkman, string clinkmantel, string citemdesc, string capprno, string capprdate, string capprunit, string capprresult,
           string clandapprno, string clandplanno, string cbuilddate, string cprovince, string ccity,
            string cinfosource, string curl, string ctextcode, string cliccode, string smsgtype, string ctxHtml)
        {
            ItemInfo info = new ItemInfo();
            info.Id = ToolDb.NewGuid;
            info.Address = caddress;
            info.ApprDate = capprdate;
            info.ApprNo = capprno;
            info.ApprResult = capprresult;
            info.ApprUnit = capprunit;
            try
            {
                info.BuildDate = DateTime.Parse(cbuilddate);
            }
            catch
            {
                info.BuildDate = null;
            }
            info.CreateTime = DateTime.Now;
            info.BuildKind = cbuildkind;
            info.BuildUnit = cbuildunit;
            info.City = ccity;
            info.InfoSource = cinfosource;
            info.InvestKind = cinvestkind;
            info.Investment = cinvestment;
            info.ItemCode = titemcode;
            info.ItemDesc = citemdesc;
            info.ItemName = citemname.GetPrjNameByName();
            info.LandApprNo = clandapprno;
            info.LandPlanNo = clandplanno;
            info.LicCode = cliccode;
            info.Linkman = clinkman;
            info.LinkmanTel = clinkmantel;
            info.MsgType = smsgtype;
            info.Province = cprovince;
            info.TextCode = ctextcode;
            info.Url = curl;
            info.CtxHtml = ctxHtml;
            return info;
        }

        /// <summary>
        /// 企业诚信行为记录
        /// </summary>
        /// <returns></returns>
        public static CreditAction GenCreditAction(string corpCode, string corpName, string targetCode, string targetDesc,
            string targetClass, string targetLevel, string targetUnit, string docNo, string beginDateTime, string actionDateTime,
            string actionType, string province, string city, string infoSource, string url, string prjName)
        {
            CreditAction info = new CreditAction();
            info.Id = ToolDb.NewGuid;
            info.CorpCode = corpCode ?? string.Empty;
            info.CorpName = corpName.GetPrjNameByName() ?? string.Empty;
            info.TargetCode = targetCode ?? string.Empty;
            info.TargetDesc = targetDesc ?? string.Empty;
            info.TargetClass = targetClass ?? string.Empty;
            info.TargetLevel = targetLevel ?? string.Empty;
            info.TargetUnit = targetUnit ?? string.Empty;
            info.DocNo = docNo ?? string.Empty;
            info.ProjectName = prjName ?? string.Empty;
            try
            {
                info.BeginDateTime = DateTime.Parse(beginDateTime);
            }
            catch
            {
                info.BeginDateTime = null;
            }
            try
            {
                info.ActionDateTime = DateTime.Parse(actionDateTime);
            }
            catch
            {
                info.ActionDateTime = null;
            }
            try
            {
                info.CreateTime = DateTime.Now;
            }
            catch
            {
                info.CreateTime = null;
            }
            info.ActionType = actionType ?? string.Empty;
            info.Province = province ?? string.Empty;
            info.City = city;
            info.InfoSource = infoSource ?? string.Empty;
            info.Url = url;
            return info;
        }

        /// <summary>
        /// 企业诚信参与凭评价信用记录
        /// </summary>
        /// <param name="corpName"></param>
        /// <param name="projectName"></param>
        /// <param name="tgargetCode"></param>
        /// <param name="targetDesc"></param>
        /// <param name="targetClass"></param>
        /// <param name="actionDateTime"></param>
        /// <param name="actionType"></param>
        /// <param name="province"></param>
        /// <param name="city"></param>
        /// <param name="infoSource"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static CreditAppraise GenCreditAppraise(string corpName, string projectName, string targetCode, string targetDesc,
            string targetClass, string actionDateTime, string actionType, string province, string city, string infoSource, string url)
        {
            CreditAppraise info = new CreditAppraise();
            info.Id = ToolDb.NewGuid;
            info.CorpName = corpName.GetPrjNameByName() ?? string.Empty;
            info.ProjectName = projectName ?? string.Empty;
            info.TargetCode = targetCode ?? string.Empty;
            info.TargetDesc = targetDesc ?? string.Empty;
            info.TargetClass = targetClass ?? string.Empty;
            try
            {
                info.ActionDateTime = DateTime.Parse(actionDateTime);
            }
            catch
            {
                info.ActionDateTime = null;
            }
            try
            {
                info.CreateTime = DateTime.Now;
            }
            catch
            {
                info.CreateTime = null;
            }
            info.ActionType = actionType;
            info.Province = province;
            info.City = city;
            info.InfoSource = infoSource;
            info.Url = url;
            return info;
        }

        /// <summary>
        /// 附件实体
        /// </summary>
        /// <param name="attachName"></param>
        /// <param name="SourceID"></param>
        /// <param name="url"></param>
        /// <param name="attachSize"></param>
        /// <param name="attachType"></param>
        /// <returns></returns>
        public static BaseAttach GenBaseAttach(string id, string attachName, string SourceID, string url, string attachSize, string attachType)
        {
            BaseAttach info = new BaseAttach();
            info.Id = id;
            info.AttachName = attachName;
            info.SourceID = SourceID;
            info.AttachServerPath = url;
            try
            {
                info.AttachSize = decimal.Parse(attachSize);
            }
            catch { info.AttachSize = 0; }
            info.AttachType = attachType;
            info.Creator = ToolDb.EmptyGuid;
            info.CreateTime = DateTime.Now;
            return info;
        }

        /// <summary>
        /// 生成招标信息实体
        /// </summary>
        /// <param name="prov">省</param>
        /// <param name="city">市</param>
        /// <param name="area">区</param>
        /// <param name="road">街道办</param>
        /// <param name="code">招标编号</param>
        /// <param name="prjName">项目名称</param>
        /// <param name="prjAdd">项目地址</param>
        /// <param name="buildUnit">建设单位</param>
        /// <param name="beginDate">公告开始时间</param>
        /// <param name="endDate">公告结束时间</param>
        /// <param name="ctx">公告内容</param>
        /// <param name="remark">备注</param>
        /// <param name="msgType">发布机构：如深圳市交易中心、广州市交易中心</param>
        /// <param name="inviteType">工程类型：如施工、监理、设计、勘察、服务、劳务分包、专业分包、小型施工、设备材料</param>
        /// <param name="specType">招标类型：如建设工程、政府采购、机电设备</param>
        /// <param name="otherType">专业类型：如建设工程下可分为房建及工业民用建筑、市政工程、园林绿化工程、装饰装修工程、电力工程、水利工程、环保工程</param>
        /// <param name="infoUrl">信息来源，公告url</param>
        /// <returns></returns>
        public static InviteInfo GenInviteInfo(string prov, string city, string area, string road, string code, string prjName, string prjAdd, string buildUnit,
            string beginDate, string endDate, string ctx, string remark, string msgType, string inviteType, string specType, string otherType, string infoUrl)
        {
            InviteInfo info = new InviteInfo();
            info.Area = area ?? string.Empty;
            try
            {
                info.BeginDate = DateTime.Parse(beginDate);
            }
            catch (Exception)
            {
                info.BeginDate = null;
            }
            info.Code = code ?? string.Empty;
            info.BuildUnit = buildUnit ?? string.Empty;
            info.City = city ?? string.Empty; 
            info.LastModifyTime = info.CreateTime = DateTime.Now;
            info.LastModifier = info.Creator = ToolDb.EmptyGuid;
            try
            {
                info.EndDate = DateTime.Parse(endDate);
            }
            catch (Exception)
            {
                info.EndDate = null;
            }
            info.Id = ToolDb.NewGuid;
            info.InfoUrl = infoUrl ?? string.Empty;
            info.InviteCtx = ctx ?? string.Empty;
            info.InviteType = inviteType ?? string.Empty;
            info.MsgType = msgType ?? string.Empty;
            info.OtherType = otherType ?? string.Empty;
            info.ProjectAddr = prjAdd ?? string.Empty;
            if (Encoding.Default.GetByteCount(info.ProjectAddr) >= 150)
                info.ProjectAddr = string.Empty;
            info.ProjectName = prjName.GetPrjNameByName() ?? string.Empty;
            info.Prov = prov ?? string.Empty;
            info.Remark = remark ?? string.Empty;
            info.Road = road ?? string.Empty;
            info.SpeType = specType ?? string.Empty;
            return info;
        }


        /// <summary>
        /// 生成中标信息实体
        /// </summary>
        /// <param name="pProvince">省</param>
        /// <param name="pCity">市</param>
        /// <param name="pInfoSource">信息来源</param>
        /// <param name="pBeginDate">开始时间</param>
        /// <param name="pEndDate">结束时间</param>
        /// <param name="prjName">项目名称</param>
        /// <param name="pConstUnit">施工单位</param>
        /// <param name="pSuperUnit">监理单位</param>
        /// <param name="pDesignUnit">设计单位</param>
        /// <param name="pProspUnit">勘察单位</param>
        /// <param name="pInviteArea">招标面积</param>
        /// <param name="pBuildArea">建筑总面积</param>
        /// <param name="pPrjClass">工程类别</param>
        /// <param name="pProClassLevel">工程类别等级</param>
        /// <param name="pChargeDept">主管部门</param>
        /// <param name="pPrjAddress">项目地址</param>
        /// <param name="pBuildUnit">建设单位</param>
        /// <param name="pPrjCode">工程编号</param>
        /// <param name="pUrl">信息来源，公告url</param>
        /// <returns></returns>
        public static BaseProject GenBaseProject(string pProvince, string pUrl, string pCity, string pInfoSource, string pBuilTime, string pBeginDate, string pEndDate, string pConstUnit, string pSuperUnit, string pDesignUnit, string pProspUnit, string pInviteArea, string pBuildArea, string pPrjClass, string pProClassLevel, string pChargeDept, string pPrjAddress, string pBuildUnit, string pPrjCode, string PrjName, string pCreatetime, string msgType)
        {
            BaseProject info = new BaseProject();
            info.Id = ToolDb.NewGuid;
            info.BuildArea = pBuildArea ?? string.Empty;
            info.BuildUnit = pBuildUnit ?? string.Empty;
            info.ChargeDept = pChargeDept ?? string.Empty;
            info.City = pCity ?? string.Empty;
            info.ConstUnit = pConstUnit ?? string.Empty;
            try
            {
                info.Createtime = DateTime.Now;
            }
            catch
            {
                info.Createtime = null;
            }
            info.DesignUnit = pDesignUnit ?? string.Empty;
            info.InfoSource = pInfoSource ?? string.Empty;
            info.InviteArea = pInviteArea ?? string.Empty;
            info.PrjAddress = pPrjAddress ?? string.Empty;
            info.PrjClass = pPrjClass ?? string.Empty;
            info.PrjCode = pPrjCode ?? string.Empty;
            try
            {
                info.BuildTime = DateTime.Parse(pBuilTime);
            }
            catch
            {
                info.BuildTime = null;
            }
            try
            {
                info.PrjEndDate = DateTime.Parse(pEndDate);
            }
            catch
            {
                info.PrjEndDate = null;
            }
            info.PrjName = PrjName.GetPrjNameByName() ?? string.Empty;
            try
            {
                info.PrjStartDate = DateTime.Parse(pBeginDate);
            }
            catch
            {
                info.PrjStartDate = null;
            }
            info.PrjClassLevel = pProClassLevel ?? string.Empty;
            info.ProspUnit = pProspUnit ?? string.Empty;
            info.Province = pProvince ?? string.Empty;
            info.SuperUnit = pSuperUnit ?? string.Empty;
            info.Url = pUrl ?? string.Empty;
            info.MsgType = msgType ?? string.Empty;
            return info;
        }


        /// <summary>
        /// 生成中标信息实体
        /// </summary>
        /// <param name="pProvince">省</param>
        /// <param name="pCity">市</param>
        /// <param name="pInfoSource">信息来源</param>
        /// <param name="prjName">项目名称</param>
        /// <param name="pRecordDate">备案日期</param>
        /// <param name="pCompactPrice">合同价</param>
        /// <param name="pCompactType">合同类型</param>
        /// <param name="pPrjMgrQual">项目经理资格</param>
        /// <param name="pPrjMgrName">项目经理名称</param>
        /// <param name="pContUnit">承包单位</param>
        /// <param name="pBuildUnit">建设单位</param>
        /// <param name="pPrjCode">工程编号</param>
        /// <param name="pUrl">信息来源，公告url</param>
        /// <returns></returns>
        public static ProjectConpact GenProjectConpact(string pProvince, string pUrl, string pCity, string pSubcontractCode, string pSubcontractName, string pSubcontractCompany, string pInfoSource, string pRecordDate, string pCompactPrice, string pCompactType, string pBuildUnit, string pPrjCode, string PrjName, string pPrjMgrQual, string pPrjMgrName, string pContUnit, string pCreatetime, string msgType)
        {
            ProjectConpact info = new ProjectConpact();
            info.Id = ToolDb.NewGuid;
            try
            {
                info.CompactPrice = decimal.Parse(pCompactPrice);
            }
            catch
            {
                info.CompactPrice = null;
            }
            info.BuildUnit = pBuildUnit ?? string.Empty;
            info.PrjMgrName = pPrjMgrName ?? string.Empty;
            info.City = pCity ?? string.Empty;
            info.PrjMgrQual = pPrjMgrQual ?? string.Empty;
            try
            {
                info.Createtime = DateTime.Now;
            }
            catch
            {
                info.Createtime = null;
            }
            try
            {
                info.RecordDate = DateTime.Parse(pRecordDate);
            }
            catch
            {
                info.RecordDate = null;
            }
            info.SubcontractCode = pSubcontractCode ?? string.Empty;
            info.SubcontractName = pSubcontractName ?? string.Empty;
            info.SubcontractCompany = pSubcontractCompany ?? string.Empty;
            info.InfoSource = pInfoSource ?? string.Empty;
            info.ContUnit = pContUnit ?? string.Empty;
            info.PrjCode = pPrjCode ?? string.Empty;
            info.CompactType = pCompactType ?? string.Empty;
            info.PrjName = PrjName.GetPrjNameByName() ?? string.Empty;
            info.Province = pProvince ?? string.Empty;
            info.Url = pUrl ?? string.Empty;
            info.MsgType = msgType ?? string.Empty;
            return info;
        }


        /// <summary>
        /// 生成中标信息实体
        /// </summary>
        /// <param name="pProvince">省</param>
        /// <param name="pUrl">来源地址</param>
        /// <param name="pCity">市</param>
        /// <param name="pEndDate">竣工验收备案时间</param>
        /// <param name="pConstUnit">施工单位</param>
        /// <param name="pSuperUnit">监理单位</param>
        /// <param name="pDesignUnit">设置单位</param>
        /// <param name="prjEndDesc">验收结果</param>
        /// <param name="pPrjAddress">工程地点</param>
        /// <param name="pBuildUnit">建设单位</param>
        /// <param name="pPrjCode">工程备案编号</param>
        /// <param name="PrjName">工程名称</param>
        /// <param name="pRecordUnit">备案机关</param>
        /// <returns></returns>
        public static ProjectFinish GenProjectFinish(string pProvince, string pUrl, string pCity, string pInfoSource, string pEndDate, string pConstUnit, string pSuperUnit, string pDesignUnit, string prjEndDesc, string pPrjAddress, string pBuildUnit, string pPrjCode, string PrjName, string pRecordUnit, string pCreatetime, string msgType, string licUnit)
        {
            ProjectFinish info = new ProjectFinish();
            info.Id = ToolDb.NewGuid;
            info.Province = pProvince ?? string.Empty;
            info.Url = pUrl ?? string.Empty;
            info.City = pCity ?? string.Empty;
            info.InfoSource = pInfoSource ?? string.Empty;
            try
            {
                info.PrjEndDate = DateTime.Parse(pEndDate);
            }
            catch
            {
                info.PrjEndDate = null;
            }
            info.ConstUnit = pConstUnit ?? string.Empty;
            try
            {
                info.Createtime = DateTime.Now.ToString();
            }
            catch
            {
                info.Createtime = null;
            }
            info.DesignUnit = pDesignUnit ?? string.Empty;
            info.PrjAddress = pPrjAddress ?? string.Empty;
            info.PrjEndDesc = prjEndDesc ?? string.Empty;
            info.PrjEndCode = pPrjCode ?? string.Empty;
            info.PrjName = PrjName.GetPrjNameByName() ?? string.Empty;
            info.BuildUnit = pBuildUnit ?? string.Empty;
            info.SuperUnit = pSuperUnit ?? string.Empty;
            info.RecordUnit = pRecordUnit ?? string.Empty;
            info.MsgType = msgType ?? string.Empty;
            info.LicUnit = licUnit ?? string.Empty;
            return info;
        }


        /// <summary>
        /// 生成中标信息实体
        /// </summary>
        /// <param name="pPrjName">工程名称</param>
        /// <param name="pBuildUnit">建设单位</param>
        /// <param name="pBuildAddress">建设地点</param>
        /// <param name="pBuildManager">建设单位负责人</param>
        /// <param name="pBuildScale">建设面积</param>
        /// <param name="pPrjStartDate">开始时间</param>
        /// <param name="PrjEndDate">结束时间</param>
        /// <param name="pConstUnit">施工单位</param>
        /// <param name="pConstUnitManager">施工单位负责人</param>
        /// <param name="pSuperUnit">监理单位</param>
        /// <param name="pSuperUnitManager">监理单位负责人</param>
        /// <param name="pProspUnit">勘察单位</param>
        /// <param name="pProspUnitManager">勘察单位负责人</param>
        /// <param name="pDesignUnit">设计单位</param>
        /// <param name="pDesignUnitManager">设计单位负责人</param>
        /// <param name="pPrjManager">项目负责人</param>
        /// <param name="pSpecialPerson">特殊作业人员</param>
        /// <param name="pLicUnit">发证机关</param>
        /// <param name="pPrjLicCode">施工许可证号</param>
        /// <param name="PrjLicDate">发证日期</param>
        /// <param name="pPrjDesc">完成情况</param>
        /// <param name="pProvince">省</param>
        /// <param name="pCity">市</param>
        /// <param name="pInfoSource">信息来源</param>
        /// <param name="pUrl">来源地址</param>
        /// <param name="pPrjPrice">工程造价</param>
        /// <returns></returns>
        public static ProjectLic GenProjectLic(string pPrjName, string pBuildUnit, string pBuildAddress, string pBuildManager, string pBuildScale, string pPrjPrice, string pPrjStartDate, string PrjEndDate, string pConstUnit, string pConstUnitManager, string pSuperUnit, string pSuperUnitManager, string pProspUnit, string pProspUnitManager, string pDesignUnit, string pDesignUnitManager, string pPrjManager, string pSpecialPerson, string pLicUnit, string pPrjLicCode, string PrjLicDate, string pPrjDesc, string pProvince, string pCity, string pInfoSource, string pUrl, string pCreatetime, string pPrjCode, string msgType)
        {
            ProjectLic info = new ProjectLic();
            info.PrjCode = pPrjCode;
            info.BuildAddress = pBuildAddress ?? string.Empty;
            info.BuildManager = pBuildManager ?? string.Empty;
            info.BuildScale = pBuildScale ?? string.Empty;
            info.BuildUnit = pBuildUnit ?? string.Empty;
            info.City = pCity ?? string.Empty;
            info.ConstManager = pConstUnitManager ?? string.Empty;
            info.ConstUnit = pConstUnit ?? string.Empty;
            try
            {
                info.Createtime = DateTime.Now;
            }
            catch
            {
                info.Createtime = null;
            }
            info.DesignManager = pDesignUnitManager ?? string.Empty;
            info.DesignUnit = pDesignUnit ?? string.Empty;
            info.Id = ToolDb.NewGuid;
            info.InfoSource = pInfoSource ?? string.Empty;
            info.LicUnit = pLicUnit ?? string.Empty;
            info.PrjDesc = pPrjDesc ?? string.Empty;
            try
            {
                info.PrjEndDate = DateTime.Parse(PrjEndDate);
            }
            catch
            {
                info.PrjEndDate = null;
            }
            info.PrjLicCode = pPrjLicCode;
            try
            {
                info.PrjLicDate = DateTime.Parse(PrjLicDate);
            }
            catch
            {
                info.PrjLicDate = null;
            }
            info.PrjManager = pPrjManager ?? string.Empty;
            info.PrjName = pPrjName.GetPrjNameByName() ?? string.Empty;
            info.PrjPrice = pPrjPrice ?? string.Empty;
            try
            {
                info.PrjStartDate = DateTime.Parse(pPrjStartDate);
            }
            catch
            {
                info.PrjStartDate = null;
            }
            info.ProspManager = pProspUnitManager ?? string.Empty;
            info.ProspUnit = pProspUnit ?? string.Empty;
            info.Province = pProvince ?? string.Empty;
            info.SpecialPerson = pSpecialPerson ?? string.Empty;
            info.SuperManager = pSuperUnitManager ?? string.Empty;
            info.SuperUnit = pSuperUnit ?? string.Empty;
            info.Url = pUrl ?? string.Empty;
            info.MsgType = msgType ?? string.Empty;
            return info;
        }

        public static InviteInfo GenInviteInfo(string prov, string city, string area, string road, string code, string prjName, string prjAdd, string buildUnit,
           string beginDate, string endDate, string ctx, string remark, string msgType, string inviteType, string specType, string otherType, string infoUrl, string CtxHtml)
        {
            InviteInfo info = GenInviteInfo(prov, city, area, road, code, prjName, prjAdd, buildUnit, beginDate, endDate, ctx, remark, msgType, inviteType, specType, otherType, infoUrl);

            info.CtxHtml = CtxHtml ?? string.Empty;
            info.Htmlmd5 = GetMD5(info.CtxHtml ?? string.Empty);
            return info;
        }

        /// <summary>
        /// 生成中标信息实体
        /// </summary>
        /// <param name="prov">省</param>
        /// <param name="city">市</param>
        /// <param name="area">区</param>
        /// <param name="road">街道办</param>
        /// <param name="code">招标编号</param>
        /// <param name="prjName">项目名称</param>
        /// <param name="buildUnit">建设单位</param>
        /// <param name="bidDate">中标时间</param>
        /// <param name="bidUnit">中标单位</param>
        /// <param name="beginDate">公告开始时间</param>
        /// <param name="endDate">公告结束时间</param>
        /// <param name="ctx">公告内容</param>
        /// <param name="remark">备注</param>
        /// <param name="msgType">发布机构：如深圳市交易中心、广州市交易中心</param>
        /// <param name="bidType">工程类型：如施工、监理、设计、勘察、服务、劳务分包、专业分包、小型施工、设备材料</param>
        /// <param name="specType">招标类型：如建设工程、政府采购、机电设备</param>
        /// <param name="otherType">专业类型：如建设工程下可分为房建及工业民用建筑、市政工程、园林绿化工程、装饰装修工程、电力工程、水利工程、环保工程</param>
        /// <param name="bidMoney">中标金额</param>
        /// <param name="infoUrl">信息来源，公告url</param>
        /// <returns></returns>
        public static BidInfo GenBidInfo1(string prov, string city, string area, string road, string code, string prjName, string buildUnit, string bidDate,
            string bidUnit, string beginDate, string endDate, string ctx, string remark, string msgType, string bidType, string specType, string otherType,
            string bidMoney, string infoUrl, string prjMgr)
        {
            BidInfo info = new BidInfo();
            info.Area = area ?? string.Empty;
            try
            {
                info.BeginDate = DateTime.Parse(beginDate);
            }
            catch (Exception)
            {
                info.BeginDate = null;
            }
            info.BidCtx = ctx ?? string.Empty;
            try
            {
                info.BidDate = DateTime.Parse(bidDate);
            }
            catch (Exception)
            {
                info.BidDate = null;
            }
            try
            {
                info.BidMoney = decimal.Round(decimal.Parse(bidMoney), 6);
            }
            catch (Exception)
            {
                info.BidMoney = 0;
            }
            info.BidType = bidType ?? string.Empty;
            info.BidUnit = bidUnit;
            info.BuildUnit = buildUnit ?? string.Empty;
            info.City = city ?? string.Empty;
            info.Code = code ?? string.Empty;
            info.LastModifyTime = info.CreateTime = DateTime.Now;
            info.LastModifier = info.Creator = ToolDb.EmptyGuid;
            try
            {
                info.EndDate = DateTime.Parse(endDate);
            }
            catch (Exception)
            {
                info.EndDate = null;
            }
            info.Id = ToolDb.NewGuid;
            info.InfoUrl = infoUrl ?? string.Empty;
            info.MsgType = msgType ?? string.Empty;
            info.OtherType = otherType ?? string.Empty;
            info.ProjectName = prjName.GetPrjNameByName() ?? string.Empty;
            info.Prov = prov ?? string.Empty;
            info.Remark = remark ?? string.Empty;
            info.Road = road ?? string.Empty;
            info.SpeType = specType ?? string.Empty;
            info.PrjMgr = prjMgr;
            return info;
        }


        public static BidInfo GenBidInfo(string prov, string city, string area, string road, string code, string prjName, string buildUnit, string bidDate,
           string bidUnit, string beginDate, string endDate, string ctx, string remark, string msgType, string bidType, string specType, string otherType,
           string bidMoney, string infoUrl, string prjMgr, string CtxHtml)
        {
            BidInfo info = GenBidInfo1(prov, city, area, road, code, prjName, buildUnit, bidDate, bidUnit, beginDate, endDate, ctx, remark, msgType, bidType, specType, otherType, bidMoney, infoUrl, prjMgr);

            info.CtxHtml = CtxHtml ?? string.Empty;
            info.Htmlmd5 = GetMD5(info.CtxHtml ?? string.Empty);
            if (InfoDbhelp.AddNoticeInfoByBidInfo(info))
            {
                BidInfo entity = new BidInfo();
                entity.Id = info.Id;
                return entity;
            }
            return info;
        }

        /// <summary>
        /// 生成处罚警示信息实体
        /// </summary>
        /// <param name="prov"></param>
        /// <param name="city"></param>
        /// <param name="area"></param>
        /// <param name="code"></param>
        /// <param name="warningName"></param>
        /// <param name="deliveryDate"></param>
        /// <param name="warningType"></param>
        /// <param name="punishmentType"></param>
        /// <param name="prjNumber"></param>
        /// <param name="totalScore"></param>
        /// <param name="resultScore"></param>
        /// <param name="corpType"></param>
        /// <param name="publicEndDate"></param>
        /// <param name="warningEndDate"></param>
        /// <param name="prjName"></param>
        /// <param name="badInfo"></param>
        /// <param name="msgType"></param>
        /// <returns></returns>
        public static CorpWarning GenCorpWarning(string prov, string city, string area, string code, string warningName, string deliveryDate, string warningType, string punishmentType, string prjNumber, string totalScore, string resultScore, string corpType, string publicEndDate, string warningEndDate, string prjName, string badInfo, string msgType, string color)
        {
            CorpWarning info = new CorpWarning();
            info.Id = ToolDb.NewGuid;
            info.Prov = prov;
            info.City = city;
            info.Code = code;
            info.WarningName = warningName;
            try { info.DeliveryDate = DateTime.Parse(deliveryDate); }
            catch { }
            info.WarningType = warningType;
            info.PunishmentType = punishmentType;
            try { info.PrjNumber = int.Parse(prjNumber); }
            catch { }
            info.TotalScore = totalScore;
            info.ResultScore = resultScore;
            info.CorpType = corpType;
            try { info.PublicEndDate = DateTime.Parse(publicEndDate); }
            catch { }
            try { info.WarningEndDate = DateTime.Parse(warningEndDate); }
            catch { }
            info.PrjName = prjName.GetPrjNameByName();
            info.BadInfo = badInfo;
            info.MsgType = msgType;
            info.CreateTime = info.LastModifierTime = DateTime.Now;
            info.Creator = info.LastModifier = EmptyGuid;
            info.Color = color;
            return info;
        }

        public static CorpBehavior GenCorpBehavior(string pcorpName, string pcorpType, string pbehavior, string pbehaviorCtx, string pothery1, string pothery2, string pothery3, string pbeginDate)
        {
            CorpBehavior beh = new CorpBehavior();
            beh.Id = ToolDb.NewGuid;
            beh.CorpName = pcorpName.GetPrjNameByName();
            beh.CorpType = pcorpType;
            beh.Behavior = pbehavior;
            beh.BehaviorCtx = pbehaviorCtx;
            beh.Othery1 = pothery1;
            beh.Othery2 = pothery2;
            beh.Othery3 = pothery3;
            try
            {
                beh.BeginDate = DateTime.Parse(pbeginDate);
            }
            catch (Exception)
            {
                beh.BeginDate = null;
            }
            return beh;
        }

        /// <summary>
        /// 生成中标信息实体
        /// </summary>
        /// <param name="prov">省</param>
        /// <param name="city">市</param>
        /// <param name="area">区</param>
        /// <param name="road">街道办</param>
        /// <param name="code">招标编号</param>
        /// <param name="prjName">项目名称</param>
        /// <param name="buildUnit">建设单位</param>
        /// <param name="bidDate">中标时间</param>
        /// <param name="bidUnit">中标单位</param>
        /// <param name="beginDate">公告开始时间</param>
        /// <param name="endDate">公告结束时间</param>
        /// <param name="ctx">公告内容</param>
        /// <param name="remark">备注</param>
        /// <param name="msgType">发布机构：如深圳市交易中心、广州市交易中心</param>
        /// <param name="bidType">工程类型：如施工、监理、设计、勘察、服务、劳务分包、专业分包、小型施工、设备材料</param>
        /// <param name="specType">招标类型：如建设工程、政府采购、机电设备</param>
        /// <param name="otherType">专业类型：如建设工程下可分为房建及工业民用建筑、市政工程、园林绿化工程、装饰装修工程、电力工程、水利工程、环保工程</param>
        /// <param name="bidMoney">中标金额</param>
        /// <param name="infoUrl">信息来源，公告url</param>
        /// <returns></returns>
        public static BidInfo GenBidInfo(string prov, string city, string area, string road, string code, string prjName, string buildUnit, string bidDate,
            string bidUnit, string beginDate, string endDate, string ctx, string remark, string msgType, string bidType, string specType, string otherType,
            string bidMoney, string infoUrl, string prjMgr, string lastModifyTime, string CreateTime)
        {
            BidInfo info = new BidInfo();
            info.Area = area ?? string.Empty;
            try
            {
                info.BeginDate = DateTime.Parse(beginDate);
            }
            catch (Exception)
            {
                info.BeginDate = null;
            }
            info.BidCtx = ctx ?? string.Empty;
            try
            {
                info.BidDate = DateTime.Parse(bidDate);
            }
            catch (Exception)
            {
                info.BidDate = null;
            }
            try
            {
                info.BidMoney = decimal.Parse(bidMoney);
            }
            catch (Exception)
            {
                info.BidMoney = 0;
            }
            info.BidType = bidType ?? string.Empty;
            info.BidUnit = bidUnit;
            info.BuildUnit = buildUnit ?? string.Empty;
            info.City = city ?? string.Empty;
            info.Code = code ?? string.Empty;
            info.LastModifyTime = DateTime.Now;
            info.CreateTime = DateTime.Now;
            info.LastModifier = info.Creator = ToolDb.EmptyGuid;
            try
            {
                info.EndDate = DateTime.Parse(endDate);
            }
            catch (Exception)
            {
                info.EndDate = null;
            }
            info.Id = ToolDb.NewGuid;
            info.InfoUrl = infoUrl ?? string.Empty;
            info.MsgType = msgType ?? string.Empty;
            info.OtherType = otherType ?? string.Empty;
            info.ProjectName = prjName.GetPrjNameByName() ?? string.Empty;
            info.Prov = prov ?? string.Empty;
            info.Remark = remark ?? string.Empty;
            info.Road = road ?? string.Empty;
            info.SpeType = specType ?? string.Empty;
            info.PrjMgr = prjMgr;
            if (InfoDbhelp.AddNoticeInfoByBidInfo(info))
            {
                BidInfo entity = new BidInfo();
                entity.Id = info.Id;
                return entity;
            }
            return info;
        }

        public static BidInfo GenBidInfo(string prov, string city, string area, string road, string code, string prjName, string buildUnit, string bidDate,
         string bidUnit, string beginDate, string endDate, string ctx, string remark, string msgType, string bidType, string specType, string otherType,
         string bidMoney, string infoUrl, string prjMgr, string lastModifyTime, string CreateTime, string CtxHtml)
        {
            BidInfo info = GenBidInfo(prov, city, area, road, code, prjName, buildUnit, bidDate, bidUnit, beginDate, endDate, ctx, remark, msgType, bidType, specType, otherType, bidMoney, infoUrl, prjMgr, lastModifyTime, CreateTime);

            info.CtxHtml = CtxHtml ?? string.Empty;
            info.Htmlmd5 = GetMD5(info.CtxHtml ?? string.Empty);
            if (InfoDbhelp.AddNoticeInfoByBidInfo(info))
            {
                BidInfo entity = new BidInfo();
                entity.Id = info.Id;
                return entity;
            }
            return info;
        }


        /// <summary>
        /// 生成会议信息实体
        /// </summary>
        /// <param name="prov"></param>
        /// <param name="city"></param>
        /// <param name="area"></param>
        /// <param name="road"></param>
        /// <param name="projectName"></param>
        /// <param name="meetPlace"></param>
        /// <param name="meetName"></param>
        /// <param name="beginDate"></param>
        /// <param name="remark"></param>
        /// <param name="infoSource"></param>
        /// <param name="infoUrl"></param>
        /// <returns></returns>
        public static MeetInfo GenMeetInfo(string prov, string city, string area, string road, string projectName, string meetPlace,
            string meetName, string beginDate, string remark, string infoSource, string infoUrl, string prjCode, string buildUnit, string other1, string other2)
        {
            MeetInfo info = new MeetInfo();
            info.Area = area;
            try
            {
                info.BeginDate = DateTime.Parse(beginDate);
            }
            catch (Exception)
            {
                info.BeginDate = null;
            }
            info.City = city;
            info.CreateTime = info.LastModifyTime = DateTime.Now;
            info.Creator = info.LastModifier = ToolDb.EmptyGuid;
            info.Id = ToolDb.NewGuid;
            info.InfoSource = infoSource;
            info.InfoUrl = infoUrl;
            info.MeetName = ToolHtml.GetMeetPrjName(meetName);
            info.MeetPlace = meetPlace;
            info.ProjectName = ToolHtml.GetMeetPrjName(projectName);
            info.Prov = prov;
            info.Remark = remark;
            info.Road = road;
            info.PrjCode = prjCode;
            info.BuildUnit = buildUnit;
            info.Other1 = other1;
            info.Other2 = other2;

            return info;
        }

        /// <summary>
        /// 生成通知及公示实体
        /// </summary>
        /// <param name="prov"></param>
        /// <param name="city"></param>
        /// <param name="area"></param>
        /// <param name="road"></param>
        /// <param name="infoTitle"></param>
        /// <param name="infoType"></param>
        /// <param name="infoCtx"></param>
        /// <param name="publishTime"></param>
        /// <param name="remark"></param>
        /// <param name="infoSource"></param>
        /// <param name="infoUrl"></param>
        /// <returns></returns>
        public static NoticeInfo GenNoticeInfo(string prov, string city, string area, string road, string infoTitle, string infoType,
            string infoCtx, string publishTime, string remark, string infoSource, string infoUrl, string prjCode, string buildUnit, string other1, string other2, string prjType, string bgType, string ctxHtml)
        {
            NoticeInfo info = new NoticeInfo();
            info.Area = area;
            info.City = city;
            info.CreateTime = info.LastModifyTime = DateTime.Now;
            info.Creator = info.LastModifier = ToolDb.EmptyGuid;
            info.Id = ToolDb.NewGuid;
            info.InfoCtx = infoCtx;
            info.InfoSource = infoSource;
            info.InfoTitle = infoTitle.GetPrjNameByName();
            info.InfoType = infoType;
            if (string.IsNullOrEmpty(info.InfoType))
            {
                info.InfoType = "通知公示";
            }
            info.InfoUrl = infoUrl;
            info.Prov = prov;
            try
            {
                info.PublishTime = DateTime.Parse(publishTime);
            }
            catch
            {
                info.PublishTime = null;
            }
            info.Remark = remark;
            info.Road = road;
            info.PrjCode = prjCode;
            info.BuildUnit = buildUnit;
            info.Other1 = other1;
            info.Other2 = other2;
            info.PrjType = prjType;
            info.BgType = bgType;
            info.CtxHtml = ctxHtml;
            return info;
        }



        /// <summary>
        ///  从业人员信息
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Sex"></param>
        /// <param name="CredType"></param>
        /// <param name="IdNum"></param>
        /// <param name="CorpName"></param>
        /// <param name="CorpCode"></param>
        /// <param name="CertCode"></param>
        /// <param name="RegLevel"></param>
        /// <param name="RegCode"></param>
        /// <param name="AuthorUnit"></param>
        /// <param name="PersonType"></param>
        /// <param name="CertGrade"></param>
        /// <param name="Province"></param>
        /// <param name="City"></param>
        /// <param name="InfoSource"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static CorpStaff GenCorpStaff(string Name, string Sex, string CredType, string IdNum, string CorpName, string CorpCode, string CertCode, string RegLevel, string RegCode, string AuthorUnit, string PersonType, string CertGrade, string Province, string City, string InfoSource, string Url, string Profession, string staffNum, string isDate, string organ, string state)
        {
            CorpStaff info = new CorpStaff();
            info.Id = ToolDb.NewGuid;
            info.Name = Name;
            info.Sex = Sex;
            info.CredType = CredType;
            info.IdNum = IdNum;
            info.CorpName = CorpName.GetPrjNameByName();
            info.CorpCode = CorpCode;
            info.CertCode = CertCode;
            info.RegLevel = RegLevel;
            info.RegCode = RegCode;
            info.AuthorUnit = AuthorUnit;
            info.PersonType = PersonType;
            info.CertGrade = CertGrade;
            info.Province = Province;
            info.City = City;
            info.InfoSource = InfoSource;
            info.Url = Url;

            info.CreateTime = DateTime.Now;
            info.Profession = Profession;
            try
            {
                info.StaffNum = int.Parse(staffNum);
            }
            catch
            {
                info.StaffNum = 0;
            }
            try
            {
                info.IssuanceTime = DateTime.Parse(isDate);
            }
            catch { info.IssuanceTime = null; }
            info.Organ = organ;
            info.CertState = state;
            return info;
        }




        public static CorpInfo GenCorpInfo(string CorpName, string CorpCode, string CorpAddress, string RegDate, string RegFund, string BusinessCode, string BusinessType, string LinkMan, string LinkPhone, string Fax, string Email, string CorpSite, string CorpType, string Province, string City, string InfoSource, string Url, string ISOQualNum, string ISOEnvironNum, string OffAdr)
        {
            CorpInfo info = new CorpInfo();
            info.Id = ToolDb.NewGuid;

            info.CorpName = CorpName.GetPrjNameByName();
            info.CorpCode = CorpCode;
            info.CorpAddress = CorpAddress;
            info.RegDate = RegDate;
            info.RegFund = RegFund;
            info.BusinessCode = BusinessCode;
            info.BusinessType = BusinessType;
            info.LinkMan = LinkMan;
            info.LinkPhone = LinkPhone;
            info.Fax = Fax;
            info.Email = Email;
            info.CorpSite = CorpSite;
            info.CorpType = CorpType;
            info.Province = Province;
            info.City = City;
            info.InfoSource = InfoSource;
            info.Url = Url;
            info.CreateTime = DateTime.Now;
            info.IsoEnvironNum = ISOEnvironNum;
            info.IsoQualNum = ISOQualNum;
            info.OffAdr = OffAdr;
            return info;
        }


        /// <summary>
        /// 企业负责人
        /// </summary>
        /// <param name="CorpId"></param>
        /// <param name="LeaderName"></param>
        /// <param name="LeaderDuty"></param>
        /// <param name="LeaderType"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static CorpLeader GenCorpLeader(string CorpId, string LeaderName, string LeaderDuty, string LeaderType, string Url, string phone = "")
        {
            CorpLeader info = new CorpLeader();
            info.Id = ToolDb.NewGuid;
            info.CorpId = CorpId;
            info.LeaderDuty = LeaderDuty;
            info.LeaderName = LeaderName.GetPrjNameByName();
            info.LeaderType = LeaderType;
            info.Url = Url;
            info.Phone = phone;
            return info;
        }


        /// <summary>
        /// 企业资质
        /// </summary>
        /// <param name="CorpId"></param>
        /// <param name="QualName"></param>
        /// <param name="QualCode"></param>
        /// <param name="QualSeq"></param>
        /// <param name="QualType"></param>
        /// <param name="QualLevel"></param>
        /// <param name="ValidDate"></param>
        /// <param name="LicDate"></param>
        /// <param name="LicUnit"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static CorpQual GenCorpQual(string CorpId, string QualName, string QualCode, string QualSeq, string QualType, string QualLevel, string ValidDate, string LicDate, string LicUnit, string Url, string qualNum, string provice, string city)
        {
            CorpQual info = new CorpQual();
            info.Id = ToolDb.NewGuid;
            info.CorpId = CorpId;
            info.QualName = QualName.GetPrjNameByName();
            info.QualCode = QualCode;
            info.QualSeq = QualSeq;
            info.QualType = QualType;
            info.QualLevel = QualLevel;
            info.ValidDate = ValidDate;
            info.LicDate = LicDate;
            info.LicUnit = LicUnit;
            info.Url = Url;
            info.Province = provice;
            info.City = city;
            try
            {
                info.QualNum = Convert.ToInt32(qualNum);
            }
            catch { info.QualNum = 0; }
            return info;

        }

        /// <summary>
        /// 企业认证信息
        /// </summary>
        /// <param name="CorpId"></param>
        /// <param name="CertInfo"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static CorpCert GenCorpCert(string CorpId, string CertInfo, string Url)
        {
            CorpCert info = new CorpCert();
            info.CorpId = CorpId;
            info.Id = ToolDb.NewGuid;
            info.Url = Url;
            info.CertInfo = CertInfo;
            return info;
        }

        /// <summary>
        /// 企业技术力量
        /// </summary>
        /// <param name="CorpId"></param>
        /// <param name="StaffName"></param>
        /// <param name="IdCard"></param>
        /// <param name="CertLevel"></param>
        /// <param name="CertNo"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static CorpTecStaff GenCorpTecStaff(string CorpId, string StaffName, string IdCard, string CertLevel, string CertNo, string Url, string staffType)
        {
            CorpTecStaff info = new CorpTecStaff();
            info.CorpId = CorpId;
            info.Id = ToolDb.NewGuid;
            info.StaffName = StaffName.GetPrjNameByName();
            info.IdCard = IdCard;
            info.CertLevel = CertLevel;
            info.CertNo = CertNo;
            info.Url = Url;
            info.SaffType = staffType;
            return info;
        }

        /// <summary>
        /// 企业机械设备
        /// </summary>
        /// <param name="CorpId"></param>
        /// <param name="DeviceName"></param>
        /// <param name="DeviceSpec"></param>
        /// <param name="DeviceCount"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static CorpDevice GenCorpDevice(string CorpId, string DeviceName, string DeviceSpec, string DeviceCount, string Url)
        {
            CorpDevice info = new CorpDevice();
            info.Id = ToolDb.NewGuid;
            info.CorpId = CorpId;
            info.DeviceName = DeviceName.GetPrjNameByName();
            info.DeviceSpec = DeviceSpec;
            info.DeviceCount = DeviceCount;
            info.Url = Url;
            return info;
        }

        /// <summary>
        /// 企业业绩
        /// </summary>
        /// <returns></returns>
        public static CorpResults GenCorpResults(string CorpId, string PrjName, string PrjCode, string BuildUnit, string GrantDate, string PrjAddress, string ChargeDept, string PrjClassLevel, string PrjClass, string BuildArea, string InviteArea, string ProspUnit, string DesignUnit, string SuperUnit, string ConstUnit, string PrjStartDate, string PrjEndDate, string Url)
        {
            CorpResults info = new CorpResults();
            info.Id = ToolDb.NewGuid;
            info.CorpId = CorpId;
            info.PrjName = PrjName.GetPrjNameByName();
            info.PrjCode = PrjCode;
            info.BuildUnit = BuildUnit;
            info.GrantDate = GrantDate;
            info.PrjAddress = PrjAddress;
            info.ChargeDept = ChargeDept;
            info.PrjClassLevel = PrjClassLevel;
            info.PrjClass = PrjClass;
            info.BuildArea = BuildArea;
            info.InviteArea = InviteArea;
            info.ProspUnit = ProspUnit;
            info.DesignUnit = DesignUnit;
            info.SuperUnit = SuperUnit;
            info.ConstUnit = ConstUnit;
            info.PrjStartDate = PrjStartDate;
            info.PrjEndDate = PrjEndDate;
            info.Url = Url;
            return info;
        }
        /// <summary>
        /// 企业安全生产许可证
        /// </summary>
        /// <param name="CorpId"></param>
        /// <param name="SecLicCode"></param>
        /// <param name="SecLicDesc"></param>
        /// <param name="ValidStartDate"></param>
        /// <param name="ValidStartEnd"></param>
        /// <param name="SecLicUnit"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static CorpSecLic GenCorpSecLic(string CorpId, string SecLicCode, string SecLicDesc, string ValidStartDate, string ValidStartEnd, string SecLicUnit, string Url)
        {
            CorpSecLic info = new CorpSecLic();
            info.Id = ToolDb.NewGuid;
            info.CorpId = CorpId;
            info.SecLicCode = SecLicCode;
            info.SecLicDesc = SecLicDesc;
            info.ValidStartDate = ValidStartDate;
            info.ValidStartEnd = ValidStartEnd;
            info.SecLicUnit = SecLicUnit;
            info.Url = Url;
            return info;
        }

        /// <summary>
        /// 企业人员安全证书
        /// </summary>
        /// <param name="CorpId"></param>
        /// <param name="PersonName"></param>
        /// <param name="PersonCertNo"></param>
        /// <param name="GrantUnit"></param>
        /// <param name="GrantDate"></param>
        /// <param name="Url"></param>
        public static CorpSecLicStaff GenCorpSecLicStaff(string CorpId, string PersonName, string PersonCertNo, string GrantUnit, string GrantDate, string Url)
        {
            CorpSecLicStaff info = new CorpSecLicStaff();
            info.Id = ToolDb.NewGuid;
            info.CorpId = CorpId;
            info.PersonName = PersonName.GetPrjNameByName();
            info.PersonCertNo = PersonCertNo;
            info.GrantUnit = GrantUnit;
            info.GrantDate = GrantDate;
            info.Url = Url;
            return info;
        }

        /// <summary>
        /// 企业获奖信息
        /// </summary>
        /// <param name="CorpId"></param>
        /// <param name="AwardName"></param>
        /// <param name="AwardDate"></param>
        /// <param name="AwardLevel"></param>
        /// <param name="GrantUnit"></param>
        /// <param name="ProjectName"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static CorpAward GenCorpAward(string CorpId, string AwardName, string AwardDate, string AwardLevel, string GrantUnit, string ProjectName, string Url)
        {
            CorpAward info = new CorpAward();
            info.Id = ToolDb.NewGuid;
            info.CorpId = CorpId;
            info.AwardName = AwardName;
            info.AwardDate = AwardDate;
            info.AwardLevel = AwardLevel;
            info.GrantUnit = GrantUnit;
            info.ProjectName = ProjectName.GetPrjNameByName();
            info.Url = Url;
            return info;
        }

        /// <summary>
        /// 企业处罚信息
        /// </summary>
        /// <param name="CorpId"></param>
        /// <param name="DocNo"></param>
        /// <param name="PunishType"></param>
        /// <param name="GrantUnit"></param>
        /// <param name="DocDate"></param>
        /// <param name="PunishCtx"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static CorpPunish GenCorpPunish(string CorpId, string DocNo, string PunishType, string GrantUnit, string DocDate, string PunishCtx, string Url, string GrantName, string isShow)
        {
            CorpPunish info = new CorpPunish();
            info.Id = ToolDb.NewGuid;
            info.CorpId = CorpId;
            info.DocNo = DocNo;
            info.PunishType = PunishType;
            info.GrantUnit = GrantUnit;
            info.DocDate = DocDate;
            info.PunishCtx = PunishCtx;
            info.Url = Url;
            info.GrantName = GrantName;
            info.IsShow = isShow;
            return info;
        }
        public static CorpPunish GenCorpPunish(string CorpId, string DocNo, string PunishType, string GrantUnit, string DocDate, string PunishCtx, string Url, string isShow)
        {
            CorpPunish info = new CorpPunish();
            info.Id = ToolDb.NewGuid;
            info.CorpId = CorpId;
            info.DocNo = DocNo;
            info.PunishType = PunishType;
            info.GrantUnit = GrantUnit;
            info.DocDate = DocDate;
            info.PunishCtx = PunishCtx;
            info.Url = Url;
            info.GrantName = string.Empty;
            info.IsShow = isShow;
            return info;
        }


        /// <summary>
        /// 企业诚信阶段得分
        /// </summary>
        /// <param name="CorpName"></param>
        /// <param name="CorpType"></param>
        /// <param name="CorpRank"></param>
        /// <param name="CorpCategory"></param>
        /// <param name="Ranking"></param>
        /// <param name="CategoryRank"></param>
        /// <param name="CalcuBeginDate"></param>
        /// <param name="CalcuEndDate"></param>
        /// <param name="RealScore"></param>
        /// <param name="Province"></param>
        /// <param name="City"></param>
        /// <param name="CreateTime"></param>
        /// <param name="InfoSource"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static CorpCreditjd GenCorpCreditJD(string CorpName, string CorpType, string CorpRank, string CorpCategory, string Ranking, string CategoryRank, string CalcuBeginDate, string CalcuEndDate, string RealScore, string Province, string City, string InfoSource, string Url, string bidhtl, string avgScore, string goodScore, string badScore)
        {
            CorpCreditjd info = new CorpCreditjd();
            info.Id = ToolDb.NewGuid;
            info.CorpType = CorpType;
            info.CorpName = CorpName.GetPrjNameByName();
            info.CorpRank = CorpRank;
            info.CorpCategory = CorpCategory;
            try
            {
                info.Ranking = int.Parse(Ranking);
            }
            catch { }
            try
            {
                info.CategoryRank = int.Parse(CategoryRank);
            }
            catch { }
            try
            {
                info.CalcuBeginDate = DateTime.Parse(CalcuBeginDate);
            }
            catch { }
            try
            {
                info.CalcuEndDate = DateTime.Parse(CalcuEndDate);
            }
            catch { }
            try
            {
                info.RealScore = Convert.ToDecimal(RealScore);
            }
            catch { }
            info.Province = Province;
            info.City = City;
            try
            {
                info.CreateTime = DateTime.Now;
            }
            catch { }
            try
            {
                info.AvgScore = decimal.Parse(avgScore);
            }
            catch { }
            try
            {
                info.BadScore = decimal.Parse(badScore);
            }
            catch { }
            try
            {
                info.GoodScore = decimal.Parse(goodScore);
            }
            catch { }
            info.InfoSource = InfoSource;
            info.Url = Url;
            info.BidHtml = bidhtl;
            info.IsNew = "1";
            return info;
        }

        /// <summary>
        /// 企业诚信实时得分
        /// </summary>
        /// <param name="CorpName"></param>
        /// <param name="CorpType"></param>
        /// <param name="CorpCategory"></param>
        /// <param name="Ranking"></param>
        /// <param name="CategoryRank"></param>
        /// <param name="QualityScore"></param>
        /// <param name="BuildScore"></param>
        /// <param name="MarketScore"></param>
        /// <param name="ProjectScore"></param>
        /// <param name="WaterScore"></param>
        /// <param name="MunicipalScore"></param>
        /// <param name="CalcuDate"></param>
        /// <param name="RealScore"></param>
        /// <param name="Province"></param>
        /// <param name="City"></param>
        /// <param name="CreateTime"></param>
        /// <param name="InfoSource"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static CorpCreditss GenCorpCreditSS(string CorpName, string CorpType, string CorpCategory, int? Ranking, int? CategoryRank, decimal? QualityScore, decimal? BuildScore, decimal? MarketScore, decimal? ProjectScore, decimal? WaterScore, decimal? MunicipalScore, DateTime? CalcuDate, decimal? RealScore, string Province, string City, DateTime? CreateTime, string InfoSource, string Url)
        {
            CorpCreditss info = new CorpCreditss();
            info.Id = ToolDb.NewGuid;
            info.CorpName = CorpName.GetPrjNameByName();
            info.CorpType = CorpType;
            info.CorpCategory = CorpCategory;
            info.Ranking = Ranking;

            info.CategoryRank = CategoryRank;
            info.QualityScore = QualityScore;
            info.BuildScore = BuildScore;
            info.MarketScore = MarketScore;
            info.ProjectScore = ProjectScore;
            info.WaterScore = WaterScore;
            info.MunicipalScore = MunicipalScore;
            info.CalcuDate = CalcuDate;
            info.RealScore = RealScore;
            info.Province = Province;
            info.City = City;
            info.CreateTime = CreateTime;
            info.InfoSource = InfoSource;
            info.Url = Url;
            return info;
        }

        /// <summary>
        /// 专家库
        /// </summary>
        /// <param name="eId"></param>
        /// <param name="eExpertName"></param>
        /// <param name="eWorkunit"></param>
        /// <param name="eProfession"></param>
        /// <param name="eRemark"></param>
        /// <param name="eInfoUrl"></param>
        /// <param name="eCreator"></param>
        /// <param name="eCreateTime"></param>
        /// <param name="eLastModifier"></param>
        /// <param name="eLastModiftime"></param>
        /// <returns></returns>
        public static ExpertInfo GenExpertInfo(string eExpertName, string eWorkunit, string eProfession, string eRemark,
            string eInfoUrl, string eCreator, string eCreateTime, string eLastModifier, string eLastModiftime)
        {
            ExpertInfo info = new ExpertInfo();
            info.Id = ToolDb.NewGuid;
            info.ExpertName = eExpertName.GetPrjNameByName() ?? string.Empty;
            info.WorkUnit = eWorkunit ?? string.Empty;
            info.Profession = eProfession ?? string.Empty;
            info.Remark = eRemark ?? string.Empty;
            info.InfoUrl = eInfoUrl;
            info.Creator = info.LastModifier = ToolDb.EmptyGuid;
            info.CreateTime = info.LastModifTime = DateTime.Now;
            return info;
        }

        /// <summary>
        /// 专家评标工程
        /// </summary>
        /// <param name="bProv"></param>
        /// <param name="bCity"></param>
        /// <param name="bArea"></param>
        /// <param name="bPrjno"></param>
        /// <param name="bPrjname"></param>
        /// <param name="bExpertendtime"></param>
        /// <param name="bBidresultendtime"></param>
        /// <param name="bBaseprice"></param>
        /// <param name="bBiddate"></param>
        /// <param name="bBuildunit"></param>
        /// <param name="bBidmethod"></param>
        /// <param name="bRemark"></param>
        /// <param name="bPrjstate"></param>
        /// <param name="bInfourl"></param>
        /// <param name="bCreator"></param>
        /// <param name="bCreatetime"></param>
        /// <param name="bLastmodifier"></param>
        /// <param name="bLastmodifytime"></param>
        /// <returns></returns>
        public static BidProject GenExpertProject(string bProv, string bCity, string bArea, string bPrjno, string bPrjname,
            string bExpertendtime, string bBaseprice, string bBiddate, string bBuildunit, string bBidmethod, string bRemark,
             string bInfourl)
        {
            BidProject info = new BidProject();
            info.Id = ToolDb.NewGuid;
            info.Prov = bProv ?? string.Empty;
            info.City = bCity ?? string.Empty;
            info.Area = bArea ?? string.Empty;
            info.PrjNo = bPrjno ?? string.Empty;
            info.PrjName = bPrjname.GetPrjNameByName() ?? string.Empty;
            info.ExpertEndTime = bExpertendtime;
            try
            {
                info.BasePrice = decimal.Parse(bBaseprice);
            }
            catch { info.BasePrice = 0; }
            try
            {
                info.BidDate = DateTime.Parse(bBiddate);
            }
            catch { info.BidDate = null; }
            info.BuildUnit = bBuildunit ?? string.Empty;
            info.BidMethod = bBidmethod ?? string.Empty;
            info.Remark = bRemark ?? string.Empty;
            info.PrjState = "1";
            info.InfoUrl = bInfourl ?? string.Empty;
            info.Creator = info.LastModifier = ToolDb.EmptyGuid;
            info.CreateTime = info.LastModifyTime = DateTime.Now;
            return info;
        }

        /// <summary>
        /// 评标专家
        /// </summary>
        /// <param name="bId"></param>
        /// <param name="bBidProjectId"></param>
        /// <param name="bExpertname"></param>
        /// <param name="bBidtype"></param>
        /// <param name="bExpertspec"></param>
        /// <param name="bExpertunit"></param>
        /// <param name="bRemark"></param>
        /// <param name="bInfourl"></param>
        /// <param name="bCreator"></param>
        /// <param name="bCreatetime"></param>
        /// <param name="bLastmodifier"></param>
        /// <param name="bLastmodifytime"></param>
        /// <returns></returns>
        public static BidProjectExpert GenProjectExpert(string bBidProjectId, string bExpertname, string bBidtype,
            string bExpertspec, string bExpertunit,
            string bRemark, string bInfourl)
        {
            BidProjectExpert info = new BidProjectExpert();
            info.Id = ToolDb.NewGuid;
            info.BidProjectId = bBidProjectId ?? string.Empty;
            info.ExpertName = bExpertname.GetPrjNameByName() ?? string.Empty;
            info.BidType = bBidtype ?? string.Empty;
            info.ExpertSpec = bExpertspec ?? string.Empty;
            info.ExpertUnit = bExpertunit ?? string.Empty;
            info.Remark = bRemark ?? string.Empty;
            info.InfoUrl = bInfourl ?? string.Empty;
            info.CreateTime = info.LastModifyTime = DateTime.Now;
            info.Creator = info.LastModifier = ToolDb.EmptyGuid;
            return info;
        }

        /// <summary>
        /// 评标工程结果
        /// </summary>
        /// <param name="bProv"></param>
        /// <param name="bCity"></param>
        /// <param name="bArea"></param>
        /// <param name="bPrjno"></param>
        /// <param name="bPrjname"></param>
        /// <param name="bExpertendtime"></param>
        /// <param name="bBidresultendtime"></param>
        /// <param name="bBaseprice"></param>
        /// <param name="bBiddate"></param>
        /// <param name="bBuildunit"></param>
        /// <param name="bBidmethod"></param>
        /// <param name="bRemark"></param>
        /// <param name="bPrjstate"></param>
        /// <param name="bInfourl"></param>
        /// <param name="bCreator"></param>
        /// <param name="bCreatetime"></param>
        /// <param name="bLastmodifier"></param>
        /// <param name="bLastmodifytime"></param>
        /// <returns></returns>
        public static BidProject GenResultProject(string bProv, string bCity, string bArea, string bPrjno, string bPrjname,
             string bBidresultendtime, string bBaseprice, string bBiddate, string bBuildunit, string bBidmethod, string bRemark,
             string bInfourl)
        {
            BidProject info = new BidProject();
            info.Id = ToolDb.NewGuid;
            info.Prov = bProv ?? string.Empty;
            info.City = bCity ?? string.Empty;
            info.Area = bArea ?? string.Empty;
            info.PrjNo = bPrjno ?? string.Empty;
            info.PrjName = bPrjname.GetPrjNameByName() ?? string.Empty;
            try
            {
                info.BasePrice = decimal.Parse(bBaseprice);
            }
            catch { info.BasePrice = 0; }
            info.BidResultEndTime = bBidresultendtime;
            try
            {
                info.BidDate = DateTime.Parse(bBiddate);
            }
            catch { info.BidDate = null; }
            info.BuildUnit = bBuildunit ?? string.Empty;
            info.BidMethod = bBidmethod ?? string.Empty;
            info.Remark = bRemark ?? string.Empty;
            info.PrjState = "1";
            info.InfoUrl = bInfourl ?? string.Empty;
            info.Creator = info.LastModifier = ToolDb.EmptyGuid;
            info.CreateTime = info.LastModifyTime = DateTime.Now;
            return info;
        }

        /// <summary>
        /// 获取立项信息
        /// </summary>
        /// <param name="prov"></param>
        /// <param name="city"></param>
        /// <param name="area"></param>
        /// <param name="itemCode"></param>
        /// <param name="itemName"></param>
        /// <param name="itemAddress"></param>
        /// <param name="buildUnit"></param>
        /// <param name="buildNature"></param>
        /// <param name="totalInvest"></param>
        /// <param name="planInvest"></param>
        /// <param name="issuedPlan"></param>
        /// <param name="investSource"></param>
        /// <param name="approvalUnit"></param>
        /// <param name="approvalDate"></param>
        /// <param name="approvalCode"></param>
        /// <param name="msgUnit"></param>
        /// <param name="planDate"></param>
        /// <param name="planType"></param>
        /// <param name="planBeginDate"></param>
        /// <param name="planEndDate"></param>
        /// <param name="ctxHtml"></param>
        /// <param name="itemCtx"></param>
        /// <param name="itemContent"></param>
        /// <param name="msgType"></param>
        /// <param name="infoUrl"></param>
        /// <returns></returns>
        public static ItemPlan GenItemPlan(string prov, string city, string area, string itemCode, string itemName, string itemAddress, string buildUnit, string buildNature, string totalInvest, string planInvest, string issuedPlan, string investSource, string approvalUnit, string approvalDate, string approvalCode, string msgUnit, string planDate, string planType, string planBeginDate, string planEndDate, string ctxHtml, string itemCtx, string itemContent, string msgType, string infoUrl)
        {
            ItemPlan info = new ItemPlan();
            info.Id = ToolDb.NewGuid;
            info.CreateTime = info.LastModifyTime = DateTime.Now;
            info.Creator = info.LastModifier = ToolDb.EmptyGuid;
            info.Prov = prov;
            info.City = city;
            info.Area = area;
            info.ItemCode = itemCode;
            info.ItemName = itemName;
            info.ItemAddress = itemAddress;
            info.BuildUnit = buildUnit;
            info.BuildNature = buildNature;
            info.InvestSource = investSource;
            info.ApprovalUnit = approvalUnit;
            info.ApprovalCode = approvalCode;
            info.MsgUnit = msgUnit;
            info.PlanType = planType;
            info.PlanBeginDate = planBeginDate;
            info.PlanEndDate = planEndDate;
            info.CtxHtml = ctxHtml;
            info.ItemCtx = itemCtx;
            info.ItemContent = itemContent;
            info.MsgType = msgType;
            info.InfoUrl = infoUrl;
            if (!string.IsNullOrEmpty(approvalDate))
            {
                try
                {
                    info.ApprovalDate = DateTime.Parse(approvalDate);
                }
                catch { }
            }
            if (!string.IsNullOrEmpty(planDate))
            {
                try
                {
                    info.PlanDate = DateTime.Parse(planDate);
                }
                catch { }
            }
            if (!string.IsNullOrEmpty(totalInvest))
            {
                try
                {
                    info.TotalInvest = decimal.Parse(totalInvest);
                }
                catch { }
            }
            if (!string.IsNullOrEmpty(planInvest))
            {
                try
                {
                    info.PlanInvest = decimal.Parse(planInvest);
                }
                catch { }
            }
            if (!string.IsNullOrEmpty(issuedPlan))
            {
                try
                {
                    info.IssuedPlan = decimal.Parse(issuedPlan);
                }
                catch { }
            }

            return info;
        }

        public static BidSituation GetBidSituation(string prov, string city, string area, string code, string prjname, string enddate, string msgtype, string infourl, string ctx, string html, string beginDate)
        {
            BidSituation info = new BidSituation();
            info.Id = NewGuid;
            info.Prov = prov;
            info.City = city;
            info.Area = area;
            info.Code = code;
            info.ProjectName = prjname;
            try
            {
                info.PublicityEndDate = Convert.ToDateTime(enddate);
            }
            catch { }
            if (!string.IsNullOrEmpty(beginDate))
            {
                try
                {
                    info.BeginDate = DateTime.Parse(beginDate);
                }
                catch { }
            }
            info.MsgType = msgtype;
            info.CreateTime = info.LastModifierTime = DateTime.Now;
            info.Creator = info.LastModifier = EmptyGuid;
            info.Ctx = ctx;
            info.Html = html;
            info.InfoUrl = infourl;
            return info;
        }

        public static ProjectResult GetProjectResult(string prov, string city, string area, string code, string prjname, string BuildUnit, string FinalistsWay, string RevStaMethod, string SetStaMethod, string VoteMethod, string RevStaDate, string infourl, string msgtype, string ctx, string html,string beginDate)
        {
            ProjectResult info = new ProjectResult();
            info.Id = NewGuid;
            info.Prov = prov;
            info.City = city;
            info.Area = area;
            info.Code = code;
            info.ProjectName = prjname;
            info.BuildUnit = BuildUnit;
            info.FinalistsWay = FinalistsWay;
            info.RevStaMethod = RevStaMethod;
            info.SetStaMethod = SetStaMethod;
            info.VoteMethod = VoteMethod;
            try
            {
                info.RevStaDate = Convert.ToDateTime(RevStaDate);
            }
            catch { }
            if (!string.IsNullOrEmpty(beginDate))
            {
                try
                {
                    info.BeginDate = DateTime.Parse(beginDate);
                }
                catch { }
            }
            info.InfoUrl = infourl;
            info.MsgType = msgtype;
            info.Ctx = ctx;
            info.Html = html;
            info.CreateTime = info.LastModifierTime = DateTime.Now;
            info.Creator = info.LastModifier = EmptyGuid;
            return info;
        }
        /// <summary>
        /// 生成企业行为记录
        /// </summary>
        /// <param name="prov"></param>
        /// <param name="city"></param>
        /// <param name="area"></param>
        /// <param name="corpId"></param>
        /// <param name="RecordCode"></param>
        /// <param name="RecordName"></param>
        /// <param name="RecordInfo"></param>
        /// <param name="ImplUnit"></param>
        /// <param name="BeginDate"></param>
        /// <param name="IsGood"></param>
        /// <param name="InfoUrl"></param>
        /// <returns></returns>
        public static CorpPrompt GetCorpPrompt(string prov,string city,string area,string corpId,string RecordCode,string RecordName,string RecordInfo,string ImplUnit,string BeginDate,bool IsGood,string InfoUrl)
        {
            CorpPrompt info = new CorpPrompt();
            info.Id = NewGuid;
            info.Prov = prov;
            info.City = city;
            info.Area = area;
            info.CorpId = corpId;
            info.RecordCode = RecordCode;
            info.RecordName = RecordName;
            info.RecordInfo = RecordInfo;
            info.ImplUnit = ImplUnit;
            try
            {
                info.BeginDate = DateTime.Parse(BeginDate);
            }
            catch { }
            info.IsGood = IsGood;
            info.InfoUrl = InfoUrl;
            info.CreateTime = DateTime.Now;
            return info;
        }
        public static ProjectResultDtl GetProjectResultDtl(string SourceId, string UnitName, string BidDate, string IsBid, string Ranking, string WinNumber, string TicketNumber)
        {
            ProjectResultDtl info = new ProjectResultDtl();
            info.Id = NewGuid;
            info.SourceId = SourceId;
            info.UnitName = UnitName;
            try
            {
                info.BidDate = Convert.ToDateTime(BidDate);
            }
            catch { }
            info.IsBid = IsBid == "1";
            try
            {
                info.Ranking = int.Parse(Ranking);
            }
            catch { }
            try
            {
                info.WinNumber = int.Parse(WinNumber);
            }
            catch { }
            try
            {
                info.TicketNumber = int.Parse(TicketNumber);
            }
            catch { }
            return info;
        }
        /// <summary>
        ///  获取中标信息实体
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static IList GetList(string sql)
        {
            DataTable dt = ToolCoreDb.GetDbData(sql);
            IList list = new ArrayList();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(DataRowToModel(row));
                }
            }
            return list;
        }

        public static List<BidInfo> GetBidInfoList(string sql)
        {
            DataTable dt = ToolCoreDb.GetDbData(sql);
            List<BidInfo> list = new List<BidInfo>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(DataRowToModel(row));
                }
            }
            return list;
        }
        private static BidInfo DataRowToModel(DataRow row)
        {
            BidInfo model = new BidInfo();
            if (row != null)
            {
                if (row["Id"] != null)
                {
                    model.Id = row["Id"].ToString();
                }
                if (row["Code"] != null)
                {
                    model.Code = row["Code"].ToString();
                }
                if (row["ProjectName"] != null)
                {
                    model.ProjectName = row["ProjectName"].ToString();
                }
                if (row["BidUnit"] != null)
                {
                    model.BidUnit = row["BidUnit"].ToString();
                }
                if (row["BeginDate"] != null && row["BeginDate"].ToString() != "")
                {
                    model.BeginDate = DateTime.Parse(row["BeginDate"].ToString());
                }
                if (row["LastModifyTime"] != null && row["LastModifyTime"].ToString() != "")
                {
                    model.LastModifyTime = DateTime.Parse(row["LastModifyTime"].ToString());
                }
                if (row["BidMoney"] != null && row["BidMoney"].ToString() != "")
                {
                    model.BidMoney = decimal.Parse(row["BidMoney"].ToString());
                }
                if (row["IsStatistical"] != null)
                {
                    model.IsStatistical = row["IsStatistical"].ToString();
                }
                //if (row["Prov"] != null)
                //{
                //    model.Prov = row["Prov"].ToString();
                //}
                //if (row["City"] != null)
                //{
                //    model.City = row["City"].ToString();
                //}
                //if (row["Area"] != null)
                //{
                //    model.Area = row["Area"].ToString();
                //}
                //if (row["Road"] != null)
                //{
                //    model.Road = row["Road"].ToString();
                //}
                //if (row["BuildUnit"] != null)
                //{
                //    model.BuildUnit = row["BuildUnit"].ToString();
                //}
                //if (row["BidDate"] != null && row["BidDate"].ToString() != "")
                //{
                //    model.BidDate = DateTime.Parse(row["BidDate"].ToString());
                //}

                //if (row["EndDate"] != null && row["EndDate"].ToString() != "")
                //{
                //    model.EndDate = DateTime.Parse(row["EndDate"].ToString());
                //}
                //if (row["BidCtx"] != null)
                //{
                //    model.BidCtx = row["BidCtx"].ToString();
                //}
                //if (row["Creator"] != null)
                //{
                //    model.Creator = row["Creator"].ToString();
                //}
                //if (row["CreateTime"] != null && row["CreateTime"].ToString() != "")
                //{
                //    model.CreateTime = DateTime.Parse(row["CreateTime"].ToString());
                //}
                //if (row["LastModifier"] != null)
                //{
                //    model.LastModifier = row["LastModifier"].ToString();
                //}

                //if (row["Remark"] != null)
                //{
                //    model.Remark = row["Remark"].ToString();
                //}
                //if (row["MsgType"] != null)
                //{
                //    model.MsgType = row["MsgType"].ToString();
                //}
                //if (row["BidType"] != null)
                //{
                //    model.BidType = row["BidType"].ToString();
                //}
                //if (row["SpeType"] != null)
                //{
                //    model.SpeType = row["SpeType"].ToString();
                //}
                //if (row["OtherType"] != null)
                //{
                //    model.OtherType = row["OtherType"].ToString();
                //}

                //if (row["InfoUrl"] != null)
                //{
                //    model.InfoUrl = row["InfoUrl"].ToString();
                //}
                //if (row["PrjMgr"] != null)
                //{
                //    model.PrjMgr = row["PrjMgr"].ToString();
                //}
                //if (row["CtxHtml"] != null)
                //{
                //    model.CtxHtml = row["CtxHtml"].ToString();
                //}
                //if (row["AgentUnit"] != null)
                //{
                //    model.AgentUnit = row["AgentUnit"].ToString();
                //} 
            }
            return model;
        }
        /// <summary>
        /// 生产MD5
        /// </summary>
        /// <param name="sDataIn"></param>
        /// <returns></returns>
        public static string GetMD5(string sDataIn)
        {
            try
            {
                if (sDataIn == null)
                    sDataIn = "";
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] bytValue, bytHash;
                bytValue = System.Text.Encoding.UTF8.GetBytes(sDataIn);
                bytHash = md5.ComputeHash(bytValue);
                md5.Clear();
                string sTemp = "";
                for (int i = 0; i < bytHash.Length; i++)
                {
                    sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
                }
                return sTemp.ToUpper();
            }
            catch {
                return string.Empty;
            }
        }


    }
}
