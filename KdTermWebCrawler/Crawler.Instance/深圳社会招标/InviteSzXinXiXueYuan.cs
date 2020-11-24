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

namespace Crawler.Instance
{
    public class InviteSzXinXiXueYuan : WebSiteCrawller
    {
        public InviteSzXinXiXueYuan()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "深圳信息职业技术学院招标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳信息职业技术学院招标信息";
            this.SiteUrl = "http://zbcg.sziit.edu.cn/zbxx.htm";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "headStyle27kkt9g3gy")));
            if (nodeList != null && nodeList.Count > 0)
            {
                try
                {
                    string temp = nodeList[0].ToPlainTextString().GetRegexBegEnd("/", "首").ToLower().Replace("&nbsp;", "");
                    page = int.Parse(temp);
                }
                catch { }
            }
            else page = 25;
            for (int i = page; i >= 1; i--)
            {
                if (i < page)
                {
                    try
                    {
                        string url = "http://zbcg.sziit.edu.cn/zbxx/"+i+".htm";
                        htl = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
                    }
                    catch   { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "winstyle66953")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = tableNodeList[0] as TableTag;
                    for (int j = 0; j < table.RowCount-1; j++)
                    {
                        TableRow tr = table.Rows[j];
                        if (tr.ColumnCount<2) continue;

                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                              prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                              specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                              remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                              CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, img = string.Empty,
                              HtmlTxt = string.Empty, downUrl = string.Empty, downName = string.Empty;


                        prjName = tr.Columns[1].ToNodePlainString();
                        if (prjName.Contains("暂停公告"))
                        {
                            continue;
                        }
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex("yyyy/MM/dd");

                        InfoUrl = "http://zbcg.sziit.edu.cn/" + tr.Columns[1].GetATagHref().Replace("../","").Replace("./","");
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch 
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "vsb_content")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            Regex regeximg = new Regex(@"<IMG[^>]*>");//去掉图片
                            HtmlTxt = regeximg.Replace(HtmlTxt, "");
                            inviteCtx = dtnode.AsString().Replace("&nbsp;", "").Trim();
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            inviteCtx = regexHtml.Replace(inviteCtx, "").Replace(" ", "").Replace("&ldquo;", "").Replace("&rdquo;", "").Trim();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            prjAddress = inviteCtx.GetAddressRegex();
                            Regex regBegin = new Regex(@"投标截止时间：[^\r\n]+[\r\n]{1}");
                            string date = regBegin.Match(inviteCtx).Value.Replace("投标截止时间：", "").Replace(" ", "").Trim();
                            Regex regDate = new Regex(@"\d{4}年\d{1,2}月\d{1,2}日");
                            endDate = regDate.Match(date).Value.Trim();
                            Regex regBuidUnit = new Regex(@"(招标机构|委托单位)(：|:)[^\r\n]+\r\n");
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (inviteType == "设备材料" || inviteType == "小型施工" || inviteType == "专业分包" || inviteType == "劳务分包" || inviteType == "服务" || inviteType == "勘察" || inviteType == "设计" || inviteType == "监理" || inviteType == "施工")
                            {
                                specType = "建设工程";
                            }
                            else
                            {
                                specType = "其他";
                            }  
                            msgType = "深圳信息职业技术学院";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "",
                             string.Empty, code, prjName, prjAddress, buildUnit,
                             beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
