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
    public class InviteTaiYuanJsgc : WebSiteCrawller
    {
        public InviteTaiYuanJsgc()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "太原建设工程信息网";
            this.Description = "自动抓取太原建设工程信息网招标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.SiteUrl = "http://www.tyjzsc.com.cn/qyxx.do?method=getZhaobgglist";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default).Replace("&nbsp;", "");
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "Body_div")), true), new TagNameFilter("li")));
            if (sNode != null && sNode.Count > 0)
            {
                for (int t = 0; t < sNode.Count; t++)
                {

                    string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                    INode node = sNode[t];
                    ATag aTag = node.GetATag();
                    prjName = aTag.GetAttribute("title");
                    beginDate = node.ToPlainTextString().GetDateRegex();
                    InfoUrl = "http://www.tyjzsc.com.cn/" + aTag.Link.GetReplace("./");
                    string htmldtl = string.Empty;
                    try
                    {
                        htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl,Encoding.Default).GetJsString();
                    }
                    catch { continue; }
                    parser = new Parser(new Lexer(htmldtl));
                    NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("style", "width:650px;")));
                    if (dtlNode != null && dtlNode.Count > 0)
                    {
                        HtmlTxt = dtlNode.AsHtml();
                        inviteCtx = HtmlTxt.ToCtxString();

                        buildUnit = inviteCtx.GetBuildRegex();
                        prjAddress = inviteCtx.GetAddressRegex();
                        code = inviteCtx.GetCodeRegex();
                        msgType = "太原市建设工程交易中心";
                        specType = "建设工程";
                        inviteType = prjName.GetInviteBidType();
                        InviteInfo info = ToolDb.GenInviteInfo("山西省", "山西省及地市", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                        list.Add(info);
                        //parser = new Parser(new Lexer(HtmlTxt));
                        //NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        //if (aNode != null && aNode.Count > 0)
                        //{
                        //    for (int k = 0; k < aNode.Count; k++)
                        //    {
                        //        ATag a = aNode[k] as ATag;
                        //        if (a.IsAtagAttach())
                        //        {
                        //            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, a.Link);
                        //            base.AttachList.Add(attach);
                        //        }
                        //    }
                        //} 
                    }
                    if (!crawlAll && list.Count >= this.MaxCount) return list;
                }
            }
            return list;
        }
    }
}
