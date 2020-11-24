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
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteGdZb : WebSiteCrawller
    {
        public InviteGdZb()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省招投标监督网招标信息";
            this.Description = "自动抓取广东省招投标监督网招标信息";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.gdzbtb.gov.cn/zhaobiao12/index.htm";
            this.MaxCount = 2000;
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
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "cn6")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().Replace("(", "kdxx").GetRegexBegEnd("kdxx", ",");
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.gdzbtb.gov.cn/zhaobiao12/index_" + (i - 1).ToString() + ".htm", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "position2")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                     prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                     specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                     remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                     CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty,
                     city = string.Empty;

                        prjName = nodeList[j].GetATagValue("title");
                        if (prjName.Contains("广东省"))
                        {
                            city = "广州市区";
                            prjName = prjName.Replace("[", "").Replace("]", "").Replace("广东省", "");
                        }
                        else
                        {
                            string temp = prjName.Replace("[", "kdxx").Replace("]", "xxdk").GetRegexBegEnd("kdxx", "xxdk");
                            prjName = prjName.Replace("[", "").Replace("]", "").Replace(temp, "");
                            city = temp + "区";
                        }

                        InfoUrl = "http://www.gdzbtb.gov.cn/zhaobiao12/" + nodeList[j].GetATagHref().Replace("../", "").Replace("./", "");
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "article")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();

                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex();

                            if (inviteCtx.IndexOf("发布日期") != -1)
                            {
                                string ctx = inviteCtx.Substring(inviteCtx.IndexOf("发布日期"), inviteCtx.Length - inviteCtx.IndexOf("发布日期"));
                                beginDate = ctx.GetDateRegex();
                            }
                            else if (inviteCtx.IndexOf("发布时间") != -1)
                            {
                                string ctx = inviteCtx.Substring(inviteCtx.IndexOf("发布时间"), inviteCtx.Length - inviteCtx.IndexOf("发布时间"));
                                beginDate = ctx.GetDateRegex();
                            }
                            if (string.IsNullOrEmpty(beginDate))
                            {
                                beginDate = DateTime.Now.ToString("yyyy-MM-dd");
                            }
                            inviteType = prjName.GetInviteBidType();
                            specType = "建设工程";
                            msgType = "广东省招标投标监管网";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", city, "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
