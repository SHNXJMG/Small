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
    public class InviteSzldzb : WebSiteCrawller
    {
        public InviteSzldzb()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "深圳龙达招标有限公司";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳龙达招标有限公司招标信息";
            this.SiteUrl = "http://www.szldzb.com/information.aspx?ClassID=23";
            this.MaxCount = 100;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8); 
            }
            catch (Exception ex)
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "digg")));
            if (tdNodes != null && tdNodes.Count > 0)
            { 
                string pageTemp = tdNodes.AsString().Replace("&nbsp;", "").Trim();
                Regex regpage = new Regex(@"共\d+页");
                try
                {
                    pageInt = int.Parse(regpage.Match(pageTemp).Value.Replace("共", "").Replace("页", "").Trim());
                }
                catch (Exception ex) { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    { 
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "&page=" + i.ToString(), Encoding.UTF8);
                    }
                    catch (Exception ex) { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("width", "100%"), new TagNameFilter("table"))); 
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = (TableTag)nodeList[4];
                    for (int j = 0; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        code = table.Rows[j].Columns[0].ToNodePlainString();
                        prjName = table.Rows[j].Columns[1].ToNodePlainString().Replace("(New!)", "").Replace(".", "");
                        ATag aTag = table.Rows[j].SearchFor(typeof(ATag), true)[0] as ATag;
                        InfoUrl = "http://www.szldzb.com/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").GetJsString().Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("width", "620"), new TagNameFilter("table")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                        }
                        catch (Exception ex) { continue; }


                        inviteCtx = HtmlTxt.ToCtxString().Trim().Replace("(new!)", "").Replace(" ", "").Replace("endfragment", "");

                        try
                        {
                            string ctx = inviteCtx.Substring(inviteCtx.Length - 80, 80);
                            beginDate = ctx.GetChinaTime();
                        }
                        catch { }
                        if (string.IsNullOrEmpty(beginDate))
                        {
                            beginDate = DateTime.Now.ToString();
                        }
                        buildUnit = inviteCtx.GetBuildRegex();
                        prjAddress = inviteCtx.GetAddressRegex();
                        inviteType = prjName.GetInviteBidType();
                        specType = "其他";
                        msgType = "深圳龙达招标有限公司";
                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                        list.Add(info);
                        parser = new Parser(new Lexer(HtmlTxt));
                        NodeList nodeTag = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                        if (nodeTag != null && nodeTag.Count > 0)
                        {
                            for (int k = 0; k < nodeTag.Count; k++)
                            {
                                ATag fileTag = nodeTag[k].GetATag();
                                if (fileTag.IsAtagAttach())
                                {
                                    string link = "http://www.szldzb.com/" + fileTag.Link;
                                    BaseAttach attach = ToolDb.GenBaseAttach(fileTag.LinkText, info.Id, link);
                                }
                            }
                        }
                        //NodeList FileTag = dtnode.ExtractAllNodesThatMatch(new TagNameFilter("a"), true);
                        //if (FileTag != null && FileTag.Count > 0)
                        //{
                        //    for (int f = 0; f < FileTag.Count; f++)
                        //    {
                        //        ATag file = FileTag[f] as ATag;
                        //        if (file.Link.ToUpper().Contains(".DOC"))
                        //        {
                        //            BaseAttach attach = ToolDb.GenBaseAttach(file.ToPlainTextString(), info.Id, file.Link);
                        //            base.AttachList.Add(attach);
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
            return list;
        }
    }
}
