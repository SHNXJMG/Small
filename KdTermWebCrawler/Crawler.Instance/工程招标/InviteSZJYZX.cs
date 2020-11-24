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
    public class InviteSZJYZX : WebSiteCrawller
    {
        public InviteSZJYZX()
            : base(true)
        {
            this.Group = "招标信息";
            this.Title = "广东省深圳市区";
            this.Description = "自动抓取广东省深圳市区招标信息";
            this.MaxCount = 20;
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,10:20,10:50,11:30,14:05,14:25,14:50,15:25,16:00,16:50,19:00";
            this.SiteUrl = "http://www.szjsjy.com.cn/BusinessInfo/ZBGGInfoList.aspx?id=100";
            this.ExistsHtlCtx = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            int sqlCount = 0;
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = ToolHtml.GetHtmlByUrlEncode(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("cellspacing", "2"), new TagNameFilter("table")));
            if (sNode != null && sNode.Count > 0)
            {
                string pageString = sNode.AsString();
                Regex regexPage = new Regex(@"，共[^页]+页，");
                Match pageMatch = regexPage.Match(pageString);
                try { pageInt = int.Parse(pageMatch.Value.Replace("，共", "").Replace("页，", "").Trim()); }
                catch (Exception) { }
            }
            string cookiestr = string.Empty;
            for (int i = 1; i <= pageInt; i++)
            { 
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "__VIEWSTATEENCRYPTED", "__EVENTVALIDATION", "ctl00$hdnPageCount" }, new string[] { "ctl00$Content$GridView1", "Page$"+i.ToString(), viewState, "",eventValidation, pageInt.ToString() });
                    html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8);
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "ctl00_Content_GridView1"), new TagNameFilter("table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty,
                            inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty,
                            endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                            CreateTime = string.Empty, msgType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j] as TableRow;
                        code = tr.Columns[1].ToPlainTextString().Trim();
                        prjName = tr.Columns[2].ToPlainTextString().Trim();
                        buildUnit = tr.Columns[3].ToPlainTextString().Trim();
                        beginDate = tr.Columns[5].ToPlainTextString().Trim();
                        endDate = tr.Columns[6].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[2].Children[0] as ATag;
                        InfoUrl = "http://www.szjsjy.com.cn/BusinessInfo/" + aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = ToolHtml.GetHtmlByUrlEncode(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                            Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                            NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "lblXXNR"), new TagNameFilter("span")));
                            HtmlTxt = dtnodeHTML.AsHtml();
                            htmldetail = ToolHtml.GetHtmlByUrlEncode(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                        }
                        catch (Exception ex) { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "lblXXNR"), new TagNameFilter("span")));

                        inviteCtx = dtnode.AsString().Replace(" ", "");
                        Regex regPrjAdd = new Regex(@"(工程地点|工程地址)：[^\r\n]+[\r\n]{1}");
                        prjAddress = regPrjAdd.Match(inviteCtx).Value.Replace("工程地点：", "").Replace("工程地址：", "").Trim();
                        msgType = "深圳市建设工程交易中心";
                        specType = "建设工程";
                        Regex regInvType = new Regex(@"[^\r\n]+[\r\n]{1}");
                        string InvType = regInvType.Match(inviteCtx).Value;

                        inviteType = ToolHtml.GetInviteTypes(InvType);
                        #region 2013-11-19修改
                        Dictionary<string, Regex> dicRegex = new Dictionary<string, Regex>();
                        dicRegex.Add("重要提示", new Regex(@"([.\S\s]*)(?=重要提示)"));
                        dicRegex.Add("温馨提示", new Regex(@"([.\S\s]*)(?=温馨提示)"));
                        foreach (string dicValue in dicRegex.Keys)
                        {
                            if (inviteCtx.Contains(dicValue))
                                inviteCtx = dicRegex[dicValue].Match(inviteCtx).Value;
                        }
                        #endregion  
                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳市工程", string.Empty, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, string.Empty, InfoUrl, HtmlTxt);
                        if (!crawlAll && sqlCount >= this.MaxCount) return null;
                        sqlCount++; 
                        if (ToolDb.SaveEntity(info, this.ExistCompareFields,this.ExistsUpdate,this.ExistsHtlCtx))
                        {
                            dtlparser.Reset();
                            NodeList dlNodes = dtlparser.ExtractAllNodesThatMatch(new TagNameFilter("a"));//  
                            if (dlNodes != null && dlNodes.Count > 0)
                            {
                                for (int f = 0; f < dlNodes.Count; f++)
                                {
                                    ATag fileTag = dlNodes[f] as ATag;
                                    if (fileTag.IsAtagAttach())
                                    {
                                        //BaseAttach attach = ToolDb.GenBaseAttach(fileTag.StringText, info.Id, fileTag.Link.Replace("..", "http://www.szjsjy.com.cn"));
                                        try
                                        {
                                            BaseAttach attach = ToolHtml.GetBaseAttach(fileTag.Link.Replace("..", "http://www.szjsjy.com.cn"), fileTag.LinkText, info.Id, "SiteManage\\Files\\InviteAttach\\");
                                            if (attach != null)
                                                ToolDb.SaveEntity(attach, "SourceID,AttachServerPath");
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                       
                    }
                }

            }
            return list;
        }
    }
}
