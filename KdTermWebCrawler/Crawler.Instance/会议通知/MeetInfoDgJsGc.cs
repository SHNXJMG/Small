using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class MeetInfoDgJsGc : WebSiteCrawller
    {
        public MeetInfoDgJsGc()
            : base()
        {
            this.Group = "会议信息";
            this.Title = "广东省东莞市建设工程交易中心会议信息";
            this.Description = "自动抓取广东省东莞市建设工程交易中心会议信息";
            this.ExistCompareFields = "Prov,Area,Road,MeetName,MeetPlace,ProjectName,BeginDate";
            this.MaxCount = 1000;
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,11:30,14:05,14:25,14:50,15:45,16:50,19:00";
            this.SiteUrl = "http://www.dgzb.com.cn:8080/DGJYWEB/SiteManage/Meeting_List.aspx";

        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<MeetInfo>();
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            int pageInt = 1;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
                // 删除
                string bDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), eDate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
                string sqlwhere = " where City='东莞市区' and BeginDate<'" + bDate + "' and BeginDate>'" + eDate + "'";
                string delAttachSql = "delete from BaseAttach where  SourceID in(select Id from MeetInfo " + sqlwhere + ")";
                string delMeetSql = "delete from MeetInfo " + sqlwhere;
                int countlAttach = ToolDb.ExecuteSql(delAttachSql);
                int countMeet = ToolDb.ExecuteSql(delMeetSql);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_cph_context_GridViewPaingTwo1_lblGridViewPagingDesc")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList[0].ToPlainTextString().GetRegexBegEnd("共", "页");
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
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                            new string[]{
                            "__VIEWSTATE","__EVENTVALIDATION","ctl00$cph_context$drp_selSeach",
                            "ctl00$cph_context$txt_strWhere","ctl00$cph_context$drp_Rq","ctl00$cph_context$GridViewPaingTwo1$txtGridViewPagingForwardTo","ctl00$cph_context$GridViewPaingTwo1$btnForwardToPage"
                            },
                            new string[]{
                            viewState,eventValidation,"1","","1",i.ToString(),"Go"
                            }
                            );
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
                        string meetTime = string.Empty, prjName = string.Empty, meetName = string.Empty, place = string.Empty,prjcode=string.Empty;
                        TableRow tr = table.Rows[j];
                        prjcode = tr.Columns[1].ToNodePlainString();
                        meetName = tr.Columns[3].ToNodePlainString();
                        prjName = tr.Columns[2].ToNodePlainString();
                        meetTime = tr.Columns[5].ToNodePlainString().Insert(10," ");
                        place = tr.Columns[4].ToNodePlainString();

                        MeetInfo info = ToolDb.GenMeetInfo("广东省", "东莞市区", string.Empty, string.Empty, prjName, place, meetName, meetTime,
                                string.Empty, MsgTypeCosnt.DongGuanMsgType, SiteUrl, prjcode, string.Empty, string.Empty, string.Empty);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount)
                        {
                            //ToolDb.ExecuteAttenProject(MsgTypeCosnt.DongGuanMsgType);
                            return list; 
                        }
                    }
                }
            }
            //if (list != null && list.Count > 0)
            //{
            //    ToolDb.ExecuteAttenProject(MsgTypeCosnt.DongGuanMsgType);
            //}
            return list;
        }
    }
}
