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
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Data;

namespace Crawler.Instance
{
    public class BidZfNeimeng : WebSiteCrawller
    {
        public BidZfNeimeng()
        {
            this.Group = "政府采购中标信息";
            this.Title = "内蒙古政府采购信息";
            this.Description = "自动抓取内蒙古政府采购中标信息";
            this.SiteUrl = "http://www.nmgp.gov.cn/category/category-ajax.php?type_name=3&byf_page=1&fun=cggg&_=";
            this.PlanTime = "9:40,11:40,14:40,16:40,18:40";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();

            //取得页码
            string html = string.Empty;
            try
            {
                DateTime time = ToolHtml.GetDateTimeByLong(1509517250628);
                DateTime dt24 = DateTime.Now.ToUniversalTime();
                string b = ToolHtml.GetDateTimeLong(dt24).ToString();
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + b, Encoding.Default);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            Parser parser = new Parser(new Lexer(html));
            int pageInt = 1;


            JavaScriptSerializer serializer = new JavaScriptSerializer();

            object[] objs = (object[])serializer.DeserializeObject(html);
            object[] items = objs[1] as object[];
            Dictionary<string, object> smsTypeJson = items[0] as Dictionary<string, object>;
            string a = Convert.ToString(smsTypeJson["page_all"]);
            int page = int.Parse(a);
            pageInt = page / 18 + 1;
            parser.Reset();
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        
                        string lian = "http://www.nmgp.gov.cn/category/category-ajax.php?type_name=3&byf_page=" + i + "&fun=cggg&_=1509441711785";
                        html = this.ToolWebSite.GetHtmlByUrl(lian, Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("分页");
                        continue;
                    }
                }

                parser = new Parser(new Lexer(html));
                JavaScriptSerializer serializer1 = new JavaScriptSerializer();
                object[] objd = (object[])serializer.DeserializeObject(html);
                object[] items1 = objd[0] as object[];
                Dictionary<string, object> smsTypeJson1 = items1[0] as Dictionary<string, object>;
                foreach (KeyValuePair<string, object> obj in smsTypeJson)
                {
                    object[] array = objd[0] as object[];
                    foreach (object arrValue in array)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty,
                            code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                            bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty,
                            bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty,
                            otherType = string.Empty, HtmlTxt = string.Empty, strHtml = string.Empty;
                        Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                        endDate = Convert.ToString(dic["ENDDATE"]).GetDateRegex("yyyy-MM-dd");
                        prjName = Convert.ToString(dic["TITLE"]);
                        string xu = Convert.ToString(dic["wp_mark_id"]);
                        InfoUrl = "http://www.nmgp.gov.cn/ay_post/post.php?tb_id=3&p_id=" + xu;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                        htmldtl = regexHtml.Replace(htmldtl, "");
                        Parser parserdtl = new Parser(new Lexer(htmldtl));
                        Parser dtlparserHTML = new Parser(new Lexer(htmldtl));
                        NodeList nodesDtl = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "center")));
                        if (nodesDtl != null && nodesDtl.Count > 0)
                        {
                            Parser begDate = new Parser(new Lexer(nodesDtl.ToHtml()));
                            NodeList begNode = begDate.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "center")));
                            if (begNode != null && begNode.Count > 0)
                            {
                                beginDate = begNode.AsString().GetDateRegex("yyyy年MM月dd日");
                            }
                            begDate.Reset();
                            NodeList dtlTable = begDate.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("border", "1")));
                            if (dtlTable != null && dtlTable.Count > 0)
                            {
                                TableTag tableDtl = dtlTable[0] as TableTag;
                                if (tableDtl.RowCount > 2)
                                {
                                    string ctx = tableDtl.Rows[2].ToPlainTextString();
                                    bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                                    bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                                }
                                if (bidMoney == "0" && tableDtl.RowCount > 4)
                                {
                                    string ctx = tableDtl.Rows[4].ToPlainTextString();
                                    bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                                    bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                                }
                                if (bidMoney == "0" && tableDtl.RowCount > 6)
                                {
                                    string ctx = tableDtl.Rows[6].ToPlainTextString();
                                    bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                                    bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                                }
                                if (bidMoney == "0" && tableDtl.RowCount > 8)
                                {
                                    string ctx = tableDtl.Rows[8].ToPlainTextString();
                                    bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                                    bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                                }
                                if (bidMoney == "0" && tableDtl.RowCount > 10)
                                {
                                    string ctx = tableDtl.Rows[10].ToPlainTextString();
                                    bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                                    bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                                }
                                if (bidMoney == "0" && tableDtl.RowCount > 12)
                                {
                                    string ctx = tableDtl.Rows[12].ToPlainTextString();
                                    bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                                    bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                                }
                            }
                            HtmlTxt = nodesDtl.ToHtml();
                            bidCtx = HtmlTxt.ToCtxString();

                            code = bidCtx.GetRegex("批准文件编号,工程编号,项目编号").Replace("无", "");
                            code = bidCtx.GetRegexBegEnd("批准文件编号：", "二");
                            buildUnit = bidCtx.GetBuildRegex();
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = bidCtx.GetRegexBegEnd("代理机构名称：", "地址");
                            prjAddress = bidCtx.GetAddressRegex();
                            if (string.IsNullOrEmpty(prjAddress))
                                prjAddress = bidCtx.GetRegexBegEnd("地址：", "邮政编码");


                            msgType = "内蒙古自治区政府采购中心";
                            specType = "政府采购";
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            prjName = ToolDb.GetPrjName(prjName);
                            BidInfo info = ToolDb.GenBidInfo("内蒙古自治区", "内蒙古自治区及盟市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount)
                                return list;
                        }
                }
                }


                //for (int i = 1; i <= pageInt; i++)
                //{
                //    if (i > 1)
                //    {
                //        try
                //        {
                //            html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&pos=" + i.ToString(), Encoding.Default);
                //        }
                //        catch (Exception ex)
                //        {
                //            Logger.Error(ex.ToString());
                //        }
                //    }
                //    parser = new Parser(new Lexer(html));
                //    NodeList nodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "recordlist")));
                //    if (nodes != null && nodes.Count > 0)
                //    {
                //        TableTag table = nodes[0] as TableTag;
                //        for (int t = 0; t < table.RowCount; t++)
                //        { 
                //            string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty,
                //                code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, 
                //                bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty,
                //                bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, 
                //                otherType = string.Empty, HtmlTxt = string.Empty,strHtml=string.Empty;
                //            TableRow tr = table.Rows[t];
                //            endDate = tr.Columns[1].ToPlainTextString().GetDateRegex();
                //            ATag alink = tr.Columns[0].GetATag();
                //            prjName = tr.Columns[0].GetATagValue("title");
                //            InfoUrl = "http://www.nmgp.gov.cn" + alink.Link;
                //            string htmldtl = string.Empty;
                //            try
                //            {
                //                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Trim();
                //            }
                //            catch (Exception ex)
                //            {
                //                continue;
                //            }

                //            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                //            htmldtl = regexHtml.Replace(htmldtl, "");
                //            Parser parserdtl = new Parser(new Lexer(htmldtl));
                //            Parser dtlparserHTML = new Parser(new Lexer(htmldtl));
                //            NodeList nodesDtl = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "hlcms_9")));
                //            if (nodesDtl != null && nodesDtl.Count > 0)
                //            {
                //                Parser begDate = new Parser(new Lexer(nodesDtl.ToHtml()));
                //                NodeList begNode = begDate.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "yzhang")));
                //                if (begNode != null && begNode.Count > 0)
                //                {
                //                    beginDate = begNode.AsString().GetDateRegex("yyyy年MM月dd日");
                //                }
                //                begDate.Reset();
                //                NodeList dtlTable = begDate.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "5")));
                //                if (dtlTable != null && dtlTable.Count > 0)
                //                {
                //                    TableTag tableDtl = dtlTable[0] as TableTag;
                //                    if (tableDtl.RowCount > 2)
                //                    {
                //                        string ctx = tableDtl.Rows[2].ToPlainTextString();
                //                        bidUnit = ctx.GetRegexBegEnd("供应商：","；");
                //                        bidMoney = ctx.GetRegexBegEnd("中标金额：","。").GetMoney();
                //                    }
                //                    if (bidMoney == "0"&& tableDtl.RowCount >4)
                //                    {
                //                        string ctx = tableDtl.Rows[4].ToPlainTextString();
                //                        bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                //                        bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                //                    }
                //                    if (bidMoney == "0" && tableDtl.RowCount > 6)
                //                    {
                //                        string ctx = tableDtl.Rows[6].ToPlainTextString();
                //                        bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                //                        bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                //                    }
                //                    if (bidMoney == "0" && tableDtl.RowCount > 8)
                //                    {
                //                        string ctx = tableDtl.Rows[8].ToPlainTextString();
                //                        bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                //                        bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                //                    }
                //                    if (bidMoney == "0" && tableDtl.RowCount > 10)
                //                    {
                //                        string ctx = tableDtl.Rows[10].ToPlainTextString();
                //                        bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                //                        bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                //                    }
                //                    if (bidMoney == "0" && tableDtl.RowCount > 12)
                //                    {
                //                        string ctx = tableDtl.Rows[12].ToPlainTextString();
                //                        bidUnit = ctx.GetRegexBegEnd("供应商：", "；");
                //                        bidMoney = ctx.GetRegexBegEnd("中标金额：", "。").GetMoney();
                //                    }
                //                } 
                //                HtmlTxt = nodesDtl.ToHtml();
                //                bidCtx = HtmlTxt.ToCtxString();

                //                code = bidCtx.GetRegex("批准文件编号,工程编号,项目编号",true,50).Replace("无", "");
                //                buildUnit = bidCtx.GetBuildRegex();
                //                if (string.IsNullOrEmpty(buildUnit))
                //                    buildUnit = bidCtx.GetRegex("采购代理机构名称,采购单位名称");
                //                prjAddress = bidCtx.GetAddressRegex();
                //                if (string.IsNullOrEmpty(prjAddress))
                //                    prjAddress = bidCtx.GetRegex("投标地点,开标地点,地址");


                //                msgType = "内蒙古自治区政府采购中心";
                //                specType = "政府采购";
                //                bidType = ToolHtml.GetInviteTypes(prjName);
                //                prjName = ToolDb.GetPrjName(prjName); 
                //                BidInfo info = ToolDb.GenBidInfo("内蒙古自治区", "内蒙古自治区及盟市", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                //                list.Add(info);
                //                if (!crawlAll && list.Count >= this.MaxCount)
                //                    return list;
                //            }
                //        }
                //    }
            }
            return list;
        }
    }
}
