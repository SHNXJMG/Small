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
using System.IO;
using System.Web.UI.MobileControls;
using System.Collections.Generic;
using System.Reflection;
using System.Data.SqlClient; 

namespace Crawler.Instance
{
    public class Info:WebSiteCrawller
    {
        public Info()
            : base()
        {
            this.Group = "其它处理";
            this.Title = "广东采联采购招标有限公司";
            this.Description = "自动抓取广东采联采购招标有限公司中标信息";
            this.PlanTime = "1 22:22";
            this.SiteUrl = "http://www.chinapsp.cn/cn/info.aspx?f_type=44";
            this.MaxCount = 5000;
            this.ExistCompareFields = "InfoUrl";
            this.ExistsUpdate = true;
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
                html = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("id", "myPages_input")), true), new TagNameFilter("option")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    OptionTag opt = pageList[pageList.Count - 1] as OptionTag;
                    string temp = opt.GetAttribute("value");
                    pageInt = int.Parse(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(this.SiteUrl + "&pageNo=" + i.ToString(), Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("dl"), new HasAttributeFilter("class", "i-news")), true), new TagNameFilter("dd")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    { 
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        if (!string.IsNullOrEmpty(beginDate))
                            prjName = nodeList[j].ToNodePlainString().Replace(beginDate, "").Replace("[", "").Replace("]", "");
                        else
                            prjName = nodeList[j].ToNodePlainString().Replace("[", "").Replace("]", "");
                        prjName = prjName.GetBidPrjName();
                        bidType = prjName.GetInviteBidType();
                        InfoUrl = "http://www.chinapsp.cn/cn/info.aspx" + nodeList[j].GetATagHref();
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = ToolWeb.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "dbDetailFV")));
                        if (dtList != null && dtList.Count > 0)
                        {
                            HtmlTxt = dtList.AsHtml();
                            bidCtx = HtmlTxt.ToLower().Replace("<tr>", "\r\n").Replace("</tr>", "\r\n").ToCtxString();
                            if (prjName.Contains("招标编号") || prjName.Contains("项目编号"))
                            {
                                if (prjName.IndexOf("（") != -1)
                                {
                                    prjName = prjName.Remove(prjName.IndexOf("（"));
                                }
                                else if (prjName.IndexOf("(") != -1)
                                {
                                    prjName = prjName.Remove(prjName.IndexOf("("));
                                }
                                else if (prjName.Contains("招标编号"))
                                {
                                    prjName = prjName.Remove(prjName.IndexOf("招标编号"));
                                }
                                else if (prjName.Contains("项目编号"))
                                {
                                    prjName = prjName.Remove(prjName.IndexOf("项目编号"));
                                }
                            }
                            buildUnit = bidCtx.GetBuildRegex();
                            prjAddress = bidCtx.Replace(" ", "").GetAddressRegex();

                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            if (!string.IsNullOrEmpty(bidMoney))
                            {
                                decimal money = Convert.ToDecimal(bidMoney);
                                if (money > 10000)
                                    bidMoney = Convert.ToString(money / 10000);
                            }
                            if (bidMoney == "0")
                            {
                                bidMoney = bidCtx.GetMoneyRegex(null, true);
                                if (string.IsNullOrEmpty(bidMoney))
                                    bidMoney = "0";
                            }
                            if (!string.IsNullOrEmpty(bidMoney))
                            {
                                decimal money = Convert.ToDecimal(bidMoney);
                                if (money > 10000)
                                    bidMoney = Convert.ToString(money / 10000);
                            }
                            if (bidMoney == "0")
                            {
                                bidMoney = bidCtx.ToLower().GetMoneyRegex(new string[] { "rmb" });
                            }
                            if (string.IsNullOrEmpty(bidUnit) && bidMoney == "0")
                            {
                                if (bidCtx.Contains("采购失败") || bidCtx.Contains("本项目招标失败"))
                                {
                                    bidUnit = "没有中标商";
                                    bidMoney = "0";
                                }
                            }
                            code = bidCtx.GetCodeRegex().GetChina();
                            specType = "其他";
                            msgType = "广东采联采购招标有限公司";
                            prjName = prjName.GetBidPrjName();
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt); 
                            ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate,this.ExistsHtlCtx, " and LastModifier ='00000000000000000000000000000000'");
                        }
                    }
                }
            }
            return list;
        } 
    }
}
