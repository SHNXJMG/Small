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
    public class InviteBeiJing : WebSiteCrawller
    {
        public InviteBeiJing()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "北京市建设工程发包承包交易中心招标信息(施工)";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取北京市建设工程发包承包交易中心招标信息(施工)";
            this.SiteUrl = "http://www.bcactc.com/home/gcxx/now_sgzbgg.aspx";
            this.MaxCount = 400;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "lblPageCount")));
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
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__LASTFOCUS",
                    "__VIEWSTATE",
                    "__VIEWSTATEGENERATOR",
                    "__EVENTVALIDATION",
                    "gcbh_Text_Box",
                    "gcmc_TextBox",
                    "num_TextBox",
                    "ImageButton3.x",
                    "ImageButton3.y"
                    }, new string[]{
                    "","","",
                    viewState,
                    "B0108473",
                    eventValidation,
                    "","","",
                    "5","12"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "DataGrid1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        if (aTag == null) continue;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        code = tr.Columns[0].ToNodePlainString();
                        prjName = aTag.LinkText.GetReplace(" ");
                        buildUnit = tr.Columns[3].ToNodePlainString();
                        endDate = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.bcactc.com/home/gcxx/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "hei_text")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            TableTag dtlTable = dtlNode[0] as TableTag;
                            HtmlTxt = dtlTable.ToHtml();
                            for (int r = 0; r < dtlTable.RowCount; r++)
                            {
                                for (int c = 0; c < dtlTable.Rows[r].ColumnCount; c++)
                                {
                                    if ((c + 1) % 2 == 0)
                                        inviteCtx += dtlTable.Rows[r].Columns[c].ToNodePlainString() + "\r\n";
                                    else
                                        inviteCtx += dtlTable.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：") + "：";
                                }
                            }
                            if (code.Contains(".."))
                                code = inviteCtx.GetCodeRegex();
                            prjAddress = inviteCtx.GetAddressRegex().GetCodeDel();
                            beginDate = inviteCtx.GetRegex("发布日期,发布时间").GetDateRegex();
                            if (string.IsNullOrEmpty(beginDate))
                                beginDate = inviteCtx.GetDateRegex();
                            if (string.IsNullOrEmpty(beginDate))
                                beginDate = inviteCtx.GetDateRegex("yyyy/MM/dd");
                            if (string.IsNullOrEmpty(beginDate))
                                beginDate = inviteCtx.GetDateRegex("yyyy年MM月dd日");
                            msgType = "北京市建设工程发包承包交易中心";
                            specType = "建设工程";
                            inviteType = "施工";
                            InviteInfo info = ToolDb.GenInviteInfo("北京市", "北京市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }

                    }
                }
            }
            return list;
        }
    }
}
