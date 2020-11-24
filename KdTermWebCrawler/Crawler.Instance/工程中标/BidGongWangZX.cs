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
using System.Web.UI.HtmlControls;

namespace Crawler.Instance
{
    public class BidGongWangZX : WebSiteCrawller
    {
        public BidGongWangZX()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "中国公网在线";
            this.Description = "自动抓取中国公网在线";
            this.PlanTime = "8:50,9:40,10:30,11:30,13:40,15:00,16:30";
            this.SiteUrl = "http://www.gy-center.net/announce/list.jhtml?action=yes&cid=97&keyword=&visi_id=";
            this.MaxCount = 100;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList noList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "yema")));
            if (noList != null && noList.Count > 0)
            {
                string temp = noList.AsString();
                try
                {
                    Regex reg = new Regex(@"/[^页]+页");
                    string result = reg.Match(temp).Value.Replace("页", "").Replace("/", "");
                    pageInt = Convert.ToInt32(result);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.gy-center.net/announce/list.jhtml?visi_id=&cid=97&chid=&gid=&thistype=&searchcid=&keyword=&action=yes&interval=&page=" + i.ToString(), Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "tab01"))), new TagNameFilter("ul"))), new TagNameFilter("li")));
                if (dtlList != null && dtlList.Count > 0)
                {
                    for (int j = 0; j < dtlList.Count - 1; j++)
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

                        string temp = dtlList[j].ToPlainTextString();
                        string tempHtl = dtlList[j].ToHtml();
                        prjName = ToolHtml.GetHtmlAtagValue("title", tempHtl);
                        beginDate = ToolHtml.GetRegexDateTime(temp);
                        InfoUrl = "http://www.gy-center.net/announce/" + ToolHtml.GetHtmlAtagValue("href", tempHtl);
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                            htlDtl = System.Text.RegularExpressions.Regex.Replace(htlDtl, "(<script)[\\s\\S]*?(</script>)", "");
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList htlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "r_content_right_main")));
                        if (htlList != null && htlList.Count > 0)
                        {
                            HtmlTxt = htlList.ToHtml();
                            bidCtx = Regex.Replace(HtmlTxt, "<[^>]*>", "").Replace("&nbsp;", "").Replace(" ", "").Replace("\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\t\t", "").Replace("\r\r", "\r").Replace("\n\n", "\n");
                            bidType = ToolHtml.GetInviteTypes(prjName);

                            string bidStr = string.Empty;
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList bidList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoNormalTable")));
                            if (bidList != null && bidList.Count > 0)
                            {
                                try
                                {
                                    TableTag tab = bidList[0] as TableTag;
                                    if (tab.RowCount > 1 && tab.Rows[0].ColumnCount > 6)
                                    {
                                        bidStr = tab.Rows[0].Columns[0].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[0].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[1].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[1].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[2].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[2].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[3].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[3].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[4].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[4].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[5].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[5].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[6].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[6].ToPlainTextString().ToNodeString() + "\r\n";
                                    }
                                    else if (tab.RowCount > 1 && tab.Rows[0].ColumnCount > 5)
                                    {
                                        bidStr = tab.Rows[0].Columns[0].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[0].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[1].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[1].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[2].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[2].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[3].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[3].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[4].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[4].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[5].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[5].ToPlainTextString().ToNodeString() + "\r\n";
                                    }
                                    else if (tab.RowCount > 1 && tab.Rows[0].ColumnCount > 4)
                                    {
                                        bidStr = tab.Rows[0].Columns[0].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[0].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[1].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[1].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[2].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[2].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[3].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[3].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[4].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[4].ToPlainTextString().ToNodeString() + "\r\n";
                                    }
                                    else if (tab.RowCount > 1 && tab.Rows[0].ColumnCount > 3)
                                    {
                                        bidStr = tab.Rows[0].Columns[0].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[0].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[1].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[1].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[2].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[2].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[3].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[3].ToPlainTextString().ToNodeString() + "\r\n";
                                    }
                                    else if (tab.RowCount > 1 && tab.Rows[0].ColumnCount > 2)
                                    {
                                        bidStr = tab.Rows[0].Columns[0].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[0].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[1].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[1].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[2].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[2].ToPlainTextString().ToNodeString() + "\r\n";
                                    }
                                    else if (tab.RowCount > 1 && tab.Rows[0].ColumnCount > 1)
                                    {
                                        bidStr = tab.Rows[0].Columns[0].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[0].ToPlainTextString().ToNodeString() + "\r\n";
                                        bidStr += tab.Rows[0].Columns[1].ToPlainTextString().ToNodeString() + "：" + tab.Rows[1].Columns[1].ToPlainTextString().ToNodeString() + "\r\n";

                                    }
                                }
                                catch { }
                            }
                            buildUnit = ToolHtml.GetRegexString(bidCtx, ToolHtml.BuildRegex);
                            prjAddress = ToolHtml.GetRegexString(bidCtx, ToolHtml.AddressRegex);
                            code = ToolHtml.GetRegexString(bidCtx, ToolHtml.CodeRegex);
                            bidUnit = ToolHtml.GetRegexString(bidCtx, ToolHtml.BidRegex);
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                bidUnit = ToolHtml.GetRegexString(bidStr.Replace("  ", ""), ToolHtml.BidRegex, false);
                            }
                            if (string.IsNullOrEmpty(bidUnit))
                            {
                                bidUnit = bidCtx.GetRegexBegEnd("确认", "为");
                            }
                            bidMoney = ToolHtml.GetRegexString(bidCtx, ToolHtml.MoneyRegex);
                            bidMoney = ToolHtml.GetRegexMoney(bidMoney);

                            if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                            {
                                bidMoney = bidCtx.GetRegexBegEnd("￥", "元").GetMoney();
                            }

                            buildUnit = ToolHtml.GetSubString(buildUnit, 150);
                            prjAddress = ToolHtml.GetSubString(prjAddress, 150);
                            code = ToolHtml.GetSubString(code, 50);
                            bidUnit = ToolHtml.GetSubString(bidUnit, 150);

                            bidUnit = ToolHtml.GetStringTemp(bidUnit);
                            buildUnit = ToolHtml.GetStringTemp(buildUnit);

                            if (string.IsNullOrEmpty(code))
                            {
                                code = "见中标信息";
                            }
                            if (string.IsNullOrEmpty(prjAddress))
                            {
                                prjAddress = "见中标信息";
                            }
                            specType = "其他";
                            msgType = "工网在线";
                            BidInfo info = ToolDb.GenBidInfo("广东省", "电网专项工程", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                   bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList nodeAtag = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (nodeAtag != null && nodeAtag.Count > 0)
                            {
                                for (int c = 0; c < nodeAtag.Count; c++)
                                {
                                    ATag a = nodeAtag[c] as ATag;
                                    if (a.Link.IsAtagAttach())
                                    {
                                        string alink = "http://www.bidding.csg.cn/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText.Replace("&nbsp", ""), info.Id, alink);
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
