using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class InviteMeiZhouGGZY : WebSiteCrawller
    {
        public InviteMeiZhouGGZY()
        {
            this.Group = "招标信息";
            this.Title = "梅州市公共资源交易中心招标";
            this.Description = "自动抓取梅州市公共资源交易中心招标公告";
            this.PlanTime = "9:10,11:10,14:10,16:10,18:10";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 300;
            this.SiteUrl = "http://www.mzggzy.com/TPFront/jsgc/004001/";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;

            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception)
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "wb-page-li")), true), new TagNameFilter("a")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    ATag atag = pageList[pageList.Count - 2] as ATag;
                    string temp = atag.LinkText.GetReplace("1/","");
                    pageInt = int.Parse(temp);
                }
                catch
                { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.mzggzy.com/TPFront/jsgc/004001/?pageing=" + i.ToString(), Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"),new HasAttributeFilter("class", "ewb-data-items ewb-pt6")),true),new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {

                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                        prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                        specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                        remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                        CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = nodeList[j].GetATag();
                        prjName = aTag.ChildrenHTML.GetReplace("\r\n","").ToRegString();
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.mzggzy.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }   
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "infodetail")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                           
                            if (string.IsNullOrWhiteSpace(buildUnit))
                            {
                                buildUnit = inviteCtx.GetRegex("招标人（盖章）");
                                if (string.IsNullOrWhiteSpace(buildUnit))
                                    buildUnit = inviteCtx.GetRegex("招标人（盖单位章）");
                            }
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                           
                            msgType = "梅州市公共资源交易中心";
                            specType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "梅州市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
