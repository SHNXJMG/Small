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
    public class InviteGuangMingSZ : WebSiteCrawller
    {
        public InviteGuangMingSZ()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省深圳市光明新区招标公告";
            this.Description = "自动抓取广东省深圳市光明新区招标公告";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl,MsgType";
            this.SiteUrl = "http://www.szgm.gov.cn/szgm/132100/xwdt17/135204/151246/index.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(htl));

            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "easysite-total-page")));
            if (nodeList != null && nodeList.Count > 0)
            {
                string temp = nodeList.AsString();
                try
                {
                    page = int.Parse(temp.GetRegexBegEnd("1/", "\n"));
                }
                catch { }
            }
            if(page==1)
                page = 42;
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl("http://www.szgm.gov.cn/szgm/132100/xwdt17/135204/151246/8d25503a-" + i.ToString() + ".html", Encoding.UTF8);
                    }
                    catch { return list; }
                }
                parser = new Parser(new Lexer(htl));
              //  NodeList tabList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellspacing", "0")), true), new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellspacing", "0"))));

               NodeList tabList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellspacing", "0"))), new TagNameFilter("tr")));

                //NodeList tabList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "easysite-article-li")));
                if (tabList != null && tabList.Count > 0)
                {
                    for (int j = 0; j < tabList.Count; j++)
                    {
                        ATag aTag = null;
                        TableRow tr = null;
                        try
                        {
                            tr = (tabList[j] as TableTag).Rows[0];
                             aTag = tr.GetATag();
                            if (aTag == null || tr.ColumnCount != 3) continue;
                        }
                        catch { continue; }
                        string code = string.Empty, buildUnit = string.Empty,
                            prjName = string.Empty, prjAddress = string.Empty,
                            inviteCtx = string.Empty, bidType = string.Empty,
                            specType = string.Empty, beginDate = string.Empty,
                            endDate = string.Empty, remark = string.Empty,
                            inviteType = string.Empty, InfoUrl = string.Empty,
                            CreateTime = string.Empty, msgType = string.Empty,
                            otherType = string.Empty, HtmlTxt = string.Empty;

                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        prjName = aTag.GetAttribute("title");

                        InfoUrl = "http://www.szgm.gov.cn" + aTag.Link;
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "article_body")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();

                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetBuildRegex();
                            inviteType = prjName.GetInviteBidType();
                            specType = "政府采购";
                            msgType = "深圳市光明新区";
                            if (string.IsNullOrEmpty(buildUnit)) buildUnit = msgType;
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳区及街道工程", "光明新区",
                                   string.Empty, code, prjName, prjAddress, buildUnit,
                                   beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
