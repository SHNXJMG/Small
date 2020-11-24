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
    public class InviteJiLinGgzy : WebSiteCrawller
    {
        public InviteJiLinGgzy()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "吉林省公共资源交易中心招标信息";
            this.Description = "自动抓取吉林省公共资源交易中心招标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://ggzyjy.jl.gov.cn/JiLinZtb/Template/Default/MoreInfoJYXX.aspx?CategoryNum=004001";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default, ref cookiestr); 
            }
            catch {  }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("nowrap", "true")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("总页数：", "当前");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__LASTFOCUS",
                    "__VIEWSTATE",
                    "__EVENTVALIDATION"
                    }, new string[]{
                    "Pager",
                    i.ToString(),
                    "",
                    viewState,
                    eventValidation
                   
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl,nvc,Encoding.Default, ref cookiestr);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("tr"));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        TableRow tr = listNode[j] as TableRow;
                        if (tr.ColumnCount != 6) continue;
                        ATag aTag = tr.GetATag();
                        if (aTag == null) continue;
                        string prjType = tr.Columns[2].ToNodePlainString().GetReplace("[", "【").GetReplace("]", "】").GetRegexBegEnd("【", "】");
                        if (!prjType.Contains("水利工程") && !prjType.Contains("建设工程") && !prjType.Contains("交通工程"))
                            continue;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                       prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                       specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                       remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                       CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        
                        
                        prjName = aTag.GetAttribute("title");
                        InfoUrl = "http://ggzyjy.jl.gov.cn/JiLinZtb/" + aTag.Link.GetReplace("../,./");
                        string area = tr.Columns[3].ToNodePlainString().GetReplace("[", "【").GetReplace("]", "】").GetRegexBegEnd("【", "】");
                        endDate = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList beginNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "tdTitle")));
                        if(beginNode!=null&&beginNode.Count>0)
                        {
                            beginDate = beginNode.AsString().GetDateRegex("yyyy/MM/dd");
                        }
                        parser.Reset();
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToLower().GetReplace("</p>,<br/>,<br>","\r\n").ToCtxString();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            msgType = "吉林省公共资源交易中心";
                            specType = "建设工程";
                            inviteType = prjType;
                            InviteInfo info = ToolDb.GenInviteInfo("吉林省", "吉林省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://ggzyjy.jl.gov.cn/" + a.Link.GetReplace("../,./");
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
