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
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.IO;

namespace Crawler.Instance
{
   public  class WraChufa:WebSiteCrawller
    {
        public WraChufa()
            : base()
        {
            this.PlanTime = "1-28 2:00,4-28 2:00,7-28 2:00,10-28 2:00";
            this.Group = "警示信息";
            this.Title = "深圳市住房和建设局行政处罚";
            this.Description = "自动抓取深圳市住房和建设局行政处罚";
            this.ExistCompareFields = "Code,WarningName,WarningType";
            this.MaxCount = 1000;
            this.SiteUrl = "http://61.144.226.2:8001/web/cxda/xzcfAction.do?method=toList";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            int count = 1,sqlCount = 1;
            IList list = new List<CorpWarning>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("a"), new HasAttributeFilter("id", "lx")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.GetATagHref().GetRegexBegEnd("page=", "&");
                    pageInt = int.Parse(temp);
                }
                catch
                {
                }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = ToolWeb.GetHtmlByUrl(this.SiteUrl + "&page=" + i.ToString(), Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(htl));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "bean")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, warningName = string.Empty, deliveryDate = string.Empty, warningType = string.Empty, punishmentType = string.Empty, prjNumber = string.Empty, totalScore = string.Empty, resultScore = string.Empty, corpType = string.Empty, publicEndDate = string.Empty, warningEndDate = string.Empty, prjName = string.Empty, badInfo = string.Empty, msgType = string.Empty, color = string.Empty;
                        TableRow tr = table.Rows[j];
                        code = tr.Columns[1].ToPlainTextString().GetATag().LinkText;
                        warningName = tr.Columns[2].ToNodePlainString();
                        deliveryDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        warningType = tr.Columns[4].ToNodePlainString();
                        punishmentType = tr.Columns[5].ToNodePlainString();
                        string infoUrl = "http://61.144.226.2:8001/web/cxda/xzcfAction.do?method=downLoadXzcfjdRemote&xzcfjdname=" + tr.Columns[1].GetATagValue("onclick").Replace("'", "lxl").GetRegexBegEnd("lxl", "lxl");
                        msgType = "深圳市住房和建设局";
                        CorpWarning info = ToolDb.GenCorpWarning("广东省", "深圳市区", "", code, warningName, deliveryDate, warningType, punishmentType, prjNumber, totalScore, resultScore, corpType, publicEndDate, warningEndDate, prjName, badInfo, msgType, color);
                        
                        sqlCount++;
                        if (!crawlAll && sqlCount >= this.MaxCount) return list;
                        if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                        {
                            BaseAttach attach = null;
                            try
                            {
                                attach = ToolHtml.GetBaseAttachByUrl(infoUrl, code, info.Id, "SiteManage\\Files\\Attach\\");
                            }
                            catch { }
                            if (attach != null)
                                ToolDb.SaveEntity(attach, "");
                        }
                        count++;
                        if (count >= 200)
                        {
                            count = 1;
                            Thread.Sleep(480000);
                        }
                    }
                }
            }
            return list;
        }
    }
}
