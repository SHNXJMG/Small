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
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Data;
using System.Linq;

namespace Crawler.Instance
{
    public class ProjectResultSzJyzxDbjg : WebSiteCrawller
    {
        public ProjectResultSzJyzxDbjg()
            : base()
        {
            this.Group = "开标定标";
            this.Title = "深圳市交易中心定标结果公示(2015版)";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市交易中心定标结果公示(2015版)";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw/queryDBJieGuoList.do?page=1&rows=";
            this.MaxCount = 150;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ProjectResult>();
            int sqlCount = 0;
            string html = string.Empty;
            List<Dictionary<string, object>> dicFile = new List<Dictionary<string, object>>();
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + (MaxCount + 20));
            }
            catch { return null; }
            int startIndex = html.IndexOf("{");
            int endIndex = html.LastIndexOf("}");
            html = html.Substring(startIndex, (endIndex + 1) - startIndex);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
            foreach (KeyValuePair<string, object> obj in smsTypeJson)
            {
                if (obj.Key == "total") continue;
                object[] array = (object[])obj.Value;
                 
                foreach (object arrValue in array)
                {
                    string Code = string.Empty, prjName = string.Empty, BuildUnit = string.Empty, FinalistsWay = string.Empty, RevStaMethod = string.Empty, SetStaMethod = string.Empty, VoteMethod = string.Empty, RevStaDate = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty, ProjectCtx = string.Empty, HtmlTxt = string.Empty, beginDate = string.Empty, attachFileGroupGuid = string.Empty, dbJieGuoGuid = string.Empty, ggGuid = string.Empty, bdGuid = string.Empty, gcLeiXing = string.Empty, zbrName = string.Empty, zhongBiaoJia = string.Empty, jsonHtml = string.Empty;
                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    Code = Convert.ToString(dic["bdBH"]);
                    prjName = Convert.ToString(dic["bdName"]);
                    //if (!prjName.Contains("茅洲河（光明新区）水环境综合整治工程项目(水景观")) continue;

                    beginDate = Convert.ToString(dic["createTime2"]);
                    InfoUrl = Convert.ToString(dic["detailUrl"]);
                    dbJieGuoGuid = Convert.ToString(dic["dbJieGuoGuid"]);
                    ggGuid = Convert.ToString(dic["ggGuid"]);
                    bdGuid = Convert.ToString(dic["bdGuid"]);
                    gcLeiXing = Convert.ToString(dic["gcLeiXing"]);
                    //zbrName = Convert.ToString(dic["zbrName"]);
                    zhongBiaoJia = Convert.ToString(dic["zhongBiaoJia"]).GetMoney();

                    string crawlUrl = "https://www.szjsjy.com.cn:8001/jyw/queryOldDataDetail.do?type=9&id=" + Code;
                    try
                    {
                        jsonHtml = this.ToolWebSite.GetHtmlByUrl(crawlUrl).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                    }
                    catch { }
                    if (!jsonHtml.Contains("<div") || string.IsNullOrEmpty(jsonHtml))
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(jsonHtml))
                            {
                                crawlUrl = "https://www.szjsjy.com.cn:8001/jyw/queryDbJieGuoByGuid.do?guid=" + Convert.ToString(dic["dbJieGuoGuid"]);
                                jsonHtml = this.ToolWebSite.GetHtmlByUrl(crawlUrl).GetJsString().GetReplace("\\t,\\r,\\n,\"").GetReplace(":RMB:", ":");
                            }

                            string strHtml = PrjResultStr(jsonHtml);//.Replace("A,B,C","")
                            JavaScriptSerializer serializerNew = new JavaScriptSerializer();
                            Dictionary<string, object> smsTypeJsonNew = null;
                            try
                            {
                                smsTypeJsonNew = (Dictionary<string, object>)serializer.DeserializeObject(strHtml);
                            }
                            catch
                            {
                                try
                                {
                                    strHtml = PrjResultStr(jsonHtml, true);
                                    smsTypeJsonNew = (Dictionary<string, object>)serializer.DeserializeObject(strHtml);
                                }
                                catch
                                {
                                    try
                                    {
                                        strHtml = GetPrjResultDtl(strHtml);
                                        smsTypeJsonNew = (Dictionary<string, object>)serializer.DeserializeObject(strHtml);
                                    }
                                    catch { }
                                }
                            }
                            string ggBdGuid = string.Empty, dbBanFa = string.Empty, piaoJueBanFa = string.Empty, dbTime = string.Empty, isChouQian = string.Empty,
                                chouQianRuWeiFangShi = string.Empty, rwFangShi = string.Empty, zbName = string.Empty, tongYongZhongBiaoJia = string.Empty,
                                isDuiWaiGongShi = string.Empty, isYiYiTime = string.Empty, Lxr = string.Empty, LxDh = string.Empty, jsDw = string.Empty,
                                                          ggMc = string.Empty, bdBh = string.Empty, ggShiXiangGuid = string.Empty,
                                isHeSuan = string.Empty, gongQi = string.Empty, isTiJiaoDbwy = string.Empty, isXuYaoZuJianDbwyh = string.Empty;

                            Dictionary<string, object> bd = (Dictionary<string, object>)smsTypeJsonNew["bd"];
                            Dictionary<string, object> gc = (Dictionary<string, object>)bd["gc"];
                            try { ggShiXiangGuid = bd["ggShiXiangGuid"].ToString(); }
                            catch { }
                            try { ggMc = bd["bdName"].ToString(); }
                            catch { }
                            try { bdBh = bd["bdBH"].ToString(); }
                            catch { }
                            try { ggBdGuid = smsTypeJsonNew["ggBdGuid"].ToString(); }
                            catch { }
                            try { dbBanFa = smsTypeJsonNew["dbBanFa"].ToString(); }
                            catch
                            {
                                try { dbBanFa = bd["dbBanFa"].ToString(); }
                                catch { }
                            }
                            try { piaoJueBanFa = smsTypeJsonNew["piaoJueBanFa"].ToString(); }
                            catch { }
                            try { dbTime = smsTypeJsonNew["dbTime"].ToString(); dbTime = ToolHtml.GetDateTimeByLong(long.Parse(dbTime)).ToString(); }
                            catch { }
                            try { isChouQian = smsTypeJsonNew["isChouQian"].ToString(); }
                            catch { }
                            try { chouQianRuWeiFangShi = smsTypeJsonNew["chouQianRuWeiFangShi"].ToString(); }
                            catch { }
                            try { rwFangShi = smsTypeJsonNew["rwFangShi"].ToString(); }
                            catch { }
                            try { zbName = smsTypeJsonNew["zbName"].ToString(); }
                            catch { }
                            try { tongYongZhongBiaoJia = smsTypeJsonNew["tongYongZhongBiaoJia"].ToString(); }
                            catch { }
                            try { isDuiWaiGongShi = smsTypeJsonNew["isDuiWaiGongShi"].ToString(); }
                            catch { }
                            try { isYiYiTime = smsTypeJsonNew["isYiYiTime"].ToString(); }
                            catch { }
                            try { isHeSuan = smsTypeJsonNew["isHeSuan"].ToString(); }
                            catch { }
                            try { gongQi = smsTypeJsonNew["gongQi"].ToString(); }
                            catch { }
                            try { isTiJiaoDbwy = smsTypeJsonNew["isTiJiaoDbwy"].ToString(); }
                            catch { }
                            try { isXuYaoZuJianDbwyh = smsTypeJsonNew["isXuYaoZuJianDbwyh"].ToString(); }
                            catch { }
                            try { Lxr = gc["jingBanRenName"].ToString(); }
                            catch { try { Lxr = gc["lianXiRenName"].ToString(); } catch { } }
                            try { LxDh = gc["lianXiRenMobile"].ToString(); }
                            catch { try { LxDh = gc["lianXiRenPhone"].ToString(); } catch { } }
                            try { jsDw = gc["zbRName"].ToString(); }
                            catch { }
                            try { attachFileGroupGuid = smsTypeJsonNew["attachFileGroupGuid"].ToString(); }
                            catch { }
                            if (dbBanFa.IsNumber())
                                dbBanFa = "无";
                            string dtlHtml = string.Empty;
                            string dtlUrl = "https://www.szjsjy.com.cn:8001/jyw/jyw/dbResult_View.do?bdGuid=" + ggGuid;
                            try
                            {
                                dtlHtml = this.ToolWebSite.GetHtmlByUrl(dtlUrl).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                                if (string.IsNullOrEmpty(dtlHtml) || dtlHtml.Length < 10)
                                {
                                    dtlUrl = "https://www.szjsjy.com.cn:8001/jyw/queryPmxtTbrListGs.do?dbGuid=" + ggGuid;
                                    dtlHtml = this.ToolWebSite.GetHtmlByUrl(dtlUrl).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                                }
                            }
                            catch { Logger.Error(prjName); continue; }
                            if (!string.IsNullOrEmpty(dtlHtml) && dtlHtml.Length > 10)
                            {
                                HtmlTxt = dtlHtml;
                                Parser parserNew = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parserNew.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "de_tab1")));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    HtmlTxt = tableNode.AsHtml();
                                    HtmlTxt = HtmlTxt.GetReplace("<td  id=ggName>&nbsp;</td>", "<td  id=\"ggName\">&nbsp;" + ggMc + "</td>");
                                    HtmlTxt = HtmlTxt.GetReplace("<td id=bdBH>&nbsp;</td>", "<td  id=\"bdBH\">&nbsp;" + bdBh + "</td>");
                                    HtmlTxt = HtmlTxt.GetReplace("<td id=bdName>&nbsp;</td>", "<td  id=\"bdName\">&nbsp;" + ggMc + "</td>");
                                    HtmlTxt = HtmlTxt.GetReplace("<td id=zbRName>&nbsp;</td>", "<td  id=\"zbRName\">&nbsp;" + jsDw + "</td>");
                                    HtmlTxt = HtmlTxt.GetReplace("<td id=zbName>&nbsp;</td>", "<td  id=\"zbName\">&nbsp;" + zbName + "</td>");
                                    HtmlTxt = HtmlTxt.GetReplace("<td id=dbTime>&nbsp;</td>", "<td  id=\"dbTime\">&nbsp;" + dbTime + "</td>");
                                    HtmlTxt = HtmlTxt.GetReplace("<td id=rwfs>&nbsp;</td>", "<td  id=\"rwfs\">&nbsp;" + rwFangShi + "</td>");
                                    HtmlTxt = HtmlTxt.GetReplace("<td id=dbBanFa>&nbsp;</td>", "<td  id=\"dbBanFa\">&nbsp;" + dbBanFa + "</td>");
                                    HtmlTxt = HtmlTxt.GetReplace("<td id=lianXiRenName>&nbsp;</td>", "<td  id=\"lianXiRenName\">&nbsp;" + Lxr + "</td>");
                                    HtmlTxt = HtmlTxt.GetReplace("<td id=lianXiRenPhone>&nbsp;</td>", "<td  id=\"lianXiRenName\">&nbsp;" + LxDh + "</td>");

                                    string resultUrl = "https://www.szjsjy.com.cn:8001/jyw/queryTbrListByBdGuidAndGgGuidForGs.do";
                                    string jsonResult = string.Empty;
                                    try
                                    {
                                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "bdGuid", "ggGuid" }, new string[] { bdGuid, ggShiXiangGuid });
                                        jsonResult = this.ToolWebSite.GetHtmlByUrl(resultUrl, nvc).GetJsString().GetReplace("\\t,\\r,\\n,\"");

                                        if (string.IsNullOrEmpty(jsonResult) || jsonResult.Length <= 10)
                                        {
                                            nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "bdGuid", "ggGuid" }, new string[] { bdGuid, ggGuid });
                                            jsonResult = this.ToolWebSite.GetHtmlByUrl(resultUrl, nvc).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                                        }

                                    }
                                    catch (Exception ex) { Logger.Error(prjName); }
                                    if (!string.IsNullOrEmpty(jsonResult) && jsonResult.Length >= 10)
                                    {
                                        string jiHua_LiXiang_BH = string.Empty, jiHua_LiXiang_BH2 = string.Empty;
                                        try
                                        {
                                            Dictionary<string, object> xm = (Dictionary<string, object>)bd["xm"];
                                            jiHua_LiXiang_BH = xm["jiHua_LiXiang_BH"].ToString().GetReplace("【", "[").GetReplace("】", "]");
                                            jiHua_LiXiang_BH2 = xm["jiHua_LiXiang_BH"].ToString();
                                        }
                                        catch { }
                                        string tempJson = jsonResult;
                                        if (!string.IsNullOrEmpty(jiHua_LiXiang_BH))
                                            tempJson = jsonResult.Replace(jiHua_LiXiang_BH, jiHua_LiXiang_BH2);
                                        string dtlTbName = PrjResultStr(tempJson, true);
                                        JavaScriptSerializer serializerDtl = new JavaScriptSerializer();
                                        object[] dtlObj = null;
                                        try
                                        {
                                            dtlObj = (object[])serializerDtl.DeserializeObject(dtlTbName);
                                        }
                                        catch
                                        {
                                            try
                                            {
                                                dtlTbName = dtlTbName.Substring(0, dtlTbName.Length - 2);
                                                dtlTbName += "}}]";
                                                dtlObj = (object[])serializerDtl.DeserializeObject(dtlTbName);
                                            }
                                            catch
                                            {
                                                try
                                                {
                                                    dtlTbName = dtlTbName.Trim().Replace("},{", "}},{");
                                                    dtlObj = (object[])serializerDtl.DeserializeObject(dtlTbName);
                                                }
                                                catch
                                                {
                                                    Logger.Error(prjName);
                                                }
                                            }
                                        }

                                        bool isOk = false;

                                        StringBuilder sb = new StringBuilder();
                                        if (dbBanFa.Contains("逐轮票决"))
                                        {
                                            StringBuilder strZlpj = new StringBuilder();
                                           
                                            List<PrjResult> prjResluts = LPrjResult.GetPrjZlResult(dtlObj);
                                            IEnumerable<IGrouping<int, PrjResult>> ienums = prjResluts.GroupBy(x => x.lunCiXuHao).OrderBy(x => x.Key);

                                            foreach(IGrouping<int,PrjResult> groups in ienums)
                                            {
                                                strZlpj.AppendFormat("<h3>第{0}大轮投票表</h3>", groups.Key);
                                                strZlpj.Append("<table width='100%' border='0' class='de_tab2'>");
                                                strZlpj.Append("<tr>");
                                                strZlpj.Append("<th style='text-align: left' class='bg_tdtop'>编号</th>");
                                                strZlpj.Append("<th style='text-align: left' class='bg_tdtop'>投标单位</th>");
                                                strZlpj.Append("<th style='text-align: left' class='bg_tdtop'>得票数</th>");
                                                strZlpj.Append("<th style='text-align: left' class='bg_tdtop'>排名</th>");
                                                strZlpj.Append("</tr>");

                                                List<PrjResult> results = groups.ToList().OrderBy(x => x.Bh).ToList();

                                                foreach(PrjResult prj in results)
                                                {
                                                    strZlpj.Append("<tr>");
                                                    strZlpj.Append("<th style='padding: 0px'>" + prj.Bh + "</th>");
                                                    strZlpj.Append("<th style='padding: 0px' class='bg_tdtop'>" + prj.Name + "</th>");
                                                    strZlpj.Append("<th style='padding: 0px' class='bg_tdtop'>" + prj.Mc + "</th>");
                                                    strZlpj.Append("<th style='padding: 0px' class='bg_tdtop'>" + prj.Xh + "</th>");
                                                    strZlpj.Append("</tr>");
                                                    isOk = true;
                                                }

                                                strZlpj.Append("</table>");
                                            }
                                            sb.Append(strZlpj.ToString());
                                        }
                                        else
                                        {
                                            StringBuilder strTmp = new StringBuilder();
                                            strTmp.Append("<table width='100%' border='0' class='de_tab2'>");

                                            
                                            switch (dbBanFa)
                                            {
                                                case "直接票决":
                                                    strTmp.Append("<tr>");
                                                    strTmp.Append("<th style='text-align: left' class='bg_tdtop'>编号</th>");
                                                    strTmp.Append("<th style='text-align: left' class='bg_tdtop'>投标单位</th>");
                                                    strTmp.Append("<th style='text-align: left' class='bg_tdtop'>取胜次数</th>");
                                                    strTmp.Append("<th style='text-align: left' class='bg_tdtop'>排名</th>");
                                                    strTmp.Append("</tr>");
                                                    List<PrjResult> PrjResults = LPrjResult.GetPrjResult(dtlObj);
                                                    foreach (PrjResult prj in PrjResults)
                                                    {
                                                        strTmp.Append("<tr>");
                                                        strTmp.Append("<th style='padding: 0px'>" + prj.Bh + "</th>");
                                                        strTmp.Append("<th style='padding: 0px' class='bg_tdtop'>" + prj.Name + "</th>");
                                                        strTmp.Append("<th style='padding: 0px' class='bg_tdtop'>" + prj.Mc + "</th>");
                                                        strTmp.Append("<th style='padding: 0px' class='bg_tdtop'>" + prj.Xh + "</th>");
                                                        strTmp.Append("</tr>");
                                                        isOk = true;
                                                    }
                                                    break;

                                                default:
                                                    strTmp.Append("<tr>");
                                                    strTmp.Append("<th style='text-align: left' class='bg_tdtop'>序号</th>");
                                                    strTmp.Append("<th style='text-align: left' class='bg_tdtop'>企业名称</th>");
                                                    strTmp.Append("<th style='text-align: left' class='bg_tdtop'>投标时间</th>");
                                                    strTmp.Append("<th style='text-align: left' class='bg_tdtop'>中标候选人</th>");
                                                    strTmp.Append("</tr>");
                                                    List<PrjResult> PrjResultBid = LPrjResult.GetPrjResultBid(dtlObj);
                                                    foreach (PrjResult prj in PrjResultBid)
                                                    {
                                                        strTmp.Append("<tr>");
                                                        strTmp.Append("<th style='padding: 0px'>" + prj.Xh + "</th>");
                                                        strTmp.Append("<th style='padding: 0px' class='bg_tdtop'>" + prj.Name + "</th>");
                                                        strTmp.Append("<th style='padding: 0px' class='bg_tdtop'>" + prj.Date + "</th>");
                                                        strTmp.Append("<th style='padding: 0px' class='bg_tdtop'>" + prj.IsBid + "</th>");
                                                        strTmp.Append("</tr>");
                                                        isOk = true;
                                                    }
                                                    break;
                                            }
                                            strTmp.Append("</table>");

                                            sb.Append(strTmp.ToString());
                                        }
                                        
                                       
                                        if (isOk)
                                            HtmlTxt += sb;
                                    }
                                }
                            }

                        }
                        catch { }
                        if (!string.IsNullOrEmpty(attachFileGroupGuid))
                        {
                            bool FileOk = false;
                            StringBuilder sb = new StringBuilder();
                            try
                            {
                                sb.Append("<table id=\"wenJian_List\" width=\"100%\" border=\"0\" class=\"de_tab2\">");
                                sb.Append("<tr>");
                                sb.Append("<td class=\"bg_tdtop\">序号</td>");
                                sb.Append("<td class=\"bg_tdtop\" >文件名</td>");
                                sb.Append("<td class=\"bg_tdtop\">创建时间</td>");
                                sb.Append("</tr>");
                                string url = "https://www.szjsjy.com.cn:8001/jyw/filegroup/queryByGroupGuidZS.do?groupGuid=" + attachFileGroupGuid;
                                string attachHtml = this.ToolWebSite.GetHtmlByUrl(url);
                                JavaScriptSerializer newSerializer = new JavaScriptSerializer();
                                Dictionary<string, object> newTypeJson = (Dictionary<string, object>)newSerializer.DeserializeObject(attachHtml);
                                string attachGuid = string.Empty, attachName = string.Empty, createTime = string.Empty;
                                foreach (KeyValuePair<string, object> newObj in newTypeJson)
                                {
                                    object[] newArray = (object[])newObj.Value;
                                    int row = 1;
                                    foreach (object newArr in newArray)
                                    {
                                        Dictionary<string, object> newDic = (Dictionary<string, object>)newArr;
                                        try
                                        {
                                            dicFile.Add(newDic);
                                            attachGuid = Convert.ToString(newDic["attachGuid"]);
                                            attachName = Convert.ToString(newDic["attachName"]);
                                            createTime = Convert.ToString(newDic["createTime"]);
                                            if (!string.IsNullOrEmpty(createTime))
                                                createTime = ToolHtml.GetDateTimeByLong(long.Parse(createTime)).ToString();
                                            string newUrl = "https://www.szjsjy.com.cn:8001/file/downloadFile?fileId=" + attachGuid;
                                            string aTag = "<a href='" + newUrl + "'  target='_blank'>" + attachName + "</a>";
                                            sb.Append("<tr>");
                                            sb.Append("<td>" + row + "</td>");
                                            sb.Append("<td>" + aTag + "</td>");
                                            sb.Append("<td>" + createTime + "</td>");
                                            sb.Append("</tr>");
                                            row++;
                                            FileOk = true;
                                        }
                                        catch { }
                                    }
                                }
                                sb.Append("</table>");
                            }
                            catch { }
                            if (FileOk)
                                HtmlTxt += sb.ToString();
                        }
                    }
                    else
                    {
                        HtmlTxt = jsonHtml;
                        Parser parserA = new Parser(new Lexer(HtmlTxt));
                        NodeList aNode = parserA.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        if (aNode != null && aNode.Count > 0)
                        {
                            for (int i = 0; i < aNode.Count; i++)
                            {
                                ATag aTag = aNode[i] as ATag;
                                if (aTag.IsAtagAttach())
                                {
                                    Dictionary<string, object> fileDic = new Dictionary<string, object>();
                                    fileDic.Add("attachGuid", aTag.Link.GetReplace("\\"));
                                    fileDic.Add("attachName", aTag.LinkText.ToNodeString());
                                    dicFile.Add(fileDic);
                                }
                            }
                        }
                    }
                    ProjectCtx = HtmlTxt.GetReplace("<br />,<br/>,</ br>,</br>", "\r\n").ToCtxString() + "\r\n";
                    Parser parser = new Parser(new Lexer(HtmlTxt.GetReplace("th", "td")));
                    NodeList ctxNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                    if (ctxNode != null && ctxNode.Count > 0)
                    {
                        string dtlCtx = string.Empty;
                        TableTag ctxTable = ctxNode[0] as TableTag;
                        for (int d = 0; d < ctxTable.RowCount; d++)
                        {
                            for (int k = 0; k < ctxTable.Rows[d].ColumnCount; k++)
                            {
                                if ((k + 1) % 2 == 0)
                                    dtlCtx += ctxTable.Rows[d].Columns[k].ToNodePlainString() + "\r\n";
                                else
                                    dtlCtx += ctxTable.Rows[d].Columns[k].ToNodePlainString() + "：";
                            }
                        }
                        BuildUnit = dtlCtx.GetRegex("建设单位");
                        FinalistsWay = dtlCtx.GetRegex("入围方式");
                        RevStaMethod = dtlCtx.GetRegex("评标方法");
                        SetStaMethod = dtlCtx.GetRegex("定标方法");
                        VoteMethod = dtlCtx.GetRegex("票决方法");
                        RevStaDate = dtlCtx.GetRegex("定标时间").GetDateRegex("yyyy/MM/dd");

                        if (!SetStaMethod.IsChina())
                            SetStaMethod = "";
                    }
                    MsgType = "深圳市建设工程交易中心";

                    sqlCount++;
                    if (!crawlAll && sqlCount >= this.MaxCount) return list;

                    ProjectResult info = ToolDb.GetProjectResult("广东省", "深圳市工程", "", Code, prjName, BuildUnit, FinalistsWay, RevStaMethod, SetStaMethod, VoteMethod, RevStaDate, InfoUrl, MsgType, ProjectCtx, HtmlTxt, beginDate);


                    if (prjName.Contains("深圳广电金融中心施工总承包工程"))
                    {
                        string delSql = string.Format("delete from ProjectResult where InfoUrl='{0}'", info.InfoUrl);
                        ToolDb.ExecuteSql(delSql);
                    }

                    if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                    {
                        if (this.ExistsUpdate)
                        {
                            object id = ToolDb.ExecuteScalar(string.Format("select Id from ProjectResult where InfoUrl='{0}'", info.InfoUrl));
                            if (id != null)
                            {
                                string sql = string.Format("delete from ProjectResultDtl where SourceId='{0}'", id);
                                ToolDb.ExecuteSql(sql);
                                string sqlAttach = string.Format("delete from BaseAttach where SourceId='{0}'", id);
                                ToolDb.ExecuteSql(sqlAttach);
                            }
                        }
                        if (dicFile.Count > 0)
                        {
                            try
                            {
                                foreach (Dictionary<string, object> newDic in dicFile)
                                {
                                    try
                                    {
                                        string attachGuid = Convert.ToString(newDic["attachGuid"]);
                                        string attachName = Convert.ToString(newDic["attachName"]);
                                        string newUrl = string.Empty;
                                        if (attachGuid.ToLower().Contains("http"))
                                            newUrl = attachGuid;
                                        else
                                            newUrl = "https://www.szjsjy.com.cn:8001/file/downloadFile?fileId=" + attachGuid;


                                        BaseAttach attach = ToolHtml.GetBaseAttach(newUrl, attachName, info.Id, "SiteManage\\Files\\Attach\\");
                                        if (attach == null) attach = ToolHtml.GetBaseAttach(newUrl, attachName, info.Id, "SiteManage\\Files\\Attach\\");

                                        if (attach != null)
                                            ToolDb.SaveEntity(attach, string.Empty);
                                    }
                                    catch
                                    {
                                        continue;
                                    }
                                }
                            }
                            catch { }
                        }

                        //if (ctxNode != null && ctxNode.Count > 1)
                        //{
                        //    TableTag prjTable = ctxNode[1] as TableTag;
                        //    string colName1 = prjTable.Rows[0].Columns[2].ToNodePlainString();
                        //    string colName2 = prjTable.Rows[0].Columns[3].ToNodePlainString();
                        //    for (int c = 2; c < prjTable.RowCount; c++)
                        //    {
                        //        TableRow dr = prjTable.Rows[c];

                        //        string UnitName = string.Empty, BidDate = string.Empty, IsBid = string.Empty, Ranking = string.Empty, WinNumber = string.Empty, TicketNumber = string.Empty;

                        //        UnitName = dr.Columns[1].ToNodePlainString();
                        //        if (colName1.Contains("投标时间") || colName1.Contains("投标日期"))
                        //            BidDate = dr.Columns[2].ToPlainTextString();
                        //        else if (colName1.Contains("得票数"))
                        //            TicketNumber = dr.Columns[2].ToNodePlainString();
                        //        else if (colName1.Contains("取胜次数"))
                        //            WinNumber = dr.Columns[2].ToNodePlainString();
                        //        if (colName2.Contains("排名"))
                        //            Ranking = dr.Columns[3].ToNodePlainString();
                        //        else if (colName2.Contains("中标候选人"))
                        //            IsBid = dr.Columns[3].ToNodePlainString() == "" ? "0" : "1";

                        //        ProjectResultDtl infoDtl = ToolDb.GetProjectResultDtl(info.Id, UnitName, BidDate, IsBid, Ranking, WinNumber, TicketNumber);
                        //        ToolDb.SaveEntity(infoDtl, "SourceId,UnitName", this.ExistsUpdate);
                        //    }
                        //}
                    }
                }
            }
            return list;
        }

        protected string PrjResultStr(string json, bool kh = false)
        {
            string strHtml = json.Replace("深圳市电子招标投标公共服务平台,", "");

            char[] chars = strHtml.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char str = chars[i];
                if (str.ToString().IsChina() || str.Equals('）'))
                {
                    if (chars.Length > (i + 2))
                    {
                        if (chars[i + 1] == ','
                            && chars[i + 2].ToString().IsNatural() == false
                            && chars[i + 2].IsJsonKh() == false)
                        {
                            string strValue1 = (str.ToString() + chars[i + 1].ToString() + chars[i + 2].ToString()).ToString();
                            string strValue2 = (str.ToString() + "，" + chars[i + 2].ToString()).ToString();
                            strHtml = strHtml.Replace(strValue1, strValue2);
                        }
                        else if (chars[i + 1] == ':')
                        {
                            string strValue1 = (str.ToString() + chars[i + 1].ToString() + chars[i + 2].ToString()).ToString();
                            string strValue2 = (str.ToString() + "：" + chars[i + 2].ToString()).ToString();
                            strHtml = strHtml.Replace(strValue1, strValue2);
                        }
                    }
                }
                else if (str.ToString().IsNumber())
                {
                    if (chars.Length > (i + 2))
                    {
                        string dateStr = chars[i + 1].ToString();
                        if (dateStr == ",")
                        {
                            string s = chars[i + 2].ToString();
                            if (s.IsNumber())
                            {
                                string strValue1 = (str.ToString() + dateStr + s).ToString();
                                string strValue2 = (str.ToString() + "，" + s).ToString();
                                strHtml = strHtml.Replace(strValue1, strValue2);
                            }
                        }
                    }
                }
            }

            //Regex reg = new Regex(@"([{]|[,])([\s]*)([\w]+?)([\s]*)([:])", RegexOptions.Multiline);//new Regex(@"([^\:\{\}\[\]\,]+)\:([^\:\,\{\}\[\]]*)", RegexOptions.Multiline);//
            //string temp = reg.Replace(jsonHtml, "$1\"$3\"$5");// reg.Replace(str, "\"$1\":\"$2\"");//
            ////temp = temp.GetReplace(@":""{",":{");
            //Dictionary<string, object> value = ToolHtml.JsonToDictionaryObject(str); 

            while (true)
            {
                string temps = Regex.Match(strHtml, @"[1-2][0-9][0-9][0-9]-([1][0-2]|0?[1-9])-([12][0-9]|3[01]|0?[1-9]) ([01]?[0-9]|[2][0-3]):[0-5]?[0-9]").Value;
                if (!string.IsNullOrEmpty(temps))
                    strHtml = strHtml.Replace(temps, "");
                else
                    break;
            }
            while (true)
            {
                string temps = Regex.Match(strHtml, @"[1-2][0-9][0-9][0-9]年([1][0-2]|0?[1-9])月([12][0-9]|3[01]|0?[1-9])日([01]?[0-9]|[2][0-3]):[0-5]?[0-9]").Value;
                if (!string.IsNullOrEmpty(temps))
                    strHtml = strHtml.Replace(temps, "");
                else
                    break;
            }

            while (true)
            {
                
                string temps = Regex.Match(strHtml.Replace(" ",""), @"[1-2][0-9][0-9][0-9]年([1][0-2]|0?[1-9])月([12][0-9]|3[01]|0?[1-9])日([01]?[0-9]|[2][0-3]):[0-5]?[0-9]").Value;
                if (!string.IsNullOrEmpty(temps))
                {
                    strHtml = strHtml.Replace(" ", "");
                    strHtml = strHtml.Replace(temps, "");
                }
                else
                    break;
            }

            strHtml = strHtml.GetReplace("[2015]", "【2015】").GetReplace("[2017]", "【2017】").GetReplace("[2013]", "【2013】").GetReplace("[2012]", "【2012】").GetReplace("[2011]", "【2011】").GetReplace("[2010]", "【2010】").GetReplace("[2009]", "【2009】").GetReplace("[2014]", "【2014】").GetReplace("[2007]", "【2007】").GetReplace("[2006]", "【2006】").GetReplace("[2005]", "【2005】").GetReplace("[2004]", "【2004】").GetReplace("[2003]", "【2003】").GetReplace("[2002]", "【2002】").GetReplace("[2001]", "【2001】").GetReplace("[2000]", "【2000】").GetReplace("[2016]", "【2016】");

            strHtml = strHtml.GetReplace("{2015}", "【2015】").GetReplace("{2017}", "【2017】").GetReplace("{2013}", "【2013】").GetReplace("{2012}", "【2012】").GetReplace("{2011}", "【2011】").GetReplace("{2010}", "【2010】").GetReplace("{2009}", "【2009】").GetReplace("{2014}", "【2014】").GetReplace("{2007}", "【2007】").GetReplace("{2006}", "【2006】").GetReplace("{2005}", "【2005】").GetReplace("{2004}", "【2004】").GetReplace("{2003}", "【2003】").GetReplace("{2002}", "【2002】").GetReplace("{2001}", "【2001】").GetReplace("{2000}", "【2000】").GetReplace("{2016}", "【2016】");

            if (kh)
            {
                strHtml = strHtml.GetReplace("[深圳市南山区前海路1366号爱心大厦15楼1502]", "【深圳市南山区前海路1366号爱心大厦15楼1502】");
            }

            chars = strHtml.ToCharArray();

            bool isChan = false;
            bool isCl = true;
            int index = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                char str = chars[i];
                if (str.ToString() == ",")
                {
                    if (!isChan)
                    {
                        isCl = false;
                        index = i;
                    }

                    if (isChan)
                    {
                        if (!isCl)
                        {
                            strHtml = strHtml.Remove(index, 1);
                            strHtml = strHtml.Insert(index, "，");
                            isCl = true;
                            index = 0;
                        }
                        if (!IsString(strHtml.Substring(i, strHtml.Length - i)))
                        {
                            strHtml = strHtml.Remove(i, 1);
                            strHtml = strHtml.Insert(i, "，");
                        }
                        isChan = false;
                    }
                    isChan = true;
                }
                if (str.ToString() == ":")
                    isChan = false;

            }

            Regex reg = new Regex(@"([^\:\{\}\[\]\,]+)\:([^\:\,\{\}\[\]]*)", RegexOptions.Multiline);
            strHtml = reg.Replace(strHtml, "\"$1\":\"$2\"");
            strHtml = strHtml.GetReplace(":\"\"{", ":{");




            #region 错误json处理
            //int beiZhu = strHtml.IndexOf("beiZhu");
            //if (beiZhu > 0)
            //{
            //    int faBuRen = strHtml.IndexOf("faBuRen");
            //    if (faBuRen > 0)
            //    {
            //        string str = strHtml.Substring(beiZhu, faBuRen - beiZhu);
            //        strHtml = strHtml.Replace(str, "beiZhu:,");
            //    }
            //}
            //beiZhu = strHtml.LastIndexOf("beiZhu");
            //if (beiZhu > 0)
            //{
            //    int faBuRen = strHtml.LastIndexOf("faBuRen");
            //    if (faBuRen > 0)
            //    {
            //        string str = strHtml.Substring(beiZhu, faBuRen - beiZhu);
            //        strHtml = strHtml.Replace(str, "beiZhu:,");
            //    }
            //}
            //int xingZheng_DiQu_BH = strHtml.IndexOf("xingZheng_DiQu_BH");
            //if (xingZheng_DiQu_BH > 0)
            //{
            //    int ziJin_LaiYuan_GuoQi = strHtml.IndexOf("ziJin_LaiYuan_GuoQi");
            //    if (ziJin_LaiYuan_GuoQi > 0)
            //    {
            //        string str = strHtml.Substring(xingZheng_DiQu_BH, ziJin_LaiYuan_GuoQi - xingZheng_DiQu_BH);
            //        strHtml = strHtml.Replace(str, "xingZheng_DiQu_BH:,");
            //    }
            //    else
            //    {
            //        ziJin_LaiYuan_GuoQi = strHtml.IndexOf("ziJin_LaiYuan_ZhengFu");
            //        if (ziJin_LaiYuan_GuoQi > 0)
            //        {
            //            string str = strHtml.Substring(xingZheng_DiQu_BH, ziJin_LaiYuan_GuoQi - xingZheng_DiQu_BH);
            //            strHtml = strHtml.Replace(str, "xingZheng_DiQu_BH:,");
            //        }
            //        else
            //        {
            //            ziJin_LaiYuan_GuoQi = strHtml.IndexOf("ziJin_LaiYuan_QiTa");
            //            if (ziJin_LaiYuan_GuoQi > 0)
            //            {
            //                string str = strHtml.Substring(xingZheng_DiQu_BH, ziJin_LaiYuan_GuoQi - xingZheng_DiQu_BH);
            //                strHtml = strHtml.Replace(str, "xingZheng_DiQu_BH:,");
            //            }
            //        }
            //    }
            //}
            //xingZheng_DiQu_BH = strHtml.LastIndexOf("xingZheng_DiQu_BH");
            //if (xingZheng_DiQu_BH > 0)
            //{
            //    int ziJin_LaiYuan_GuoQi = strHtml.LastIndexOf("ziJin_LaiYuan_GuoQi");
            //    if (ziJin_LaiYuan_GuoQi > 0)
            //    {
            //        string str = strHtml.Substring(xingZheng_DiQu_BH, ziJin_LaiYuan_GuoQi - xingZheng_DiQu_BH);
            //        strHtml = strHtml.Replace(str, "xingZheng_DiQu_BH:,");
            //    }
            //    else
            //    {
            //        ziJin_LaiYuan_GuoQi = strHtml.LastIndexOf("ziJin_LaiYuan_ZhengFu");
            //        if (ziJin_LaiYuan_GuoQi > 0)
            //        {
            //            string str = strHtml.Substring(xingZheng_DiQu_BH, ziJin_LaiYuan_GuoQi - xingZheng_DiQu_BH);
            //            strHtml = strHtml.Replace(str, "xingZheng_DiQu_BH:,");
            //        }
            //        else
            //        {
            //            ziJin_LaiYuan_GuoQi = strHtml.LastIndexOf("ziJin_LaiYuan_QiTa");
            //            if (ziJin_LaiYuan_GuoQi > 0)
            //            {
            //                string str = strHtml.Substring(xingZheng_DiQu_BH, ziJin_LaiYuan_GuoQi - xingZheng_DiQu_BH);
            //                strHtml = strHtml.Replace(str, "xingZheng_DiQu_BH:,");
            //            }
            //        }
            //    }
            //}
            //int proClassificationSub = strHtml.IndexOf("proClassificationSub");
            //if (proClassificationSub > 0)
            //{
            //    int keyArea = strHtml.IndexOf("keyArea");
            //    if (keyArea > 0)
            //    {
            //        string str = strHtml.Substring(proClassificationSub, keyArea - proClassificationSub);
            //        strHtml = strHtml.Replace(str, "proClassificationSub:,");
            //    }
            //}
            //proClassificationSub = strHtml.LastIndexOf("proClassificationSub");
            //if (proClassificationSub > 0)
            //{
            //    int keyArea = strHtml.LastIndexOf("keyArea");
            //    if (keyArea > 0)
            //    {
            //        string str = strHtml.Substring(proClassificationSub, keyArea - proClassificationSub);
            //        strHtml = strHtml.Replace(str, "proClassificationSub:,");
            //    }
            //}




            #endregion
            return strHtml;
        }

        protected bool IsString(string strHtml)
        {
            char[] chars = strHtml.ToCharArray();
            int index1 = 0, index2 = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                char str = chars[i];
                if (str.ToString() == "," && index1 < 1)
                    index1 = i;
                if (str.ToString() == ":" && index2 < 1)
                    index2 = i;

                if (index1 > 0 && index2 > 0)
                {
                    return index2 < index1;
                }
            }
            return true;
        }

        /// <summary>
        /// 剔除多余逗号
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        protected string GetPrjResultDtl(string json)
        {
            string strHtml = json;
            //StringBuilder sb = new StringBuilder();
            char[] chars = strHtml.ToCharArray();
            bool isChan = false;
            for (int i = 0; i < chars.Length; i++)
            {
                char str = chars[i];
                if (str.ToString() == ",")
                {
                    if (isChan)
                    {
                        strHtml = strHtml.Remove(i, 1);
                        isChan = false;
                    }
                    isChan = true;
                }
                if (str.ToString() == ":")
                    isChan = false;

            }
            return strHtml;
        }
    }
}
