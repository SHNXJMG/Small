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
namespace Crawler.Instance
{
    public class BidSzymcw : WebSiteCrawller
    {
        public BidSzymcw()
            : base()
        {
            this.Group = "代理机构中标信息";
            this.Title = "深圳市裕明财务咨询有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳市裕明财务咨询有限公司中标信息";
            this.ExistCompareFields = "Prov,City,Area,Road,Code,ProjectName,InfoUrl";
            this.SiteUrl = "http://www.ymcw.com/message2.htm";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码 
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string HtmlTxt = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {

                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "749")));
            if (nodeList != null && nodeList.Count > 0)
            {
                HtmlTxt = nodeList.AsHtml();
                TableTag table = nodeList[0] as TableTag;
                //int rowIndex = 8;
                //for (int j = 6; j < table.RowCount - 3; j++)
                //{
                //    TableRow tr = table.Rows[j];
                //    if (tr.ToPlainTextString().Contains("中标通知书"))
                //    {
                //        rowIndex = j+2;
                //    }

                //}

                for (int j = 13; j < table.RowCount - 3; j++)
                {  
                    TableRow tr = table.Rows[j]; 
                    if (tr.ToPlainTextString().Contains("注："))
                    {
                        continue;
                    }
                    if (tr.ToPlainTextString().Contains("中标通知书"))
                    {
                        j++;
                        continue;
                    }
                    
                    string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, 
                        code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, 
                        bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, 
                        bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty;
                  
                    code = tr.Columns[2].ToPlainTextString().Trim();
                    prjName = tr.Columns[3].ToPlainTextString().Trim();
                    bidUnit = tr.Columns[4].ToPlainTextString().Trim();
                    string bid = tr.Columns[5].ToPlainTextString().Trim();
                    beginDate = tr.Columns[7].ToPlainTextString().Trim();
                    InfoUrl = "http://www.ymcw.com/message2.htm";

                    HtmlTxt = string.Format("<p>招标编号：{0}<br/>项目名称：{1}<br/>中标单位：{2}<br/>中标项目：{3}<br/>中标时间：{4}<br/></p>", code, prjName, bidUnit,bid, beginDate);
                    bidCtx = string.Format("招标编号:{0}\r\n项目名称:{1}\r\n中标单位:{2}\r\n中标时间:{3}\r\n",code,prjName,bidUnit,beginDate);
                    specType = "其他";
                    msgType = "深圳市裕明财务咨询有限公司";
                    bidType = ToolHtml.GetInviteTypes(prjName);
                    prjName = ToolDb.GetPrjName(prjName);
                    if (prjName.Contains("深圳市人民检察院电子物证设备"))
                        continue;
                    BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
                    list.Add(info);
                    if (!crawlAll && list.Count >= this.MaxCount)
                        return list;
                }
            }
            return list;
        }

    }
}
