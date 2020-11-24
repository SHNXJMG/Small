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
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
 
namespace Crawler.Instance
{
    public class InviteGuangzhou : WebSiteCrawller
    {
        public InviteGuangzhou()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省广州市区";
            this.Description = "自动抓取广州市区招标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.MaxCount = 80;
            this.SiteUrl = "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=503&channelids=15&pchannelid=466&curgclb=01,02&curxmlb=01,02,03,04,05&curIndex=1&pcurIndex=1";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("房建市政", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=503&channelids=15&pchannelid=466&curgclb=01,02&curxmlb=01,02,03,04,05&curIndex=1&pcurIndex=1");
            dic.Add("交通", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=510&channelids=15&pchannelid=467&curgclb=03&curxmlb=01,02,03,04,05&curIndex=1&pcurIndex=2");
            dic.Add("电力", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=515&channelids=15&pchannelid=468&curgclb=05&curxmlb=01,02,03,04,05&curIndex=1&pcurIndex=3");
            dic.Add("铁路", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=520&channelids=15&pchannelid=469&curgclb=06&curxmlb=01,02,03,04,05&curIndex=2&pcurIndex=4");
            dic.Add("水利", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=525&channelids=15&pchannelid=470&curgclb=04&curxmlb=01,02,03,04,05&curIndex=1&pcurIndex=5");
            dic.Add("民航", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=539&channelids=15&pchannelid=471&curgclb=07&curxmlb=01,02,03,04,05&curIndex=1&pcurIndex=6");
            dic.Add("园林", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=543&channelids=15&pchannelid=472&curgclb=08&curxmlb=01,02,03,04,05&curIndex=1&pcurIndex=7");
            dic.Add("小额", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=530&channelids=15&pchannelid=473&curgclb=&curxmlb=01,02,03,04,05&curIndex=2&pcurIndex=8");
            dic.Add("其他", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=535&channelids=15&pchannelid=474&curgclb=13&curxmlb=01,02,03,04,05&curIndex=1&pcurIndex=9");

            IList listTotal = new List<InviteInfo>();
            foreach (string key in dic.Keys)
            {
                IList list = new List<InviteInfo>();
                //取得页码
                int pageInt = 1;
                string html = string.Empty;
                string viewState = string.Empty;
                string eventValidation = string.Empty;
                string cookiestr = string.Empty;
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(dic[key], Encoding.Default);
                }
                catch (Exception ex)
                {
                     continue;
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagination page-mar")),true), new TagNameFilter("ul")));
                if (tdNodes != null && tdNodes.Count > 0)
                {
                    NodeList liNode = new Parser(new Lexer(tdNodes.ToHtml())).ExtractAllNodesThatMatch(new TagNameFilter("li"));
                    if (liNode != null && liNode.Count > 0)
                    {
                        try
                        {
                            string temp = liNode[liNode.Count - 4].GetATagValue("onclick");
                            temp = temp.Replace("goPage", "").Replace("(","").Replace(")","").Replace(";","");
                            pageInt = int.Parse(temp);
                        }
                        catch { }
                    }
                }
                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "page","xmmc","xmjdbmid"
                        },new string[]{i.ToString(),"",""});
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(dic[key], nvc, Encoding.Default);
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "wsbs-table")));
                    if (tableNode != null && tableNode.Count > 0)
                    {
                        TableTag table = tableNode[0] as TableTag;
                        for (int j = 1; j < table.RowCount; j++)
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            TableRow tr = table.Rows[j];
                            ATag aTag = tr.Columns[1].GetATag();
                            prjName = aTag.LinkText;
                            beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                            InfoUrl = "http://www.gzggzy.cn" + aTag.Link;
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "Section1")));
                             if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.AsHtml(); 
                                inviteCtx = HtmlTxt.Replace("</p>","\r\n").ToCtxString();
                                string str = prjName.Replace("...","");
                                if (prjName.Contains("..."))
                                {
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList nameNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("p"));
                                    if (nameNode != null && nameNode.Count > 0)
                                    {
                                        prjName = nameNode[0].ToNodePlainString().Replace("&#8212;", "—").Replace("&", "").Replace("#", "");
                                    } 
                                }
                                if (string.IsNullOrEmpty(prjName)||!prjName.Contains(str)||prjName.Contains("..."))
                                { 
                                    prjName = inviteCtx.GetRegex("工程名称");
                                }
                                if (string.IsNullOrEmpty(prjName) || !prjName.Contains(str) || prjName.Contains("..."))
                                {
                                    prjName = inviteCtx.GetRegex("项目名称");
                                }
                                if (string.IsNullOrEmpty(prjName))
                                {
                                    prjName = str;
                                }

                                code = inviteCtx.GetCodeRegex().Replace("】","").Replace("、","");
                                prjAddress = inviteCtx.GetAddressRegex();
                                buildUnit = inviteCtx.GetBuildRegex();

                                msgType = "广州公共资源交易中心";
                                specType = "建设工程";
                                inviteType = key;
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "广州市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                parser = new Parser(new Lexer(htmldtl));
                                NodeList fileNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter( new TagNameFilter("div"), new HasAttributeFilter("class", "xx-text")),true),new TagNameFilter("a")));
                                if (fileNode != null && fileNode.Count > 0)
                                {
                                    for (int k = 0; k < fileNode.Count; k++)
                                    {
                                        ATag fileAtag = fileNode[k].GetATag();
                                        if (fileAtag.IsAtagAttach())
                                        {
                                            try
                                            {
                                                BaseAttach attach = ToolDb.GenBaseAttach(fileAtag.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, "http://www.gzggzy.cn" + fileAtag.Link);
                                                base.AttachList.Add(attach);
                                            }
                                            catch { }
                                        }
                                    }
                                }
                                listTotal.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) goto end;
                            }
                        }
                    }
                }
            end:
                continue;
            }
            return listTotal;

            
        }
    }
}
