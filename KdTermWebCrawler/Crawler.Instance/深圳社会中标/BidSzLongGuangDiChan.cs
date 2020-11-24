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
    public class BidSzLongGuangDiChan : WebSiteCrawller
    {
        public BidSzLongGuangDiChan()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "广东省龙光地产采购";
            this.Description = "自动抓取广东省龙光地产采购";
            this.PlanTime = "21:08,11:12,14:02,16:33";
            this.SiteUrl = "http://pur.logan.com.cn/InviteBidGrid_Grid.aspx";
            this.MaxCount = 200;
            this.ExistCompareFields = "ProjectName,BidUnit,BeginDate,MsgType";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "AspNetPager1")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("总计", "条记录");
                    pageInt = int.Parse(temp) / 10 + 1;
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    i++;
                    try
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                            new string[]{
                            "__VIEWSTATE",
                            "__EVENTTARGET",
                            "__EVENTARGUMENT",
                            "__EVENTVALIDATION",
                            "hidInviteBidNoticeTitle",
                            "hidBuGUID",
                            "hidPublishTimeFrom",
                            "hidPublishTimeTo",
                            "hidProviderNameList",
                            "AspNetPager1_input"
                            }, new string[]{
                            viewState,
                            "AspNetPager1",
                            i.ToString(),
                            eventValidation,
                            "","","","","","1"
                            }
                            );
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "grid")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow row = table.Rows[j];
                        prjName = row.Columns[1].GetAttribute("title");
                        bidUnit = row.Columns[3].ToNodePlainString();
                        beginDate = row.Columns[2].ToPlainTextString().GetDateRegex();

                        bidCtx = HtmlTxt = "项目名称：" + prjName + "\r\n" + "中标单位：" + bidUnit + "\r\n发布时间：" + beginDate;
                        specType = "其他";
                        bidType = prjName.GetInviteBidType();
                        buildUnit = msgType = "龙光地产控股有限公司";
                        BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
