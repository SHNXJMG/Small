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
    public class InviteSzGas : WebSiteCrawller
    {
        public InviteSzGas()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "广东省深圳市燃气集团公司招标信息";
            this.Description = "自动抓取广东省深圳市燃气集团公司招标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.szgas.com.cn/node_200865.htm";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 10;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl);
            }
            catch
            {
                return list;
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl("http://www.szgas.com.cn/node_200865_" + i + ".htm");
                    }
                    catch
                    {
                        continue;
                    }
                }
                Parser parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "news")), true), new TagNameFilter("li")));
                if (tableNodeList.Count > 0)
                {
                    for (int j = 0; j < tableNodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        ATag aTag = tableNodeList[j].GetATag();
                        prjName = aTag.LinkText.Trim();
                        beginDate = tableNodeList[j].ToPlainTextString().GetDateRegex();
                        if (aTag.Link.Contains("http"))
                            InfoUrl = aTag.Link.GetReplace("&#38;", "&");
                        else
                            InfoUrl = "http://www.szgas.com.cn/" + aTag.Link.Trim().GetReplace("&#38;", "&");
                        string[] urls = InfoUrl.Split('?');
                        if (urls.Length > 1)
                            InfoUrl = "http://www.sztc.com/tender/InfoPubDisplay.aspx?" + urls[1];

                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ninfo-con")));
                       
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            inviteCtx = HtmlTxt.Replace("</span>", "\r\n").ToCtxString();
                            //inviteCtx = inviteCtx.Replace("&#160", "").Replace("http://www.szgas.com.cn", "").Replace(";", "").Trim();
                            prjAddress = inviteCtx.GetAddressRegex();
                            
                            code = inviteCtx.GetCodeRegex();
                            if (string.IsNullOrWhiteSpace(code))
                                code = inviteCtx.GetRegexBegEnd("招标编号：", "进行公开招标");
                            if (string.IsNullOrWhiteSpace(code))
                                code = inviteCtx.GetRegexBegEnd("公开招标", "，欢迎");

                            msgType = "深圳燃气集团公司";
                            specType = "建设工程";
                            prjAddress = "见中标信息";
                            buildUnit = "深圳燃气集团公司";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "",
                                     string.Empty, code, prjName, prjAddress, buildUnit,
                                     beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
