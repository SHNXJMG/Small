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
using System.Web.Script.Serialization;

namespace Crawler.Instance
{
    public class InviteZhongGuoGJZBW : WebSiteCrawller        
    {
        public InviteZhongGuoGJZBW()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "中国国际招标网招标公告";
            this.Description = "自动抓取中国国际招标网招标公告";
            this.PlanTime = "9:10,11:10,14:10,16:10,18:10";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 100;
            this.SiteUrl = "http://www.chinabidding.com/search/proj.htm";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch { return null; }

            for (int i = 1; i >= pageInt; i++)
            {
                if (i > 1)
                {

                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]
                    {
                    "fullText",
                    "pubDate",
                    "infoClassCodes",
                    "normIndustry",
                    "zoneCode",
                    "fundSourceCodes",
                    "poClass",
                    "rangeType",
                    "currentPage"
                    },
                    new string[] {
                        "",
                        "",
                        "0105",
                        "",
                        "",
                        "",
                        "",
                        "",
                        i.ToString(),
                       });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "as-pager")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                        prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                        specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                        remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                        CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                       
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        //          buildUnit = tr.Columns[1].ToNodePlainString();
                        InfoUrl = aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "as-floor-normal")));
                        parser.Reset();
                        NodeList btNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "as-article")), true), new TagNameFilter("h3")));
                        //if (dtlNode != null && dtlNode.Count > 0)
                        //{
                        //    HtmlTxt = dtlNode.AsHtml();
                        //    parser = new Parser(new Lexer(HtmlTxt));
                        //    NodeList bidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("div"));
                        //    if (bidNode != null && bidNode.Count > 0)
                        //    {
                        //        string ctx = string.Empty;
                        //        TableTag bidTable = bidNode[0] as TableTag;
                        //        for (int r = 0; r < bidTable.RowCount; r++)
                        //        {
                        //            for (int c = 0; c < bidTable.Rows[r].ColumnCount; c++)
                        //            {
                        //                string temp = bidTable.Rows[r].Columns[c].ToNodePlainString();
                        //                if (c % 2 == 0)
                        //                    ctx += temp + "：";
                        //                else
                        //                    ctx += temp + "\r\n";
                        //            }
                        //        }
                        //    }
                        //}
                        prjName = btNode.AsString();
                        HtmlTxt = dtlNode.ToHtml();
                        inviteCtx = HtmlTxt.Replace("</td>", "\r\n").Replace("</tr>", "\r\n").ToCtxString().Replace("\r\n\t", "\r\n").Replace("\r\n\r\n", "\r\n");
                        buildUnit = inviteCtx.GetBuildRegex();
                        code = inviteCtx.GetCodeRegex().GetCodeDel();
                        prjAddress = inviteCtx.GetAddressRegex();

                        inviteCtx = HtmlTxt.ToCtxString();
                        prjAddress = inviteCtx.GetAddressRegex();
                        msgType = "中国国际招标网";
                        specType = "建设工程";
                        inviteType = prjName.GetInviteBidType();
                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                        list.Add(info);
                        parser = new Parser(new Lexer(HtmlTxt));
                        NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        if (aNode != null && aNode.Count > 0)
                        {
                            for (int k = 0; k < aNode.Count; k++) 
                            {
                                ATag a = aNode[k] as ATag;
                                if (a.IsAtagAttach() || a.Link.Contains("downloadfile"))
                                {
                                    string link = string.Empty;
                                    if (a.Link.ToLower().Contains("http"))
                                        link = a.Link;
                                    else
                                        link = "http://183.63.34.189/" + a.Link;
                                    BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                    base.AttachList.Add(attach);
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
