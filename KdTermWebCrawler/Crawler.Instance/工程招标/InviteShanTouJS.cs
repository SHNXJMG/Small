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
    public class InviteShanTouJS : WebSiteCrawller
    {
        public InviteShanTouJS()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省汕头市建设工程招标信息";
            this.Description = "自动抓取广东省汕头建设工程招标信息";
            this.ExistCompareFields = "ProjectName,InfoUrl";
            this.SiteUrl = "http://www.stjs.org.cn/zbtb/zhaobiao_gonggao.asp";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
        }


        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "34")));
            if (nodeList != null && nodeList.Count > 0)
            {
                string pageString = nodeList.AsString();
                Regex regexPage = new Regex(@"1/[^页]+");
                Match pageMatch = regexPage.Match(pageString);
                try
                {
                    page = int.Parse(pageMatch.Value.Replace("1/", "").Replace("下一", ""));
                }
                catch { page = 1; }
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "?page=" + i.ToString()), Encoding.Default);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellPadding", "5")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                           prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                           specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                           remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                           CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[1].ToPlainTextString().Replace("&#8226;", "").Trim();
                        beginDate = tr.Columns[2].ToPlainTextString().Replace("&nbsp; ", "").Trim();
                        ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://www.stjs.org.cn/zbtb/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "");
                        }
                        catch
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellPadding", "4")));
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            TableTag tableRow = dtnode.SearchFor(typeof(TableTag), true)[0] as TableTag;
                            for (int row = 0; row < tableRow.RowCount; row++)
                            {
                                TableRow r = tableRow.Rows[row];

                                for (int k = 0; k < r.ColumnCount; k++)
                                {
                                    string st = string.Empty;
                                    string st1 = string.Empty;
                                    st = r.Columns[k].ToPlainTextString().Trim();
                                    if (k + 1 < r.ColumnCount)
                                    {
                                        st1 = r.Columns[k + 1].ToPlainTextString().Trim();
                                    }
                                    inviteCtx += st + "：" + st1 + "\r\n";
                                    if (k + 1 <= r.ColumnCount)
                                    {
                                        k++;
                                    }
                                }
                            }
                            code = inviteCtx.GetCodeRegex().GetReplace("/"); ;
                            Regex regBuidUnit = new Regex(@"(招标人|建设单位)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(inviteCtx).Value.Replace("招标人：", "").Replace("建设单位:", "").Trim();
                            Regex regPrjAddr = new Regex(@"(工程地点|项目地址)(：|:)[^\r\n]+\r\n");
                            prjAddress = regPrjAddr.Match(inviteCtx).Value.Replace("工程地点：", "").Trim();
                            inviteCtx = inviteCtx.Replace("<?", "").Replace("xml:namespace prefix = st1 ns = ", "").Replace("urn:schemas-microsoft-com:office:smarttags", "").Replace("/>", "").Replace("：", "").Trim();
                            inviteCtx = inviteCtx.Replace("<?", "").Replace("xml:namespace prefix = o ns = ", "").Replace("urn:schemas-microsoft-com:office:office", "").Replace("/>", "").Replace("：", "").Trim();
                            msgType = "汕头市建设工程交易中心";
                            specType = "建设工程";

                            string[] prjNames = prjName.Split(':');
                            prjName = prjNames[prjNames.Length - 1];
                            beginDate = beginDate.GetReplace(".", "-");
                            string temp = inviteCtx.GetRegex("工程名称", false);
                            if (!string.IsNullOrWhiteSpace(temp))
                                prjName = temp;
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "汕头市区", "",
                             string.Empty, code, prjName, prjAddress, buildUnit,
                             beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parserdetail.Reset();
                            NodeList fileNode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "30")), true), new TagNameFilter("a")));
                            if (fileNode.Count > 0)
                            {
                                for (int f = 0; f < fileNode.Count; f++)
                                {
                                    ATag aTa3g = fileNode[f] as ATag;
                                    BaseAttach attach = ToolDb.GenBaseAttach(aTa3g.LinkText, info.Id, "http://www.stjs.org.cn/zbtb/" + aTa3g.Link);
                                    base.AttachList.Add(attach);
                                }

                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return null;
        }
    }
}
