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
    public class BidSzsfcx : WebSiteCrawller
    {
        public BidSzsfcx()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "深圳市三方诚信招标有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳市三方诚信招标有限公司中标信息";
            this.SiteUrl = "http://www.sfcx.cn/purchase.aspx?ClassID=25";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagin")));
            if (tdNodes != null)
            {
                string pageTemp = tdNodes.AsString().Replace("&nbsp;", "").Replace(" ", "");
                try
                {
                    pageInt = int.Parse(pageTemp.GetRegexBegEnd("/", "页"));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&page=" + i.ToString()), Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "wz")), true), new TagNameFilter("table")));

                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        TableTag table = nodeList[j] as TableTag;

                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[1];
                        ATag aTag = tr.GetATag();
                        beginDate = tr.ToPlainTextString().GetDateRegex();
                        prjName = aTag.LinkText.Trim();
                        InfoUrl = "http://www.sfcx.cn/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }

                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "wz"), new TagNameFilter("td")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();

                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = bidCtx.GetBuildRegex();

                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("中标（成交）单位,中标(成交)单位");
                            bidMoney = bidCtx.GetMoneyRegex(new string[] { "￥" }, false, "万元"); 
                            if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                bidMoney = bidCtx.GetMoneyRegex(null, false, "万元");
                             
                            specType = "其他";
                            msgType = "深圳市三方诚信招标有限公司";
                            prjName = ToolDb.GetPrjName(prjName);
                            bidType = prjName.GetInviteBidType();

                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            dtlparser = new Parser(new Lexer(HtmlTxt));
                            NodeList FileTag = dtlparser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (FileTag != null && FileTag.Count > 0)
                            {
                                for (int k = 0; k < FileTag.Count; k++)
                                {
                                    ATag a = FileTag[k].GetATag();
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://www.sfcx.cn/" + a.Link;
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
