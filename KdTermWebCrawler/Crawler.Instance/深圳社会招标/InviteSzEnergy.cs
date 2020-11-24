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
    public class InviteSzEnergy : WebSiteCrawller
    {
        public InviteSzEnergy()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "广东省深圳能源集团公司招标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取广东省深圳能源集团公司招标信息";
            this.SiteUrl = "http://www.sec.com.cn/Bidding_list.aspx?TypeId=69";
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
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(htl));
            NodeList ulNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "f-l")));
            if (ulNode == null || ulNode.Count < 1) return null;
            parser = new Parser(new Lexer(ulNode[0].ToHtml()));
            NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new TagNameFilter("ul"), true), new TagNameFilter("li")));
            if (tableNodeList != null && tableNodeList.Count > 0)
            {
                for (int j = 0; j < tableNodeList.Count; j++)
                {
                    string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                     prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                     specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                     remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                     CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                    ATag aTag = tableNodeList[j].GetATag();
                    prjName = aTag.LinkText.ToNodeString().Replace(" ","");
                    beginDate = prjName.GetDateRegex();
                    prjName = prjName.Replace(beginDate, "");
                    InfoUrl = "http://www.sec.com.cn/" + aTag.Link.Trim();
                    string htmldetail = string.Empty;
                    try
                    {
                        htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "");
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    Parser parserdetail = new Parser(new Lexer(htmldetail));
                    NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "g-n-con")));
                    HtmlTxt = dtnode.AsHtml();
                    inviteCtx = dtnode.AsString().Replace("\t", "").Trim();
                    if (inviteCtx.Contains("\r\n\r\n"))
                    {
                        inviteCtx = inviteCtx.Substring(inviteCtx.IndexOf("\r\n\r\n")).ToString().Replace("&amp", "").Trim();
                    }
                    inviteCtx = inviteCtx.Replace("&#61548;", "").Replace("&Oslash;", "").Trim();
                    code = inviteCtx.GetCodeRegex().Replace("能源大厦施工总承包", "").Replace("?", "");
                    buildUnit = inviteCtx.GetBuildRegex();
                    if (string.IsNullOrEmpty(buildUnit))
                        buildUnit = inviteCtx.GetRegex("招  标  人");
                    prjAddress = inviteCtx.GetAddressRegex();
                    if (string.IsNullOrEmpty(prjAddress))
                        prjAddress = inviteCtx.GetRegex("详细地址");
                    msgType = "深圳能源集团公司";
                    specType = "建设工程";
                    prjAddress = "见招标信息"; 
                    if (prjName == "宝安区老虎坑垃圾焚烧发电厂二期项目飞灰固化车间屋面墙面材料采购" && code == "")
                    {
                        code = "0708-124003ZXY205";
                    } 
                    inviteType = ToolHtml.GetInviteTypes(prjName);
                    InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "",
                             string.Empty, code, prjName, prjAddress, buildUnit,
                             beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                    list.Add(info);
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }
            return list;
        }
    }
}
