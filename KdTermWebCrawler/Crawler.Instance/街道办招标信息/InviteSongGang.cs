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
    public class InviteSongGang : WebSiteCrawller
    {
        public InviteSongGang()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省深圳市松岗街道办事处";
            this.Description = "自动抓取广东省深圳市松岗街道办事处招标信息";
            this.PlanTime = "9:17,13:51";
            this.SiteUrl = "http://sgjd.baoan.gov.cn/zbcg/zbgg_139207/index.html";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch( new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content clearfix")),true),new TagNameFilter("script")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string page = sNode.ToString().Replace("createPageHTML(", "").Replace(",", "kd").Replace("****", "").Replace("\n","");
                    page = page.GetRegexBegEnd("Code", "kd");
                    pageInt = int.Parse(page);
                    //80CGBMPRINKJACJ
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://sgjd.baoan.gov.cn/zbcg/zbgg_139207/index_" + (i-1)+ ".html", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content clearfix")),true),new TagNameFilter("li")));
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
                        prjName = aTag.GetAttribute("title");
                        beginDate = viewList[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link;
                        InfoUrl = InfoUrl.GetRegexBegEnd("./", ".html");
                        InfoUrl = "http://sgjd.baoan.gov.cn/zbcg/zbgg_139207/" + InfoUrl + ".html";
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "con")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            HtmlTxt = dtlList.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            inviteType = prjName.GetInviteBidType();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            if (prjAddress.Contains("电话"))
                                prjAddress = prjAddress.Remove(prjAddress.IndexOf("电话"));
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            if (code.Contains("》"))
                                code = code.Remove(code.IndexOf("》"));
                            if (code.Contains("现对本采购项目的招标事宜公告如下"))
                                code = code.Replace("现对本采购项目的招标事宜公告如下", "");
                            specType = "建设工程";
                            inviteType = "小型工程";
                            msgType = "深圳市宝安区松岗街道办事处";
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "深圳市宝安区松岗街道办事处";
                            }
                            inviteType = ToolHtml.GetInviteType(inviteType);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳区及街道工程", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
