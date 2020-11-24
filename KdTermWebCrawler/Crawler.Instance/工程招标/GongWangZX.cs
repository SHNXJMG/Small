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
using System.Web.UI.HtmlControls;

namespace Crawler.Instance
{
    public class GongWangZX : WebSiteCrawller
    {
        public GongWangZX()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "中国公网在线";
            this.Description = "自动抓取中国公网在线";
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.gy-center.net/announce/list-76-1.jhtml";//"http://www.bidding.csg.cn/"
            this.MaxCount = 100;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList noList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "yema")));
            if (noList != null && noList.Count > 0)
            {
                string temp = noList.AsString();
                try
                {
                    Regex reg = new Regex(@"/[^页]+页");
                    string result = reg.Match(temp).Value.Replace("页", "").Replace("/", "");
                    pageInt = Convert.ToInt32(result);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.gy-center.net/announce/list.jhtml?visi_id=&cid=76&chid=&gid=&thistype=&searchcid=&keyword=&action=yes&interval=&page="+i.ToString(), Encoding.Default);
                    }
                    catch 
                    {
                        continue; 
                    }
                }
                parser = new Parser(new Lexer(html)); 
                NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "tab01"))), new TagNameFilter("ul"))), new TagNameFilter("li")));
                if (dtlList != null && dtlList.Count > 0)
                { 
                    for (int j = 0; j < dtlList.Count-1; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                       prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                       specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                       remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                       CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                         
                        string temp = dtlList[j].ToPlainTextString();
                        string tempHtl = dtlList[j].ToHtml();
                        prjName = ToolHtml.GetHtmlAtagValue("title", tempHtl);
                        beginDate = ToolHtml.GetRegexDateTime(temp);
                        InfoUrl = "http://www.gy-center.net/announce/" + ToolHtml.GetHtmlAtagValue("href", tempHtl);
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.Default); 
                            htlDtl = System.Text.RegularExpressions.Regex.Replace(htlDtl, "(<script)[\\s\\S]*?(</script>)", "");
                        }
                        catch 
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList htlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "r_content_right_main")));
                        if (htlList != null && htlList.Count > 0)
                        {
                            HtmlTxt = htlList.ToHtml();
                            inviteCtx = Regex.Replace(HtmlTxt, "<[^>]*>", "").Replace("&nbsp;", "").Replace(" ", "").Replace("\t\t","").Replace("\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            prjAddress = ToolHtml.GetRegexString(inviteCtx, ToolHtml.AddressRegex);
                            buildUnit = ToolHtml.GetRegexString(inviteCtx, ToolHtml.BuildRegex);
                            code = ToolHtml.GetRegexString(inviteCtx, ToolHtml.CodeRegex);
                            prjAddress = ToolHtml.GetSubString(prjAddress,150);
                            buildUnit = ToolHtml.GetSubString(buildUnit,150);
                            code = ToolHtml.GetSubString(code, 50);
                            if (string.IsNullOrEmpty(code))
                            {
                                code = "见招标信息";
                            }
                            if (string.IsNullOrEmpty(prjAddress))
                            {
                                prjAddress = "见招标信息";
                            }
                            specType = "其他";
                            msgType = "工网在线";
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "工网在线";
                            }
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "电网专项工程", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
