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
    public class InviteTaiYuanGgzy : WebSiteCrawller
    {
        public InviteTaiYuanGgzy()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "太原市公共资源交易网招标信息";
            this.Description = "自动抓取太原市公共资源交易网招标信息";
            this.PlanTime = "9:10,11:10,14:10,16:10,18:10";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.SiteUrl = "http://www.tyztb.com/XXFB/XXFBList.aspx?ModuleIndex=2&type=13";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter( new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "pages")),true),new TagNameFilter("a")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count - 1].GetATagHref();
                    temp = temp.Substring(temp.ToLower().IndexOf("p="), temp.Length - temp.ToLower().IndexOf("p=")).GetReplace("P,p,=");
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
                     
                        html = this.ToolWebSite.GetHtmlByUrl("http://ggzy.tylr.gov.cn/JiaoYiInfo.aspx?pn=22-26-1.xtj&p=" + i);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter( new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "newlist pList")),true),new TagNameFilter("a")));
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
                        prjName = aTag.LinkText.GetReplace(" ");
                        beginDate = node.ToPlainTextString().GetDateRegex();
                        if (!string.IsNullOrEmpty(beginDate))
                            prjName = prjName.GetReplace(beginDate);
                        InfoUrl = "http://ggzy.tylr.gov.cn/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "Details")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex().GetReplace("&mdash,&emsp");
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址")) + "地址";
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            msgType = "太原市公共资源交易中心";
                            specType = "政府采购";
                            inviteType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("山西省", "山西省及地市", "太原市", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://ggzy.tylr.gov.cn/" + a.Link.GetReplace("../,./");
                                        if (Encoding.Default.GetByteCount(link) > 500)
                                            continue;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
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
