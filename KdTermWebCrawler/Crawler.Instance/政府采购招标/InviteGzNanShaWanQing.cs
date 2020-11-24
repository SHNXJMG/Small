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
using System.Threading;

namespace Crawler.Instance
{
    public class InviteGzNanShaWanQing : WebSiteCrawller
    {
        public InviteGzNanShaWanQing()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "广东省广州市南沙区万顷沙镇人民政府招标公告";
            this.Description = "自动抓取广东省广州市南沙区万顷沙镇人民政府招标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://www.gzwqs.gov.cn/special_topic/wqsggzyjyxx/ggzyjyxx/";
            this.MaxCount = 80;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 18;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "index_" + (i - 1).ToString() + ".html", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                Parser parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "gllist")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = listNode[j].GetATag();
                        prjName = aTag.GetAttribute("title");
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();

                        InfoUrl = this.SiteUrl + aTag.Link.GetReplace("../,./");
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "nr")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();//.Replace("<br", "\r\n<br");
                            inviteCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("招标代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";

                            prjAddress = inviteCtx.GetAddressRegex();
                            inviteType = prjName.GetInviteBidType();
                            msgType = "广州市南沙区万顷沙镇人民政府";
                            specType = "政府采购";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "广州政府采购", "南沙区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList fileNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (fileNode != null && fileNode.Count > 0)
                            {
                                for (int k = 0; k < fileNode.Count; k++)
                                {
                                    ATag fileAtag = fileNode[k].GetATag();
                                    if (fileAtag.IsAtagAttach())
                                    {
                                        string fileName = fileAtag.LinkText.ToNodeString().Replace(" ", "");
                                        string fileLink = fileAtag.Link;

                                        if (!fileLink.ToLower().Contains("http"))
                                        {
                                            int length = InfoUrl.LastIndexOf("/");
                                            if (length > 0)
                                            {
                                                fileLink = InfoUrl.Remove(length) +"/"+ fileAtag.Link.GetReplace("../,./");
                                            }

                                        }
                                        base.AttachList.Add(ToolDb.GenBaseAttach(fileName, info.Id, fileLink));
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
