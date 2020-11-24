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
   public class InviteShanWei : WebSiteCrawller
   {
       public InviteShanWei()
           : base(true)
       {
           this.Group = "招标信息";
           this.Title = "广东省汕尾市工程建设招标信息";
           this.Description = "自动抓取广东省汕尾市工程建设招标信息";
           this.ExistCompareFields = "Code,ProjectName,InfoUrl";
           this.PlanTime = "9:07,10:07,11:37,14:07,16:07,17:37,20:07"; 
           this.SiteUrl = "http://219.129.166.87/website/buildproject/buildProjectSjAction!proMainList.action?gkmlbh=XMJS_ZBTB&tId=10&tpId=2";
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
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_bottom")));
            if (nodeList != null && nodeList.Count > 0)
            {
                Regex regexPage = new Regex(@"/\d+页");
                page = int.Parse(regexPage.Match(nodeList.AsString()).Value.Trim(new char[] { '/', '页' }));
            }
            for (int j = 1; j < page; j++)
            {
                //if (j > 1)
                //{
                //    try
                //    {
                //        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(GetStartUrl() + "&ipage=" + j.ToString()), Encoding.Default);
                //    }
                //    catch (Exception ex) { continue; }
                //}
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "list_div")));
                if (tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
                    for (int i = 0; i < table.RowCount; i++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                               prjAddress = string.Empty, inviteCtx = string.Empty, bidType = string.Empty,
                               specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                               remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                               CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[i];
                        prjName = tr.Columns[0].ToPlainTextString().Trim();
                    }
                }
            }
            return null;
        }
    }
}
