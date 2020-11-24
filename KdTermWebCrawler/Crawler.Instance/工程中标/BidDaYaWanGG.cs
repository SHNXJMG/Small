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
    public class BidDaYaWanGG : WebSiteCrawller
    {
        public BidDaYaWanGG()
             : base()
        {
            this.Group = "中标信息";
            this.Title = "惠州大亚湾经济技术开发区公共资源交易中心中标公告";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取惠州大亚湾经济技术开发区公共资源交易中心中标公告";
            this.SiteUrl = "http://zyjy.dayawan.gov.cn/website/zyjyzx/html/artList.html?cataId=201501031615165890";
            this.MaxCount = 200;
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));

            NodeList pageo = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "leftnav")), true), new TagNameFilter("span")));
            if (pageo != null && pageo.Count > 0)
            {
                string pages = pageo.AsString().GetRegexBegEnd("条", "页");
                try
                {

                    pageInt = int.Parse(pages.Replace("/", ""));
                }
                catch { }
            }

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&pageNo=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "div_list")), true), new TagNameFilter("ul")));
                if (nodeList != null && nodeList.Count > 0)
                {

                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
          bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://zyjy.dayawan.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "div_view")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</p>,</br>", "\r\n").GetReplace("<br />", "\r\n").ToCtxString();


                            bidUnit = bidCtx.GetBidRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("中标人");
                            bidMoney = bidCtx.GetMoneyRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            if (string.IsNullOrWhiteSpace(buildUnit))
                                buildUnit = bidCtx.GetRegex("招标人");
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            if (!string.IsNullOrWhiteSpace(code))
                                if (code[code.Length - 1] != '号')
                                    code = "";

                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            msgType = "惠州大亚湾经济技术开发区公共资源交易中心";
                            specType = "建设工程";
                            bidType = ToolHtml.GetInviteTypes(prjName);

                            BidInfo info = ToolDb.GenBidInfo("广东省", "惠州市区", "大亚湾区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);

                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNodes = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNodes != null && aNodes.Count > 0)
                            {
                                for (int a = 0; a < aNodes.Count; a++)
                                {
                                    ATag aFile = aNodes[a] as ATag;
                                    if (aFile.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (aFile.Link.ToLower().Contains("http"))
                                            link = aFile.Link;
                                        else
                                            link = "http://zyjy.dayawan.gov.cn/" + aFile.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(aFile.LinkText, info.Id, link);
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
