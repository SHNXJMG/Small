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
    public class InviteMeiZhouCityJS : WebSiteCrawller
    {
        public InviteMeiZhouCityJS()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省梅州市各县（市）招标信息";
            this.Description = "自动抓取广东省梅州各县（市）招标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.SiteUrl = "http://market.meizhou.gov.cn/deptWebsiteAction.do?action=secondIndex&deptId=1925&issueTypeCode=009002002&issueTypeName=各县(市)招标公告&showSubNodeflag=1";
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
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&otype=&pageNum=" + i.ToString()), Encoding.Default);
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
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, bidType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[0].ToPlainTextString().Trim();
                        beginDate = tr.Columns[1].ToPlainTextString().Replace("&nbsp; ", "").Trim().Substring(0, 10);
                        ATag aTag = tr.Columns[0].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://market.meizhou.gov.cn/website/deptwebsite/1925/Content.jsp?issueId=15488&msgType=00&filePath=" + aTag.GetAttribute("onclick").Replace("showDeptContent('1925','", "");
                        int ii = InfoUrl.IndexOf("'");
                        string oo = InfoUrl.Remove(ii).Trim();

                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(oo), Encoding.Default).Replace("&nbsp;", "");
                            Regex regexHtml = new Regex(@"<script[^<]*</script>");
                            htmldetail = regexHtml.Replace(htmldetail, "");
                        }
                        catch (Exception)
                        {
                            Logger.Error("InviteMeiZhouCityJS");
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("P"), new HasAttributeFilter("class", "MsoNormal")));
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
                                    bidType = ToolHtml.GetInviteTypes(InvType);
                                }
                                inviteCtx += tr1 + "：" + "\r\n";
                            }
                            Regex regPrjAddr = new Regex(@"(工程地点|建设地点)：[^\r\n]+\r\n");
                            prjAddress = regPrjAddr.Match(inviteCtx).Value.Replace("工程地点：", "").Replace("建设地点", "").Replace("：", "").Replace("；", "").Trim();

                            Regex bildUnit = new Regex(@"(招标人|招标人（盖章）|招标人)：[^\r\n]+[\r\n]{1}");
                            buildUnit = bildUnit.Match(inviteCtx).Value.Replace("招  标人：", "").Replace("招标人（盖章）：", "").Replace("招标人：", "").Trim();
                            if (buildUnit != "")
                            {
                                int zz = buildUnit.IndexOf("：");
                                buildUnit = buildUnit.Remove(zz).ToString();
                            }
                            Regex regcode = new Regex(@"(招标项目编号|项目编号)(：|:)[^\r\n]+[\r\n]{1}");
                            code = regcode.Match(inviteCtx).Value.Replace("招标项目编号", "").Replace("项目编号", "").Replace("：", "").Replace(":", "").Trim();

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
                            if (Encoding.Default.GetByteCount(code) > 50)
                                code = string.Empty;
                            msgType = "梅州市建设工程交易中心";
                            specType = "建设工程";
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            inviteCtx = regexHtml.Replace(inviteCtx, "");
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "梅州市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, bidType, specType, otherType, oo, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }

                    }
                }
            }
            return null;
        }
    }
}
