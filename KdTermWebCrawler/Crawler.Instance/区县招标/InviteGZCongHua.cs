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

namespace Crawler.Instance
{
    public class InviteGZCongHua : WebSiteCrawller
    {
        public InviteGZCongHua()
            : base()
        {
            this.Group = "区县招标信息";
            this.Title = "广东省广州从化市城乡建设局";
            this.Description = "自动抓取广东省广州从化市城乡建设局";
            this.PlanTime = "9:34,10:36,14:34,16:36";
            this.SiteUrl = "http://www.chcxjs.gov.cn/NewsClassztbgl1.asp?bigclass=招投标管理&SmallClass=招标公告";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
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
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "50")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString();
                    Regex reg = new Regex(@"/[^页]+页");
                    string page = reg.Match(temp).Value.Replace("/", "").Replace("页", "");
                    pageInt = Convert.ToInt32(page);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl) + "&page=" + i.ToString(), Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList tabList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                if (tabList != null && tabList.Count > 0)
                {
                    TableTag table = tabList[4] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                   prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                   specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                   remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                   CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[1].ToPlainTextString().ToNodeString();
                        inviteType = tr.Columns[0].ToPlainTextString();
                        beginDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.chcxjs.gov.cn/" + tr.Columns[1].ToHtml().GetATagHref();
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                            htlDtl = htlDtl.GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            HtmlTxt = dtlList[3].ToHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            string ctxHtml = HtmlTxt;
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aLists = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aLists != null && aLists.Count > 0)
                            {
                                for (int c = 0; c < aLists.Count; c++)
                                {
                                    ATag a = aLists[c] as ATag;
                                    if (a.Link.IsAtagAttach())
                                    {
                                        string alink = "http://www.chcxjs.gov.cn" + a.Link;
                                        HtmlTxt = HtmlTxt.Replace(a.Link, alink);
                                    }
                                }
                            }
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("联系人"))
                            {
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系人")).Replace("联系人", "");
                            }
                            code = inviteCtx.GetCodeRegex();
                            msgType = "从化市城乡建设局";
                            
                            specType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "广州市区", "化州市", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(ctxHtml));
                            NodeList aList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aList != null && aList.Count > 0)
                            {
                                for (int c = 0; c < aList.Count; c++)
                                {
                                    ATag a = aList[c] as ATag;
                                    if (a.Link.IsAtagAttach())
                                    {
                                        string alink = "http://www.chcxjs.gov.cn" + a.Link.Replace("./", "");
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, alink);
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
