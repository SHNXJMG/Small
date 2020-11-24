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
    public class InviteSzClcg : WebSiteCrawller
    {
        public InviteSzClcg()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "广东采联采购招标有限公司";
            this.Description = "自动抓取广东采联采购招标有限公司招标信息";
            this.PlanTime = "9:09,10:19,14:11,16:12";
            this.SiteUrl = "http://www.chinapsp.cn/xinxigonggao/list.php?catid=74166";
            this.MaxCount = 10000;
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pages")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    string temp = pageList.AsString().GetRegexBegEnd("/", "页");
                    pageInt = int.Parse(temp);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "&page=" + i.ToString(), Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "xxcon_main_left")), true), new TagNameFilter("li")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                    prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                    specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                    remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                    CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        ATag aTag = nodeList[j].GetATag(1);
                        prjName = aTag.LinkText;
                        InfoUrl = aTag.Link;//"http://www.chinapsp.cn/cn/info.aspx" + 
                        string htldtl = string.Empty;
                        try
                        {
                            htldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htldtl));
                        NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "content")));
                        if (dtList != null && dtList.Count > 0)
                        {

                            inviteType = prjName.GetInviteBidType();
                            HtmlTxt = dtList.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();

                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            prjAddress = inviteCtx.GetAddressRegex();
                            buildUnit = inviteCtx.GetBuildRegex();

                            msgType = "广东采联采购招标有限公司";
                            specType = "其他";
                            string city = string.Empty;
                            try
                            {
                                string temp = nodeList[j].ToPlainTextString().GetRegexBegEnd("areaid", ";").GetReplace("(,)");
                                city = Areaid(int.Parse(temp));
                            }
                            catch { }
                            if (string.IsNullOrWhiteSpace(city))
                                city = "深圳社会招标";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", city, "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int a = 0; a < aNode.Count; a++)
                                {
                                    ATag fileTag = aNode[a] as ATag;
                                    if (fileTag.IsAtagAttach() || fileTag.LinkText.Contains("招标文件"))
                                    {
                                        if (Encoding.Default.GetByteCount(fileTag.Link) > 500)
                                            continue;
                                        BaseAttach item = ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, fileTag.Link);
                                        base.AttachList.Add(new BaseAttach());
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

        protected string Areaid(int id)
        {

            switch (id)
            {
                case 231: return "广州市区";
                case 232: return "韶关市区";
                case 233: return "深圳社会招标";
                case 234: return "珠海市区";
                case 235: return "汕头市区";
                case 236: return "佛山市区";
                case 237: return "江门市区";
                case 238: return "湛江市区";
                case 239: return "茂名市区";
                case 240: return "肇庆市区";
                case 241: return "惠州市区";
                case 242: return "梅州市区";
                case 243: return "汕尾市区";
                case 244: return "河源市区";
                case 245: return "阳江市区";
                case 246: return "清远市区";
                case 247: return "东莞市区";
                case 248: return "中山市区";
                case 249: return "潮州市区";
                case 250: return "揭阳市区";
                case 251: return "云浮市区";
                default: return "广东";
            }
        }

    }
}
