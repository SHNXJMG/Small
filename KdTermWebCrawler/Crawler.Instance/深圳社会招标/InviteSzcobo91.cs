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
    public class InviteSzcobo91 : WebSiteCrawller
    {
        public InviteSzcobo91()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "中邦国际招标&邦迪工程顾问";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取中邦国际招标&邦迪工程顾问招标信息";
            this.SiteUrl = "http://www.cobo91.com/project/bid.aspx";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default, ref cookiestr);
               
            }
            catch (Exception ex)
            {

                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "PageDataList")));
            if (tdNodes != null)
            {
                string pageTemp = tdNodes.AsString().Replace("&nbsp;", "").Replace(" ", "").Trim();
                Regex regpage = new Regex(@"共[0-9]+条");
                try
                {
                    int pageCount = int.Parse(regpage.Match(pageTemp).Value.Replace("共", "").Replace("条", "").Trim());
                    if (pageCount % 15 > 0)
                    {
                        pageInt = (pageCount / 15) + 1;
                    }
                    else
                    {
                        pageInt = pageCount / 15;
                    }
                }
                catch (Exception ex) { }
                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        viewState = this.ToolWebSite.GetAspNetViewState(html);
                        eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { 
                            "cataId",
                            "find_yn",
                            "key_word",
                            "typeId",
                            "__EVENTARGUMENT",
                            "__EVENTTARGET",
                            "__EVENTVALIDATION",
                            "__VIEWSTATE"
                        
                        }, new string[] { 
                            "1,2,3,4,5,6,7,8,",
                            string.Empty,
                            string.Empty,
                            "1,2,3,4,5,6,7,8,",
                            string.Empty,
                            "PageDataList$ctl12$LinkButton1",
                            eventValidation,
                            viewState
                        });
                        try { html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default, ref cookiestr); }
                        catch (Exception ex) { continue; }

                    }
                    parser = new Parser(new Lexer(html));
                    NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("id", "StaffList"), new TagNameFilter("table")));

                    if (nodeList != null && nodeList.Count > 0)
                    {
                        TableTag table = nodeList[0] as TableTag;
                        for (int j = 1; j < table.RowCount; j++)
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt=string.Empty;
                            TableRow tr = table.Rows[j];
                            code = tr.Columns[0].ToPlainTextString().Trim();
                            prjName = tr.Columns[1].ToPlainTextString().Trim();
                            beginDate = tr.Columns[2].ToPlainTextString().Trim();
                            ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                            InfoUrl = "http://www.cobo91.com/project/" + aTag.Link;
                            string htmldetail = string.Empty;
                            try
                            {
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Trim();
                                Parser dtlparserHTML = new Parser(new Lexer(htmldetail));
                                NodeList dtnodeHTML = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "info"), new TagNameFilter("table")));
                                HtmlTxt = dtnodeHTML.AsHtml();
                                htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).Replace("&nbsp;", "").Replace("</br>", "\r\n").Replace("<br>", "\r\n");
                                Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                                htmldetail = regexHtml.Replace(htmldetail, "");
                            }
                            catch (Exception ex) { continue; }
                            Parser dtlparser = new Parser(new Lexer(htmldetail));
                            NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "info"), new TagNameFilter("table")));
                             
                            inviteCtx = System.Web.HttpUtility.HtmlDecode(dtnode.AsString().Replace("【打印本页】", "").Replace("【关闭窗口】", "").Replace("版权所有：中邦国际招标&邦迪工程顾问", ""));
                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex();
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = inviteCtx.GetRegex("采购单位,招标代理");
                            if (string.IsNullOrEmpty(prjAddress))
                                prjAddress = inviteCtx.GetRegex("开标地点");
                            specType = "其他";
                            msgType = "中邦国际招标&邦迪工程顾问";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            dtlparser.Reset();
                            NodeList FileTag = dtlparser.ExtractAllNodesThatMatch( new TagNameFilter("a"));
                            if (FileTag != null && FileTag.Count > 0)
                            {
                                for (int f = 0; f < FileTag.Count; f++)
                                {
                                    ATag file = FileTag[f] as ATag;
                                    if (file.Link.ToUpper().Contains(".DOC"))
                                    {
                                        BaseAttach attach = ToolDb.GenBaseAttach(file.ToPlainTextString(), info.Id, "http://www.cobo91.com" + file.Link);
                                        base.AttachList.Add(attach);
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
    }
}

