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

namespace Crawler.Instance
{
    public class InviteSzJyzxZbgg : WebSiteCrawller
    {
        public InviteSzJyzxZbgg()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省深圳市区招标信息(2015版)";
            this.Description = "自动抓取广东省深圳市区招标信息(2015版)";
            this.MaxCount = 20;
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,10:20,10:50,11:30,14:05,14:25,14:50,15:25,16:00,16:50,19:00";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw/queryGongGaoList.do?page=1&isHistoryGG=true&rows=";
            this.ExistsHtlCtx = true;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int sqlCount = 0;
            string html = string.Empty;
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
                    string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty,
                               inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, HtmlTxt = string.Empty;
                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    code = Convert.ToString(dic["gcBH"]);
                    prjName = Convert.ToString(dic["gcName"]);
                   
                    inviteType = Convert.ToString(dic["gcLeiXing2"]);

                    beginDate = Convert.ToString(dic["ggStartTime2"]);
                    string addUrl = Convert.ToString(dic["detailUrl"]);
                    //https://www.szjsjy.com.cn:8001/jyw/showGongGao.do?ggGuid=03fb1287-935e-4e39-ab1a-35423a81928a&gcbh=&bdbhs=
                    InfoUrl = "https://www.szjsjy.com.cn:8001/jyw/queryOldDataDetail.do?type=1&id=" + Convert.ToString(dic["ggGuid"]);
                    try
                    {
                        try
                        {
                            HtmlTxt = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                        }
                        catch{}
                        if (string.IsNullOrEmpty(HtmlTxt))
                        {
                            HtmlTxt = this.ToolWebSite.GetHtmlByUrl("https://www.szjsjy.com.cn:8001/jyw/showGongGao.do?ggGuid=" + Convert.ToString(dic["ggGuid"])).GetJsString().GetReplace("\\t,\\r,\\n,\",{maoDian:,}");
                            Parser dtlparser = new Parser(new Lexer(HtmlTxt));
                            NodeList dtlNode = dtlparser.ExtractAllNodesThatMatch(new TagNameFilter("table"));//(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zbgk")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                inviteCtx = string.Empty;
                                HtmlTxt = dtlNode.AsHtml();
                                for (int j = 0; j < dtlNode.Count; j++)
                                {
                                    TableTag table = dtlNode[j] as TableTag;
                                    for (int r = 0; r < table.RowCount; r++)
                                    {
                                        for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                                        {
                                            string temp = table.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                            if ((c + 1) % 2 == 0)
                                                inviteCtx += temp + "\r\n";
                                            else
                                                inviteCtx += temp + "：";
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch { continue; }
                    if (string.IsNullOrEmpty(inviteCtx))
                        inviteCtx = HtmlTxt.GetReplace("<br />,<br/>,</ br>,</br>", "\r\n").ToCtxString() + "\r\n";
                    buildUnit = inviteCtx.GetBuildRegex();
                    if (string.IsNullOrEmpty(buildUnit))
                        buildUnit = inviteCtx.Replace(" ", "").GetBuildRegex();
                    if (string.IsNullOrEmpty(buildUnit))
                        buildUnit = inviteCtx.GetRegex("建 设 单 位");
                    specType = "建设工程";
                    prjAddress = inviteCtx.GetAddressRegex();
                    if (string.IsNullOrEmpty(prjAddress))
                        prjAddress = inviteCtx.Replace(" ", "").GetAddressRegex();
                    if (string.IsNullOrEmpty(prjAddress))
                        prjAddress = inviteCtx.GetRegex("工 程 地 址");
                    msgType = "深圳市建设工程交易中心";
                    #region 2013-11-19修改
                    Dictionary<string, Regex> dicRegex = new Dictionary<string, Regex>();
                    dicRegex.Add("重要提示", new Regex(@"([.\S\s]*)(?=重要提示)"));
                    dicRegex.Add("温馨提示", new Regex(@"([.\S\s]*)(?=温馨提示)"));
                    foreach (string dicValue in dicRegex.Keys)
                    {
                        if (inviteCtx.Contains(dicValue))
                            inviteCtx = dicRegex[dicValue].Match(inviteCtx).Value;
                    }
                    #endregion

                    InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳市工程", string.Empty, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, string.Empty, addUrl, HtmlTxt);
                    sqlCount++;
                    if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                    {
                        Parser parser = new Parser(new Lexer(HtmlTxt));
                        NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        if (aNode != null && aNode.Count > 0)
                        {
                            for (int a = 0; a < aNode.Count; a++)
                            {
                                ATag aTag = aNode[a].GetATag();
                                if (aTag.Link.Contains("download"))
                                {
                                    try
                                    {
                                        BaseAttach attach = ToolHtml.GetBaseAttach(aTag.Link, aTag.LinkText, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                        if (attach != null)
                                            ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                                    }
                                    catch
                                    {

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
    }
}
