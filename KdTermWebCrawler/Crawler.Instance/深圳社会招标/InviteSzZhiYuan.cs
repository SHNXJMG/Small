using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Util;

namespace Crawler.Instance
{
    public class InviteSzZhiYuan : WebSiteCrawller
    {
        public InviteSzZhiYuan()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "深职院招标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深职院招标信息";
            this.SiteUrl = "http://zhaobiao.szpt.edu.cn/article_list.asp?classid=1";
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
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("align", "right")));
            Regex regexPage = new Regex(@"\d+页");
            try
            {
                page = Convert.ToInt32(regexPage.Match(nodeList.AsString()).Value.Replace("页", "").Trim());
            }
            catch (Exception)
            { }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&page=" + i.ToString()), Encoding.Default);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("align", "center")));
                if (tableNodeList != null && tableNodeList.Count > 1)
                {
                    TableTag table = (TableTag)tableNodeList[3];
                    for (int j = 0; j < table.RowCount - 1; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                  prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                  specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                  remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                  CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, img = string.Empty,
                                  HtmlTxt = string.Empty, downUrl = string.Empty, downName = string.Empty;
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.GetATag(1);
                        prjName = aTag.LinkText;
                        if (prjName == "参加网上竞价招标供应商,敬请浏览以下网站")
                        {
                            continue;
                        }
                        beginDate = tr.Columns[1].ToPlainTextString().Trim();
                        InfoUrl = "http://zhaobiao.szpt.edu.cn/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new TagNameFilter("p"));
                        if (dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            Regex regeximg = new Regex(@"<IMG[^>]*>");//去掉图片
                            HtmlTxt = regeximg.Replace(HtmlTxt, "");
                            for (int z = 0; z < dtnode.Count; z++)
                            {
                                inviteCtx += dtnode[z].ToPlainTextString().Replace("&nbsp;", "").Trim() + "\r\n";
                            }
                            Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            inviteCtx = regexHtml.Replace(inviteCtx, "");
                            Regex regcode = new Regex(@"(项目编号|招标编号)(：|:)[^\r\n]+\r\n");
                            code = regcode.Match(inviteCtx).Value.Replace("项目编号：", "").Replace("招标编号：", "").Replace("：", "").Trim();
                            code = ToolHtml.GetSubString(code, 30);
                            Regex regprjAddress = new Regex(@"地址(：|:)[^\r\n]+\r\n");
                            prjAddress = regprjAddress.Match(inviteCtx).Value.Replace("地址：", "").Trim();
                            //Regex regBegin = new Regex(@"投标报名时间：[^\r\n]+[\r\n]{1}");
                            //string date = regBegin.Match(inviteCtx).Value.Replace("投标报名时间：", "").Replace(" ", "").Trim();
                            //Regex regDate = new Regex(@"\d{4}年\d{1,2}月\d{1,2}日");
                            //endDate = regDate.Match(date).Value.Trim();
                            Regex regBuidUnit = new Regex(@"(招标机构|委托单位)(：|:)[^\r\n]+\r\n");
                            buildUnit = regBuidUnit.Match(inviteCtx).Value.Replace("招标机构：", "").Replace("委托单位：", "").Trim();
                            if (inviteType == "设备材料" || inviteType == "小型施工" || inviteType == "专业分包" || inviteType == "劳务分包" || inviteType == "服务" || inviteType == "勘察" || inviteType == "设计" || inviteType == "监理" || inviteType == "施工")
                            {
                                specType = "建设工程";
                            }
                            else
                            {
                                specType = "其他";
                            }
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            if (prjAddress == "")
                            {
                                prjAddress = "见招标信息";
                            }
                            msgType = "深职院";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "",
                             string.Empty, code, prjName, prjAddress, buildUnit,
                             beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parserdetail.Reset();
                            parserdetail = new Parser(new Lexer(htmldetail));
                            NodeList nodedown = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new TagNameFilter("p"), true), new TagNameFilter("a")));
                            for (int k = 0; k < nodedown.Count; k++)
                            {
                                ATag aTagdown = nodedown.SearchFor(typeof(ATag), true)[k] as ATag;
                                if (aTagdown.LinkText.Contains(".doc") || aTagdown.LinkText.Contains(".dwg") || aTagdown.LinkText.Contains(".xls"))
                                {
                                    downName = aTagdown.LinkText;
                                    downUrl = "http://zhaobiao.szpt.edu.cn" + aTagdown.Link;
                                    BaseAttach attach = ToolDb.GenBaseAttach(downName, info.Id, downUrl);
                                    base.AttachList.Add(attach);
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
            }
            return null;
        }
    }
}
