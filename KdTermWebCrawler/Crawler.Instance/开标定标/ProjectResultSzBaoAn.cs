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
using System.Linq;

namespace Crawler.Instance
{
    public class ProjectResultSzBaoAn : WebSiteCrawller
    {
        public ProjectResultSzBaoAn()
            : base()
        {
            this.Group = "开标定标";
            this.Title = "深圳市交易中心(宝安分中心)定标结果公示";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取深圳市交易中心(宝安分中心)定标结果公示";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryDBJieGuoList.do?page=1&rows=";
            this.MaxCount = 200;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<ProjectResult>();
            int sqlCount = 0;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + this.MaxCount);
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
                    string Code = string.Empty, prjName = string.Empty, BuildUnit = string.Empty,
                        FinalistsWay = string.Empty, RevStaMethod = string.Empty, SetStaMethod = string.Empty,
                        VoteMethod = string.Empty, RevStaDate = string.Empty, InfoUrl = string.Empty, MsgType = string.Empty,
                        Ctx = string.Empty, Html = string.Empty, beginDate = string.Empty;

                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    Code = Convert.ToString(dic["bdBH"]);
                    prjName = Convert.ToString(dic["bdName"]);
                    beginDate = Convert.ToString(dic["createTime2"]);
                    string dbjieGuoid = Convert.ToString(dic["dbJieGuoGuid"]);
                    string bdId = Convert.ToString(dic["bdGuid"]);
                    string ggId = Convert.ToString(dic["ggGuid"]);
                    string detailUrl = Convert.ToString(dic["detailUrl"]);



                    InfoUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryOldOTDataDetail.do?type=9&id=" + Code;

                    string attachJson = string.Empty;
                    try
                    {

                        Html = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        string temp = Html.GetReplace("\"\"");
                        if (string.IsNullOrWhiteSpace(temp))
                        {
                            InfoUrl = " https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/dbResult_View.do?bdGuid=" + bdId + "&ggGuid=" + ggId + "&dbJieGuoGuid=" + dbjieGuoid;
                            Html = this.ToolWebSite.GetHtmlByUrl(InfoUrl);
                            string url = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryDbJieGuoByGuid.do?guid=" + dbjieGuoid;
                            attachJson = this.ToolWebSite.GetHtmlByUrl(url);
                        }
                    }
                    catch { continue; }


                    string gcName = string.Empty, bdName = string.Empty,
                        zbrName = string.Empty, createTime = string.Empty,
                        lxr = string.Empty, lxdh = string.Empty, dbBanFa = string.Empty, piaoJueBanFa = string.Empty;
                    bool isChouQian = false;
                    string attachId = string.Empty;
                    string rwFs = string.Empty;
                    string unitUrl = string.Empty;
                    string lxrxx = string.Empty;
                    string lxdhxx = string.Empty;
                    if (!string.IsNullOrWhiteSpace(attachJson))
                    {
                        JavaScriptSerializer newSerializer = new JavaScriptSerializer();
                        Dictionary<string, object> newTypeJson = (Dictionary<string, object>)newSerializer.DeserializeObject(attachJson);
                        Dictionary<string, object> kdInfo = (Dictionary<string, object>)newTypeJson;
                        Dictionary<string, object> ggbd = (Dictionary<string, object>)kdInfo["ggbd"];
                        Dictionary<string, object> gc = (Dictionary<string, object>)ggbd["gc"];
                        Dictionary<string, object> bd = (Dictionary<string, object>)kdInfo["bd"];
                        Dictionary<string, object> bdgc = (Dictionary<string, object>)bd["gc"];
                        try
                        {
                            attachId = Convert.ToString(kdInfo["attachFileGroupGuid"]);
                        }
                        catch { }
                        try
                        {
                            string ggGuid = Convert.ToString(kdInfo["ggGuid"]);
                            string bdGuid = Convert.ToString(kdInfo["bdGuid"]);
                            unitUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryTbrListByBdGuidAndGgGuidForGs.do?bdGuid=" + bdGuid + "&ggGuid=" + ggGuid;
                        }
                        catch { }
                        gcName = Convert.ToString(gc["gcName"]);
                        try
                        {
                            bdName = Convert.ToString(kdInfo["bdName"]);
                        }
                        catch { bdName = gcName; }
                        zbrName = Convert.ToString(gc["zbRName"]);
                        createTime = Convert.ToString(kdInfo["dbTime"]);
                        createTime = ToolHtml.GetDateTimeByLong(Convert.ToInt64(createTime)).ToString();
                        try
                        {
                            lxr = Convert.ToString(bdgc["lianXiRenName"]);
                        }
                        catch { }
                        try
                        {
                            lxrxx = Convert.ToString(bdgc["jingBanRenName"]);
                        }
                        catch { }
                        try
                        {
                            lxdh = Convert.ToString(bdgc["lianXiRenPhone"]);
                        }
                        catch { }
                        try
                        {
                            lxdhxx = Convert.ToString(bdgc["jingBanRenMobile"]);
                        }
                        catch { }
                        try
                        {
                            rwFs = Convert.ToString(kdInfo["rwFangShi"]);
                        }
                        catch { }
                        try
                        {
                            dbBanFa = Convert.ToString(kdInfo["dbBanFa"]);
                        }
                        catch { }
                        try
                        {
                            piaoJueBanFa = Convert.ToString(kdInfo["piaoJueBanFa"]);
                        }
                        catch { }
                        try
                        {
                            isChouQian = (bool)kdInfo["isChouQian"];
                        }
                        catch { }
                        string surl = " https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/dbResult_View.do?bdGuid=" + bdId + "&ggGuid=" + ggId + "&dbJieGuoGuid=" + dbjieGuoid;
                        attachJson = this.ToolWebSite.GetHtmlByUrl(surl);
                        Html = attachJson;
                        Parser parserNew = new Parser(new Lexer(Html));
                        NodeList tableNode = parserNew.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "de_tab1")));
                        if (tableNode != null && tableNode.Count > 0)
                        {
                            Html = tableNode.AsHtml();
                            Html = Html.GetReplace("<td  id=\"ggName\">&nbsp;</td>", "<td  id=\"ggName\">&nbsp;" + prjName + "</td>");
                            Html = Html.GetReplace("<td id=\"bdBH\">&nbsp;</td>", "<td id=\"bdBH\">&nbsp;" + Code + "</td>");
                            Html = Html.GetReplace("<td id=\"bdName\">&nbsp;</td>", "<td id=\"bdName\">&nbsp;" + bdName + "</td>");
                            Html = Html.GetReplace("<td id=\"zbRName\">&nbsp;</td>", "<td id=\"zbRName\">&nbsp;" + zbrName + "</td>");
                            Html = Html.GetReplace("<td id=\"dbTime\">&nbsp;</td>", "<td id=\"dbTime\">&nbsp;" + createTime + "</td>");
                            Html = Html.GetReplace("<td id=\"rwfs\">&nbsp;</td>", "<td id=\"rwfs\">&nbsp;" + rwFs + "</td>");
                            Html = Html.GetReplace("<td id=\"dbBanFa\">&nbsp;</td>", "<td id=\"dbBanFa\">&nbsp;" + dbBanFa + "</td>");
                            Html = Html.GetReplace("<td id=\"lianXiRenName\">&nbsp;</td>", "<td id=\"lianXiRenName\">&nbsp;" + lxrxx + "</td>");
                            Html = Html.GetReplace("<td id=\"lianXiRenPhone\">&nbsp;</td>", "<td id=\"lianXiRenPhone\">&nbsp;" + lxdhxx + "</td>");
                            Ctx = Html.Replace("</tr>", "\r\n").ToCtxString();
                        }


                    }

                    string resultCtx = string.Empty;
                    Parser parser = new Parser(new Lexer(Html.GetReplace("\\\"", "\"").GetReplace("0:00:00", "")));
                    NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "de_tab1")));
                    if (listNode != null && listNode.Count > 0)
                    {
                        TableTag table = listNode[0] as TableTag;
                        for (int r = 0; r < table.RowCount; r++)
                        {
                            for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                            {
                                string temp = table.Rows[r].Columns[c].ToPlainTextString().GetReplace(":,：");
                                if (c % 2 == 0)
                                    resultCtx += temp + "：";
                                else
                                    resultCtx += temp + "\r\n";
                            }
                        }
                    }

                    string strTmp = string.Empty;
                    if (!string.IsNullOrEmpty(unitUrl))
                    {
                        string unithtml = string.Empty;
                        try
                        {
                            unithtml = this.ToolWebSite.GetHtmlByUrl(unitUrl);
                        }
                        catch { }
                        object[] unitTypeJson = (object[])serializer.DeserializeObject(unithtml);
                        if (unitTypeJson.Length > 0)
                        {
                            List<LongGangResult> unitLists = this.GetUnits(unitTypeJson);
                            if (isChouQian)
                            {
                                strTmp += "<table width='100%' border='0' class='de_tab2'>";
                                strTmp += "<tr>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>序号</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>投标人名称</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>投标时间</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>中标候选人</th>";
                                strTmp += "</tr>";
                                foreach (LongGangResult unitInfo in unitLists.OrderBy(x => x.Xh))
                                {
                                    strTmp = strTmp + "<tr>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Xh + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.UnitName + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.TbDate + "</td>";
                                    if (unitInfo.BidStatus == "3")
                                    {
                                        strTmp = strTmp + "<td><input type='checkbox'  checked=true disabled=true/></td>";
                                    }
                                    else
                                    {
                                        strTmp = strTmp + "<td><input type='checkbox' disabled=true/></td>";
                                    }
                                    strTmp = strTmp + "</tr>";
                                }
                                strTmp = strTmp + "</table>";
                            }
                            else if (dbBanFa == "其他方法")
                            {
                                strTmp += "<table width='100%' border='0' class='de_tab2'>";
                                strTmp += "<tr>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>序号</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>企业名称</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>确定中标候选人</th>";
                                strTmp += "</tr>";

                                foreach (LongGangResult unitInfo in unitLists.OrderBy(x => x.Xh))
                                {
                                    strTmp = strTmp + "<tr>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Xh + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.UnitName + "</td>";
                                    if (unitInfo.BidStatus == "3")
                                    {
                                        strTmp = strTmp + "<td><input type='checkbox'  checked=true disabled=true/></td>";
                                    }
                                    else
                                    {
                                        strTmp = strTmp + "<td><input type='checkbox' disabled=true/></td>";
                                    }
                                    strTmp = strTmp + "</tr>";
                                }
                                strTmp = strTmp + "</table>";
                            }
                            else if (dbBanFa == "逐轮淘汰")
                            {
                                strTmp += "<table width='100%' border='0' class='de_tab2'>";
                                strTmp += "<tr>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>序号</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>投标人名称</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>投标报价(元)</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>投标时间</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>是否入围</th>";
                                strTmp += "</tr>";
                                foreach (LongGangResult unitInfo in unitLists.OrderBy(x => x.Xh))
                                {
                                    strTmp = strTmp + "<tr>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Xh + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.UnitName + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.BidMoney + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.TbDate + "</td>";
                                    if (unitInfo.IsNo == "是")
                                    {
                                        strTmp = strTmp + "<td><input type='checkbox'  checked=true disabled=true/></td>";
                                    }
                                    else
                                    {
                                        strTmp = strTmp + "<td><input type='checkbox' disabled=true/></td>";
                                    }

                                    strTmp = strTmp + "</tr>";
                                }
                                strTmp = strTmp + "</table>";
                            }
                            else if (dbBanFa == "集体议事法")
                            {
                                strTmp += "<table width='100%' border='0' class='de_tab2'>";
                                strTmp += "<tr>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>序号</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>企业名称</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>确定中标候选人</th>";
                                strTmp += "</tr>";

                                foreach (LongGangResult unitInfo in unitLists.OrderBy(x => x.Code))
                                {
                                    strTmp = strTmp + "<tr>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Code + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.UnitName + "</td>";
                                    if (unitInfo.IsNo == "是")
                                    {
                                        strTmp = strTmp + "<td><input type='checkbox'  checked=true disabled=true/></td>";
                                    }
                                    else
                                    {
                                        strTmp = strTmp + "<td><input type='checkbox' disabled=true/></td>";
                                    }
                                    strTmp = strTmp + "</tr>";
                                }
                                strTmp = strTmp + "</table>";
                            }
                            else if (dbBanFa == "价格竞争法")
                            {
                                strTmp += "<table width='100%' border='0' class='de_tab2'>";
                                strTmp += "<tr>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>序号</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>企业名称</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>确定中标候选人</th>";
                                strTmp += "</tr>";

                                foreach (LongGangResult unitInfo in unitLists.OrderBy(x => x.Xh))
                                {
                                    strTmp = strTmp + "<tr>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Xh + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.UnitName + "</td>";
                                    if (unitInfo.BidStatus == "3")
                                    {
                                        strTmp = strTmp + "<td><input type='checkbox'  checked=true disabled=true/></td>";
                                    }
                                    else
                                    {
                                        strTmp = strTmp + "<td><input type='checkbox' disabled=true/></td>";
                                    }
                                    strTmp = strTmp + "</tr>";
                                }
                                strTmp = strTmp + "</table>";
                            }
                            else if (piaoJueBanFa == "简单多数法")
                            {
                                strTmp += "<table width='100%' border='0' class='de_tab2'>";
                                strTmp += "<tr>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>编号</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>投标单位</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>得票数</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>排名</th>";
                                strTmp += "</tr>";
                                foreach (LongGangResult unitInfo in unitLists.OrderBy(x => x.Code))
                                {
                                    strTmp = strTmp + "<tr>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Code + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.UnitName + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Piao + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Xh + "</td>";
                                    strTmp = strTmp + "</tr>";
                                }
                                strTmp = strTmp + "</table>";
                            }
                            else if (piaoJueBanFa == "一对一比较法")
                            {
                                strTmp += "<table width='100%' border='0' class='de_tab2'>";
                                strTmp += "<tr>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>编号</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>投标单位</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>取胜次数</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>排名</th>";
                                strTmp += "</tr>";
                                foreach (LongGangResult unitInfo in unitLists.OrderBy(x => x.Code))
                                {
                                    strTmp = strTmp + "<tr>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Code + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.UnitName + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Piao + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Xh + "</td>";
                                    strTmp = strTmp + "</tr>";
                                }
                                strTmp = strTmp + "</table>";
                            }
                            else
                            {
                                strTmp += "<table width='100%' border='0' class='de_tab2'>";
                                strTmp += "<tr>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>编号</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>投标单位</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>取胜次数</th>";
                                strTmp += "<th style='text-align: left' class='bg_tdtop'>排名</th>";
                                strTmp += "</tr>";
                                foreach (LongGangResult unitInfo in unitLists.OrderBy(x => x.Code))
                                {
                                    strTmp = strTmp + "<tr>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Code + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.UnitName + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Piao + "</td>";
                                    strTmp = strTmp + "<td style='padding: 0px'>" + unitInfo.Xh + "</td>";
                                    strTmp = strTmp + "</tr>";
                                }
                                strTmp = strTmp + "</table>";
                            }
                        }
                    }

                    Ctx = Html.GetReplace("</tr> ", "\r\n").ToCtxString();
                    BuildUnit = resultCtx.GetRegex("建设单位").GetReplace("&nbsp", "");
                    if (string.IsNullOrEmpty(BuildUnit))
                        BuildUnit = zbrName;
                    FinalistsWay = resultCtx.GetRegex("入围方式").GetReplace("&nbsp", "");
                    RevStaMethod = resultCtx.GetRegex("评标方法");
                    SetStaMethod = resultCtx.GetRegex("定标方法").GetReplace("&nbsp", "");
                    VoteMethod = resultCtx.GetRegex("票决方法");
                    RevStaDate = resultCtx.GetRegex("定标时间").GetDateRegex();
                    if (string.IsNullOrEmpty(RevStaDate))
                        RevStaDate = createTime;

                    if (!string.IsNullOrWhiteSpace(strTmp))
                    {
                        Html += strTmp;
                        Ctx = Html.GetReplace("</tr> ", "\r\n").ToCtxString();
                    }

                    MsgType = "深圳市建设工程交易中心宝安分中心";

                    ProjectResult info = ToolDb.GetProjectResult("广东省", "深圳宝安区工程", "宝安区", Code, prjName, BuildUnit, FinalistsWay, RevStaMethod, SetStaMethod,
                                 VoteMethod, RevStaDate, detailUrl, MsgType, Ctx, Html, beginDate);
                    sqlCount++;

                    if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                    {
                        if (!string.IsNullOrWhiteSpace(attachId))
                        {
                            string url = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/filegroup/queryByGroupGuidZS.do?groupGuid=" + attachId;

                            string attachHtml = string.Empty;
                            try
                            {
                                attachHtml = this.ToolWebSite.GetHtmlByUrl(url);
                            }
                            catch { }
                            if (!string.IsNullOrWhiteSpace(attachHtml))
                            {
                                JavaScriptSerializer newSerializers = new JavaScriptSerializer();
                                Dictionary<string, object> newTypeJsons = (Dictionary<string, object>)newSerializers.DeserializeObject(attachHtml);
                                Dictionary<string, object> mofo = (Dictionary<string, object>)newTypeJsons;
                                object[] objs = (object[])mofo["rows"];
                                foreach (object objAttach in objs)
                                {
                                    Dictionary<string, object> attachs = (Dictionary<string, object>)objAttach;
                                    string attachguid = Convert.ToString(attachs["attachGuid"]);
                                    string attachName = Convert.ToString(attachs["attachName"]);
                                    string link = "https://www.szjsjy.com.cn:8001/file/downloadFile?fileId=" + attachguid;
                                    BaseAttach attach = ToolHtml.GetBaseAttach(link, attachName, info.Id, "SiteManage\\Files\\Attach\\");
                                    if (attach != null)
                                        ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                                }
                            }
                        }
                        else
                        {
                            parser = new Parser(new Lexer(Html));
                            NodeList fileNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (fileNode != null && fileNode.Count > 0)
                            {
                                for (int f = 0; f < fileNode.Count; f++)
                                {
                                    ATag tag = fileNode[f] as ATag;
                                    if (tag.IsAtagAttach() || tag.Link.ToLower().Contains("downloadfile"))
                                    {
                                        try
                                        {
                                            BaseAttach attach = null;
                                            string link = string.Empty;
                                            if (tag.Link.ToLower().Contains("http"))
                                            {
                                                link = tag.Link;
                                                if (link.StartsWith("\\"))
                                                    link = link.Substring(link.IndexOf("\\"), link.Length - link.IndexOf("\\"));
                                                if (link.EndsWith("//"))
                                                    link = link.Remove(link.LastIndexOf("//"));
                                                link = link.GetReplace("\\", "");
                                            }
                                            else
                                                link = "https://www.szjsjy.com.cn:8001/" + tag.Link;
                                            attach = ToolHtml.GetBaseAttach(link, tag.LinkText, info.Id, "SiteManage\\Files\\Attach\\");

                                            if (attach != null)
                                                ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");

                                        }
                                        catch { continue; }
                                    }
                                }
                            }
                        }
                    }
                    if (!crawlAll && sqlCount >= this.MaxCount) return null;
                }
            }
            return list;
        }

        protected List<LongGangResult> GetUnits(object[] objs)
        {
            List<LongGangResult> list = new List<LongGangResult>();
            foreach (object obj in objs)
            {
                Dictionary<string, object> dic = (Dictionary<string, object>)obj;
                LongGangResult info = new LongGangResult();
                info.UnitName = Convert.ToString(dic["tbrName"]);
                try
                {
                    info.Xh = Convert.ToInt32(dic["tbrSequence"]);
                }
                catch { }
                try
                {
                    info.Win = Convert.ToString(dic["dePiaoShu"]);
                }
                catch { }
                try
                {
                    info.TbDate = ToolHtml.GetDateTimeByLong(Convert.ToInt64(dic["tbTime"])).ToString();
                }
                catch { }
                try
                {
                    info.Piao = Convert.ToString(dic["dePiaoShu"]);
                }
                catch { }
                try
                {
                    info.Code = Convert.ToString(dic["tbrTouPiaoBH"]);
                }
                catch { }
                try
                {
                    info.BidMoney = Convert.ToString(dic["tbBaoJia"]);
                }
                catch { }
                try
                {
                    info.BidStatus = Convert.ToString(dic["zhongBiaoZhuangTai"]);
                }
                catch { }
                try
                {
                    info.IsNo = Convert.ToString(dic["jinJiFangShi"]) == "2" ? "是" : "否";
                }
                catch { }
                try
                {
                    info.Ming = Convert.ToString(dic["tbrSequence"]);
                }
                catch { }
                list.Add(info);
            }
            return list;
        }
    }
}
