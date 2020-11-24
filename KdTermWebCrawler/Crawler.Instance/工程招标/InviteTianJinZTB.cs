using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class InviteTianJinZTB : WebSiteCrawller
    {
        public InviteTianJinZTB()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "天津市招投标网";
            this.Description = "自动抓取天津市招投标网";
            this.ExistCompareFields = "InfoUrl";
            this.MaxCount = 1000;
            this.PlanTime = "8:54,9:44,10:34,11:34,13:44,15:04,16:34";
            this.SiteUrl = "http://www.tjztb.gov.cn/zbgg/";
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("工程招标", "gczb/");
            dic.Add("货物招标", "hwzb/");
            dic.Add("服务招标", "fwzb/");
            foreach (string key in dic.Keys)
            {
                int pageInt = 1, listCount = 0;
                string html = string.Empty;
                string viewState = string.Empty;
                string eventValidation = string.Empty;
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + dic[key]);
                }
                catch { continue; }
                Parser parser = new Parser(new Lexer(html));
                NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_page_text")));
                if (pageNode != null && pageNode.Count > 0)
                {
                    string temp = pageNode.AsString();
                    try
                    {
                        pageInt = int.Parse(temp.GetRegexBegEnd("HTML", ",").Replace("(",""));
                    }
                    catch { }
                }
                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + dic[key] + "index_" + (i - 1).ToString() + ".shtml");
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_1_right_list")), true), new TagNameFilter("ul")));
                    if (listNode != null && listNode.Count > 0)
                    {
                        for (int j = 0; j < listNode.Count; j++)
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
              prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
              specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
              remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
              CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                            ATag aTag = listNode[j].GetATag();
                            prjName = aTag.LinkText;
                            InfoUrl = this.SiteUrl + dic[key] + aTag.Link.Replace("../", "").Replace("./", "").Replace(" ","");
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "text")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.AsHtml();
                                inviteCtx = HtmlTxt.ToCtxString();
                                buildUnit = inviteCtx.GetBuildRegex();
                                prjAddress = inviteCtx.GetAddressRegex();
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList namNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "text_bt")));
                                if (namNode != null && namNode.Count > 0)
                                {
                                    string temp = prjName;
                                    prjName = namNode[0].ToNodePlainString();
                                    if (!prjName.Contains(temp.Replace(".", "")))
                                        prjName = temp;
                                }
                                inviteType = key;
                                specType = "建设工程";
                                msgType = "天津市发展和改革委员会";
                                InviteInfo info = ToolDb.GenInviteInfo("天津市", "天津市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                listCount++;
                                if (!crawlAll && listCount >= this.MaxCount) goto type;
                            }
                        }
                    }
                }
            type: continue;
            }
            return list;
        }
    }
}
