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
    public class BidGuangzhou : WebSiteCrawller
    {
        public BidGuangzhou()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省广州市区";
            this.Description = "自动抓取广州市区中标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.MaxCount = 60;
            this.SiteUrl = "http://www.gzzb.gd.cn/cms/view/zbhxrlist?channelId=17";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList listTotal = new List<BidInfo>();
            Dictionary<string, string> dic = new Dictionary<string, string>();
          
            dic.Add("房建市政", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=506&channelids=17&pchannelid=466&curgclb=01,02&curxmlb=01,02,03,04,05&curIndex=3&pcurIndex=1");
            dic.Add("交通", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=513&channelids=17&pchannelid=467&curgclb=03&curxmlb=01,02,03,04,05&curIndex=3&pcurIndex=2");
            dic.Add("电力", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=518&channelids=17&pchannelid=468&curgclb=05&curxmlb=01,02,03,04,05&curIndex=3&pcurIndex=3");
            dic.Add("铁路", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=523&channelids=17&pchannelid=469&curgclb=06&curxmlb=01,02,03,04,05&curIndex=4&pcurIndex=4");
            dic.Add("水利", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=528&channelids=17&pchannelid=470&curgclb=04&curxmlb=01,02,03,04,05&curIndex=3&pcurIndex=5");
            dic.Add("民航", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=542&channelids=17&pchannelid=471&curgclb=07&curxmlb=01,02,03,04,05&curIndex=3&pcurIndex=6");
            dic.Add("园林", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=546&channelids=17&pchannelid=472&curgclb=08&curxmlb=01,02,03,04,05&curIndex=3&pcurIndex=7");
            dic.Add("小额", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=531&channelids=17&pchannelid=473&curgclb=&curxmlb=01,02,03,04,05&curIndex=3&pcurIndex=8");
            dic.Add("其他", "http://www.gzggzy.cn/cms/wz/view/index/layout2/szlist.jsp?siteId=1&channelId=538&channelids=17&pchannelid=474&curgclb=13&curxmlb=01,02,03,04,05&curIndex=3&pcurIndex=9");

            foreach (string key in dic.Keys)
            {
                int pageInt = 1;
                string html = string.Empty;
                string viewState = string.Empty;
                string eventValidation = string.Empty;
                string cookiestr = string.Empty;
                int count = 0;
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(dic[key], Encoding.Default);
                }
                catch (Exception ex)
                {
                    continue;
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagination page-mar")), true), new TagNameFilter("ul")));
                if (tdNodes != null && tdNodes.Count > 0)
                {
                    NodeList liNode = new Parser(new Lexer(tdNodes.ToHtml())).ExtractAllNodesThatMatch(new TagNameFilter("li"));
                    if (liNode != null && liNode.Count > 0)
                    {
                        try
                        {
                            string temp = liNode[liNode.Count - 4].GetATagValue("onclick");
                            temp = temp.Replace("goPage", "").Replace("(", "").Replace(")", "").Replace(";", "");
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
                        "page","xmmc","fbrq","xmjdbmid"
                        }, new string[] { i.ToString(), "", "", "" });
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
                            string prjName = string.Empty,
                             buildUnit = string.Empty, bidUnit = string.Empty,
                             bidMoney = string.Empty, code = string.Empty,
                             bidDate = string.Empty,
                             beginDate = string.Empty,
                             endDate = string.Empty, bidType = string.Empty,
                             specType = string.Empty, InfoUrl = string.Empty,
                             msgType = string.Empty, bidCtx = string.Empty,
                             prjAddress = string.Empty, remark = string.Empty,
                             prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            TableRow tr = table.Rows[j];
                            if (tr.ColumnCount < 2) continue;

                            ATag aTag = tr.Columns[2].GetATag(); 
                            code = tr.Columns[1].ToNodePlainString();
                            prjName = aTag.LinkText;
                            beginDate = tr.Columns[4].ToPlainTextString().GetDateRegex();

                            InfoUrl = "http://www.gzggzy.cn" + aTag.Link;
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                            }
                            catch { continue; }

                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "block")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.ToHtml();
                                bidCtx = HtmlTxt.ToCtxString();

                                buildUnit = bidCtx.GetBuildRegex();
                                if (buildUnit.IndexOf("日期") > 0)
                                {
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("日期"));
                                }
                                prjAddress = bidCtx.GetAddressRegex();
                                if (prjAddress.IndexOf("联系") > 0)
                                {
                                    prjAddress = prjAddress.Remove(prjAddress.IndexOf("联系"));
                                }
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tabNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "gridtable")));
                                if (tabNode != null && tabNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag tag = tabNode[0] as TableTag;
                                    for (int k = 1; k < tag.RowCount; k++)
                                    {
                                        ctx += tag.Rows[k].Columns[0].ToNodePlainString() + "：";
                                        ctx += tag.Rows[k].Columns[1].ToNodePlainString() + "\r\n";
                                    }
                                    bidUnit = ctx.GetRegex("单位名称,承包意向人名称");
                                    bidMoney = ctx.GetRegex("投标价（万元）");
                                    prjMgr = ctx.GetRegex("项目经理姓名及资质证书编号,项目负责人姓名及证书编号");
                                    if (prjMgr.IndexOf("/") > 0)
                                    {
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                                    }
                                    if (prjMgr.Contains("见附件"))
                                        prjMgr = string.Empty;
                                    if (prjMgr == "/")
                                        prjMgr = string.Empty;
                                }
                                if (key == "小额" && (bidMoney == "0" || string.IsNullOrEmpty(bidMoney)))
                                    bidMoney = bidCtx.GetRegexBegEnd("本项目交易价为", "万元");
                                msgType = "广州公共资源交易中心";
                                specType = "建设工程";
                                bidType = key;
                                BidInfo info = ToolDb.GenBidInfo("广东省", "广州市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                parser = new Parser(new Lexer(htmldtl));
                                NodeList fileNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "xx-text")), true), new TagNameFilter("a")));
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
                                count++;
                                listTotal.Add(info);
                                if (!crawlAll && count >= this.MaxCount) goto end;
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
