﻿using System.Text;
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
    public class InviteDgSzzfcg : WebSiteCrawller
    {
        public InviteDgSzzfcg()
            : base()
        {
            this.Group = "政府采购招标信息";
            this.Title = "东莞市政府采购招标信息(市直)";
            this.PlanTime = "9:04,11:04,13:04,15:04,17:04";
            this.Description = "自动抓取东莞市政府采购招标信息(市直)";
            this.SiteUrl = "https://www.dgzb.com.cn/ggzy/website/WebPagesManagement/TradeInfo/GovProcurement/govlist?fcInfotype=1&openbidbelong=SS&belongIndex=0&govTypeIndex=0";
            this.MaxCount = 800;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch
            {
                return list;
            }




            Parser parser = new Parser(new Lexer(html));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("script"), new HasAttributeFilter("type", "text/javascript")));
            string b = pageNode.AsString().GetCtxBr();
            string c = b.Replace("('", "徐鑫").Replace("')", "凯德");
            if (pageNode != null && pageNode.Count > 0)
            {
                try
                {
                    string temp = c.GetRegexBegEnd("徐鑫", "凯德");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
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
                        string a1=nvc[0];
                        html = this.ToolWebSite.GetHtmlByUrl("https://www.dgzb.com.cn/ggzy/website/WebPagesManagement/TradeInfo/GovProcurement/findListByPage?fcInfotype=1&openbidbelong=SS", nvc, Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
                foreach (KeyValuePair<string, object> obj in smsTypeJson)
                {
                    object[] array = (object[])obj.Value;

                    foreach (object arrValue in array)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                        code = Convert.ToString(dic["fcTendersn"]);
                        prjName = Convert.ToString(dic["fcInfotitle"]);
                        beginDate=Convert.ToString(dic["fcInfostartdate"]).GetDateRegex("yyyy-MM-dd");
                        string xu = Convert.ToString(dic["publishinfoid"]);
                        InfoUrl = "https://www.dgzb.com.cn/ggzy/website/WebPagesManagement/TradeInfo/GovProcurement/govdetail?publishinfoid=" + xu + "&fcInfotype=1";
                        //fcInfocontent

                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = dic["fcInfocontent"].ToString();
                           // htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch
                        {
                            continue;
                        }
                        bool isTable = true;
                        parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("align", "center")));
                        if (dtlNode == null || dtlNode.Count < 1)
                        {
                            isTable = false;
                            parser = new Parser(new Lexer(htmldtl));
                            dtlNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        }
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.ToHtml();
                            if (isTable)
                            {
                                TableTag dtlTable = dtlNode[0] as TableTag;
                                for (int d = 0; d < dtlTable.RowCount; d++)
                                {
                                    try
                                    {
                                        inviteCtx += dtlTable.Rows[d].Columns[0].ToPlainTextString().Replace("：", "").Replace(":", "") + "：";
                                        inviteCtx += dtlTable.Rows[d].Columns[1].ToPlainTextString() + "\r\n";
                                    }
                                    catch { }
                                }
                                if (string.IsNullOrEmpty(inviteCtx)) inviteCtx = HtmlTxt.ToCtxString();
                            }
                            else
                                // inviteCtx = HtmlTxt.ToCtxString();

                                inviteCtx = HtmlTxt.Replace("</p>", "\r\n").ToCtxString();
                            prjAddress = inviteCtx.GetRegexBegEnd("地点", "十");
                            if (string.IsNullOrEmpty(prjAddress))
                                prjAddress = inviteCtx.GetAddressRegex();
                            inviteType = prjName.GetInviteBidType();

                            msgType = "东莞市政府采购";
                            specType = "政府采购";

                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "东莞市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
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

            }
            return list;
        }
    }
}
