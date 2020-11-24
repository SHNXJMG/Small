using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;

namespace Crawler.Instance
{
    /// <summary>
    /// 韶关市
    /// </summary>
    public class InviteShaoguan : WebSiteCrawller
    {
        public InviteShaoguan()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省韶关市";
            this.Description = "自动抓取广东省韶关市招标信息";
            this.ExistCompareFields = "InfoUrl";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.SiteUrl = "http://www.sgjsj.gov.cn/html/news/zbxx/";
        }


        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch{ return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class","pages")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                string pageTemp = tdNodes[0].ToPlainTextString().Trim();
                string paTe = "kd" + pageTemp;
                pageTemp = paTe.GetRegexBegEnd("kd", "条");
                try
                {
                    pageInt = int.Parse(pageTemp);
                }
                catch (Exception ex) { }
                try
                {
                    if (pageInt % 20 > 0)
                    {
                        pageInt = (pageInt / 20) + 1;
                    }
                    else
                    {
                        pageInt = pageInt / 20;
                    }
                }
                catch (Exception ex) { }
            }
            for (int i = 1; i<= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + i + ".html" , Encoding.Default);
                    }
                    catch { continue; };
                }
                 parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "list")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                           prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                           specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                           remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                           CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        prjName = aTag.LinkText;
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link;
                        string dtlBeginDate = string.Empty;

                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));

                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "pd0 par1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            try
                            {
                                Parser parses = new Parser(new Lexer(htmldtl));
                                NodeList codel = parses.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("p"), new HasAttributeFilter("id", "name")));
                                if (codel != null && codel.Count > 0)
                                {
                                    code = codel[0].ToPlainTextString().Trim();
                                }
                            }
                            catch {}
                                if (string.IsNullOrWhiteSpace(code))
                                code = inviteCtx.GetCodeRegex().GetChina();
                            if (code.Contains("）"))
                                code = code.GetReplace("）", "");
                            
                            inviteType = prjName.GetInviteBidType();

                            //inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "韶关市区", string.Empty, string.Empty, code, prjName, prjAddress, buildUnit,
                                beginDate, string.Empty, inviteCtx, string.Empty, "韶关市住房和城乡建设局", inviteType, "建设工程", specType, InfoUrl, HtmlTxt);
                            list.Add(info);
                        }
                        if (!crawlAll && list.Count >= this.MaxCount) return list;      
                    }
                }
            }
            return list;
        }
    }
}
