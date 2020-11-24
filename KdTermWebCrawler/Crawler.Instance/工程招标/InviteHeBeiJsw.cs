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

namespace Crawler.Instance
{
    public class InviteHeBeiJsw : WebSiteCrawller
    {
        public InviteHeBeiJsw()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "河北建设工程信息网招标信息";
            this.Description = "自动抓取河北建设工程信息网招标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.hebgc.com/hebgc2009/?a=eng-anc-list";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "fl_lt_pager_bottom")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("第1/", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&p=" + i);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter( new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "fl_lt_div")),true),new TagNameFilter("table")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                       prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                       specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                       remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                       CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j]; 
                        ATag aTag = tr.Columns[0].GetATag();
                        prjName = aTag.LinkText;
                        beginDate = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.hebgc.com/hebgc2009/" + aTag.Link.GetReplace("amp;");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "eng_anc_detail single_line")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            TableTag tableTag = dtlNode[0] as TableTag;
                            for (int r = 0; r < tableTag.RowCount; r++)
                            {
                                for (int c = 0; c < tableTag.Rows[r].ColumnCount; c++)
                                {
                                    string temp = tableTag.Rows[r].Columns[c].ToNodePlainString();
                                    if (string.IsNullOrWhiteSpace(temp)) continue;
                                    if ((c + 1) % 2 == 0)
                                        if (temp.Length > 500)
                                            inviteCtx += tableTag.Rows[r].Columns[c].ToHtml().GetReplace("</p>,<br/>", "\r\n").ToCtxString() + "\r\n";
                                        else
                                            inviteCtx += temp.GetReplace(":,：") + "\r\n";
                                    else
                                        inviteCtx += temp.GetReplace(":,：") + "：";
                                }
                            }
                            prjAddress = inviteCtx.GetAddressRegex();
                           
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            msgType = "河北省建设工程招投标管理办公室";
                            specType = inviteType = "建设工程"; 
                            InviteInfo info = ToolDb.GenInviteInfo("河北省", "河北省及地市", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://www.hebgc.com/" + a.Link.GetReplace("../,./");
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
