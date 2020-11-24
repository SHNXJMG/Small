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
    public class InviteZhaoqing : WebSiteCrawller
    {
        public InviteZhaoqing()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省肇庆市";
            this.Description = "自动抓取广东省肇庆市区招标信息";
            this.SiteUrl = "http://ggzy.zhaoqing.gov.cn/zqfront/showinfo/moreinfolist.aspx?categorynum=003001001&Paging=";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
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
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "1", Encoding.UTF8);
            }
            catch
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "paging"), new TagNameFilter("div")));
            if (sNode != null && sNode.Count > 0)
            {
                string temp = sNode[0].ToNodePlainString();
                try
                {
                    temp = temp.GetRegexBegEnd("/", "转到");
                    pageInt = int.Parse(temp);
                }
                catch { }
            }

            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + i, Encoding.UTF8);
                    }
                    catch
                    {
                        continue;
                    }
                }

                parser = new Parser(new Lexer(html));
                sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new HasAttributeFilter("class", "column-info-list"), new TagNameFilter("div")), true), new TagNameFilter("li")));
                if (sNode != null && sNode.Count > 0)
                {
                    for (int t = 0; t < sNode.Count; t++)
                    {

                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = sNode[t].GetATag();
                        prjName = aTag.LinkText.ToNodeString();
                        InfoUrl = "http://ggzy.zhaoqing.gov.cn" + aTag.Link;
                        beginDate = sNode[t].ToPlainTextString().GetDateRegex();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).GetJsString();
                        }
                        catch { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new TagNameFilter("body"));
                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();



                            buildUnit = inviteCtx.GetBuildRegex();
                            if (buildUnit.Contains("中心"))
                                buildUnit = buildUnit.Remove(buildUnit.IndexOf("中心")) + "中心";
                            prjAddress = inviteCtx.GetAddressRegex();
                            code = inviteCtx.GetCodeRegex().GetCodeDel();

                            msgType = "肇庆市公共资源交易中心";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            specType = "建设工程";
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "肇庆市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
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
