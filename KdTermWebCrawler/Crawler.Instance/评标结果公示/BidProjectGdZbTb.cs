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
    public class BidProjectGdZbTb : WebSiteCrawller
    {
        public BidProjectGdZbTb()
            : base()
        {
            this.Group = "评标结果公示";
            this.Title = "广东省招投标监督网评标报告";
            this.Description = "自动抓取广东省招投标监督网评标报告";
            this.PlanTime = "04:10";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.gdzbtb.gov.cn/pbbgbd/pingbiaobaogao.htm";
            this.MaxCount = 2000;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookieStr = string.Empty;
            int sqlCount = 0;
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
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.gdzbtb.gov.cn/pbbgbd/pingbiaobaogao_" + (i - 1).ToString() + ".htm", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "position2")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string bProv = string.Empty, bCity = string.Empty, bArea = string.Empty, bPrjno = string.Empty,
                                   bPrjname = string.Empty, bBidresultendtime = string.Empty,
                                   bBaseprice = string.Empty, bBiddate = string.Empty, bBuildunit = string.Empty, bBidmethod = string.Empty, bRemark = string.Empty, bInfourl = string.Empty;

                        bPrjname = nodeList[j].GetATagValue("title");
                        if (bPrjname.Contains("广东省"))
                        {
                            bCity = "广州市区";
                            bPrjname = bPrjname.Replace("[", "").Replace("]-", "").Replace("]", "").Replace("广东省", "");
                        }
                        else
                        {
                            string temp = bPrjname.Replace("[", "kdxx").Replace("]", "xxdk").GetRegexBegEnd("kdxx", "xxdk");
                            bPrjname = bPrjname.Replace("[", "").Replace("]-", "").Replace("]", "").Replace(temp, "");
                            bCity = temp + "区";
                        }
                        bInfourl = "http://www.gdzbtb.gov.cn/pbbgbd/" + nodeList[j].GetATagHref().Replace("../", "").Replace("./", "");
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(bInfourl, Encoding.Default);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellSpacing", "1")));

                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            string htmlTxt = dtlNode.AsHtml();
                            bBiddate = htmlTxt.GetDateRegex();
                            if (string.IsNullOrEmpty(bBiddate))
                                bBiddate = DateTime.Now.ToString("yyyy-MM-dd");

                            string attachUrl = string.Empty;
                            int len1 = 0, len2 = 0;
                            len1 = htldtl.IndexOf("$(\"#pbbg_shongti\")");
                            len2 = htldtl.IndexOf("</a>");
                            string aurl = string.Empty;
                            string attachName = string.Empty;
                            if (len1 > 0 && len2 > 0)
                            {
                                aurl = htldtl.Substring(len1, len2 - len1) + "</a>";
                                parser = new Parser(new Lexer(aurl));
                                NodeList atagNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (atagNode != null && atagNode.Count > 0)
                                {
                                    ATag aTag = atagNode.GetATag();
                                    attachUrl = aTag.Link;
                                    attachName = aTag.LinkText;
                                }
                            }

                            if (string.IsNullOrEmpty(attachName))
                                attachName = bPrjname;
                            BidProject info = ToolDb.GenResultProject("广东省", bCity, "", bPrjno, bPrjname, bBidresultendtime, bBaseprice, bBiddate, bBuildunit, bBidmethod, bRemark, bInfourl);
                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return list;
                            }
                            sqlCount++;
                            string sql = string.Format("select Id from BidProject where 1=1 and InfoUrl='{0}'", info.InfoUrl);
                            string result = Convert.ToString(ToolDb.ExecuteScalar(sql));
                            if (!string.IsNullOrEmpty(result))
                            {
                                if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                                {
                                    if (!string.IsNullOrEmpty(attachUrl))
                                    {
                                        string fileUrl = string.Empty;
                                        try
                                        {
                                            fileUrl = DateTime.Parse(bBiddate).ToString("yyyyMM");
                                        }
                                        catch { fileUrl = DateTime.Now.ToString("yyyyMM"); }
                                        string alink = "http://www.gdzbtb.gov.cn/pbbgbd/" + fileUrl + "/" + attachUrl.Replace("../", "").Replace("./", "");
                                        BaseAttach attach = null;
                                        try
                                        {
                                            attach = ToolHtml.GetBaseAttach(alink, attachName, info.Id, "SiteManage\\Files\\Attach\\");
                                            if (attach == null)
                                            {
                                                attach = ToolHtml.GetBaseAttachByUrl(alink, attachName, info.Id, "SiteManage\\Files\\Attach\\");
                                            }
                                        }
                                        catch { }
                                        if (attach != null)
                                        {
                                            string sqlDelete = string.Format("delete from BaseAttach where SourceId='{0}'", result);
                                            ToolDb.ExecuteSql(sqlDelete);
                                            ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                                {
                                    if (!string.IsNullOrEmpty(attachUrl))
                                    {
                                        string fileUrl = string.Empty;
                                        try
                                        {
                                            fileUrl = DateTime.Parse(bBiddate).ToString("yyyyMM");
                                        }
                                        catch { fileUrl = DateTime.Now.ToString("yyyyMM"); }
                                        string alink = "http://www.gdzbtb.gov.cn/pbbgbd/" + fileUrl + "/" + attachUrl.Replace("../", "").Replace("./", "");
                                        BaseAttach attach = null;
                                        try
                                        {
                                            attach = ToolHtml.GetBaseAttach(alink, attachName, info.Id, "SiteManage\\Files\\Attach\\");
                                            if (attach == null)
                                            {
                                                attach = ToolHtml.GetBaseAttachByUrl(alink, attachName, info.Id, "SiteManage\\Files\\Attach\\");
                                            }
                                        }
                                        catch { }
                                        if (attach != null)
                                        {
                                            ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                                        }
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
