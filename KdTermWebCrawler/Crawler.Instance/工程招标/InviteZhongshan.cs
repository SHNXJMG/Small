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
    public class InviteZhongshan : WebSiteCrawller
    {
        public InviteZhongshan()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省中山市";
            this.Description = "自动抓取广东省中山市招标信息";
            this.ExistCompareFields = "Prov,City,Area,Road,Code,ProjectName";
            this.SiteUrl = "http://p.zsjyzx.gov.cn/port/Application/NewPage/PageSubItem.jsp?node=58";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.MaxCount = 50;
        }
        Dictionary<string, string> _dicSiteUrl;
        protected Dictionary<string, string> DicSiteUrl
        {
            get
            {
                if (_dicSiteUrl == null)
                {
                    _dicSiteUrl = new Dictionary<string, string>();
                    
                    _dicSiteUrl.Add("建设工程招标公告", "http://p.zsjyzx.gov.cn/port/Application/NewPage/PageSubItem.jsp?node=58");
                    _dicSiteUrl.Add("政府采购公告", "http://p.zsjyzx.gov.cn/port/Application/NewPage/PageSubItem.jsp?node=53");


                }
                return _dicSiteUrl;
            }
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            foreach (string area in this.DicSiteUrl.Keys)
            {
                int pageInt = 1, count = 0;
                string html = string.Empty;
                string viewState = string.Empty;
                string eventValidation = string.Empty;
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(this.DicSiteUrl[area], Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    return list;
                }

                Parser parser = new Parser(new Lexer(html));
                NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("li"), new HasAttributeFilter("class", "pageintro")));
                if (sNode != null && sNode.Count > 0)
                {
                    try
                    {
                        string temp = sNode.AsString().ToCtxString().GetRegexBegEnd("页共", "页");
                        pageInt = int.Parse(temp);
                    }
                    catch (Exception) { }
                }
                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(this.DicSiteUrl[area] + "&page=" + i.ToString(), Encoding.UTF8);
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "nav_list"))), new TagNameFilter("ul"))), new TagNameFilter("li")));
                    if (sNode != null && sNode.Count > 0)
                    {
                        for (int t = 0; t < sNode.Count; t++)
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            if (area == "建设工程招标公告")
                            {
                                endDate = sNode[t].ToNodePlainString().GetDateRegex();
                            }
                            
                            prjName = sNode[t].GetATagValue("title");
                            InfoUrl = "http://p.zsjyzx.gov.cn" + sNode[t].GetATagHref();
                            string url = string.Empty,shurl = string.Empty,urls = string.Empty;
                            urls = InfoUrl + "s";
                            shurl = urls.GetRegexBegEnd("articalID=", "s");
                            url = "http://p.zsjyzx.gov.cn/port/Application/NewPage/ggnr.jsp?articalID=" + shurl;
                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8);
                                Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                                NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "details_1")));
                                HtmlTxt = dtnodeHTML.AsHtml();
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(url, Encoding.UTF8).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n").Replace("<br/>", "\r\n");
                            }
                            catch (Exception ex) { continue; }
                            Parser dtlparser = new Parser(new Lexer(htmldetail));
                            NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "details_1")));
                            HtmlTxt = dtnode.AsHtml();
                            
                            if (area == "建设工程招标公告")
                            {
                                inviteCtx = HtmlTxt.ToCtxString();
                            }
                            else { inviteCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString(); }
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("招标代理机构"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理机构"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            if (string.IsNullOrWhiteSpace(buildUnit))
                                buildUnit = inviteCtx.GetRegex("采购人");
                            prjAddress = inviteCtx.GetAddressRegex();
                            if (string.IsNullOrWhiteSpace(prjAddress))
                                prjAddress = inviteCtx.GetRegex("地址");
                            if (string.IsNullOrWhiteSpace(prjAddress))
                                prjAddress = inviteCtx.GetRegex("联系地址");
                            if (prjAddress.Contains("购买"))
                                prjAddress = prjAddress.Remove(prjAddress.IndexOf("购买"));
                          
                            if(string.IsNullOrWhiteSpace(beginDate))
                                beginDate = inviteCtx.GetRegexBegEnd("时间：", "点击");
                            code = inviteCtx.GetCodeRegex();
                            if (code.Contains("采购"))
                                code = code.Remove(code.IndexOf("采购"));
                            msgType = "中山市公共资源交易中心";
                            if (area == "建设工程招标公告")
                            {
                                specType = "建设工程";
                            }
                            else { specType = "政府采购"; }
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            string are = area != "建设工程招标公告" ? area : "";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "中山市区", are, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            count++;
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
                                        {
                                            link = a.Link;
                                        }
                                        else
                                        {
                                            string ulra = a.GetATagValue("onclick");
                                            string sht = ulra.Replace("','_black')", "").Replace("javascript:window.open('", "");
                                            link = "http://p.zsjyzx.gov.cn" + sht;
                                        }
                                        if (Encoding.Default.GetByteCount(link) > 500)
                                            continue;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && count >= this.MaxCount) goto Funcs;

                        }
                    }
                }
                Funcs:;
            }
            return list;
        }
     
    }
}
