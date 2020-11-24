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
    public class BidSZLonggang : WebSiteCrawller
    {
        public BidSZLonggang()
            : base(true)
        {
            this.Group = "中标信息";
            this.Title = "广东省深圳市龙岗区";
            this.Description = "自动抓取广东省深圳市龙岗区中标信息";
            this.SiteUrl = "http://61.144.224.189:8001/LGjyzxWeb/SiteManage/PublicInfoList.aspx?MenuName=PublicInformation&ModeId=4&ItemId=zbgs&ItemName=%e4%b8%ad%e6%a0%87%e5%85%ac%e7%a4%ba&clearpaging=true";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();

            DateTime start = DateTime.Parse("2016-11-30");
            DateTime end = DateTime.Parse("2016-12-14");
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookieStr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8,ref cookieStr);
            }
            catch (Exception ex)
            {

                return list;
            }

            Parser parser = new Parser(new Lexer(html));

            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_InfoList2_GridViewPaging1_PagingDescTd"), new TagNameFilter("td")));
            string pageString = sNode.AsString();
            Regex regexPage = new Regex(@"，共[^页]+页");
            Match pageMatch = regexPage.Match(pageString);
            try { pageInt = int.Parse(pageMatch.Value.Replace("，共", "").Replace("页", "").Trim()); }
            catch (Exception) { }

            string cookiestr = string.Empty;
            for (int i = 5; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                      new string[] {
                          "ctl00$ScriptManager1",
                            "__EVENTTARGET",
                            "__EVENTARGUMENT",
                            "__LASTFOCUS",
                            "__VIEWSTATE",
                            "ctl00$cph_context$InfoList2$ddlProjectType",
                            "ctl00$cph_context$InfoList2$ddlSearch",
                            "ctl00$cph_context$InfoList2$txtProjectName",
                            "ctl00$cph_context$InfoList2$GridViewPaging1$txtGridViewPagingForwardTo",
                            "__VIEWSTATEENCRYPTED",
                            "ctl00$cph_context$InfoList2$GridViewPaging1$btnForwardToPage" },
                        new string[] {
                            "ctl00$cph_context$InfoList2$update1|ctl00$cph_context$InfoList2$GridViewPaging1$btnForwardToPage",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            viewState,
                            string.Empty,
                            "gcbh",
                            string.Empty,
                            i.ToString(),
                            "",
                            "GO" });
                    try
                    {
                        //string postDatas = string.Empty;
                        //foreach(string post in nvc.AllKeys)
                        //{
                        //    postDatas += string.Format("{0}={1}&", post, nvc.GetValues(post));
                        //}
                        //postDatas = postDatas.Remove(postDatas.Length - 1, 1);
                        //html = ToolHtml.GetHtmlByUrlPost(this.SiteUrl, postDatas, Encoding.UTF8, ref cookieStr);
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch(Exception ex) { continue; }
                }

                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_InfoList2_GridView1"), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j] as TableRow;
                        code = tr.Columns[1].ToPlainTextString().Trim();
                        prjName = tr.Columns[2].ToPlainTextString().Trim();
                        buildUnit = tr.Columns[3].ToPlainTextString().Trim();
                        beginDate = tr.Columns[5].ToPlainTextString().Trim();
                        endDate = tr.Columns[6].ToPlainTextString().Trim();
                        string InvType = tr.Columns[4].ToPlainTextString().Trim();
                        bidType = ToolHtml.GetInviteTypes(InvType);
                        ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://61.144.224.189:8001/LGjyzxWeb/SiteManage/" + aTag.Link.Replace("openNewWindowByMenu(\"", "").Replace("\")", "");
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_lblContent"), new TagNameFilter("span")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                        }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_cph_context_lblContent"), new TagNameFilter("span")));
                        bidCtx = dtnode.AsString().Replace(" ", "");

                        Regex regPrjAdd = new Regex(@"(工程地点|工程地址)：[^\r\n]+[\r\n]{1}");
                        prjAddress = regPrjAdd.Match(bidCtx).Value.Replace("工程地点：", "").Replace("工程地址：", "").Trim();
                        msgType = "深圳市建设工程交易中心龙岗分中心";
                        specType = "建设工程";
                        Regex regMoney = new Regex(@"(中标价)：[^\r\n]+[\r\n]{1}");
                        bidMoney = regMoney.Match(bidCtx).Value.Replace("中标价：", "").Replace("万元", "").Trim();
                        Regex regprjMgr = new Regex(@"(项目经理|项目负责人|项目总监|建造师)：[^\r\n]+[\r\n]{1}");
                        prjMgr = regprjMgr.Match(bidCtx).Value.Replace("项目经理：", "").Trim();
                        Regex regBidUnit = new Regex(@"(中标人|中标单位)：[^\r\n]+[\r\n]{1}");
                        bidUnit = regBidUnit.Match(bidCtx).Value.Replace("中标人：", "").Replace("中标单位", "").Trim();
                        Regex regOtherType = new Regex(@"(工程类型)：[^\r\n]+[\r\n]{1}");
                        string oType = regOtherType.Match(bidCtx).Value.Replace("工程类型：", "").Trim();
                        if (oType.Contains("房建"))
                        {
                            otherType = "房建及工业民用建筑";
                        }
                        if (oType.Contains("市政"))
                        {
                            otherType = "市政工程";
                        }
                        if (oType.Contains("园林绿化"))
                        {
                            otherType = "园林绿化工程";
                        }
                        if (oType.Contains("装饰装修"))
                        {
                            otherType = "装饰装修工程";
                        }
                        if (oType.Contains("电力"))
                        {
                            otherType = "电力工程";
                        }
                        if (oType.Contains("水利"))
                        {
                            otherType = "水利工程";
                        }
                        if (oType.Contains("环保"))
                        {
                            otherType = "环保工程";
                        }
                        if (Encoding.Default.GetByteCount(bidUnit) > 150)
                        {
                            bidUnit = "";
                        }
                        if (Encoding.Default.GetByteCount(prjMgr) > 50)
                        {
                            prjMgr = "";
                        }
                        //prjName = ToolDb.GetPrjName(prjName);
                        BidInfo info = ToolDb.GenBidInfo("广东省", "深圳龙岗区工程", "龙岗区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);

                        if (info.BeginDate < start) return list;

                        if (info.BeginDate > start && info.BeginDate < end)
                            list.Add(info);
                        else
                            continue;
                        dtlparser.Reset();



                        NodeList fileNode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_AccessoriesControl1_GridView1")));
                        if (fileNode != null && fileNode.Count > 0 && fileNode[0] is TableTag)
                        {
                            TableTag fileTable = fileNode[0] as TableTag;
                            for (int f = 1; f < fileTable.Rows.Length; f++)
                            {
                                BaseAttach attach = ToolDb.GenBaseAttach(fileTable.Rows[f].Columns[0].ToPlainTextString().Trim(), info.Id, "http://jyzx.cb.gov.cn/LGjyzxWeb/" + (fileTable.Rows[f].Columns[0].SearchFor(typeof(ATag), true)[0] as ATag).Link.Replace("../", ""));
                                base.AttachList.Add(attach);
                            }

                        }

                       

                        if (!crawlAll && list.Count >= this.MaxCount)
                            return list;
                    }
                }

            }



            return list;
        }
    }
}
