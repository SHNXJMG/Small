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
    public class InivteYuanDongGj : WebSiteCrawller
    {
        public InivteYuanDongGj()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "中国远东国际招标有限公司招标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取中国远东国际招标有限公司招标信息";
            this.SiteUrl = "http://www.cfet.com.cn/index.php?optionid=679";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl);

            }
            catch
            {
                return list;
            }

            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pagePanel")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                string pageTemp = tdNodes.AsString().Replace("&nbsp;", "").Replace(" ", "").Trim();
                try
                {
                    pageInt = int.Parse(pageTemp.GetRegexBegEnd("总", "页"));
                }
                catch (Exception ex) { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "&page=" + i);
                    }
                    catch { continue; }

                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "ST12")), true), new TagNameFilter("li")));

                if (nodeList != null && nodeList.Count > 0)
                {
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        ATag aTag = nodeList[j].GetATag();

                        prjName = aTag.GetAttribute("title");
                        if (prjName.Contains("声明"))
                            continue;
                        beginDate = nodeList[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link;
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString();
                        }
                        catch { continue; }
                        Parser dtlparser = new Parser(new Lexer(htmldetail));
                        NodeList dtnode = dtlparser.ExtractAllNodesThatMatch(new AndFilter(new HasAttributeFilter("class", "content"), new TagNameFilter("div")));

                        if (dtnode != null && dtnode.Count > 0)
                        {
                            HtmlTxt = dtnode.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();

                            buildUnit = inviteCtx.GetBuildRegex();
                            prjAddress = inviteCtx.GetAddressRegex(new string[] { "地址" });
                            if (string.IsNullOrEmpty(prjAddress))
                                prjAddress = inviteCtx.GetRegex("开标地点");
                            else
                                prjAddress = inviteCtx.GetAddressRegex();
                            if (string.IsNullOrEmpty(buildUnit))
                                buildUnit = inviteCtx.GetRegex("采购单位,招标代理");
                            if (prjAddress.Contains("用户指定"))
                                prjAddress = ""; 
                            specType = "政府采购";
                            msgType = "中国远东国际招标公司";
                            inviteType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            dtlparser = new Parser(new Lexer(HtmlTxt));
                            NodeList FileTag = dtlparser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (FileTag != null && FileTag.Count > 0)
                            {
                                for (int f = 0; f < FileTag.Count; f++)
                                {
                                    ATag file = FileTag[f] as ATag;
                                    if (file.IsAtagAttach())
                                    {
                                        string link = string.Empty;
                                        if (file.Link.ToLower().Contains("http"))
                                            link = file.Link;
                                        else
                                            link = "http://www.cfet.com.cn/" + file.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(file.ToPlainTextString(), info.Id, link);
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
