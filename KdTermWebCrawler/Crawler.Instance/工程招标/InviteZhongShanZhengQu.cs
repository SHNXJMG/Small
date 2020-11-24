using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Lex;
using System.Collections;
using System.Collections.Specialized;

namespace Crawler.Instance
{
    public class InviteZhongShanZhengQu : WebSiteCrawller
    {
        public InviteZhongShanZhengQu()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省中山市镇区招标工程";
            this.Description = "自动抓取广东省中山市镇区招标工程";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "http://www.zsjyzx.gov.cn/zsweb/index/showList/000000000002/000000000383";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            //取得页码
            string html = string.Empty;
            int pageInt = 33;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch 
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList aNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "scott"))), new TagNameFilter("a")));
            if (aNodes != null && aNodes.Count > 0)
            {
                try
                {
                    string temp = aNodes.GetATagHref(aNodes.Count - 1);
                    pageInt = Convert.ToInt32(temp.GetRegexBegEnd("(", ")"));
                }
                catch (Exception)
                {
                    pageInt = 33;
                }
            }

            parser.Reset();

            //逐页读取数据
            for (int page = 1; page <= pageInt; page++)
            {
                try
                {
                    if (page > 1)
                    {
                        string typeId = html.GetInputValue("typeId");
                        string boardId = html.GetInputValue("boardId");
                        string totalRows = html.GetInputValue("totalRows");
                        NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[]{
                        "typeId","boardId","newstitle","sTime","eTime","totalRows","pageNO"
                        }, new string[]{
                        typeId,boardId,string.Empty,string.Empty,string.Empty,totalRows,page.ToString()
                        });
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.Default);
                    }
                }
                catch 
                {
                    continue;
                }
                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "lefttable")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = nodeList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[1].ToNodePlainString();
                        endDate = tr.Columns[2].ToPlainTextString().GetDateRegex();
                        InfoUrl = tr.GetATagHref();

                        string htlDtl = string.Empty;
                        try
                        {
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default);
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList ifrm = parser.ExtractAllNodesThatMatch(new TagNameFilter("iframe"));
                            IFrameTag iframe = ifrm.SearchFor(typeof(IFrameTag), true)[0] as IFrameTag;
                            htlDtl = this.ToolWebSite.GetHtmlByUrl(iframe.GetAttribute("src").Replace("/zsweb/..", ""), Encoding.Default);
                        }
                        catch { Logger.Error("InviteZhongshan"); continue; }
                        parser = new Parser(new Lexer(htlDtl.Replace("th", "td").Replace("TH", "td")));
                        NodeList dtlList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "newtalbe_c")));
                        if (dtlList != null && dtlList.Count > 0)
                        {
                            HtmlTxt = dtlList.AsHtml();
                            inviteCtx = HtmlTxt.ToCtxString();
                            TableTag tab = dtlList[0] as TableTag;
                            string ctx = string.Empty;
                            for (int k = 0; k < tab.RowCount; k++)
                            {
                                for (int d = 0; d < tab.Rows[k].ColumnCount; d++)
                                {
                                    if ((d + 1) % 2 == 0)
                                    {
                                        ctx += tab.Rows[k].Columns[d].ToNodePlainString() + "\r\n";
                                    }
                                    else
                                    {
                                        ctx += tab.Rows[k].Columns[d].ToNodePlainString() + "：";
                                    }
                                }
                            }
                            code = htlDtl.ToCtxString().GetCodeRegex().Replace("[", "").Replace("]", "");
                            buildUnit = ctx.GetBuildRegex();
                            prjAddress = ctx.GetAddressRegex();
                            beginDate = ctx.GetTimeRegex().GetDateRegex("yyyy年MM月dd日");
                            inviteType = prjName.GetInviteBidType();
                            specType = "建设工程";
                            msgType = "中山市住房和城乡建设局";
                            if (string.IsNullOrEmpty(beginDate))
                            {
                                beginDate = DateTime.Now.ToString();
                            }
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "中山市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                            list.Add(info);
                            parser = new Parser(new Lexer(htlDtl));
                            NodeList aList = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (aList != null && aList.Count > 0)
                            {
                                for (int c = 0; c < aList.Count; c++)
                                {
                                    ATag a = aList[c] as ATag;
                                    if (a.LinkText.IsAtagAttach())
                                    {
                                        string alink = a.Link;
                                        BaseAttach attach = ToolDb.GenBaseAttach(a.LinkText.Replace("&nbsp", "").Replace(";", "").Replace("；", ""), info.Id, alink);
                                        base.AttachList.Add(attach);
                                    }
                                }
                            }
                            if (!crawlAll && list.Count >= this.MaxCount) return list;
                        }
                    }
                }
                parser.Reset();
            }
            return list;
        }
    }
}
