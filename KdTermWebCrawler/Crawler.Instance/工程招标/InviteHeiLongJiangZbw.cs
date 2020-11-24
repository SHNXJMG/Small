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
using System.Web.Script.Serialization;


namespace Crawler.Instance
{
    public class InviteHeiLongJiangZbw : WebSiteCrawller
    {
        public InviteHeiLongJiangZbw()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "黑龙江省发展和改革委员会招标信息";
            this.Description = "自动抓取黑龙江省发展和改革委员会招标信息";
            this.PlanTime = "09:05,09:50,10:50,11:30,14:25,15:25,16:50";
            this.MaxCount = 400;
            this.SiteUrl = "http://www.hljdpc.gov.cn/col/col148/index.html";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string url = "http://www.hljdpc.gov.cn/col/col148/index.html";

            Dictionary<string, string> ggType = new Dictionary<string, string>();
            ggType.Add("勘察设计", "18101");
            ggType.Add("施工", "18102");
            ggType.Add("监理", "18103");
            ggType.Add("设备", "18104");

            IList list = new List<InviteInfo>();

            foreach (string key in ggType.Keys)
            {
                int pageInt = 1;
                int count = 0;
                string html = string.Empty;
                string viewState = string.Empty;
                string eventValidation = string.Empty;
                string cookiestr = string.Empty;
                NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] {
                "dopost",
                "pagesize",
                "CategoryID",
                "sort",
                "keyword",
                "pageno"
            },
                    new string[] {
                        "product_list",
                        "5",
                        "2",
                        ggType[key],
                        "",
                        "1"
                    });
                try
                {
                    html = ToolWeb.GetHtmlByUrl(url, nvc);
                }
                catch { return list; }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)(((Dictionary<string, object>)serializer.DeserializeObject(html))["listpage"]);

                pageInt = Convert.ToInt32(smsTypeJson["pagecount"]);

                for (int i = 1; i <= pageInt; i++)
                {
                    if (i > 1)
                    {
                        nvc = ToolWeb.GetNameValueCollection(new string[] {
                "dopost",
                "pagesize",
                "CategoryID",
                "sort",
                "keyword",
                "pageno"
            },
                   new string[] {
                        "product_list",
                        "5",
                        "2",
                        ggType[key],
                        "",
                        i.ToString()
                   });
                        try
                        {
                            html = ToolWeb.GetHtmlByUrl(url, nvc);
                        }
                        catch { continue; }
                        serializer = new JavaScriptSerializer();
                        smsTypeJson = (Dictionary<string, object>)(((Dictionary<string, object>)serializer.DeserializeObject(html))["listpage"]);
                    }

                    object[] listDatas = (object[])smsTypeJson["listdata"];

                    foreach (object obj in listDatas)
                    {
                        Dictionary<string, object> dic = (Dictionary<string, object>)obj;

                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                         
                        string tempName = Convert.ToString(dic["Name"]);
                        string area = tempName.GetRegexBegEnd("【", "】");
                        prjName = tempName.GetReplace("【"+area+"】");
                        beginDate = Convert.ToString(dic["FTime"]);
                        InfoUrl = "http://www.hljztb.com/" + dic["SUrl"];
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = ToolWeb.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        Parser parser = new Parser(new Lexer(htmldtl));
                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "TABLE1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            TableTag table = dtlNode[0] as TableTag;
                            for (int r = 0; r < table.RowCount; r++)
                            {
                                for (int c = 0; c < table.Rows[r].ColumnCount; c++)
                                {
                                    string temp = table.Rows[r].Columns[c].ToNodePlainString();
                                    if ((c + 1) % 2 == 0)
                                        inviteCtx += temp.GetReplace(":,：") + "\r\n";
                                    else
                                        inviteCtx += temp.GetReplace(":,：") + "：";
                                }
                            }
                            prjAddress = inviteCtx.GetAddressRegex().GetCodeDel().GetReplace("　");
                            buildUnit = inviteCtx.GetBuildRegex().GetReplace("　");
                            if (buildUnit.Contains("公司"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("公司")) + "公司";
                            if (buildUnit.Contains("联系"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("联系"));
                            if (buildUnit.Contains("地址"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("地址"));
                            code = inviteCtx.GetCodeRegex().GetCodeDel();
                            if (string.IsNullOrEmpty(code))
                                code = inviteCtx.GetRegex("编码");
                            msgType = "黑龙江住房和城乡建设厅";
                            inviteType = key;
                            specType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("黑龙江省", "黑龙江省及地市", area, string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            count++;
                             parser = new Parser(new Lexer(HtmlTxt));
                            NodeList aNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aNode != null && aNode.Count > 0)
                            {
                                for (int k = 0; k < aNode.Count; k++)
                                {
                                    ATag a = aNode[k] as ATag;
                                    if (a.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (a.Link.ToLower().Contains("http"))
                                            link = a.Link;
                                        else
                                            link = "http://www.hljztb.com/" + a.Link.GetReplace("../,./");
                                        if (Encoding.Default.GetByteCount(link) > 500)
                                            continue;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText, info.Id, link);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && count >= this.MaxCount) goto Found;
                        }
                    }
                }
                Found:;
            }
            return list;
        }
    }
}
