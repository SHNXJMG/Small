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
    public class InviteSzShenDa : WebSiteCrawller
    {
        public InviteSzShenDa()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "深圳大学招标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳大学招标信息";
            this.SiteUrl = "http://bidding.szu.edu.cn/list.asp?ftype=%D5%D0%B1%EA%B9%AB%B8%E6";
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
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default, ref cookiestr);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("align", "right")));
            Regex regexPage = new Regex(@"共\d+页");
            try
            {
                page = Convert.ToInt32(regexPage.Match(nodeList[0].ToPlainTextString()).Value.Replace("共", "").Replace("页", "").Trim());
            }
            catch (Exception)
            { }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl("http://bidding.szu.edu.cn/list.asp?page=" + i.ToString(), Encoding.Default);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("style", "border-collapse: collapse")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    TableTag table = (TableTag)tableNodeList[0];
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                  prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                  specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                  remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                  CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, img = string.Empty,
                                  HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];

                        string m = tr.ChildrenHTML.ToString();
                        prjName = tr.Columns[0].ToPlainTextString().Trim().Replace("·", "");
                        beginDate = tr.Columns[0].ToPlainTextString().GetDateRegex();
                        ATag aTag = tr.Columns[0].SearchFor(typeof(ATag), true)[1] as ATag;
                        if (prjName.Contains("）") && prjName.Contains("（"))
                        {
                            int leng = prjName.IndexOf("（");
                            code = prjName.Replace("（", "kdxx").Replace("）", "xxdk").GetRegexBegEnd("kdxx", "xxdk");
                            prjName = prjName.Remove(leng);
                            string l = prjName.GetRegexBegEnd("&nbsp;", "&nbsp;");
                            code = prjName.GetRegexBegEnd("招标公告", "&nbsp;");
                            prjName = prjName.Replace(l, "").Replace("&nbsp;", "");
                        }
                        else if (prjName.Contains(")") && prjName.Contains("("))
                        {
                            int leng = prjName.IndexOf("（");
                            code = prjName.Replace("（", "kdxx").Replace("）", "xxdk").GetRegexBegEnd("kdxx", "xxdk");
                            string l = prjName.GetRegexBegEnd("&nbsp;", "&nbsp;");
                            code = prjName.GetRegexBegEnd("招标公告", "&nbsp;");
                            prjName = prjName.Replace(l, "").Replace("&nbsp;", "");
                        }
                        InfoUrl = "http://bidding.szu.edu.cn/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellspacing", "0")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            inviteCtx = HtmlTxt.Replace("<li>", "\r\n").Replace("</li>", "\r\n").ToCtxString().Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n");
                            if (string.IsNullOrEmpty(code))
                                code = inviteCtx.GetCodeRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = inviteCtx.GetRegex("招标机构名称");

                            prjAddress = inviteCtx.GetAddressRegex();
                            if (string.IsNullOrEmpty(prjAddress))
                                prjAddress = inviteCtx.GetRegexBegEnd("开标室","。");
                            msgType = "深圳大学";
                            if (inviteType == "设备材料" || inviteType == "小型施工" || inviteType == "专业分包" || inviteType == "劳务分包" || inviteType == "服务" || inviteType == "勘察" || inviteType == "设计" || inviteType == "监理" || inviteType == "施工")
                            {
                                specType = "建设工程";
                            }
                            else
                            {
                                specType = "其他";
                            }
                            if (prjAddress == "")
                            {
                                prjAddress = "见招标信息";
                            }
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "",
                             string.Empty, code, prjName, prjAddress, buildUnit,
                             beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag aTag1 = aNode[a] as ATag;
                                    if (aTag1.IsAtagAttach())
                                    {
                                        string fileUrl = string.Empty; 
                                        if (aTag1.Link.Contains("http"))
                                            fileUrl = aTag1.Link;
                                        else
                                            fileUrl =ToolWeb.UrlEncode( "http://bidding.szu.edu.cn/" + aTag1.Link);// System.Web.HttpUtility.UrlEncode( aTag1.Link);
                                        
                                    }
                                    
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return list;
        }
    }
}
