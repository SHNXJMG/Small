
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
namespace Crawler.Instance
{
    public class InivteShangHaiMHZYJSXY : WebSiteCrawller
    {
        public InivteShangHaiMHZYJSXY()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "上海民航职业技术学院通知公告";
            this.Description = "自动抓取上海民航职业技术学院通知公告";
            this.PlanTime = "9:20,11:20,14:20,17:20";
            this.SiteUrl = "http://www.shcac.edu.cn/html/xxdt/tzgg/1.html";
            this.MaxCount = 200;
            this.ExistCompareFields = "InfoUrl";

        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookieStr = string.Empty;
            int pageInt = 1;

            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {

                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ecms_pagination")), true), new TagNameFilter("a")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    ATag atag = pageList[pageList.Count - 2] as ATag;
                    string temp = atag.LinkText;
                    pageInt = int.Parse(temp);
                }
                catch
                { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl("http://www.shcac.edu.cn:80/html/xxdt/tzgg/" + i.ToString() + ".html", Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }

                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "list_main_content")), true), new TagNameFilter("ul")), true), new TagNameFilter("li")));

               
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        for (int j = 0; j < nodeList.Count; j++)
                    {
                        string btName = string.Empty, btTime = string.Empty, btUrl = string.Empty;
                            ATag aTag = nodeList[j].GetATag();
                            btName = nodeList[j].ToNodePlainString();
                            btTime = nodeList[j].ToNodePlainString().GetDateRegex();
                            btName = btName.Replace(btTime, "");
                            btUrl = aTag.Link;
                            string htldtl = string.Empty;
                            try
                            {
                                htldtl = this.ToolWebSite.GetHtmlByUrl(btUrl, Encoding.UTF8);
                                htldtl = htldtl.GetJsString();
                            }
                            catch
                            {
                                continue;
                            }
                            parser = new Parser(new Lexer(htldtl));
                      
                        NodeList dtlBt = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "detail_main_content")), true), new TagNameFilter("h3")));
                        if (dtlBt != null && dtlBt.Count > 0)
                        {
                            btName = dtlBt.AsString();
                         
                            if (btName.Contains("招标公告")||btName.Contains("补充公告"))
                            {
                                string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                              prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                              specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                              remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                              CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                                parser.Reset();
                                NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("style", "line-height:22px;")));
                                if (dtlList != null && dtlList.Count > 0)
                                {
                                    prjName = btName;
                                    beginDate = btTime;
                                    InfoUrl = btUrl;
                                    HtmlTxt = dtlList.ToHtml();
                                    inviteCtx = dtlList.ToHtml().Replace("</p>", "\r\n").ToCtxString().Replace("\r\n\t", "\r\n").Replace("\r\n\r\n", "\r\n");
                                    buildUnit = inviteCtx.GetBuildRegex();
                                    prjAddress = inviteCtx.GetAddressRegex();
                                    msgType = "上海民航职业技术学院";
                                    specType = "";
                                    InviteInfo info = ToolDb.GenInviteInfo("上海市", "上海市区", string.Empty, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                    list.Add(info);



                                    parser = new Parser(new Lexer(HtmlTxt));
                                    NodeList aNodes = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                    if (aNodes != null && aNodes.Count > 0)
                                    {
                                        for (int a = 0; a < aNodes.Count; a++)
                                        {
                                            ATag aFile = aNodes[a] as ATag;
                                            if (aFile.IsAtagAttach())
                                            {
                                                string link = string.Empty;
                                                if (aFile.Link.ToLower().Contains("http"))
                                                    link = aFile.Link;
                                                else
                                                    link = aFile.Link;
                                                BaseAttach attach = ToolDb.GenBaseAttach(aFile.LinkText, info.Id, link);
                                                base.AttachList.Add(attach);
                                            }
                                        }
                                    }

                                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                                }
                            }else if (btName.Contains("中标结果") || btName.Contains("结果公示") || btName.Contains("中标公示"))
                            {
                                string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
                              bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty,
                              msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty,
                              HtmlTxt = string.Empty, area = string.Empty;
                                parser.Reset();
                                NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("style", "line-height:22px;")));
                                if (dtlList != null && dtlList.Count > 0)
                                {
                                    prjName = btName;
                                    beginDate = btTime;
                                    InfoUrl = btUrl;
                                    HtmlTxt = dtlList.ToHtml();
                                    bidCtx = dtlList.ToHtml().Replace("</p>", "\r\n").ToCtxString().Replace("\r\n\t", "\r\n").Replace("\r\n\r\n", "\r\n");
                                    buildUnit = bidCtx.GetBuildRegex();

                                    bidUnit = bidCtx.GetBidRegex();
                                    if (string.IsNullOrWhiteSpace(bidUnit))
                                        bidUnit = bidCtx.GetRegex("中标人");
                                    bidMoney = bidCtx.GetMoneyRegex();
                                    buildUnit = bidCtx.GetBuildRegex();
                                    if (string.IsNullOrWhiteSpace(buildUnit))
                                        buildUnit = bidCtx.GetRegex("招标人");
                                    code = bidCtx.GetCodeRegex().GetCodeDel();
                                    if (!string.IsNullOrWhiteSpace(code))
                                        if (code[code.Length - 1] != '号')
                                            code = "";
                                    if (bidUnit.Contains("公司"))
                                        bidUnit = bidUnit.Remove(bidUnit.IndexOf("公司")) + "公司";
                                    msgType = "上海民航职业技术学院";
                                    specType = "";
                                    bidType = ToolHtml.GetInviteTypes(prjName);

                                    BidInfo info = ToolDb.GenBidInfo("上海市", "上海市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                                BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                                base.AttachList.Add(attach);
                                            }
                                        }
                                    }
                                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                                }
                            }
                            else { continue; }

                        }
                        else { continue; }
                    }
                }                                         
            }
            return list;
        }
    }
}
