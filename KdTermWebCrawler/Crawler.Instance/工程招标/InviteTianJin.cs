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
    public class InviteTianJin : WebSiteCrawller
    {
        public InviteTianJin()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "天津建设工程信息网";
            this.Description = "自动抓取天津建设工程信息网";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 1000;
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.tjconstruct.cn/zbxx.aspx";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("施工招标", "sgzb");
            dic.Add("监理招标", "jlzb");
            dic.Add("设计招标", "sjzb");
            dic.Add("设备招标", "sbzb");
            dic.Add("公路招标", "glzb");
            dic.Add("专业招标", "qtzb");
            foreach (string key in dic.Keys)
            {
                int pageInt = 1, listCount = 0;
                string html = string.Empty;
                string viewState = string.Empty;
                string eventValidation = string.Empty;
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?type=" + dic[key]);
                }
                catch { continue; }
                Parser parser = new Parser(new Lexer(html));
                NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "ctl00_ContentPlaceHolder1_ctl00_AspNetPager1")));
                if (pageNode != null && pageNode.Count > 0)
                {
                    string temp = pageNode.AsString();
                    try
                    {
                        string page = temp.GetRegexBegEnd(",共", "页");
                        pageInt = int.Parse(page);
                    }
                    catch { }
                }
                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "?type=" + dic[key] + "&page=" + i);
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Tp1")));
                    if (listNode != null && listNode.Count > 0)
                    {
                        TableTag table = listNode[0] as TableTag;
                        for (int j = 1; j < table.RowCount; j++)
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            TableRow tr = table.Rows[j];
                            prjName = tr.Columns[0].ToNodePlainString();
                            buildUnit = tr.Columns[1].ToNodePlainString();
                            code = tr.Columns[2].ToNodePlainString();
                            endDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
                            InfoUrl = "http://www.tjconstruct.cn/" + tr.Columns[0].GetATagHref().Replace("new/../", "").Replace(" ","");
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, key == "公路招标" ? Encoding.UTF8 : Encoding.Default).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", key == "公路招标" ? "WordSection1" : "body")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.AsHtml();
                                inviteCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString();
                                prjAddress = inviteCtx.GetAddressRegex();
                                inviteType = key;
                                specType = "建设工程";
                                beginDate = inviteCtx.GetRegexBegEnd("招标公告发布时间", "至").Replace("：", "");
                                if (string.IsNullOrEmpty(beginDate))
                                {
                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList dateNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("p"), new HasAttributeFilter("class", "MsoNormal")));
                                    if (dateNode != null && dateNode.Count > 0)
                                    {
                                        beginDate = dateNode[0].ToPlainTextString();
                                    }
                                }
                                if (string.IsNullOrEmpty(beginDate))
                                    beginDate = endDate;
                                msgType = "天津市工程建设交易服务中心";
                                InviteInfo info = ToolDb.GenInviteInfo("天津市", "天津市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                listCount++;
                                if (!crawlAll && listCount >= this.MaxCount) goto type;
                            }
                        }
                    }
                }
            type: continue;
            }
            return list;
        }
    }
}
