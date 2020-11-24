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
    public class BidJiangSuZww : WebSiteCrawller
    {
        public BidJiangSuZww()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "江苏省政务网中标信息";
            this.Description = "自动抓取江苏省政务网中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.jszwfw.gov.cn/module/jslib/jquery/jpage/dataproxy.jsp?perpage=15&endrecord=45&startrecord=1";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                string str = System.Web.HttpUtility.UrlDecode("appid=1&webid=1&path=%2F&columnid=808&sourceContentType=1&unitid=620&webname=%E6%B5%99%E6%B1%9F%E7%9C%81%E5%8F%91%E5%B1%95%E5%92%8C%E6%94%B9%E9%9D%A9%E5%A7%94%E5%91%98%E4%BC%9A&permissiontype=0");
                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                "col",
                "appid",
                "webid",
                "path",
                "columnid",
                "sourceContentType",
                "unitid",
                "webname",
                "permissiontype"
                },
                    new string[]{
                      "1",
                "1",
                "1",
                "/",
                "148",
                "1",
                "363",
                "江苏政务服务网",
                "0"
                    });
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
            }
            catch { return null; }

            try
            {
                string temp = html.GetRegexBegEnd("<totalpage>", "</totalpage>");
                pageInt = int.Parse(temp);
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                "col",
                "appid",
                "webid",
                "path",
                "columnid",
                "sourceContentType",
                "unitid",
                "webname",
                "permissiontype"
                },
        new string[]{
                      "1",
                "1",
                "1",
                "/",
                "148",
                "1",
                "363",
                "江苏政务服务网",
                "0"
                    });
                    try
                    {
                        int endrecord = i * 45;
                        int startrecord = 45 * i - 44;
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.jszwfw.gov.cn/module/jslib/jquery/jpage/dataproxy.jsp?perpage=15&endrecord=" + endrecord + "&startrecord=" + startrecord, nvc);
                    }
                    catch { continue; }
                }

                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "99%")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
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
                              prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;
                        TableRow tr = (listNode[j] as TableTag).Rows[0];
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.GetAttribute("title");
                        if (prjName.Contains(" "))
                        {
                            string[] str = prjName.Split(' ');
                            code = str[0];
                            prjName = str[1];
                        }
                        else
                        {
                            string str = prjName.GetNotChina();
                            if (str.Length > 2 && prjName.IsNumber())
                            {
                                try
                                {
                                    int index = prjName.IndexOf(str.Substring(0, 2));
                                    code = prjName.Substring(0, index);
                                    prjName = prjName.Substring(index, prjName.Length - index);
                                }
                                catch { }
                            }
                        }
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.jszwfw.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zoom")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>", "\r\n").ToCtxString();
                            buildUnit = bidCtx.GetBuildRegex();
                            if (string.IsNullOrEmpty(code))
                                code = bidCtx.GetCodeRegex().GetCodeDel();
                            bidUnit = bidCtx.GetBidRegex().GetReplace("名称");
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegex("第一中标候选单位为,第一名,中标（成交）候选人名称").GetReplace("名称");
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                bidMoney = bidCtx.GetMoneyRegex(null, true);
                            prjMgr = bidCtx.GetMgrRegex();
                            try
                            {
                                if (decimal.Parse(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            msgType = "江苏省政务服务管理办公室";
                            specType = "政府采购";
                            bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("江苏省", "江苏省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.jszwfw.gov.cn/" + a.Link;
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
