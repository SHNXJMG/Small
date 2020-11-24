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
    public class InviteZhXiangZhou : WebSiteCrawller
    {
        public InviteZhXiangZhou()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省珠海市香洲区教育局信息招标信息";
            this.Description = "自动抓取珠海市香洲区教育局信息招标信息";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://xzedu.zhuhai.gov.cn/StyleList1.aspx?parentID=1080000&ID=1080100";
            this.MaxCount = 150;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl,Encoding.Default);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "cNavBar_cTotalPages")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToNodePlainString();
                    pageInt = int.Parse(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                    "__VIEWSTATE",
                    "__VIEWSTATEGENERATOR",
                    "__EVENTVALIDATION",
                    "cSortField",
                    "cSortDirection",
                    "cID",
                    "cParentID",
                    "cLeft:cParentID",
                    "cLeft:cID",
                    "cNavBar:cPageIndex"
                    }, new string[] { 
                    viewState,
                    "8A9C3F4D",
                    eventValidation,
                    "",
                    "",
                    "1080100",
                    "1080000",
                    "1080000",
                    "1080100",
                    i.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list")), true), new TagNameFilter("li")));

                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty,
                  prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                  specType = string.Empty, endDate = string.Empty,
                  remark = string.Empty, inviteCon = string.Empty,
                  CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;


                        ATag aTag = viewList[j].GetATag();

                        string beginDate = viewList[j].ToPlainTextString().GetDateRegex();
                        string prjName = aTag.GetAttribute("title");
                        string InfoUrl = "http://xzedu.zhuhai.gov.cn/" + aTag.Link.GetReplace("./");
                        string htmDtl = string.Empty;
                        try
                        {
                            htmDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "news_view_main")), true), new TagNameFilter("li")));
                        if (dtl != null && dtl.Count > 1)
                        {  
                            HtmlTxt = dtl.AsHtml().ToLower();
                            inviteCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();

                            buildUnit = inviteCtx.GetBuildRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                              
                            

                            specType = "政府采购";
                            inviteType = prjName.GetInviteBidType();
                            msgType = "珠海市香洲区教育局";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "珠海市区", "香洲区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
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
                                            link = "http://xzedu.zhuhai.gov.cn/" + a.Link;
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
