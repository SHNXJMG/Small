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
using System.Web.UI.MobileControls;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Crawler.Instance
{
    public class NoticeInfoGdZbtb : WebSiteCrawller
    {
        public NoticeInfoGdZbtb()
            : base()
        {
            this.Group = "通知公示";
            this.Title = "广东省招投标监督网资格审查";
            this.Description = "自动抓取广东省招投标监督网资格审查";
            this.PlanTime = "8:52,9:42,10:32,11:32,13:42,15:02,16:32";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.gdzbtb.gov.cn/zgscbgbd/index.htm";
            this.MaxCount = 2000;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookieStr = string.Empty;
            int pageInt = 1;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "cn6")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().Replace("(", "kdxx").GetRegexBegEnd("kdxx", ",");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.gdzbtb.gov.cn/zgscbgbd/index_" + (i - 1).ToString() + ".htm", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "position2")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string InfoTitle = string.Empty, InfoType = string.Empty, PublistTime = string.Empty, InfoCtx = string.Empty, InfoUrl = string.Empty, prjCode = string.Empty, buildUnit = string.Empty, htmlTxt = string.Empty,
                            city = string.Empty;
                        InfoType = "资格审查";
                        InfoTitle = nodeList[j].GetATagValue("title");
                        if (InfoTitle.Contains("广东省"))
                        {
                            city = "广州市区";
                            InfoTitle = InfoTitle.Replace("[", "").Replace("]-", "").Replace("]", "").Replace("广东省", "");
                        }
                        else
                        {
                            string temp = InfoTitle.Replace("[", "kdxx").Replace("]", "xxdk").GetRegexBegEnd("kdxx", "xxdk");
                            InfoTitle = InfoTitle.Replace("[", "").Replace("]-", "").Replace("]", "").Replace(temp, "");
                            city = temp + "区";
                        }
                        InfoUrl = "http://www.gdzbtb.gov.cn/zgscbgbd/" + nodeList[j].GetATagHref().Replace("../", "").Replace("./", "");
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellSpacing", "1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            htmlTxt = dtlNode.AsHtml();
                            InfoCtx = htmlTxt.ToCtxString();
                            string attachUrl = string.Empty;
                            string aurl = htldtl.Replace("\"", "").GetRegexBegEnd("#pbbg_shongti", "target");
                            string attachName = htldtl.Replace("\"", "").GetRegexBegEnd("target=_blank>", "</a>");
                            if (aurl.ToLower().IndexOf("href") != -1)
                            {
                                attachUrl = aurl.Substring(aurl.ToLower().IndexOf("href"), aurl.Length - aurl.ToLower().IndexOf("href"));
                                attachUrl = attachUrl.Replace("href=", "").Replace("../", "").Replace("./", "");
                            }
                            if (string.IsNullOrEmpty(attachName))
                                attachName = InfoTitle;
                            PublistTime = InfoCtx.GetDateRegex();
                            if (string.IsNullOrEmpty(PublistTime))
                                PublistTime = DateTime.Now.ToString("yyyy-MM-dd");
                            NoticeInfo info = ToolDb.GenNoticeInfo("广东省", city, string.Empty, string.Empty, InfoTitle, InfoType, InfoCtx, PublistTime, string.Empty, "广东省招标投标监管网", InfoUrl, prjCode, buildUnit, string.Empty, string.Empty, string.Empty, string.Empty, htmlTxt);
                            list.Add(info);
                            if (!string.IsNullOrEmpty(attachUrl))
                            {
                                string fileUrl = string.Empty;
                                try
                                {
                                    fileUrl = DateTime.Parse(PublistTime).ToString("yyyyMM");
                                }
                                catch { fileUrl = DateTime.Now.ToString("yyyyMM"); }
                                string alink = "http://www.gdzbtb.gov.cn/zgscbgbd/" + fileUrl + "/" + attachUrl;
                                BaseAttach attach = ToolDb.GenBaseAttach(attachName, info.Id, alink);
                                base.AttachList.Add(attach);
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                            //WebBrowser web = new WebBrowser();
                            ////web.DocumentText = htldtl;                             //web.Navigate("http://www.jd.com/"); 
                            //HtmlElementCollection ctx = web.Document.GetElementsByTagName("a");

                        }
                    }
                }
            }
            return list;
        }
    }
}
