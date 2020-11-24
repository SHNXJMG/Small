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
    public class InviteZhuHaiJS : WebSiteCrawller
    {
        public InviteZhuHaiJS()
            : base() 
        {
            this.Group = "招标信息";
            this.Title = "广东省珠海市建设工程招标信息";
            this.Description = "自动抓取广东省珠海市建设工程招标信息";
            this.MaxCount = 30;
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.SiteUrl = "http://www.cpinfo.com.cn/index/showList/000000000001/000000000431";
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
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "scott")), true), new TagNameFilter("a")));
            Regex regexPage = new Regex(@"共\d+页");
            try
            {
                Regex numpage = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                ATag link = (ATag)nodeList[nodeList.Count - 1];
                page = Convert.ToInt32(numpage.Match(link.Link).Value.Trim());
            }
            catch (Exception)
            { }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(htl);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "newtitle",
                        "totalRows",
                        "pageNO"  
                    }, new string[]{
                        string.Empty,
                        "0",
                        i.ToString()
                    });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default, ref cookiestr).Replace("<th", "<td").Replace("</th>", "</td>").Replace("&nbsp;", "");
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "cnewslist")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
                    for (int j = 1; j < table.RowCount - 2; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                  prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                  specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                  remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                  CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, img = string.Empty,
                                  HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[0].ToPlainTextString().Trim();
                        endDate = tr.Columns[2].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[0].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = aTag.Link;
                        ImageTag image = aTag.SearchFor(typeof(ImageTag), true)[0] as ImageTag;
                        //beginDate = DateTime.Now.Date.ToString();
                        //if (image == null)
                        //{
                        //    beginDate = endDate;
                        //    endDate = string.Empty;
                        //}
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("<th", "<td").Replace("</th>", "</td>").Replace("</TH>", "</td>").Replace("<TH", "<td").Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            Logger.Error("InviteZhuHaiJS"); 
                            continue;
                        }

                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "borderTB")));
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            TableTag tabletwo = (TableTag)dtnode[0];
                            for (int row = 0; row < tabletwo.RowCount; row++)
                            {
                                TableRow r = tabletwo.Rows[row];

                                for (int k = 0; k < r.ColumnCount; k++)
                                {
                                    string st = string.Empty;
                                    string st1 = string.Empty;
                                    st = r.Columns[k].ToPlainTextString().Trim();
                                    if (k + 1 < r.ColumnCount)
                                    {
                                        st1 = r.Columns[k + 1].ToPlainTextString().Trim();
                                    }
                                    inviteCtx += st + "：" + st1 + "\r\n";
                                    if (k + 1 <= r.ColumnCount)
                                    {
                                        k++;
                                    }
                                }
                            }

                            Regex regBuidUnit = new Regex(@"(招标人|招标人/招标代理)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(inviteCtx).Value.Replace("招标人：", "").Replace("招标人/招标代理：", "").Trim();
                            Regex regPrjAddr = new Regex(@"(建设地点|项目地址|建设单位)(：|:)[^\r\n]+\r\n");
                            prjAddress = regPrjAddr.Match(inviteCtx).Value.Replace("建设单位：", "").Replace("建设地点：", "").Replace("项目地址", "").Replace("：", "").Trim();
                            if (Encoding.Default.GetByteCount(prjAddress) > 200 || prjAddress == "")
                            {
                                prjAddress = "见招标信息";
                            }
                            Regex regcode = new Regex(@"项目编号(：|:)[^\r\n]+\r\n");
                            code = regcode.Match(inviteCtx).Value.Replace("项目编号：", "").Replace("：", "").Trim();
                            beginDate = inviteCtx.GetRegex("报名时间").GetDateRegex();
                            if (string.IsNullOrEmpty(beginDate)||DateTime.Parse(beginDate)>DateTime.Now)
                                beginDate = DateTime.Now.ToString("yyyy-MM-dd");
                            msgType = "珠海市建设工程交易中心";
                            specType = "建设工程";
                            inviteCtx = inviteCtx.Replace("<?", "").Replace("xml:namespace prefix = st1 ns = ", "").Replace("urn:schemas-microsoft-com:office:smarttags", "").Replace("/>", "").Trim();
                            inviteCtx = inviteCtx.Replace("<?", "").Replace("xml:namespace prefix = o ns = ", "").Replace("urn:schemas-microsoft-com:office:office", "").Replace("/>", "").Trim();
                            Regex regInvType = new Regex(@"[^\r\n]+[\r\n]{1}");

                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "珠海市区", "",
                                string.Empty, code, prjName, prjAddress, buildUnit,
                                beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parserdetail.Reset();
                            NodeList nodeListtwo = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "Noprint")), true), new TagNameFilter("a")));
                            if (nodeListtwo.Count > 0)
                            {
                                ATag aTa3g = nodeListtwo[0] as ATag;
                                BaseAttach attach = ToolDb.GenBaseAttach("工作议程(点击下载)", info.Id, aTa3g.Link);
                                base.AttachList.Add(attach);
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
