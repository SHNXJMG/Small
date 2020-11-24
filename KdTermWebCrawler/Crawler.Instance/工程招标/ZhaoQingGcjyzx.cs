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
    public class ZhaoQingGcjyzx : WebSiteCrawller
    {
        public ZhaoQingGcjyzx()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "肇庆工程交易中心";
            this.Description = "自动抓取肇庆工程交易中心招标信息、中标信息";
            this.PlanTime = "9:16,13:46";
            this.SiteUrl = "http://www.zqgcjy.com/zbgg.asp?txt01=";
            
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {

            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string cookiestr = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table1")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("共", "页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i,Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "position6")), true), new TagNameFilter("li")));
                if (pageNode != null && pageNode.Count > 0)
                {
                    for (int j = 3; j < pageNode.Count; j++)
                    {
                        INode node = pageNode[j];
                        ATag aTag = node.GetATag();
                        string psName = aTag.LinkText;
                        if (psName.Contains("中标") || psName.Contains("结果"))
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
                            prjName = aTag.GetAttribute("title"); 
                            InfoUrl = "http://www.zqgcjy.com/" + aTag.Link;
                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                            }
                            catch (Exception)
                            {

                                continue;
                            }
                            Parser parserdetail = new Parser(new Lexer(htmldetail));
                            NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table1")));
                            if (dtnode != null && dtnode.Count > 0)
                            {
                                HtmlTxt = dtnode.AsHtml();
                                bidCtx = HtmlTxt.ToCtxString();
                                beginDate = bidCtx.GetDateRegex();
                                code = bidCtx.GetCodeRegex();
                                bidMoney = bidCtx.GetMoneyRegex();
                                if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                    bidMoney = bidCtx.GetMoneyRegex(null, true);
                                if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                    bidMoney = bidCtx.GetRegex("总额").GetMoney();
                                prjMgr = bidCtx.GetMgrRegex();
                                bidUnit = bidCtx.GetBidRegex();
                                bidDate = bidCtx.GetTimeRegex();
                                buildUnit = bidCtx.GetBuildRegex();
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    for (int t = 0; t < tableNode.Count; t++)
                                    {
                                        TableTag tag = tableNode[t] as TableTag;
                                        string classStr = tag.GetAttribute("class");
                                        if (!string.IsNullOrEmpty(classStr) && classStr.ToLower().Contains("table1")) continue;

                                        string ctx = string.Empty;
                                        for (int r = 0; r < tag.RowCount; r++)
                                        {
                                            for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                            {
                                                string temp = tag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                                if ((c + 1) % 2 == 0)
                                                {
                                                    ctx += temp + "\r\n";
                                                }
                                                else
                                                    ctx += temp + "：";
                                            }
                                        }


                                        if (string.IsNullOrEmpty(bidUnit))
                                            bidUnit = ctx.GetRegex("成交候选人,中标单位名称,第一中标候选人,第一候选人");
                                        if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                            bidMoney = ctx.GetMoneyRegex();
                                        if (string.IsNullOrEmpty(prjMgr))
                                            prjMgr = ctx.GetMgrRegex();
                                        if (string.IsNullOrEmpty(prjMgr))
                                            prjMgr = ctx.GetRegex("拟任总监,拟任项目经理");

                                        if (!bidUnit.Contains("公司"))
                                        {
                                            ctx = string.Empty;
                                            try
                                            {
                                                for (int r = 1; r < tag.Rows[4].ColumnCount; r++)
                                                {
                                                    string temp = tag.Rows[4].Columns[r].ToNodePlainString().GetReplace(":,：");
                                                    ctx += temp + "：";
                                                    ctx += tag.Rows[5].Columns[r].ToNodePlainString().GetReplace(":,：") + "\r\n";
                                                }
                                                if (string.IsNullOrEmpty(bidUnit))
                                                    bidUnit = ctx.GetRegex("成交候选人,中标单位名称,第一中标候选人,第一成交候选人");
                                                if (bidMoney == "0" || string.IsNullOrEmpty(bidMoney))
                                                    bidMoney = ctx.GetMoneyRegex();
                                                if (string.IsNullOrEmpty(prjMgr))
                                                    prjMgr = ctx.GetMgrRegex();
                                                if (string.IsNullOrEmpty(prjMgr))
                                                    prjMgr = ctx.GetRegex("拟任总监,拟任项目经理");
                                            }
                                            catch { }
                                        }
                                    }

                                }
                                msgType = "肇庆工程交易中心";
                                specType = bidType = "建设工程";
                                BidInfo info = ToolDb.GenBidInfo("广东省", "肇庆市区", "", string.Empty, code, prjName, buildUnit, beginDate,
                                           bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                           bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);

                                //ToolDb.SaveEntity(info, "");
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                        else
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                       prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                       specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                       remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                       CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                             
                            prjName = aTag.GetAttribute("title");  
                            InfoUrl = "http://www.zqgcjy.com/" + aTag.Link;
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                            }
                            catch (Exception)
                            {

                                continue;
                            }
                            Parser parserdetail = new Parser(new Lexer(htmldtl));
                            NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "table1")));
                            if (dtnode != null && dtnode.Count > 0)
                            {
                                HtmlTxt = dtnode.AsHtml();
                                inviteCtx = HtmlTxt.ToCtxString();
                                buildUnit = inviteCtx.GetBidUnitDel().GetBuildRegex();
                                beginDate = inviteCtx.GetDateRegex();
                                prjAddress = ToolHtml.GetRegexString(inviteCtx, ToolHtml.AddressRegex); //inviteCtx.GetAddressRegex();
                                code = inviteCtx.GetReplace(" ").GetCodeRegex().GetCodeDel();
                                prjAddress = ToolHtml.GetSubString(prjAddress, 150);
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {
                                    string ctx = string.Empty;
                                    TableTag tag = tableNode[0] as TableTag;
                                    for (int r = 0; r < tag.RowCount; r++)
                                    {
                                        for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                        {
                                            string temp = tag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                            if ((c + 1) % 2 == 0)
                                                ctx += temp + "\r\n";
                                            else
                                                ctx += temp + "：";
                                        }
                                    }
                                    if (string.IsNullOrEmpty(code))
                                        code = ctx.GetCodeRegex();
                                    if (string.IsNullOrEmpty(buildUnit))
                                        buildUnit = ctx.GetBuildRegex();
                                    if (string.IsNullOrEmpty(prjAddress))
                                        prjAddress = ctx.GetAddressRegex();
                                    if (string.IsNullOrEmpty(prjAddress))
                                    {
                                        prjAddress = "见招标信息";
                                    }
                                }
                                msgType = "肇庆工程交易中心";
                                specType = "建设工程";
                                inviteType = ToolHtml.GetInviteTypes(prjName);
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "肇庆市区", "",
                                 string.Empty, code, prjName, prjAddress, buildUnit,
                                 beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                    }
                }
            }
            return null;
        }

    }
}
