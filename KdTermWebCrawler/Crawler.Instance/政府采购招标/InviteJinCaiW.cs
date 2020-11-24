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
    public class InviteJinCaiW : WebSiteCrawller
    {
        public InviteJinCaiW()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "中国金融集中采购网招标信息";
            this.Description = "自动抓取中国金融集中采购网招标信息";
            this.PlanTime = "9:40,11:40,14:40,16:40,18:40";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 100;
            //this.SiteUrl = "http://www.cfcpn.com/plist/caigou";
            this.SiteUrl = "http://www.cfcpn.com/plist/caigou?pageNo=1&kflag=0&keyword=&keywordType=&province=&city=&typeOne=&ptpTwo=,";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            int pageInt = 1;
            try
            {
                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                "pageNo",
                "kflag",
                "keyword",
                "keywordType",
                "province",
                "city",
                "typeOne",
                "ptpTwo"
            }, new string[] {
                "1",
                "0",
                "",
                "0",
                "",
                "",
                "",
                ","
            });

                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                }
                catch { }
            }
            catch { return list; }


            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "pagination")), true), new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 2].ToNodePlainString();
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.cfcpn.com/plist/caigou?pageNo="+i+"&kflag=0&keyword=&keywordType=&province=&city=&typeOne=", Encoding.UTF8);

                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "cfcpn_list_content text-left")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                            prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                            specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                            remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                            CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        INode node = listNode[j];
                        ATag aTag = node.GetATag();

                        beginDate = node.ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.cfcpn.com" + aTag.Link;


                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList telNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("p"), new HasAttributeFilter("class", "cfcpn_news_title")));
                        if (telNode != null && telNode.Count > 0)
                        {
                            prjName = telNode.AsHtml();
                            prjName = prjName.ToCtxString();
                        }

                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "news_content")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.GetReplace("<br/>,</p>,<br>,<br />", "\r\n").ToCtxString().Replace("&#160", "");
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();

                            buildUnit = buildUnit.GetReplace("&#160");
                            if (prjAddress.Contains("@"))
                                prjAddress = "";
                            code = code.GetReplace("&#160");
                            specType = "政府采购";
                            inviteType = prjName.GetInviteBidType();
                            msgType = "中国金融集中采购网";
                            InviteInfo info = ToolDb.GenInviteInfo("全国", "金融专项采购", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
