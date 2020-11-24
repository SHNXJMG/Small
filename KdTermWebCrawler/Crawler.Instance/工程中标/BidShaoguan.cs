using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Windows.Forms;
using System.Web;

namespace Crawler.Instance
{
    /// <summary>
    /// 中标信息--韶关
    /// </summary>
    public class BidShaoguan : WebSiteCrawller
    {
        public BidShaoguan()
            : base()
        {
            this.Group = "中标信息";
            this.Title = "广东省韶关市";
            this.Description = "自动抓取广东省韶关市中标信息";
            this.PlanTime = "9:30,11:30,14:30,16:30,18:30";
            this.ExistCompareFields = "InfoUrl";//"Prov,City,Area,Road,Code,ProjectName,BidUnit,BidMoney,Remark";
            this.SiteUrl = "http://www.sgjsj.gov.cn/html/news/biaoxx/";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidInfo>();

            //取得页码
            int pageInt = 1;
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.Default);
            }
            catch
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "pages")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                string pageTemp = tdNodes[0].ToPlainTextString().Trim();
                string paTe = "kd" + pageTemp;
                pageTemp = paTe.GetRegexBegEnd("kd", "条");
                try
                {
                    pageInt = int.Parse(pageTemp);
                }
                catch (Exception ex) { }
                try
                {
                    if (pageInt % 20 > 0)
                    {
                        pageInt = (pageInt / 20) + 1;
                    }
                    else
                    {
                        pageInt = pageInt / 20;
                    }
                }
                catch (Exception ex) { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + i + ".html", Encoding.Default);
                    }
                    catch { continue; };
                }
                parser = new Parser(new Lexer(html));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("ul"), new HasAttributeFilter("class", "list")), true), new TagNameFilter("li")));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        INode node = listNode[j];
                        ATag aTag = node.GetATag();
                        if (aTag == null) continue;
                        string prjName = string.Empty,
                          buildUnit = string.Empty, bidUnit = string.Empty,
                          bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty,
                          endDate = string.Empty, bidType = string.Empty,
                          specType = string.Empty, InfoUrl = string.Empty,
                          msgType = string.Empty, bidCtx = string.Empty,
                          prjAddress = string.Empty, remark = string.Empty,
                          prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        prjName = aTag.LinkText;
                        beginDate = listNode[j].ToPlainTextString().GetDateRegex();
                        InfoUrl = aTag.Link;
                        string htmldtl = string.Empty;
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.Default).GetJsString();
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldtl));

                        NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("class", "pd0 par1")));
                        if (dtlNode != null && dtlNode.Count > 0)
                        {
                            HtmlTxt = dtlNode.AsHtml();
                            bidCtx = HtmlTxt.GetReplace("</p>", "\r\n").ToCtxString();
                            string ctx = string.Empty;
                            bidUnit = bidCtx.GetBidRegex();
                            bidMoney = bidCtx.GetMoneyRegex();
                            prjMgr = bidCtx.GetMgrRegex();
                            prjAddress = bidCtx.GetAddressRegex();
                            if (string.IsNullOrWhiteSpace(bidUnit))
                            {
                                parser = new Parser(new Lexer(HtmlTxt));
                                NodeList tableNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                                if (tableNode != null && tableNode.Count > 0)
                                {

                                    TableTag tag = tableNode[0] as TableTag;
                                    for (int r = 0; r < tag.RowCount; r++)
                                    {
                                        for (int c = 0; c < tag.Rows[r].ColumnCount; c++)
                                        {
                                            string temp = tag.Rows[r].Columns[c].ToNodePlainString().GetReplace(":,：");
                                            if ((c + 1) % 2 == 0)
                                            {
                                                ctx += temp + "\r\n";
                                            }
                                            else
                                                ctx += temp + "：";
                                        }

                                    }
                                    if (ctx.Contains("第三中标候选人")&&string.IsNullOrWhiteSpace(bidUnit))
                                        bidUnit = ctx.GetRegexBegEnd("第三中标候选人", "公司").GetReplace("\r\n", "") + "公司";
                                    if(!ctx.Contains("第三中标候选人")&&ctx.Contains("第二中标候选人")&&string.IsNullOrWhiteSpace(bidUnit))
                                        bidUnit = ctx.GetRegexBegEnd("第二中标候选人", "公司").GetReplace("\r\n", "") + "公司";
                                    if (!ctx.Contains("第二中标候选人") && string.IsNullOrWhiteSpace(bidUnit))
                                        bidUnit = ctx.GetRegexBegEnd("第一中标候选人（中标人）", "公司").GetReplace("\r\n", "") + "公司";
                                    if (string.IsNullOrWhiteSpace(buildUnit))
                                        buildUnit = ctx.GetBuildRegex();
                                   

                                   
                                        bidMoney = ctx.GetMoneyRegex();
                                    if (string.IsNullOrWhiteSpace(prjMgr))
                                    prjMgr = ctx.GetMgrRegex();
                                    

                                }
                            }
                           
                            try
                            {
                                Parser parses = new Parser(new Lexer(htmldtl));
                                NodeList codel = parses.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("p"), new HasAttributeFilter("id", "name")));
                                if (codel != null && codel.Count > 0)
                                {
                                    code = codel[0].ToPlainTextString().Trim();
                                }
                            }
                            catch { }
                            if (string.IsNullOrWhiteSpace(prjMgr))
                                prjMgr = ctx.GetRegexBegEnd("项目经理", "技术");
                            if (bidUnit.Contains("："))
                                bidUnit = bidUnit.GetReplace("：","");
                            if (!bidUnit.Contains("有限公司"))
                                bidUnit = bidUnit.GetReplace("公司","");
                            bidType = prjName.GetInviteBidType();
                            BidInfo info = ToolDb.GenBidInfo("广东省", "韶关市区", null, null, code, prjName, buildUnit, beginDate, bidUnit, beginDate, null, bidCtx, remark, "韶关市住房和城乡建设局", bidType,
                                           "建设工程", otherType, bidMoney, InfoUrl, string.Empty, HtmlTxt);
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
