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
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteHbHuangGang : WebSiteCrawller
    {
        public InviteHbHuangGang()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "黄冈市公共资源交易中心信息招标公告";
            this.Description = "自动抓取黄冈市公共资源交易中心信息招标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://www.hgggzy.com/ceinwz/WebInfo_List.aspx?&newsid=700&jsgc=0100000&zbdl=1&FromUrl=jygg";
            this.MaxCount = 120;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookie = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default, ref cookie);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_myGV_ctl23_LabelPageCount")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToNodePlainString();
                    pageInt = Convert.ToInt32(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "ctl00_myTreeView_ExpandState",
                    "ctl00_myTreeView_SelectedNode",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "ctl00_myTreeView_PopulateLog",
                    "__LASTFOCUS",
                    "__VIEWSTATE",
                    "__VIEWSTATEENCRYPTED",
                    "__EVENTVALIDATION",
                    "ctl00$ContentPlaceHolder1$txtGcmc",
                    "ctl00$ContentPlaceHolder1$DDLPageSize"
                    }, new string[]{
                    "ennnnn",
                    "ctl00_myTreeViewt1",
                    "ctl00$ContentPlaceHolder1$myGV$ctl23$LinkButtonNextPage",
                    "",
                    "",
                    "",
                    viewState,
                    "",
                    eventValidation,
                    "",
                    "20"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default,ref cookie);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_myGV")));
                if (viewList != null && viewList.Count > 0)
                {
                    TableTag table = viewList[0] as TableTag;
                    for (int j = 1; j < table.RowCount-1; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        code = tr.Columns[0].ToNodePlainString();
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.LinkText.ToNodeString().GetReplace(" ,[查看公告]");
                        InfoUrl = "http://www.hgggzy.com/ceinwz/" + aTag.Link;
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl)); 
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "newsImage")));
                        if (dtl == null || dtl.Count < 1)
                        {
                            parser.Reset();
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "pnlZbgg")),true),new TagNameFilter("a")));
                            if (aNode != null && aNode.Count > 0)
                            {
                                ATag dtlTag = null;
                                for (int a = 0; a < aNode.Count; a++)
                                { 
                                     dtlTag = aNode[a].GetATag();
                                     if (dtlTag.Link.Contains(".doc"))
                                         break;
                                }
                                 
                                string link = "http://www.hgggzy.com/WordHtml/BestHtml.aspx?id=" + dtlTag.Link.GetReplace("/doc/");
                                try
                                {
                                    htlDtl = this.ToolWebSite.GetHtmlByUrl(link, Encoding.Default).GetJsString();
                                }
                                catch { continue; }
                                parser = new Parser(new Lexer(htlDtl));
                                dtl = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                            }
                        }
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml().ToLower();
                            inviteCtx = HtmlTxt.GetReplace("</p>,</br>,<br>,</div>", "\r\n").ToCtxString();
                            inviteType = prjName.GetInviteBidType();


                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                            if (buildUnit.Contains("代理机构"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("代理机构"));

                            msgType = "黄冈市公共资源交易中心";

                            specType = "建设工程";


                            InviteInfo info = ToolDb.GenInviteInfo("湖北省", "湖北省及地市", "黄冈市", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://www.hgggzy.com/" + a.Link;
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
