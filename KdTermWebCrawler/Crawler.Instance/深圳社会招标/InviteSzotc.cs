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
    public class InviteSzotc : WebSiteCrawller
    {
        public InviteSzotc()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "深圳市东方招标有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳市东方招标有限公司招标信息";
            this.SiteUrl = "http://www.sz-otc.com/zhaobiao/index_1.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "fenye123")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                string pageTemp = tdNodes.AsString().Replace("&nbsp;", "").Trim(); 
                try
                {
                    pageInt = int.Parse(ToolHtml.GetRegexString(pageTemp, "共", "页"));
                }
                catch (Exception ex) { }


                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.sz-otc.com/zhaobiao/index_" + i.ToString()) + ".html", Encoding.Default);
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }

                    parser = new Parser(new Lexer(html));
                    NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "zhaobiao_list")));

                    if (nodeList != null && nodeList.Count > 0)
                    {
                        TableTag table = nodeList[0] as TableTag;
                        for (int j = 0; j < nodeList.Count; j++)
                        {
                            string htl = string.Empty;
                            htl = nodeList[j].ToHtml();
                            Parser ul = new Parser(new Lexer(htl));
                            NodeFilter filter = new TagNameFilter("li");
                            NodeList liList = ul.ExtractAllNodesThatMatch(filter);
                            if (liList != null && liList.Count > 0)
                            {
                                for (int k = 0; k < liList.Count; k++)
                                {
                                    string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                                    ATag aTag = liList.SearchFor(typeof(ATag), true)[k] as ATag;
                                    InfoUrl = "http://www.sz-otc.com" + aTag.Link;
                                    prjName = aTag.LinkText.Replace("[新]", "").Replace("&#160;", "");
                                    if (prjName.Contains("]"))
                                    {
                                        try
                                        {
                                            int beg = prjName.IndexOf("]");
                                            prjName = prjName.Substring(beg + 1, prjName.Length - beg - 1);
                                        }
                                        catch { }
                                    }
                                    string htmldetail = string.Empty;
                                    try
                                    {
                                        htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                                    }
                                    catch { return null; }
                                    Parser dtlparser = new Parser(new Lexer(htmldetail));
                                    NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "right_content"), new TagNameFilter("div")));
                                    if (dtnode != null && dtnode.Count > 0)
                                    {
                                        HtmlTxt = dtnode.ToHtml();
                                        inviteCtx = dtnode.AsString().Replace("&nbsp;", "").Replace(" ", "");
                                        string invite = inviteCtx.Replace("点击", "\r\n").Replace("发布人", "\r\n");
                                        specType = "其他";
                                        msgType = "深圳市东方招标有限公司";
                                        if (string.IsNullOrEmpty(prjName))
                                        {
                                            Regex regexName = new Regex(@"(工程名称|项目名称)(:|：)[^\r\n]+\r\n");
                                            prjName = regexName.Match(inviteCtx).Value.Replace("工程名称", "").Replace("项目名称", "").Replace(":", "").Replace("：", "").Trim();
                                        }
                                        Regex regex = new Regex(@"(工程编号|招标编号)(:|：)[^\r\n]+\r\n");
                                        code = regex.Match(invite).Value.Replace("工程编号", "").Replace("招标编号", "").Replace(":", "").Replace("：", "").Trim();

                                        Regex regexAddress = new Regex(@"(地址|项目地址)(:|：)[^\r\n]+\r\n");
                                        prjAddress = regexAddress.Match(inviteCtx).Value.Replace("地址", "").Replace("项目地址", "").Replace(":", "").Replace("：", "").Trim();

                                        Regex regexUnit = new Regex(@"(招标单位|招标机构)(:|：)[^\r\n]+\r\n");
                                        buildUnit = regexUnit.Match(inviteCtx).Value.Replace("招标单位", "").Replace("招标机构", "").Replace(":", "").Replace("：", "").Trim();


                                        Regex regexCar = new Regex(@"(开始日期|发布日期)(:|：)[^\r\n]+\r\n");
                                        beginDate = regexCar.Match(invite).Value.Replace("开始日期", "").Replace("发布日期", "").Replace(":", "").Replace("：", "").Trim();

                                        if (!string.IsNullOrEmpty(beginDate))
                                        {
                                            string time = string.Empty;
                                            for (int leng = 0; leng < beginDate.Length; leng++)
                                            {
                                                if (leng < 10)
                                                {
                                                    time += beginDate.Substring(leng, 1);
                                                }
                                            }
                                            beginDate = time;
                                        }

                                        specType = "其他";
                                        msgType = "深圳市东方招标有限公司"; 
                                        if (buildUnit == "")
                                        {
                                            buildUnit = "";
                                        } 
                                        inviteType = ToolHtml.GetInviteTypes(prjName);
                                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                        list.Add(info);

                                        if (!crawlAll && list.Count >= 20) return list;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
