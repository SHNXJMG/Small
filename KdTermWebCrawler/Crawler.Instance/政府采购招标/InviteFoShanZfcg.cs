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
    public class InviteFoShanZfcg : WebSiteCrawller
    {
        public InviteFoShanZfcg()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "广东省佛山市政府采购招标信息";
            this.Description = "自动抓取广东省佛山市政府采购招标信息";
            this.SiteUrl = "http://www.fsggzy.cn/gcjy/gc_zbxx/gc_zbsz/";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
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
                    _dicSiteUrl.Add("市直", "http://www.fsggzy.cn/zfcg/zf_cggsgg/zf_ggsz/");
                    _dicSiteUrl.Add("禅城", "http://www.fsggzy.cn/zfcg/zf_cggsgg/zcgg_gq/zcgg_cc/");
                    _dicSiteUrl.Add("南海", "http://www.fsggzy.cn/zfcg/zf_cggsgg/zcgg_gq/zcgg_nh/");
                    _dicSiteUrl.Add("三水", "http://www.fsggzy.cn/zfcg/zf_cggsgg/zcgg_gq/zcgg_ss/");
                    _dicSiteUrl.Add("高明", "http://www.fsggzy.cn/zfcg/zf_cggsgg/zcgg_gq/zcgg_gm/");
                    _dicSiteUrl.Add("顺德", "http://www.fsggzy.cn/zfcg/zf_cggsgg/zcgg_gq/zcgg_sd/");
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
                NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "page")));
                if (sNode != null && sNode.Count > 0)
                {
                    try
                    {
                        string page = sNode.AsString().ToNodeString().Replace("createPageHTML(", "");
                        string temp = page.Remove(page.IndexOf(","));
                        pageInt = Convert.ToInt32(temp);
                    }
                    catch (Exception) { }
                }

                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        try
                        {
                            html = this.ToolWebSite.GetHtmlByUrl(this.DicSiteUrl[area] + "index_" + (i - 1) + ".html".ToString(), Encoding.UTF8);
                        }
                        catch { continue; }
                    }
                    parser = new Parser(new Lexer(html));
                    sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "secondrightlistbox"))), new TagNameFilter("ul"))), new TagNameFilter("li")));
                    if (sNode != null && sNode.Count > 0)
                    {
                        for (int t = 0; t < sNode.Count; t++)
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                            beginDate = sNode[t].ToNodePlainString().GetDateRegex();
                            prjName = sNode[t].GetATagValue("title");

                            InfoUrl = this.DicSiteUrl[area] + sNode[t].GetATagHref().Replace("./", "");

                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                                Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                                NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "contentrightlistbox2")));
                                HtmlTxt = dtnodeHTML.AsHtml();
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n").Replace("<br/>", "\r\n");
                            }
                            catch (Exception ex) { continue; }
                            Parser dtlparser = new Parser(new Lexer(htmldetail));
                            NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "contentrightlistbox2")));

                            Regex regexCtx = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                            inviteCtx = regexCtx.Replace(dtnode.AsString(), "").Replace(" ", "");
                            Regex regPrjAdd = new Regex(@"(工程地点|工程地址|项目地址)[：|:][^\r\n]+[\r\n]{1}");
                            prjAddress = regPrjAdd.Match(inviteCtx).Value.Replace("工程地点", "").Replace("工程地址", "").Replace("项目地址", "").Replace("：", "").Replace(":", "").Replace(")", "").Trim();

                            Regex regbuildUnit = new Regex(@"(招标单位|招标人|采购人)：[^\r\n]+[\r\n]{1}");
                            buildUnit = regbuildUnit.Match(inviteCtx).Value.Replace("招标单位：", "").Replace("招标人：", "").Replace("采购人：", "").Trim();
                            if (buildUnit.Contains("招标代理机构"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理机构"));
                            if (buildUnit.Contains("联系人"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系人"));
                            if (buildUnit.Contains("；"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("；"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            msgType = "佛山市建设工程交易中心";
                            specType = "政府采购";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            string are = area != "市直" ? area : "";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "佛山市区", are, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                            link = a.Link;
                                        else
                                            link = "http://www.fsggzy.cn/" + a.Link.GetReplace("../,./");
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
