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
    public class InviteDaPengXinQu : WebSiteCrawller
    {
        public InviteDaPengXinQu()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "大鹏新区政府在线招标采购";
            this.Description = "自动抓取大鹏新区政府在线招标采购";
            this.PlanTime = "21:08,11:12,14:02,16:33";
            this.SiteUrl = "http://dp.szzfcg.cn/portal/topicView.do?method=view&siteId=10&id=1660";
            this.MaxCount = 100;
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
            NodeList pageo = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "statusBar")));
            if (pageo != null && pageo.Count > 0)
            {
                string pages = pageo.AsString().GetRegexBegEnd("找到", "条");         
                try
                {
                     
                    pageInt = int.Parse(pages.Replace(",",""));
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://dp.szzfcg.cn/portal/topicView.do?method=view&siteId=10&id=1660", Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "topicChrList_20070702_table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j =3; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                             prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                             specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                             remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                             CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty,
                             HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.GetATag();
                        //ATag aTag = nodeList[j].GetATag();
                        if (aTag == null)
                            continue;
                        //获取列表行的时间
                        beginDate = tr.ToString().GetDateRegex();
                        //获取目录标题
                        prjName = aTag.LinkText.ToNodeString();
                        //获取详细页地址
                        string urlId = aTag.Link.ToString().Replace("/viewer.do?id=", "");
                        InfoUrl = "http://dp.szzfcg.cn/portal/documentView.do?method=view&id=" +urlId;
                        string htmlDtl = string.Empty;
                        try
                        {
                            //进入详细页
                            htmlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmlDtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "93%")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            //获取详细页的信息
                            HtmlTxt = dtlNode.AsHtml();
                            //去掉信息的HTML字符
                            inviteCtx = HtmlTxt.ToCtxString().Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\t\r\n\t", "\r\n\t").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            //parser = new Parser(new Lexer(HtmlTxt));
                            //NodeList dateNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "holder"))); 

                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            inviteType = prjName.GetInviteBidType();
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = "深圳市大鹏新区公共资源交易中心";
                            specType = "建设工程";
                            msgType = "大鹏新区公共资源交易中心";
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
        