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
    public class InviteMinZhi : WebSiteCrawller
    {
        public InviteMinZhi()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省深圳市民治街道办事处";
            this.Description = "自动抓取广东省深圳市民治街道办事处招标信息";
            this.PlanTime = "9:16,13:46";
            this.SiteUrl = "http://www.szlhxq.gov.cn/mzbsc/zwgk69/cgzb/zbgg282/index.html";
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
            catch 
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "yesh fl")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToNodePlainString().GetRegexBegEnd("/","页");
                    pageInt = int.Parse(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.szlhxq.gov.cn/mzbsc/zwgk69/cgzb/zbgg282/14843-" + i.ToString() + ".html", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "news1_list")),true),new TagNameFilter("li")));
                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        beginDate = viewList[j].ToNodePlainString().GetDateRegex();
                        ATag aTag = viewList[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        InfoUrl = "http://www.szlhxq.gov.cn" + aTag.Link;
                        string htmDtl = string.Empty;
                        try
                        {
                            htmDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "tit-content")));
                        if (dtl != null && dtl.Count > 0)
                        { 
                            HtmlTxt = dtl.AsHtml();
                            inviteCtx = System.Text.RegularExpressions.Regex.Replace(HtmlTxt, "(<script)[\\s\\S]*?(</script>)", "");
                            inviteCtx = System.Text.RegularExpressions.Regex.Replace(inviteCtx.Replace("<br/>", "\r\n").Replace("<BR/>", "\r\n").Replace("<BR>", "\r\n").Replace("<br>", "\r\n"), "<[^>]*>", "").Replace("&nbsp;", "").Replace(" ", "").Replace("\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\r\n", "\r\n").Replace("\r\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\t", "").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            inviteType = prjName.GetInviteBidType(); 
                            prjAddress = inviteCtx.GetAddressRegex();
                            if (prjAddress.Contains("**"))
                                prjAddress = string.Empty;
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("资质"))
                            {
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("资质"));
                            } 
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            msgType = "深圳市龙华新区民治街道办事处";
                            if (string.IsNullOrEmpty(prjAddress)) prjAddress = "见中标信息"; 
                            specType = "建设工程";
                            inviteType = "小型工程";  
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "深圳市龙华新区民治街道办事处";
                            }
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
