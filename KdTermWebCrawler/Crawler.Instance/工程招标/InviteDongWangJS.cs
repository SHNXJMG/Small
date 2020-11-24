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
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Data;

namespace Crawler.Instance
{
    public class InviteDongWangJS : WebSiteCrawller
    {
        public InviteDongWangJS()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省东莞市建设工程招标信息";
            this.Description = "自动抓取广东省东莞市建设工程招标信息";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.ExistCompareFields = "Code,ProjectName,InfoUrl,Code";
            this.MaxCount = 50;
            this.SiteUrl = "https://www.dgzb.com.cn/ggzy/website/WebPagesManagement/jslist?fcInfotype=1&tenderkind=A&projecttendersite=SS&TypeIndex=0&KindIndex=0";
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
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8, ref cookiestr);
            }
            catch (Exception ex)
            {

                return list;
            }

            Parser parser = new Parser(new Lexer(htl));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("script"), new HasAttributeFilter("type", "text/javascript")));
            string b = pageNode.AsString().GetCtxBr();
            string c = b.Replace("('", "徐鑫").Replace("')", "凯德");
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = c.GetRegexBegEnd("徐鑫", "凯德");
                    page = int.Parse(temp);
                }
                catch { }
            }

            for (int i = 1; i <= page; i++)
            {
                if (i >= 1)
                {
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(
                        new string[] { "fcInfotitle",
                            "currentPage"},
                        new string[]{
                        "",
                        i.ToString()
                        }
                        );
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl("https://www.dgzb.com.cn/ggzy/website/WebPagesManagement/findListByPage?fcInfotype=1&tenderkind=A&projecttendersite=SS&orderFiled=fcInfoenddate&orderValue=desc", nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(htl);
                foreach (KeyValuePair<string, object> obj in smsTypeJson)
                {
                    object[] array = (object[])obj.Value;

                    foreach (object arrValue in array)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                        code = Convert.ToString(dic["fcTendersn"]);
                        prjName = Convert.ToString(dic["fcInfotitle"]);
                        beginDate = Convert.ToString(dic["fcInfostartdate"]).GetDateRegex("yyyy-MM-dd");

                        string xu = Convert.ToString(dic["id"]);
                        InfoUrl = "https://www.dgzb.com.cn/ggzy/website/WebPagesManagement/jsdetail?publishId=" + xu + "&fcInfotype=1";
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "detail")));
                        if (dtnode.Count > 0 && dtnode != null)
                        {
                            HtmlTxt = dtnode.AsHtml();

                            inviteCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString();

                            prjAddress = inviteCtx.GetRegexBegEnd("工程地址：", "\r");
                            buildUnit = inviteCtx.GetRegexBegEnd("建设单位：", "\r");
                            
                            msgType = "东莞市建设工程交易中心";
                            specType = "建设工程";
                            Regex regoType = new Regex(@"工程类型(：|:)[^\r\n]+\r\n");
                            otherType = regoType.Match(inviteCtx).Value.Replace("工程类型：", "").Trim();
                            inviteCtx = inviteCtx.Replace("ctl00_cph_context_span_MetContent", "").Replace("<span id=", "").Replace("</span>", "").Replace(">", "").Trim();
                            if (buildUnit == "")
                            {
                                buildUnit = "见招标信息";
                            }
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "东莞市区", "",
                                string.Empty, code, prjName, prjAddress, buildUnit,
                                beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);//附件搜索
                            parserdetail.Reset();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aTagNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aTagNode != null && aTagNode.Count > 0)
                            {
                                for (int k = 0; k < aTagNode.Count; k++)
                                {
                                    ATag aTag = aTagNode[k].GetATag();
                                    if (aTag.IsAtagAttach())
                                    {
                                        string linkurl = aTag.Link;
                                        linkurl = linkurl.Replace("&amp;", "&");
                                        string cc = string.Empty;
                                        string aa = linkurl.GetRegexBegEnd("&", "id");
                                        if (aa == "")
                                        {
                                            cc = linkurl;
                                        }
                                        else
                                        {
                                            cc = linkurl.Replace(aa, "");
                                        }
                                        BaseAttach attach = ToolDb.GenBaseAttach(aTag.LinkText, info.Id, cc);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }


                //for (int i = 1; i < page; i++)
                //{
                //    if (i > 1)
                //    {
                //        viewState = this.ToolWebSite.GetAspNetViewState(htl);
                //        eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                //        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                //            "__EVENTTARGET",
                //            "__EVENTARGUMENT",
                //            "__LASTFOCUS",
                //            "__VIEWSTATE",
                //            "__EVENTVALIDATION",
                //            "ctl00$cph_context$drp_selSeach",
                //            "ctl00$cph_context$txt_strWhere",
                //            "ctl00$cph_context$drp_Rq",
                //            "ctl00$cph_context$GridViewPaingTwo1$txtGridViewPagingForwardTo",
                //            "ctl00$cph_context$GridViewPaingTwo1$btnNext.x",
                //            "ctl00$cph_context$GridViewPaingTwo1$btnNext.y"        
                //        }, new string[]{
                //            string.Empty,
                //            string.Empty,
                //            string.Empty,
                //            viewState,
                //            eventValidation,
                //            "1",
                //            string.Empty,
                //            "3",
                //            (i-1).ToString(),
                //            "8",
                //            "10"
                //        });
                //        try
                //        {
                //            htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                //        }
                //        catch (Exception ex) { continue; }
                //    }
                //    parser = new Parser(new Lexer(htl));
                //    NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_GridView1")));
                //    if (tableNodeList != null && tableNodeList.Count > 0)
                //    {
                //        TableTag table = (TableTag)tableNodeList[0];
                //        for (int j = 1; j < table.RowCount; j++)
                //        {
                //            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                //                prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                //                specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                //                remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                //                CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                //            TableRow tr = table.Rows[j];
                //            code = tr.Columns[1].ToPlainTextString().Trim();
                //            prjName = tr.Columns[2].ToPlainTextString().Trim();
                //            beginDate = tr.Columns[4].ToPlainTextString().Trim().GetReplace(" - ", "&").Split('&')[0].Trim();
                //            try
                //            {
                //                endDate = tr.Columns[4].ToPlainTextString().Trim().GetReplace(" - ", "&").Split('&')[1].Trim();
                //            }
                //            catch { }
                //            ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                //            InfoUrl = "http://www.dgzb.com.cn:8080/dgjyweb/sitemanage/" + aTag.Link.Replace("amp;", "").Trim();
                //            string htmldetail = string.Empty;
                //            try
                //            {
                //                htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8).Replace("&nbsp;", "");
                //            }
                //            catch (Exception)
                //            {
                //                continue;
                //            }
                //            Parser parserdetail = new Parser(new Lexer(htmldetail));
                //            NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_cph_context_span_MetContent")));
                //            if (dtnode.Count > 0 && dtnode != null)
                //            {
                //                HtmlTxt = dtnode.AsHtml();
                //                inviteCtx = dtnode.ToHtml().Replace("<br/>", "\r\n");
                //                Regex regBuidUnit = new Regex(@"建设单位：[^\r\n]+\r\n");
                //                buildUnit = regBuidUnit.Match(inviteCtx).Value.Replace("建设单位：", "").Replace("：", "").Trim();
                //                Regex regPrjAddr = new Regex(@"(工程地点|工程地址)(：|:)[^\r\n]+\r\n");
                //                prjAddress = regPrjAddr.Match(inviteCtx).Value.Replace("工程地点：", "").Replace("工程地址", "").Replace("：", "").Trim();
                //                msgType = "东莞市建设工程交易中心";
                //                specType = "建设工程";
                //                Regex regoType = new Regex(@"工程类型(：|:)[^\r\n]+\r\n");
                //                otherType = regoType.Match(inviteCtx).Value.Replace("工程类型：", "").Trim();
                //                inviteCtx = inviteCtx.Replace("ctl00_cph_context_span_MetContent", "").Replace("<span id=", "").Replace("</span>", "").Replace(">", "").Trim();
                //                if (buildUnit == "")
                //                {
                //                    buildUnit = "见招标信息";
                //                }
                //                inviteType = ToolHtml.GetInviteTypes(prjName);
                //                InviteInfo info = ToolDb.GenInviteInfo("广东省", "东莞市区", "",
                //                    string.Empty, code, prjName, prjAddress, buildUnit,
                //                    beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                //                list.Add(info);//附件搜索
                //                parserdetail.Reset();
                //                NodeList fileNode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_DownLoadFiles1_GridView2")));
                //                if (fileNode != null && fileNode.Count > 0)
                //                {
                //                    string iii = fileNode.AsString().Trim();
                //                    TableTag tablefile = (TableTag)fileNode[0];
                //                    for (int k = 1; k < tablefile.RowCount; k++)
                //                    {
                //                        string fileName = string.Empty, fileUrl = string.Empty;
                //                        TableRow trfile = tablefile.Rows[k];
                //                        if (trfile.Columns[1].ToPlainTextString().Trim() != "")
                //                        {
                //                            ATag aTagfile = trfile.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                //                            fileName = trfile.Columns[1].ToPlainTextString().Trim();
                //                            fileUrl = "http://www.dgzb.com.cn/dgjyweb/sitemanage/" + aTagfile.Link.Replace("amp;", "").Trim();
                //                            BaseAttach attach = ToolDb.GenBaseAttach(fileName, info.Id, fileUrl);
                //                            base.AttachList.Add(attach);
                //                        }
                //                    }
                //                }
                //                parserdetail.Reset();//补充文件搜索
                //                NodeList fileBuChongNode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_BuChongFileDown1_GridView2")));
                //                if (fileBuChongNode != null && fileBuChongNode.Count > 0)
                //                {
                //                    string iii = fileBuChongNode.AsString().Trim();
                //                    TableTag tableBuChongfile = (TableTag)fileBuChongNode[0];
                //                    for (int k = 1; k < tableBuChongfile.RowCount; k++)
                //                    {
                //                        string fileName = string.Empty, fileUrl = string.Empty;
                //                        TableRow trfileBuChong = tableBuChongfile.Rows[k];
                //                        if (trfileBuChong.Columns[1].ToPlainTextString().Trim() != "")
                //                        {
                //                            ATag aTagfile = trfileBuChong.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                //                            fileName = trfileBuChong.Columns[1].ToPlainTextString().Trim();
                //                            fileUrl = "http://www.dgzb.com.cn/dgjyweb/sitemanage/" + aTagfile.Link.Replace("amp;", "").Trim();
                //                            BaseAttach attach = ToolDb.GenBaseAttach(fileName, info.Id, fileUrl);
                //                            base.AttachList.Add(attach);
                //                        }
                //                    }
                //                }
                //                if (!crawlAll && list.Count >= this.MaxCount) return list;
                //            }
                //        }
                //    }
            }
                return null;
            }
        }
    }

