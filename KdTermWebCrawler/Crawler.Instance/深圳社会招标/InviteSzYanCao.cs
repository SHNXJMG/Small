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

namespace Crawler.Instance
{
    public class InviteSzYanCao : WebSiteCrawller
    {
        public InviteSzYanCao()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "深圳烟草工业有限责任公司招标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取深圳烟草工业有限责任公司招标信息";
            this.SiteUrl = "http://www.szjyc.com/ExPortal/InfoListCommand.aspx?CmdType=getlist&FuWuQiBiaoShi=CPCNS_XinXiMenHu&LanMuBiaoShi=169&CurPage=1&PageSize=12&RecCount=-1&rnd=0.3884795850959004";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            string Mylist = string.Empty;
            int page = 3;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8, ref cookiestr);
            }
            catch (Exception ex)
            {
                return list;
            }
            for (int i = 1; i < page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.szjyc.com/ExPortal/InfoListCommand.aspx?CmdType=getlist&FuWuQiBiaoShi=CPCNS_XinXiMenHu&LanMuBiaoShi=169&CurPage=" + i.ToString() + "&PageSize=12&RecCount=-1&rnd=0.3884795850959004"), Encoding.UTF8, ref cookiestr);
                    }
                    catch (Exception ex) { continue; }
                }
                Mylist = htl.ToString().Replace("[", "").Replace("]", "").Trim();
                string[] str = Mylist.Split('}');
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == "")
                    {
                        continue;
                    }
                    string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                      prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                      specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                      remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                      CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HangBiaoShi = string.Empty, HtmlTxt = string.Empty;
                    string[] str1 = str[j].Split(',');
                    for (int k = 0; k < str1.Length; k++)
                    {
                        HangBiaoShi = str1[k].ToString();
                        if (HangBiaoShi == "")
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    Regex regNum = new Regex(@"\d{1,9}");
                    HangBiaoShi = regNum.Match(HangBiaoShi).Value.Trim();
                    InfoUrl = "http://www.szjyc.com/ExPortal/Details.aspx?HangBiaoShi=" + HangBiaoShi + "&LanMuBiaoShi=169&rnd=0.26078120219395623";
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
                    NodeList dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("h1"), new HasAttributeFilter("class", "infoTitle")));
                    prjName = dtnode.AsString().Replace("\r\n", "").Trim();
                    if ((prjName.Contains("招标公告") || prjName.Contains("招标采购项目") || prjName.Contains("招标项目")) && !prjName.Contains("结果公示"))
                    {
                        parserdetail = new Parser(new Lexer(htmldetail));
                        dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "infoContent")));
                        HtmlTxt = dtnode.AsHtml();
                        inviteCtx = dtnode.AsString().Replace("&rdquo", "").Replace("&ldquo", "").Replace("&middot", "").Replace("&mdash;", "").Replace("UIDataBegin", "").Replace("&times;", "").Trim();
                        Regex regCode = new Regex(@"招标编号(：|:)[^\r\n]+\r\n");
                        code = inviteCtx.GetCodeRegex();

                        //Regex regDate = new Regex(@"投标截止时间(：|:)[^\r\n]+\r\n");
                        //endDate = regDate.Match(inviteCtx).Value.Replace("投标截止时间：", "").Trim();
                        Regex regEndDate = new Regex(@"\d{4}年\d{1,2}月\d{1,2}日");
                        //endDate = regEndDate.Match(endDate).Value.Trim();
                        parserdetail = new Parser(new Lexer(htmldetail));
                        dtnode = parserdetail.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "infoOther")));
                        beginDate = regEndDate.Match(dtnode.AsString()).Value.Trim();
                        msgType = "深圳烟草工业有限责任公司";
                        specType = "其他";
                        buildUnit = "深圳烟草工业有限责任公司";
                        prjAddress = "见招标信息";
                        if (beginDate == "")
                        {
                            beginDate = string.Empty;
                        }

                        endDate = string.Empty;
                        inviteType = ToolHtml.GetInviteTypes(prjName);
                        InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "",
                         string.Empty, code, prjName, prjAddress, buildUnit,
                         beginDate, endDate, inviteCtx, remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
                        list.Add(info);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }
    }
}
