using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;

namespace Crawler.Instance
{
    /// <summary>
    /// 中标信息--江门
    /// </summary>
    public class BidJiangmen : WebSiteCrawller
    {
        public BidJiangmen()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省江门市";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.Description = "自动抓取广东省江门市中标信息";
            this.ExistCompareFields = "InfoUrl";//"Prov,City,Area,Road,Code,ProjectName,BidUnit";
            this.SiteUrl = "http://zyjy.jiangmen.gov.cn//zbgs/index.htm";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));


            NodeList sNodes = parser.ExtractAllNodesThatMatch(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagesite"))));
            if (sNodes != null && sNodes.Count > 0)
            {
                string strPage = sNodes.AsString().GetRegexBegEnd("/", "页");
                try
                {
                    pageInt = int.Parse(strPage);
                }
                catch { }
            }
            for (int page = 1; page <= pageInt; page++)
            {
                if (page > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://zyjy.jiangmen.gov.cn//zbgs/index_" + page + ".htm", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                //NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "c1-bline")));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tab-item itemtw")), true), new TagNameFilter("li")));

                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string prjName = string.Empty,
                          buildUnit = string.Empty, bidUnit = string.Empty,
                          bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty,beginDate = string.Empty,
                          endDate = string.Empty, bidType = string.Empty,
                          specType = string.Empty, InfoUrl = string.Empty,
                          msgType = string.Empty, bidCtx = string.Empty,
                          prjAddress = string.Empty, remark = string.Empty,
                          prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = listNode[j].GetATag();

                        prjName = aTag.GetAttribute("title");
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "newsCon")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();


                           // bidUnit = bidCtx.GetReplace("\r\n", "kdxx").GetRegexBegEnd("kdxx", "公司", 5000);//.GetReplace("kdxx", "\r\n");
                            Regex reg = new Regex("(?<=(kdxx))[.\\s\\S]*?(?=(公司))", RegexOptions.Multiline | RegexOptions.Singleline);

                            bidUnit = reg.Match(bidCtx.GetReplace("\r\n", "kdxx")).Value.GetReplace("kdxx", "\r\n");
                            if (bidUnit.Contains("\t") || bidUnit.Contains("\r") || bidUnit.Contains("\n"))
                            {
                                List<int> index = new List<int>();
                                index.Add(bidUnit.LastIndexOf("\t"));
                                index.Add(bidUnit.LastIndexOf("\r"));
                                index.Add(bidUnit.LastIndexOf("\n"));
                                bidUnit = bidUnit.Substring(index.Max(), bidUnit.Length - index.Max()).GetReplace("\r,\t,\n")+"公司";
                            }
                            bidMoney = bidCtx.GetRegexBegEnd("中标价","元").GetMoney();
                            prjMgr = bidCtx.GetRegexBegEnd("项目经理", ",").GetReplace(":,：").GetReplace("\r,\t,\n");
                            if(string.IsNullOrWhiteSpace(prjMgr))
                                prjMgr = bidCtx.GetRegexBegEnd("项目经理", "，").GetReplace(":,：").GetReplace("\r,\t,\n");
                            prjMgr = prjMgr.GetReplace("（姓名）,(姓名)");
                            if (prjMgr.Contains("；"))
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("；"));

                            //buildUnit = bidCtx.GetBuildRegex();
                            //if (buildUnit.Contains("联系人") || buildUnit.Contains("代理机构") || buildUnit.Contains("电话"))
                            //    buildUnit = "";

                            //prjAddress = bidCtx.GetAddressRegex();
                            //code = bidCtx.GetCodeRegex();

                            bidType = prjName.GetInviteBidType();

                            msgType = "江门市公共资源交易中心";
                            specType = "建设工程";

                            BidInfo info = ToolDb.GenBidInfo("广东省", "江门市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag fileTag = aNode[a] as ATag;
                                    if (fileTag.IsAtagAttach())
                                    {
                                        string fileUrl = string.Empty;
                                        if (fileTag.Link.Contains("http"))
                                            fileUrl = fileTag.Link;
                                        else
                                            fileUrl = "http://zyjy.jiangmen.gov.cn/" + fileTag.Link;

                                        base.AttachList.Add(ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, fileUrl));
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
