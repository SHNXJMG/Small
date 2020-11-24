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
    public class InviteDgsj : WebSiteCrawller
    {
        public InviteDgsj()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "广东省东莞市石碣镇政府采购招标信息(包含中标)";
            this.Description = "自动抓取广东省东莞市石碣镇政府采购招标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.shijie.dg.gov.cn/desktop/publish/FileView.aspx?CategoryID=24&PageID=1";
            this.MaxCount = 200;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, Encoding.UTF8);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "hei12")));
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = pageNode.AsString().GetRegexBegEnd("/","页");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{ 
                        "__ControlState",
                        "__EventCaller",
                        "__EventParam",
                        "__VIEWSTATE",
                        "username",
                        "password",
                        "domain"
                    }, new string[]{
                    "",
                    "0__Paging",
                    i.ToString(),
                    viewState,
                    "",
                    "",
                    "dg.gov.cn"
                    });
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl, nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "96%")));
                if (listNode != null && listNode.Count > 0)
                {
                    TableTag table = listNode[0] as TableTag;
                    for (int j = 1; j < table.RowCount-1; j++)
                    {
                        TableRow tr = table.Rows[j];
                        string tempPrjName = tr.Columns[1].ToNodePlainString(); 
                        if (tempPrjName.Contains("中标") || tempPrjName.Contains("结果"))
                        {
                            string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                             
                            beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                            InfoUrl =  "http://www.shijie.dg.gov.cn" + tr.Columns[1].GetATagHref();
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "96%")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode[0].ToHtml();
                                bidCtx = HtmlTxt.ToCtxString();

                                prjName = bidCtx.GetRegex("工程名称,项目名称");
                                if (string.IsNullOrEmpty(prjName))
                                    prjName = tempPrjName;
                                code = bidCtx.GetCodeRegex();
                                buildUnit = bidCtx.GetBuildRegex();
                                bidUnit = bidCtx.GetBidRegex();
                                bidMoney = bidCtx.GetMoneyRegex();
                                bidType = prjName.GetInviteBidType();
                                prjAddress = bidCtx.GetAddressRegex();
                                prjMgr = bidCtx.GetMgrRegex();

                                msgType = "东莞市石碣政府采购";
                                specType = "政府采购";

                                List<string> listImg = new List<string>();
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList imgNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                                if (imgNode != null && imgNode.Count > 0)
                                {
                                    for (int k = 0; k < imgNode.Count; k++)
                                    {
                                        ImageTag img = imgNode[k] as ImageTag;
                                        string src = img.ImageURL;
                                        if (src.Contains("qpxs.gif"))
                                            continue;
                                        string url = string.Empty;
                                        if (!src.Contains("http"))
                                            url = "http://www.shijie.dg.gov.cn" + src;
                                        else
                                            url = src;

                                        HtmlTxt = HtmlTxt.Replace(src, url);
                                        listImg.Add(url);
                                    }
                                }
                                BidInfo info = ToolDb.GenBidInfo("广东省", "东莞市区", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, HtmlTxt);
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
                                                fileLink = "http://www.shijie.dg.gov.cn" + fileAtag.Link;
                                            base.AttachList.Add(ToolDb.GenBaseAttach(fileName, info.Id, fileLink));
                                        }
                                    }
                                }
                                if (listImg != null && listImg.Count > 0)
                                {
                                    for (int d = 0; d < listImg.Count; d++)
                                    {
                                        base.AttachList.Add(ToolDb.GenBaseAttach(prjName, info.Id,listImg[d]));
                                    }
                                }
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                        }
                        else
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                                beginDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                            InfoUrl = "http://www.shijie.dg.gov.cn" + tr.Columns[1].GetATagHref();
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "96%")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode[0].ToHtml();
                                inviteCtx = HtmlTxt.ToCtxString();

                                prjName = inviteCtx.GetRegex("工程名称,项目名称");
                                if (string.IsNullOrEmpty(prjName))
                                    prjName = tempPrjName;
                                buildUnit = inviteCtx.GetBuildRegex();
                                prjAddress = inviteCtx.GetAddressRegex();
                                code = inviteCtx.GetCodeRegex();
                                inviteType = prjName.GetInviteBidType();
                                msgType = "东莞市石碣政府采购";
                                specType = "政府采购";

                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "东莞市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
                                                fileLink = "http://www.shijie.dg.gov.cn" + fileAtag.Link;
                                            base.AttachList.Add(ToolDb.GenBaseAttach(fileName, info.Id, fileLink));
                                        }
                                    }
                                }
                                list.Add(info);
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }

                        }
                    }

                     
                }
            }
            return list;
        }
    }
}
