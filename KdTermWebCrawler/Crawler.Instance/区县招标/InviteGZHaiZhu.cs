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
    public class InviteGZHaiZhu : WebSiteCrawller
    {
        public InviteGZHaiZhu()
            : base()
        {
            this.Group = "区县招标信息";
            this.Title = "广东省广州市海珠区公众信息网";
            this.Description = "自动抓取广东省广州市海珠区公众信息网";
            this.PlanTime = "9:32,10:24,14:32,16:34";
            this.SiteUrl = "http://www.haizhu.gov.cn/site/main/news/tzgg/zbtz/index.html";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html.GetJsString()));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "page_bar")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString();
                    Regex reg = new Regex(@"共[^页]页");
                    pageInt = Convert.ToInt32(reg.Match(temp).Value);
                }
                catch { pageInt = 30; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.haizhu.gov.cn/site/main/news/tzgg/zbtz/index_" + (i - 1).ToString() + ".html", Encoding.UTF8);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "table"))), new TagNameFilter("ul"))), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                        prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                        specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                        remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                        CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        INode node = nodeList[j];
                        prjName = node.ToPlainTextString();

                        beginDate = prjName.GetDateRegex();
                        if (prjName.Contains("【"))
                        {
                            prjName = prjName.Remove(prjName.IndexOf("【")).Replace("【", "");
                        }
                        InfoUrl = "http://www.haizhu.gov.cn/site/main/news/tzgg/zbtz/" + node.ToHtml().GetATag().Link.Replace("./", "");
                        inviteType = prjName.GetInviteBidType();
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                            htlDtl = htlDtl.GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "table01")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            HtmlTxt = dtlList.ToHtml();
                            inviteCtx = HtmlTxt.ToCtxString();

                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            code = inviteCtx.GetCodeRegex();
                            msgType = "海珠区公众信息网";
                            specType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "广州市区", "海珠区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
