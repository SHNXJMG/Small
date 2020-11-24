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
    /// <summary>
    /// 内蒙古政府采购  盟市信息
    /// </summary>
    public class InviteZfNeimengMS : WebSiteCrawller
    {
        public InviteZfNeimengMS()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "内蒙古政府采购盟市信息";
            this.Description = "自动抓取内蒙古政府采购招标信息";
            this.SiteUrl = "http://www.nmgp.gov.cn/category/category-ajax.php?type_name=1&purorgform=1&byf_page=1&fun=cggg&_=";
            this.MaxCount = 50;
            this.PlanTime = "9:40,11:40,14:40,16:40,18:40";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList(); 
            //取得页码
            string html = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                // html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
                DateTime time = ToolHtml.GetDateTimeByLong(1509517250628);
                DateTime  dt24 = DateTime.Now.ToUniversalTime();
                string b = ToolHtml.GetDateTimeLong(dt24).ToString();
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl+b, Encoding.Default);
            }
            catch (Exception ex)
            { 
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            int pageInt = 1;


            JavaScriptSerializer serializer = new JavaScriptSerializer();

            object[] objs = (object[])serializer.DeserializeObject(html);
            object[] items = objs[1] as object[];
            Dictionary<string, object> smsTypeJson = items[0] as Dictionary<string, object>;
            string a = Convert.ToString(smsTypeJson["page_all"]);
            int page = int.Parse(a);
            pageInt = page / 18 + 1;
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                       string SiteUrl1 = "http://www.nmgp.gov.cn/category/category-ajax.php?type_name=1&purorgform=1&byf_page=" + i + "&fun=cggg&_=1509437380682";
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl1, Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("分页");
                        continue;
                    }
                }

                parser = new Parser(new Lexer(html));
                JavaScriptSerializer serializer1 = new JavaScriptSerializer();
                object[] objd = (object[])serializer.DeserializeObject(html);
                object[] items1 = objd[0] as object[];
                Dictionary<string, object> smsTypeJson1 = items1[0] as Dictionary<string, object>;
                foreach (KeyValuePair<string, object> obj in smsTypeJson)
                {
                    object[] array = objd[0] as object[];
                    foreach (object arrValue in array)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, HtmlTxt = string.Empty;

                        Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                        endDate = Convert.ToString(dic["ENDDATE"]).GetDateRegex("yyyy-MM-dd");
                        prjName = Convert.ToString(dic["TITLE"]);
                        string xu = Convert.ToString(dic["wp_mark_id"]);
                        InfoUrl = "http://www.nmgp.gov.cn/ay_post/post.php?tb_id=1&p_id=" + xu;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8).Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                        htmldtl = regexHtml.Replace(htmldtl, "");
                        Parser parserdtl = new Parser(new Lexer(htmldtl));
                        Parser dtlparserHTML = new Parser(new Lexer(htmldtl));
                        NodeList nodesDtl = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "center")));
                        if (nodesDtl != null && nodesDtl.Count > 0)
                        {
                            Parser begDate = new Parser(new Lexer(nodesDtl.ToHtml()));
                            NodeList begNode = begDate.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "center")));
                            if (begNode != null && begNode.Count > 0)
                            {
                                beginDate = begNode.AsString().GetDateRegex("yyyy年MM月dd日");
                            }

                            HtmlTxt = nodesDtl.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();

                            code = inviteCtx.GetRegex("采购文件编号,工程编号,项目编号").Replace("无", "");
                            code = inviteCtx.GetRegexBegEnd("采购文件编号：", "2、");
                            buildUnit = inviteCtx.GetBuildRegex();
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = inviteCtx.GetRegexBegEnd("代理机构名称：", "地址");
                            prjAddress = inviteCtx.GetAddressRegex();
                            if (string.IsNullOrEmpty(prjAddress))
                                prjAddress = inviteCtx.GetRegexBegEnd("地址：", "邮政编码");

                            msgType = "内蒙古政府采购盟市";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("内蒙古自治区", "内蒙古自治区及盟市", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, "政府采购", string.Empty, InfoUrl, HtmlTxt);
                            list.Add(info);
                            if (!crawlAll && list.Count >= this.MaxCount)
                                return list;
                        }
                    }
                }
                //    if (nodes != null && nodes.Count > 0)
                //{
                //    TableTag table = nodes[0] as TableTag;

                //    for (int t = 0; t < table.RowCount; t++)
                //    {
                //        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, HtmlTxt = string.Empty;

                //        TableRow tr = table.Rows[t];
                //        endDate = tr.Columns[1].ToPlainTextString().GetDateRegex();
                //        ATag alink = tr.Columns[0].GetATag();
                //        prjName = alink.LinkText;
                //        InfoUrl = "http://www.nmgp.gov.cn" + alink.Link;
                //        string htmldtl = string.Empty;
                //        try
                //        {
                //            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("&nbsp;", "").Trim();
                //        }
                //        catch (Exception ex)
                //        {
                //            continue;
                //        }

                //        Regex regexHtml = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>");
                //        htmldtl = regexHtml.Replace(htmldtl, "");
                //        Parser parserdtl = new Parser(new Lexer(htmldtl));
                //        Parser dtlparserHTML = new Parser(new Lexer(htmldtl));
                //        NodeList nodesDtl = dtlparserHTML.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("id", "hlcms_9")));
                //        if (nodesDtl != null && nodesDtl.Count > 0)
                //        {
                //            Parser begDate = new Parser(new Lexer(nodesDtl.ToHtml()));
                //            NodeList begNode = begDate.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "yzhang")));
                //            if (begNode != null && begNode.Count > 0)
                //            {
                //                beginDate = begNode.AsString().GetDateRegex("yyyy年MM月dd日");
                //            }

                //            HtmlTxt = nodesDtl.AsHtml();
                //            inviteCtx = HtmlTxt.ToCtxString();

                //            code = inviteCtx.GetRegex("采购文件编号,工程编号,项目编号").Replace("无", "");
                //            buildUnit = inviteCtx.GetBuildRegex();
                //            if (string.IsNullOrEmpty(buildUnit))
                //                buildUnit = inviteCtx.GetRegex("采购代理机构名称,采购单位名称");
                //            prjAddress = inviteCtx.GetAddressRegex();
                //            if (string.IsNullOrEmpty(prjAddress))
                //                prjAddress = inviteCtx.GetRegex("投标地点,开标地点,地址");

                //            msgType = "内蒙古政府采购盟市";
                //            inviteType = ToolHtml.GetInviteTypes(prjName);
                //            InviteInfo info = ToolDb.GenInviteInfo("内蒙古自治区", "内蒙古自治区及盟市", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, "政府采购", string.Empty, InfoUrl, HtmlTxt);
                //            list.Add(info);
                //            if (!crawlAll && list.Count >= this.MaxCount)
                //                return list;
                //        }
                //    }
                //}
            }
            return list;
        }
    }
}
