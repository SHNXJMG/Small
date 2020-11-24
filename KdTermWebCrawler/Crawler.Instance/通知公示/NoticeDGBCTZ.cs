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
    public class NoticeDGBCTZ : WebSiteCrawller
    {
        public NoticeDGBCTZ()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省东莞市建设工程交易中心补充通知";
            this.PlanTime = "9:18,11:18,14:18,17:28";
            this.Description = "自动抓取广东省东莞市建设工程交易中心补充通知";
            this.ExistCompareFields = "Prov,City,InfoSource,InfoTitle,InfoType,PublishTime,PrjCode";
            this.SiteUrl = "http://www.dgzb.com.cn/DGJYWEB/SiteManage/GcBuchongList.aspx?ModeId=2";
            this.MaxCount = 2000;
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
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "ctl00_cph_context_GridViewPaingTwo1_PagingDescTd")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString().GetRegexBegEnd("共", "页");
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
                            "__VIEWSTATE",
                            "__EVENTVALIDATION",
                            "ctl00$cph_context$drp_selSeach",
                            "ctl00$cph_context$txt_strWhere",
                            "ctl00$cph_context$drp_Rq",
                            "ctl00$cph_context$GridViewPaingTwo1$txtGridViewPagingForwardTo",
                            "ctl00$cph_context$GridViewPaingTwo1$btnForwardToPage" 
                            },
                            new string[]{
                            viewState,
                            eventValidation,
                            "1","","3",i.ToString(),"GO"
                            });
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_GridView1")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty,htmlTxt =string.Empty;

                        TableRow tr = table.Rows[j];
                        InfoType = "补充通知";
                        InfoTitle = tr.Columns[2].ToNodePlainString();
                        prjCode = tr.Columns[1].ToNodePlainString();
                        PublistTime = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        InfoUrl = SiteUrl;
                        InfoCtx = "招标编号：" + prjCode + "\r\n工程名称：" + InfoTitle + "\r\n附件类型：" + tr.Columns[3].ToNodePlainString() + "\r\n上传时间：" + PublistTime;
                        htmlTxt = InfoCtx;
                        NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "东莞市区", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.DongGuanMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty, htmlTxt);
                        list.Add(info);
                        ATag fileUrl = tr.Columns[2].GetATag();
                        if (fileUrl != null)
                        {
                            string alink = "http://www.dgzb.com.cn/DGJYWEB/SiteManage/" + fileUrl.Link;
                            BaseAttach attach = ToolDb.GenBaseAttach(fileUrl.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, alink);
                            base.AttachList.Add(attach);
                        }
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
