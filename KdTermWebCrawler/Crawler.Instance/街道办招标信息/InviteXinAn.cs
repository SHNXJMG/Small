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
    public class InviteXinAn : WebSiteCrawller
    {
        public InviteXinAn()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省深圳市新安街道办事处";
            this.Description = "自动抓取广东省深圳市新安街道办事处招标信息";
            this.PlanTime = "9:17,13:51";
            this.ExistCompareFields = "InfoUrl,MsgType";
            this.SiteUrl = "http://xajdb.baoan.gov.cn/xxgk_11984/ywxx/zbcg/zbxxgs/index.html";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_page")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode.AsString().Replace("createPageHTML(", "").Replace("index", "").Replace("html", "").Replace(",0,", "").Replace(");", "").Replace(",", "").Replace(";", "").Replace(")", "").Replace("\"", "").Replace(" ", "");
                    pageInt = Convert.ToInt32(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://xajdb.baoan.gov.cn/xxgk_11984/ywxx/zbcg/zbxxgs/index_" + (i - 1).ToString() + ".html", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "right_list"))), new TagNameFilter("ul"))), new TagNameFilter("li")));
                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                        beginDate = regDate.Match(viewList[j].ToPlainTextString().Trim()).Value;
                        string temp = viewList[j].ToPlainTextString().Trim().Replace(beginDate, "");
                        try
                        {
                            int beg = temp.IndexOf("else"), end = temp.Length;
                            temp = temp.Substring(beg, end - beg);
                            beg = temp.IndexOf("<a");
                            end = temp.IndexOf("/a>");
                            temp = temp.Substring(beg, (end - beg) + 3);
                            beg = temp.IndexOf(">");
                            end = temp.IndexOf("</");
                            prjName = temp.Substring(beg + 1, end - beg - 1);
                            Parser p = new Parser(new Lexer(temp));
                            NodeList l = p.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            ATag aTag = l.SearchFor(typeof(ATag), true)[0] as ATag;
                            InfoUrl = "http://xajdb.baoan.gov.cn/xxgk_11984/ywxx/zbcg/zbxxgs/" + aTag.Link.Replace("../", "").Replace("./", "");
                        }
                        catch { continue; }

                        string htmDtl = string.Empty;
                        try
                        {
                            htmDtl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8);
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
                            htmDtl = regexHtml.Replace(htmDtl, "");
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "TRS_PreAppend")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml();
                            parser = new Parser(new Lexer(HtmlTxt.Replace("th", "td")));
                            NodeList dtlTab = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "texttable")));
                            if (dtlTab != null && dtlTab.Count > 0)
                            {
                                TableTag table = dtlTab[0] as TableTag;
                                for (int k = 0; k < table.RowCount; k++)
                                {
                                    for (int c = 0; c < table.Rows[k].ColumnCount; c++)
                                    {
                                        string strCtx = table.Rows[k].Columns[c].ToPlainTextString().Replace("&nbsp;", "").Replace(" ", "");
                                        if (strCtx == "工程类型")
                                            break;
                                        if (c % 2 == 0)
                                            inviteCtx += strCtx + "：";
                                        else
                                            inviteCtx += strCtx + "\r\n";
                                    }
                                }
                            }
                            else
                            {
                                inviteCtx = dtl.AsString().Replace("&nbsp;", "").Replace(" ", "").Replace("\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            }
                            string InvType = prjName;
                            if (InvType.Contains("施工"))
                            {
                                inviteType = "施工";
                            }
                            if (InvType.Contains("监理"))
                            {
                                inviteType = "监理";
                            }
                            if (InvType.Contains("设计"))
                            {
                                inviteType = "设计";
                            }
                            if (InvType.Contains("勘察"))
                            {
                                inviteType = "勘察";
                            }
                            if (InvType.Contains("服务"))
                            {
                                inviteType = "服务";
                            }
                            if (InvType.Contains("劳务分包"))
                            {
                                inviteType = "劳务分包";
                            }
                            if (InvType.Contains("专业分包"))
                            {
                                inviteType = "专业分包";
                            }
                            if (InvType.Contains("小型施工"))
                            {
                                inviteType = "小型工程";
                            }
                            if (InvType.Contains("设备材料"))
                            {
                                inviteType = "设备材料";
                            }
                       
                            Regex regPrjAddr = new Regex(@"(工程位置|工程地点|工程地址|地址)(:|：)[^\r\n]+\r\n");
                            prjAddress = regPrjAddr.Match(inviteCtx).Value.Replace("工程位置", "").Replace("工程地点", "").Replace("工程地址", "").Replace(":", "").Replace("：", "").Trim();
                            Regex regBuildUnit = new Regex(@"(招标单位|招标人|招标单位（盖章）|采购人)(:|：)[^\r\n]+\r\n");
                            buildUnit = regBuildUnit.Match(inviteCtx).Value.Replace("招标单位", "").Replace("招标人", "").Replace("（盖章）", "").Replace(":", "").Replace("：", "").Trim();
                            Regex regPrjCode = new Regex(@"(工程编号|项目编号|编号)(:|：)[^\r\n]+\r\n");
                            code = regPrjCode.Match(inviteCtx).Value.Replace("工程编号", "").Replace("项目编号", "").Replace("编号", "").Replace(":", "").Replace("：", "").Trim();
                            msgType = "深圳市宝安区新安街道办事处";
                            if (string.IsNullOrEmpty(prjAddress) || Encoding.Default.GetByteCount(prjAddress) > 150)
                            { prjAddress = "见招标信息"; }
                            if (string.IsNullOrWhiteSpace(prjAddress))
                                prjAddress = inviteCtx.GetAddressRegex();
                            if (string.IsNullOrWhiteSpace(buildUnit))
                                inviteCtx.GetBuildRegex();
                            if (string.IsNullOrWhiteSpace(code))
                                code = inviteCtx.GetCodeRegex();
                            if (code.Contains("）"))
                                code = code.Remove(code.IndexOf("）"));
                            if (buildUnit.Contains("采购人"))
                                buildUnit = buildUnit.Replace("采购人","");
                            code = ToolHtml.GetSubString(code, 50);
                            buildUnit = ToolHtml.GetSubString(buildUnit, 150);
                            specType = "建设工程";
                            inviteType = "小型工程"; 
                            if (string.IsNullOrEmpty(buildUnit))
                            {
                                buildUnit = "深圳市宝安区新安街道办事处";
                            }
                            inviteType = ToolHtml.GetInviteType(inviteType);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳区及街道工程", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
