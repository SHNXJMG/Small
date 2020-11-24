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
namespace Crawler.Instance
{
    public class BidSzjianshe : WebSiteCrawller
    {
        public BidSzjianshe()
            : base(true)
        {
            this.Group = "中标信息";
            this.Title = "广东省深圳市住房和建设局历史中标公告";
            this.Description = "自动抓取广东省深圳市住房和建设局历史中标公告";
            this.PlanTime = "9:30,11:42,14:30,16:30,18:30";
            this.ExistCompareFields = "Prov,Code,City,MsgType";
            this.SiteUrl = "http://61.144.226.2/zbgg/browse.aspx?xxlxbh=2";
            this.MaxCount = 200;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            int sqlCount = 0;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default, ref cookiestr);
            }
            catch (Exception ex)
            {
                return list;
            }
            IList arr = GetPrjCode();
            IList del = arr;
            if (arr.Count > 0)
            {
                for (int d = (arr.Count-1); d >= 0; d--)
                {
                    string htmtxt = string.Empty;
                    viewState = this.ToolWebSite.GetAspNetViewState(htl);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                    string prjcode = arr[d].ToString();
                    NameValueCollection nvc1 = this.ToolWebSite.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "txtPrj_ID", "txtPrj_Name", "Chk_Query", "Radiobuttonlist1", "QUERY", "ucPageNumControl:gotopage" },
                        new string[] { string.Empty, string.Empty, viewState, prjcode, "", "0", "1", "查询", "" });
                    try
                    {
                        htmtxt = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), nvc1, Encoding.Default, ref cookiestr);
                    }
                    catch (Exception ex)
                    {
                        return list;
                    }
                    Parser parser = new Parser(new Lexer(htmtxt));
                    NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "dgConstBid")));
                    if (dtList != null && dtList.Count > 0)
                    {
                        TableTag table = dtList[0] as TableTag;
                        for (int j = 1; j < table.RowCount; j++)
                        {
                            TableRow dr = table.Rows[j];
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
                            prjName = dr.Columns[2].ToPlainTextString().Trim();
                            code = dr.Columns[1].ToPlainTextString().Trim();
                            buildUnit = dr.Columns[3].ToPlainTextString().Trim();
                            ATag aTag = dr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                            InfoUrl = "http://61.144.226.2/zbgg/Detail.aspx?ID=" + aTag.Link.Trim().Replace("GoDetail('", "").Replace("');", "") + "&xxlxbh=2&PRJ_TYPE=0";
                            string htmlde = string.Empty;
                            try
                            {
                                htmlde = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "");
                            }
                            catch { continue; } 
                            parser = new Parser(new Lexer(htmlde));
                            NodeList deList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "Table8"), new TagNameFilter("table")));
                            if (deList != null && deList.Count > 0)
                            {
                                string ctx = string.Empty;
                                HtmlTxt = deList.ToHtml();
                                TableTag tab = deList[0] as TableTag;
                                string text = string.Empty;
                                try
                                {
                                    for (int k = 0; k < tab.RowCount; k++)
                                    {
                                        TableRow tr = tab.Rows[k];
                                        text = tr.Columns[0].ToPlainTextString().Replace(":", "").Replace("：", "").Replace(" ", "") + "：".Trim();
                                        ctx += text + tr.Columns[1].ToPlainTextString().Trim().Replace(" ", "") + "\r\n";
                                    }
                                    for (int k = 0; k < tab.RowCount; k++)
                                    {
                                        TableRow tr = tab.Rows[k];
                                        text = tr.Columns[0].ToPlainTextString().Replace(":", "").Replace("：", "") + "：".Trim();
                                        bidCtx += text + tr.Columns[1].ToPlainTextString().Trim() + "\r\n";
                                    }
                                }
                                catch { }
                                Regex regDate = new Regex(@"发布日期(：|:)[^\r\n]+[\r\n]{1}");
                                string datestr = regDate.Match(bidCtx).Value.Replace("发布日期", "").Replace("：", "").Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                                if (!string.IsNullOrEmpty(datestr))
                                {
                                    try
                                    {
                                        int len = datestr.IndexOf("到");
                                        beginDate = datestr.Substring(0, len);
                                        endDate = datestr.Substring(len + 1, datestr.Length - len - 1);
                                    }
                                    catch { }
                                }

                                Regex regPrjAdd = new Regex(@"(工程地点|工程地址)：[^\r\n]+[\r\n]{1}");
                                prjAddress = regPrjAdd.Match(ctx).Value.Replace("工程地点：", "").Replace("工程地址：", "").Trim();

                                Regex regBidUnit = new Regex(@"(中标单位|中标人)(：|:)[^\r\n]+[\r\n]{1}");
                                bidUnit = regBidUnit.Match(ctx).Value.Replace("中标单位", "").Replace("中标人", "").Replace("：", "").Replace(":", "").Trim();

                                Regex regprjMgr = new Regex(@"项目经理(：|:)[^\r\n]+[\r\n]{1}");
                                prjMgr = regprjMgr.Match(ctx).Value.Replace("项目经理：", "").Trim();

                                Regex regMoney = new Regex(@"(中标价|中标价格)(：|:)[^\r\n]+[\r\n]{1}");
                                bidMoney = regMoney.Match(ctx).Value.Replace("中标价：", "").Replace("中标价格：", "").Replace(",", "").Trim();
                                Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                                if (bidMoney.Contains("万"))
                                {
                                    bidMoney = bidMoney.Remove(bidMoney.IndexOf("万")).Trim();
                                    bidMoney = regBidMoney.Match(bidMoney).Value;
                                }
                                else
                                {
                                    try
                                    {
                                        bidMoney = (decimal.Parse(regBidMoney.Match(bidMoney).Value) / 10000).ToString();
                                        if (decimal.Parse(bidMoney) < decimal.Parse("0.1"))
                                        {
                                            bidMoney = "0";
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        bidMoney = "0";
                                    }
                                }
                                msgType = "深圳市建设工程交易中心";
                                specType = "建设工程";
                                prjName = ToolDb.GetPrjName(prjName);
                                bidType = ToolHtml.GetInviteTypes(prjName);
                                BidInfo info = ToolDb.GenBidInfo("广东省", "深圳市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                if (sqlCount <= this.MaxCount)
                                {
                                    ToolDb.SaveEntity(info, this.ExistCompareFields);
                                    sqlCount++;
                                }
                                else
                                    return list;
                            }
                        }
                    }
                    del.RemoveAt(d);
                    DeleteCode(del);
                }
            }
            return list;
        }

        public void DeleteCode(IList list)
        {
            try
            {
                string path = "E:\\ProjectInfo\\中标.txt";
                File.Delete(path);
                for (int i = 0; i < list.Count; i++)
                {
                    File.AppendAllText(path, list[i].ToString() + "\r\n", Encoding.GetEncoding("GB2312"));
                }
            }
            catch { }
        }

        public IList GetPrjCode()
        {
            IList list = new ArrayList();
            try
            { 
                using (FileStream fileStream = File.OpenRead("E:\\ProjectInfo\\中标.txt")) //选txt文本
                {
                    using (StreamReader streamreader = new StreamReader(fileStream, Encoding.GetEncoding("GB2312")))
                    {
                        string lines = null;
                        while ((lines = streamreader.ReadLine()) != null)
                        {
                            string strs = lines.ToString();
                            list.Add(strs);
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        //public IList Add()
        //{
        //string[] redstr = new string[] { "0", "1", "0", "1" };
        //string[] questr = new string[] { "0", "0", "1", "1" };
        //int page = 1;
        //for (int d = 0; d < 4; d++)
        //    {   

        //        viewState = this.ToolWebSite.GetAspNetViewState(htl);
        //        eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
        //        NameValueCollection nvc1 = this.ToolWebSite.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "txtPrj_ID", "txtPrj_Name", "Chk_Query", "Radiobuttonlist1", "ucPageNumControl:gotopage" },
        //                        new string[] { string.Empty, string.Empty, viewState, "", "", questr[d], redstr[d], "1" });  

        //        try
        //        {
        //            htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(GetStartUrl()),nvc1, Encoding.Default, ref cookiestr);
        //        }
        //        catch (Exception ex)
        //        {
        //            return list;
        //        }
        //        Parser parser = new Parser(new Lexer(htl));
        //        NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ucPageNumControl_lbltotal"), new TagNameFilter("span")));
        //        if (nodeList != null && nodeList.Count > 0)
        //        {
        //            try
        //            {
        //                page = int.Parse(nodeList[0].ToPlainTextString().Trim());
        //            }
        //            catch (Exception)
        //            { page = 1; }
        //        }
        //        for (int i = 1; i <= page; i++)
        //        {
        //            if (i > 1)
        //            {
        //                viewState = this.ToolWebSite.GetAspNetViewState(htl);
        //                eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
        //                NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "txtPrj_ID", "txtPrj_Name", "Chk_Query", "Radiobuttonlist1", "ucPageNumControl:gotopage" },
        //                    new string[] { string.Empty, string.Empty, viewState, "", "", questr[d], redstr[d], i.ToString() });
        //                try { htl = this.ToolWebSite.GetHtmlByUrl(GetStartUrl(), nvc, Encoding.Default, ref cookiestr); }
        //                catch (Exception ex) { continue; }
        //            }
        //            parser = new Parser(new Lexer(htl));
        //            NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "dgConstBid")));
        //            if (dtList != null && dtList.Count > 0)
        //            {
        //                TableTag table = dtList[0] as TableTag;
        //                for (int j = 1; j < table.RowCount; j++)
        //                {
        //                    TableRow dr = table.Rows[j];
        //                    string prjName = string.Empty,
        //                      buildUnit = string.Empty, bidUnit = string.Empty,
        //                      bidMoney = string.Empty, code = string.Empty,
        //                      bidDate = string.Empty,
        //                      beginDate = string.Empty,
        //                      endDate = string.Empty, bidType = string.Empty,
        //                      specType = string.Empty, InfoUrl = string.Empty,
        //                      msgType = string.Empty, bidCtx = string.Empty,
        //                      prjAddress = string.Empty, remark = string.Empty,
        //                      prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
        //                    prjName = dr.Columns[2].ToPlainTextString().Trim();
        //                    code = dr.Columns[1].ToPlainTextString().Trim();
        //                    buildUnit = dr.Columns[3].ToPlainTextString().Trim();
        //                    ATag aTag = dr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
        //                    InfoUrl = "http://61.144.226.2/zbgg/Detail.aspx?ID=" + aTag.Link.Trim().Replace("GoDetail('", "").Replace("');", "") + "&xxlxbh=2&PRJ_TYPE=0";
        //                    string htmlde = string.Empty;
        //                    try
        //                    {
        //                        htmlde = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "");
        //                    }
        //                    catch { continue; }
        //                    if (prjName.Contains("设计"))
        //                    {
        //                        bidType = "设计";
        //                    }
        //                    else if (prjName.Contains("勘察"))
        //                    {
        //                        bidType = "勘察";
        //                    }
        //                    else if (prjName.Contains("服务"))
        //                    {
        //                        bidType = "服务";
        //                    }
        //                    else if (prjName.Contains("监理"))
        //                    {
        //                        bidType = "监理";
        //                    }
        //                    else if (prjName.Contains("施工"))
        //                    {
        //                        bidType = "施工";
        //                    }
        //                    else if (prjName.Contains("劳务分包"))
        //                    {
        //                        bidType = "劳务分包";
        //                    }
        //                    else if (prjName.Contains("专业分包"))
        //                    {
        //                        bidType = "专业分包";
        //                    }
        //                    else if (prjName.Contains("设备"))
        //                    {
        //                        bidType = "设备材料";
        //                    }
        //                    else if (prjName.Contains("材料"))
        //                    {
        //                        bidType = "设备材料";
        //                    }

        //                    parser = new Parser(new Lexer(htmlde));
        //                    NodeList deList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "Table8"), new TagNameFilter("table")));
        //                    if (deList != null && deList.Count > 0)
        //                    {
        //                        string ctx = string.Empty;
        //                        HtmlTxt = deList.ToHtml();
        //                        TableTag tab = deList[0] as TableTag;
        //                        string text = string.Empty;
        //                        try
        //                        {
        //                            for (int k = 0; k < tab.RowCount; k++)
        //                            {
        //                                TableRow tr = tab.Rows[k];
        //                                text = tr.Columns[0].ToPlainTextString().Replace(":", "").Replace("：", "").Replace(" ", "") + "：".Trim();
        //                                ctx += text + tr.Columns[1].ToPlainTextString().Trim().Replace(" ", "") + "\r\n";
        //                            }
        //                            for (int k = 0; k < tab.RowCount; k++)
        //                            {
        //                                TableRow tr = tab.Rows[k];
        //                                text = tr.Columns[0].ToPlainTextString().Replace(":", "").Replace("：", "") + "：".Trim();
        //                                bidCtx += text + tr.Columns[1].ToPlainTextString().Trim() + "\r\n";
        //                            }
        //                        }
        //                        catch { }
        //                        Regex regDate = new Regex(@"发布日期(：|:)[^\r\n]+[\r\n]{1}");
        //                        string datestr = regDate.Match(bidCtx).Value.Replace("发布日期", "").Replace("：", "").Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
        //                        if (!string.IsNullOrEmpty(datestr))
        //                        {
        //                            try
        //                            {
        //                                int len = datestr.IndexOf("到");
        //                                beginDate = datestr.Substring(0, len);
        //                                endDate = datestr.Substring(len + 1, datestr.Length - len - 1);
        //                            }
        //                            catch { }
        //                        }

        //                        Regex regPrjAdd = new Regex(@"(工程地点|工程地址)：[^\r\n]+[\r\n]{1}");
        //                        prjAddress = regPrjAdd.Match(ctx).Value.Replace("工程地点：", "").Replace("工程地址：", "").Trim();

        //                        Regex regBidUnit = new Regex(@"(中标单位|中标人)(：|:)[^\r\n]+[\r\n]{1}");
        //                        bidUnit = regBidUnit.Match(ctx).Value.Replace("中标单位", "").Replace("中标人", "").Replace("：", "").Replace(":", "").Trim();

        //                        Regex regprjMgr = new Regex(@"项目经理(：|:)[^\r\n]+[\r\n]{1}");
        //                        prjMgr = regprjMgr.Match(ctx).Value.Replace("项目经理：", "").Trim();

        //                        Regex regMoney = new Regex(@"(中标价|中标价格)(：|:)[^\r\n]+[\r\n]{1}");
        //                        bidMoney = regMoney.Match(ctx).Value.Replace("中标价：", "").Replace("中标价格：", "").Replace(",", "").Trim();
        //                        Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
        //                        if (bidMoney.Contains("万"))
        //                        {
        //                            bidMoney = bidMoney.Remove(bidMoney.IndexOf("万")).Trim();
        //                            bidMoney = regBidMoney.Match(bidMoney).Value;
        //                        }
        //                        else
        //                        {
        //                            try
        //                            {
        //                                bidMoney = (decimal.Parse(regBidMoney.Match(bidMoney).Value) / 10000).ToString();
        //                                if (decimal.Parse(bidMoney) < decimal.Parse("0.1"))
        //                                {
        //                                    bidMoney = "0";
        //                                }
        //                            }
        //                            catch (Exception)
        //                            {
        //                                bidMoney = "0";
        //                            }
        //                        }

        //                        msgType = "深圳市建设工程交易中心";
        //                        specType = "建设工程";
        //                        prjName = ToolDb.GetPrjName(prjName);
        //                        BidInfo info = ToolDb.GenBidInfo("广东省", "深圳市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
        //                        list.Add(info);
        //                        if (!crawlAll && list.Count >= this.MaxCount) return list;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
