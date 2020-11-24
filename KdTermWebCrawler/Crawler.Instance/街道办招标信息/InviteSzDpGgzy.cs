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
    public class InviteSzDpGgzy : WebSiteCrawller
    {
        public InviteSzDpGgzy()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "深圳市大鹏新区公共资源交易中心小型建设工程招标公告";
            this.Description = "自动抓取深圳市大鹏新区公共资源交易中心小型建设工程招标公告";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://113.105.69.184:51201/info_data/PT003-PT00301?pageIndex=1&pageSize=";
            this.MaxCount = 150;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + MaxCount, Encoding.UTF8);
            }
            catch { return list; }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = 50000000;
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
            object[] dicList = (object[])smsTypeJson["data"];
            foreach (object obj in dicList)
            {
                Dictionary<string, object> dic = obj as Dictionary<string, object>;
                string code = string.Empty, buildUnit = string.Empty,
                         prjName = string.Empty, prjAddress = string.Empty,
                         inviteCtx = string.Empty, bidType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty,
                         endDate = string.Empty, remark = string.Empty,
                         inviteType = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty,
                         otherType = string.Empty, HtmlTxt = string.Empty;

                prjName = Convert.ToString(dic["TITLE"]);
                beginDate = Convert.ToString(dic["CREATED_ON"]);
                InfoUrl = Convert.ToString(dic["URL"]);
                string htldtl = string.Empty;
                try
                {
                    htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                }
                catch { continue; }
                Parser parser = new Parser(new Lexer(htldtl));
                NodeList dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                if (dtlNode != null && dtlNode.Count > 0)
                {
                    HtmlTxt = dtlNode.AsHtml();
                    inviteCtx = HtmlTxt.ToCtxString();
                    code = inviteCtx.GetCodeRegex().GetCodeDel();
                    buildUnit = inviteCtx.GetBuildRegex();
                    prjAddress = inviteCtx.GetBuildRegex();
                    inviteType = prjName.GetInviteBidType();
                    specType = "建设工程";
                    msgType = "大鹏新区公共资源交易中心"; 
                    InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳区及街道工程", "大鹏新区",
                           string.Empty, code, prjName, prjAddress, buildUnit,
                           beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                    list.Add(info);
                    parser = new Parser(new Lexer(HtmlTxt));
                    NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                    if (aNode != null && aNode.Count > 0)
                    {
                        for (int k = 0; k < aNode.Count; k++)
                        {
                            ATag a = aNode[k] as ATag;
                            string KD = a.Link;
                           
                            if (KD.Contains("download"))
                            {
                                string link = string.Empty;
                                if (a.Link.ToLower().Contains("http"))
                                {
                                    link = a.Link;
                                    if (link.Contains("amp;"))
                                        link = link.Replace("amp;", "");
                                }
                                else
                                    link = "http://www.szzfcg.cn:7001/" + a.Link;
                             
                                BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                base.AttachList.Add(attach);
                            }
                        }
                    }
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }
            return list;
        }
    }
}
