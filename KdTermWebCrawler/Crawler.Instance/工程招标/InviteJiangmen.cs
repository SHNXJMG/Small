using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;

namespace Crawler.Instance
{
    /// <summary>
    /// 江门招标信息
    /// </summary>
    public class InviteJiangmen : WebSiteCrawller
    {
        public InviteJiangmen()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省江门市";
            this.Description = "自动抓取广东省江门市招标信息";
            this.ExistCompareFields = "InfoUrl";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.SiteUrl = "http://zyjy.jiangmen.gov.cn//szqjszbgg/index.htm";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        { 
            IList list = new List<InviteInfo>();
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

            NodeList sNodes = parser.ExtractAllNodesThatMatch(new HasParentFilter( new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagesite"))));
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://zyjy.jiangmen.gov.cn//szqjszbgg/index_" + page + ".htm", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
               // NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tab-item itemtw")));
                  NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tab-item itemtw")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                             prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                             specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                             remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                             CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        ATag aTag = listNode[j].GetATag();

                        prjName = aTag.GetAttribute("title");
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "newsCon")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString();

                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                            if (buildUnit.Contains("代理单位"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("代理单位"));
                            if (buildUnit.Contains("联系人") || buildUnit.Contains("代理") || buildUnit.Contains("电话"))
                                buildUnit = "";

                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex().GetChina();

                            inviteType = prjName.GetInviteBidType();

                            msgType = "江门市公共资源交易中心";
                            specType = "建设工程";

                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "江门市区", string.Empty, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://183.237.135.180/" + a.Link;

                                        if(Encoding.Default.GetByteCount(link)<=500)
                                        {
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
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
