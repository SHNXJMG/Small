using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using System.Text.RegularExpressions;


namespace Crawler.Instance
{    /// <summary>
    /// 内蒙古 自治区建设工程信息
    /// </summary>
    public class InviteNeimeng : WebSiteCrawller
    {
        public InviteNeimeng()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "内蒙古自治区招标信息";
            this.Description = "自动抓取内内蒙古自治区招标信息";
            this.ExistCompareFields = "InfoUrl";
            this.PlanTime = "9:40,10:30,11:40,15:18,16:40,18:40";
            this.SiteUrl = "http://www.nmgztb.com/Html/gongchengxinxi/zhaobiaogonggao/index.htm";
            this.MaxCount = 20;
        }
       

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();

            //取得页码
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            { 
                return list;
            }

            int pageInt = 1; 
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "totalpage")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    pageInt = Convert.ToInt32(pageNode[0].ToNodePlainString());
                }
                catch { }
            }
            for (int i = pageInt; i >=1; i--)
            { 
                if (i < pageInt)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.nmgztb.com/Html/gongchengxinxi/zhaobiaogonggao/index_" + (i-1) + ".htm", Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));

                NodeList sNodes = parser.ExtractAllNodesThatMatch(new  AndFilter( new TagNameFilter("table"),new HasAttributeFilter("width","100%")));
               //parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter( new TagNameFilter("div"),new HasAttributeFilter("class","lanmu_con")),true),new TagNameFilter("table")));
               
               //NodeList div = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "lanmu_con")));
               //parser = new Parser(new Lexer(div.ToHtml()));
               //NodeList table = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                 
                if (sNodes != null && sNodes.Count > 0)
                {
                    TableTag table = sNodes[0] as TableTag;
                    for (int t = 0; t < table.RowCount; t++)
                    {
                        if (table.Rows[t].ColumnCount < 2)
                            continue;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, 
                            inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, 
                            endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                            CreateTime = string.Empty, msgType = string.Empty, HtmlTxt=string.Empty;
                        
                        StringBuilder ctx = new StringBuilder();
                        TableRow tr = table.Rows[t] as TableRow;
                        NodeList nodeList = tr.SearchFor(typeof(ATag), true);
                        if (nodeList.Count > 0)
                        {
                            ATag aTag = nodeList[0] as ATag;
                            InfoUrl = "http://www.nmgztb.com"+ aTag.Link;
                            prjName = aTag.GetAttribute("title");
                            string htmldtl = string.Empty;//this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).ToLower();
                            try
                            {
                                 htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).ToLower();
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            htmldtl = regexHtml.Replace(htmldtl, "");
                            Parser parserdtl = new Parser(new Lexer(htmldtl));
                            NodeList nodesDtl = parserdtl.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "link_con_con")));
                            if (nodesDtl != null && nodesDtl.Count > 0)
                            {
                                Regex regex = new Regex(@"更新时间：\d{4}年\d{1,2}月\d{1,2}日");
                                Match math = regex.Match(nodesDtl.AsString());
                                if (math != null)
                                {
                                    beginDate = math.Value.Replace("更新时间：", "").Replace("年", "-").Replace("月", "-").Replace("日", "").Trim();
                                }
                            }
                            parserdtl.Reset();
                            nodesDtl = parserdtl.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "oo")));
                            HtmlTxt = nodesDtl.AsHtml();
                            string str = nodesDtl.AsString().Replace("&nbsp;", "").Replace(" ", "");
                            Regex regexCTX = new Regex(@"作者：[^更新时间]+更新时间：\d{4}年\d{1,2}月\d{1,2}日");
                            str = str.Replace(regexCTX.Match(str).Value,"");
                            if (str.IndexOf("上一篇：")>-1)
                            {
                                ctx.Append(str.Substring(0, str.IndexOf("上一篇：")));
                            }
                            else
                            {
                                ctx.Append(str);
                            }
                          
                            if (ctx.ToString().Contains("招标人：") || ctx.ToString().Contains("招标单位：") || ctx.ToString().Contains("招标采购单位："))
                            {
                                Regex regex = new Regex("(招标人|招标单位|招标采购单位)：[^\r\n]+[\r\n]{1}");
                                Match match = regex.Match(ctx.ToString());
                                buildUnit = match.Value.Replace("招标人：", "").Replace("招标单位：", "").Replace("招标采购单位：", "").Trim(); 
                            }
                            if (ctx.ToString().Contains("招标编号："))
                            {
                                Regex regex = new Regex("(招标编号)：[^\r\n]+[\r\n]{1}");
                                Match match = regex.Match(ctx.ToString());
                                code = match.Value.Replace("招标编号：", "").ToUpper().Trim();
                                if (code.Length>=50)
                                {
                                    code = "";
                                }
                            }
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "";
                            }
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            buildUnit = ToolHtml.GetSubString(buildUnit,150);
                            prjAddress = ToolHtml.GetAddress(prjAddress);
                            code = ToolHtml.GetSubString(code, 50);
                            InviteInfo info = ToolDb.GenInviteInfo("内蒙古自治区", "内蒙古自治区及盟市", "", string.Empty, code, prjName, "", buildUnit, beginDate, endDate, ctx.ToString(), remark, "内蒙古自治区建设工程招标投标服务中心", inviteType, "建设工程", string.Empty, InfoUrl, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount)
                                return list;
                        }

                    }
                }
            } 
            return list;
        }
    }
}
