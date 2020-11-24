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
    public class InviteZhongHan : WebSiteCrawller
    {
        public InviteZhongHan()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "中航技国际经贸发展有限公司招标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取中航技国际经贸发展有限公司招标信息";
            this.SiteUrl = "http://bid.aited.cn/front/ajax_getBidList.do";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {

            IList list = new List<InviteInfo>();
            int pageInt = 295;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;


            for (int i = 1; i < pageInt; i++)
            {
                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "classId", "key", "page" }, new string[] { "151", "-1", i.ToString() });
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                }
                catch { return list; }

                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("li"));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                        prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                        specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                        remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                        CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = listNode[j].GetATag();

                        prjName = aTag.GetAttribute("title").GetReplace("\\\"");
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();

                        InfoUrl = "http://bid.aited.cn/" + aTag.Link.GetReplace("../,\\\"");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "news_article")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        { 
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex().GetCodeDel(); 
                            msgType = "中航技国际经贸发展有限公司";
                            specType = "建设工程";
                            inviteType = prjName.GetInviteBidType();
                            InviteInfo info = ToolDb.GenInviteInfo("北京市", "北京市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k] as ATag;
                                    if (a.IsAtagAttach()|| a.Link.Contains("DownloadServlet"))
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link; 
                                        else
                                            link = "http://bid.aited.cn/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    } 
                } 
            }
            return list;
        }
    }
}
