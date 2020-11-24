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
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Data;

namespace Crawler.Instance
{
    public class InviteSzNanShan : WebSiteCrawller
    {
        public InviteSzNanShan()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "深圳市政府采购南山区小型工程招标信息";
            this.Description = "自动抓取深圳市政府采购南山区小型工程招标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.SiteUrl = "http://www.szns.gov.cn/cgzx/xxgk89/fwzl58/xxjsgczbgg93/index.html";
            this.MaxCount = 800;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hyxma03")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("/", "跳").Replace(" ", "");//.Replace("","");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 0; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.szns.gov.cn/cgzx/xxgk89/fwzl58/xxjsgczbgg93/21211-" + i + ".html");
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "hyxmalb")), true), new TagNameFilter("table")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty,
         inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty,
         endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();
                        prjName = aTag.GetAttribute("title");
                        InfoUrl = "http://www.szns.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "jwRercon")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            inviteType = prjName.GetInviteBidType();
                            msgType = "深圳市南山区政府采购及招标中心";
                            specType = "政府采购";

                            beginDate = inviteCtx.GetRegex("时间").GetDateRegex();
                            if (string.IsNullOrEmpty(beginDate))
                                beginDate = inviteCtx.GetRegex("时间").GetDateRegex("yyyy年MM月dd日");
                            if (string.IsNullOrEmpty(beginDate))
                                beginDate = DateTime.Now.ToShortDateString();

                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳政府采购", "南山区", "", code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList fileNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (fileNode != null && fileNode.Count > 0)
                            {
                                for (int k = 0; k < fileNode.Count; k++)
                                {
                                    ATag aNode = fileNode[k].GetATag();
                                    if (aNode.IsAtagAttach())
                                    {
                                        BaseAttach attach = ToolDb.GenBaseAttach(aTag.LinkText, info.Id, "http://www.szns.gov.cn" + aTag.Link);
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
