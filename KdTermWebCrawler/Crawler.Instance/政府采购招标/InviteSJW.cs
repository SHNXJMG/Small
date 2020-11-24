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
using System.IO;
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteSJW : WebSiteCrawller
    {
        public InviteSJW()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "深圳市交通运输委员会招标信息";
            this.Description = "自动抓取深圳市交通运输委员会招标信息";
            this.PlanTime = "21:08,11:12,14:02,16:33";
            this.SiteUrl = "http://61.144.227.212/was5/web/search?token=64.1504521027694.76&channelid=235507&templet=jw_list.jsp";
            this.MaxCount = 1000;
            this.ExistCompareFields = "InfoUrl";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList nodePage = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "r_con")), true), new TagNameFilter("a")));
            if (nodePage != null && nodePage.Count > 0)
            {
                try
                {
                    Regex reg = new Regex(@"[0-9]+");
                    string temp = reg.Match(nodePage[nodePage.Count - 1].GetATagHref().Replace("&#39;", "")).Value;
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://61.144.227.212/was5/web/search?page="+i+ "&channelid=235507&token=64.1504521027694.76&perpage=15&outlinepage=10&templet=jw_list.jsp", Encoding.UTF8);
                    }
                    catch { continue; } 
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "zx_ml_list zx_ml_list_right")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 1; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        endDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        if (prjName.Contains("]"))
                        {
                            int len = prjName.LastIndexOf("]");
                            prjName = prjName.Substring(len + 1, prjName.Length - len - 1);
                        }
                        InfoUrl = "http://61.144.227.212/was5/web/" + aTag.Link.Replace("./", "");
                        string htmlDtl = string.Empty;
                        try
                        {
                            htmlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }

                        parser = new Parser(new Lexer(htmlDtl));
                        NodeList nodeDtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "zx_xxgk_cont")));
                        if (nodeDtl != null && nodeDtl.Count > 0)
                        {
                            HtmlTxt = nodeDtl.AsHtml();
                            inviteCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString(); 
                            parser.Reset();
                            NodeList dateNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tit")));
                            if (dateNode != null && dateNode.Count > 0)
                            {
                                beginDate = dateNode.AsString().GetDateRegex();
                            }

                           // NodeList buildNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tit")));


                            buildUnit = inviteCtx.GetBuildRegex();
                            code = inviteCtx.GetCodeRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            inviteType = prjName.GetInviteBidType();
                            specType = "政府采购";
                            msgType = "深圳市交通运输委员会";
                            if (string.IsNullOrEmpty(buildUnit)) buildUnit = msgType;
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳政府采购", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
