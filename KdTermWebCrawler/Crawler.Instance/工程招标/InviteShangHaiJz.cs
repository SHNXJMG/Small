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
    public class InviteShangHaiJz : WebSiteCrawller
    {
        public InviteShangHaiJz()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "上海市建筑建材业招标信息";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取上海市建筑建材业招标信息";
            this.SiteUrl = "https://www.ciac.sh.cn/NetInterBidweb/GKTB/SgfbZbxx.aspx";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("id", "DropDownList_page")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    SelectTag tag = pageNode[0] as SelectTag;
                    string temp = tag.OptionTags[tag.OptionTags.Length - 1].Value;
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
                    "__VIEWSTATE",
                    "__EVENTVALIDATION",
                    "dr_gglb",
                    "txt_beginTime",
                    "txt_endTime",
                    "nextPages",
                    "DropDownList_page",
                    "hdInputNum",
                    "hdPageCount",
                    "hdState"
                    }, new string[]{
                    "","",
                    viewState,
                    eventValidation,
                    "0","","",
                    "下一页",
                    "1",
                    "1",
                    pageInt.ToString(),
                    ""
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("style", "text-align: center;")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        if (aTag == null) continue;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;


                        prjName = aTag.LinkText.ToNodeString().GetReplace(" ");
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex("yyyy/MM/dd");
                        InfoUrl = "https://www.ciac.sh.cn/NetInterBidweb/GKTB/DefaultV2011.aspx?gkzbxh=" + aTag.GetAttribute("onclick").GetRegexBegEnd("'", "'");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table_css")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            TableTag dtlTable = dtlNode[0] as TableTag;
                            HtmlTxt = dtlTable.ToHtml();//dtlNode.AsHtml();
                            inviteCtx = "";
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
                            prjAddress = inviteCtx.GetAddressRegex().GetCodeDel();
                            buildUnit = inviteCtx.GetBuildRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            msgType = "上海市建筑业管理办公室";
                            specType = inviteType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("上海市", "上海市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
