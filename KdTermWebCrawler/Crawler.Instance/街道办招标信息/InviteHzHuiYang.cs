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
    public class InviteHzHuiYang : WebSiteCrawller
    {
        public InviteHzHuiYang()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省惠州市惠阳区信息招标公告";
            this.Description = "自动抓取广东省惠州市惠阳区信息招标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://hyjyzx.huiyang.gov.cn/gcjy/zbgg/index.shtml";
            this.MaxCount = 80;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "noBorder")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToNodePlainString();
                    pageInt = Convert.ToInt32(temp.GetRegexBegEnd("/", "页"));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://hyjyzx.huiyang.gov.cn/gcjy/zbgg/index_" + i + ".shtml", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("UL"), new HasAttributeFilter("class", "list lh24 f14")), true), new TagNameFilter("li")));
                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = viewList[j].GetATag();
                        prjName = aTag.LinkText.Trim();
                        InfoUrl = "http://hyjyzx.huiyang.gov.cn/" + aTag.Link.GetReplace("../");
                        beginDate = viewList[j].ToPlainTextString().GetDateRegex();
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml();
                            if (prjName.Contains(".."))
                            {
                                parser = new Parser(new Lexer(htlDtl));
                                NodeList nameNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("h1"));
                                if (nameNode != null && nameNode.Count > 0)
                                    prjName = nameNode[0].ToNodePlainString();
                            }
                            inviteCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                            inviteType = prjName.GetInviteBidType();

                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            msgType = "惠州市惠阳区公共资源交易中心";

                            specType = "政府采购";

                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "惠州市区", "惠阳区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k].GetATag();
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://hyjyzx.huiyang.gov.cn/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                            if (list.Count % 30 == 0)
                            {
                                Thread.Sleep(1000 * 60 * 10);
                            }
                        }
                    }
                }

            }
            return list;
        }
    }
}
