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
    public class BidQingYuanZF : WebSiteCrawller
    {
        public BidQingYuanZF()
            : base() 
        { 
            this.Group = "中标信息";
            this.Title = "广东省清远市住房与城市建设中标信息";
            this.Description = "自动抓取广东省清远市住房与城市建设中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "Code,ProjectName,InfoUrl";
            this.SiteUrl = "http://www.qyggzy.cn/webIndex/newsLeftBoard//0102/010202";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
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
                    }, new string[]{
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
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "newtable")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount-2; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
             bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

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
                            bidCtx = HtmlTxt.ToCtxString();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList prjNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"),new HasAttributeFilter("class", "toptd1")));
                            if (prjNode != null && prjNode.Count > 0)
                            {
                                prjName = prjNode.AsString().ToNodeString();
                            }
                            string ctx = string.Empty;
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter( new TagNameFilter("div"),new HasAttributeFilter("id","context_div")),true),new TagNameFilter("table")));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                int colIndex = 0;
                                ctx = string.Empty;
                                TableTag dtlTable = tableNode[0] as TableTag;
                                for (int r = 1; r < dtlTable.RowCount; r++)
                                {
                                    if (dtlTable.Rows[r].ColumnCount > 1)
                                    {
                                        string temp1 = dtlTable.Rows[r].Columns[0].ToNodePlainString();
                                        if (string.IsNullOrEmpty(temp1))
                                        {
                                            colIndex = 1;
                                            temp1 = dtlTable.Rows[r].Columns[colIndex].ToNodePlainString();
                                        }
                                        else
                                            colIndex = 0;
                                        string temp2 = string.Empty;
                                        string mgr = dtlTable.Rows[r].ToNodePlainString();
                                        if (colIndex > 0)
                                        {
                                            if (dtlTable.Rows[r].ColumnCount > 2)
                                            {
                                                
                                                if (mgr.Contains("经理") || mgr.Contains("总监") || mgr.Contains("负责人"))
                                                    temp2 = dtlTable.Rows[r].Columns[colIndex + 1].ToHtml().GetReplace("</span>,</p>", "\r\n").ToCtxString();
                                                else
                                                    temp2 = dtlTable.Rows[r].Columns[colIndex + 1].ToNodePlainString();

                                            }
                                        }
                                        else
                                        { 
                                            if (mgr.Contains("经理") || mgr.Contains("总监") || mgr.Contains("负责人"))
                                                temp2 = dtlTable.Rows[r].Columns[colIndex + 1].ToHtml().GetReplace("</span>,</p>","\r\n").ToCtxString();
                                            else
                                                temp2 = dtlTable.Rows[r].Columns[colIndex + 1].ToNodePlainString();
                                        }
                                        ctx += temp1 + "：";
                                        ctx += temp2 + "\r\n";
                                    }
                                }
                                ctx = ctx.GetReplace("&nbsp,&nbsp;");
                                while (true)
                                {
                                    ctx = ctx.GetReplace("：\r,：\n,：\t", "：");
                                    if (!ctx.Contains("：\r") && !ctx.Contains("：\n") && !ctx.Contains("：\t"))
                                        break;
                                }
                                
                                bidUnit = ctx.GetBidRegex();
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = ctx.GetRegex("投标单位");
                                bidMoney = ctx.GetMoneyRegex();
                                prjMgr = ctx.GetMgrRegex();
                                if (string.IsNullOrEmpty(prjMgr))
                                    prjMgr = ctx.GetReplace("（,）,(,),/", "A").GetRegex("项目负责人姓名及注册证书编号,项目负责人A姓名A证书编号A,项目负责人姓名及证书编号,项目负责人姓名及注册证书编号", true, 50);
                                if (prjMgr.Contains("A"))
                                    prjMgr = prjMgr.Remove(prjMgr.IndexOf("A"));
                            }

                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            code = bidCtx.GetCodeRegex();
                            if (code.Contains("公示"))
                                code = code.Remove(code.IndexOf("公示"));

                            bidType = prjName.GetInviteBidType();

                            msgType = "清远市公共资源交易中心";
                            specType = "建设工程";

                            BidInfo info = ToolDb.GenBidInfo("广东省", "清远市区", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                        string fileUrl = string.Empty;
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
