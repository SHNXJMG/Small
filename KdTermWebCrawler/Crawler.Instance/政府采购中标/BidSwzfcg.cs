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
    public class BidSwzfcg : WebSiteCrawller
    {
        public BidSwzfcg()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "汕尾市政府采购中标信息";
            this.PlanTime = "9:13,11:13,13:13,15:13,17:13";
            this.Description = "自动抓取汕尾市政府采购中标信息";
            this.SiteUrl = "http://shanwei.gdgpo.com/queryMoreInfoList/channelCode/0008.html";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("form"), new HasAttributeFilter("name", "qPageForm")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    NodeList aNode = new Parser(new Lexer(pageNode.ToHtml())).ExtractAllNodesThatMatch(new TagNameFilter("a"));
                    if (aNode != null && aNode.Count > 0)
                    {
                        string temp = aNode[aNode.Count - 2].GetATagHref().Replace("turnOverPage", "").Replace("(", "").Replace(")", "").Replace(";", "");
                        pageInt = int.Parse(temp);
                    }
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{ "channelCode","pageIndex","pageSize","pointPageIndexId"
                    }, new string[]{
                    "0008",i.ToString(),"15","1"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://shanwei.gdgpo.com/queryMoreInfoList.do", nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "m_m_c_list")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = listNode[j].GetATag(1);
                        prjName = aTag.GetAttribute("title");
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://shanwei.gdgpo.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "zw_c_c_cont")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml().Replace("<br", "\r\n<br");
                            bidCtx = HtmlTxt.Replace("</p>", "\r\n").Replace("</pre>", "\r\n").ToCtxString();

                            buildUnit = bidCtx.GetBuildRegex();
                            code = bidCtx.Replace("（招标编号", "000000").GetCodeRegex().GetCodeDel();
                            if (string.IsNullOrEmpty(code))
                                code = bidCtx.GetRegex("招标编号", true, 50).GetCodeDel();
                            bidUnit = bidCtx.GetBidRegex().GetBidUnitDel();
                            bidMoney = bidCtx.GetMoneyString();
                            if (bidMoney.Contains("（"))
                                bidMoney = bidMoney.Remove(bidMoney.IndexOf("（")).GetMoney();
                            else
                                bidMoney = bidMoney.GetMoney();
                            if (bidMoney == "0")
                            {
                                bidMoney = bidCtx.GetMoneyString(null, true);
                                if (bidMoney.Contains("（"))
                                    bidMoney = bidMoney.Remove(bidMoney.IndexOf("（")).GetMoney();
                                else if (bidMoney.Contains("大写"))
                                    bidMoney = bidMoney.Remove(bidMoney.IndexOf("大写")).GetMoney();
                                else
                                    bidMoney = bidMoney.GetMoney();
                            }
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                            {
                                bidMoney = bidCtx.GetMoneyString(null, true);
                                if (bidMoney.Contains("大写"))
                                    bidMoney = bidMoney.Remove(bidMoney.IndexOf("大写")).GetMoney();
                                else
                                    bidMoney = bidMoney.GetMoney("万元");
                            }
                            if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                            {
                                bidMoney = bidCtx.GetMoneyString(null, true).GetMoney();
                            }
                            if (!string.IsNullOrEmpty(bidMoney) && bidMoney != "0" && decimal.Parse(bidMoney) > 10000)
                                bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            if (bidUnit.Contains("名称"))
                                bidUnit = bidUnit.Replace("名称", "");
                            bidType = prjName.GetInviteBidType();
                            msgType = "汕尾市政府采购";
                            specType = "政府采购";

                            BidInfo info = ToolDb.GenBidInfo("广东省", "汕尾市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList fileNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (fileNode != null && fileNode.Count > 0)
                            {
                                for (int k = 0; k < fileNode.Count; k++)
                                {
                                    ATag fileAtag = fileNode[k].GetATag();
                                    if (fileAtag.IsAtagAttach())
                                    {
                                        string fileName = fileAtag.LinkText.ToNodeString().Replace(" ", "");
                                        string fileLink = fileAtag.Link;
                                        if (!fileLink.ToLower().Contains("http"))
                                            fileLink = "http://shanwei.gdgpo.gov.cn" + fileAtag.Link;
                                        base.AttachList.Add(ToolDb.GenBaseAttach(fileName, info.Id, fileLink));
                                    }
                                }
                            }
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
