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
namespace Crawler.Instance
{
    public class InviteSzjianshe : WebSiteCrawller
    {
        public InviteSzjianshe()
            : base(true)
        {
            this.Group = "招标信息";
            this.Title = "广东省深圳市住房和建设局历史招标公告";
            this.Description = "自动抓取广东省深圳市住房和建设局历史招标公告";
            this.ExistCompareFields = "Prov,City,Code,MsgType";
            this.SiteUrl = "http://61.144.226.2/zbgg/browse.aspx?xxlxbh=1";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.MaxCount = 200;

        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string html = string.Empty;
            string viewState = string.Empty;
            int sqlCount = 0;
            string eventValidation = string.Empty;
            string cookiestr = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default, ref cookiestr);
            }
            catch (Exception ex)
            {
                return list;
            }
            IList arr = GetPrjCode();
            IList del = arr;
            if (arr.Count > 0)
            {
                for (int d = (arr.Count - 1); d >= 0; d--)
                {
                    string htmtxt = string.Empty;
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc1 = this.ToolWebSite.GetNameValueCollection(new string[] { "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "txtPrj_ID", "txtPrj_Name", "Chk_Query", "Radiobuttonlist1", "QUERY", "ucPageNumControl:gotopage" },
                        new string[] { string.Empty, string.Empty, viewState, arr[d].ToString(), "", "0", "1", "查询", "" });
                    try
                    {
                        htmtxt = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), nvc1, Encoding.Default, ref cookiestr);
                    }
                    catch (Exception ex)
                    {
                        return list;
                    }
                    Parser parser = new Parser(new Lexer(htmtxt));
                    NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "dgConstBid")));
                    if (dtList != null && dtList.Count > 0)
                    {
                        TableTag table = dtList[0] as TableTag;
                        for (int j = 1; j < table.RowCount; j++)
                        {
                            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                  prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                  specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                  remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                  CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;
                            TableRow dr = table.Rows[j];
                            code = dr.Columns[1].ToPlainTextString().Trim();
                            prjName = dr.Columns[2].ToPlainTextString().Trim();
                            buildUnit = dr.Columns[3].ToPlainTextString().Trim();
                            ATag aTag = dr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                            InfoUrl = "http://61.144.226.2/zbgg/Detail.aspx?ID=" + aTag.Link.Trim().Replace("GoDetail('", "").Replace("');", "") + "&xxlxbh=1&PRJ_TYPE=0";
                            string htmlde = string.Empty;
                            try
                            {
                                htmlde = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default).Replace("&nbsp;", "");
                            }
                            catch { continue; }
                            parser = new Parser(new Lexer(htmlde));
                            NodeList dealList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "Table8")));
                            if (dealList != null && dealList.Count > 0)
                            {
                                string ctx = string.Empty;
                                HtmlTxt = dealList.ToHtml();
                                TableTag tab = dealList[0] as TableTag;
                                string text = string.Empty;
                                try
                                {
                                    for (int k = 0; k < tab.RowCount; k++)
                                    {
                                        TableRow tr = tab.Rows[k];
                                        text = tr.Columns[0].ToPlainTextString().Replace(":", "").Replace("：", "").Replace(" ", "") + "：".Trim();
                                        ctx += text + tr.Columns[1].ToPlainTextString().Trim().Replace(" ", "") + "\r\n";
                                    }
                                    for (int k = 0; k < tab.RowCount; k++)
                                    {
                                        TableRow tr = tab.Rows[k];
                                        text = tr.Columns[0].ToPlainTextString().Replace(":", "").Replace("：", "") + "：".Trim();
                                        inviteCtx += text + tr.Columns[1].ToPlainTextString().Trim() + "\r\n";
                                    }
                                }
                                catch { }
                                Regex regDate = new Regex(@"发布日期(：|:)[^\r\n]+[\r\n]{1}");
                                string datestr = regDate.Match(inviteCtx).Value.Replace("发布日期", "").Replace("：", "").Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                                if (!string.IsNullOrEmpty(datestr))
                                {
                                    try
                                    {
                                        int len = datestr.IndexOf("到");
                                        beginDate = datestr.Substring(0, len);
                                        endDate = datestr.Substring(len + 1, datestr.Length - len - 1);
                                    }
                                    catch { }
                                }
                                Regex regPrjAdd = new Regex(@"(工程地点|工程地址)：[^\r\n]+[\r\n]{1}");
                                prjAddress = regPrjAdd.Match(ctx).Value.Replace("工程地点：", "").Replace("工程地址：", "").Trim();

                                Regex regOth = new Regex(@"(工程类型|项目类型)：[^\r\n]+[\r\n]{1}");
                                otherType = regOth.Match(ctx).Value.Replace("工程类型：", "").Replace("项目类型：", "").Trim();

                                msgType = "深圳市建设工程交易中心";
                                specType = "建设工程";
                                inviteType = ToolHtml.GetInviteTypes(prjName);
                                InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳市区", "", string.Empty, code, prjName, prjAddress, buildUnit, beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);

                                if (sqlCount <= this.MaxCount)
                                {
                                    ToolDb.SaveEntity(info, this.ExistCompareFields);
                                    sqlCount++;
                                }
                                else return list;
                            }
                        }
                    }
                    del.RemoveAt(d);
                    DeleteCode(del);
                }
            }
            return list;
        }

        public void DeleteCode(IList list)
        {
            try
            {
                string path = "E:\\ProjectInfo\\招标.txt";
                File.Delete(path);
                for (int i = 0; i < list.Count; i++)
                {
                    File.AppendAllText(path, list[i].ToString() + "\r\n", Encoding.GetEncoding("GB2312"));
                }
            }
            catch { }
        }

        public IList GetPrjCode()
        {
            IList list = new ArrayList();
            try
            {
                using (FileStream fileStream = File.OpenRead("E:\\ProjectInfo\\招标.txt")) //选txt文本
                {
                    using (StreamReader streamreader = new StreamReader(fileStream, Encoding.GetEncoding("GB2312")))
                    {
                        string lines = null;

                        while ((lines = streamreader.ReadLine()) != null)
                        {
                            string strs = lines.ToString();
                            list.Add(strs);
                        }
                    }
                }
            }
            catch { }
            return list;
        }
    }
}
