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
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteHbRmzf : WebSiteCrawller
    {
        public InviteHbRmzf()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "湖北省人民政府办公厅信息招标公告";
            this.Description = "自动抓取湖北省人民政府办公厅信息招标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://gcjs.cnhubei.com/dict/420000/18/";
            this.MaxCount = 120;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 409;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl);
            }
            catch { return list; }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + i);
                    }
                    catch { continue; }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "msgTable")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty, area = string.Empty;

                        TableRow tr = table.Rows[j];

                        ATag aTag = tr.GetATag();
                        area = tr.Columns[2].ToNodePlainString();
                        if (area.Contains("市"))
                        {
                            area = area.Remove(area.IndexOf("市")) + "市";
                        }
                        else if (area.Contains("区"))
                        {
                            area = area.Remove(area.IndexOf("区")) + "区";
                        }
                        else if (area.Contains("县"))
                        {
                            area = area.Remove(area.IndexOf("县")) + "县";
                        }
                        else if (area.Contains("镇"))
                        {
                            area = area.Remove(area.IndexOf("镇")) + "镇";
                        }
                        else if (area.Contains("州"))
                        {
                            area = area.Remove(area.IndexOf("州")) + "州";
                        }
                        beginDate = tr.Columns[3].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://gcjs.cnhubei.com" + aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellpadding", "3")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            inviteCtx = string.Empty;
                            HtmlTxt = dtlNode.AsHtml();
                            TableTag dtlTable = dtlNode[0] as TableTag;
                            for (int r = 0; r < dtlTable.RowCount; r++)
                            {
                                for (int c = 0; c < dtlTable.Rows[r].ColumnCount; c++)
                                {
                                    string temp = string.Empty;
                                    if (r + 1 == dtlTable.RowCount && c % 2 != 0)
                                        temp = dtlTable.Rows[r].Columns[c].ToHtml().GetReplace("<br>,</p>,</br>", "\r\n").ToCtxString();
                                    else
                                        temp = dtlTable.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                    if (c % 2 == 0)
                                        inviteCtx += temp + "：";
                                    else
                                        inviteCtx += temp + "\r\n";
                                }
                            }
                            prjName = inviteCtx.GetRegex("项目名称,工程名称");
                            code = inviteCtx.GetRegex("项目编码");
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                            msgType = "湖北省人民政府办公厅";
                            specType = "建设工程";
                            inviteType = prjName.GetInviteBidType();
                            InviteInfo info = ToolDb.GenInviteInfo("湖北省", "湖北省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = "http://gcjs.cnhubei.com/" + a.Link;
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
