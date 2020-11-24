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
    public class InviteHbEZhou : WebSiteCrawller
    {
        public InviteHbEZhou()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "鄂州市公共资源交易中心招标公告";
            this.Description = "自动抓取鄂州市公共资源交易中心招标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://www.ezztb.gov.cn/jiaoyixingxi/jyxx.html?type=10&index=1";
            this.MaxCount = 80;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string urlList = "http://www.ezztb.gov.cn/jiaoyixinxi/queryJiaoYiXinXiPagination.do?bianHao=&gongChengLeiBie=&gongChengType=&gongShiType=10&page=1&title=&type=10&rows=";
            IList list = new List<InviteInfo>();
            int sqlCount = 0;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(urlList + this.MaxCount);
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
                string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                code = Convert.ToString(dic["bianHao"]);
                prjName = Convert.ToString(dic["title"]);
                beginDate = Convert.ToString(dic["faBuStartTimeText"]).GetDateRegex();
                inviteType = Convert.ToString(dic["gongChengTypeText"]);

                if (prjName.Contains("测试"))
                    continue;
                InfoUrl = "http://www.ezztb.gov.cn/jyw/jyw/showGongGao.do?ggGuid=" + dic["yuanXiTongId"];

                try
                {
                    HtmlTxt = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                    JavaScriptSerializer Newserializer = new JavaScriptSerializer();
                    Dictionary<string, object> newTypeJson = (Dictionary<string, object>)Newserializer.DeserializeObject(HtmlTxt);
                    HtmlTxt = Convert.ToString(newTypeJson["html"]);
                    if (string.IsNullOrWhiteSpace(HtmlTxt))
                    {
                        string url = "http://www.ezztb.gov.cn/jiaoyixingxi/zbgg_view.html?guid=" + dic["yuanXiTongId"];
                        string htmldtl = this.ToolWebSite.GetHtmlByUrl(url);

                    }
                }
                catch (Exception ex) { continue; }
                inviteCtx = HtmlTxt.Replace("</span>", "\r\n").ToCtxString();

                prjAddress = inviteCtx.GetAddressRegex();
                buildUnit = inviteCtx.GetBuildRegex();
                //if (string.IsNullOrWhiteSpace(buildUnit))
                //    buildUnit = inviteCtx.GetRegex("招标人与招标代理建设单位");
                if (string.IsNullOrEmpty(code))
                    code = inviteCtx.GetCodeRegex();
                msgType = "鄂州市公共资源交易中心";
                specType = "建设工程";
                if (string.IsNullOrWhiteSpace(inviteType))
                    inviteType = prjName.GetInviteBidType();
                buildUnit = buildUnit.Replace(" ", "");
                InviteInfo info = ToolDb.GenInviteInfo("湖北省", "湖北省及地市", "鄂州市", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                list.Add(info);
                if (!crawlAll && list.Count >= this.MaxCount) return list;
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
                                link = a.Link;
                            else
                                link = "http://www.ezztb.gov.cn/" + a.Link;
                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                            base.AttachList.Add(attach);
                        }
                    }
                }
            }
            return list;
        }
    }
}
