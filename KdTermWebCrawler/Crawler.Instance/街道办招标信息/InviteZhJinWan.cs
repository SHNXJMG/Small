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
using System.Threading;

namespace Crawler.Instance
{
    public class InviteZhJinWan : WebSiteCrawller
    {
        public InviteZhJinWan()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "金湾区政府采购信息招标公告";
            this.Description = "自动抓取金湾区政府采购信息招标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://www.jinwan.gov.cn/NewsList.aspx?ID=12004&PID=10002&flag=2";
            this.MaxCount = 150;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "cNavBar_cTotalPages")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode[0].ToNodePlainString();
                try
                {
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__VIEWSTATE",
                    "cID",
                    "cFlag",
                    "Input",
                    "Left1:cID",
                    "Left1:cFlag",
                    "cNavBar:cPageSize",
                    "cNavBar:cPageIndex",
                    "Foot1:ddlLink1",
                    "Foot1:ddlLink2",
                    "Foot1:ddlLink3",
                    "Foot1:ddlLink4",
                    "Foot1:ddlLink5",
                    "__EVENTVALIDATION"
                    }, new string[]{
                    viewState,
                    "12004",
                    "2",
                    "",
                    "12004",
                    "2",
                    "12",
                    i.ToString(),
                    "",
                    "",
                    "",
                    "",
                    "",
                    eventValidation
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "95%")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {

                        TableRow tr = table.Rows[j];
                        string code = string.Empty, prjName = string.Empty, beginDate = string.Empty, InfoUrl = string.Empty;

                        ATag atag = tr.Columns[0].GetATag();



                        beginDate = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.jinwan.gov.cn/" + atag.Link.GetReplace("../");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "fonth21")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList nameNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "fonth19")));

                            prjName = System.Web.HttpUtility.HtmlDecode(nameNode[0].ToNodePlainString()).Trim(); 

                            string buildUnit = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                            HtmlTxt = dtlNode.AsHtml().ToLower();
                            inviteCtx = HtmlTxt.GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();

                            Parser imgParser = new Parser(new Lexer(HtmlTxt.ToLower()));
                            NodeList imgNode = imgParser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                            string src = string.Empty;
                            if (imgNode != null && imgNode.Count > 0)
                            {
                                string imgUrl = (imgNode[0] as ImageTag).GetAttribute("src");
                                src = "http://www.jinwan.gov.cn/" + imgUrl;
                                HtmlTxt = HtmlTxt.ToLower().GetReplace(imgUrl, src);
                            }

                            specType = "政府采购";
                            inviteType = prjName.GetInviteBidType();
                            msgType = "珠海市金湾区人民政府";

                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "珠海市区", "金湾区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            if (!string.IsNullOrEmpty(src))
                            {
                                string sql = string.Format("select Id from InviteInfo where InfoUrl='{0}'", info.InfoUrl);
                                object obj = ToolDb.ExecuteScalar(sql);
                                if (obj == null || obj.ToString() == "")
                                {
                                    try
                                    {
                                        BaseAttach attach = ToolHtml.GetBaseAttach(src, prjName, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                        if (attach != null)
                                            ToolDb.SaveEntity(attach, "");
                                    }
                                    catch { }
                                }
                            }
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k].GetATag();
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://www.jinwan.gov.cn/" + a.Link;
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
