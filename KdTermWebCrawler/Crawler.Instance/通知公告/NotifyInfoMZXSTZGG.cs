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
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class NotifyInfoMZXSTZGG : WebSiteCrawller
    {
        public NotifyInfoMZXSTZGG()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省梅州市建设工程交易中心（县市）通知公告";
            this.Description = "自动抓取广东省梅州市建设工程交易中心（县市）通知公告";
            this.PlanTime = "22:10";
            this.SiteUrl = "http://market.meizhou.gov.cn/deptWebsiteAction.do?action=secondIndex&deptId=1925&issueTypeCode=009006002&issueTypeName=各县(市)通知公告&showSubNodeflag=1";
            this.MaxCount = 500;
            this.ExistCompareFields = "HeadName,InfoUrl";
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
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "28")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString().GetRegexBegEnd("，共", "页");
                    pageInt = Convert.ToInt32(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://market.meizhou.gov.cn/deptWebsiteAction.do?action=secondIndex&deptId=1925&showSubNodeflag=1&issueTypeCode=009006002&issueTypeName=各县(市)通知公告&pageNum=" + i.ToString(), Encoding.Default).GetJsString();
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "95%")));
                if (nodeList != null && nodeList.Count > 1)
                {
                    TableTag table = nodeList[1] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        infoType = "通知公告";
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
                        if (dtlList != null && dtlList.Count > 1)
                        {
                            ctxHtml = dtlList[1].ToHtml();
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
