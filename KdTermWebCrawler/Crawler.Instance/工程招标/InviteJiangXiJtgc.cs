using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class InviteJiangXiJtgc:WebSiteCrawller
    {
        public InviteJiangXiJtgc()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "江西省公共资源交易中心招标信息(交通工程)";
            this.Description = "自动抓取江西省公共资源交易中心招标信息(交通工程)";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.jxsggzy.cn/web/jyxx/002002/002002002/jyxx.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "wb-page-li")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("/", "\r");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {

                SiteUrl = "http://www.jxsggzy.cn/web/jyxx/002002/002002002/" + i + ".html";
                try
                {
                    html = ToolWeb.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8, ref cookiestr);
                }
                    catch { continue; }
                
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "ewb-list-node clearfix")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j =0; j < listNode.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;
                        ATag aTag = listNode[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        if (string.IsNullOrWhiteSpace(prjName))
                            prjName = aTag.LinkText;
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        if (prjName[2].Equals('县') || prjName[2].Equals('区') || prjName[2].Equals('市'))
                            area = prjName.Substring(0, 3);
                        InfoUrl = "http://www.jxsggzy.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "article-info")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            prjAddress = inviteCtx.GetAddressRegex();
                            if (string.IsNullOrWhiteSpace(prjAddress))
                                prjAddress = inviteCtx.GetRegexBegEnd("地址：", "地址：");

                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("单位章"))
                                buildUnit = string.Empty;
                            if (buildUnit.Contains("联系人"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系人"));
                            code = inviteCtx.GetCodeRegex();
                            inviteType = "交通工程";
                            specType = "政府采购";
                            msgType = "江西省公共资源交易中心";
                            InviteInfo info = ToolDb.GenInviteInfo("江西省", "江西省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
