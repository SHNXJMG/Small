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
    public class BidFoShanZfcg : WebSiteCrawller
    {
        public BidFoShanZfcg()
            : base()
        {
            this.Group = "政府采购中标信息";
            this.Title = "广东省佛山市政府采购中标信息";
            this.Description = "自动抓取广东省佛山市政府采购中标信息";
            this.SiteUrl = "http://www.fsggzy.cn/zfcg/zf_zbxx/zf_zbsz/";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
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
                    _dicSiteUrl.Add("市直", "http://www.fsggzy.cn/zfcg/zf_zbxx/zf_zbsz/");
                    _dicSiteUrl.Add("禅城", "http://www.fsggzy.cn/zfcg/zf_zbxx/zfzbgq/zbxxcc/");
                    _dicSiteUrl.Add("南海", "http://www.fsggzy.cn/zfcg/zf_zbxx/zfzbgq/zbxxnh/");
                    _dicSiteUrl.Add("三水", "http://www.fsggzy.cn/zfcg/zf_zbxx/zfzbgq/zbxxss/");
                    _dicSiteUrl.Add("高明", "http://www.fsggzy.cn/zfcg/zf_zbxx/zfzbgq/zbxxgm/");
                    _dicSiteUrl.Add("顺德", "http://www.fsggzy.cn/zfcg/zf_zbxx/zfzbgq/zbxxsd/");
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
                    html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    return list;
                }

                Parser parser = new Parser(new Lexer(html));
                NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page")));
                if (sNode != null && sNode.Count > 0)
                {
                    try
                    {
                        string page = sNode.AsString().ToNodeString().Replace("createPageHTML(", "");
                        string temp = page.Remove(page.IndexOf(",")).Replace("  ", "");
                        pageInt = Convert.ToInt32(temp);
                    }
                    catch (Exception) { }
                }

                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(this.DicSiteUrl[area] + "index_" + (i - 1) + ".html".ToString(), Encoding.UTF8);
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "secondrightlistbox"))), new TagNameFilter("ul"))), new TagNameFilter("li")));
                    if (sNode != null && sNode.Count > 0)
                    {
                        for (int t = 0; t < sNode.Count; t++)
                        {
                            string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            beginDate = sNode[t].ToNodePlainString().GetDateRegex();
                            prjName = sNode[t].GetATagValue("title");

                            InfoUrl = this.DicSiteUrl[area] + sNode[t].GetATagHref().Replace("./", "");

                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                                Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                                NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "contentrightlistbox2")));
                                HtmlTxt = dtnodeHTML.AsHtml();
                            }
                            catch (Exception ex) { continue; }
                            Parser dtlparser = new Parser(new Lexer(htmldetail));
                            NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "contentrightlistbox2")));
                            if (dtnode != null && dtnode.Count > 0)
                            {
                                string ctx = HtmlTxt.ToCtxString();
                                //TableTag tab = dtnode[0] as TableTag;
                                //if (tab.Rows[0].ColumnCount > 1)
                                //{
                                //    for (int d = 0; d < tab.RowCount; d++)
                                //    {
                                //        ctx += tab.Rows[d].Columns[0].ToNodePlainString() + "：";
                                //        ctx += tab.Rows[d].Columns[1].ToNodePlainString() + "\r\n";
                                //    }
                                //}
                                bidCtx = dtnode.ToHtml().ToCtxString();
                                bidUnit = ctx.GetBidRegex();
                                buildUnit = ctx.GetBuildRegex();
                                code = ctx.GetCodeRegex().GetCodeDel().GetChina();
                                prjAddress = ctx.GetAddressRegex();
                                bidType = prjName.GetInviteBidType();
                                bidMoney = ctx.GetMoneyRegex(null, false, "万元");

                                if (bidUnit.Contains("公司"))
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                                if (bidUnit.Contains("招标代理机构"))
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("招标代理机构"));
                                if (bidUnit.Contains("联系人"))
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("联系人"));
                                if (bidUnit.Contains("；"))
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("；"));
                                if (bidUnit.Contains("地址"))
                                    bidUnit = bidUnit.Remove(bidUnit.IndexOf("地址"));

                                bidUnit = bidUnit.Replace("名称", "");

                                if (buildUnit.Contains("招标代理机构"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理机构"));
                                if (buildUnit.Contains("联系"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                                if (buildUnit.Contains("；"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("；"));
                                if (buildUnit.Contains("地址"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                                if (buildUnit.Contains("电话"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("电话"));
                                msgType = "佛山市建设工程交易中心";
                                specType = "政府采购";
                                prjName = ToolDb.GetPrjName(prjName);
                                string are = area != "市直" ? area : "";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "佛山市区", are, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                count++;
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
                                                link = "http://www.fsggzy.cn/" + a.Link.GetReplace("../,./");
                                            if (Encoding.Default.GetByteCount(link) > 500)
                                                continue;
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }
                                if (!crawlAll && count >= this.MaxCount) goto Funcs;
                            }
                        }
                    }
                }
                Funcs:;
            }
            return list;
        }
    }
}
