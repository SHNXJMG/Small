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
    public class InviteSZBaoAnXXGC : WebSiteCrawller
    {
        public InviteSZBaoAnXXGC()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省深圳市宝安区小型工程";
            this.Description = "自动抓取广东省深圳市宝安区小型工程招标信息";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryXXGongGaoList.do?page=1&rows=";
            this.MaxCount = 200;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
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
                string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                code = Convert.ToString(dic["gcBH"]);
                prjName = Convert.ToString(dic["gcName"]);
                beginDate = Convert.ToString(dic["ggStartTime2"]).GetDateRegex();
                string end = Convert.ToString(dic["ggEndTime"]);
                try
                {
                    endDate = ToolHtml.GetDateTimeByLong(Convert.ToInt64(end)).ToString();
                }
                catch { }
                inviteType = Convert.ToString(dic["gcLeiXing2"]);
                InfoUrl = Convert.ToString(dic["detailUrl"]);
                try
                {
                    string urll = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryOldOTDataDetail.do?type=1&id=" + dic["gcGuid"];
                    try
                    {
                        HtmlTxt = this.ToolWebSite.GetHtmlByUrl(urll).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                    }
                    catch { }
                    if (string.IsNullOrWhiteSpace(HtmlTxt))
                        urll = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/showGongGao.do?ggGuid=" + dic["ggGuid"];

                    HtmlTxt = this.ToolWebSite.GetHtmlByUrl(urll).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                    HtmlTxt = HtmlTxt.GetReplace("},{,maoDian:,html:");

                    if (string.IsNullOrWhiteSpace(HtmlTxt))
                    {
                        urll = "https://www.szjsjy.com.cn:8001/jyw-ba/jyxx/queryOldOTDataDetail.do?type=1&id=" + dic["gcGuid"];
                        HtmlTxt = this.ToolWebSite.GetHtmlByUrl(urll).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                    }
                }
                catch
                {
                    Logger.Error(prjName);
                    continue;
                }
                inviteCtx = HtmlTxt.Replace("</span>", "\r\n").Replace("<br />", "\r\n").Replace("<BR>", "\r\n").Replace("<br/>", "\r\n").ToCtxString();

                prjAddress = inviteCtx.GetAddressRegex();
                buildUnit = inviteCtx.GetBuildRegex();
                if (string.IsNullOrEmpty(code))
                    code = inviteCtx.GetCodeRegex();
                msgType = "深圳市建设工程交易中心宝安分中心";
                specType = "建设工程";
                inviteType = "小型工程";
                InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳宝安区工程", "宝安区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                sqlCount++;

                if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                {
                    Parser parser = new Parser(new Lexer(HtmlTxt));
                    NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                    if (aNode != null && aNode.Count > 0)
                    {
                        for (int k = 0; k < aNode.Count; k++)
                        {
                            ATag a = aNode[k] as ATag;
                            if (a.IsAtagAttach())
                            {
                                string link = string.Empty;
                                if (a.Link.ToLower().Contains("http"))
                                {
                                    link = a.Link.Replace("\\", "");

                                    BaseAttach attach = null;
                                    try
                                    {
                                        attach = ToolHtml.GetBaseAttach(link, a.LinkText, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                    }
                                    catch { }
                                    if (attach != null)
                                        ToolDb.SaveEntity(attach, "");
                                }
                            }
                        }
                    }
                }
                if (!crawlAll && sqlCount >= this.MaxCount) return list;
            }
            return list;
        }
    }
}
