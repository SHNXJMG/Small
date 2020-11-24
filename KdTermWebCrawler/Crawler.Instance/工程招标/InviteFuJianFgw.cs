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
    public class InviteFuJianFgw : WebSiteCrawller
    {
        public InviteFuJianFgw()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "福建省发展和改革委员会招标信息";
            this.Description = "自动抓取福建省发展和改革委员会招标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.fjbid.gov.cn/was5/web/search?channelid=275422&random=0.9505256003241871&page=1&callback=jQuery1113008122876474818197_1502764717887&_=1502764717888&prepage=";
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
            object[] objvalues = smsTypeJson["docs"] as object[];
            foreach (object objValue in objvalues)
            {
                Dictionary<string, object> dic = (Dictionary<string, object>)objValue;
                string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                 beginDate = Convert.ToString(dic["CRTIME"]);
                if (beginDate.Contains("发布时间"))
                    continue;
                string ZN_ID = Convert.ToString(dic["ZN_ID"]);
                 prjName = Convert.ToString(dic["ZN_TITLE"]);
                InfoUrl = "http://www.fjbid.gov.cn/was5/web/detail?channelid=275422&classsql=ZN_ID=" + ZN_ID;
                string htmldtl = string.Empty;
                try
                {
                    htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                }
                catch { continue; }
                Parser parser = new Parser(new Lexer(htmldtl));
                NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "xl_column")));
                if (dtlNode != null && dtlNode.Count > 0)
                {

                    HtmlTxt = dtlNode.AsHtml();
                    inviteCtx = HtmlTxt.Replace("</span>", "\r\n").Replace("<br />", "\r\n").Replace("<BR>", "\r\n").Replace("<br/>", "\r\n").ToCtxString();
                    prjAddress = inviteCtx.GetAddressRegex();
                    buildUnit = inviteCtx.GetBuildRegex();
                    if (string.IsNullOrEmpty(code))
                        code = inviteCtx.GetCodeRegex();
                    msgType = "福建省发展和改革委员会";
                    specType = inviteType = "建设工程";
                    InviteInfo info = ToolDb.GenInviteInfo("福建省", "福建省及地市", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                    list.Add(info);
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }
            return list;
        }
    }
}
