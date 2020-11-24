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

namespace Crawler.Instance
{
    public class InviteGuangMingJsGc : WebSiteCrawller
    {
        public InviteGuangMingJsGc()
            : base()  
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省深圳市光明新区建设工程招标公告";
            this.Description = "自动抓取广东省深圳市光明新区建设工程招标公告";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.szgm.gov.cn/gmbscn/144192/144212/144216/index.html";
            this.MaxCount = 100;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(htl));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "Normal")));
            try
            {
                string temp = pageNode.AsString().GetRegexBegEnd("/", "跳");
                page = int.Parse(temp);
            }
            catch { }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl("http://www.szgm.gov.cn/gmbscn/144192/144212/144216/0f8b8505-" + i.ToString() + ".html", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "jwRercon")), true),new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        INode node = nodeList[j];
                        ATag aTag = node.GetATag(); 
                        string code = string.Empty, buildUnit = string.Empty,
                            prjName = string.Empty, prjAddress = string.Empty,
                            inviteCtx = string.Empty, bidType = string.Empty,
                            specType = string.Empty, beginDate = string.Empty,
                            endDate = string.Empty, remark = string.Empty,
                            inviteType = string.Empty, InfoUrl = string.Empty,
                            CreateTime = string.Empty, msgType = string.Empty,
                            otherType = string.Empty, HtmlTxt = string.Empty;

                        beginDate = node.ToPlainTextString().GetDateRegex();
                        prjName = aTag.GetAttribute("title");

                        InfoUrl = "http://www.szgm.gov.cn" + aTag.Link;
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch {   continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "jwRercon")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();

                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetBuildRegex();
                            inviteType = prjName.GetInviteBidType();
                            specType = "建设工程";
                            msgType = "深圳市光明新区";
                            if (string.IsNullOrEmpty(buildUnit)) buildUnit = msgType;
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳区及街道工程", "光明新区",
                                   string.Empty, code, prjName, prjAddress, buildUnit,
                                   beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if(aNode!=null&&aNode.Count>0)
                            {
                                for(int a=0;a<aNode.Count;a++)
                                {
                                    ATag tag = aNode[a] as ATag;
                                    if(tag.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (tag.Link.Contains("http"))
                                            link = tag.Link;
                                        else
                                            link = "http://www.szgm.gov.cn" + tag.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(tag.LinkText, info.Id, link);
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
