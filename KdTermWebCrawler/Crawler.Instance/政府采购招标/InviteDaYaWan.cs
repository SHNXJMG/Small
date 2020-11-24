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
using System.Threading;

namespace Crawler.Instance
{
    public class InviteDaYaWan : WebSiteCrawller
    {
        public InviteDaYaWan()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "惠州大亚湾区公共资源交易中心招标采购";
            this.Description = "自动抓取惠州大亚湾区公共资源交易中心招标采购";
            this.PlanTime = "21:08,11:12,14:02,16:33";
            this.SiteUrl = "http://zyjy.dayawan.gov.cn/website/zyjyzx/html/artList.html?cataId=201501031617195932";
            this.MaxCount = 100;
            this.ExistCompareFields = "InfoUrl";
        }
        protected override IList ExecuteCrawl(bool crawlAll)

        {
            IList list = new ArrayList();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
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

            NodeList pageo = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "leftnav")), true), new TagNameFilter("span")));
            if (pageo != null && pageo.Count > 0)
            {
                string pages = pageo.AsString().GetRegexBegEnd("条", "页");
                try
                {

                    pageInt = int.Parse(pages.Replace("/", ""));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&pageNo=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "div_list")), true), new TagNameFilter("ul")));
                if (nodeList != null && nodeList.Count > 0)
                {
                   
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                             prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                             specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                             remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                             CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty,
                             HtmlTxt = string.Empty;


                        ATag aTag = nodeList[j].GetATag();
                         
                        //获取列表行的时间
                        beginDate =nodeList[j].ToPlainTextString().ToString().GetDateRegex();
                        //获取目录标题
                        prjName = aTag.GetAttribute("title");
                        //获取详细页地址
                        string urlId = aTag.Link.ToString();
                        InfoUrl = "http://zyjy.dayawan.gov.cn/" + urlId;
                        string htmlDtl = string.Empty;
                        try
                        {
                            //进入详细页
                            htmlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmlDtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "div_view")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            //获取详细页的信息
                            HtmlTxt = dtlNode.AsHtml();
                            //去掉信息的HTML字符
                            inviteCtx = HtmlTxt.GetReplace("</p>","\r\n").ToCtxString().GetReplace("<o:p>", "\r\n").ToCtxString().Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex(null,true,100);
                            if (string.IsNullOrWhiteSpace(prjAddress))
                                prjAddress = inviteCtx.GetRegex("提交投标文件地点", true, 100);
                            if (string.IsNullOrWhiteSpace(prjAddress))
                                prjAddress = inviteCtx.GetRegex("联系地址：",true,100);
                            inviteType = prjName.GetInviteBidType();
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = "惠州大亚湾区公共资源交易中心招标采购";
                            specType = "建设工程";
                            msgType = "惠州大亚湾区公共资源交易中心招标采购";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "政府采购", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);

                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNodes = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNodes != null && aNodes.Count > 0)
                            {
                                for (int a = 0; a < aNodes.Count; a++)
                                {
                                    ATag aFile = aNodes[a] as ATag;
                                    if (aFile.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (aFile.Link.ToLower().Contains("http"))
                                            link = aFile.Link;
                                        else
                                            link = "http://zyjy.dayawan.gov.cn/" + aFile.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(aFile.LinkText, info.Id, link);
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
