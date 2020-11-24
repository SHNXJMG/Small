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
    public class MeetInfoZh : WebSiteCrawller
    {
        public MeetInfoZh()
            : base()
        {
            this.Group = "会议信息";
            this.Title = "广东省珠海市公共资源交易中心今日议程(今日工作)";
            this.Description = "自动抓取广东省珠海市公共资源交易中心今日议程(今日工作)";
            this.ExistCompareFields = "Prov,Area,Road,MeetName,MeetPlace,ProjectName,BeginDate,PrjCode";
            this.MaxCount = 1000;
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,11:30,14:05,14:25,14:50,15:45,16:50,19:00";
            this.SiteUrl = "http://www.cpinfo.com.cn/index/showList/000000000004/000000000411";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<MeetInfo>();
            string html = string.Empty;
            int pageInt = 1;
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
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "cnewslist")));
            if (nodeList != null && nodeList.Count > 0)
            {
                TableTag table = nodeList[0] as TableTag;
                for (int i = 0; i < table.RowCount; i++)
                {
                    TableRow tr = table.Rows[i];
                    string temp = tr.Columns[1].ToNodePlainString();
                    if (!temp.Contains("开标时间")) continue;

                    string meetTime = string.Empty, prjName = string.Empty, meetName = string.Empty, place = string.Empty, builUnit = string.Empty;

                    prjName = tr.Columns[0].ToNodePlainString();
                    meetName = "开标会";
                    meetTime = tr.Columns[2].ToPlainTextString();
                    place = tr.Columns[3].ToNodePlainString();

                    MeetInfo info = ToolDb.GenMeetInfo("广东省", "珠海市区", string.Empty, string.Empty, prjName, place, meetName, meetTime,
                        string.Empty, "珠海市公共资源交易中心", SiteUrl, "", builUnit, string.Empty, string.Empty);
                    list.Add(info);

                    if (!crawlAll && list.Count >= this.MaxCount)
                    {
                        IList<MeetInfo> result = list as IList<MeetInfo>;
                        // 删除 
                        string bDate = result.OrderBy(x => x.BeginDate).ToList()[0].BeginDate.ToString().GetDateRegex("yyyy/MM/dd");
                        string eDate = Convert.ToDateTime(result.OrderByDescending(x => x.BeginDate).ToList()[0].BeginDate).AddDays(1).ToString().GetDateRegex("yyyy/MM/dd");
                        string sqlwhere = " where City='珠海市区' and InfoSource='珠海市公共资源交易中心' and InfoUrl='"+SiteUrl+"' and BeginDate>='" + bDate + "' and BeginDate<'" + eDate + "'";
                        string delMeetSql = "delete from MeetInfo " + sqlwhere;
                        int countMeet = ToolDb.ExecuteSql(delMeetSql);
                        return list;
                    }
                }
            }
            if (list != null && list.Count > 0)
            {
                IList<MeetInfo> result = list as IList<MeetInfo>;
                // 删除 
                string bDate = result.OrderBy(x => x.BeginDate).ToList()[0].BeginDate.ToString().GetDateRegex("yyyy/MM/dd"), eDate = Convert.ToDateTime(result.OrderByDescending(x => x.BeginDate).ToList()[0].BeginDate).AddDays(1).ToString().GetDateRegex("yyyy/MM/dd");
                string sqlwhere = " where City='珠海市区' and InfoSource='珠海市公共资源交易中心' and InfoUrl='" + SiteUrl + "' and BeginDate>='" + bDate + "' and BeginDate<'" + eDate + "'";
                string delMeetSql = "delete from MeetInfo " + sqlwhere;
                int countMeet = ToolDb.ExecuteSql(delMeetSql); 
            }
            return list;
        }
    }
}
