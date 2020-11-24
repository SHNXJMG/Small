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
    public class BidGuiZhou : WebSiteCrawller
    {
        public BidGuiZhou()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "贵州省住房和城乡建设厅中标公示";
            this.Description = "自动抓取贵州省住房和城乡建设厅中标公示";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 500;
            this.SiteUrl = "http://www.gzjyfw.gov.cn/gcms/queryZjt.jspx?title=&businessCatalog=&businessType=JYJGGS&inDates=0&ext=&origin=ALL";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "pages-list")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("/", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    string pageUrl = string.Format("http://www.gzjyfw.gov.cn/gcms/queryZjt_"+i+".jspx?title=&businessCatalog=&businessType=JYJGGS&inDates=0&ext=&origin=ALL");
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(pageUrl);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("id", "news_list1")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string prjName = string.Empty;
                        ATag aTag = listNode[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        string buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
   bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        area = listNode[j].GetSpan().ToNodePlainString();
                        InfoUrl = aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "contents")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = System.Web.HttpUtility.HtmlDecode(dtlNode.AsHtml()).Replace(" ", "");
                            bidCtx = HtmlTxt.ToLower().Replace("div", "span").Replace("font", "span").Replace("td", "span").GetReplace("<br>,<br/>,<BR>,</p>", "\r\n").ToCtxString().Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").GetReplace("第一中标候选人\r\n,第一中标候选人:\r\n,第一中标候选人：\r\n,第一中标候选人投标人名称\r\n", "第一中标候选人").GetReplace("报价（人民币：元）\r\n,报价\r\n,报价(元):\r\n,报价(元)：\r\n,报价（元）\r\n", "报价").GetReplace("项目经理:\r\n,项目经理：\r\n,技术负责人\r\n,技术负责人:\r\n,技术负责人：\r\n,项目经理姓名\r\n,首席设计师\r\n,项目负责人(项目经理):,项目负责人(项目经理)：", "项目经理:").GetReplace("中标价（万元）：\r\n", "中标价（万元）：").GetReplace("中标价\r\n", "中标价").Replace(" ", "").GetReplace("第二中标候选人", "\r\n第二中标候选人").GetReplace("责任公司", "责任公司\r\n").GetReplace("有限公司", "有限公司\r\n").GetReplace("投标报价:\r\n,投标报价：\r\n,投标报价\r\n", "投标报价:").GetReplace("项目经理:\r\n,项目负责人\r\n,项目经理\r\n,项目总监\r\n,项目总监理工程师\r\n", "项目经理:");
                            code = bidCtx.GetCodeRegex();
                            buildUnit = bidCtx.GetBuildRegex();
                            bidUnit = bidCtx.GetBidRegex(null, false);
                            if (bidUnit.Equals("公司名称"))
                                bidUnit = bidCtx.GetRegex("公司名称");
                            if (bidUnit.Equals("公示"))
                                bidUnit = bidCtx.GetRegex("第一中标候选人", false);
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("投标人名称", false);
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            string[] prjmgrss = new string[] { "总监理工程师", "项目经理或总监或者首席设计师或技术负责人", "负责人", "项目负责人(项目经理)", "项目总监理工程师" };
                            if (string.IsNullOrEmpty(prjMgr))
                                prjMgr = bidCtx.GetMgrRegex(null, false);
                            if (string.IsNullOrEmpty(prjMgr))
                                prjMgr = bidCtx.GetMgrRegex(prjmgrss, false);
                            if (prjMgr.Contains("或总监") || prjMgr.Contains("工程师"))
                                prjMgr = bidCtx.GetMgrRegex(prjmgrss, false);
                            if (prjMgr.Contains("()"))
                                prjMgr = prjMgr.Replace("()", "");
                            if (prjMgr.Contains("（）"))
                                prjMgr = prjMgr.Replace("（）", "");
                            if (prjMgr.Contains("（总监、）"))
                                prjMgr = prjMgr.Replace("（总监、）", "");
                            if (prjMgr.Contains("（姓名）"))
                                prjMgr = prjMgr.Replace("（姓名）", "");
                            if (prjMgr.Contains("得分"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("得分"));
                            if (prjMgr.Contains("投诉"))
                                prjMgr = "";
                            if (prjMgr.Contains("注册"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("注册"));
                            if (prjMgr.Contains("工程"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("工程"));
                            if (prjMgr.Contains("工期"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("工期"));
                            if (prjMgr.Contains("　"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("　"));
                            if (prjMgr.Contains(" "))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf(" "));
                            if (prjMgr.Length == 1 || prjMgr.Contains("资质") || prjMgr.Contains("综合") || prjMgr.Contains("(设总)") || prjMgr.Contains("(设总)") || prjMgr.Contains("执业") || prjMgr.Contains("项目"))
                                prjMgr = "";
                            if (bidUnit.Contains("公司名称"))
                                bidUnit = bidCtx.GetRegex("公司名称");
                            if (bidUnit.Contains("公示变更") || bidUnit.IsNumber())
                                bidUnit = "";
                            bidType = prjName.GetInviteBidType();
                            specType = "建设工程";
                            msgType = "贵州省住房和城乡建设厅";

                            if (buildUnit.Contains("运输局"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("运输局")) + "运输局";
                            if (buildUnit.Contains("管理局"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("管理局")) + "管理局";
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));

                            BidInfo info = ToolDb.GenBidInfo("贵州省", "贵州省及地市", area, string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag fileTag = aNode[a].GetATag();
                                    if (fileTag.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (fileTag.Link.Contains("http"))
                                            link = fileTag.Link;
                                        else
                                            link = "http://www.gzjyfw.gov.cn/" + fileTag.Link;
                                        base.AttachList.Add(ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, link));
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
