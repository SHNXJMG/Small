using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using Crawler;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Util;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteSZLonggang : WebSiteCrawller
    {
        public InviteSZLonggang()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省深圳市龙岗区";
            this.Description = "自动抓取广东省深圳市龙岗区招标信息";
            this.SiteUrl = "http://61.144.224.189:8001/LGjyzxWeb/SiteManage/PublicInfoList.aspx?MenuName=PublicInformation&ModeId=4&ItemId=zbgs&ItemName=%e4%b8%ad%e6%a0%87%e5%85%ac%e7%a4%ba&clearpaging=true";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int sqlCount = 0;
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));

            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_InfoList2_GridViewPaging1_PagingDescTd"), new TagNameFilter("td")));
            string pageString = sNode.AsString();
            Regex regexPage = new Regex(@"，共[^页]+页");
            Match pageMatch = regexPage.Match(pageString);
            try { pageInt = int.Parse(pageMatch.Value.Replace("，共", "").Replace("页", "").Trim()); }
            catch (Exception) { }

            string cookiestr = string.Empty;
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                        new string[] { "ctl00$ScriptManager1",
                            "__EVENTTARGET", 
                            "__EVENTARGUMENT",
                            "__LASTFOCUS", 
                            "__VIEWSTATE", 
                            "ctl00$cph_context$InfoList2$ddlProjectType",
                            "ctl00$cph_context$InfoList2$ddlSearch", 
                            "ctl00$cph_context$InfoList2$txtProjectName", 
                            "ctl00$cph_context$InfoList2$GridViewPaging1$txtGridViewPagingForwardTo", 
                            "__VIEWSTATEENCRYPTED",
                            "ctl00$cph_context$InfoList2$GridViewPaging1$btnForwardToPage" }, 
                        new string[] {
                            "ctl00$cph_context$InfoList2$update1|ctl00$cph_context$InfoList2$GridViewPaging1$btnForwardToPage",
                            string.Empty, 
                            string.Empty,
                            string.Empty,
                            viewState,
                            string.Empty,
                            "gcbh", 
                            string.Empty,
                            i.ToString(),
                            "",
                            "GO" });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }

                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_InfoList2_GridView1"), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j] as TableRow;
                        code = tr.Columns[1].ToPlainTextString().Trim();
                        prjName = tr.Columns[2].ToPlainTextString().Trim();
                        buildUnit = tr.Columns[3].ToPlainTextString().Trim();
                        beginDate = tr.Columns[5].ToPlainTextString().Trim();
                        endDate = tr.Columns[6].ToPlainTextString().Trim();
                        string InvType = tr.Columns[4].ToPlainTextString().Trim();

                        ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://61.144.224.189:8001/LGjyzxWeb/SiteManage/" + aTag.Link.Replace("openNewWindowByMenu(\"", "").Replace("\")", "");
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_lblContent"), new TagNameFilter("span")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = htmldetail.Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                        }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_lblContent"), new TagNameFilter("span")));
                        inviteCtx = dtnode.AsString().Replace("\r\r\n", "\r\n");


                        Regex regPrjAdd = new Regex(@"(工程地点|工程地址)：[^\r\n]+[\r\n]{1}");
                        prjAddress = regPrjAdd.Match(inviteCtx).Value.Replace("工程地点：", "").Replace("工程地址：", "").Trim();
                        msgType = "深圳市建设工程交易中心龙岗分中心";
                        specType = "建设工程";
                        inviteType = ToolHtml.GetInviteTypes(InvType);
                        Regex regOtherType = new Regex(@"(工程类型)：[^\r\n]+[\r\n]{1}");
                        string oType = regOtherType.Match(inviteCtx).Value.Replace("工程类型：", "").Trim();
                        if (oType.Contains("房建"))
                        {
                            otherType = "房建及工业民用建筑";
                        }
                        if (oType.Contains("市政"))
                        {
                            otherType = "市政工程";
                        }
                        if (oType.Contains("园林绿化"))
                        {
                            otherType = "园林绿化工程";
                        }
                        if (oType.Contains("装饰装修"))
                        {
                            otherType = "装饰装修工程";
                        }
                        if (oType.Contains("电力"))
                        {
                            otherType = "电力工程";
                        }
                        if (oType.Contains("水利"))
                        {
                            otherType = "水利工程";
                        }
                        if (oType.Contains("环保"))
                        {
                            otherType = "环保工程";
                        }
                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳龙岗区工程", "龙岗区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, string.Empty, InfoUrl, HtmlTxt);
                        if (!crawlAll && sqlCount >= this.MaxCount) return null;
                            
                        sqlCount++;
                        if (ToolDb.SaveEntity(info, this.ExistCompareFields,this.ExistsUpdate,this.ExistsHtlCtx))
                        {
                            dtlparser.Reset();
                            NodeList fileNode = dtlparser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (fileNode != null && fileNode.Count > 0)
                            {
                                for (int f = 1; f < fileNode.Count; f++)
                                { 
                                    ATag tag = fileNode[f] as ATag;
                                    if (tag.IsAtagAttach())
                                    {
                                        try
                                        {
                                            BaseAttach attach = null;
                                            string url = "http://61.144.224.189:8001/LGjyzxWeb/" + tag.Link.Replace("../", "");
                                            attach = ToolHtml.GetBaseAttachByUrl(url, tag.LinkText, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                            if (attach != null)
                                                ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                                        }
                                        catch { continue; }
                                    }
                                }
                            }
                        }
                       
                    }
                }
            }
            return list;
        }
    }
}
