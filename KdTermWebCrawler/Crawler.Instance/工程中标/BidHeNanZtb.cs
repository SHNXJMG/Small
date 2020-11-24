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
using System.Threading;

namespace Crawler.Instance
{
    public class BidHeNanZtb : WebSiteCrawller
    {
        public BidHeNanZtb()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "河南省招投标网中标信息";
            this.Description = "自动抓取河南省招投标网中标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 100;
            this.SiteUrl = "http://www.hnsztb.com.cn/zbxx/zhbgg.asp";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        { 
            IList list = new List<BidInfo>();
            int count = 0;
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
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("colspan", "3")), true), new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("align", "center"))));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].ToNodePlainString().GetRegexBegEnd("/", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                //if (i > 1)
                //{
                //    try
                //    {
                //        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?page=" + i, Encoding.Default);
                //    }
                //    catch { continue; }
                //}
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("colspan", "3")), true), new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "100%"))));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        TableRow tr = (listNode[j] as TableTag).Rows[0];
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
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.LinkText;
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.hnsztb.com.cn/zbxx/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "2")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("<br/>,<br />,<br>", "\r\n").ToCtxString();
                            prjAddress = bidCtx.GetAddressRegex().GetCodeDel();
                            buildUnit = bidCtx.GetBuildRegex();
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            code = bidCtx.GetCodeRegex().GetCodeDel();
                            bidUnit = bidCtx.GetBidRegex().GetReplace("A标,B标,C标,第一标段");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("第一中标候选人：");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("中标人名称：");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("竞争谈判第一中标成交候选人：");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("项目负责人：");
                            if (string.IsNullOrWhiteSpace(bidUnit))
                                bidUnit = bidCtx.GetRegex("成交人名称");
                            if (bidUnit.Contains("公司"))
                                bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                            if (bidUnit.Contains("详见")
                                || bidUnit.Contains("/")
                        || bidUnit.Contains("根据"))
                                bidUnit = string.Empty;
                            bidMoney = bidCtx.GetMoneyRegex();
                            try
                            {
                                if (decimal.Parse(bidMoney) > 1000000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }

                            prjMgr = bidCtx.GetMgrRegex();
                            if (prjMgr.Contains("证书"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("证书"));
                            if (prjMgr.Contains("等级"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("等级"));
                            if (prjMgr.Contains("注册"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("注册"));
                            if (prjMgr.Contains("岗位"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("岗位"));
                            if (prjMgr.Contains("（"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("（"));
                            if (prjMgr.Contains("("))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("("));
                            if (prjMgr.Contains("证号"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("证号"));
                            if (prjMgr.Contains(":"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf(":"));
                            if (prjMgr.Contains("："))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("："));
                            if (prjMgr.Contains("中标"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("中标"));
                            if (prjMgr.Contains("级别"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("级别"));
                            if (prjMgr.Contains("质保"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("质保"));
                            if (prjMgr.Contains("工期"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("工期"));

                            msgType = "河南省建设工程招标投标协会";
                            specType = bidType = "建设工程";
                            BidInfo info = ToolDb.GenBidInfo("河南省", "河南省及地市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                            link = "http://www.hnsztb.com.cn/" + a.Link.GetReplace("../,./");
                                        if (Encoding.Default.GetByteCount(link) > 500)
                                            continue;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            //if (count >= 50)
                            //{
                            //    Thread.Sleep(1000 * 60 * 5);
                            //    count = 0;
                            //}
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
