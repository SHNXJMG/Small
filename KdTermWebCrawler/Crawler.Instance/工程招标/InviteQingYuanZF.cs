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
    public class InviteQingYuanZF : WebSiteCrawller
    {
        public InviteQingYuanZF()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省清远市住房与城市建设招标信息";
            this.Description = "自动抓取广东省清远市住房与城市建设招标信息";
            this.ExistCompareFields = "Code,ProjectName,InfoUrl";
            this.SiteUrl = "http://www.qyggzy.cn/webIndex/newsLeftBoard//0102/010201";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            int pageInt = 1;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("colspan", "5")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                string temp = pageNode[pageNode.Count - 1].GetATagHref();
                try
                {
                    temp = temp.GetReplace("goPage(,)");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "fpid",
                    "fid",
                    "ftitle",
                    "totalRows",
                    "pageNO"
                    },new string[]{
                    "",
                    "",
                    "",
                    "",
                    i.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
                    }
                    catch { continue;  }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "newtable")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount-2; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string code = string.Empty, buildUnit = string.Empty,prjName=string.Empty,
                              prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                              specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                              remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                              CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = tr.Columns[0].GetATag();
                        prjName = aTag.LinkText.ToNodeString().GetReplace(" ");
                        string[] prjNames = prjName.Split('、');
                        if (prjNames.Length > 1)
                            prjName = prjNames[1];

                        beginDate = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        InfoUrl =  aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "maincont")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList prjNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "toptd1")));
                            if (prjNode != null && prjNode.Count > 0)
                            {
                                prjName = prjNode.AsString().ToNodeString();
                            }
                            inviteCtx = HtmlTxt.ToCtxString();

                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("联系人") || buildUnit.Contains("代理机构") || buildUnit.Contains("电话"))
                                buildUnit = "";
                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex();

                            inviteType = prjName.GetInviteBidType();

                            msgType = "清远市公共资源交易中心";
                            specType = "建设工程";

                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "清远市区", string.Empty, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag fileTag = aNode[a] as ATag;
                                    if (fileTag.IsAtagAttach())
                                    { 
                                        string fileUrl=string.Empty;
                                        if (fileTag.Link.Contains("http"))
                                            fileUrl = fileTag.Link;
                                        else
                                            fileUrl = "http://www.qyggzy.cn/" + fileTag.Link;

                                        base.AttachList.Add(ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, fileUrl));
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
