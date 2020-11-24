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
using System.IO;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class NotifyInfoAllSZ : WebSiteCrawller
    {
        public NotifyInfoAllSZ()
            : base()
        {
            this.Group = "通知公告";
            this.PlanTime = "12:00,03:20";
            this.Title = "深圳市政府采购全区站点";
            this.MaxCount = 100000;
            this.Description = "自动抓取深圳市政府采购全区站点";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://ns.szzfcg.cn/portal/topicView.do?method=view&id=40074439";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NotifyInfo>();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            //dic.Add("盐田区", "http://yt.szzfcg.cn/portal/topicView.do?method=view&id=50074439");
            dic.Add("龙华新区", "http://lhxq.szzfcg.cn/portal/topicView.do?method=view&id=110074439");
            dic.Add("大鹏新区", "http://dp.szzfcg.cn/portal/topicView.do?method=view&id=100074439");
            dic.Add("坪山新区", "http://ps.szzfcg.cn/portal/topicView.do?method=view&id=90074439");
            dic.Add("龙岗区", "http://lg.szzfcg.cn/portal/topicView.do?method=view&id=70074439");
            dic.Add("光明新区", "http://gm.szzfcg.cn/portal/topicView.do?method=view&id=10170626");
            dic.Add("福田区", "http://ft.szzfcg.cn/portal/topicView.do?method=view&id=30074439");
            dic.Add("罗湖区", "http://lh.szzfcg.cn/portal/topicView.do?method=view&id=20074439");
            dic.Add("南山区", "http://ns.szzfcg.cn/portal/topicView.do?method=view&id=40074439");

            Dictionary<string, string> dicCity = new Dictionary<string, string>();
            //dicCity.Add("盐田区", "yt");
            dicCity.Add("龙华新区", "lhxq");
            dicCity.Add("大鹏新区", "dp");
            dicCity.Add("坪山新区", "ps");
            dicCity.Add("龙岗区", "lg");
            dicCity.Add("光明新区", "gm");
            dicCity.Add("福田区", "ft");
            dicCity.Add("罗湖区", "lh");
            dicCity.Add("南山区", "ns");

            foreach (string key in dic.Keys)
            {
                int pageInt = 1, sqlCount = 0;
                string html = string.Empty;
                string viewState = string.Empty;
                string eventValidation = string.Empty;
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(dic[key]);
                }
                catch { continue; }
                Parser parser = new Parser(new Lexer(html));
                NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("select"), new HasAttributeFilter("name", "__ec_pages")));
                if (pageNode != null && pageNode.Count > 0)
                {
                    SelectTag select = pageNode[0] as SelectTag;
                    try
                    {
                        pageInt = int.Parse(select.OptionTags[select.OptionTags.Length - 1].Value);
                    }
                    catch { }
                }
                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        string id = dic[key].Substring(dic[key].IndexOf("id"), dic[key].Length - dic[key].IndexOf("id")).Replace("id=","");
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                          "ec_i",
                          "topicChrList_20070702_crd",
                          "topicChrList_20070702_f_a",
                          "topicChrList_20070702_p",
                          "topicChrList_20070702_s_name",
                          "topicChrList_20070702_s_topName",
                          "id",
                          "method",
                          "__ec_pages",
                          "topicChrList_20070702_rd",
                          "topicChrList_20070702_f_name",
                          "topicChrList_20070702_f_topName",
                          "topicChrList_20070702_f_ldate",
                        }, new string[]{
                        "topicChrList_20070702",
                        "20",
                        "",
                        i.ToString(),
                        "",
                        "",
                        id,
                        "view",
                        i.ToString(),
                        "20",
                        "",
                        "",
                        ""
                        });
                        html = this.ToolWebSite.GetHtmlByUrl(dic[key], nvc);
                    }
                    parser = new Parser(new Lexer(html));
                    NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "topicChrList_20070702_table")));
                    if (listNode != null & listNode.Count > 0)
                    {
                        TableTag table = listNode[0] as TableTag;
                        for (int j = 3; j < table.RowCount; j++)
                        {
                            string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

                            TableRow tr = table.Rows[j];
                            headName = tr.Columns[1].ToNodePlainString();
                            releaseTime = tr.Columns[3].ToPlainTextString();
                            infoType = "通知公告";
                            msgType = "深圳市" + key + "政府采购中心";

                            infoUrl = "http://" + dicCity[key] + ".szzfcg.cn" + tr.Columns[1].GetATagHref();
                            string htmldtl = string.Empty;
                            try
                            {
                                htmldtl = this.ToolWebSite.GetHtmlByUrl(infoUrl).GetJsString();
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmldtl));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"),new HasAttributeFilter("align","center")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                ctxHtml = dtlNode[0].ToHtml();
                                infoCtx = ctxHtml.ToCtxString();
                                NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳政府采购", key, infoCtx, infoType);
                                sqlCount++;
                                if (!crawlAll && sqlCount >= this.MaxCount)
                                {
                                    goto type;
                                }
                                ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate);
                            }
                            else
                            {
                                parser.Reset();
                                NodeList bodyNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                                if (bodyNode != null && bodyNode.Count > 0)
                                {
                                    ctxHtml = bodyNode.AsHtml();
                                    infoCtx = ctxHtml.ToCtxString();
                                    NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳政府采购", key, infoCtx, infoType);
                                    sqlCount++;
                                    if (!crawlAll && sqlCount >= this.MaxCount)
                                    {
                                        return null;
                                    }
                                    if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                                    {
                                        parser.Reset();
                                        NodeList imgList = parser.ExtractAllNodesThatMatch(new TagNameFilter("img"));
                                        if (imgList != null && imgList.Count > 0)
                                        {
                                            for (int m = 0; m < imgList.Count; m++)
                                            {
                                                try
                                                {
                                                    ImageTag img = imgList[m] as ImageTag;
                                                    string src = img.GetAttribute("src");
                                                    BaseAttach obj = null;
                                                    if (src.Contains("http"))
                                                    {
                                                        obj = ToolHtml.GetBaseAttach(src, headName, info.Id);
                                                    }
                                                    else
                                                    {
                                                        obj = ToolHtml.GetBaseAttach("http://" + dicCity[key] + ".szzfcg.cn" + src, headName, info.Id);
                                                    }
                                                    if (obj != null)
                                                        ToolDb.SaveEntity(obj, string.Empty);
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            type: continue;
            }
            return list;
        }


    }
}
