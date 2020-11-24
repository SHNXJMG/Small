﻿using System.Text;
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

namespace Crawler.Instance
{
    public class InviteZheJiangJsgc:WebSiteCrawller
    {
        public InviteZheJiangJsgc()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "浙江省公共资源交易中心招标信息";
            this.Description = "自动抓取浙江省公共资源交易中心招标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.zmctc.com/zjgcjy/Notice/AfficheInfoMore.aspx";
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
            catch { }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "AfficheInfoList1_Pager")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToNodePlainString().GetRegexBegEnd("1/", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html); 
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__VIEWSTATE",
                    "AfficheInfoList1:KeyWord",
                    "__EVENTTARGET",
                    "__EVENTARGUMENT" 
                    }, new string[]{
                    viewState,
                    "",
                    "AfficheInfoList1:Pager",
                    i.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "AfficheInfoList1_DataGrid1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                        prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                        specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                        remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                        CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.GetAttribute("title").GetReplace(";");
                        code = tr.Columns[1].ToNodePlainString().GetReplace("[", "【").GetReplace("]", "】").GetRegexBegEnd("【", "】"); 
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link;
                        if (!InfoUrl.Contains("http"))
                        {
                            Logger.Error(prjName);
                            continue;
                        }
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("align", "center")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            msgType = "浙江省公共资源交易中心";
                            specType = "政府采购";
                            inviteType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("浙江省", "浙江省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
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
                                            link = "http://downc.zmctc.com/" + a.Link;
                                        if (Encoding.Default.GetByteCount(link) > 500)
                                            continue;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            } 
                        }
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
