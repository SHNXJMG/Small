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
    public class InviteChaoZhouJS : WebSiteCrawller
    {
        public InviteChaoZhouJS()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省潮州市建设工程招标信息";
            this.Description = "自动抓取广东省潮州市建设工程招标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.czjsw.net/class.asp?id=69";
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
                htl = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            { 
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "page")), true), new TagNameFilter("a")));
            if (nodeList != null && nodeList.Count > 0)
            {
                try
                {
                    string temp = nodeList[nodeList.Count - 1].GetATagHref();
                    temp = temp.Remove(0, temp.LastIndexOf('=')+1);
                    page = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i,Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter( new TagNameFilter("ul"), new HasAttributeFilter("id", "listul")),true),new TagNameFilter("li")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                { 
                    for (int j = 0; j < tableNodeList.Count; j++)
                    {

                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                 prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                 specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                 remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                 CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                         
                        ATag aTag = tableNodeList[j].GetATag();
                        prjName = aTag.LinkText;
                        InfoUrl = "http://www.czjsw.net" + aTag.Link.Replace("amp;", "").Trim();
                        beginDate = tableNodeList[j].ToPlainTextString().GetDateRegex();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").GetJsString();
                        }
                        catch (Exception)
                        { 
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));
                        if (dtnode!=null&&dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex();

                            msgType = "潮州市建设工程交易中心";
                            specType = "建设工程";
                            inviteType = ToolHtml.GetInviteTypes(prjName);  
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "潮州市区", "",
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
