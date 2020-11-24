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

namespace Crawler.Instance
{
    public class InviteMMHuaZhou : WebSiteCrawller
    {
        public InviteMMHuaZhou()
            : base()
        {
            this.Group = "区县招标信息";
            this.Title = "广东省茂名化州市住房和城乡建设局";
            this.Description = "自动抓取广东省茂名化州市住房和城乡建设局";
            this.PlanTime = "9:21,10:21,14:21,16:22";
            this.SiteUrl = "http://gcgk.maoming.gov.cn:8088/html/ZTBXX/";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page textRight")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString();
                    Regex reg = new Regex(@"共分[^页]+页");
                    string page = reg.Match(temp).Value.Replace("共分", "").Replace("页", "");
                    pageInt = Convert.ToInt32(page);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://gcgk.maoming.gov.cn:8088/html/ZTBXX/index_" + i.ToString() + ".html", Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "text"))), new TagNameFilter("ul"))), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string prjType = nodeList[j].ToPlainTextString();
                        if (prjType.Contains("结果") || prjType.Contains("中标"))
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
                     prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                            prjName = prjType.Replace(prjType.GetDateRegex(), "");
                            bidType = prjName.GetInviteBidType();
                            beginDate = prjType.GetDateRegex();
                            InfoUrl = "http://gcgk.maoming.gov.cn:8088" + nodeList[j].GetATagHref();
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "text")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                bidCtx = HtmlTxt.ToCtxString();
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex();
                                string ctx = bidCtx.ToNodeString();
                                if (string.IsNullOrEmpty(bidUnit))
                                {
                                    bidUnit = ctx.GetRegexBegEnd("中标候选人为", "；").Replace("：", "").Replace(":", "");
                                    if (string.IsNullOrEmpty(bidUnit))
                                    {
                                        ctx = string.Empty;
                                        parser = new Parser(new Lexer(HtmlTxt));
                                        NodeList ctxList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "616")));
                                        if (ctxList != null && ctxList.Count > 0)
                                        {
                                            TableTag tab = ctxList[0] as TableTag;
                                            for (int k = 0; k < tab.RowCount; k++)
                                            {
                                                for (int d = 0; d < tab.Rows[k].ColumnCount; d++)
                                                {
                                                    if (d % 2 == 0)
                                                        ctx += tab.Rows[k].Columns[d].ToPlainTextString().Replace(" ", "").Replace("<?xml:namespaceprefix=ons=\"urn:schemas-microsoft-com:office:office\"/>", "").Replace("\r\n", "") + "：";
                                                    else
                                                        ctx += tab.Rows[k].Columns[d].ToPlainTextString().Replace(" ", "").Replace("\r\n", "") + "\r\n";
                                                }
                                            }
                                        }
                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                                        {
                                            bidMoney = ctx.GetMoneyRegex();
                                        }
                                    }
                                }
                                buildUnit = bidCtx.GetBuildRegex();
                                if (buildUnit.Contains("日期"))
                                {
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("日期")).Replace("日期", "");
                                }
                                prjAddress = bidCtx.GetAddressRegex();
                                if (prjAddress.Contains("联系电话"))
                                {
                                    prjAddress = prjAddress.Remove(prjAddress.IndexOf("联系电话")).Replace("联系电话", "");
                                }
                                prjMgr = bidCtx.GetMgrRegex();
                                msgType = "化州市住房和城乡建设局";
                                specType = "建设工程";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "茂名市区", "化州市", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                       bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                        else
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                            prjName = prjType.Replace(prjType.GetDateRegex(), "");
                            inviteType = prjName.GetInviteBidType();
                            beginDate = prjType.GetDateRegex();
                            InfoUrl = "http://gcgk.maoming.gov.cn:8088" + nodeList[j].GetATagHref();
                            string htlDtl = string.Empty;
                            try
                            {
                                htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                htlDtl = htlDtl.GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "text")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                inviteCtx = HtmlTxt.ToCtxString();

                                buildUnit = inviteCtx.GetBuildRegex(); 
                                code = inviteCtx.GetCodeRegex();
                                prjAddress = inviteCtx.GetAddressRegex();

                                msgType = "化州市住房和城乡建设局";
                                specType = "建设工程";
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "茂名市区", "化州市", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
