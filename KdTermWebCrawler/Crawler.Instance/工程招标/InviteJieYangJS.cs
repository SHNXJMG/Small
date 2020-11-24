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
    public class InviteJieYangJS : WebSiteCrawller
    {
        public InviteJieYangJS()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省揭阳市建设工程招标信息";
            this.Description = "自动抓取广东省揭阳市建设工程招标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.jysggzy.com/TPFront/jsgc/004001/";
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
                htl = this.ToolWebSite.GetHtmlByUrl(base.SiteUrl, Encoding.Default);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("a"), new HasAttributeFilter("class", "wb-page-default wb-page-number wb-page-family")));
            if (nodeList != null && nodeList.Count > 0)
            {
                try
                {
                    string temp = nodeList.AsString().Replace("1/","");
                    page = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    //NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                    //     "theID=","pageNo"}, new string[] { "76", i.ToString() });

                    try
                    {
                        //htl = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                        htl = this.ToolWebSite.GetHtmlByUrl("http://www.jysggzy.com/TPFront/jsgc/004001/?pageing=" + i, Encoding.Default);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "ewb-data-items ewb-pt6")), true), new TagNameFilter("li")));
                if (tableNodeList != null && tableNodeList.Count > 0)
                {
                    for (int j = 0; j < tableNodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                               prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                               specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                               remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                               CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        prjName = tableNodeList[j].ToNodePlainString().Replace(" ", "");
                        beginDate = tableNodeList[j].ToPlainTextString().GetDateRegex();
                        if (!string.IsNullOrEmpty(beginDate))
                            prjName = prjName.Replace(beginDate, "");
                        InfoUrl = "http://www.jysggzy.com/" + tableNodeList[j].GetATagHref();
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "TDContent")));

                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            if (buildUnit.Contains("代理"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("代理"));
                            code = inviteCtx.GetCodeRegex();
                            if (buildUnit.Contains("联系电话"))
                                buildUnit = string.Empty;
                            msgType = "揭阳市建设工程交易中心";
                            inviteType = prjName.GetInviteBidType();
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "揭阳市区", "",
                         string.Empty, code, prjName, prjAddress, buildUnit,
                         beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            //parser = new Parser(new Lexer(HtmlTxt));
                            //NodeList fileNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            //if (fileNode != null && fileNode.Count > 0)
                            //{
                            //    for (int k = 0; k < fileNode.Count; k++)
                            //    {
                            //        ATag fileAtag = fileNode[k].GetATag();
                            //        if (fileAtag.IsAtagAttach())
                            //        {
                            //            try
                            //            {
                            //                string link = string.Empty;
                            //                if (fileAtag.Link.Contains("gdgpo"))
                            //                    link = fileAtag.Link;
                            //                else
                            //                    link = "http://jieyang.gdgpo.com" + fileAtag.Link;
                            //                BaseAttach attach = ToolDb.GenBaseAttach(fileAtag.LinkText, info.Id, link);
                            //                base.AttachList.Add(attach);
                            //            }
                            //            catch { }
                            //        }
                            //    }
                            //}
                            if (!crawlAll && list.Count >= this.MaxCount)
                            {
                                return list;
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
