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
    public class NoticeSZJZFAJG : WebSiteCrawller
    {
        public NoticeSZJZFAJG()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省深圳市交易中心建筑方案设计结果公示";
            this.PlanTime = "9:31,11:31,14:31,17:31";
            this.Description = "自动抓取广东省深圳市交易中心建筑方案设计结果公示";
            this.ExistCompareFields = "Prov,City,InfoSource,InfoTitle,InfoType,PublishTime,PrjCode";
            this.SiteUrl = "http://www.szjsjy.com.cn/BusinessInfo/PsgsList.aspx";
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
                { }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_Content_GridView1")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    int result = pageInt > 1 ? table.RowCount - 1 : table.RowCount;
                    for (int j = 1; j < result; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        InfoType = "建筑方案设计评审结果公示";
                        prjCode = tr.Columns[1].ToNodePlainString();
                        InfoTitle = tr.Columns[2].ToNodePlainString();
                        PublistTime = tr.Columns[4].ToPlainTextString();
                        InfoUrl = SiteUrl;
                        InfoCtx = "工程编号：" + prjCode + "\r\n工程名称：" + InfoTitle + "\r\n信息内容：" + tr.Columns[3].ToNodePlainString() + "\r\n发布时间：" + PublistTime;
                        htmlTxt = InfoCtx;
                        NoticeInfo info = ToolDb.GenNoticeInfo("广东省", "深圳市工程", string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, MsgTypeCosnt.ShenZhenMsgType, InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty, htmlTxt); 
                        string url = "http://www.szjsjy.com.cn/BusinessInfo/" + tr.Columns[4].GetATagHref();
                        try
                        {
                            string fileHtl = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
                            parser = new Parser(new Lexer(fileHtl));
                            NodeList a = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (a != null && a.Count > 0)
                            {
                                for (int d = 0; d < a.Count; d++)
                                {
                                    ATag file = a[d] as ATag;
                                    if (file.IsAtagAttach())
                                    {
                                        string alink = "http://www.szjsjy.com.cn/" + file.Link.Replace("../","");
                                        BaseAttach attach = ToolDb.GenBaseAttach(file.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, alink);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }

                        }
                        catch { }

                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
