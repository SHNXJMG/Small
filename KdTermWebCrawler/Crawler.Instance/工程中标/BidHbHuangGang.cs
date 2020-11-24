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
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class BidHbHuangGang : WebSiteCrawller
    {
        public BidHbHuangGang()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "黄冈市公共资源交易中心信息中标信息";
            this.Description = "自动抓取黄冈市公共资源交易中心信息中标信息";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://www.hgggzy.com/ceinwz/msjyjggs.aspx?xmlx=10&FromUrl=msjyjggs.htm&zbdl=1";
            this.MaxCount = 120;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookie = string.Empty;
            try
            {
                html = ToolHtml.GetHtmlByUrlCookie(this.SiteUrl, Encoding.Default,ref cookie);
                //html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default,ref cookie);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_myGV_ctl23_LabelPageCount")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToNodePlainString();
                    pageInt = Convert.ToInt32(temp);
                }
                catch { pageInt = 1; }
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
                    "__VIEWSTATEENCRYPTED",
                    "__EVENTVALIDATION",
                    "ctl00$ContentPlaceHolder1$txtGcmc",
                    "ctl00$ContentPlaceHolder1$DDLGclx"
                    }, new string[]{
                    "ctl00$ContentPlaceHolder1$myGV$ctl23$LinkButtonNextPage",
                    "",
                      viewState,
                    "",
                    eventValidation,
                    "",
                    "全部类型"
                    });
                    StringBuilder post = new StringBuilder();
                    for (int n = 0; n < nvc.Count; n++)
                    {
                        if (n == 0)
                            post.Append(nvc.AllKeys[n] + "=" + nvc[n]);
                        else
                            post.Append("&" + nvc.AllKeys[n] + "=" + nvc[n]);
                    }
                    try
                    {
                       html = ToolHtml.GetHtmlGJByUrlPost(this.SiteUrl, post.ToString(), Encoding.Default, ref cookie);
                        
                        //html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.Default, ref cookie);
                    }
                    catch {  }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_myGV")));
                if (viewList != null && viewList.Count > 0)
                {
                    TableTag table = viewList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
                            bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty,
                            InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, 
                            prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        code = tr.Columns[0].ToNodePlainString();
                        beginDate = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        bidType = tr.Columns[2].ToNodePlainString();
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.LinkText.ToNodeString().GetReplace(" ,[查看公告],[查看公示]");

                        InfoUrl = "http://www.hgggzy.com/ceinwz/" + aTag.Link;
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));

                        NodeList dtlNode = null;
                        NodeList aNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("valign", "top")), true), new TagNameFilter("a")));
                        if (aNode != null && aNode.Count > 0)
                        {
                            ATag dtlTag = null;
                            for (int a = 0; a < aNode.Count; a++)
                            {
                                dtlTag = aNode[a].GetATag();
                                if (dtlTag.Link.Contains(".doc"))
                                    break;
                            }

                            string link = "http://www.hgggzy.com/WordHtml/BestHtml.aspx?id=" + dtlTag.Link.GetReplace("/doc/");
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(link, Encoding.Default).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        }
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml().ToLower();
                            bidCtx = HtmlTxt.GetReplace("</p>,</br>,<br>,</div>", "\r\n").ToCtxString();

                            buildUnit = bidCtx.GetBuildRegex();
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));

                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                string ctx = string.Empty;
                                TableTag dtlTable = tableNode[0] as TableTag;
                                for (int r = 1; r < dtlTable.RowCount; r++)
                                {
                                    if (dtlTable.Rows[r].ColumnCount < 2)
                                        break;

                                    ctx += dtlTable.Rows[r].Columns[0].ToNodePlainString() + "：";
                                    ctx += dtlTable.Rows[r].Columns[1].ToNodePlainString() + "\r\n";
                                }
                                bidUnit = ctx.GetBidRegex();
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = ctx.GetBidRegex(new string[] { "中标候选人名称" });
                                bidMoney = ctx.GetMoneyRegex();
                                prjMgr = ctx.GetMgrRegex();
                            }
                            else
                            {
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex();
                                prjMgr = bidCtx.GetMgrRegex();

                            }
                            try
                            {
                                if (decimal.Parse(bidMoney) < 1)
                                    bidMoney = "0";
                            }
                            catch { }

                            msgType = "黄冈市公共资源交易中心";
                            specType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("湖北省", "湖北省及地市", "黄冈市", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNodes = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNodes != null && aNodes.Count > 0)
                            {
                                for (int k = 0; k < aNodes.Count; k++)
                                {
                                    ATag a = aNodes[k].GetATag();
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://www.hgggzy.com/" + a.Link;
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
