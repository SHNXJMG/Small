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
    public class InviteYunFuJS : WebSiteCrawller
    {
        public InviteYunFuJS()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省云浮市建设工程招标信息";
            this.Description = "自动抓取广东省云浮市建设工程招标信息";
            this.ExistCompareFields = "Code,ProjectName,InfoUrl";
            this.SiteUrl = "http://ggzy.yunfu.gov.cn/yfggzy/jsgc/002001/";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "wb-page-items clearfix")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[0].ToPlainTextString().GetRegexBegEnd("/", "转到");
                    page = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?pageing=" + i.ToString(), Encoding.UTF8);
                    }
                    catch  { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList node = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "r-items")),true),new TagNameFilter("li")));
                if (node != null && node.Count > 0)
                { 
                    for (int j = 0; j < node.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty; 
                        ATag aTag = node[j].GetATag();
                        InfoUrl = "http://ggzy.yunfu.gov.cn" + aTag.Link;
                        prjName = aTag.LinkText;
                        beginDate = node[j].ToPlainTextString().GetDateRegex();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").GetJsString();
                        }
                        catch  
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "mainContent")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString().Replace("&ldquo;", "").Replace("&rdquo;", "");
                            string ctx = inviteCtx;
                            if (string.IsNullOrEmpty(inviteCtx))
                            {
                                ctx = dtnode.ToHtml().Replace("</span>", "\r\n").ToCtxString().Replace("-->", "").Replace("&nbsp", "").Replace("。", "").Replace("\r", "\r\n").Replace("\r\n\n\t\r\n\n\t", "\r\n").Replace("\r\n\n\t\r\n\n\t", "\r\n").Replace("\r\n\n\t\r\n\n\t", "\r\n").Replace("\r\n\n\t\r\n\n\t", "\r\n").Replace("\r\n\n\r\n\n", "\r\n").Replace("\r\n\n\r\n\n", "\r\n").Replace("\r\n\n\r\n\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Trim();
                                inviteCtx = dtnode.ToHtml().ToCtxString().Replace("-->", "").Replace("&nbsp", "").Replace("。", "").Replace("\r", "\r\n").Replace("\r\n\n\t\r\n\n\t", "\r\n").Replace("\r\n\n\t\r\n\n\t", "\r\n").Replace("\r\n\n\t\r\n\n\t", "\r\n").Replace("\r\n\n\t\r\n\n\t", "\r\n").Replace("\r\n\n\r\n\n", "\r\n").Replace("\r\n\n\r\n\n", "\r\n").Replace("\r\n\n\r\n\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Trim();
                                Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                                inviteCtx = regexHtml.Replace(inviteCtx, "").Replace("（盖章）", "").Replace(" ", "").Trim();
                                ctx = regexHtml.Replace(ctx, "").Replace("（盖章）", "").Replace(" ", "").Trim();
                            }
                            buildUnit = ctx.GetBuildRegex();
                            prjAddress = ctx.GetAddressRegex();
                            msgType = "云浮市建设工程交易中心";
                            specType = "建设工程";
                            if (buildUnit.Contains("招标代理机构"))
                            {
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理机构")).ToString().Trim();
                            }
                            if (buildUnit.Contains("招标代理"))
                            {
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理")).ToString().Trim();
                            }
                            buildUnit = buildUnit.Replace("：潘先生联系电话：0766--3481216", "").Trim();

                            parserdetail.Reset();
                            NodeList nameNode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("h2"),new HasAttributeFilter("class", "post-title")));
                            if (nameNode != null && nameNode.Count > 0)
                            {
                                string tempName = nameNode[0].ToNodePlainString();
                                if (!string.IsNullOrWhiteSpace(tempName))
                                    prjName = tempName;
                            }
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "云浮市区", "",
                             string.Empty, code, prjName, prjAddress, buildUnit,
                             beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parserdetail = new Parser(new Lexer(HtmlTxt));
                            NodeList file = parserdetail.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (file != null && file.Count > 0)
                            {
                                for (int d = 0; d < file.Count; d++)
                                {
                                    ATag aFile = file.SearchFor(typeof(ATag), true)[d] as ATag;
                                    if (aFile.IsAtagAttach())
                                    {
                                        string downloadURL = "http://ggzy.yunfu.gov.cn/" + aFile.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(aFile.LinkText, info.Id, downloadURL);
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
