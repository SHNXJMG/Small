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
using System.Web.UI.MobileControls;
using System.Collections.Generic;
namespace Crawler.Instance
{
    public class BidSongGang : WebSiteCrawller
    {
        public BidSongGang()
            : base()
        {
            this.Group = "街道办中标信息";
            this.Title = "广东省深圳市松岗街道办事处";
            this.Description = "自动抓取广东省深圳市松岗街道办事处中标信息";
            this.PlanTime = "9:23,13:55";
            this.SiteUrl = "http://sgjd.baoan.gov.cn/zbcg/zhbgg_139208/index.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
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
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "venycms-page")), true), new TagNameFilter("script")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string page = sNode.ToString().Replace("createPageHTML(", "").Replace(",", "kd").Replace("****", "").Replace("\n", "");
                    page = page.GetRegexBegEnd("Code", "kd");
                    pageInt = int.Parse(page);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://sgjd.baoan.gov.cn/zbcg/zhbgg_139208/index_" + (i - 1) + ".html", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "content clearfix")), true), new TagNameFilter("li")));
                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
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
                        ATag aTag = viewList[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = viewList[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link;
                        InfoUrl = InfoUrl.GetRegexBegEnd("./", ".html");
                        InfoUrl = "http://sgjd.baoan.gov.cn/zbcg/zhbgg_139208/" + InfoUrl + ".html";
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; } 
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "con")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            HtmlTxt = dtlList.AsHtml();
                            bidCtx = HtmlTxt.ToCtxString();

                            bidType = prjName.GetInviteBidType();
                            Regex regPrjCode = new Regex(@"(工程编号|项目编号|招标编号|中标编号|编号)(:|：)[^\r\n]+\r\n");
                            code = regPrjCode.Match(bidCtx.Replace(" ", "")).Value.Replace("工程编号", "").Replace("项目编号", "").Replace("招标编号", "").Replace("中标编号", "").Replace("编号", "").Replace(":", "").Replace("：", "").Trim();

                            Regex regBuidUnit = new Regex(@"(建设单位|招标人|承包人|招标单位|招标方|招标代理机构)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(bidCtx.Replace(" ", "")).Value.Replace("招标代理机构", "").Replace("建设单位", "").Replace("招标人", "").Replace("承包人", "").Replace("招标单位", "").Replace("招标方", "").Replace("：", "").Replace(":", "").Trim();

                            Regex regMoney = new Regex(@"(中标价|投标价|总投资|发包价|投标报价|价格|金额|总价)(：|:|)[^\r\n]+\r\n");
                            bidMoney = regMoney.Match(bidCtx.Replace(" ", "")).Value.Replace("中标价", "").Replace("总投资", "").Replace("发包价", "").Replace("总价", "").Replace("投标报价", "").Replace("投标价", "").Replace("价格", "").Replace("金额", "").Replace("：", "").Replace(":", "").Replace("，", "").Replace(",", "").Trim();

                            Regex regBidUnit = new Regex(@"(成交供应商|中标供应商|第一候选人|中标候选人|中标单位|中标人|中标方)(：|:)[^\r\n]+\r\n");
                            bidUnit = regBidUnit.Match(bidCtx.Replace(" ", "")).Value.Replace("成交供应商", "").Replace("中标供应商", "").Replace("中标候选人", "").Replace("第一候选人", "").Replace("中标单位", "").Replace("中标人", "").Replace("中标方", "").Replace("：", "").Replace(":", "").Trim();

                            Regex regprjMgr = new Regex(@"(项目经理姓名|项目经理（或建造师）|项目经理|项目负责人|项目总监|建造师|总工程师|监理师)(：|:)[^\r\n]+\r\n");
                            prjMgr = regprjMgr.Match(bidCtx.Replace(" ", "")).Value.Replace("项目经理（或建造师）", "").Replace("项目经理姓名", "").Replace("总工程师", "").Replace("项目经理", "").Replace("项目总监", "").Replace("建造师", "").Replace("监理师", "").Replace("项目负责人", "").Replace("：", "").Replace(":", "").Trim();

                            Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                           
                            if (prjMgr.Contains("资格"))
                            {
                                prjMgr = prjMgr.Remove(prjMgr.IndexOf("资格"));
                            }
                            bidUnit = ToolHtml.GetStringTemp(bidUnit);
                            buildUnit = ToolHtml.GetSubString(buildUnit, 150);
                            bidUnit = ToolHtml.GetSubString(bidUnit, 150);
                            code = ToolHtml.GetSubString(code, 50);
                            prjMgr = ToolHtml.GetSubString(prjMgr, 50);
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "MsoNormalTable")));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    for (int t = 0; t < tableNode.Count; t++)
                                    {
                                        TableTag table = tableNode[t] as TableTag;

                                        string ctx = string.Empty;
                                        for (int r = 0; r < table.Rows[0].ColumnCount; r++)
                                        {
                                            try
                                            {
                                                ctx += table.Rows[0].Columns[r].ToNodePlainString() + "：";
                                                ctx += table.Rows[1].Columns[r].ToNodePlainString() + "\r\n";
                                            }
                                            catch { }
                                        }
                                        bidUnit = ctx.GetBidRegex();
                                        if (string.IsNullOrEmpty(bidUnit))
                                            bidUnit = ctx.GetRegex("中标供应商");
                                     
                                        if (string.IsNullOrWhiteSpace(code))
                                            code = ctx.GetCodeRegex();
                                        // break;
                                       
                                             bidMoney = ctx.GetMoneyRegex();

                                    }

                                }
                            }


                            try
                            {
                                if (Convert.ToDecimal(bidMoney) > 100000)
                                    bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                            }
                            catch { }
                            if (bidMoney.Contains("万"))
                            {
                                bidMoney = bidMoney.Remove(bidMoney.IndexOf("万")).Trim();
                                bidMoney = regBidMoney.Match(bidMoney).Value;
                            }
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "深圳市宝安区松岗街道办事处";
                            }
                            msgType = "深圳市宝安区松岗街道办事处";
                            specType = "建设工程";
                            bidType = "小型工程"; 
                            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳区及街道工程", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                   bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
