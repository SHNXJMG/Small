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
using System.Data;

namespace Crawler.Instance
{
    public class BidUnitInfoLg : WebSiteCrawller
    {
        public BidUnitInfoLg()
            : base()
        {
            this.Group = "投标单位";
            this.Title = "深圳市建设工程交易中心龙岗分中心";
            this.PlanTime = "9:15";
            this.Description = "自动抓取深圳市建设工程交易中心龙岗分中心投标单位";
            this.SiteUrl = "http://jyzx.cb.gov.cn/LGJYZXWEB/SiteManage/SignUpPrint.aspx?gcbh=";
            this.ExistCompareFields = "CorpName,ProjectName,BuildUnit,Code,MsgType";
            this.MaxCount = 60;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidUnitInfo>();
            string bidUnitSql = "select Code from BidUnitInfo group by Code ";
            DataTable bidUnitDt = ToolDb.GetDbData(bidUnitSql);
            string whereId = string.Empty;
            if (bidUnitDt != null && bidUnitDt.Rows.Count > 0)
            {
                foreach (DataRow row in bidUnitDt.Rows)
                {
                    whereId += "'" +Convert.ToString(row["Code"])+ "',";
                }
            }
            string sql = string.Empty;
            if (!string.IsNullOrEmpty(whereId))
            {
                whereId = whereId.Remove(whereId.LastIndexOf(','));
                sql = "select Code from InviteInfo where MsgType='深圳市建设工程交易中心龙岗分中心' and City='深圳龙岗区工程' and Area='龙岗区' and BeginDate>='" + DateTime.Now.AddDays(-this.MaxCount).ToString("yyyy-MM-dd") + "' and Code not in ("+whereId+")";
            }
            else
            {
                sql = "select Code from InviteInfo where MsgType='深圳市建设工程交易中心龙岗分中心' and City='深圳龙岗区工程' and Area='龙岗区' and BeginDate>='" + DateTime.Now.AddDays(-this.MaxCount).ToString("yyyy-MM-dd") + "'";
            }
          

            DataTable dt = ToolDb.GetDbData(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string html = string.Empty;
                    string code = Convert.ToString(row["Code"]);
                    try
                    {
                        html = ToolWeb.GetHtmlByUrl(this.SiteUrl + code, Encoding.UTF8);
                    }
                    catch { continue; }

                    Parser parser = new Parser(new Lexer(html));
                    NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "tb")));
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        TableTag tablePrj = nodeList[0] as TableTag;
                        string prjName = tablePrj.Rows[1].Columns[1].ToNodePlainString();
                        string buildUnit = tablePrj.Rows[1].Columns[3].ToNodePlainString();
                        parser.Reset();
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            TableTag table = dtlNode[0] as TableTag;
                            for (int i = 1; i < table.RowCount; i++)
                            {
                                string prov = string.Empty, city = string.Empty, area = string.Empty,  corpName = string.Empty, registerMode = string.Empty, prjMgr = string.Empty, bidMember = string.Empty, phone = string.Empty, signUpDate = string.Empty, msgType = string.Empty;

                                TableRow tr = table.Rows[i];
                                 
                                corpName = tr.Columns[1].ToNodePlainString();
                                registerMode = tr.Columns[2].ToNodePlainString();
                                prjMgr = tr.Columns[3].ToNodePlainString();
                                bidMember = tr.Columns[4].ToNodePlainString();
                                phone = tr.Columns[5].ToNodePlainString();
                                signUpDate = tr.Columns[6].ToPlainTextString();
                                try
                                {
                                    DateTime.Parse(signUpDate);
                                }
                                catch { signUpDate = tr.Columns[6].ToPlainTextString().GetDateRegex(); }
                                msgType = "深圳市建设工程交易中心龙岗分中心";
                                BidUnitInfo info = ToolDb.GenBidUnitInfo("广东省", "深圳龙岗区工程", "龙岗区", prjName, buildUnit, code, corpName, registerMode, prjMgr, bidMember, phone, signUpDate, msgType);
                                list.Add(info); 
                            }
                        }

                    }
                }
            }
            return list;
        }
    }
}
