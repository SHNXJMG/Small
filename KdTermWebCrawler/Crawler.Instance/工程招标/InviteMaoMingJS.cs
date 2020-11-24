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
using Winista.Text.HtmlParser.Data;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteMaoMingJS : WebSiteCrawller
    {
        public InviteMaoMingJS()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省茂名市建设工程招标信息";
            this.Description = "自动抓取广东省茂名市建设工程招标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "ProjectName,InfoUrl";
            this.SiteUrl = "http://jyzx.maoming.gov.cn/mmzbtb/jyxx/";
        }

        private Dictionary<string, string> _AllSiteUrl;
        protected Dictionary<string, string> AllSiteUrl
        {
            get
            {
                if (_AllSiteUrl == null)
                {
                    _AllSiteUrl = new Dictionary<string, string>();
                    _AllSiteUrl.Add("施工", "033001/033001001/033001001001/033001001001001");
                    _AllSiteUrl.Add("勘察设计", "033001/033001001/033001001001/033001001001002");
                    _AllSiteUrl.Add("监理", "033001/033001001/033001001001/033001001001003");
                    _AllSiteUrl.Add("其他", "033001/033001001/033001001001/033001001001004");
                }
                return _AllSiteUrl;
            }
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();

            foreach (string siteUrl in AllSiteUrl.Keys)
            {
                int result = 0;
                string webUrl = this.SiteUrl + AllSiteUrl[siteUrl];
                string html = string.Empty;
                string cookiestr = string.Empty;
                string viewState = string.Empty;
                int pageInt = 1;
                string eventValidation = string.Empty;
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(webUrl, Encoding.UTF8, ref cookiestr);
                }
                catch
                {
                    return list;
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "Paging")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    string temp = nodeList.AsString().GetRegexBegEnd("总页数：", "当前");
                    try
                    {
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
                            html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(webUrl + "?Paging=" + i.ToString()));
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("valign", "top")));
                    if (tableNodeList != null && tableNodeList.Count > 0)
                    {
                        TableTag table = (TableTag)tableNodeList[0];
                        for (int j = 0; j < table.RowCount - 2; j++)
                        {
                            TableRow tr = table.Rows[j];
                            ATag aTag = tr.GetATag();
                            if (aTag == null) continue;
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                   prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                   specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                   remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                   CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            prjName = aTag.GetAttribute("title");
                            InfoUrl = "http://jyzx.maoming.gov.cn" + aTag.Link;
                            beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                            }
                            catch
                            {
                                continue;
                            }
                            parser = new Parser(new Lexer(htmldetail));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.AsHtml();
                                inviteCtx = HtmlTxt.GetReplace("</p>", "\r\n").ToCtxString();

                                buildUnit = inviteCtx.GetBuildRegex();
                                prjAddress = inviteCtx.GetAddressRegex();
                                code = inviteCtx.GetCodeRegex().GetCodeDel();
                                inviteType = siteUrl;

                                msgType = "茂名市公共资源交易中心";
                                specType = "建设工程";
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "茂名市区", "",
                                    string.Empty, code, prjName, prjAddress, buildUnit,
                                    beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                result++;
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int a = 0; a < aNode.Count; a++)
                                    {
                                        ATag fileTag = aNode[a] as ATag;
                                        if (fileTag.IsAtagAttach())
                                        {
                                            string fileUrl = string.Empty;
                                            if (fileTag.Link.Contains("http"))
                                                fileUrl = fileTag.Link;
                                            else
                                                fileUrl = "http://jyzx.maoming.gov.cn/" + fileTag.Link;

                                            base.AttachList.Add(ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, fileUrl));
                                        }
                                    }
                                }
                                if (result >= this.MaxCount && !crawlAll)
                                    goto Finish;
                            }
                        }
                    }
                }
            Finish: continue;
            }
            return list;

        }
    }
}
