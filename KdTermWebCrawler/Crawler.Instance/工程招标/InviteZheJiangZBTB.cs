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
using System.Collections.Generic;

namespace Crawler.Instance
{
   public class InviteZheJiangZBTB : WebSiteCrawller
    {
        public InviteZheJiangZBTB()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "浙江省招标投标网招标公示";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取浙江省招标投标网招标信息";
            this.SiteUrl = "http://www.zjbid.cn/zjwz/template/default/GGInfo.aspx?CategoryNum=001001001";
            this.MaxCount = 200;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch { return list; }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "MoreInfoListGG_Pager")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("页数：", "当前");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] {
                         "__VIEWSTATE","__EVENTTARGET","__EVENTARGUMENT"
                    }, new string[] {
                        viewState ,"MoreInfoListGG$Pager",i.ToString()
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "MoreInfoListGG_DataGrid1")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        ATag aTag = tr.Columns[1].GetATag();
                        if (aTag == null) continue;
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                         prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                         specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                         remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                         CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                         
                        string Xian = tr.Columns[1].ToNodePlainString().GetRegexBegEnd("【","】");
                        prjName = aTag.GetAttribute("title");
                        beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://www.zjbid.cn" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        Parser parserdetail = new Parser(new Lexer(htmldtl));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "infodetail")));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            inviteCtx = HtmlTxt.GetReplace("</tr>", "\r\n").ToCtxString();
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (string.IsNullOrWhiteSpace(buildUnit))
                                buildUnit = inviteCtx.GetRegex("采购单位名称");
                            if (buildUnit.ToString().Contains("招标代理机构"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理机构"));
                            if (buildUnit.ToString().Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                            prjAddress = inviteCtx.GetAddressRegex();
                            if (string.IsNullOrWhiteSpace(prjAddress))
                                prjAddress = inviteCtx.GetRegex("投标地点,地点");
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            if (string.IsNullOrWhiteSpace(code))
                                code = inviteCtx.GetRegex("招标项目编号");
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            msgType = "浙江省招标投标办公室";
                            specType = "建设工程";
                            inviteType = ToolHtml.GetInviteTypes(prjName);

                            InviteInfo info = ToolDb.GenInviteInfo("浙江省", "浙江省及地市",Xian,
                             string.Empty, code, prjName, prjAddress, buildUnit,
                             beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);

                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k] as ATag;
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://www.zjbid.cn/" + a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
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
