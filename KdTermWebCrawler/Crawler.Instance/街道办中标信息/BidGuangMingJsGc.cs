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

namespace Crawler.Instance
{
    public class BidGuangMingJsGc : WebSiteCrawller
    {
        public BidGuangMingJsGc()
            : base() 
        {
            this.Group = "街道办中标信息";
            this.Title = "广东省深圳市光明新区建设工程中标公告";
            this.Description = "自动抓取广东省深圳市光明新区建设工程中标公告";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.szgm.gov.cn/gmbscn/144192/144212/144220/index.html";
            this.MaxCount = 100;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(htl));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "Normal")));
            try
            {
                string temp = pageNode.AsString().GetRegexBegEnd("/", "跳");
                page = int.Parse(temp);
            }
            catch { }
            for (int i = 1; i <= page; i++)
            {  
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl("http://www.szgm.gov.cn/gmbscn/144192/144212/144220/19dcd628-" + i.ToString() + ".html", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "jwRercon")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        INode node = nodeList[j];
                        ATag aTag = node.GetATag();
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

                        beginDate = node.ToPlainTextString().GetDateRegex();
                        prjName = aTag.GetAttribute("title");

                        InfoUrl = "http://www.szgm.gov.cn" + aTag.Link;
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch {  continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "jwRercon")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</br>,</p>","\r\n").ToCtxString();

                            bidType = prjName.GetInviteBidType();

                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetBidRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = "深圳市光明新区";
                            prjMgr = bidCtx.GetMgrRegex();
                            if (string.IsNullOrWhiteSpace(prjMgr))
                                prjMgr = bidCtx.GetRegex("项目经理（项目负责人）");
                            if (string.IsNullOrEmpty(bidUnit))
                                bidUnit = bidCtx.GetRegexBegEnd("现确认", "为中标");
                            msgType = "深圳市光明新区";
                            specType = "建设工程";
                            try
                            {
                                if (decimal.Parse(bidMoney) > 50000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳区及街道工程", "光明新区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                              bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag tag = aNode[a] as ATag;
                                    if (tag.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (tag.Link.Contains("http"))
                                            link = tag.Link;
                                        else
                                            link = "http://www.szgm.gov.cn" + tag.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(tag.LinkText, info.Id, link);
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
