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
    public class InviteFoshanSS : WebSiteCrawller
    {
        public InviteFoshanSS()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省佛山市三水区";
            this.Description = "自动抓取广东省佛山市三水区招标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.SiteUrl = "http://ssggzy.ss.gov.cn/CgtExpandFront/tender/list.do?cid=1&type=0";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        { 
            IList list = new List<InviteInfo>();
            //取得页码
            int pageInt = 1; 
            string html = string.Empty;
            string viewState = string.Empty;
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
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "page"), new TagNameFilter("div")));
            if (sNode != null && sNode.Count > 0)
            {
                try
                {
                    string temp = sNode[0].ToPlainTextString().GetRegexBegEnd("/","页");
                    pageInt = int.Parse(temp);
                }
                catch {   }
            }

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + "&currentPage="+i, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"),new HasAttributeFilter("width","730")));
                if (sNode != null && sNode.Count > 0)
                {
                    TableTag table = sNode[0] as TableTag;
                    for (int t = 1; t < table.RowCount; t++)
                    {
                        TableRow tr = table.Rows[t];
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        code = tr.Columns[1].ToNodePlainString();
                        prjName = tr.Columns[2].ToNodePlainString();
                        inviteType = tr.Columns[3].ToNodePlainString();
                        beginDate = tr.Columns[4].ToPlainTextString().GetDateRegex();
                        endDate = tr.Columns[5].ToPlainTextString().GetDateRegex();
                        InfoUrl = "http://ssggzy.ss.gov.cn" + tr.Columns[2].GetATagHref();
                        if (InfoUrl.ToLower().Contains("url="))
                        {
                            InfoUrl = "http://ssggzy.ss.gov.cn" + InfoUrl.Substring(InfoUrl.ToLower().IndexOf("url=")).Replace("url=", "");
                        } 
                        string htmldetail = string.Empty;
                        try
                        { 
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString(); 
                        }
                        catch  { continue; }
                        parser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            msgType = "佛山市三水区建设工程交易中心";
                            specType = "建设工程";

                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "佛山市区", "三水区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag aTag = aNode[k] as ATag;
                                    if (aTag.IsAtagAttach())
                                    {
                                        BaseAttach attach = ToolDb.GenBaseAttach(aTag.LinkText, info.Id, aTag.Link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }  
                    }
                }
            }
            return list;


        }
    }

}