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
    public class InviteHaiErBin : WebSiteCrawller
    {
        public InviteHaiErBin()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "哈尔滨市建设工程信息网招标信息";
            this.Description = "自动抓取哈尔滨市建设工程信息网招标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400; 
            this.SiteUrl = "http://www.hrbjjzx.cn/Bid_Front/ZBMore.aspx?t=%E5%85%A8%E9%83%A8";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1000;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl,Encoding.UTF8,ref cookiestr);
            }
            catch { return list; }
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
                    "__VIEWSTATEENCRYPTED",
                    "__EVENTVALIDATION",
                    "searchIndex1$tbx_Content",
                    "searchIndex1$ddl_Type"
                    }, new string[]{
                    "GV_Data",
                    "Page$"+i,
                    viewState,
                    "",
                    eventValidation,
                    "--标题关键字--",
                    "4"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GV_Data")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount - 1; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.LinkText;
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        string htmldtl = string.Empty;
                        string postid = aTag.GetAttribute("href").GetRegexBegEnd("'","'");
                        try
                        {
                            htmldtl = System.Web.HttpUtility.HtmlDecode(GetHtml(html, postid).GetJsString());
                        }
                        catch { }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "3")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            parser.Reset();
                            NodeList formNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("form"), new HasAttributeFilter("id", "form1")));
                            if (formNode != null && formNode.Count > 0)
                            {
                                InfoUrl = "http://www.hrbjjzx.cn/Bid_Front/" + (formNode[0] as FormTag).GetAttribute("action");
                            }
                            else
                                continue;
                            HtmlTxt = dtlNode.AsHtml().GetReplace("<br>", "<br />");
                            inviteCtx = HtmlTxt.ToLower().GetReplace("<br/>,<br>,</p>", "\r\n").GetReplace("untitleddocument").ToCtxString().GetReplace("untitleddocument, , ");
                            TableTag tag = dtlNode[dtlNode.Count - 1] as TableTag;
                            string ctx = string.Empty;
                            for (int r = 0; r < tag.RowCount; r++)
                            {
                                if (r > 2) break;
                                for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                {
                                    string temp = tag.Rows[r].Columns[c].ToNodePlainString();
                                    if ((c + 1) % 2 == 0)
                                        ctx += temp.GetReplace(":,：, ") + "\r\n";
                                    else
                                        ctx += temp.GetReplace(":,：, ") + "：";
                                }
                            }
                            if (prjName.Contains("..."))
                            {
                                prjName = ctx.GetRegex("项目");
                            }
                            buildUnit = ctx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex().GetReplace("　");
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            if (string.IsNullOrEmpty(code))
                                code = inviteCtx.GetRegex("编码");
                            msgType = "哈尔滨建设工程交易中心";
                            specType = inviteType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("黑龙江省", "黑龙江省及地市", "哈尔滨市", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                             
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }

                    }
                }
            }
            return list;
        }

        private string GetHtml(string html, string postId)
        {
            string viewState = this.ToolWebSite.GetAspNetViewState(html);
            string eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
            NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__VIEWSTATE",
                        "__VIEWSTATEENCRYPTED",
                        "__EVENTVALIDATION",
                        "searchIndex1$tbx_Content",
                        "searchIndex1$ddl_Type"
                        }, new string[]{
                        postId,
                        "",
                        viewState,
                        "",
                        eventValidation,
                          "--标题关键字--",
                    "4"
                        }); 
            return this.ToolWebSite.GetHtmlByUrl(this.SiteUrl,nvc, Encoding.UTF8);
        }
    }
}
