using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Windows.Forms;

namespace Crawler.Instance
{
    /// <summary>
    /// 广东广州从化
    /// </summary>
    public class InviteGzChonghua : WebSiteCrawller
    {
        public InviteGzChonghua()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省从化市";
            this.Description = "自动抓取广东省从化市招标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.SiteUrl = "http://www.conghua.gov.cn/zgch/zbzb/list.shtml";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new TagNameFilter("div")), new HasAttributeFilter("id", "page_div")));
            if (sNode != null && sNode.Count > 0)
            {
                string page = ToolHtml.GetRegexString(sNode.AsString(), "共", "页");
                try
                {
                    pageInt = int.Parse(page);
                }
                catch
                {
                    pageInt =7;
                }
            }
            parser.Reset();
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.conghua.gov.cn/zgch/zbzb/list_" + i.ToString() + ".shtml", Encoding.Default);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_list"))), new TagNameFilter("table")));
                if (sNode != null && sNode.Count > 0)
                {
                    TableTag table = sNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string projectName = ToolHtml.GetHtmlAtagValue("title", tr.ToHtml());
                        if (!projectName.Contains("中标")&&!projectName.Contains("结果")&&!projectName.Contains("候选单位公示"))
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                            prjName = projectName;
                            inviteType = ToolHtml.GetInviteTypes(projectName);
                            beginDate = ToolHtml.GetRegexDateTime(tr.Columns[1].ToPlainTextString());
                            InfoUrl = "http://www.conghua.gov.cn" + ToolHtml.GetHtmlAtagValue("href", tr.ToHtml()).Replace("..", "");
                            string htmlDtl = string.Empty;
                            try
                            {
                                htmlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                                htmlDtl = ToolHtml.GetRegexHtlTxt(htmlDtl);
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zoomcon")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                inviteCtx = dtlList.AsString().Replace("&nbsp;", "");
                                
                                buildUnit = ToolHtml.GetRegexString(inviteCtx, ToolHtml.BuildRegex, true);
                                if(!string.IsNullOrEmpty(buildUnit) && buildUnit.Contains(" "))
                                {
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf(" "));
                                }
                                
                                buildUnit = ToolHtml.GetSubString(buildUnit,150);
                                msgType = "广州建设工程交易中心";
                                specType = "建设工程";
                                inviteType = inviteType == "" ? "小型工程" : inviteType;
                                if (string.IsNullOrEmpty(buildUnit))
                                {
                                    buildUnit = "广州建设工程交易中心";
                                }
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "广州市区", "从化市", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                        else
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
                            prjName = projectName;
                            bidType = ToolHtml.GetInviteTypes(projectName);
                            beginDate = ToolHtml.GetRegexDateTime(tr.Columns[1].ToPlainTextString());
                            InfoUrl = "http://www.conghua.gov.cn" + ToolHtml.GetHtmlAtagValue("href", tr.ToHtml()).Replace("..", "");
                            string htmlDtl = string.Empty;
                            try
                            {
                                htmlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                                htmlDtl = ToolHtml.GetRegexHtlTxt(htmlDtl);
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmlDtl));
                            NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "zoomcon")));
                            if (dtlList != null && dtlList.Count > 0)
                            {
                                HtmlTxt = dtlList.ToHtml();
                                bidCtx = dtlList.AsString();
                                buildUnit = ToolHtml.GetRegexString(bidCtx, ToolHtml.BuildRegex, true);
                                buildUnit = ToolHtml.GetSubString(buildUnit, 150);
                                msgType = "广州建设工程交易中心";
                                specType = "建设工程";
                                bidType = bidType == "" ? bidType : "小型工程";

                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList bidNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (bidNode != null && bidNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag bidTable = bidNode[0] as TableTag;
                                    try
                                    {
                                        for (int r = 0; r < bidTable.RowCount; r++)
                                        {
                                            ctx += bidTable.Rows[r].Columns[0].ToNodePlainString() + "：";
                                            ctx += bidTable.Rows[r].Columns[1].ToNodePlainString() + "\r\n";
                                        }
                                    }
                                    catch { }

                                    bidUnit = ctx.GetRegex("单位名称,承包意向人名称");
                                    bidMoney = ctx.GetMoneyRegex();
                                    prjMgr = ctx.GetMgrRegex();
                                    if (prjMgr.Contains("/"))
                                        prjMgr = prjMgr.Remove(prjMgr.IndexOf("/"));
                                }

                                if (string.IsNullOrEmpty(buildUnit))
                                {
                                    buildUnit = "广州建设工程交易中心";
                                }
                                BidInfo info = ToolDb.GenBidInfo("广东省", "广州市区", "从化市", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                   bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
