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
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class NoticeSZJYZXBG : WebSiteCrawller
    {
        public NoticeSZJYZXBG()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市交易中心变更公示";
            this.PlanTime = "9:27,11:27,14:17,17:27";
            this.Description = "自动抓取广东省深圳市交易中心变更公示";
            this.ExistCompareFields = "Prov,City,InfoSource,InfoTitle,InfoType,PublishTime,PrjCode";
            this.SiteUrl = "http://www.szjsjy.com.cn/BusinessInfo/BGXXIndex.aspx";
            this.MaxCount = 1000;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookieStr = string.Empty;
            int pageInt = 1;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("colspan", "7")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString().GetRegexBegEnd("，共", "页，");
                    pageInt = int.Parse(temp);
                }
                catch
                { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    try
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                            new string[]{
                            "__EVENTTARGET",
                            "__EVENTARGUMENT",
                            "__VIEWSTATE",
                            "__EVENTVALIDATION",
                            "ctl00$Header$drpSearchType",
                            "ctl00$Header$beginDate",
                            "ctl00$Header$endDate",
                            "ctl00$Header$txtGcxm",
                            "ctl00$hdnPageCount"
                            },
                            new string[]{
                            "ctl00$Content$GridView1",
                            "Page$"+i.ToString(),
                            viewState,
                            eventValidation,
                            "0",
                            "","","",
                            pageInt.ToString()
                            }
                            );
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_Content_GridView1")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty,htmlTxt=string.Empty;

                        TableRow tr = table.Rows[j];
                        InfoType = "变更公示";
                        prjCode = tr.Columns[1].ToNodePlainString();
                        InfoTitle = tr.Columns[3].ToNodePlainString();
                        PublistTime = tr.Columns[6].ToPlainTextString();
                        InfoUrl = this.SiteUrl; 
                        InfoCtx += "工程编号：" + prjCode + "\r\n工程名称：" + tr.Columns[2].ToNodePlainString() + "\r\n变更标题：" + InfoTitle + "\r\n变更类型：" + tr.Columns[4].ToNodePlainString() + "\r\n变更内容：" + tr.Columns[5].ToNodePlainString() + "\r\n发布时间：" + PublistTime;
                        htmlTxt = InfoCtx; 
                        NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳市工程", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.ShenZhenMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty, htmlTxt);

                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
