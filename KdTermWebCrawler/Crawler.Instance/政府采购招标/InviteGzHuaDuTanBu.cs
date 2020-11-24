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
using System.Threading;

namespace Crawler.Instance
{
    public class InviteGzHuaDuTanBu : WebSiteCrawller
    {
        public InviteGzHuaDuTanBu()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "广东省广州市花都区炭步镇人民政府招标、中标公告";
            this.Description = "自动抓取广东省广州市花都区炭步镇人民政府招标、中标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://www.tanbu.gov.cn/govAffairsList.php?vid=M185XzJfMTk4Xw==";
            this.MaxCount = 120;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 3;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch
            {
                return list;
            } 
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("valign", "top"))), new TagNameFilter("table")));
                if (viewList != null && viewList.Count > 0)
                {
                    TableTag table = null;
                    for (int l = 0; l < viewList.Count; l++)
                    {
                        TableTag tagTable = viewList[l] as TableTag;
                        if (tagTable.RowCount >= 1)
                        {
                            TableColumn cell = tagTable.Rows[0].Columns[0];
                            if (cell.GetAttribute("width") == "30" && cell.GetAttribute("height") == "25")
                                table = tagTable;
                        }
                    }
                    if (table == null)
                        continue;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, InfoUrl = string.Empty, beginDate = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.GetATag();
                        if (aTag == null) continue;
                        prjName = aTag.GetAttribute("title");
                        beginDate = table.ToNodePlainString().GetDateRegex();
                        InfoUrl = "http://www.tanbu.gov.cn/" + aTag.Link;
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("width", "706")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml();

                            if (prjName.Contains("中标") || prjName.Contains("成交") || prjName.Contains("结果") || prjName.Contains("候选人公示"))
                            {
                                string buildUnit = string.Empty, bidUnit = string.Empty,
                          bidMoney = string.Empty, code = string.Empty,
                          bidDate = string.Empty,endDate = string.Empty,
                          bidType = string.Empty, specType = string.Empty,
                          msgType = string.Empty, bidCtx = string.Empty,
                          prjAddress = string.Empty, remark = string.Empty,
                          prjMgr = string.Empty, otherType = string.Empty;
                                bidCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                               
                                code = bidCtx.GetCodeRegex().GetCodeDel();
                                buildUnit = bidCtx.GetBuildRegex();
                                if (buildUnit.Contains("招标代理"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";

                                bidUnit = bidCtx.GetBidRegex();
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = bidCtx.GetRegex("中标候选公司,中标候选人");
                                bidMoney = bidCtx.GetMoneyRegex();
                                if (string.IsNullOrEmpty(bidUnit))
                                {
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList bidNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellspacing", "1")));
                                    if (bidNode != null && bidNode.Count > 0)
                                    {
                                        string ctx = string.Empty;
                                        TableTag bidTable = bidNode[0] as TableTag;
                                        if (bidTable.RowCount >= 2 && bidTable.Rows[0].ColumnCount >= 2)
                                        {
                                            ctx = bidTable.Rows[0].Columns[1].ToNodePlainString() + "：";
                                            ctx += bidTable.Rows[1].Columns[1].ToNodePlainString() + "\r\n";
                                        }
                                        bidUnit = ctx.GetBidRegex(new string[] { "候选人" });
                                    }
                                }
                                try
                                {
                                    if (decimal.Parse(bidMoney) > 100000)
                                        bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                                }
                                catch { }
                               
                                msgType = "广州市花都区炭步镇人民政府";
                                specType = "政府采购";
                                bidType = prjName.GetInviteBidType();
                                BidInfo info = ToolDb.GenBidInfo("广东省", "广州政府采购", "花都区", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                      bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag a = aNode[k].GetATag();
                                        if (a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = a.Link;
                                            else
                                                link = "http://www.tanbu.gov.cn/" + a.Link;
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                            else
                            {
                                string code = string.Empty, buildUnit = string.Empty,
                       prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                       specType = string.Empty, endDate = string.Empty,
                       remark = string.Empty, inviteCon = string.Empty,
                       CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty;

                                inviteCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                              
                                inviteType = prjName.GetInviteBidType();

                                code = inviteCtx.GetCodeRegex().GetCodeDel();
                                buildUnit = inviteCtx.GetBuildRegex();
                                prjAddress = inviteCtx.GetAddressRegex();
                                if (buildUnit.Contains("招标代理"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";

                             
                                msgType = "广州市花都区炭步镇人民政府";

                                specType = "政府采购";

                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "广州政府采购", "花都区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                               
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag a = aNode[k].GetATag();
                                        if (a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = a.Link;
                                            else
                                                link = "http://www.tanbu.gov.cn/" + a.Link;
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

            }
            return list;
        }
    }
}
