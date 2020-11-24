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
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class InviteSzBaoanJD : WebSiteCrawller
    {
        public InviteSzBaoanJD()
            : base()
        {
            this.Group = "招标信息";
            this.Title = "广东省深圳市宝安区街道办";
            this.Description = "自动抓取广东省深圳市宝安区街道办招标信息";
            this.PlanTime = "1:00,09:25,09:50,10:15,10:50,11:30,14:05,14:25,14:50,16:50,19:00";
            this.SiteUrl = "http://www.bajsjy.com/JDZB";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<InviteInfo>();
            int pageInt = 1;
            string html = string.Empty;
            string viewState = string.Empty;
            string cookiestr = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList tdNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "input-group-addon")));
            if (tdNodes != null && tdNodes.Count > 0)
            {
                try
                {
                    string reTemp = tdNodes.AsString().GetRegexBegEnd("共", "项");
                    string pageTemp = tdNodes.AsString().GetRegexBegEnd("项", "页").GetReplace("共,项,页," + reTemp + ",，");
                    pageInt = int.Parse(pageTemp);
                }
                catch (Exception) { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(SiteUrl + "?pi=" + (i-1), Encoding.UTF8);
                    }
                    catch { continue; }
                }

                parser = new Parser(new Lexer(html));
                NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "inside_table")));
                if (nodeList != null && nodeList.Count > 0)
                {
                    TableTag table = (TableTag)nodeList[0];

                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty, prjAddress = string.Empty, inviteCtx = string.Empty, bidType = string.Empty, specType = string.Empty, beginDate = string.Empty, endDate = string.Empty, remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty, CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        TableRow tr = table.Rows[j];
                        prjName = tr.Columns[1].ToPlainTextString().Trim();
                        buildUnit = tr.Columns[2].ToPlainTextString().Trim();
                        beginDate = tr.Columns[3].ToPlainTextString().Trim();
                        InfoUrl = "http://www.bajsjy.com/" + tr.Columns[1].GetATagHref();
                        string htmldetail = string.Empty;
                        try
                        {
                            htmldetail = this.ToolWebSite.GetHtmlByUrl(InfoUrl, Encoding.UTF8).Replace("<th", "<td").Replace("</th>", "</td>").Replace("&nbsp;", "");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        Parser parserdetail = new Parser(new Lexer(htmldetail));
                        NodeList nodeDetailList = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "inside_table")));
                        if (nodeDetailList != null && nodeDetailList.Count > 0)
                        {
                            HtmlTxt = nodeDetailList.AsHtml();
                            TableTag tabledetail = (TableTag)nodeDetailList[0];

                            for (int r = 0; r < tabledetail.RowCount; r++)
                            {
                                TableRow trdetail = tabledetail.Rows[r];

                                for (int c = 0; c < trdetail.ColumnCount; c++)
                                {

                                    string tr1 = string.Empty;
                                    string tr2 = string.Empty;
                                    NodeList inptList;
                                    NodeList selList;
                                    if (trdetail.ColumnCount <= 1)
                                    {

                                        continue;
                                    }
                                    tr1 = trdetail.Columns[c].ToPlainTextString().Trim();
                                    tr2 = trdetail.Columns[c + 1].ToPlainTextString().Trim();

                                    inptList = trdetail.Columns[c + 1].SearchFor(typeof(InputTag), true);
                                    selList = trdetail.Columns[c + 1].SearchFor(typeof(SelectTag), true);
                                    if (inptList != null && inptList.Count > 0)
                                    {
                                        if (inptList.Count > 1)
                                        {
                                            for (int inp = 0; inp < inptList.Count; inp++)
                                            {
                                                InputTag inputTage = (InputTag)inptList[inp];
                                                if (inputTage.GetAttribute("checked") == "checked")
                                                {
                                                    tr2 = inputTage.GetAttribute("value");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            InputTag inputTage = (InputTag)inptList[0];
                                            tr2 = inputTage.GetAttribute("value");
                                        }



                                    }
                                    if (selList != null && selList.Count > 0)
                                    {
                                        SelectTag selTag = (SelectTag)selList[0];
                                        NodeList opList = new NodeList();
                                        selTag.CollectInto(opList, new HasAttributeFilter("selected", "selected"));
                                        tr2 = opList.AsString();
                                    }
                                    inviteCtx += tr1 + "：" + tr2 + "\r\n";
                                    if (trdetail.ColumnCount > (c + 1))
                                    {
                                        c = c + 1;
                                    }
                                }
                            }


                            Regex regPrjAddr = new Regex(@"工程地址：[^\r\n]+\r\n");
                            prjAddress = regPrjAddr.Match(inviteCtx).Value.Replace("工程地址：", "").Trim();

                            Regex regoType = new Regex(@"工程类型：[^\r\n]+\r\n");
                            string oType = regoType.Match(inviteCtx).Value.Replace("工程类型：", "").Trim();

                            if (oType.Contains("房建"))
                            {
                                otherType = "房建及工业民用建筑";
                            }
                            else if (oType.Contains("市政"))
                            {
                                otherType = "市政工程";
                            }
                            else if (oType.Contains("园林绿化"))
                            {
                                otherType = "园林绿化工程";
                            }
                            else if (oType.Contains("装饰") || oType.Contains("装修"))
                            {
                                otherType = "装饰装修工程";
                            }
                            else if (oType.Contains("电力"))
                            {
                                otherType = "电力工程";
                            }
                            else if (oType.Contains("水利"))
                            {
                                otherType = "水利工程";
                            }
                            if (oType.Contains("环保"))
                            {
                                otherType = "环保工程";
                            }

                            msgType = "深圳市建设工程交易中心宝安分中心";
                            specType = "建设工程";
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳宝安区工程", "宝安区", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, bidType, specType, otherType, InfoUrl, HtmlTxt);
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
