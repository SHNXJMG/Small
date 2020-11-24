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
    public class MeetInfoSzGm : WebSiteCrawller
    {
        public MeetInfoSzGm()
            : base()
        {
            this.Group = "会议信息";
            this.Title = "广东省深圳市政府采购光明新区分中心";
            this.Description = "自动抓取广东省深圳市政府采购光明新区分中心";
            this.ExistCompareFields = "Prov,Area,Road,MeetName,MeetPlace,ProjectName,BeginDate,PrjCode";
            this.MaxCount = 1000;
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,11:30,14:05,14:25,14:50,15:45,16:50,19:00";
            this.SiteUrl = "http://gm.szzfcg.cn/";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<MeetInfo>();
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("bordercolor", "#222222")));
            if (nodeList != null && nodeList.Count > 0)
            {
                TableTag table = nodeList[0] as TableTag;
                for (int i = 0; i < table.RowCount; i++)
                {
                    string meetTime = string.Empty, prjName = string.Empty, meetName = string.Empty, place = string.Empty, builUnit = string.Empty;
                    TableRow tr = table.Rows[i];
                    if (tr.GetAttribute("valign") == "top")
                    {
                        string meetstr = string.Empty;
                        string temp = tr.Columns[1].ToPlainTextString().Replace("（Y）", "").Replace("(Y)", "").Replace("(一年)", "").Replace("（一年）", "");
                        string code = temp.Replace("(", "（").Replace(")", "）").GetRegexBegEnd("（", "）");

                        if (!string.IsNullOrEmpty(code))
                        {
                            meetstr = temp.Replace("（" + code + "）", "").Replace("(" + code + ")", "");//.Replace("（", "").Replace("(", "").Replace("）", "").Replace(")", "");
                        }
                        else
                            meetstr = temp;
                        place = meetstr.Replace("(", "（").Replace(")", "）").GetRegexBegEnd("（", "）");
                        if (!string.IsNullOrEmpty(place))
                        {
                            meetstr = meetstr.Replace("（" + place + "）", "").Replace("(" + place + ")", "");
                        }
                        string[] str = meetstr.Split(' ');
                        prjName = str[0];
                        if (str.Length > 1)
                            builUnit = str[1];
                        if (string.IsNullOrEmpty(builUnit) && str.Length > 2)
                            builUnit = str[2];
                        if (builUnit.Contains("开标") || builUnit.Contains("评标"))
                        {
                            place = builUnit;
                            builUnit = string.Empty;
                        }
                        meetName = "开标会";
                        System.Text.RegularExpressions.Regex regDate = new System.Text.RegularExpressions.Regex(@"\d{4}-\d{1,2}-\d{1,2} \d{1,2}:\d{1,2}");
                        meetTime = regDate.Match(meetstr).Value;
                        if (string.IsNullOrEmpty(meetTime))
                        {
                            meetTime = meetstr.GetDateRegex();
                        }

                        MeetInfo info = ToolDb.GenMeetInfo("广东省", "深圳政府采购", string.Empty, string.Empty, prjName, place, meetName, meetTime,
                              string.Empty, "深圳市政府采购光明新区分中心", SiteUrl, code, builUnit, string.Empty, string.Empty);
                        list.Add(info);

                        if (!crawlAll && list.Count >= this.MaxCount)
                        {
                            IList<MeetInfo> result = list as IList<MeetInfo>;
                            // 删除 
                            string bDate = result.OrderBy(x => x.BeginDate).ToList()[0].BeginDate.ToString().GetDateRegex("yyyy/MM/dd");
                            string eDate = Convert.ToDateTime(result.OrderByDescending(x => x.BeginDate).ToList()[0].BeginDate).AddDays(1).ToString().GetDateRegex("yyyy/MM/dd");
                            string sqlwhere = " where City='深圳政府采购' and InfoSource='深圳市政府采购光明新区分中心' and BeginDate>='" + bDate + "' and BeginDate<='" + eDate + "'";
                            string delMeetSql = "delete from MeetInfo " + sqlwhere;
                            int countMeet = ToolDb.ExecuteSql(delMeetSql); 
                            return list;
                        }
                    }
                }
            }
            if (list != null && list.Count > 0)
            {
                IList<MeetInfo> result = list as IList<MeetInfo>;
                // 删除 
                string bDate = result.OrderBy(x => x.BeginDate).ToList()[0].BeginDate.ToString().GetDateRegex("yyyy/MM/dd"), eDate = Convert.ToDateTime(result.OrderByDescending(x => x.BeginDate).ToList()[0].BeginDate).AddDays(1).ToString().GetDateRegex("yyyy/MM/dd");
                string sqlwhere = " where City='深圳政府采购' and InfoSource='深圳市政府采购光明新区分中心' and BeginDate>='" + bDate + "' and BeginDate<='" + eDate + "'";
                string delMeetSql = "delete from MeetInfo " + sqlwhere;
                int countMeet = ToolDb.ExecuteSql(delMeetSql); 
            }
            return list;
        }
    }
}
