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
    public class NotifyInfoSZJYZXBSZN : WebSiteCrawller
    {
        public NotifyInfoSZJYZXBSZN()
            : base()
        {
            this.Group = "办事指南";
            this.Title = "广东省深圳市交易中心办事指南";
            this.Description = "自动抓取广东省深圳市交易中心办事指南";
            this.PlanTime = "21:03";
            this.SiteUrl = "http://www.szjsjy.com.cn/ServiceGuide/AffairsGuide.aspx";
            this.MaxCount = 500;
            this.ExistCompareFields = "HeadName,ReleaseTime,MsgType,InfoUrl";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("tr"), new HasAttributeFilter("valign", "top")), true), new TagNameFilter("table")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    TableTag tab = pageList[0] as TableTag;
                    pageInt = tab.Rows[0].ColumnCount;
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "__EVENTTARGET","__EVENTARGUMENT","__VIEWSTATE","__EVENTVALIDATION","sel","beginDate","endDate","infotitle"
                        }, new string[]{
                        "GridView1","Page$"+i.ToString(),viewState,eventValidation,"1","","",""
                        });
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                        TableRow tr = table.Rows[j];
                        infoType = "办事指南";
                        headName = tr.Columns[1].ToNodePlainString();
                        infoScorce = tr.Columns[2].ToNodePlainString();
                        releaseTime = tr.Columns[3].ToPlainTextString().GetDateRegex();

                        infoUrl = "http://www.szjsjy.com.cn/" + tr.Columns[1].GetATagHref().Replace("../", "");
                        ctxHtml = "<p>信息标题：" + headName + "<br/>信息来源：" + infoScorce + "<br/>发布时间：" + releaseTime + "</p>";
                        infoCtx = "信息标题：" + headName + "\r\n信息来源：" + infoScorce + "\r\n发布时间：" + releaseTime + "\r\n";
                        msgType = MsgTypeCosnt.ShenZhenMsgType;
                        NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳市工程", string.Empty, infoCtx, infoType);
                        if (!crawlAll && sqlCount >= this.MaxCount)
                        {
                            return null;
                        }
                        else
                        {
                            sqlCount++;
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                            {
                                try
                                {
                                    BaseAttach obj = ToolHtml.GetBaseAttach(infoUrl, headName, info.Id);
                                    if (obj != null)
                                        ToolDb.SaveEntity(obj, string.Empty);
                                }
                                catch { }
                            }
                        }  
                    }
                }
            }
            return null;
        }
    }
}
