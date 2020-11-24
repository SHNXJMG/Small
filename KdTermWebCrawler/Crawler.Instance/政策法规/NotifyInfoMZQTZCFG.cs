﻿using System.Text;
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
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class NotifyInfoMZQTZCFG : WebSiteCrawller
    {
        public NotifyInfoMZQTZCFG()
            : base()
        {
            this.Group = "政策法规";
            this.Title = "广东省梅州市建设工程交易中心（其它）政策法规";
            this.Description = "自动抓取广东省梅州市建设工程交易中心（其它）政策法规";
            this.PlanTime = "1 22:28";
            this.SiteUrl = "http://market.meizhou.gov.cn/deptWebsiteAction.do?action=secondIndex&deptId=1925&issueTypeCode=009001&issueTypeName=政策法规&showSubNodeflag=1";
            this.MaxCount = 500;
            this.ExistCompareFields = "InfoUrl";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            //取得页码
            int pageInt = 1, sqlCount = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default).GetJsString();
            }
            catch (Exception ex)
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"),new HasAttributeFilter("width","95%")));
            if (nodeList != null && nodeList.Count > 0)
            {
                List<INode> list = new List<INode>();
                list.Add(nodeList[10]);
                list.Add(nodeList[4]);
                list.Add(nodeList[2]);
                foreach (INode t in list)
                {
                    TableTag table = t as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    { 
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty,
                              infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        infoType = "政策法规";
                        headName = tr.Columns[0].ToNodePlainString();
                        releaseTime = tr.Columns[1].ToPlainTextString().GetDateRegex();
                        infoUrl = "http://market.meizhou.gov.cn" + tr.Columns[0].GetATagValue("onclick").GetRegexBegEnd(",'", "',");
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.Default).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("align", "center")));

                        if (dtlList != null && dtlList.Count > 0)
                        {
                            if (dtlList.Count > 1) ctxHtml = dtlList[1].ToHtml();
                            else ctxHtml = dtlList.ToHtml();
                            infoCtx = ctxHtml.ToCtxString().Replace("&gt;", "");
                            msgType = MsgTypeCosnt.MeiZhouMsgType;
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "梅州市区", string.Empty, infoCtx, infoType);
                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return null;
                            }
                            else
                            {
                                sqlCount++;
                                if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                                {
                                    parser = new Parser(new Lexer(htldtl));
                                    NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                    if (aNode != null && aNode.Count > 0)
                                    {
                                        for (int a = 0; a < aNode.Count; a++)
                                        {
                                            ATag aTag = aNode[a] as ATag;
                                            if (aTag.IsAtagAttach())
                                            {
                                                try
                                                {
                                                    BaseAttach baseInfo = ToolHtml.GetBaseAttach("http://market.meizhou.gov.cn" + aTag.Link, aTag.LinkText, info.Id);
                                                    if (baseInfo != null)
                                                    {
                                                        ToolDb.SaveEntity(baseInfo, string.Empty);
                                                    }
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
