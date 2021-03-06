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


namespace Crawler.Instance
{
    public class InviteSzgxzb : WebSiteCrawller
    {
        public InviteSzgxzb()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "深圳市国信招标有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳市国际招标有限公司招标信息"; 
            this.SiteUrl = "http://www.szgxzb.com/zbgg/Index.aspx?columnId=379";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {


                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);

            }
            catch (Exception ex)
            {

                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "anp1")));
            if (tdNodes != null)
            {
                string pageTemp = tdNodes.AsString().Replace("&nbsp;", "").Trim();
                Regex regpage = new Regex(@"当前第[^页]+页");
                try
                {
                    pageInt = int.Parse(regpage.Match(pageTemp).Value.Split('/')[1].Replace("页", "").Trim());
                }
                catch (Exception ex) { }
                string cookiestr = string.Empty;
                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { 
                           "anp1_input",
                           "Columnlist2_DepartmentTreeView_CheckedList",
                            "Columnlist2_DepartmentTreeView_EditEvents",
                            "Columnlist2_DepartmentTreeView_ExpandedList",
                            "Columnlist2_DepartmentTreeView_MoveEvents",
                            "Columnlist2_DepartmentTreeView_MultipleSelectedList",
                            "Columnlist2_DepartmentTreeView_ScrollData",
                            "Columnlist2_DepartmentTreeView_SelectedNode",
                            "Columnlist2_DepartmentTreeView_ValueChangeEvents",
                            "Login2:txtPassword",
                            "Login2:txtUserName",
                            "__EVENTARGUMENT",
                            "__EVENTTARGET",
                            "__VIEWSTATE"
                        }, new string[] { 
                             (i-1).ToString(),
                             string.Empty,
                             string.Empty,
                             string.Empty,
                             string.Empty,
                             string.Empty,
                             "0,0",
                             "p_379",
                             string.Empty,
                             string.Empty,
                             string.Empty,
                             i.ToString(),
                             "anp1",
                             viewState
                        });

                        try { html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default, ref cookiestr); }
                        catch (Exception ex) { continue; }
                    }
                    Regex regHTML1 = new Regex(@"<td>[^<]+<td>");
                    Regex regHTML2 = new Regex(@"</td>[^<]+</td>");
                    html = regHTML2.Replace(regHTML1.Replace(html, "<td>"), "</td>");
                    parser = new Parser(new Lexer(html));
                    NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "dlstNews")));
                    if (nodeList != null)
                    {
                        if (nodeList != null && nodeList.Count > 0)
                        {
                            TableTag table = nodeList[0] as TableTag;
                            for (int j = 0; j < table.RowCount; j++)
                            {
                                string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt=string.Empty;
                                TableRow tr = table.Rows[j];

                                beginDate = tr.Columns[1].ToPlainTextString().Trim();
                                ATag aTag = tr.Columns[0].SearchFor(typeof(ATag), true)[0] as ATag;
                                InfoUrl = "http://www.szgxzb.com/zbgg/" + aTag.Link;
                                string htmldetail = string.Empty;
                                try
                                {
                                    htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Trim();
                                    Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                                    NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "newsContent"), new TagNameFilter("div"))); 
                                    HtmlTxt = dtnodeHTML.AsHtml();
                                    htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                                    Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                                    htmldetail = regexHtml.Replace(htmldetail, "");
                                }
                                catch (Exception ex) { continue; }
                                Parser dtlparser = new Parser(new Lexer(htmldetail));
                                NodeList prjNameNode = dtlparser.ExtractAllNodesThatMatch(new TagNameFilter("title"));
                                prjName = prjNameNode.AsString().Replace("国信招标--", "");
                                dtlparser.Reset();
                                NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "newsContent"), new TagNameFilter("div"))); 
                                
                                inviteCtx = dtnode.AsString();
                                Regex regcode = new Regex(@"（招标编号：[^）]+）");
                                code = regcode.Match(inviteCtx).Value.Replace("招标编号", "").Replace("（", "").Replace("）", "").Replace("：", "").Trim();
                                if (Encoding.Default.GetByteCount(code) > 50)
                                {
                                    code = "";
                                }
                                specType = "其他";
                                msgType = "深圳市国信招标有限公司";
                                inviteType = ToolHtml.GetInviteTypes(prjName);
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }

                        }
                    }
                }

            }
            return list;
        }

    }
}
