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
    public class InviteCZChaoAn : WebSiteCrawller
    {
        public InviteCZChaoAn()
            : base()
        {
            this.Group = "区县招标信息";
            this.Title = "广东省潮州市潮安县住房和建设局";
            this.Description = "自动抓取广东省潮州市潮安县住房和建设局";
            this.PlanTime = "9:38,10:40,14:38,16:40";
            this.SiteUrl = "http://www.cajsw.gov.cn/bigclassdeta.asp?typeid=32&bigclassid=131";
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
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("vAlign", "middle")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString();
                    Regex reg = new Regex(@"/[^页]+页");
                    string page = reg.Match(temp).Value.Replace("/", "").Replace("页", "");
                    pageInt = Convert.ToInt32(page);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl) + "&page=" + i.ToString(), Encoding.Default);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("width", "100%"))), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                        prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                        specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                        remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                        CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        
                        TableTag table = nodeList[j] as TableTag;
                        TableRow tr = table.Rows[0];

                        prjName = tr.Columns[0].ToNodePlainString();
                        beginDate = tr.Columns[1].ToPlainTextString();
                        InfoUrl = "http://www.cajsw.gov.cn/" + tr.Columns[0].GetATagHref(2);
                          string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                            htlDtl = htlDtl.GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "fontzoom")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            HtmlTxt = dtlList.ToHtml();
                            inviteCtx = HtmlTxt.ToCtxString();

                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("电话"))
                            {
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("电话")).Replace("联系", "").Replace("电话", "");
                            }

                            code = inviteCtx.GetCodeRegex();
                            prjAddress = inviteCtx.GetAddressRegex();

                            msgType = "潮州市潮安县住房和城乡建设局";
                            specType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "潮州市区", "潮安县", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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