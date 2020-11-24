using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using Crawler;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Util;

namespace Crawler.Instance
{
    public class InviteZhanJiangJSTwo : WebSiteCrawller
    {
        public InviteZhanJiangJSTwo()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省湛江市建设工程招标信息";
            this.Description = "自动抓取广东省湛江市建设工程招标信息";
            this.SiteUrl = "http://zb.zjcic.net/Default.aspx?tabid=95";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default, ref cookiestr);
            }
            catch (Exception ex)
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("id", "dnn_ctr513_ArticleList_cboPages")));
            if (nodeList != null && nodeList.Count > 0)
            {
                string oo = nodeList.AsString().Trim();
                page = Convert.ToInt32(oo.Substring(oo.LastIndexOf("第")).ToString().Replace("第", "").Replace("页", "").Trim());
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(htl);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "__EVENTTARGET",
                        "__EVENTARGUMENT",
                        "__LASTFOCUS"  ,
                        "__VIEWSTATE",
                        "dnn$ctr513$ArticleList$cboPages",
                        "ScrollTop",
                        "__dnnVariable"
                    }, new string[]{
                        "dnn$ctr513$ArticleList$cboPages",
                       string.Empty,
                        string.Empty,
                        viewState,
                        (i-1).ToString(),
                        "716",
                        eventValidation
                    });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default, ref cookiestr);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "dnn_ctr513_ArticleList_PanelA")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = tableNodeList.SearchFor(typeof(TableTag), true)[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        if (tr.ColumnCount < 2)
                        {
                            continue;
                        }
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                  prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                  specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                  remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                  CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        prjName = tr.Columns[0].ToPlainTextString().Trim();
                        beginDate = tr.Columns[1].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[0].SearchFor(typeof(ATag), true)[0] as ATag;

                        InfoUrl = "http://zb.zjcic.net" + aTag.Link.Replace("amp;", "").Trim();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            Logger.Error("InviteZhanJiangJSTwo");
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "dnn_ctr377_ArticleShow_lblContent")));
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            inviteCtx = dtnode.AsString().Trim().Replace("&#160;", "").Trim();
                            Regex regBuidUnit = new Regex(@"(招标单位|招标人|招  标 单 位)：[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(inviteCtx).Value.Replace("招标单位：", "").Replace("招  标 单 位：", "").Replace("：", "").Replace("&#160;", "").Trim();
                            if (buildUnit == "")
                            {
                                Regex regBuidUnitT = new Regex(@"招 标 单 位： [^\r\n]+\r\n");
                                buildUnit = regBuidUnitT.Match(inviteCtx).Value.Replace("招 标 单 位： ", "").Replace("&#160;", "").Trim();
                            }
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            Regex regPrjAddr = new Regex(@"(工程地点|工程地址|地 址|工  程 地 点)(：|:)[^\r\n]+\r\n");
                            prjAddress = regPrjAddr.Match(inviteCtx).Value.Replace("工程地点：", "").Replace("工程地址", "").Replace("地 址", "").Replace("工  程 地 点：", "").Replace("：", "").Trim();
                            if (prjAddress == "")
                            {
                                Regex regPrjAddrT = new Regex(@"工 程 地 点： [^\r\n]+\r\n");
                                prjAddress = regPrjAddrT.Match(inviteCtx).Value.Replace("工 程 地 点： ", "").Trim();
                            }
                            msgType = "湛江市建设工程交易中心";
                            specType = "建设工程";
                            if (prjAddress == "")
                            {
                                prjAddress = "见招标信息";
                            }
                            prjName = prjName.Replace("·", "");
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "湛江市区", "",
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
