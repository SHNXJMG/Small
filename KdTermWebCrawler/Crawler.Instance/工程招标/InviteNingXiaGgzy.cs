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
    public class InviteNingXiaGgzy : WebSiteCrawller
    {
        public InviteNingXiaGgzy()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "宁夏回族自治区公告资源交易网招标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取宁夏回族自治区公告资源交易网招标信息";
            this.SiteUrl = "http://www.nxggzyjy.org/ningxiaweb/002/002001/002001001/about.html";
            this.MaxCount = 400;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string urlFormt = "http://www.nxzfcg.gov.cn/ningxia/services/BulletinWebServer/getBulletinInfoList?response=application/json&pageIndex=1&pageSize={0}&siteguid={1}&categorynum=002001001&cityname=";
            IList list = new List<InviteInfo>(); 
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch { return list; }
            string siteId = ToolHtml.GetHtmlInputValueById(html, "siteguid");
            string url = string.Format(urlFormt, this.MaxCount, siteId);

            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(url);
            }
            catch { return null; }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
            smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(smsTypeJson["return"].ToString());
            object[] listDatas = (object[])smsTypeJson["Table"];

            foreach (object data in listDatas)
            {
                string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                 prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                 specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                 remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                  CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                Dictionary<string, object> dic = data as Dictionary<string, object>;
                prjName = Convert.ToString(dic["title"]);
                string infoid = Convert.ToString(dic["infoid"]);
                beginDate = Convert.ToString(dic["infodate"]);
                string area = prjName.GetReplace("[,]", "kdxx").GetRegexBegEnd("kdxx","kdxx");
                if (area.Contains("报名"))
                    area = "";
                prjName = prjName.GetReplace(string.Format("[{0}],[{1}],[{2}]",area,"正在报名","报名结束"));
                if (prjName.Contains("[自治区]"))
                    prjName = prjName.GetReplace("[自治区]");
                InfoUrl = string.Format("http://www.nxggzyjy.org/ningxia/WebbuilderMIS/RedirectPage/RedirectPage.jspx?infoid={0}&categorynum=002001001&locationurl=http://www.nxggzyjy.org/ningxiaweb", infoid);
                string htmldtl = string.Empty;
                try
                {
                    htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                }
                catch { continue; }
                Parser parser = new Parser(new Lexer(htmldtl));
                NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "mainContent")));
                if (dtlNode != null && dtlNode.Count > 0)
                {
                    HtmlTxt = dtlNode.AsHtml();
                    inviteCtx = HtmlTxt.ToLower().GetReplace("<br/>,<br>,</p>", "\r\n").ToCtxString();
                    buildUnit = inviteCtx.GetReplace(" ").GetBuildRegex();
                    if (buildUnit.Contains("招标代理"))
                        buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                    if (buildUnit.Contains("代理"))
                        buildUnit = buildUnit.Remove(buildUnit.IndexOf("代理"));
                    if (buildUnit.Contains("联系"))
                        buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                    if (buildUnit.Contains("公司"))
                        buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司"))+ "公司";
                    code = inviteCtx.GetReplace(" ").GetCodeRegex().GetCodeDel();
                    prjAddress = inviteCtx.GetReplace(" ").GetAddressRegex();
                    msgType = "宁夏公共资源交易管理局";
                    specType = "建设工程";
                    inviteType = prjName.GetInviteBidType();
                    InviteInfo info = ToolDb.GenInviteInfo("宁夏回族自治区", "宁夏回族自治区及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                    list.Add(info);
                    parser = new Parser(new Lexer(HtmlTxt));
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
                                    link = a.Link;
                                else
                                    link = "http://www.nxzfcg.gov.cn/" + a.Link;
                                BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                base.AttachList.Add(attach);
                            }
                        }
                    }
                    if (!crawlAll && list.Count >= this.MaxCount)
                        return list;
                    
                } 
            }
            return list;
        }
    }
}
