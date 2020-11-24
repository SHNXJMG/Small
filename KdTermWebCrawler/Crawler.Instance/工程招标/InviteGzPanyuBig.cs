using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    /// <summary>
    /// 广州市番禺区
    /// </summary>
    public class InviteGzPanyuBig : WebSiteCrawller
    {
        public InviteGzPanyuBig()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省广州市番禺区大中型项目";
            this.Description = "自动抓取广东省广州市番禺区招标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.SiteUrl = "http://js.panyu.gov.cn/cd_dzxxm.aspx";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder2_lblSumPage")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString();
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {               
                if (i > 1)
                {
                    //__EVENTTARGET:"ctl00$ContentPlaceHolder2$lnkBtnNext"__VIEWSTATEGENERATOR:"96852609"
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                     "__VIEWSTATEGENERATOR",
                    "__EVENTVALIDATION"
                    
                   
                    }, new string[] { 
                    "ctl00$ContentPlaceHolder2$lnkBtnNext",
                    "",
                    viewState,
                     "96852609",
                    eventValidation
                   
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "695")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount-1; j++)
                    {
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();
                        if (aTag == null) continue;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        prjName = aTag.LinkText.ToNodeString();
                        beginDate = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://js.panyu.gov.cn/" + aTag.Link.GetReplace("amp;");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_txtContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.GetReplace("</p>", "\r\n").ToCtxString();
                            buildUnit = inviteCtx.GetBuildRegex();
                            code = inviteCtx.GetReplace(" ").GetCodeRegex().GetCodeDel();
                            prjAddress = inviteCtx.GetReplace(" ").GetAddressRegex();
                            if (buildUnit.Contains("管理局"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("管理局")) + "管理局";
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            msgType = "广州建设工程交易中心";
                            specType  = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "广州市区", "番禺区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag fileTag = aNode[a] as ATag;
                                    if (fileTag.IsAtagAttach())
                                    {
                                        string fileUrl = string.Empty;
                                        if (fileTag.Link.Contains("http"))
                                            fileUrl = fileTag.Link;
                                        else
                                            fileUrl = "http://js.panyu.gov.cn/" + fileTag.Link;

                                        base.AttachList.Add(ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, fileUrl));
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
