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
   public class InviteFoshanGM : WebSiteCrawller
    {
       public InviteFoshanGM()
           : base()
       {
           this.Group = "招标信息";
           this.Title = "广东省佛山市高明区";
           this.Description = "自动抓取广东省佛山市高明区招标信息";
           this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
           this.SiteUrl = "http://ztb.gaoming.gov.cn/jsgc/zbxx/";
       }

        protected override IList ExecuteCrawl(bool crawlAll)
        {

            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            int crawlMax = 0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default).Replace("&nbsp;", "");
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ny_21 tc")));
            if (sNode != null && sNode.Count > 0)
            {
                string pageString = sNode.AsString().Trim();
                Regex regexPage = new Regex(@"createPageHTML\([^\)]+\)");
                Match pageMatch = regexPage.Match(pageString);
                try { pageInt = int.Parse(pageMatch.Value.Replace("createPageHTML(", "").Replace(")", "").Split(',')[0].Trim()); }
                catch (Exception) { }
            }
            string cookiestr = string.Empty;

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {

                    try { html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "index_" + (i - 1).ToString() + ".html", Encoding.Default); }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(html));
                sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter( new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "ny_22"))),new TagNameFilter("li")));
                if (sNode!=null&&sNode.Count>0)
                {
                    for (int j = 0; j < sNode.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, bidType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt=string.Empty;
                        INode node = sNode[j];
                        ATag aTag = node.Children.SearchFor(typeof(ATag), true)[0] as ATag;
                        Div divTag = node.Children.SearchFor(typeof(Div), true)[1] as Div;
                        prjName = aTag.ToPlainTextString().Trim();
                        beginDate = divTag.ToPlainTextString().Trim(new char[]{'[',']',' '});
                        
                        InfoUrl = "http://ztb.gaoming.gov.cn/jsgc/zbxx/" + aTag.Link.Replace("../", "").Replace("./", "");
                        if (aTag.Link.Contains("../"))
                        {
                            InfoUrl = "http://ztb.gaoming.gov.cn/" + aTag.Link.Replace("../", "").Replace("./", "");
                        }
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "con_10 tl"), new TagNameFilter("div")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n").Replace("<br/>", "\r\n"); }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "con_10 tl"), new TagNameFilter("div")));
                        
                        inviteCtx = dtnode.AsString().Replace(" ","");
                        Regex regCtx = new Regex(@"[\n]+");
                        inviteCtx = regCtx.Replace(inviteCtx, "\r\n");
                        Regex regPrjAdd = new Regex(@"(工程地点|工程地址|项目地址)：[^\r\n]+[\r\n]{1}");
                        prjAddress = regPrjAdd.Match(inviteCtx).Value.Replace("工程地点：", "").Replace("工程地址：", "").Replace("项目地址：", "").Replace(")", "").Replace("。", "").Trim();
                        Regex regCode = new Regex(@"GMJ[0-9]+");
                        code = regCode.Match(inviteCtx).Value;
                        Regex regbuildUnit = new Regex(@"(招标单位|招标人)：[^\r\n]+[\r\n]{1}");
                        buildUnit = regbuildUnit.Match(inviteCtx).Value.Replace("招标单位：", "").Replace("招标人：", "").Replace("。","").Trim();
                        if (Encoding.Default.GetByteCount(buildUnit) > 150)
                        {
                            buildUnit = buildUnit.Substring(0,150);
                        }
                        if (Encoding.Default.GetByteCount(prjAddress) > 200)
                        {
                            prjAddress = "见招标信息";
                        }
                        msgType = "佛山市高明区建设工程交易中心";
                        specType = "建设工程";
                        bidType = ToolHtml.GetInviteTypes(bidType);
                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "佛山市区", "高明区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, bidType, specType, otherType, InfoUrl, HtmlTxt);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                       
                    }
                    
                    
                  
                }
            }
            return list;
        }
    
    }
}
