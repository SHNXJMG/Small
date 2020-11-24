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
    public class BidSzClcg : WebSiteCrawller
    {
        public BidSzClcg()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Title = "广东采联采购招标有限公司";
            this.Description = "自动抓取广东采联采购招标有限公司中标信息";
            this.SiteUrl = "http://www.chinapsp.cn/xinxigonggao/list.php?catid=74168";
            this.MaxCount = 20;
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pages")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString().GetRegexBegEnd("/", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i.ToString(), Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "xxcon_main_left")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        ATag aTag = nodeList[j].GetATag(1);
                        prjName = aTag.LinkText;
                        InfoUrl = aTag.Link;//"http://www.chinapsp.cn/cn/info.aspx" + 
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));

                        if (dtList != null && dtList.Count > 0)
                        {
                            HtmlTxt = dtList.AsHtml();
                            bidCtx = HtmlTxt.ToLower().Replace("<tr>", "\r\n").Replace("</tr>", "\r\n").ToCtxString();

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
                            string city = string.Empty;
                            try
                            {
                                string temp = nodeList[j].ToPlainTextString().GetRegexBegEnd("areaid", ";").GetReplace("(,)");
                                city = Areaid(int.Parse(temp));
                            }
                            catch { }
                            if (string.IsNullOrWhiteSpace(city))
                                city = "深圳社会招标";
                            code = bidCtx.GetCodeRegex().GetChina();
                            if (!string.IsNullOrWhiteSpace(code))
                                code = code.ToUpper().GetCodeDel();
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("事务所"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("事务所")) + "事务所"; 
                            bidUnit = bidUnit.GetReplace("名称");
                            specType = "其他";
                            msgType = "广东采联采购招标有限公司";
                            prjName = prjName.GetBidPrjName();
                            BidInfo info = ToolDb.GenBidInfo("广东省", city, "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag fileTag = aNode[a] as ATag;
                                    if (fileTag.IsAtagAttach() || fileTag.LinkText.Contains("招标文件"))
                                    {
                                        BaseAttach item = ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, fileTag.Link);
                                        base.AttachList.Add(new BaseAttach());
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

        protected string Areaid(int id)
        {

            switch (id)
            {
                case 231: return "广州市区";
                case 232: return "韶关市区";
                case 233: return "深圳社会招标";
                case 234: return "珠海市区";
                case 235: return "汕头市区";
                case 236: return "佛山市区";
                case 237: return "江门市区";
                case 238: return "湛江市区";
                case 239: return "茂名市区";
                case 240: return "肇庆市区";
                case 241: return "惠州市区";
                case 242: return "梅州市区";
                case 243: return "汕尾市区";
                case 244: return "河源市区";
                case 245: return "阳江市区";
                case 246: return "清远市区";
                case 247: return "东莞市区";
                case 248: return "中山市区";
                case 249: return "潮州市区";
                case 250: return "揭阳市区";
                case 251: return "云浮市区";
                default: return "广东";
            }
        }
    }
}
