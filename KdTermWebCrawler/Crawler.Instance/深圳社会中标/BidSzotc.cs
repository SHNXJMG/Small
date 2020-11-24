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


namespace Crawler.Instance
{
    public class BidSzotc : WebSiteCrawller
    {
        public BidSzotc()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "深圳市东方招标有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳市东方招标有限公司中标信息";
            this.MaxCount = 20;
            this.SiteUrl = "http://www.sz-otc.com/a/zhaobiao/zhongbiao/index.html";
        }
        
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "mt20 fenye2"))), new TagNameFilter("li")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                try
                {
                    for (int i = 0; i < tdNodes.Count; i++)
                    {
                        ATag aTag = tdNodes.SearchFor(typeof(ATag), true)[i] as ATag;
                        if (aTag.LinkText.Contains("末页"))
                        {
                            pageInt = Convert.ToInt32(aTag.Link.Replace("list_36_", "").Replace(".html", ""));
                            break;
                        }
                    }
                }
                catch (Exception ex) { }
            }

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.sz-otc.com/a/zhaobiao/zhongbiao/list_36_" + i.ToString() + ".html"), Encoding.Default);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter( new TagNameFilter("ul"), new HasAttributeFilter("class", "zhaobiao_list"))),new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                         
                        try
                        {
                            prjName = nodeList[j].ToPlainTextString().Trim();
                            prjName = prjName.Remove(prjName.IndexOf("&"));
                            if (prjName.Contains("]"))
                            {
                                int index = prjName.IndexOf("]");
                                prjName = prjName.Substring(index, prjName.Length - index).Replace("]","");
                            }
                            bidDate = nodeList[j].ToPlainTextString().Trim();
                            int indexS = bidDate.IndexOf("&");
                            bidDate = bidDate.Substring(indexS, bidDate.Length - indexS);
                            Regex regDate = new Regex(@"\d{4}-\d{2}-\d{2}");
                            beginDate = regDate.Match(bidDate).Value;
                        }
                        catch { }
                        ATag aTag = nodeList.SearchFor(typeof(ATag), true)[j] as ATag; 
                        InfoUrl = "http://www.sz-otc.com" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "right_content"), new TagNameFilter("div")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            htmldetail = regexHtml.Replace(htmldetail, "");
                        }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "right_content"), new TagNameFilter("div")));
                        bidCtx = dtnode.AsString();
                        Regex regBidUnit = new Regex(@"单位(：|:)[^\r\n]+\r\n");
                        bidUnit = regBidUnit.Match(bidCtx).Value.Replace("单位", "").Replace("：", "").Replace(":", "").Trim();
                        try
                        {
                            Regex regCode = new Regex(@"编号(：|:)[^\r\n]+\r\n");
                            code = regCode.Match(bidCtx).Value.Replace("编号", "").Replace("：", "").Replace(":", "").Trim();
                            if (code.Contains("点击"))
                            {
                                code = code.Remove(code.IndexOf("点击"));
                            } 
                        }
                        catch { }
                        if (bidUnit == "" || bidUnit == null)
                        {
                            bidUnit = "";
                        }
                        if (Encoding.Default.GetByteCount(bidUnit) > 150)
                        {
                            bidUnit = bidUnit.Substring(0, 150);
                        }
                        Regex regBidMoneystr = new Regex(@"金额(：|:)[^\r\n]+\r\n"); 
                        string monerystr = regBidMoneystr.Match(bidCtx).Value.Replace("金额", "").Replace("：", "").Replace(":", "").Replace(",", "").Replace("，", "").Trim();
                        Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");

                        if (!string.IsNullOrEmpty(regBidMoney.Match(monerystr).Value))
                        {
                            if (monerystr.Contains("万元") || monerystr.Contains("万美元"))
                            {
                                bidMoney = regBidMoney.Match(monerystr).Value;
                            }
                            else
                            {
                                try
                                {
                                    bidMoney = (decimal.Parse(regBidMoney.Match(monerystr).Value) / 10000).ToString();
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
                        }
                        else
                        {
                            bidMoney = "0";
                        }
                        if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                        {
                            Regex regBidMoneystr1 = new Regex(@"￥[^\r\n]+\r\n");
                            monerystr = regBidMoneystr1.Match(bidCtx).Value.Replace("￥", "").Replace("：", "").Replace(":", "").Replace(",", "").Replace("，", "").Trim();
                            Regex regBidMoney1 = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                            if (!string.IsNullOrEmpty(regBidMoney1.Match(monerystr).Value))
                            {
                                if (monerystr.Contains("万元") || monerystr.Contains("万美元"))
                                {
                                    bidMoney = regBidMoney1.Match(monerystr).Value;
                                }
                                else
                                {
                                    try
                                    {
                                        bidMoney = (decimal.Parse(regBidMoney1.Match(monerystr).Value) / 10000).ToString();
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
                            }
                            else
                            {
                                bidMoney = "0";
                            }
                        }
                        specType = "其他";
                        msgType = "深圳市东方招标有限公司";
                        prjName = ToolDb.GetPrjName(prjName);
                        bidType = ToolHtml.GetInviteTypes(prjName);
                        BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        list.Add(info);
                        NodeList FileTag = dtnode.SearchFor(typeof(ATag), true);
                        if (FileTag != null && FileTag.Count > 0)
                        {
                            for (int f = 0; f < FileTag.Count; f++)
                            {
                                ATag file = FileTag[f] as ATag;
                                if (file.Link.ToUpper().Contains(".DOC"))
                                {
                                    BaseAttach attach = ToolDb.GenBaseAttach(file.Link.Replace("Ads/", "").Replace(".DOC", "").Replace(".doc", ""), info.Id, "http://www.sz-otc.com/" + file.Link);
                                    base.AttachList.Add(attach);
                                }
                            }
                        }

                        if (!crawlAll && list.Count >= this.MaxCount) return list;

                    }
                }
            }

            return list;
        }
    }
}
