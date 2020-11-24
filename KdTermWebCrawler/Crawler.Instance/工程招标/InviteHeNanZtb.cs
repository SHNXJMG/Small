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
    public class InviteHeNanZtb : WebSiteCrawller
    {
        public InviteHeNanZtb()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "河南省招投标网招标信息";
            this.Description = "自动抓取河南省招投标网招标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 100;
            this.SiteUrl = "http://www.hnsztb.com.cn/zbxx/zbgg.asp";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl,Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"),new HasAttributeFilter("align","center")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode[pageNode.Count-4].ToNodePlainString().GetRegexBegEnd("/", "页");
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
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?page=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                 NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("colspan", "4")), true), new AndFilter( new TagNameFilter("table"),new HasAttributeFilter("width","100%"))));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        TableRow tr = (listNode[j] as TableTag).Rows[0];
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        string area = tr.Columns[0].ToNodePlainString(); 
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.LinkText;
                        endDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.hnsztb.com.cn/zbxx/" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "2")));
                            if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.GetReplace("</DIV>", "\r\n").ToCtxString();
                            prjAddress = inviteCtx.GetAddressRegex().GetCodeDel().GetReplace("　");
                            buildUnit = inviteCtx.GetBuildRegex().GetCodeDel().GetReplace("　");
                            if(string.IsNullOrWhiteSpace(prjAddress))
                                prjAddress = inviteCtx.GetRegex("地址");
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (string.IsNullOrWhiteSpace(buildUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList build = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "2")), true), new TagNameFilter("TD")));
                                if (build != null && build.Count > 0)
                                {
                                    try
                                    {
                                        buildUnit = build[1].ToNodePlainString().GetNotChina();
                                    }
                                    catch { }
                                }
                            }
                            string beginCtx = inviteCtx.GetRegexBegEnd("请于", "，每日");
                            beginDate = beginCtx.GetDateRegex();
                            if (string.IsNullOrEmpty(beginDate))
                                beginDate = beginCtx.GetDateRegex("yyyy年MM月dd日");
                            if (beginDate.Contains("年"))
                                beginDate = beginDate.GetReplace("年","-").GetReplace("月","-").GetReplace("日","");
                            if (string.IsNullOrEmpty(beginDate))
                                beginDate = beginCtx.GetDateRegex("yyyy/MM/dd/");
                            if (string.IsNullOrEmpty(beginDate))
                                beginDate = DateTime.Today.ToString();
                            msgType = "河南省建设工程招标投标协会";
                            specType = inviteType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("河南省", "河南省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://www.hnsztb.com.cn/" + a.Link.GetReplace("../,./");
                                        if (Encoding.Default.GetByteCount(link) > 500)
                                            continue;
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
