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
using System.Web.UI.MobileControls;
using System.Collections.Generic;
namespace Crawler.Instance
{
    public class InviteYunNanJst : WebSiteCrawller
    {
        public InviteYunNanJst()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "云南省建设工程招投标监督管理网招标信息";
            this.Description = "自动抓取云南省建设工程招投标监督管理网招标信息";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.ynzb.com.cn/Project_FindContractor.aspx";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "LblPageCount")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString();
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
                    "TBuildInc",
                    "TFindContractorName",
                    "SArea",
                    "SCCSort",
                    "txtGO",
                    "__EVENTVALIDATION"
                    },new string[]{
                    "lbtnGO",
                    "",
                    viewState,
                    "","",
                    "0",
                    "",
                    i.ToString(),
                    eventValidation
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl,nvc);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "gv_List")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                           prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                           specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                           remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                           CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty,
                           city = string.Empty;
                        TableRow tr = table.Rows[j];
                        buildUnit = tr.Columns[1].ToNodePlainString();
                        ATag aTag = tr.Columns[2].GetATag();
                        prjName = aTag.GetAttribute("title");
                        inviteType = tr.Columns[3].ToNodePlainString();
                        beginDate = tr.Columns[5].ToPlainTextString().GetDateRegex();
                        city = tr.Columns[6].ToNodePlainString();
                        InfoUrl = "http://www.ynzb.com.cn/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = ToolHtml.GetHtmlByUrl(this.SiteUrl,InfoUrl,Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellspacing", "1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml(); 
                            TableTag tag = dtlNode[0] as TableTag;
                            for (int r = 0; r < tag.RowCount; r++)
                            {
                                if (r == 0)
                                {
                                    inviteCtx += tag.Rows[r].Columns[0].ToNodePlainString() + "\r\n";
                                    if (string.IsNullOrWhiteSpace(prjName))
                                        prjName = tag.Rows[r].Columns[0].ToNodePlainString();
                                    continue;
                                }
                                for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                {
                                    string temp = tag.Rows[r].Columns[c].ToNodePlainString(); 
                                    if ((c + 1) % 2 == 0)
                                        inviteCtx += temp + "\r\n";
                                    else
                                        inviteCtx += temp + "：";
                                }
                            }
                             
                            code = inviteCtx.GetCodeRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            if (buildUnit.Contains(".."))
                            {
                                string temp = inviteCtx.GetBuildRegex();
                                buildUnit = !string.IsNullOrEmpty(temp) ? temp : buildUnit.Replace(".","");
                            }
                            specType = "建设工程";
                            msgType = "云南省住房和城乡建设厅";
                            InviteInfo info = ToolDb.GenInviteInfo("云南省", "云南省及地市", city, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://www.ynzb.com.cn/" + a.Link;
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
