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
    public class BidHuiZhouZjcs : WebSiteCrawller
    {
        public BidHuiZhouZjcs()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "惠州市中介超市建设领导小组办公室中标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取惠州市中介超市建设领导小组办公室中标信息";
            this.SiteUrl = "http://183.63.34.189/zjcs-pub/bidResultNotice";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 993;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;

            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return null; }

            for (int i = 1; i < pageInt; i++)
            { 
                if (i > 1)
                {

                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]
                    { "listVo.projectName",
                        "listVo.serviceType",
                        "pageNumber",
                     "sourtType"},
                    new string[] {
                        "",
                        "",
                        (i-1).ToString(),
                        ""
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://183.63.34.189/zjcs-pub/bidResultNotice/rest", nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table table-hover table-bordered table-list")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[0].GetATag();

                        prjName = aTag.LinkText.ToRegString();
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://183.63.34.189" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pannel-cont")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList bidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                            if (bidNode != null && bidNode.Count > 0)
                            {
                                TableTag bidTable = bidNode[0] as TableTag;
                                for (int r = 0; r < bidTable.RowCount; r++)
                                {
                                    for (int c = 0; c < bidTable.Rows[r].ColumnCount; c++)
                                    {
                                        string temp = bidTable.Rows[r].Columns[c].ToNodePlainString();
                                        if (c % 2 == 0)
                                            bidCtx += temp + "：";
                                        else
                                            bidCtx += temp + "\r\n";
                                    }
                                }
                            }
                            else
                                bidCtx = HtmlTxt.ToCtxString();
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            prjAddress = bidCtx.GetAddressRegex().GetCodeDel();
                            bidUnit = bidCtx.GetBidRegex(new string[] { "中选企业名称" });
                            bidMoney = bidCtx.GetMoneyRegex(new string[] { "中选金额" });

                            if (bidUnit.Contains("根据"))
                                bidUnit = "";

                            msgType = "惠州市中介超市建设领导小组办公室";
                            specType = "建设工程";
                            bidType = prjName.GetInviteBidType();
                            BidInfo info = ToolDb.GenBidInfo("广东省", "惠州市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
            }
            return list;
        }
    }


}
