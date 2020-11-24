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
using System.Web.UI.MobileControls;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.Collections.Generic;


namespace Crawler.Instance
{
    public class InviteSzairport : WebSiteCrawller
    {
        public InviteSzairport()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "深圳宝安国际机场";
            this.PlanTime = "8:00,9:15,10:10,11:15,12:8,13:45,14:30,15:45,17:45";
            this.Description = "自动抓取深圳宝安国际机场招标信息";
            this.SiteUrl = "https://zhaobiao.szairport.com/SZWI/portal/homeInformList.do";
            this.MaxCount = 100;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string url = "https://zhaobiao.szairport.com/SZWI/portal/homeInformListJson.do";
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string cookieStr = string.Empty;
            try
            {
                string post = string.Format("start={0}&limit={1}", 0, this.MaxCount);
                html = ToolHtml.GetHtmlByUrlPost(url, post, Encoding.UTF8,ref cookieStr);
            }

            catch(Exception ex)
            {
               
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);

            Dictionary<string, object> tempDic = smsTypeJson["recordData"] as Dictionary<string, object>;
            if (tempDic == null) return list;

            //string totalCount = tempDic["totalCount"].ToString();
            //try
            //{
            //    pageInt = int.Parse(totalCount) / 20 + 1;
            //}
            //catch { }

            object[] objList = tempDic["records"] as object[];
            foreach (object obj in objList)
            {
                string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                    prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                    specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                    remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                    CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, img = string.Empty,
                                    HtmlTxt = string.Empty;

                Dictionary<string, object> dic = obj as Dictionary<string, object>;
                prjName = Convert.ToString(dic["title"]);
                beginDate = Convert.ToString(dic["releaseTimeStr"]);
                string seqNo = Convert.ToString(dic["seqNo"]);
                InfoUrl = "http://zhaobiao.szairport.com/SZWI/portal/homeInformView.do?seqNo=" + seqNo;
                string htmldtl = string.Empty;
                try
                {
                    htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                }
                catch { continue; }

                Parser parser = new Parser(new Lexer(htmldtl));
                NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "newsBox")));
                if (dtlNode != null && dtlNode.Count > 0)
                {
                    HtmlTxt = dtlNode.AsHtml();
                    inviteCtx = HtmlTxt.ToCtxString();

                    buildUnit = inviteCtx.GetBuildRegex();
                    prjAddress = inviteCtx.GetAddressRegex();
                    code = inviteCtx.GetCodeRegex().GetCodeDel();
                    if (code.Contains("__"))
                        code = "";

                    specType = "其他";
                    msgType = "深圳宝安国际机场";
                    inviteType = ToolHtml.GetInviteTypes(prjName);
                    InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                    list.Add(info);
                    parser = new Parser(new Lexer(HtmlTxt));
                    NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                    if (aNode != null && aNode.Count > 0)
                    {
                        for (int a = 0; a < aNode.Count; a++)
                        {
                            ATag aTag = aNode[a] as ATag;
                            if (aTag.IsAtagAttach())
                            {
                                string fileUrl = string.Empty;
                                if (aTag.Link.Contains("http"))
                                    fileUrl = aTag.Link;
                                else
                                    fileUrl = "http://zhaobiao.szairport.com/" + aTag.Link;
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
