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
   public class BidDongWangJS : WebSiteCrawller
    {
       public BidDongWangJS()
           : base()
       {
           this.Group = "中标信息";
           this.Title = "广东省东莞市建设工程中标信息";
           this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
           this.Description = "自动抓取广东省东莞市建设工程中标信息";
           this.MaxCount = 50;
           this.SiteUrl = "https://www.dgzb.com.cn/ggzy/website/WebPagesManagement/jslist?fcInfotype=7&tenderkind=A&projecttendersite=SS&TypeIndex=4&KindIndex=0";
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


            for (int i = 1; i < page; i++)
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
                        htl = this.ToolWebSite.GetHtmlByUrl("https://www.dgzb.com.cn/ggzy/website/WebPagesManagement/findListByPage?fcInfotype=7&tenderkind=A&projecttendersite=SS&orderFiled=fcInfoenddate&orderValue=desc", nvc, Encoding.UTF8);
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
                        string prjName = string.Empty,
                          buildUnit = string.Empty, bidUnit = string.Empty,
                          bidMoney = string.Empty, code = string.Empty,
                          bidDate = string.Empty,
                          beginDate = string.Empty,
                          endDate = string.Empty, bidType = string.Empty,
                          specType = string.Empty, InfoUrl = string.Empty,
                          msgType = string.Empty, bidCtx = string.Empty,
                          prjAddress = string.Empty, remark = string.Empty,
                          prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                        Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                        code = Convert.ToString(dic["fcTendersn"]);
                        prjName = Convert.ToString(dic["fcInfotitle"]);
                        beginDate = Convert.ToString(dic["fcInfostartdate"]).GetDateRegex("yyyy-MM-dd");
                        string xu = Convert.ToString(dic["id"]);
                        InfoUrl = "https://www.dgzb.com.cn/ggzy/website/WebPagesManagement/jsdetail?publishId=" + xu + "&fcInfotype=7";
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
                            bidCtx = dtnode.ToHtml().Replace("<br/>", "\r\n");
                            buildUnit = bidCtx.GetRegexBegEnd("建设单位：", "\r");
                            bidUnit = bidCtx.GetRegexBegEnd("中标单位：", "\r");
                            bidMoney = bidCtx.GetRegexBegEnd("中标值：", "\r");
                            
                            Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                            if (!string.IsNullOrEmpty(regBidMoney.Match(bidMoney).Value))
                            {
                                if (bidMoney.Contains("万元") || bidMoney.Contains("万美元") || bidMoney.Contains("万"))
                                {
                                    bidMoney = regBidMoney.Match(bidMoney).Value;
                                }
                                else
                                {
                                    try
                                    {
                                        bidMoney = (decimal.Parse(regBidMoney.Match(bidMoney).Value) / 10000).ToString();
                                        if (decimal.Parse(bidMoney) < decimal.Parse("0.1"))
                                        {
                                            bidMoney = "0";
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        bidMoney = "0";
                                    }
                                }
                            }


                            Regex regbegin = new Regex(@"(中标时间)[:|：][^\r\n]+[\r\n]{1}");
                            bidDate = bidCtx.GetRegexBegEnd("中标时间：", "\r");
                            regbegin = new Regex(@"\d{4}年\d{1,2}月\d{1,2}日");
                            bidDate = regbegin.Match(bidDate).Value;
                            if (bidDate == "")
                            {
                                bidDate = beginDate;
                            }
                            msgType = "东莞市建设工程交易中心";
                            specType = "建设工程";
                            otherType = bidCtx.GetRegexBegEnd("工程类型：", "\r");

                            prjMgr = bidCtx.GetRegexBegEnd("项目总监：", "\r");
                            if (string.IsNullOrWhiteSpace(prjMgr))
                                prjMgr = bidCtx.GetRegexBegEnd("项目负责人：", "\r");
                            if (string.IsNullOrWhiteSpace(prjMgr))
                                prjMgr = bidCtx.GetRegexBegEnd("项目经理：", "\r");
                            bidCtx = bidCtx.Replace("ctl00_cph_context_span_MetContent", "").Replace("<span id=", "").Replace("</span>", "").Replace(">", "").Trim();
                            if (buildUnit == "")
                            {
                                buildUnit = "";
                            }
                            if (bidUnit == "")
                            {
                                bidUnit = "";
                            }
                            prjName = ToolDb.GetPrjName(prjName);
                            bidType = ToolHtml.GetInviteTypes(prjName);
                            if (!string.IsNullOrEmpty(bidUnit))
                            {
                                string[] unit = bidUnit.Split(',');
                                if (unit.Length > 0)
                                {
                                    bidUnit = unit[0];
                                }
                            }
                            if (Encoding.Default.GetByteCount(bidUnit) > 150)
                                bidUnit = string.Empty;
                            BidInfo info = ToolDb.GenBidInfo("广东省", "东莞市区", "", string.Empty, code, prjName, buildUnit, bidDate,
                              bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                              bidMoney, InfoUrl, prjMgr, HtmlTxt);
                            list.Add(info);
                            parserdetail.Reset();
                            parser = new Parser(new Lexer(HtmlTxt));
                            NodeList fileNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                            if (fileNode != null && fileNode.Count > 0)
                            {
                                string iii = fileNode.AsString().Trim();
                                for (int k = 0; k < fileNode.Count; k++)
                                {
                                    ATag aTag = fileNode[k].GetATag();
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


                //    parser = new Parser(new Lexer(htl));
                //NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_GridView1")));
                // if (tableNodeList != null && tableNodeList.Count > 0)
                //{
                //    TableTag table = (TableTag)tableNodeList[0];
                //    for (int j = 1; j < table.RowCount; j++)
                //    {
                //        string prjName = string.Empty,
                //          buildUnit = string.Empty, bidUnit = string.Empty,
                //          bidMoney = string.Empty, code = string.Empty,
                //          bidDate = string.Empty,
                //          beginDate = string.Empty,
                //          endDate = string.Empty, bidType = string.Empty,
                //          specType = string.Empty, InfoUrl = string.Empty,
                //          msgType = string.Empty, bidCtx = string.Empty,
                //          prjAddress = string.Empty, remark = string.Empty,
                //          prjMgr = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                //        TableRow tr = table.Rows[j]; 
                //        code = tr.Columns[1].ToPlainTextString().Trim();
                //        prjName = tr.Columns[2].ToPlainTextString().Trim();
                //        beginDate = tr.Columns[4].ToPlainTextString().Trim().GetReplace(" - ", "&").Split('&')[0].Trim();
                //        try
                //        {
                //            endDate = tr.Columns[4].ToPlainTextString().Trim().GetReplace(" - ", "&").Split('&')[1].Trim();
                //        }
                //        catch { }
                //        ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                //        InfoUrl = "http://www.dgzb.com.cn:8080/dgjyweb/sitemanage/" + aTag.Link.Replace("amp;", "").Trim();
                //        string htmldetail = string.Empty;
                //        try
                //        {
                //            htmldetail = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.UTF8).Replace("&nbsp;", "");
                //        }
                //        catch (Exception)
                //        {
                //            continue;
                //        }
                //        Parser parserdetail = new Parser(new Lexer(htmldetail));
                //        NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("id", "ctl00_cph_context_span_MetContent")));
                //        if (dtnode.Count > 0 && dtnode != null)
                //        {
                //            HtmlTxt = dtnode.AsHtml();
                //            bidCtx = dtnode.ToHtml().Replace("<br/>", "\r\n");
                //            Regex regBuidUnit = new Regex(@"建设单位：[^\r\n]+\r\n");
                //            buildUnit = regBuidUnit.Match(bidCtx).Value.Replace("建设单位：", "").Replace("：", "").Trim();
                //            Regex regBidUnit = new Regex(@"中标单位：[^\r\n]+\r\n");
                //            bidUnit = regBidUnit.Match(bidCtx).Value.Replace("中标单位：", "").Replace("：", "").Trim();
                //            Regex regMoney = new Regex(@"中标值：[^\r\n]+\r\n");
                //            bidMoney = regMoney.Match(bidCtx).Value;
                //            Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
                //            if (!string.IsNullOrEmpty(regBidMoney.Match(bidMoney).Value))
                //            {
                //                if (bidMoney.Contains("万元") || bidMoney.Contains("万美元") || bidMoney.Contains("万"))
                //                {
                //                    bidMoney = regBidMoney.Match(bidMoney).Value;
                //                }
                //                else
                //                {
                //                    try
                //                    {
                //                        bidMoney = (decimal.Parse(regBidMoney.Match(bidMoney).Value) / 10000).ToString();
                //                        if (decimal.Parse(bidMoney) < decimal.Parse("0.1"))
                //                        {
                //                            bidMoney = "0";
                //                        }
                //                    }
                //                    catch (Exception)
                //                    {
                //                        bidMoney = "0";
                //                    }
                //                }
                //            }
                //            Regex regbegin = new Regex(@"(中标时间)[:|：][^\r\n]+[\r\n]{1}");
                //            bidDate = regbegin.Match(bidCtx).Value.Replace("中标时间", "").Replace(" ", "").Trim();
                //            regbegin = new Regex(@"\d{4}年\d{1,2}月\d{1,2}日");
                //            bidDate = regbegin.Match(bidDate).Value;
                //            if (bidDate == "")
                //            {
                //                bidDate = beginDate;
                //            }
                //            msgType = "东莞市建设工程交易中心";
                //            specType = "建设工程";
                //            Regex regoType = new Regex(@"工程类型(：|:)[^\r\n]+\r\n");
                //            otherType = regoType.Match(bidCtx).Value.Replace("工程类型：", "").Trim();
                //            Regex regprjMgr = new Regex(@"(项目负责人|项目总监|项目经理)：[^\r\n]+\r\n");
                //            prjMgr = regprjMgr.Match(bidCtx).Value.Replace("项目负责人：", "").Replace("项目总监：", "").Replace("项目经理：", "").Trim();
                //            bidCtx = bidCtx.Replace("ctl00_cph_context_span_MetContent", "").Replace("<span id=", "").Replace("</span>", "").Replace(">", "").Trim();
                //            if (buildUnit == "")
                //            {
                //                buildUnit = "";
                //            }
                //            if (bidUnit == "")
                //            {
                //                bidUnit = "";
                //            }
                //            prjName = ToolDb.GetPrjName(prjName);
                //            bidType = ToolHtml.GetInviteTypes(prjName);
                //            if (!string.IsNullOrEmpty(bidUnit))
                //            {
                //                string[] unit = bidUnit.Split(',');
                //                if (unit.Length > 0)
                //                {
                //                    bidUnit = unit[0];
                //                }
                //            }
                //            if (Encoding.Default.GetByteCount(bidUnit) > 150)
                //                bidUnit = string.Empty;
                //            BidInfo info = ToolDb.GenBidInfo("广东省", "东莞市区", "", string.Empty, code, prjName, buildUnit, bidDate,
                //              bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, otherType,
                //              bidMoney, InfoUrl, prjMgr, HtmlTxt);
                //            list.Add(info);
                //            parserdetail.Reset();
                //            NodeList fileNode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("cellPadding", "4")));
                //            if (fileNode != null && fileNode.Count > 0)
                //            {
                //                string iii = fileNode.AsString().Trim();
                //                TableTag tablefile = (TableTag)fileNode[0];
                //                for (int k = 1; k < tablefile.RowCount; k++)
                //                {
                //                    string fileName = string.Empty, fileUrl = string.Empty;
                //                    TableRow trfile = tablefile.Rows[k];
                //                    if (trfile.Columns[1].ToPlainTextString().Trim() != "")
                //                    {
                //                        ATag aTagfile = trfile.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                //                        fileName = trfile.Columns[1].ToPlainTextString().Trim();
                //                        fileUrl = "http://www.dgzb.com.cn/dgjyweb/sitemanage/" + aTagfile.Link.Replace("amp;", "").Trim();
                //                        BaseAttach attach = ToolDb.GenBaseAttach(fileName, info.Id, fileUrl);
                //                        base.AttachList.Add(attach);
                //                    }
                //                }
                //            }
                //            if (!crawlAll && list.Count >= this.MaxCount) return list;
                 //       }
                 //   }
                 //}
            }
            return null;
        }
    }
}
