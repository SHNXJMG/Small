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
using System.Threading;


namespace Crawler.Instance
{
    public class BidSZBaoAnXXGC : WebSiteCrawller
    {
        public BidSZBaoAnXXGC()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省深圳市宝安区小型工程";
            this.Description = "自动抓取广东省深圳市宝安区小型工程中标信息";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryXXZBJieGuoList.do?page=1&rows=";
            this.MaxCount = 200;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
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
            object[] objvalues = smsTypeJson["rows"] as object[];
            foreach (object objValue in objvalues)
            {
                Dictionary<string, object> dic = (Dictionary<string, object>)objValue;
                string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
                        bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                string ziGeDengJi = string.Empty, ziGeZhengShu = string.Empty, zbFangShi = string.Empty;
                code = Convert.ToString(dic["bdBH"]);
                prjName = Convert.ToString(dic["bdName"]);
                if (prjName.Contains("测试"))
                    continue;
                string sel = Convert.ToString(dic["zbgsStartTime"]);
                try
                {
                    beginDate = ToolHtml.GetDateTimeByLong(Convert.ToInt64(sel)).ToString();
                }
                catch { }
                string end = Convert.ToString(dic["zbgsEndTime"]);
                try
                {
                    endDate = ToolHtml.GetDateTimeByLong(Convert.ToInt64(end)).ToString();
                }
                catch { }
                try
                {
                    bidType = Convert.ToString(dic["gcLeiXing2"]);
                }
                catch (Exception ex)
                {

                    bidType = ToolHtml.GetInviteTypes(prjName);
                }
                InfoUrl = Convert.ToString(dic["detailUrl"]);
                bool isJson = false;
                string urll = string.Empty;
                List<Dictionary<string, object>> listAttachs = new List<Dictionary<string, object>>();
                try
                {
                    urll = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryOldOTDataDetail.do?type=4&id=" + dic["dbZhongBiaoJieGuoGuid"];
                    HtmlTxt = this.ToolWebSite.GetHtmlByUrl(urll).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                }
                catch (Exception ex)
                {
                    Logger.Error(prjName);
                    continue;
                }
                try
                {
                    if (string.IsNullOrWhiteSpace(HtmlTxt))
                    {
                        string htmldtl = string.Empty;
                        isJson = true;
                        try
                        {
                            urll = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryZbgs.do?guid=" + dic["dbZhongBiaoJieGuoGuid"] + "&ggGuid=&bdGuid=";
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(urll);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(prjName);
                            continue;
                        }
                        Dictionary<string, object> dtlJsons = (Dictionary<string, object>)serializer.DeserializeObject(htmldtl);

                        buildUnit = Convert.ToString(dtlJsons["zbrAndLht"]);
                        bidUnit = Convert.ToString(dtlJsons["tbrName"]);
                        bidMoney = Convert.ToString(dtlJsons["zhongBiaoJE"]);
                        try
                        {
                            bidMoney = (decimal.Parse(bidMoney) / 1000000).ToString();
                        }
                        catch { }
                        prjMgr = Convert.ToString(dtlJsons["xiangMuJiLi"]);
                        Dictionary<string, object> gg = null;
                        try
                        {
                            gg = dtlJsons["gg"] as Dictionary<string, object>;
                        }
                        catch { }
                        Dictionary<string, object> bd = null;
                        Dictionary<string, object> gc = null;
                        Dictionary<string, object> xm = null;
                        try
                        {
                            bd = dtlJsons["bd"] as Dictionary<string, object>;
                        }
                        catch { }
                        try
                        {
                            gc = bd["gc"] as Dictionary<string, object>;
                        }
                        catch { }
                        try
                        {
                            xm = bd["xm"] as Dictionary<string, object>;
                        }
                        catch { }
                        try
                        {

                            if (prjMgr.Contains("----;"))
                                prjMgr = null;
                            ziGeDengJi = Convert.ToString(dtlJsons["ziGeDengJi"]);
                            if (ziGeDengJi.Contains("----;"))
                                ziGeDengJi = null;
                            ziGeZhengShu = Convert.ToString(dtlJsons["ziGeZhengShu"]);
                            if (ziGeZhengShu.Contains("----;"))
                                ziGeZhengShu = null;
                            zbFangShi = Convert.ToString(gc["zbFangShi"]);

                            if (zbFangShi.Contains("1"))
                            {
                                zbFangShi = "公开招标";
                            }
                            else if (zbFangShi.Contains("2"))
                            {
                                zbFangShi = "邀请招标";
                            }
                            else if (zbFangShi.Contains("4"))
                            {
                                zbFangShi = "单一来源";
                            }
                            else if (zbFangShi.Contains("5"))
                            {
                                zbFangShi = "预选招标子工程";
                            }

                        }
                        catch { }
                        string htl = this.ToolWebSite.GetHtmlByUrl(InfoUrl);
                        Parser parser = new Parser(new Lexer(htl));
                        NodeList nodelist = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "right_bg")));
                        if (nodelist != null && nodelist.Count > 0)
                        {
                            HtmlTxt = nodelist.AsHtml();
                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"gcBH\"></span>", "<span id=\"gcBH\">" + code + "</span>");
                            }
                            catch { }
                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"gcName\"></span>", "<span id=\"gcBH\">" + gc["gcName"] + "</span>");
                            }
                            catch { }
                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"bdName\"></span>", "<span id=\"bdName\">" + prjName + "</span>");
                            }
                            catch { }
                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"xmBH\"></span>", "<span id=\"xmBH\">" + xm["xm_BH"] + "</span>");
                            }
                            catch { }
                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"xmName\"></span>", "<span id=\"xmName\">" + xm["xm_Name"] + "</span>");
                            }
                            catch { }
                            try
                            {
                                long zbgsStartTime = Convert.ToInt64(dtlJsons["zbgsStartTime"]);
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"zbgsStartTime\"></span>", "<span id=\"zbgsStartTime\">" + ToolHtml.GetDateTimeByLong(zbgsStartTime) + "</span>");
                            }
                            catch { }
                            try
                            {
                                long zbgsEndTime = Convert.ToInt64(dtlJsons["zbgsEndTime"]);
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"zbgsEndTime\"></span>", "<span id=\"zbgsEndTime\">" + ToolHtml.GetDateTimeByLong(zbgsEndTime) + "</span>");
                            }
                            catch { }
                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"zbRName\"></span>", "<span id=\"zbRName\">" + gc["zbRName"] + "</span>");
                            }
                            catch { }
                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"zbdlJG\"></span>", "<span id=\"zbdlJG\">" + gc["creatorName"] + "</span>");
                            }
                            catch { }
                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"zbFangShi\"></span>", "<span id=\"zbFangShi\">" + zbFangShi + "</span>");
                            }
                            catch { }
                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"tbrName\"></span>", "<span id=\"tbrName\">" + dtlJsons["tbrName"] + "</span>");
                            }
                            catch { }
                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"zhongBiaoJE\"></span>", "<span id=\"zhongBiaoJE\">" + bidMoney + "万元</span>");
                            }
                            catch { }
                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"zhongBiaoGQ\"></span>", "<span id=\"zhongBiaoGQ\">" + dtlJsons["zhongBiaoGQ"] + "</span>");
                            }
                            catch { }
                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"xiangMuJiLi\"></span>", "<span id=\"xiangMuJiLi\">" + prjMgr + "</span>");
                            }
                            catch { }
                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"ziGeDengJi\"></span>", "<span id=\"ziGeDengJi\">" + ziGeDengJi + "</span>");
                            }
                            catch { }

                            try
                            {
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"ziGeZhengShu\"></span>", "<span id=\"ziGeZhengShu\">" + ziGeZhengShu + "</span>");
                            }
                            catch { }
                            try
                            {
                                string zanding = string.IsNullOrWhiteSpace(Convert.ToString(dtlJsons["isZanDingJinE"])) ? "否" : "是";
                                HtmlTxt = HtmlTxt.GetReplace("<span id=\"isZanDingJinE\"></span>", "<span id=\"isZanDingJinE\">" + zanding + "</span>");
                            }
                            catch { }


                        }
                        try
                        {
                            string fileUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/filegroup/queryByGroupGuidZS.do?groupGuid=" + dtlJsons["ztbFileGroupGuid"];
                            string fileJson = this.ToolWebSite.GetHtmlByUrl(fileUrl);
                            Dictionary<string, object> fileDic = (Dictionary<string, object>)serializer.DeserializeObject(fileJson);
                            object[] objFile = fileDic["rows"] as object[];

                            foreach (object file in objFile)
                            {
                                Dictionary<string, object> attach = file as Dictionary<string, object>;
                                listAttachs.Add(attach);
                            }
                        }
                        catch { }
                    }
                }
                catch { continue; }



                bidCtx = HtmlTxt.Replace("<br />", "\r\n").Replace("<BR>", "\r\n").Replace("</P>", "\r\n").ToCtxString();
                if (!isJson)
                {
                    prjAddress = bidCtx.GetAddressRegex();
                    buildUnit = bidCtx.GetBuildRegex();
                    bidUnit = bidCtx.GetBidRegex();
                    bidMoney = bidCtx.GetMoneyRegex();
                    prjMgr = bidCtx.GetMgrRegex();
                    if (string.IsNullOrEmpty(code))
                        code = bidCtx.GetCodeRegex();
                }
                msgType = "深圳市建设工程交易中心宝安分中心";
                specType = "建设工程";
                bidType = "小型工程";
                BidInfo info = ToolDb.GenBidInfo("广东省", "深圳宝安区工程", "宝安区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                if (!crawlAll && sqlCount >= this.MaxCount) return null;

                sqlCount++;
                if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                {
                    if (!isJson)
                    {
                        Parser parser = new Parser(new Lexer(HtmlTxt));
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
                                        attach = ToolHtml.GetBaseAttachByUrl(link, tag.LinkText, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                        if (attach != null)
                                            ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                                    }
                                    catch { continue; }
                                }
                            }
                        }
                    }
                    else if (listAttachs.Count > 0)
                    {
                        foreach (Dictionary<string, object> attach in listAttachs)
                        {
                            BaseAttach attachBase = null;
                            try
                            {
                                string attachName = Convert.ToString(attach["attachName"]);
                                string attachId = Convert.ToString(attach["attachGuid"]);
                                string link = "https://www.szjsjy.com.cn:8001/file/downloadFile?fileId=" + attachId;

                                attachBase = ToolHtml.GetBaseAttach(link, attachName, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                if (attachBase != null)
                                    ToolDb.SaveEntity(attachBase, "SourceID,AttachServerPath");
                            }
                            catch { }
                        }
                    }
                }
            }
            return list;
        }
    }
}
