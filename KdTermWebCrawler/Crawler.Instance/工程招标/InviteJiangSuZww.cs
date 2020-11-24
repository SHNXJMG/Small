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
    public class InviteJiangSuZww : WebSiteCrawller
    {
        public InviteJiangSuZww()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "江苏省政务网招标信息";
            this.Description = "自动抓取江苏省政务网招标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.jszwfw.gov.cn/module/jslib/jquery/jpage/dataproxy.jsp?perpage=15&endrecord=45&startrecord=1";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            //http://www.jsggzy.com.cn/services/ZtmkWebservice/getList?response=application/json&pageIndex=1&pageSize=12&fieldvalue=&categorynum=003001&fieldvalue2=%E7%9C%81%E7%BA%A7&xmbh=&xmmc=&_=1497575021808
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                "col",
                "appid",
                "webid",
                "path",
                "columnid",
                "sourceContentType",
                "unitid",
                "webname",
                "permissiontype"
                },
                    new string[]{
                      "1",
                "1",
                "1",
                "/",
                "146",
                "1",
                "363",
                "江苏政务服务网",
                "0"
                    });
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc);
            }
            catch { return null; }

            try
            {
                string temp = html.GetRegexBegEnd("<totalpage>", "</totalpage>");
                pageInt = int.Parse(temp);
            }
            catch { }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                "col",
                "appid",
                "webid",
                "path",
                "columnid",
                "sourceContentType",
                "unitid",
                "webname",
                "permissiontype"
                },
        new string[]{
                      "1",
                "1",
                "1",
                "/",
                "146",
                "1",
                "363",
                "江苏政务服务网",
                "0"
                    });
                    try
                    {
                        int endrecord = i * 45;
                        int startrecord = 45 * i - 44;
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.jszwfw.gov.cn/module/jslib/jquery/jpage/dataproxy.jsp?perpage=15&endrecord=" + endrecord + "&startrecord=" + startrecord, nvc);
                    }
                    catch { continue; }
                }

                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "99%")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                        prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                        specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                        remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                        CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;
                        TableRow tr = (listNode[j] as TableTag).Rows[0];
                        ATag aTag = tr.Columns[1].GetATag();
                        prjName = aTag.GetAttribute("title"); 
                        if (prjName.Contains(" "))
                        {
                            string[] str = prjName.Split(' ');
                            code = str[0];
                            prjName = str[1];
                        }
                        else
                        {
                            string str = prjName.GetNotChina();
                            if (str.Length > 2 && prjName.IsNumber())
                            {
                                try
                                {
                                    int index = prjName.IndexOf(str.Substring(0, 2));
                                    code = prjName.Substring(0, index);
                                    prjName = prjName.Substring(index, prjName.Length - index);
                                }
                                catch { }
                            }
                        }
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.jszwfw.gov.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zoom")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>", "\r\n").ToCtxString();
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            if (buildUnit.Contains("咨询"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("咨询"));
                            if (buildUnit.Contains("电话"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("电话"));
                            if (string.IsNullOrEmpty(code))
                                code = inviteCtx.GetCodeRegex().GetCodeDel();
                            msgType = "江苏省政务服务管理办公室";
                            specType = "政府采购";
                            inviteType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("江苏省", "江苏省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://www.jszwfw.gov.cn/" + a.Link;
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
