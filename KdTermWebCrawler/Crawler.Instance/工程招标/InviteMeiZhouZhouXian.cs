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
    public class InviteMeiZhouZhouXian : WebSiteCrawller
    {
        public InviteMeiZhouZhouXian()
            : base(true)
        {
            this.Group = "招标信息";
            this.Title = "广东省梅州市(各县市)建设工程招标信息";
            this.Description = "自动抓取广东省梅州市(各县市)建设工程招标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.MaxCount = 600;
            this.SiteUrl = "http://market.meizhou.gov.cn/deptWebsiteAction.do?action=secondIndex&deptId=1925&issueTypeCode=009002002&issueTypeName=各县(市)招标公告&showSubNodeflag=1&pageNum=1";
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
                Regex regexHtml = new Regex(@"<script[^<]*</script>");
                htl = regexHtml.Replace(htl, "");
            }
            catch (Exception ex)
            { 
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("align", "right")));
            Regex regexPage = new Regex(@"共\d+页");
            try
            {
                page = int.Parse(regexPage.Match(nodeList.AsString()).Value.Trim(new char[] { '共', '页' }));
            }
            catch (Exception)
            { }
            for (int i = 1; i < page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl("http://market.meizhou.gov.cn/deptWebsiteAction.do?action=secondIndex&deptId=1925&issueTypeCode=009002002&issueTypeName=各县(市)招标公告&showSubNodeflag=1&pageNum=" + i.ToString(), Encoding.Default);
                        Regex regexHtml = new Regex(@"<script[^<]*</script>");
                        htl = regexHtml.Replace(htl, "");
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "1")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, 
                            inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, 
                            endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                            CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[0].ToPlainTextString().Trim();
                        beginDate = ToolHtml.GetRegexDateTime(tr.Columns[1].ToPlainTextString()); 
                        string aTag = tr.Columns[0].ToHtml();
                        parser = new Parser(new Lexer(aTag));
                        NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        string aLink = aNode.ToHtml().ToLower();
                        try
                        {
                            string strValue1 = aLink.Substring(aLink.IndexOf("("), aLink.Length - aLink.IndexOf("("));
                            string strValue2 = strValue1.Remove(strValue1.IndexOf(")"));
                            string[] strValue3 = strValue2.Split(',');
                            string strValue4 = strValue3[1];
                            InfoUrl = "http://market.meizhou.gov.cn/website/deptwebsite/1925/Content.jsp?issueId=17039&msgType=00&filePath="+strValue4.Replace("'","");
                        }
                        catch
                        { continue; }
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "");
                            Regex regexHtml = new Regex(@"<script[^<]*</script>");
                            htmldetail = regexHtml.Replace(htmldetail, "");
                        }
                        catch (Exception)
                        {
                            Logger.Error("InviteMeiZhouZhouXian");
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("P"), new HasAttributeFilter("class", "MsoNormal")));
                        if (dtnode == null || dtnode.Count < 1)
                        {
                            parserdetail = new Parser(new Lexer(htmldetail));
                            dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "WordSection1")));
                        }
                        if (dtnode.Count > 0 && dtnode != null)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            for (int k = 0; k < dtnode.Count; k++)
                            {
                                string tr1 = string.Empty;
                                tr1 = dtnode[k].ToPlainTextString().Replace(" ", "").Trim();
                                if (k == 0)
                                {
                                    string InvType = tr1;
                                    inviteType = ToolHtml.GetInviteTypes(InvType);
                                }
                                inviteCtx += tr1 + "：" + "\r\n";
                            } 
                            prjAddress = ToolHtml.GetRegexString(inviteCtx, ToolHtml.AddressRegex); 
                            buildUnit = ToolHtml.GetRegexString(inviteCtx.Replace("（盖章）", ""), ToolHtml.BuildRegex); 
                            if (buildUnit != "" && buildUnit.Contains("："))
                            {
                                int zz = buildUnit.IndexOf("：");
                                buildUnit = buildUnit.Remove(zz).ToString();
                            }
                            code = ToolHtml.GetRegexString(inviteCtx,ToolHtml.CodeRegex); 
                            Regex regoType = new Regex(@"工程类型：[^\r\n]+\r\n");
                            string oType = regoType.Match(inviteCtx).Value.Replace("工程类型：", "").Trim();
                            if (oType.Contains("房建"))
                            {
                                otherType = "房建及工业民用建筑";
                            }
                            else if (oType.Contains("市政"))
                            {
                                otherType = "市政工程";
                            }
                            else if (oType.Contains("园林绿化"))
                            {
                                otherType = "园林绿化工程";
                            }
                            else if (oType.Contains("装饰") || oType.Contains("装修"))
                            {
                                otherType = "装饰装修工程";
                            }
                            else if (oType.Contains("电力"))
                            {
                                otherType = "电力工程";
                            }
                            else if (oType.Contains("水利"))
                            {
                                otherType = "水利工程";
                            }
                            if (oType.Contains("环保"))
                            {
                                otherType = "环保工程";
                            }
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            if (buildUnit.Contains("梅州市建设工程交易中心"))
                            {
                                buildUnit = "";
                            }
                            msgType = "梅州市建设工程交易中心";
                            specType = "建设工程";
                            inviteCtx = inviteCtx.Replace("<?", "").Replace("xml:namespace prefix = o ns = ", "").Replace("urn:schemas-microsoft-com:office:office", "").Replace("/>", "").Trim();
                            inviteCtx = inviteCtx.Replace("<?", "").Replace("xml:namespace prefix = ns0 ns = ", "").Replace("urn:schemas-microsoft-com:office:smarttags", "").Replace("/>", "").Trim();
                            inviteCtx = inviteCtx.Replace("<?", "").Replace("xml:namespace prefix = st1 ns = ", "").Replace("urn:schemas-microsoft-com:office:smarttags", "").Replace("/>", "").Trim();
                            inviteCtx = inviteCtx.Replace("?xml:namespaceprefix=o/>", "").Trim();
                            inviteCtx = inviteCtx.Replace("<?xml:namespaceprefix=st1/>", "").Trim();
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
