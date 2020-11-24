using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Windows.Forms;
using System.Web;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    /// <summary>
    /// 中标信息--中山
    /// </summary>
    public class BidZhongshan : WebSiteCrawller
    {
        public BidZhongshan()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省中山市";
            this.Description = "自动抓取广东省中山市中标信息";
            this.ExistCompareFields = "InfoUrl";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.SiteUrl = "http://p.zsjyzx.gov.cn/port/Application/NewPage/PageSubItem.jsp?node=61";
            this.MaxCount = 50;
        }
        Dictionary<string, string> _dicSiteUrl;
        protected Dictionary<string, string> DicSiteUrl
        {
            get
            {
                if (_dicSiteUrl == null)
                {
                    _dicSiteUrl = new Dictionary<string, string>();
                   
                    _dicSiteUrl.Add("建设工程中标公告", "http://p.zsjyzx.gov.cn/port/Application/NewPage/PageSubItem.jsp?node=61");
                    _dicSiteUrl.Add("政府采购中标公告", "http://p.zsjyzx.gov.cn/port/Application/NewPage/PageSubItem.jsp?node=55");


                }
                return _dicSiteUrl;
            }
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            foreach (string area in this.DicSiteUrl.Keys)
            {
                int pageInt = 1, count = 0;
                string html = string.Empty;
                string viewState = string.Empty;
                string eventValidation = string.Empty;
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(this.DicSiteUrl[area], Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    return list;
                }

                Parser parser = new Parser(new Lexer(html));
                NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "pageintro")));
                if (sNode != null && sNode.Count > 0)
                {
                    try
                    {
                        string temp = sNode.AsString().ToCtxString().GetRegexBegEnd("页共", "页");
                        pageInt = int.Parse(temp);
                    }
                    catch (Exception) { }
                }
                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(this.DicSiteUrl[area] + "&page=" + i.ToString(), Encoding.UTF8);
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "nav_list"))), new TagNameFilter("ul"))), new TagNameFilter("li")));
                    if (sNode != null && sNode.Count > 0)
                    {
                        for (int t = 0; t < sNode.Count; t++)
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
                               prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty,
                               hele = string.Empty;

                            beginDate = sNode[t].ToPlainTextString().GetDateRegex();
                            prjName = sNode[t].GetATagValue("title");
                            InfoUrl = "http://p.zsjyzx.gov.cn" + sNode[t].GetATagHref();
                            string url = string.Empty, shurl = string.Empty, urls = string.Empty;
                            urls = InfoUrl + "s";
                            shurl = urls.GetRegexBegEnd("articalID=", "s");
                            url = "http://p.zsjyzx.gov.cn/port/Application/NewPage/ggnr.jsp?articalID=" + shurl;
                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
                                Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                                NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "details_1")));
                                hele = dtnodeHTML.AsHtml();
                                HtmlTxt = dtnodeHTML.AsHtml();
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n").Replace("<br/>", "\r\n");
                            }
                            catch (Exception ex) { continue; }

                            if (area == "建设工程中标公告")
                            {
                                Parser dtlparser = new Parser(new Lexer(htmldetail));
                                NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "newtalbe_c")));
                                HtmlTxt = dtnode.AsHtml();
                                bidCtx = HtmlTxt.Replace("</th>", "：").Replace("</tr>", "\r\n").ToCtxString();
                                code = HtmlTxt.ToCtxString().GetCodeRegex().Replace("[", "").Replace("]", "");
                                buildUnit = bidCtx.GetBuildRegex();
                                prjAddress = bidCtx.GetAddressRegex();
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex();
                                bidType = prjName.GetInviteBidType();

                                msgType = "中山市公共资源交易中心";
                                specType = "建设工程";
                            }
                            else
                            {
                                Parser dtlparser = new Parser(new Lexer(htmldetail));
                                NodeList dtlList = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "details_1")));
                                if (dtlList != null && dtlList.Count > 0)
                                {
                                    HtmlTxt = dtlList.AsHtml();
                                    bidCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString();
                                    code = bidCtx.GetCodeRegex().Replace("[", "").Replace("]", "");
                                    buildUnit = bidCtx.GetBuildRegex();
                                    if (string.IsNullOrWhiteSpace(buildUnit))
                                        buildUnit = bidCtx.GetRegex("采购单位");
                                    prjAddress = bidCtx.GetAddressRegex();
                                    bidUnit = bidCtx.GetRegex("中标供应商名称");
                                    if(string.IsNullOrWhiteSpace(bidUnit))
                                       bidUnit = bidCtx.GetBidRegex();
                                    if (bidUnit.Contains("负责人"))
                                        bidUnit = bidUnit.Remove(bidUnit.IndexOf("负责人"));
                                    if (bidUnit.Contains("法人"))
                                        bidUnit = bidUnit.Remove(bidUnit.IndexOf("法人"));
                                    bidMoney = bidCtx.GetMoneyRegex();
                                    bidType = prjName.GetInviteBidType();
                                    msgType = "中山市公共资源交易中心";
                                    specType = "建设工程";
                                }
                            }
                            string are = area != "建设工程中标公告" ? area : "";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "中山市区", string.Empty, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                   bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            count++;
                            list.Add(info);
                            if (!crawlAll && count >= this.MaxCount) goto Funcs;

                        }
                    }
                }
                Funcs:; 
            }
            return list;
        }
    }
}
