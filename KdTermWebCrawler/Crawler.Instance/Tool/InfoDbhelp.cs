using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Crawler.Instance;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Util;
using System.Windows.Forms;
using Crawler.Base.KdService;

namespace Crawler.Instance
{
    public class InfoDbhelp
    {
        public static bool IsBidInfoNull(BidInfo info)
        {
            if (info != null)
            {
                if (string.IsNullOrEmpty(info.ProjectName) && string.IsNullOrEmpty(info.InfoUrl))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 根据中标信息判断是否为通知公示
        /// </summary>
        /// <param name="info"></param>
        public static bool AddNoticeInfoByBidInfo(BidInfo info)
        { 
            if (info != null)
            {
                if (IsNoticeInfo(info.ProjectName))
                {
                    string infoType = string.Empty;
                    if (info.ProjectName.Contains("变更"))
                        infoType = "变更公告";
                    else if (info.ProjectName.Contains("资格") || info.ProjectName.Contains("预审"))
                        infoType = "资格审查";
                    else
                        infoType = "通知公示";
                    NoticeInfo entity = ToolDb.GenNoticeInfo(info.Prov, info.City, info.Area, info.Road, info.ProjectName,
                        infoType, info.BidCtx, Convert.ToString(info.BeginDate), info.Remark, info.MsgType, info.InfoUrl, info.Code, info.BuildUnit, "", "", "", "", info.CtxHtml);
                    entity.Id = info.Id;
                    ToolDb.SaveEntity(entity, "InfoUrl");
                    return true;
                }
            }
            return false;
        }

        public static bool IsNoticeInfo(string prjName)
        {
            if (string.IsNullOrEmpty(prjName))
            {
                return false;
            }
            if (prjName.Contains("变更") && !prjName.Contains("土地"))
            {
                return true;
            }
            if (prjName.Contains("资格预审"))
            {
                return true;
            }
            if (prjName.Contains("预审合格"))
            {
                return true;
            }
            if (prjName.Contains("资格核查"))
            {
                return true;
            }
            if (prjName.Contains("资格审查"))
            {
                return true;
            }
            return false;
        }
    }
}
