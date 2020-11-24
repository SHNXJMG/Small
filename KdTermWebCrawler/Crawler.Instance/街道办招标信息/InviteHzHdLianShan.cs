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
    public class InviteHzHdLianShan : WebSiteCrawller
    {
        public InviteHzHdLianShan()
            : base()
        {
            this.Group = "街道办招标信息";
            this.Title = "广东省惠州市惠东县稔山镇人民政府招标、中标公告";
            this.Description = "自动抓取广东省惠州市惠东县稔山镇人民政府招标、中标公告";
            this.PlanTime = "9:18";
            this.SiteUrl = "http://renshan.huidong.gov.cn/bigClassdeta.asp?typeid=71&BigClassid=311";
            this.MaxCount = 120;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "0h120")), true), new TagNameFilter("a")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[sNode.Count - 2].GetATagValue("title");
                    pageInt = Convert.ToInt32(temp.GetReplace("第,页"));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&page=" + i, Encoding.Default);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList viewList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "0h120")));
                if (viewList != null && viewList.Count > 0)
                {
                    for (int j = 0; j < viewList.Count; j++)
                    {
                        TableTag table = viewList[j] as TableTag;
                        string prjName = string.Empty, InfoUrl = string.Empty, beginDate = string.Empty, HtmlTxt = string.Empty;
                        ATag aTag = viewList[j].GetATag();
                        if (aTag == null) continue;
                        prjName = aTag.GetAttribute("title");
                        beginDate = table.ToNodePlainString().GetDateRegex();
                        InfoUrl = "http://renshan.huidong.gov.cn/" + aTag.Link;
                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlDtl));
                        NodeList dtl = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "fontzoom")));
                        if (dtl != null && dtl.Count > 0)
                        {
                            HtmlTxt = dtl.AsHtml();

                            if (prjName.Contains("中标") || prjName.Contains("成交") || prjName.Contains("结果"))
                            {
                                string buildUnit = string.Empty, bidUnit = string.Empty,
                          bidMoney = string.Empty, code = string.Empty,
                          bidDate = string.Empty,
                          endDate = string.Empty, bidType = string.Empty,
                          specType = string.Empty,
                          msgType = string.Empty, bidCtx = string.Empty,
                          prjAddress = string.Empty, remark = string.Empty,
                          prjMgr = string.Empty, otherType = string.Empty;
                                bidCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();

                            
                                code = bidCtx.GetCodeRegex().GetCodeDel();
                                buildUnit = bidCtx.GetBuildRegex();
                                if (buildUnit.Contains("招标代理"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";

                                bidUnit = bidCtx.GetBidRegex();
                                if (string.IsNullOrEmpty(bidUnit))
                                    bidUnit = bidCtx.GetRegex("中标候选公司,中标候选人");
                                bidMoney = bidCtx.GetMoneyRegex();
                                try
                                {
                                    if (decimal.Parse(bidMoney) > 100000)
                                        bidMoney = (decimal.Parse(bidMoney) / 10000).ToString();
                                }
                                catch { }
                                Parser imgParser = new Parser(new Lexer(HtmlTxt.ToLower()));
                                NodeList imgNode = imgParser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                                string src = string.Empty;
                                if (imgNode != null && imgNode.Count > 0)
                                {
                                    string imgUrl = (imgNode[0] as ImageTag).GetAttribute("src");
                                    src = "http://renshan.huidong.gov.cn/" + imgUrl;
                                    HtmlTxt = HtmlTxt.ToLower().GetReplace(imgUrl, src);
                                }
                                msgType = "惠东县稔山镇人民政府";
                                specType = "政府采购";
                                bidType = prjName.GetInviteBidType();
                                BidInfo info = ToolDb.GenBidInfo("广东省", "惠州市区", "惠东县", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                                      bidMoney, InfoUrl, prjMgr, HtmlTxt);
                                list.Add(info);
                                if (!string.IsNullOrEmpty(src))
                                {
                                    string sql = string.Format("select Id from BidInfo where InfoUrl='{0}'", info.InfoUrl);
                                    object obj = ToolDb.ExecuteScalar(sql);
                                    if (obj == null || obj.ToString() == "")
                                    {
                                        try
                                        {
                                            BaseAttach attach = ToolHtml.GetBaseAttach(src, prjName, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                            if (attach != null)
                                                ToolDb.SaveEntity(attach, "");
                                        }
                                        catch { }
                                    }
                                }
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag a = aNode[k].GetATag();
                                        if (a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = a.Link;
                                            else
                                                link = "http://renshan.huidong.gov.cn/" + a.Link;
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }
                                if (!crawlAll && list.Count >= this.MaxCount) return list;
                            }
                            else
                            {
                                string code = string.Empty, buildUnit = string.Empty,
                       prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                       specType = string.Empty, endDate = string.Empty,
                       remark = string.Empty, inviteCon = string.Empty,
                       CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty;

                                inviteCtx = HtmlTxt.ToLower().GetReplace("</p>,</br>,<br>", "\r\n").ToCtxString();
 

                                inviteType = prjName.GetInviteBidType();

                                code = inviteCtx.GetCodeRegex().GetCodeDel();
                                buildUnit = inviteCtx.GetBuildRegex();
                                prjAddress = inviteCtx.GetAddressRegex();
                                if (buildUnit.Contains("招标代理"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("招标代理"));
                                if (buildUnit.Contains("公司"))
                                    buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";

                                Parser imgParser = new Parser(new Lexer(HtmlTxt.ToLower()));
                                NodeList imgNode = imgParser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                                string src = string.Empty;
                                if (imgNode != null && imgNode.Count > 0)
                                {
                                    string imgUrl = (imgNode[0] as ImageTag).GetAttribute("src");
                                    src = "http://renshan.huidong.gov.cn/" + imgUrl;
                                    HtmlTxt = HtmlTxt.ToLower().GetReplace(imgUrl, src);
                                }
                                msgType = "惠东县稔山镇人民政府";

                                specType = "政府采购";

                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "惠州市区", "惠东县", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                                list.Add(info);
                                if (!string.IsNullOrEmpty(src))
                                {
                                    string sql = string.Format("select Id from InviteInfo where InfoUrl='{0}'", info.InfoUrl);
                                    object obj = ToolDb.ExecuteScalar(sql);
                                    if (obj == null || obj.ToString() == "")
                                    {
                                        try
                                        {
                                            BaseAttach attach = ToolHtml.GetBaseAttach(src, prjName, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                            if (attach != null)
                                                ToolDb.SaveEntity(attach, "");
                                        }
                                        catch { }
                                    }
                                }
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                                if (aNode != null && aNode.Count > 0)
                                {
                                    for (int k = 0; k < aNode.Count; k++)
                                    {
                                        ATag a = aNode[k].GetATag();
                                        if (a.IsAtagAttach())
                                        {
                                            string link = string.Empty;
                                            if (a.Link.ToLower().Contains("http"))
                                                link = a.Link;
                                            else
                                                link = "http://renshan.huidong.gov.cn/" + a.Link;
                                            BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                            base.AttachList.Add(attach);
                                        }
                                    }
                                }
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
