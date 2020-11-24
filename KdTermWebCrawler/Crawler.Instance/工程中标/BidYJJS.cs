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
    public class BidYJJS : WebSiteCrawller
    {
        public BidYJJS()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省阳江市建设工程中标信息";
            this.Description = "自动抓取广东省阳江市建设工程中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.SiteUrl = "http://www.yjggzy.cn/Query/JsgcWinBidAfficheQuery2/46eb01f656f4468cb65a434b77d73065";
            this.ExistCompareFields = "InfoUrl";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            int sqlCount = 0;
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch  
            { 
                return null;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagination")), true), new TagNameFilter("a")));
            if (nodeList != null && nodeList.Count > 0)
            {
                try
                {
                    string temp = nodeList[nodeList.Count - 1].GetATagHref();
                    string pageCount = temp.Replace(temp.Remove(temp.IndexOf("=")), "").Replace("=", "");
                    page = int.Parse(pageCount);
                }
                catch { }
            }
            for (int i = 1; i <= page; i++)
            { 
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "?page=" + i.ToString(), Encoding.UTF8, ref cookiestr);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList liNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "list")), true), new TagNameFilter("li")));
                if (liNode != null && liNode.Count > 0)
                {
                    for (int j = 0; j < liNode.Count; j++)
                    { 
                        string prjName = string.Empty,
                           buildUnit = string.Empty, bidUnit = string.Empty,
                           bidMoney = string.Empty, code = string.Empty,
                           bidDate = string.Empty,
                           beginDate = string.Empty,
                           endDate = string.Empty, bidType = string.Empty,
                           specType = string.Empty, InfoUrl = string.Empty,
                           msgType = string.Empty, bidCtx = string.Empty,
                           prjAddress = string.Empty, remark = string.Empty,
                           prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        beginDate = liNode[j].ToPlainTextString().GetDateRegex("yyyy/MM/dd");
                        prjName = liNode[j].ToPlainTextString().Replace(beginDate, "").ToNodeString().Replace(" ", "").Replace("·", "");
                        ATag aTag = liNode[j].GetATag();
                        InfoUrl = "http://www.yjggzy.cn" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "");
                        }
                        catch
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtlNode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("dl"), new HasAttributeFilter("class", "acticlecontent")));
                        if (dtlNode == null || dtlNode.Count < 1)
                        {
                            parserdetail.Reset();
                            dtlNode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "nr")));
                        }
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.ToHtml();

                            bidCtx = HtmlTxt.ToCtxString().Replace("\r\n\t\r\n\t", "\r\n\t");
                            bidCtx = bidCtx.Replace("\r\n\t\r\n\t", "\r\n\t");
                            bidCtx = bidCtx.Replace("\r\n\t\r\n\t", "\r\n\t");
                            bidCtx = bidCtx.Replace("\r\n\t\r\n\t", "\r\n\t");
                            bidCtx = bidCtx.Replace("\r\n\t\r\n\t", "\r\n\t");
                            bidCtx = bidCtx.Replace("\r\n\r\n\t", "\r\n\t");
                            bidCtx = bidCtx.Replace("\r\n\r\n\t", "\r\n\t");
                            bidCtx = bidCtx.Replace("\r\n\r\n\t", "\r\n\t");
                            bidCtx = bidCtx.Replace("\r\n\r\n\t", "\r\n\t");
                            bidCtx = bidCtx.Replace("\r\n\r\n", "\r\n");
                            bidCtx = bidCtx.Replace("\r\n\r\n", "\r\n");
                            bidCtx = bidCtx.Replace("\r\n\r\n", "\r\n");
                            bidCtx = bidCtx.Replace("\r\n\r\n", "\r\n");
                            bidCtx = bidCtx.Replace("\r\n\r\n", "\r\n");
                            bidCtx = bidCtx.Replace("\r\n\t\r\n\t", "\r\n\t");
                            bidCtx = bidCtx.Replace("\r\n\r\n", "\r\n");
                            bidCtx = bidCtx.Replace("\r\n\t\r\n\t", "\r\n\t");

                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList dtlNodeList = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (dtlNodeList != null && dtlNodeList.Count > 0)
                            {
                                string ctx = string.Empty;
                                TableTag tableTag = dtlNodeList[0] as TableTag;
                                foreach (TableRow row in tableTag.Rows)
                                {
                                    int colIndex = 0;
                                    foreach (TableColumn col in row.Columns)
                                    {
                                        if (row.Columns.Length == 3)
                                        {
                                            if (colIndex == 0 && col.GetAttribute("colspan") != "2")
                                            {
                                                colIndex++;
                                                continue;
                                            }
                                            else if (col.GetAttribute("colspan") == "2" && colIndex == 1)
                                            {
                                                ctx += col.ToNodePlainString() + "：";
                                            }
                                            else if (!string.IsNullOrEmpty(col.GetAttribute("colspan")) && colIndex == 2)
                                            {
                                                ctx += col.ToNodePlainString() + "\r\n";
                                            }
                                            else if (string.IsNullOrEmpty(col.GetAttribute("colspan")) && colIndex == 1)
                                            {
                                                ctx += col.ToNodePlainString() + "：";
                                            }
                                            else if (string.IsNullOrEmpty(col.GetAttribute("colspan")) && colIndex == 2)
                                            {
                                                ctx += col.ToNodePlainString() + "\r\n";
                                            }
                                            colIndex++;
                                            continue;
                                        }
                                        if (row.Columns.Length == 2)
                                        {
                                            if (colIndex == 0)
                                            {
                                                ctx += col.ToNodePlainString() + "：";
                                            }
                                            else if (colIndex == 1)
                                            {
                                                ctx += col.ToNodePlainString() + "\r\n";
                                            }
                                            colIndex++;
                                            continue;
                                        }
                                        if (colIndex == 0 && col.GetAttribute("colspan") != "2")
                                        {
                                            colIndex++;
                                            continue;
                                        }
                                        else if (colIndex == 1 && col.GetAttribute("colspan") != "2")
                                        {
                                            ctx += col.ToNodePlainString() + "：";
                                        }
                                        else if (colIndex == 2 && col.GetAttribute("colspan") != "2")
                                        {
                                            ctx += col.ToNodePlainString() + "\r\n";
                                        }
                                        else if (col.GetAttribute("colspan") == "2" && colIndex == 0)
                                        {
                                            ctx += col.ToNodePlainString() + "：";
                                        }

                                        else if (!string.IsNullOrEmpty(col.GetAttribute("colspan")) && colIndex == 1)
                                        {
                                            ctx += col.ToNodePlainString() + "\r\n";
                                        }

                                        colIndex++;
                                    }
                                }

                                buildUnit = ctx.GetBuildRegex();
                                bidUnit = ctx.GetBidRegex();
                                code = ctx.GetCodeRegex();
                                prjAddress = ctx.GetAddressRegex();
                                prjMgr = ctx.GetMgrRegex();
                                if (string.IsNullOrEmpty(prjMgr))
                                    prjMgr = ctx.GetRegex("项目负责人姓名", true, 50);
                                bidMoney = ctx.GetMoneyRegex();
                            }
                            else
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList imgNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                                if (imgNode != null && imgNode.Count > 0)
                                {
                                    ImageTag img = imgNode[0] as ImageTag;
                                    string link = "http://www.yjggzy.cn" + img.GetAttribute("src");
                                    HtmlTxt = HtmlTxt.GetReplace(img.GetAttribute("src"),link);
                                }
                            }
                            msgType = "阳江市建设工程交易中心";
                            specType = "建设工程";
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            BidInfo info = ToolDb.GenBidInfo("广东省", "阳江市区", "", string.Empty, code, prjName, buildUnit, beginDate,
                                   bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                   bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            sqlCount++;
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate, this.ExistsHtlCtx))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int a = 0; a < aNode.Count; a++)
                                    {
                                        ImageTag img = aNode[a] as ImageTag; 
                                        try
                                        {
                                            BaseAttach attach = ToolHtml.GetBaseAttach(img.GetAttribute("src"), prjName, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                            if (attach != null)
                                                ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                                        }
                                        catch { }
                                    }
                                }
                            }
                            if (!crawlAll && sqlCount >= this.MaxCount) return null;
                        }
                    }
                }
            }
            return null;
        }
    }
}
