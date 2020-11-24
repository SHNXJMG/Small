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
    public class InviteSzRail : WebSiteCrawller
    {
        public InviteSzRail()
            : base()
        {
            this.Group = "代理机构招标信息";
            this.Title = "广东省深圳市广深铁路股份有限公司招标信息";
            this.PlanTime = "9:15,11:15,13:45,15:45,17:45";
            this.Description = "自动抓取广东省深圳市广深铁路股份有限公司招标信息";
            this.SiteUrl = "http://www.gsrc.com/down.php?classid=8&aid=1";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int page = 5;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.UTF8, ref cookiestr);
            }
            catch
            {
                return list;
            }

            for (int i = 1; i <= page; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode("http://www.gsrc.com/down.php?classid=8&aid=" + i.ToString()), Encoding.UTF8, ref cookiestr);
                    }
                    catch (Exception ex) { continue; }
                }
                Parser parser = new Parser(new Lexer(htl));
                NodeList tableNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("span"), new HasAttributeFilter("class", "l")));
                if (tableNodeList.Count > 0)
                {
                    
                    for (int j = 0; j < tableNodeList.Count; j++)
                    {
                        string temp = tableNodeList[j].ToPlainTextString().Replace("&nbsp;", "").Replace("&bull;", "").Trim();
                        Regex regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                        string beginDate = regDate.Match(temp).Value;
                        //prjName = prjName.Remove(prjName.IndexOf("(")).ToString();
                        ATag aTag = tableNodeList.SearchFor(typeof(ATag), true)[j] as ATag; 
                        if (temp.Contains("结果") || temp.Contains("中标"))
                            AddBidInfo(temp, aTag.Link, beginDate, list);
                        else
                            AddInviteInfo(temp, aTag.Link, beginDate, list);
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }
            }
            return list;
        }

        private void AddBidInfo(string itemName, string dtlUrl, string begin, IList list)
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

            beginDate = begin;
            InfoUrl = "http://www.gsrc.com/" + dtlUrl.Replace("./","");
            prjName = itemName.GetReplace("./," + begin + ",(,)");
            msgType = "广深铁路股份有限公司";
            specType = "建设工程";
            buildUnit = "广深铁路股份有限公司";
            prjAddress = "见附件";
            bidType = ToolHtml.GetInviteTypes(prjName);
            BidInfo info = ToolDb.GenBidInfo("广东省", "深圳社会招标", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, "见附件", string.Empty, msgType, bidType, specType, otherType, bidMoney, InfoUrl, prjMgr, "见附件");
            list.Add(info);
            BaseAttach attach = ToolDb.GenBaseAttach(prjName, info.Id, InfoUrl);
            base.AttachList.Add(attach); 
        }
        private void AddInviteInfo(string itemName, string dtlUrl, string begin,IList list)
        {
            string code = string.Empty, buildUnit = string.Empty, prjName = string.Empty,
                                               prjAddress = string.Empty, inviteCtx = string.Empty, inviteType = string.Empty,
                                               specType = string.Empty, beginDate = string.Empty, endDate = string.Empty,
                                               remark = string.Empty, inviteCon = string.Empty, InfoUrl = string.Empty,
                                               CreateTime = string.Empty, msgType = string.Empty, otherType = string.Empty, HtmlTxt = string.Empty;

            beginDate = begin;
            InfoUrl = "http://www.gsrc.com/" + dtlUrl.Replace("./", "");
            try
            {
                prjName = itemName.GetReplace("./," + begin + ",(,)");
            }
            catch 
            {
                prjName = itemName;
            }
            msgType = "广深铁路股份有限公司";
            specType = "建设工程";
            buildUnit = "广深铁路股份有限公司";
            prjAddress = "见附件";
            inviteType = ToolHtml.GetInviteTypes(prjName);
            InviteInfo info = ToolDb.GenInviteInfo("广东省", "深圳社会招标", "",
                    string.Empty, code, prjName, prjAddress, buildUnit,
                    beginDate, endDate, "见附件", remark, msgType, inviteType, specType, otherType, InfoUrl, HtmlTxt);
            list.Add(info);
            BaseAttach attach = ToolDb.GenBaseAttach(prjName, info.Id, InfoUrl);
            base.AttachList.Add(attach); 
        }
    }
}
