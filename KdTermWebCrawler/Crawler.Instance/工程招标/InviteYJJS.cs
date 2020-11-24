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
    public class InviteYJJS : WebSiteCrawller
    {
        public InviteYJJS()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省阳江市建设工程招标信息";
            this.Description = "自动抓取广东省阳江市建设工程招标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.SiteUrl = "http://www.yjggzy.cn/Query/JsgcBidAfficheQuery2/d4f193435ad04447a997719474139181";
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
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
                Regex regexHtml = new Regex(@"<script[^<]*</script>");
                htl = regexHtml.Replace(htl, "");
            }
            catch (Exception ex)
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagination")), true), new TagNameFilter("a")));
            if (nodeList != null && nodeList.Count > 0)
            {
                try
                {
                    string temp = nodeList[nodeList.Count - 1].GetATagHref();
                    string pageCount = temp.Replace(temp.Remove(temp.IndexOf("=")), "").Replace("=", "");
                    page = int.Parse(pageCount);
                }
                catch { }
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "?page="+i.ToString(), Encoding.UTF8, ref cookiestr);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList liNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "list")), true), new TagNameFilter("li")));
                if (liNode != null && liNode.Count > 0)
                {
                    for (int j = 0; j < liNode.Count; j++)
                    { 
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                            prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                            specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                            remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                            CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        beginDate = liNode[j].ToPlainTextString().GetDateRegex("yyyy/MM/dd");
                        prjName = liNode[j].ToPlainTextString().Replace(beginDate, "").ToNodeString().Replace(" ", "").Replace("·", "");
                        ATag aTag = liNode[j].GetATag();
                        InfoUrl = "http://www.yjggzy.cn" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            Logger.Error("InviteYJJS");
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtlNode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("dl"), new HasAttributeFilter("class", "acticlecontent")));
                        if (dtlNode == null || dtlNode.Count < 1)
                        {
                            parserdetail.Reset();
                            dtlNode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "nr")));
                        }
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.ToHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            inviteType = prjName.GetInviteBidType();
                            code = inviteCtx.GetCodeRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = inviteCtx.GetRegex("招标代理");
                            msgType = "阳江市建设工程交易中心";
                            specType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "阳江市区", "", string.Empty, code, prjName,
                                prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType,
                                inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parserdetail.Reset();
                            NodeList attachNode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("style", "width: 500px")));
                            if (attachNode != null && attachNode.Count > 0)
                            {
                                for (int k = 0; k < attachNode.Count; k++)
                                { 
                                    Parser nameParser =new Parser(new Lexer(attachNode.ToHtml()));
                                    NodeList nameNode = nameParser.ExtractAllNodesThatMatch(new TagNameFilter("span"));
                                    string attachName = nameNode[0].ToNodePlainString();
                                    nameParser.Reset();
                                    NodeList aTagNode = nameParser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                    ATag attachATag = aTagNode.GetATag();
                                    base.AttachList.Add(ToolDb.GenBaseAttach(attachName, info.Id, "http://www.yjggzy.cn" + attachATag.Link));
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
        private string returnS(string st)
        {
            if (st.Contains("十月"))
            {
                st = st.Replace("十月", "10月").ToString();
            }
            if (st.Contains("十日"))
            {
                st = st.Replace("十日", "10").ToString();
            }
            if (st.Contains("二十"))
            {
                st = st.Replace("二十", "2").ToString();
            }
            if (st.Contains("三十"))
            {
                st = st.Replace("三十", "3").ToString();
            }
            if (st.Contains("十"))
            {
                st = st.Replace("十", "1").ToString();
            }
            if (st.Contains("一"))
            {
                st = st.Replace("一", "1").ToString();
            }
            if (st.Contains("二"))
            {
                st = st.Replace("二", "2").ToString();
            }
            if (st.Contains("三"))
            {
                st = st.Replace("三", "3").ToString();
            }
            if (st.Contains("四"))
            {
                st = st.Replace("四", "4").ToString();
            }
            if (st.Contains("五"))
            {
                st = st.Replace("五", "5").ToString();
            }
            if (st.Contains("六"))
            {
                st = st.Replace("六", "6").ToString();
            }
            if (st.Contains("七"))
            {
                st = st.Replace("七", "7").ToString();
            }
            if (st.Contains("八"))
            {
                st = st.Replace("八", "8").ToString();
            }
            if (st.Contains("九"))
            {
                st = st.Replace("九", "9").ToString();
            }
            if (st.Contains("〇") || st.Contains("零"))
            {
                st = st.Replace("〇", "0").ToString();
            }
            return st;
        }
    }
}
